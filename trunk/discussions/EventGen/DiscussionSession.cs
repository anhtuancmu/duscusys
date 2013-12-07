using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoginEngine;

namespace EventGen
{
    public class DiscussionSession
    {
        private int _discussionId;

        //public bool GetStartTime(out DateTime time)
        //{
        //    throw new NotSupportedException("broken by new event and start/stop system"); 

        //    //time = DateTime.Now;
        //    //var startEvent = DbCtx.Get().StatsSet.FirstOrDefault(e0 => e0.DiscussionId == _discussionId &&
        //    //                                                           e0.Event == (int)StatsEvent.DiscussionSessionStarted);
        //    //if (startEvent == null)
        //    //    return false;

        //    //time = startEvent.Time;
        //    //return true;
        //}

        //public bool GetEndTime(out DateTime time)
        //{
        //    throw new NotSupportedException("broken by new event and start/stop system"); 

        //    //time = DateTime.Now;
        //    //var endEvent = DbCtx.Get().StatsSet.FirstOrDefault(e0 => e0.DiscussionId == _discussionId &&
        //    //                                                         e0.Event == (int)StatsEvent.DiscussionSessionStopped);
        //    //if (endEvent == null)
        //    //    return false;

        //    //time = endEvent.Time;
        //    //return true;
        //}

        public DiscussionSession(int discussionId)
        {
            _discussionId = discussionId;
        }
    }
}