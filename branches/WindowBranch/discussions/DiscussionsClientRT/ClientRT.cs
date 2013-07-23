using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Discussions.RTModel.Operations;
using System.Collections;
using ExitGames.Client.Photon.Lite;
using Discussions.RTModel.Model;
using Lite.Operations;
using Discussions.model;
using DistributedEditor;

namespace DiscussionsClientRT
{
    public class ClientRT : IPhotonPeerListener
    {
        public static readonly string LOBBY = "discussion_lobby";

        public delegate void OnJoin();

        public OnJoin onJoin;

        public delegate void StructureChanged(int activeTopic, int initiaterId, DeviceType devType);

        public StructureChanged onStructChanged;

        public delegate void UserLeaves(DiscUser usr);

        public UserLeaves userLeaves;

        public delegate void UserJoins(DiscUser usr);

        public UserJoins userJoins;

        public delegate void OnlineListChanged(IEnumerable<DiscUser> onlineUsers);

        public OnlineListChanged onlineListChanged;

        //public delegate void SrvGeometryRequest();
        //public SrvGeometryRequest srvGeometryRequest;

        public delegate void ArgPointChanged(int ArgPointId, int topicId, PointChangedType change);

        public ArgPointChanged argPointChanged;

        public delegate void InstantSomebodyLeaved();

        public InstantSomebodyLeaved smbdLeaved;

        public delegate void UserAccPlusMinus();

        public UserAccPlusMinus userAccPlusMinus;

        public delegate void OnStatsEvent(StEvent e, int userId, int discussionId,
                                          int topicId, DeviceType devType);

        public OnStatsEvent onStatsEvent;

        public delegate void OnBadgeViewRequest(BadgeViewMessage bv);

        public OnBadgeViewRequest onBadgeViewRequest;

        public delegate void OnSourceViewRequest(ExplanationModeSyncMsg sv);

        public OnSourceViewRequest onSourceViewRequest;

        #region vector editor

        public delegate void OnLinkCreateEvent(LinkCreateMessage ev);

        public OnLinkCreateEvent onLinkCreateEvent;

        public delegate void OnUnclusterBadgeEvent(UnclusterBadgeMessage ev);

        public OnUnclusterBadgeEvent onUnclusterBadgeEvent;

        public delegate void OnClusterBadgeEvent(ClusterBadgeMessage ev);

        public OnClusterBadgeEvent onClusterBadgeEvent;

        public delegate void OnClusterCreateEvent(ClusterCreateMessage ev);

        public OnClusterCreateEvent onClusterCreateEvent;

        public delegate void OnCursorEvent(CursorEvent ev);

        public OnCursorEvent cursorEvent;

        public delegate void OnCreateShapeEvent(CreateShape ev);

        public OnCreateShapeEvent createShapeEvent;

        public delegate void OnUnselectAll(UnselectAllEvent ev);

        public OnUnselectAll unselectAll;

        public delegate void OnDeleteSingleShape(DeleteSingleShapeEvent ev);

        public OnDeleteSingleShape deleteSingleShape;

        public delegate void OnApplyPoint(PointMove ev);

        public OnApplyPoint applyPoint;

        public delegate void SyncStateEvent(ShapeState st);
        public SyncStateEvent syncStateEvent;

        public delegate void InkEvent(InkMessage ink);
        public InkEvent inkEvent;

        public delegate void LaserPointerEvent(LaserPointer ptr);
        public LaserPointerEvent onAttachLaserPointer;
        public LaserPointerEvent onDetachLaserPointer;

        public delegate void LaserPointerMovedEvent(LaserPointer ptr);
        public LaserPointerMovedEvent onLaserPointerMoved;

        public delegate void LoadingDoneEvent();

        public LoadingDoneEvent loadingDoneEvent;

        #endregion vector editor

        #region reporting

        public delegate void DEditorReportResponse(DEditorStatsResponse resp);

        public DEditorReportResponse dEditorReportResponse;

        public delegate void ClusterStatsResponseEvent(ClusterStatsResponse resp, bool ok);

        public ClusterStatsResponseEvent clusterStatsResponse;

        public delegate void LinkStatsResponseEvent(LinkReportResponse resp, bool ok);

        public LinkStatsResponseEvent linkStatsResponseEvent;

