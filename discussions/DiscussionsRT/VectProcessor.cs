using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discussions.DbModel;
using Discussions.RTModel.Model;
using Discussions.RTModel.Operations;
using DistributedEditor;
using ExitGames.Logging;
using Lite;
using Photon.SocketServer;

namespace Discussions.RTModel
{
    //there is one vector processor per topic
    public class VectProcessor
    {
        private readonly DiscussionRoom _room;
        private readonly int _topicId;

        private ServerVdDoc _doc;

        private readonly Random _coordsRnd = new Random();

        private ClusterTopology _topology;

        private static readonly ILogger _log = LogManager.GetCurrentClassLogger();

        //if true, annotation was changed since last save, need to resave
        private bool _pendingChanges;

        public VectProcessor(int topicId, DiscussionRoom room)
        {
            _topicId = topicId;
            _room = room;

            //restore annotation
            using (var dbCtx = new DiscCtx(Discussions.ConfigManager.ConnStr))
            {
                var topic = dbCtx.Topic.FirstOrDefault(t0 => t0.Id == topicId);
                if (topic.Annotation != null)
                {
                    var str = new MemoryStream();
                    str.Write(topic.Annotation, 0, topic.Annotation.Count());
                    str.Position = 0;
                    var reader = new BinaryReader(str);
                    Read(reader);
                }
                else
                {
                    //no annotations saved for this topic yet
                    Read(null);
                }
            }
        }

        private bool Write(BinaryWriter annotation)
        {
            _topology.Write(annotation);
            return _doc.Write(annotation);
        }

        private void Read(BinaryReader annotation)
        {
            _topology = new ClusterTopology(_room, annotation);
            _topology.onLinkRemove += __linkRemove;
            _topology.onLinkableDeleted += __onLinkableDeleted;
            _topology.onUnclusterBadge += __unclusterBadge;

            _doc = new ServerVdDoc(annotation);
        }

        public void CheckPersist()
        {
            if (!_pendingChanges)
                return;
            _pendingChanges = false;

            using (var dbCtx = new DiscCtx(Discussions.ConfigManager.ConnStr))
            {
                var topic = dbCtx.Topic.FirstOrDefault(t0 => t0.Id == _topicId);
                var str = new MemoryStream();
                var writer = new BinaryWriter(str);
                if (this.Write(writer))
                {
                    topic.Annotation = str.ToArray();
                    dbCtx.SaveChanges();
                }
            }
        }

        //user U can take cursor on shape S <=> (U doesn't have other cursors && S is free)
        public void HandleCursorRequest(LitePeer peer,
                                        OperationRequest operationRequest,
                                        SendParameters sendParameters)
        {
            var req = CursorRequest.Read(operationRequest.Parameters);

            var changes = false;
            if (req.doSet)
            {
                if (_doc.UserHasCursor(req.ownerId))
                    return; //already holds another cursor 

                var sh = _doc.TryGetShape(req.shapeId);
                if (sh == null)
                    return; //no such shape

                if (sh.GetCursor() != null)
                    return; // shape is busy      

                //ok, lock shape 
                _doc.LockShape(sh, req.ownerId);
                changes = true;
            }
            else
            {
                //cursor remove operation is always ok
                var sh = _doc.TryGetShape(req.shapeId);
                if (sh == null)
                    return; //no such shape
                _doc.UnlockShape(sh, req.ownerId);
                changes = true;
            }

            if (changes)
            {
                _room.Broadcast(peer,
                                CursorEvent.Write(req.doSet, req.ownerId, req.shapeId, _topicId),
                                sendParameters,
                                (byte) DiscussionEventCode.CursorEvent,
                                BroadcastTo.RoomAll); //include self
            }
        }

