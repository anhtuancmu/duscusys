using System;
using System.Linq;
using Discussions.DbModel;
using Discussions.DbModel.model;
using Discussions.model;
using Discussions.RTModel.Model;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel
{
    internal class EventLogger
    {
        public static bool Log(DiscCtx ctx, StEvent e, int userId, int discussionId, int topicId, DeviceType devType)
        {
            var pers = ctx.Person.FirstOrDefault(p0 => p0.Id == userId);
            if (pers == null && userId != -1)
                return false;

            var disc = ctx.Discussion.FirstOrDefault(d0 => d0.Id == discussionId);
            if (disc == null)
                return false;

            var topic = ctx.Topic.FirstOrDefault(t0 => t0.Id == topicId);
            if (topic == null)
                return false;

            if (!topic.Running && e != StEvent.RecordingStarted &&
                e != StEvent.RecordingStopped)
            {
                return false;
            }

            var s = new StatsEvent
            {
                DiscussionId = discussionId,
                DiscussionName = disc.Subject,
                TopicId = topicId,
                TopicName = topic.Name,
                UserId = userId
            };
            if (pers != null)
                s.UserName = pers.Name;
            else
                s.UserName = "SYSTEM";
            s.Event = (int) e;
            s.Time = DateTime.Now;
            s.DeviceType = (int) devType;

            ctx.AddToStatsEvent(s);
            ctx.SaveChanges();

            return true;
        }

        public static bool LogAndBroadcast(DiscCtx ctx, DiscussionRoom room, StEvent ev, int usrId, int topicId)
        {
            var res = EventLogger.Log(ctx,
                                      ev,
                                      usrId,
                                      room.DiscId,
                                      topicId,
                                      DeviceType.Wpf);

            if (res)
            {
                room.BroadcastReliableToRoom((byte) DiscussionEventCode.StatsEvent,
                                             Serializers.WriteStatEventParams(ev,
                                                                              usrId,
                                                                              room.DiscId,
                                                                              topicId,
                                                                              DeviceType.Wpf)
                    );
            }
            return res;
        }
    }
}