        public delegate void OnScreenshotResponse(Dictionary<int, byte[]> resp);

        public OnScreenshotResponse onScreenshotResponse;

        #endregion reporting

        public delegate void OnCommentRead(CommentsReadEvent ev);

        public OnCommentRead onCommentRead;

        private LiteLobbyPeer peer;
        private Hashtable gameList = new Hashtable();
        private Dictionary<int, DiscUser> usersOnline = new Dictionary<int, DiscUser>();

        private int discussionId;
        private string dbSrvAddr;
        private DeviceType devType;

        private DiscUser localUsr = new DiscUser();

        private static string APP_NAME = "DiscussionsRT";

        public ClientRT(int discussionId, string dbSrvAddr, string UsrName, int usrDbId, DeviceType devType)
        {
            this.discussionId = discussionId;
            this.dbSrvAddr = dbSrvAddr;
            this.localUsr.Name = UsrName;
            this.localUsr.usrDbId = usrDbId;
            this.devType = devType;

            peer = new LiteLobbyPeer(this);

            Connect();
        }

        public void Connect()
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                if (!peer.Connect(Discussions.ConfigManager.PhotonSrv, APP_NAME))
                    throw new Exception("Unknown photon hostname!");
        }

        public void Service()
        {
            if (peer != null)
                peer.Service();
        }

        public void Stop()
        {
            if (peer != null)
            {
                peer.Disconnect();
                peer = null;
            }
        }

        private void DbgPrintOnlineList()
        {
            if (onlineListChanged != null)
                onlineListChanged(usersOnline.Values);

            Console.WriteLine(">>************************");
            foreach (var v in usersOnline.Values)
                Console.WriteLine("{0}({1})", v.Name, v.ActNr);
            Console.WriteLine("<<************************");
        }

        #region IPhotonPeerListener

        public void DebugReturn(DebugLevel level, string message)
        {
            // throw new NotImplementedException();
        }

