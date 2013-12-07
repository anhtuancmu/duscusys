using System.Collections.Generic;
using System.Linq;
using Discussions.DbModel.model;
using LiteLobby;
using Photon.SocketServer;
using Discussions.RTModel.Operations;
using PhotonHostRuntimeInterfaces;
using LiteLobby.Operations;
using Lite.Caching;
using Discussions.DbModel;
using Discussions.RTModel.Model;
using Discussions.RTModel.Caching;
using System.IO;

namespace Discussions.RTModel
{
    public class DiscussionPeer : LiteLobbyPeer
    {
        private int _dbId = -1;

        public int DbId
        {
            get { return _dbId; }
        }

        private readonly IPhotonPeer _photonPer = null;

        private const string DISCUSSION_LOBBY = "discussion_lobby";

        //person Db Id -> count online 
        private static readonly Dictionary<int, int> DbInstancesOnline = new Dictionary<int, int>();

        //photon peer -> db id
        //no record -> offline 
        private static readonly Dictionary<IPhotonPeer, int> PeerInstancesOnline = new Dictionary<IPhotonPeer, int>();

        public DiscussionPeer(IRpcProtocol rpcProtocol, IPhotonPeer photonPeer)
            : base(rpcProtocol, photonPeer)
        {
            _photonPer = photonPeer;
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            switch ((DiscussionOpCode) operationRequest.OperationCode)
            {
                case DiscussionOpCode.Test:
                    var operation = new TestOperation(this.Protocol, operationRequest);
                    if (ValidateOperation(operation, sendParameters))
                    {
                        SendOperationResponse(operation.GetResponse(), sendParameters);
                        return;
                    }
                    break;
                case DiscussionOpCode.NotifyUserAccPlusMinus:
                case DiscussionOpCode.NotifyStructureChanged:
                case DiscussionOpCode.NotifyArgPointChanged:
                case DiscussionOpCode.CursorRequest:
                case DiscussionOpCode.CreateShapeRequest:
                case DiscussionOpCode.DeleteShapesRequest:
                case DiscussionOpCode.UnselectAllRequest:
                case DiscussionOpCode.DeleteSingleShapeRequest:
                case DiscussionOpCode.StateSyncRequest:
                case DiscussionOpCode.InitialSceneLoadRequest:
                case DiscussionOpCode.LinkCreateRequest:
                case DiscussionOpCode.UnclusterBadgeRequest:
                case DiscussionOpCode.ClusterBadgeRequest:
                case DiscussionOpCode.ClusterMoveRequest:
                case DiscussionOpCode.InkRequest:
                case DiscussionOpCode.DEditorReport:
                case DiscussionOpCode.ClusterStatsRequest:
                case DiscussionOpCode.LinkReportRequest:
                case DiscussionOpCode.BadgeViewRequest:
                case DiscussionOpCode.ExplanationModeSyncViewRequest:
                case DiscussionOpCode.CommentReadRequest:
                case DiscussionOpCode.AttachLaserPointerRequest:
                case DiscussionOpCode.DetachLaserPointerRequest:
                case DiscussionOpCode.LaserPointerMovedRequest:
                case DiscussionOpCode.ImageViewerManipulateRequest:
                case DiscussionOpCode.ImageViewerStateRequest:
                case DiscussionOpCode.BrowserScrollChanged:
                case DiscussionOpCode.GetBrowserScrollPos:                  
                    HandleGameOperation(operationRequest, sendParameters);
                    break;
                case DiscussionOpCode.NotifyLeaveUser:
                    handleOnlineStatus(_photonPer, _dbId, false, (int) DeviceType.Sticky);
                    break;
                case DiscussionOpCode.StatsEvent:
                    if (LogEvent(operationRequest.Parameters))
                        HandleGameOperation(operationRequest, sendParameters); // broadcast stats event
                    break;
                case DiscussionOpCode.ScreenshotRequest:
                    HandleScreenshotRequest(operationRequest, sendParameters);
                    break;            
            }

            base.OnOperationRequest(operationRequest, sendParameters);
        }

        /// <summary>
        ///   Joins the peer to a <see cref = "LiteLobbyGame" />.
        ///   Called by <see cref = "HandleJoinOperation">HandleJoinOperation</see>.
        ///   Overridden to inject custom discussion rooms
        /// </summary>
        /// <param name = "joinOperation">
        ///   The join operation.
        /// </param>
        /// <param name = "sendParameters">
        ///   The send Parameters.
        /// </param>
        protected override void HandleJoinGameWithLobby(JoinRequest joinOperation, SendParameters sendParameters)
        {
            _dbId = (int) joinOperation.ActorProperties[(byte) ActProps.DbId];
            var devType = (int) joinOperation.ActorProperties[(byte) ActProps.DevType];

            handleOnlineStatus(_photonPer, _dbId, true, devType);

            // remove the peer from current game if the peer
            // allready joined another game
            this.RemovePeerFromCurrentRoom();

            // get a game reference from the game cache 
            // the game will be created by the cache if it does not exists allready 
            RoomReference gameReference = DiscussionGameCache.Instance.GetRoomReference(joinOperation.GameId,
                                                                                        joinOperation.LobbyId);

            // save the game reference in peers state                    
            this.RoomReference = gameReference;

            // enqueue the operation request into the games execution queue
            gameReference.Room.EnqueueOperation(this, joinOperation.OperationRequest, sendParameters);

            ////no base.HandleJoinGameWithLobby(), we've duplicated all its code here    

            RoomReference lobbyReference = DiscussionLobbyCache.Instance.GetRoomReference(joinOperation.LobbyId);
            var discLobby = lobbyReference.Room as DiscussionLobby;
            if (discLobby != null)
                discLobby.SaveRoomName(joinOperation.GameId);
        }

