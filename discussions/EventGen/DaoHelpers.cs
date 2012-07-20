using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.DbModel;
using Discussions.model;
using LoginEngine;

namespace EventGen
{
    public class DaoHelpers
    {
        public static IEnumerable<Discussion> discussionsOfSession(Session s)
        {
            var sessionId = s.Id;
            var q = from d in DbCtx.Get().Discussion
                    where d.GeneralSide.Any(gs0 => gs0.Person.Session.Id == sessionId)
                    select d;
            return q;
        }

        public static IEnumerable<Person> personsOfDiscussion(Discussion d)
        {
            var q = from p in DbCtx.Get().Person
                    where p.Topic.Any(t0 => t0.Discussion.Id == d.Id)
                    select p;
            return q;
        }

        public static void recordEvent(EventInfo evt)
        {
            var _ctx = DbCtx.Get();

            var pers = _ctx.Person.FirstOrDefault(p0 => p0.Id == evt.userId);
            var disc = _ctx.Discussion.FirstOrDefault(d0 => d0.Id == evt.discussionId);
            if (disc == null)
            {
                return;
            }
            var topic = _ctx.Topic.FirstOrDefault(t0 => t0.Id == evt.topicId);

            var s = new StatsEvent();
            s.DiscussionId = evt.discussionId;
            s.DiscussionName = disc.Subject;

            s.TopicId = evt.topicId;
            if (topic != null)
                s.TopicName = topic.Name;
            else
                s.TopicName = "";

            s.UserId = evt.userId;
            s.UserName = pers.Name;
            s.Event = (int)evt.e;
            s.Time = evt.timestamp;
            s.DeviceType = (int)evt.devType;

            _ctx.StatsEvent.AddObject(s);            
        }
    }
}