        public void HandleCreateShape(LitePeer peer,
                                      OperationRequest operationRequest,
                                      SendParameters sendParameters)
        {
            var req = CreateShape.Read(operationRequest.Parameters);

            var newSh = new ServerBaseVdShape(req.shapeId, req.ownerId, req.shapeType, req.tag);

            switch (req.shapeType)
            {
                case VdShapeType.Cluster:
                    _topology.CreateCluster(newSh.Id());
                    _doc.AddShape(newSh);
                    break;
                case VdShapeType.FreeForm:
                    _doc.AddShape(newSh);
                    EventLogger.LogAndBroadcast(
                        new DiscCtx(Discussions.ConfigManager.ConnStr),
                        _room,
                        model.StEvent.FreeDrawingCreated,
                        req.ownerId,
                        _topicId);
                    break;
                default:
                    _doc.AddShapeAndLock(newSh);
                    break;
            }

            _room.Broadcast(peer,
                            operationRequest,
                            sendParameters,
                            (byte) DiscussionEventCode.CreateShapeEvent,
                            BroadcastTo.RoomExceptSelf);
                //don't include self, we play shape creation locally without continuation

            _pendingChanges = true;
        }

        public void HandleBadgeCreated(int argPointId,
                                       LitePeer peer,
                                       OperationRequest operationRequest,
                                       SendParameters sendParameters)
        {
            var ctx = new DiscCtx(Discussions.ConfigManager.ConnStr);
            var ap = ctx.ArgPoint.FirstOrDefault(ap0 => ap0.Id == argPointId);
            if (ap == null)
                throw new NotSupportedException("cannot find badge in db!");

            var badgeSh = new ServerBaseVdShape(_doc.BadgeIdGen.NextId(), ap.Person.Id, VdShapeType.Badge, argPointId);
            _doc.AddShape(badgeSh);
            _topology.CreateBadge(badgeSh.Id());

            //set initial badge state 
            var st = new ShapeState(VdShapeType.Badge,
                                    ap.Person.Id,
                                    badgeSh.Id(),
                                    null,
                                    null,
                                    new double[] {100 + _coordsRnd.Next(400), 100 + _coordsRnd.Next(400)},
                                    _topicId);
            badgeSh.ApplyState(st);

            var badgeCreateEv = CreateShape.Write(ap.Person.Id, badgeSh.Id(),
                                                  VdShapeType.Badge,
                                                  st.doubles[0], st.doubles[1], false,
                                                  argPointId, _topicId);

            //include self, badge is created in private board, and if our public board is open, we want to play new badge
            _room.Broadcast(peer,
                            badgeCreateEv,
                            sendParameters,
                            (byte) DiscussionEventCode.CreateShapeEvent,
                            BroadcastTo.RoomAll);

            _pendingChanges = true;
        }

        public void HandleDeleteShapes(LitePeer peer,
                                       OperationRequest operationRequest,
                                       SendParameters sendParameters)
        {
            var req = DeleteShapesRequest.Read(operationRequest.Parameters);

            var owner = req.initialOwnerId;
            var shapesBeingRemoved = _doc.GetShapes().Where(sh => sh.InitialOwner() == owner &&
                                                                  sh.ShapeCode() != VdShapeType.Badge &&
                                                                  sh.ShapeCode() != VdShapeType.Cluster &&
                                                                  sh.ShapeCode() != VdShapeType.ClusterLink
                );

            //check permissions
            foreach (var sr in shapesBeingRemoved)
                if (!_doc.editingPermission(sr, owner))
                    return;

            //ok, remove 
            foreach (var sr in shapesBeingRemoved.ToArray())
                UnlockDeleteBroadcast(sr.Id(), req.ownerId);

            _pendingChanges = true;
        }

        private void __linkRemove(Linkable end1, Linkable end2, int linkShapeId, int usrId)
        {
            UnlockDeleteBroadcast(linkShapeId, usrId);

            EventLogger.LogAndBroadcast(new DiscCtx(Discussions.ConfigManager.ConnStr),
                                        _room,
                                        model.StEvent.LinkRemoved,
                                        usrId,
                                        _topicId);

            _pendingChanges = true;
        }

