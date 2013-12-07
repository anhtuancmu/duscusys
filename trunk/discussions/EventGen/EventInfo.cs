using System;
using Discussions.DbModel.model;

namespace EventGen
{
    public class EventInfo
    {
        public StEvent e;
        public int userId;
        public int discussionId;
        public DateTime timestamp;
        public int topicId;
        public DeviceType devType;

        public EventInfo(StEvent e, int userId, int discussionId, DateTime timestamp, int topicId, DeviceType devType)
        {
            this.e = e;
            this.userId = userId;
            this.discussionId = discussionId;
            this.timestamp = timestamp;
            this.topicId = topicId;
            this.devType = devType;
        }
    }
}