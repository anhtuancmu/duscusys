using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventGen.timeline
{
    public class CreateEventCommand : CommandBase
    {
        private TimelineEvent _event;

        public CreateEventCommand(TimelineEvent ev, bool doCmd)
        {
            _event = ev;

            if (doCmd)
                ToDone();
        }

        public override void ToUndone()
        {
            base.ToUndone();
            _event.timeline.RemoveEvent(_event);
        }

        public override void ToDone()
        {
            base.ToDone();
            _event.timeline.AddEvent(_event);
        }
    }
}