        public void HandleDeleteSingleShape(LitePeer peer,
                                            OperationRequest operationRequest,
                                            SendParameters sendParameters)
        {
            var req = DeleteSingleShapeRequest.Read(operationRequest.Parameters);

            var sh = _doc.TryGetShape(req.shapeId);
            if (sh == null)
                return;

            switch (sh.ShapeCode())
            {
                case VdShapeType.ClusterLink:
                    var edge = _topology.GetForwardEdge(req.shapeId);
                    _topology.Unlink(edge.curr.GetId(), edge.next.GetId(), req.ownerId); //see __linkRemove
                    break;
                case VdShapeType.Cluster:
                    //not sent manually, but by system (client)
                    _topology.DeleteCluster(_topology.GetCluster(req.shapeId), -1);
                    break;
                default:
                    UnlockDeleteBroadcast(sh.Id(), req.ownerId);
                    break;
            }

            _pendingChanges = true;
        }

        private void UnlockDeleteBroadcast(int shapeId, int indirectOwner)
        {
            var shape = _doc.TryGetShape(shapeId);
            _doc.UnlockAndRemoveShape(shape);

            //single shape removal includes initiator 
            _room.BroadcastReliableToRoom((byte) DiscussionEventCode.DeleteSingleShapeEvent,
                                          DeleteSingleShapeEvent.Write(shapeId, _topicId, indirectOwner));

            if (shape.ShapeCode() == VdShapeType.FreeForm)
            {
                EventLogger.LogAndBroadcast(
                    new DiscCtx(Discussions.ConfigManager.ConnStr),
                    _room,
                    model.StEvent.FreeDrawingRemoved,
                    shape.InitialOwner(),
                    _topicId);
            }

            _pendingChanges = true;
        }

        private void __onLinkableDeleted(Linkable end, int usrId)
        {
            UnlockDeleteBroadcast(end.GetId(), usrId);

            //record event
            if (end is Cluster)
            {
                EventLogger.LogAndBroadcast(new DiscCtx(Discussions.ConfigManager.ConnStr),
                                            _room,
                                            model.StEvent.ClusterDeleted,
                                            -1, //owner unknown 
                                            _topicId);
            }

            _pendingChanges = true;
        }

        public void HandleBadgeDeleted(int argPointId,
                                       LitePeer peer,
                                       OperationRequest operationRequest,
                                       SendParameters sendParameters)
        {
            var sh = _doc.TryGetBadgeShapeByArgPt(argPointId);
            if (sh == null)
                return;

            _topology.DeleteBadge(sh.Id(), sh.InitialOwner());

            _pendingChanges = true;
        }

        public void HandleBadgeModified(int argPointId,
                                        LitePeer peer,
                                        OperationRequest operationRequest,
                                        SendParameters sendParameters)
        {
            var sh = _doc.TryGetBadgeShapeByArgPt(argPointId);

            using (var dbCtx = new DiscCtx(Discussions.ConfigManager.ConnStr))
            {
                var pt = dbCtx.ArgPoint.FirstOrDefault(pt0 => pt0.Id == argPointId);
                if (pt.SharedToPublic && sh == null)
                {
                    HandleBadgeCreated(argPointId, peer, operationRequest, sendParameters);
                }
                else if (!pt.SharedToPublic && sh != null)
                {
                    HandleBadgeDeleted(argPointId, peer, operationRequest, sendParameters);
                }
            }
        }

        public void HandleUnselectAll(LitePeer peer,
                                      OperationRequest operationRequest,
                                      SendParameters sendParameters)
        {
            var ev = UnselectAllEvent.Write();
            _room.Broadcast(peer, ev, sendParameters,
                            (byte) DiscussionEventCode.UnselectAllEvent,
                            BroadcastTo.RoomExceptSelf);
        }

