using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiteLobby;
using Lite;
using LiteLobby.Operations;
using Photon.SocketServer;
using Discussions.RTModel.Operations;
using Discussions.RTModel.Model;
using Lite.Events;
using Discussions.DbModel;
using ExitGames.Logging;
using Discussions.RTModel;
using Lite.Messages;

namespace Discussions.RTModel
{
    //each room is devoted to single discussion
    public class DiscussionRoom : LiteLobbyGame
    {
        public const int ANNOT_PERSIST_PERIOD = 5000; //ms
                
        //discussion id in DB
        int _discussionId;
        public int DiscId
        {
            get
            {
                return _discussionId;
            }
        }

        static readonly ILogger _log = LogManager.GetCurrentClassLogger();

        //maps topic id to its editor 
        Dictionary<int, VectProcessor> _vectEditors = new Dictionary<int, VectProcessor>();
        public VectProcessor VectEditor(int topicId)
        {
            if(!_vectEditors.ContainsKey(topicId))
                _vectEditors.Add(topicId, new VectProcessor(topicId, this));
                    
            return _vectEditors[topicId];           
        }

        public DiscussionRoom(string gameName, string lobbyName)
            : base(gameName, lobbyName)
        {
            _discussionId = extractDiscussionId(gameName);

            SchedulePersist();
        }
        
        static int extractDiscussionId(string roomName)
        {
            //gameName follows "discussion#" + discussionId;
            string[] roomNameParts = roomName.Split('#');
            return int.Parse(roomNameParts.Last());
        }

