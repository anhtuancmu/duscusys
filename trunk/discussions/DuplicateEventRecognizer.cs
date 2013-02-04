using System;

namespace Discussions
{
    public class DuplicateEventRecognizer
    {
        private readonly double _maxDeltaMs;
        private DateTime _recent;

        public DuplicateEventRecognizer(double maxDeltaMs)
        {
            _maxDeltaMs = maxDeltaMs;
        }

        public void RecordEvent()
        {
            _recent = DateTime.Now;
        }

        public bool IsDuplicate()
        {
            return DateTime.Now.Subtract(_recent).TotalMilliseconds < _maxDeltaMs;
        }
    }
}