        public void HandleStateSync(LitePeer peer,
                                    OperationRequest operationRequest,
                                    SendParameters sendParameters)
        {
            var state = ShapeState.Read(operationRequest.Parameters);
            var sh = _doc.TryGetShape(state.shapeId);
            if (sh == null)
                return;

            sh.ApplyState(state);

            if (state.doBroadcast)
            {
                _room.Broadcast(peer, operationRequest, sendParameters,
                                (byte) DiscussionEventCode.StateSyncEvent,
                                BroadcastTo.RoomExceptSelf); // don't send state sync to initiator
            }

            //correct badge positions
            switch (sh.ShapeCode())
            {
                case VdShapeType.Cluster:
                    var contents = _topology.GetCluster(sh.Id()).GetClusterables();
                    foreach (var badgeIntf in contents)
                    {
                        var badge = _doc.TryGetShape(badgeIntf.GetId());
                        var st = badge.GetState();
                        st.doubles[0] += state.doubles[0];
                        st.doubles[1] += state.doubles[1];
                        badge.ApplyState(st);
                    }
                    break;
            }

            _pendingChanges = true;
        }

        private void CleanupEmptyClusters()
        {
            var clusters = _doc.GetShapes().Where(sh => sh.ShapeCode() == VdShapeType.Cluster);
            foreach (var c in clusters.ToArray())
            {
                var cluster = _topology.GetCluster(c.Id());
                if (!cluster.GetClusterables().Any())
                {
                    _log.Error("found empty clusters during cleanup");
                    _doc.UnlockAndRemoveShape(c);
                    _pendingChanges = true;
                }
            }
        }

        public void HandleInitialSceneLoad(LitePeer peer,
                                           OperationRequest operationRequest,
                                           SendParameters sendParameters)
        {
            //// var req = InitialSceneLoadRequest.Read(operationRequest);
            /// req.topicId
            _log.Debug("scene load request");

            CleanupEmptyClusters();

            //1st phase, send creation events for simple shapes (+cluster)  in the scene
            var simpleShapes = _doc.GetShapes().Where(sh => sh.ShapeCode() != VdShapeType.ClusterLink).ToArray();
            foreach (var sh in simpleShapes)
            {
                _room.PublishEventToSingle(peer,
                                           CreateShape.Write(sh.InitialOwner(), sh.Id(),
                                                             sh.ShapeCode(), 400, 400, false, sh.Tag(),
                                                             _topicId),
                                           sendParameters,
                                           (byte) DiscussionEventCode.CreateShapeEvent);
            }

            //2nd phase, send state update packets
            foreach (var sh in simpleShapes)
            {
                var st = sh.GetState();
                if (st == null)
                    continue;
                _room.PublishEventToSingle(peer,
                                           st.ToDict(),
                                           sendParameters,
                                           (byte) DiscussionEventCode.StateSyncEvent);
            }

            //3rd phase, sequence of cluster-add operations (enumerate contents of all existing clusters)
            var clusterShapes = _doc.GetShapes().Where(sh => sh.ShapeCode() == VdShapeType.Cluster);
            foreach (var clShape in clusterShapes)
            {
                var cluster = _topology.GetCluster(clShape.Id());
                _log.Info("scene load, cluster updates, num badges =" + cluster.GetClusterables());
                foreach (var badge in cluster.GetClusterables())
                {
                    var clustMsg = ClusterBadgeMessage.Write(badge.GetId(),
                                                             _doc.TryGetShape(badge.GetId()).InitialOwner(),
                                                             badge == cluster.GetClusterables().Last(),
                                                             cluster.GetId(),
                                                             _topicId,
                                                             -1);
                    _room.PublishEventToSingle(peer,
                                               clustMsg,
                                               sendParameters,
                                               (byte) DiscussionEventCode.ClusterBadgeEvent);
                }
            }

            //4rd phase, create links 
            var linkShapes = _doc.GetShapes().Where(sh => sh.ShapeCode() == VdShapeType.ClusterLink);
            foreach (var lnk in linkShapes)
            {
                var edge = _topology.GetForwardEdge(lnk.Id());

                var ev = LinkCreateMessage.Write(edge.curr.GetId(),
                                                 edge.next.GetId(),
                                                 lnk.InitialOwner(),
                                                 lnk.Id(),
                                                 _topicId,
                                                 false,
                                                 (LinkHeadType) lnk.Tag());
                _room.PublishEventToSingle(peer, ev, sendParameters, (byte) DiscussionEventCode.LinkCreateEvent);

                //send link state update
                var st = lnk.GetState();
                if (st != null)
                {
                    _room.PublishEventToSingle(peer,
                                               st.ToDict(),
                                               sendParameters,
                                               (byte) DiscussionEventCode.StateSyncEvent);
                }
            }

            //5th phase, send cursor events
            foreach (var sh in _doc.GetShapes())
            {
                if (sh.GetCursor() == null)
                    continue; //cursors are unset by default for all shapes  

                _room.PublishEventToSingle(peer,
                                           CursorEvent.Write(true,
                                                             sh.GetCursor().OwnerId,
                                                             sh.Id(),
                                                             _topicId),
                                           sendParameters,
                                           (byte) DiscussionEventCode.CursorEvent);
            }

            //6th phase, send ink
            if (_doc.inkData != null)
            {
                _room.PublishEventToSingle(peer,
                                           InkMessage.Write(-1,
                                                            _topicId,
                                                            _doc.inkData),
                                           sendParameters,
                                           (byte) DiscussionEventCode.InkEvent);
            }

            //7th phase, send laser pointers
            foreach (var laserPointer in _doc.LaserPointers)
            {                
                _room.PublishEventToSingle(peer,
                                           laserPointer.ToDict(),
                                           sendParameters,
                                           (byte)DiscussionEventCode.AttachLaserPointerEvent);
            }
            
            //notify client loading sequence complete
            _room.PublishEventToSingle(peer,
                                       null,
                                       sendParameters,
                                       (byte) DiscussionEventCode.SceneLoadingDone);
        }