        //overritten to inject discussion lobby cache 
        /// <summary>
        ///   Joins the peer to a <see cref = "LiteLobby" />.
        ///   Called by <see cref = "HandleJoinOperation">HandleJoinOperation</see>.
        /// </summary>
        /// <param name = "joinRequest">
        ///   The join operation.
        /// </param>
        /// <param name = "sendParameters">
        ///   The send Parameters.
        /// </param>
        protected override void HandleJoinLobby(JoinRequest joinRequest, SendParameters sendParameters)
        {
            _dbId = (int) joinRequest.ActorProperties[(byte) ActProps.DbId];
            var devType = (int) joinRequest.ActorProperties[(byte) ActProps.DevType];

            handleOnlineStatus(_photonPer, _dbId, true, devType);

            // remove the peer from current game if the peer
            // allready joined another game
            this.RemovePeerFromCurrentRoom();

            // get a lobby reference from the game cache 
            // the lobby will be created by the cache if it does not exists allready
            RoomReference lobbyReference = DiscussionLobbyCache.Instance.GetRoomReference(joinRequest.GameId);

            // save the lobby(room) reference in peers state                    
            this.RoomReference = lobbyReference;

            // enqueue the operation request into the games execution queue
            lobbyReference.Room.EnqueueOperation(this, joinRequest.OperationRequest, sendParameters);

            lobbyReference.Room.EnqueueOperation(this, new OperationRequest((byte) DiscussionOpCode.NotifyLeaveUser),
                                                 sendParameters);
        }

        private void broadcastOnlineListChanged()
        {
            RoomReference lobbyReference = DiscussionLobbyCache.Instance.GetRoomReference(DISCUSSION_LOBBY);
            var dLobby = lobbyReference.Room as DiscussionLobby;
            if (dLobby != null)
            {
                dLobby.AllRoomsBroadcast(null, new OperationRequest(), new SendParameters(),
                                         (byte) DiscussionEventCode.InstantUserPlusMinus);
            }
        }

        private void handleOnlineStatus(IPhotonPeer peer, int dbId, bool online, int deviceType)
        {
            lock (PeerInstancesOnline)
            {
                if (online)
                {
                    if (!PeerInstancesOnline.ContainsKey(peer))
                    {
                        PeerInstancesOnline.Add(peer, dbId);
                        ChangeDbOnlineStatus(dbId, true, deviceType);
                    }
                }
                else
                {
                    if (PeerInstancesOnline.ContainsKey(peer))
                    {
                        RequestDetachLaserPointers();
                        
                        ChangeDbOnlineStatus(PeerInstancesOnline[peer], false, deviceType);
                        PeerInstancesOnline.Remove(peer);                        
                    }
                }
            }
        }

        void RequestDetachLaserPointers()
        {
            var req = new OperationRequest
                {
                    Parameters = new Dictionary<byte, object> {{(byte)DiscussionParamKey.UserId, _dbId}}
                };
            HandleGameOperation(req, new SendParameters());                        
        }

        private void ChangeDbOnlineStatus(int dbId, bool online, int deviceType)
        {
            lock (DbInstancesOnline)
            {
                if (online)
                {
                    if (!DbInstancesOnline.ContainsKey(dbId))
                        DbInstancesOnline.Add(dbId, 1);
                    else
                        DbInstancesOnline[dbId] = DbInstancesOnline[dbId] + 1;
                }
                else
                {
                    if (!DbInstancesOnline.ContainsKey(dbId))
                        DbInstancesOnline.Add(dbId, 0);
                    else
                        DbInstancesOnline[dbId] = DbInstancesOnline[dbId] - 1;
                }

                var ctx = new DiscCtx(Discussions.ConfigManager.ConnStr);
                Person p = ctx.Person.FirstOrDefault(p0 => p0.Id == _dbId);
                if (p != null)
                {
                    p.Online = DbInstancesOnline[dbId] > 0;
                    if (online)
                    {
                        //only change dev type if user goes online
                        p.OnlineDevType = deviceType;
                    }
                    ctx.SaveChanges();
                    broadcastOnlineListChanged();
                }
            }
        }

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            handleOnlineStatus(_photonPer, _dbId, false, (int) DeviceType.Sticky);
            base.OnDisconnect(reasonCode, reasonDetail);
        }

        private bool LogEvent(Dictionary<byte, object> req)
        {
            StEvent evt;
            int discussionId = -1;
            int topicId = -1;
            int userId = -1;
            DeviceType devType;
            Serializers.ReadStatEventParams(req, out evt, out userId, out discussionId, out topicId, out devType);
            return EventLogger.Log(new DiscCtx(Discussions.ConfigManager.ConnStr), evt, userId, discussionId, topicId,
                                   devType);
        }

        private void HandleScreenshotRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var param = ScreenshotRequest.Read(operationRequest.Parameters);
            var handler = new ScreenshotHandler();

            //launch client and make screens
            var metaInfoPathName = handler.RunClientAndWait(param.topicId, param.discussionId);
            var screenDict = handler.MetaInfoToDict(metaInfoPathName);
            File.Delete(metaInfoPathName);

            //build screenshot response
            var resp = new Dictionary<int, byte[]>();
            foreach (var kvp in screenDict)
                resp.Add(kvp.Key, File.ReadAllBytes(kvp.Value));

            this.SendOperationResponse(
                new OperationResponse((byte) DiscussionOpCode.ScreenshotRequest, ScreenshotResponse.Write(resp)),
                sendParameters);

            //cleanup
            foreach (var kvp in screenDict)
                File.Delete(kvp.Value);
        }
    }
}