        protected override void ExecuteOperation(LitePeer peer, OperationRequest operationRequest,
                                                 SendParameters sendParameters)
        {
            switch (operationRequest.OperationCode)
            {
                case (byte)DiscussionOpCode.NotifyStructureChanged:
                        HandleNotifyStructureChanged(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.NotifyArgPointChanged: 
                        HandleNotifyArgPointChanged(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.NotifyUserAccPlusMinus:
                        HandleUserAccPlusMinus(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.StatsEvent:
                        HandleStatsEvent(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.NotifyNameChanged:
                        HandleNameChanged(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.CursorRequest:
                        var topicId = Serializers.ReadChangedTopicId(operationRequest.Parameters);
                        VectEditor(topicId).HandleCursorRequest(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.CreateShapeRequest:
                        topicId = Serializers.ReadChangedTopicId(operationRequest.Parameters);
                        VectEditor(topicId).HandleCreateShape(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.DeleteShapesRequest:
                        topicId = Serializers.ReadChangedTopicId(operationRequest.Parameters);
                        VectEditor(topicId).HandleDeleteShapes(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.UnselectAllRequest:
                        topicId = Serializers.ReadChangedTopicId(operationRequest.Parameters);
                        VectEditor(topicId).HandleUnselectAll(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.DeleteSingleShapeRequest:
                        topicId = Serializers.ReadChangedTopicId(operationRequest.Parameters);
                        VectEditor(topicId).HandleDeleteSingleShape(peer, operationRequest, sendParameters);
                        break;                
                case (byte)DiscussionOpCode.StateSyncRequest:
                        topicId = Serializers.ReadChangedTopicId(operationRequest.Parameters);
                        VectEditor(topicId).HandleStateSync(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.InitialSceneLoadRequest:
                        topicId = Serializers.ReadChangedTopicId(operationRequest.Parameters);
                        VectEditor(topicId).HandleInitialSceneLoad(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.LinkCreateRequest:
                        topicId = Serializers.ReadChangedTopicId(operationRequest.Parameters);
                        VectEditor(topicId).HandleLinkCreateRequest(peer, operationRequest, sendParameters);
                        break;             
                case (byte)DiscussionOpCode.UnclusterBadgeRequest:
                        topicId = Serializers.ReadChangedTopicId(operationRequest.Parameters);
                        VectEditor(topicId).HandleUnclusterBadgeRequest(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.ClusterBadgeRequest: 
                        topicId = Serializers.ReadChangedTopicId(operationRequest.Parameters);
                        VectEditor(topicId).HandleClusterBadgeRequest(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.InkRequest:
                        topicId = Serializers.ReadChangedTopicId(operationRequest.Parameters);
                        VectEditor(topicId).HandleInkRequest(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.DEditorReport:            
                        topicId = Serializers.ReadChangedTopicId(operationRequest.Parameters);
                        VectEditor(topicId).HandleDEditorStatsRequest(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.ClusterStatsRequest:                        
                        topicId = Serializers.ReadChangedTopicId(operationRequest.Parameters);
                        VectEditor(topicId).HandleClusterStatsRequest(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.LinkReportRequest:
                        topicId = Serializers.ReadChangedTopicId(operationRequest.Parameters);
                        VectEditor(topicId).HandleLinkReportRequest(peer, operationRequest, sendParameters);
                        break;
                case (byte)DiscussionOpCode.BadgeViewRequest:
                        HandleBadgeView(peer, operationRequest, sendParameters);
                        break;
                default:
                    base.ExecuteOperation(peer, operationRequest, sendParameters);
                    break;
            }
        }
        
        protected override void ProcessMessage(IMessage message)
        {           
            switch ((DiscussionMsgCode)message.Action)
            {
                case DiscussionMsgCode.CheckPersistAnnotations:
                    foreach (var vp in _vectEditors.Values)                    
                        vp.CheckPersist();
                   
                    //sched next message  
                    SchedulePersist();
                    break;     
                default:
                    base.ProcessMessage(message);
                    break;
            }           
        }

        public void SchedulePersist()
        {
            var message = new RoomMessage((byte)DiscussionMsgCode.CheckPersistAnnotations);
            ScheduleMessage(message, ANNOT_PERSIST_PERIOD); 
        }

        public void PublishDiscussionEvent(byte eventCode, Dictionary<byte, object> data,
                                           IEnumerable<Actor> actorList,
                                           SendParameters sendParameters)
        {
            IEnumerable<PeerBase> peers = actorList.Select(actor => actor.Peer);
            var eventData = new EventData(eventCode, data);
            eventData.SendTo(peers, sendParameters);            
        }

        public void PublishEventToSingle(LitePeer peer,
                                         Dictionary<byte, object> data,
                                         SendParameters sendParameters,
                                         byte EventCode)
        {
            Actor actor = this.GetActorByPeer(peer);
            PublishDiscussionEvent(EventCode,
                                    data,
                                    new Actor[]{actor},
                                    sendParameters);
        }

        public void BroadcastReliableToRoom(byte eventCode, Dictionary<byte, object> data)
        {
            var eventData = new EventData(eventCode, data);
            var sendParams = new SendParameters();
            eventData.SendTo(Actors.Select(actor=>actor.Peer), sendParams);
        }

        public void Broadcast(LitePeer peer,
                             Dictionary<byte, object> data,
                             SendParameters sendParameters,
                             byte EventCode,
                             BroadcastTo addresses = BroadcastTo.RoomExceptSelf)
        {
            IEnumerable<Actor> recipients;

            // get the actor who send the operation request
            if (addresses==BroadcastTo.RoomExceptSelf)
            {
                Actor actor = this.GetActorByPeer(peer);
                if (actor == null)
                {
                    return;
                }
                recipients = this.Actors.GetExcludedList(actor);
            }
            else
            {
                //peer==null ok
                recipients = this.Actors;
            }

            PublishDiscussionEvent(EventCode,
                                    data,
                                    recipients,
                                    sendParameters);
        }

        public void Broadcast(LitePeer peer,
                            OperationRequest operationRequest,
                            SendParameters sendParameters,
                            byte EventCode,
                            BroadcastTo addresses = BroadcastTo.RoomExceptSelf)
        {
            Broadcast(peer, operationRequest.Parameters, sendParameters, EventCode, addresses);
        }

        void HandleNotifyArgPointChanged(LitePeer peer,
                                        OperationRequest operationRequest,
                                        SendParameters sendParameters)
        {
            PointChangedType pointChangeType;
            int topicId; 
            var pointId = Serializers.ReadChangedArgPoint(operationRequest.Parameters, out pointChangeType, out topicId);            
            switch (pointChangeType)
            {
                case PointChangedType.Created:
                    VectEditor(topicId).HandleBadgeCreated(pointId, peer, operationRequest, sendParameters);
                    break;
                case PointChangedType.Deleted:
                    VectEditor(topicId).HandleBadgeDeleted(pointId, peer, operationRequest, sendParameters);
                    break;
                case PointChangedType.Modified:
                    VectEditor(topicId).HandleBadgeModified(pointId, peer, operationRequest, sendParameters);
                    break;
                default:
                    throw new NotSupportedException();
            }
            
            //broadcast initial event anyway 
            Broadcast(peer, operationRequest, sendParameters,
                      (byte)DiscussionEventCode.ArgPointChanged,
                      BroadcastTo.RoomAll); 
        }
        
        void HandleNotifyStructureChanged(LitePeer peer,
                                          OperationRequest operationRequest,
                                          SendParameters sendParameters)
        {
            Broadcast(peer, operationRequest, sendParameters,
                         (byte)DiscussionEventCode.StructureChanged, BroadcastTo.RoomAll);
        }

        void HandleUserAccPlusMinus(LitePeer peer,
                                    OperationRequest operationRequest,
                                    SendParameters sendParameters)
        {
            Broadcast(peer, operationRequest, sendParameters,
                         (byte)DiscussionEventCode.UserAccPlusMinus, BroadcastTo.RoomAll);
        }

        void HandleNameChanged(LitePeer peer,
                               OperationRequest operationRequest,
                               SendParameters sendParameters)
        {
            Broadcast(peer, operationRequest, sendParameters,
                         (byte)DiscussionEventCode.UserAccPlusMinus, BroadcastTo.RoomAll);
        }

        void HandleStatsEvent(LitePeer peer,
                              OperationRequest operationRequest,
                              SendParameters sendParameters)
        {                        
            Broadcast(peer, operationRequest, sendParameters,
                     (byte)DiscussionEventCode.StatsEvent, BroadcastTo.RoomAll);
        }

        void HandleBadgeView(LitePeer peer,
                             OperationRequest operationRequest,
                             SendParameters sendParameters)
        {                        
            Broadcast(peer, operationRequest, sendParameters,
                     (byte)DiscussionEventCode.BadgeViewEvent, BroadcastTo.RoomExceptSelf);
        }
    }
}