        /// cluster engine 
        public void HandleLinkCreateRequest(LitePeer peer,
                                            OperationRequest operationRequest,
                                            SendParameters sendParameters)
        {
            var req = LinkCreateMessage.Read(operationRequest.Parameters);

            //shape             
            var link = new ServerBaseVdShape(req.shapeId, req.ownerId, VdShapeType.ClusterLink, (int) req.HeadType);
            _doc.AddShape(link);

            //topology
            _topology.Link(req.end1Id, req.end2Id, req.shapeId);

            _room.BroadcastReliableToRoom((byte) DiscussionEventCode.LinkCreateEvent,
                                          LinkCreateMessage.Write(req.end1Id, req.end2Id,
                                                                  req.ownerId, req.shapeId,
                                                                  _topicId, false, req.HeadType));

            EventLogger.LogAndBroadcast(new DiscCtx(Discussions.ConfigManager.ConnStr),
                                        _room,
                                        model.StEvent.LinkCreated,
                                        req.ownerId,
                                        req.topicId);

            //transient state until link state update, don't save 
            //pendingChanges = true;
        }

        public void HandleUnclusterBadgeRequest(LitePeer peer,
                                                OperationRequest operationRequest,
                                                SendParameters sendParameters)
        {
            var req = UnclusterBadgeMessage.Read(operationRequest.Parameters);
            _topology.UnclusterBadge(req.badgeId, req.usrId);
        }

        private void __unclusterBadge(Clusterable badge, Cluster cluster, int userId)
        {
            _room.BroadcastReliableToRoom((byte) DiscussionEventCode.UnclusterBadgeEvent,
                                          UnclusterBadgeMessage.Write(
                                              badge.GetId(),
                                              cluster.GetId(),
                                              true, _topicId, userId, -1));

            EventLogger.LogAndBroadcast(new DiscCtx(Discussions.ConfigManager.ConnStr),
                                        _room,
                                        model.StEvent.ClusterOut,
                                        userId,
                                        _topicId);
            _pendingChanges = true;
        }

