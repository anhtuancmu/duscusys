using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discussions.RTModel
{
    #region using directives

    using Lite;
    using Lite.Caching;
    using Lite.Messages;
    using Lite.Operations;

    using LiteLobby.Caching;
    using LiteLobby.Messages;

    using Photon.SocketServer;
    using Discussions.RTModel.Caching;
    using Discussions.RTModel.Operations;
    using LiteLobby;

    #endregion

    class DiscussionLobby : LiteLobbyRoom
    {
        List<String> roomList = new List<String>();
        
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "LiteLobbyGame" /> class.
        /// </summary>
        /// <param name = "gameName">
        ///   The name of the game.
        /// </param>
        /// <param name = "lobbyName">
        ///   The name of the lobby for the game.
        /// </param>
        public DiscussionLobby(string gameName, string lobbyName)
            : base(gameName)
        {
        }

        #endregion

        public void SaveRoomName(string roomName)
        {
            if (!roomList.Contains(roomName))
                roomList.Add(roomName); 
        }

        protected override void ExecuteOperation(LitePeer peer, OperationRequest operationRequest,
                                                 SendParameters sendParameters)
        {
            switch (operationRequest.OperationCode)
            {
                case (byte)DiscussionOpCode.NotifyLeaveUser:
                    {
                        broadcastNewInLobby();
                        break;
                    }
                default:
                    base.ExecuteOperation(peer, operationRequest, sendParameters);
                    break;
            }

            base.ExecuteOperation(peer, operationRequest, sendParameters);
        }

        void broadcastNewInLobby()
        {
            var sp = new SendParameters();
            sp.Flush = true;
            sp.Unreliable = false;
            AllRoomsBroadcast(null, new OperationRequest(), new SendParameters(),
                              (byte)DiscussionEventCode.InstantUserPlusMinus);
        }

        public void AllRoomsBroadcast(LitePeer peer,
                                       OperationRequest operationRequest,
                                       SendParameters sendParameters,
                                       byte EventCode)
        {
            foreach (string roomName in roomList)
            {
                RoomReference rr = DiscussionGameCache.Instance.GetRoomReference(roomName);
                DiscussionRoom discussionRoom = rr.Room as DiscussionRoom;
                if (discussionRoom != null)
                {
                    discussionRoom.Broadcast(peer, 
                                            operationRequest,
                                            sendParameters,
                                            EventCode,
                                            BroadcastTo.RoomAll);
                }
            }
            BroadcastLobby(operationRequest, sendParameters, EventCode);
        }

        public void PublishDiscussionEvent(byte eventCode, Dictionary<byte, object> data,
                                          IEnumerable<Actor> actorList,
                                          SendParameters sendParameters)
        {
            IEnumerable<PeerBase> peers = actorList.Select(actor => actor.Peer);
            var eventData = new EventData(eventCode, data);
            eventData.SendTo(peers, sendParameters);
        }

        public void BroadcastLobby(OperationRequest operationRequest,
                                   SendParameters sendParameters,
                                   byte EventCode)
        {
            PublishDiscussionEvent(EventCode,
                                    operationRequest.Parameters,
                                    this.Actors,
                                    sendParameters);
        }
    }
}
