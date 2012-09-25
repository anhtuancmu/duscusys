using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventGen.timeline
{
    public class DeleteEventCommand : CommandBase
    {
        TimelineEvent _event;

        public DeleteEventCommand(TimelineEvent ev, bool doCmd)
        {
            _event = ev;

            if (doCmd)
                ToDone();
        }

        public override void ToUndone()
        {
            base.ToUndone();
            _event.timeline.AddEvent(_event); 
        }

        public override void ToDone()
        {
            base.ToDone();
            _event.timeline.RemoveEvent(_event); 
        }
    }
}