        public void HandleClusterBadgeRequest(LitePeer peer,
                                              OperationRequest operationRequest,
                                              SendParameters sendParameters)
        {
            var req = ClusterBadgeMessage.Read(operationRequest.Parameters);
            if (_topology.ClusterBadge(req.badgeId, req.clusterId))
            {
                _room.Broadcast(peer,
                                operationRequest,
                                sendParameters,
                                (byte) DiscussionEventCode.ClusterBadgeEvent,
                                BroadcastTo.RoomAll); //might fail, we need message too

                EventLogger.LogAndBroadcast(new DiscCtx(Discussions.ConfigManager.ConnStr),
                                            _room,
                                            model.StEvent.ClusterIn,
                                            req.ownerId,
                                            req.topicId);
            }
            else
            {
                _log.Info("cluster badge request failed badgeId=" + req.badgeId +
                          "clusterId=" + req.clusterId);
            }

            _pendingChanges = true;
        }

        public void HandleInkRequest(LitePeer peer,
                                     OperationRequest operationRequest,
                                     SendParameters sendParameters)
        {
            var req = InkMessage.Read(operationRequest.Parameters);

            _doc.inkData = req.inkData;

            _room.Broadcast(peer,
                            operationRequest,
                            sendParameters,
                            (byte) DiscussionEventCode.InkEvent,
                            BroadcastTo.RoomExceptSelf);

            _pendingChanges = true;
        }

        #region laser pointers

        public void HandleAttachLaserPointer(LitePeer peer,
                                             LaserPointer pointer, 
                                             OperationRequest operationRequest,
                                             SendParameters sendParameters)
        {
            if(_doc.AttachLaserPointer(pointer))
            {
                _room.Broadcast(peer, 
                                operationRequest, 
                                sendParameters,
                                (byte)DiscussionEventCode.AttachLaserPointerEvent,
                                BroadcastTo.RoomExceptSelf);
            }
        }

        public void HandleDetachLaserPointer(LitePeer peer,
                                             LaserPointer pointer,
                                             OperationRequest operationRequest,
                                             SendParameters sendParameters)
        {                        
            if (_doc.DetachLaserPointer(pointer))
            {
                _room.Broadcast(peer,
                                operationRequest,
                                sendParameters,
                                (byte)DiscussionEventCode.DetachLaserPointerEvent,
                                BroadcastTo.RoomExceptSelf);
            }
        }

        public void HandleLaserPointerMoved(LitePeer peer,
                                            LaserPointer ptr,
                                            OperationRequest operationRequest,
                                            SendParameters sendParameters)
        {
            if (_doc.MoveLaserPointer(ptr))
            {
                _room.Broadcast(peer,
                                operationRequest,
                                sendParameters,
                                (byte)DiscussionEventCode.LaserPointerMovedEvent,
                                BroadcastTo.RoomExceptSelf);
            }
        }
        #endregion

        public void HandleManipulateImageViewer(LitePeer peer,
                                                ImageViewerMatrix imgMatrix,
                                                OperationRequest operationRequest,
                                                SendParameters sendParameters)
        {
            _doc.SetImageViewer(imgMatrix);
            
            _room.Broadcast(peer,
                            operationRequest,
                            sendParameters,
                            (byte)DiscussionEventCode.ImageViewerManipulatedEvent,
                            BroadcastTo.RoomExceptSelf);            
        }

        public void HandleImageViewerStateRequest(LitePeer peer,
                                                ImageViewerStateRequest req,
                                                OperationRequest operationRequest,
                                                SendParameters sendParameters)
        {
            var state  = _doc.GetImageViewer(req.ImageAttachmentId);
            if (state!=null)
                _room.PublishEventToSingle(peer, 
                                           state.ToDict(), 
                                           sendParameters, 
                                           (byte)DiscussionEventCode.ImageViewerManipulatedEvent);                       
        }

        public void HandleBrowserScrollSubmitted(LitePeer peer,
                                               BrowserScrollPosition req,
                                               OperationRequest operationRequest,
                                               SendParameters sendParameters)
        {            
            _doc.BrowserScrollbarPosition = req;
            _room.Broadcast(peer,
                            req.ToDict(),
                            sendParameters,
                            (byte)DiscussionEventCode.BrowserScrollChangedEvent);
        }