        public void OnEvent(EventData eventData)
        {
            switch (eventData.Code)
            {
                case (byte)LiteEventCode.Join:
                    int[] actNrs = (int[])eventData.Parameters[(byte)ParameterKey.Actors];
                    int ActorNr = (int)eventData.Parameters[(byte)ParameterKey.ActorNr];
                    var actProps = (Hashtable)eventData.Parameters[(byte)ParameterKey.ActorProperties];

                    Console.WriteLine("Join event actors.len={0}", actNrs.Length);
                    List<int> unknownPeersNrs = new List<int>();
                    for (int i = 0; i < actNrs.Length; i++)
                    {
                        if (!usersOnline.ContainsKey(actNrs[i]))
                        {
                            if (!unknownPeersNrs.Contains(actNrs[i]))
                                unknownPeersNrs.Add(actNrs[i]);
                        }
                    }
                    requestPeersInfo(unknownPeersNrs.ToArray());
                    DbgPrintOnlineList();
                    break;

                case (byte)EventCode.Leave:
                    actNrs = (int[])eventData.Parameters[(byte)ParameterKey.Actors];
                    int leftActNr = (int)eventData.Parameters[(byte)ParameterKey.ActorNr];
                    Console.WriteLine("Leave event, actors.len={0}", actNrs.Length);
                    if (usersOnline.ContainsKey(leftActNr))
                    {
                        if (userLeaves != null)
                        {
                            DiscUser leaving = usersOnline[leftActNr];
                            userLeaves(leaving);
                        }
                        usersOnline.Remove(leftActNr);
                    }
                    DbgPrintOnlineList();
                    break;

                case (byte)DiscussionEventCode.InstantUserPlusMinus:
                    if (smbdLeaved != null)
                        smbdLeaved();
                    break;
                case (byte)DiscussionEventCode.StructureChanged:
                    int initiater = (int)eventData.Parameters[(byte)DiscussionParamKey.UserId];
                    int devType = (int)eventData.Parameters[(byte)DiscussionParamKey.DeviceType];
                    if (eventData.Parameters.ContainsKey((byte)DiscussionParamKey.ForceSelfNotification))
                    {
                        //topic updated 
                        if (onStructChanged != null)
                            onStructChanged(Serializers.ReadChangedTopicId(eventData.Parameters),
                                            initiater, (DeviceType)devType);
                    }
                    else if (initiater != -1 && initiater != localUsr.ActNr)
                    {
                        if (onStructChanged != null)
                            onStructChanged(Serializers.ReadChangedTopicId(eventData.Parameters),
                                            initiater, (DeviceType)devType);
                    }
                    break;
                case (byte)DiscussionEventCode.ArgPointChanged:
                    PointChangedType changeType = PointChangedType.Modified;
                    int topicId;
                    int argPointId = Serializers.ReadChangedArgPoint(eventData.Parameters, out changeType, out topicId);
                    if (argPointChanged != null)
                        argPointChanged(argPointId, topicId, changeType);
                    break;
                case (byte)DiscussionEventCode.UserAccPlusMinus:
                    if (userAccPlusMinus != null)
                        userAccPlusMinus();
                    break;
                case (byte)DiscussionEventCode.StatsEvent:
                    if (onStatsEvent != null)
                    {
                        StEvent e;
                        int userId = -1;
                        int discussionId = -1;
                        int statsTopicId = -1;
                        DeviceType devTyp;
                        Serializers.ReadStatEventParams(eventData.Parameters, out e, out userId, out discussionId,
                                                        out statsTopicId, out devTyp);
                        onStatsEvent(e, userId, discussionId, statsTopicId, devTyp);
                    }
                    break;
                case (byte)DiscussionEventCode.CursorEvent:
                    if (cursorEvent != null)
                        cursorEvent(CursorEvent.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.CreateShapeEvent:
                    if (createShapeEvent != null)
                        createShapeEvent(CreateShape.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.UnselectAllEvent:
                    if (unselectAll != null)
                        unselectAll(UnselectAllEvent.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.DeleteSingleShapeEvent:
                    if (deleteSingleShape != null)
                        deleteSingleShape(DeleteSingleShapeEvent.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.ApplyPointEvent:
                    if (applyPoint != null)
                        applyPoint(PointMove.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.StateSyncEvent:
                    if (syncStateEvent != null)
                        syncStateEvent(ShapeState.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.LinkCreateEvent:
                    if (onLinkCreateEvent != null)
                        onLinkCreateEvent(LinkCreateMessage.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.UnclusterBadgeEvent:
                    if (onUnclusterBadgeEvent != null)
                        onUnclusterBadgeEvent(UnclusterBadgeMessage.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.ClusterBadgeEvent:
                    if (onClusterBadgeEvent != null)
                        onClusterBadgeEvent(ClusterBadgeMessage.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.InkEvent:
                    if (inkEvent != null)
                        inkEvent(InkMessage.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.SceneLoadingDone:
                    if (loadingDoneEvent != null)
                        loadingDoneEvent();
                    break;
                case (byte)DiscussionEventCode.DEditorReportEvent:
                    if (dEditorReportResponse != null)
                        dEditorReportResponse(DEditorStatsResponse.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.ClusterStatsEvent:
                    if (clusterStatsResponse != null)
                    {
                        if (eventData.Parameters == null || eventData.Parameters.Count() == 0)
                            clusterStatsResponse(default(ClusterStatsResponse), false);
                        else
                            clusterStatsResponse(ClusterStatsResponse.Read(eventData.Parameters), true);
                    }
                    break;
                case (byte)DiscussionEventCode.LinkStatsEvent:
                    if (linkStatsResponseEvent != null)
                    {
                        if (eventData.Parameters == null || eventData.Parameters.Count() == 0)
                            linkStatsResponseEvent(default(LinkReportResponse), false);
                        else
                            linkStatsResponseEvent(LinkReportResponse.Read(eventData.Parameters), true);
                    }
                    break;
                case (byte)DiscussionEventCode.BadgeViewEvent:
                    if (onBadgeViewRequest != null)
                        onBadgeViewRequest(BadgeViewMessage.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.SourceViewEvent:
                    if (onSourceViewRequest != null)
                        onSourceViewRequest(ExplanationModeSyncMsg.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.CommentReadEvent:
                    if (onCommentRead != null)
                        onCommentRead(CommentsReadEvent.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.AttachLaserPointerEvent:
                    if (onAttachLaserPointer != null)
                        onAttachLaserPointer(LaserPointer.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.DetachLaserPointerEvent:
                    if (onDetachLaserPointer != null)
                        onDetachLaserPointer(LaserPointer.Read(eventData.Parameters));
                    break;
                case (byte)DiscussionEventCode.LaserPointerMovedEvent:
                    if (onLaserPointerMoved != null)
                        onLaserPointerMoved(LaserPointer.Read(eventData.Parameters));
                    break;
                default:
                    Console.WriteLine("Unhandled event " + eventData.Code);
                    break;
            }
        }

        public void OnOperationResponse(OperationResponse operationResponse)
        {
            switch (operationResponse.OperationCode)
            {
                case (byte)DiscussionOpCode.Test:
                    //Console.WriteLine(operationResponse.Parameters[(byte)DiscussionParamKey.Message]);
                    break;

                case (byte)LiteOpCode.Join:
                    Console.WriteLine("OpResp: Join " + operationResponse.Parameters);
                    if (operationResponse.Parameters.ContainsKey((byte)LiteOpKey.ActorNr))
                        this.localUsr.ActNr = (int)operationResponse.Parameters[(byte)LiteOpKey.ActorNr];
                    if (onJoin != null)
                        onJoin();
                    break;

                case (byte)LiteOpCode.Leave:
                    ResetConnState();
                    break;

                case (byte)OperationCode.GetProperties:
                    const byte magic = 249;
                    Hashtable resp = (Hashtable)operationResponse.Parameters[(byte)magic];

                    foreach (var k in resp.Keys)
                    {
                        int ActorNr = (int)k;
                        Hashtable props = (Hashtable)resp[k];
                        string name = (string)props[(byte)ActProps.Name];
                        int usrDbId = (int)props[(byte)ActProps.DbId];

                        DiscUser usr;
                        if (usersOnline.ContainsKey(ActorNr))
                        {
                            usr = usersOnline[ActorNr];
                            usr.Name = name;
                            usr.usrDbId = usrDbId;
                        }
                        else
                        {
                            usr = new DiscUser(name, ActorNr);
                            usr.usrDbId = usrDbId;
                            usersOnline.Add(ActorNr, usr);

                            if (userJoins != null)
                                userJoins(usr);
                        }

                        DbgPrintOnlineList();
                    }
                    break;

                case (byte)DiscussionOpCode.ScreenshotRequest:
                    if (onScreenshotResponse != null)
                        onScreenshotResponse(ScreenshotResponse.Read(operationResponse.Parameters).screenshots);
                    break;
                default:
                    //Console.WriteLine("Unhandled OnOperationResponse " + operationResponse.OperationCode);
                    break;
            }
        }

        public void OnStatusChanged(StatusCode statusCode)
        {
            switch ((StatusCode)statusCode)
            {
                case StatusCode.Connect:
                    JoinRandomWithLobby();
                    break;

                case StatusCode.Disconnect:
                    //Console.Beep();

                    if (localUsr != null && usersOnline != null)
                        usersOnline.Remove(localUsr.ActNr);

                    ResetConnState();

                    //try to reconnect
                    Connect();
                    break;

                case StatusCode.ExceptionOnConnect:
                    throw new Exception("ExceptionOnConnect. Peer.state: " + peer.PeerState);

                case StatusCode.Exception:                    
                    throw new Exception("Exception. Peer.state: " + peer.PeerState);
                case StatusCode.SendError:
                    //throw new Exception("SendError! Peer.state: " + peer.PeerState);
                    break;
                default:
                    Console.WriteLine("Unhandled OnStatusChanged " + statusCode);
                    break;
            }
        }

        #endregion IPhotonPeerListener

        public void PrintDbg(Dictionary<byte, object> par)
        {
            foreach (var v in par)
            {
                Console.WriteLine(v);
            }
        }

        private void ResetConnState()
        {
            this.localUsr.ActNr = -1;
        }

        private void requestPeersInfo(int[] ActorNrs)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            if (ActorNrs.Length > 0)
            {
                var propReq = new Dictionary<byte, object>();
                propReq.Add((byte)ParameterKey.Actors, ActorNrs);
                propReq.Add((byte)ParameterKey.Properties, (byte)PropertyType.Actor);
                peer.OpCustom((byte)OperationCode.GetProperties, propReq, true);
            }
        }

        public string getDiscussionRoom()
        {
            if (discussionId == -1)
                return LOBBY;
            else
                return dbSrvAddr + "discussion#" + discussionId;
        }

        public bool IsConnected()
        {
            return peer == null && peer.PeerState == PeerStateValue.Connected;
        }

        public bool IsConnecting()
        {
            return peer != null && peer.PeerState == PeerStateValue.Connecting;
        }

        public void JoinRandomWithLobby()
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            // You can take a look at the implementation of OpJoinFromLobby in LiteLobbyPeer.cs           

            Hashtable actorProperties = new Hashtable();
            actorProperties.Add((byte)ActProps.Name, this.localUsr.Name);
            actorProperties.Add((byte)ActProps.DbId, this.localUsr.usrDbId);
            actorProperties.Add((byte)ActProps.DevType, (int)devType);
            peer.OpJoinFromLobby(getDiscussionRoom(), LOBBY, actorProperties, true);
        }

        public void SendChatMessage(string line)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            var parameter = new Dictionary<byte, object>
                {
                    {(byte) DiscussionParamKey.Message, "**************************"}
                };
            peer.OpCustom((byte)DiscussionOpCode.Test, parameter, true);
        }

        ///arg. point 
        ///other users can edit arg.point too, by editing comments
        public void SendArgPointChanged(int argPointId, int topicId)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            var parameter = Serializers.WriteChangedArgPoint(argPointId, topicId, PointChangedType.Modified);
            peer.OpCustom((byte)DiscussionOpCode.NotifyArgPointChanged, parameter, true);
        }

        public void SendArgPointCreated(int argPointId, int topicId)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            var parameter = Serializers.WriteChangedArgPoint(argPointId, topicId, PointChangedType.Created);
            peer.OpCustom((byte)DiscussionOpCode.NotifyArgPointChanged, parameter, true);
        }

        public void SendArgPointDeleted(int argPointId, int topicId)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            var parameter = Serializers.WriteChangedArgPoint(argPointId, topicId, PointChangedType.Deleted);
            peer.OpCustom((byte)DiscussionOpCode.NotifyArgPointChanged, parameter, true);
        }

        public void SendNotifyStructureChanged(int activeTopicId, int initiaterDbId, DeviceType devType)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            Dictionary<byte, object> changedTopicId = Serializers.WriteChangedTopicId(activeTopicId);
            changedTopicId.Add((byte)DiscussionParamKey.ForceSelfNotification, (byte)1);
            changedTopicId.Add((byte)DiscussionParamKey.UserId, initiaterDbId);
            changedTopicId.Add((byte)DiscussionParamKey.DeviceType, devType);

            peer.OpCustom((byte)DiscussionOpCode.NotifyStructureChanged, changedTopicId, true);
        }

        public void SendLiveRequest()
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.NotifyLeaveUser, new Dictionary<byte, object>(), true);

            //if it fails, disconnection handler on server will remove the client (with delay a couple of seconds)
            Service();
            Service();
            Service();
        }

        public void SendUserAccPlusMinus()
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.NotifyUserAccPlusMinus, new Dictionary<byte, object>(), true);
        }

        public void SendStatsEvent(StEvent e, int userId, int discussionId, int topicId, DeviceType devType)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            if (e == StEvent.LocalIgnorableEvent)
                return;

            var par = Serializers.WriteStatEventParams(e, userId, discussionId, topicId, devType);
            peer.OpCustom((byte)DiscussionOpCode.StatsEvent, par, true);
        }

        public void SendAvaNameChanged()
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.NotifyNameChanged, null, true);
        }

        public void SendCursorRequest(bool doSet, int ownerId, int shapeId, int topicId)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.CursorRequest,
                          CursorRequest.Write(doSet, ownerId, shapeId, topicId),
                          true);
        }

        public void SendCreateShapeRequest(int ownerId, int shapeId, VdShapeType shapeType, bool takeCursor,
                                           double startX, double startY, int tag, int topicId)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.CreateShapeRequest,
                          CreateShape.Write(ownerId, shapeId, shapeType, startX, startY, takeCursor, tag, topicId),
                          true);
        }

        public void SendDeleteShapesRequest(int ownerId, int initialOwnerId, int topicId)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.DeleteShapesRequest,
                          DeleteShapesRequest.Write(ownerId, initialOwnerId, topicId),
                          true);
        }

        public void SendDeleteSingleShapeRequest(int ownerId, int shapeId, int topicId)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.DeleteSingleShapeRequest,
                          DeleteSingleShapeRequest.Write(ownerId, shapeId, topicId),
                          true);
        }

        public void SendUnselectAll(int ownerId)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.UnselectAllRequest,
                          UnselectAllRequest.Write(ownerId),
                          true);
        }

        //public void SendApplyPoint(int ownerId, int shapeId, double x, double y)
        //{
        //    if (peer == null || peer.PeerState != PeerStateValue.Connected)
        //        return;

        //    peer.OpCustom((byte)DiscussionOpCode.ApplyPointRequest,
        //                   PointMove.Write(ownerId, shapeId, x, y),
        //                   false);
        //}

        public void SendSyncState(int shapeId, ShapeState st)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.StateSyncRequest,
                          st.ToDict(),
                          true);
            Service();
        }

        public void SendInitialSceneLoadRequest(int topicId)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.InitialSceneLoadRequest,
                          InitialSceneLoadRequest.Write(topicId),
                          true);
        }

        public void SendLinkCreateRequest(int end1, int end2, int ownerId, int shapeId, int topicId, bool takeCursor,
                                          LinkHeadType linkHead)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.LinkCreateRequest,
                          LinkCreateMessage.Write(end1, end2, ownerId, shapeId, topicId, takeCursor, linkHead),
                          true);
            Service();
        }

        public void SendUnclusterBadgeRequest(int badgeId, int clusterId, int topicId, int usrId, bool playImmidiately,
                                              int callToken)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.UnclusterBadgeRequest,
                          UnclusterBadgeMessage.Write(badgeId, clusterId, playImmidiately,
                                                      topicId, usrId, callToken),
                          true);
            Service();
        }

        public void SendClusterBadgeRequest(int badgeId, int clusterId, int ownerId, int topicId, bool playImmidiately,
                                            int callToken)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.ClusterBadgeRequest,
                          ClusterBadgeMessage.Write(badgeId, ownerId, playImmidiately, clusterId, topicId, callToken),
                          true);
        }

        public void SendInkRequest(int ownerId, int topicId, byte[] inkData)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.InkRequest,
                          InkMessage.Write(ownerId, topicId, inkData),
                          true);
        }

        public void SendDEditorRequest(int topicId)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.DEditorReport,
                          DEditorStatsRequest.Write(topicId),
                          true);
        }

        public void SendClusterStatsRequest(int clusterId, int topicId)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.ClusterStatsRequest,
                          ClusterStatsRequest.Write(clusterId, topicId),
                          true);
        }

        public void SendLinkStatsRequest(int linkId, int topicId)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.LinkReportRequest,
                          LinkReportRequest.Write(linkId, topicId),
                          true);
        }

        public void SendBadgeViewRequest(int argPointId, bool doExpand)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.BadgeViewRequest,
                          BadgeViewMessage.Write(argPointId, doExpand),
                          true);
        }

        public void SendExplanationModeSyncRequest(SyncMsgType type, int viewObjectId, bool doExpand)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.ExplanationModeSyncViewRequest,
                          ExplanationModeSyncMsg.Write(type, viewObjectId, doExpand),
                          true);
        }

        public void SendScreenshotRequest(int topicId, int discussionId)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.ScreenshotRequest,
                          ScreenshotRequest.Write(topicId, discussionId),
                          true);
        }

        public void SendCommentsRead(int PersonId, int TopicId, int ArgPointId)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.CommentReadRequest,
                          CommentsReadEvent.Write(PersonId, TopicId, ArgPointId),
                          true);
        }

        public void SendAttachLaserPointer(LaserPointer ptr)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.AttachLaserPointerRequest,
                          ptr.ToDict(),
                          true);
        }

        public void SendDetachLaserPointer(LaserPointer ptr)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.DetachLaserPointerRequest,
                          ptr.ToDict(),
                          true);
        }

        public void SendLaserPointerMoved(LaserPointer ptr)
        {
            if (peer == null || peer.PeerState != PeerStateValue.Connected)
                return;

            peer.OpCustom((byte)DiscussionOpCode.LaserPointerMovedRequest,
                          ptr.ToDict(),
                          true);
        }
    }
}