using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventGen.timeline
{
    public class EventMoveCommand : CommandBase
    {
        private TimelineEvent _event;

        private TimeSpan initialSpan;
        private double initialStickHeight;

        private TimeSpan finalSpan;
        private double finalStickHeight;

        public EventMoveCommand(TimelineEvent ev)
        {
            _event = ev;

            initialSpan = _event.Span;
            initialStickHeight = _event.StickHeight;
        }

        //returns true if command has changes
        public bool EndCommand()
        {
            finalSpan = _event.Span;
            finalStickHeight = _event.StickHeight;

            base.ToDone(); //command state done 

            return (finalSpan != initialSpan || finalStickHeight != initialStickHeight);
        }

        public override void ToUndone()
        {
            base.ToUndone();
            _event.Span = initialSpan;
            _event.StickHeight = initialStickHeight;
            _event.view.UpdatePositionByModel(_event);
        }

        public override void ToDone()
        {
            base.ToDone();
            _event.Span = finalSpan;
            _event.StickHeight = finalStickHeight;
            _event.view.UpdatePositionByModel(_event);
        }
    }
}