        public void HandleBrowserScrollRequested(LitePeer peer,
                                                 OperationRequest operationRequest,
                                                 SendParameters sendParameters)
        {            
            if (_doc.BrowserScrollbarPosition != null)
            {
                _room.PublishEventToSingle(peer,
                        _doc.BrowserScrollbarPosition.ToDict(),
                        sendParameters,
                        (byte)DiscussionEventCode.BrowserScrollChangedEvent);
            }
        }


        #region reporting

        public void HandleDEditorStatsRequest(LitePeer peer,
                                              OperationRequest operationRequest,
                                              SendParameters sendParameters)
        {
            var req = DEditorStatsRequest.Read(operationRequest.Parameters);
            var resp = _topology.CollectStats(req);
            resp.TopicId = req.topicId;

            //operation response has bug and doesn't send response. use event instead
            _room.Broadcast(peer,
                            resp.ToDict(),
                            sendParameters,
                            (byte) DiscussionEventCode.DEditorReportEvent,
                            BroadcastTo.RoomAll);
        }


        /// <summary>
        /// Both cluster and link keep Id of text caption in ints[0]
        /// </summary>
        /// <param name="captionHostSh"></param>
        /// <returns></returns>
        private string TryGetTextCaption(IServerVdShape captionHostSh)
        {
            var st = captionHostSh.GetState();
            var captionShId = st.ints[0];
            if (captionShId != -1)
            {
                //caption exists
                var captionSh = _doc.TryGetShape(captionShId);
                if (captionSh != null)
                {
                    var st2 = captionSh.GetState();
                    if (st2.bytes != null)
                    {
                        using (var s = new MemoryStream(st2.bytes))
                        {
                            using (var br = new BinaryReader(s))
                            {
                                return br.ReadString();
                            }
                        }
                    }
                }
            }
            return null;
        }

        private int BadgeShapeIdToArgPointId(int badgeShId)
        {
            var badgeSh = _doc.TryGetShape(badgeShId);
            if (badgeSh != null)
                return badgeSh.Tag();
            return -1;
        }

        public void HandleClusterStatsRequest(LitePeer peer,
                                              OperationRequest operationRequest,
                                              SendParameters sendParameters)
        {
            var req = ClusterStatsRequest.Read(operationRequest.Parameters);

            var resp = default(ClusterStatsResponse);

            //cluster is under rebuild
            if (!_topology.ReportCluster(req.clusterId, out resp))
            {
                _room.Broadcast(peer,
                                (Dictionary<byte, object>) null,
                                sendParameters,
                                (byte) DiscussionEventCode.ClusterStatsEvent,
                                BroadcastTo.RoomAll);
                return;
            }

            resp.clusterTextTitle = "<No text name " + req.clusterId + ">";
            resp.topicId = req.topicId;

            //get cluster shape
            var clusterSh = _doc.TryGetShape(resp.clusterId);
            if (clusterSh == null)
            {
                _room.Broadcast(peer,
                                (Dictionary<byte, object>) null,
                                sendParameters,
                                (byte) DiscussionEventCode.ClusterStatsEvent,
                                BroadcastTo.RoomAll);
                return;
            }


            resp.clusterShId = clusterSh.Id();
            resp.clusterTextTitle = TryGetTextCaption(clusterSh); //get text caption shape id
            resp.initialOwnerId = clusterSh.InitialOwner();

            //badge id -> arg.point id
            var clusteredPoints = new List<int>();
            for (int i = 0; i < resp.points.Length; i++)
            {
                var apId = BadgeShapeIdToArgPointId(resp.points[i]);
                if (apId != -1)
                    clusteredPoints.Add(apId);
            }

            if (clusteredPoints.Count() == 0)
            {
                _room.Broadcast(peer,
                                (Dictionary<byte, object>) null,
                                sendParameters,
                                (byte) DiscussionEventCode.ClusterStatsEvent,
                                BroadcastTo.RoomAll);
            }
            else
            {
                resp.points = clusteredPoints.ToArray();
                _room.Broadcast(peer,
                                resp.ToDict(),
                                sendParameters,
                                (byte) DiscussionEventCode.ClusterStatsEvent,
                                BroadcastTo.RoomAll);
            }
        }


