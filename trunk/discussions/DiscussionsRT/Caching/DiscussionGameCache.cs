using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lite.Caching;
using Lite;

namespace Discussions.RTModel
{
    public class DiscussionGameCache : RoomCacheBase
    {
        /// <summary>
        /// The instance.
        /// </summary>
        public static readonly DiscussionGameCache Instance = new DiscussionGameCache();

        Dictionary<string, Room> _rooms = new Dictionary<string, Room>();

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
        /// a <see cref="LiteLobbyRoom"/>  
        /// </returns>
        protected override Room CreateRoom(string roomId, params object[] args)
        {            
            return new DiscussionRoom(roomId, "discussions_lobby");  
        }
    }
}
