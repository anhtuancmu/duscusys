using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lite;
using Lite.Caching;

namespace Discussions.RTModel.Caching
{
    internal class DiscussionLobbyCache : RoomCacheBase
    {
        /// <summary>
        /// The instance.
        /// </summary>
        public static readonly DiscussionLobbyCache Instance = new DiscussionLobbyCache();

        /// <summary>
        /// The create room.
        /// </summary>
        /// <param name="roomId">
        /// The room id.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// a <see cref="LiteLobbyGame"/>
        /// </returns>
        protected override Room CreateRoom(string roomId, params object[] args)
        {
            var lobbyName = "discussion_lobby";
            return new DiscussionLobby(roomId, lobbyName);
        }
    }
}