        public void HandleLinkReportRequest(LitePeer peer,
                                            OperationRequest operationRequest,
                                            SendParameters sendParameters)
        {
            var req = LinkReportRequest.Read(operationRequest.Parameters);

            var linkSh = _doc.TryGetShape(req.LinkShapeId);
            if (linkSh == null)
            {
                _room.Broadcast(peer,
                                (Dictionary<byte, object>) null,
                                sendParameters,
                                (byte) DiscussionEventCode.LinkStatsEvent,
                                BroadcastTo.RoomAll);
                return;
            }

            var resp = default(LinkReportResponse);
            resp.linkShId = linkSh.Id();
            resp.Caption = TryGetTextCaption(linkSh);
            resp.topicId = req.TopicId;
            resp.initialOwner = linkSh.InitialOwner();

            var fwdEdge = _topology.GetForwardEdge(linkSh.Id());

            //endpoint 1
            if (fwdEdge.curr is Clusterable)
            {
                resp.EndpointArgPoint1 = true;
                resp.ArgPointId1 = fwdEdge.curr.GetId();

                //badge id -> arg.point id
                var badgeSh = _doc.TryGetShape(resp.ArgPointId1);
                if (badgeSh == null)
                {
                    _room.Broadcast(peer,
                                    (Dictionary<byte, object>) null,
                                    sendParameters,
                                    (byte) DiscussionEventCode.LinkStatsEvent,
                                    BroadcastTo.RoomAll);
                    return;
                }
                resp.ArgPointId1 = badgeSh.Tag();
                resp.ClusterCaption1 = null;
            }
            else
            {
                resp.EndpointArgPoint1 = false;
                resp.ArgPointId1 = -1;

                var clusterSh = _doc.TryGetShape(fwdEdge.curr.GetId());
                if (clusterSh == null)
                {
                    _room.Broadcast(peer,
                                    (Dictionary<byte, object>) null,
                                    sendParameters,
                                    (byte) DiscussionEventCode.ClusterStatsEvent,
                                    BroadcastTo.RoomAll);
                    return;
                }

                resp.ClusterCaption1 = TryGetTextCaption(clusterSh);
                resp.IdOfCluster1 = clusterSh.Id();
            }

            //endpoint 2
            if (fwdEdge.next is Clusterable)
            {
                resp.EndpointArgPoint2 = true;
                resp.ArgPointId2 = fwdEdge.next.GetId();

                //badge id -> arg.point id
                var badgeSh = _doc.TryGetShape(resp.ArgPointId2);
                if (badgeSh == null)
                {
                    _room.Broadcast(peer,
                                    (Dictionary<byte, object>) null,
                                    sendParameters,
                                    (byte) DiscussionEventCode.LinkStatsEvent,
                                    BroadcastTo.RoomAll);
                    return;
                }
                resp.ArgPointId2 = badgeSh.Tag();
                resp.ClusterCaption2 = null;
            }
            else
            {
                resp.EndpointArgPoint2 = false;
                resp.ArgPointId2 = -1;

                var clusterSh = _doc.TryGetShape(fwdEdge.next.GetId());
                if (clusterSh == null)
                {
                    _room.Broadcast(peer,
                                    (Dictionary<byte, object>) null,
                                    sendParameters,
                                    (byte) DiscussionEventCode.ClusterStatsEvent,
                                    BroadcastTo.RoomAll);
                    return;
                }

                resp.ClusterCaption2 = TryGetTextCaption(clusterSh);
                resp.IdOfCluster2 = clusterSh.Id();
            }

            _room.Broadcast(peer,
                            resp.ToDict(),
                            sendParameters,
                            (byte) DiscussionEventCode.LinkStatsEvent,
                            BroadcastTo.RoomAll);
        }

        #endregion reporting
    }
}