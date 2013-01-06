using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Threading;

namespace Discussions
{
    public class ContactTimer
    {
        private DispatcherTimer aTimer = new DispatcherTimer();

        public ContactTimer(EventHandler OnTimedEvent, double seconds = 0.5, bool doStart = true)
        {
            if (OnTimedEvent != null)
                aTimer.Tick += OnTimedEvent;
            aTimer.Interval = TimeSpan.FromSeconds(seconds);
            if (doStart)
                aTimer.Start();
        }

        public void Run()
        {
            aTimer.Start();
        }

        public void Stop()
        {
            aTimer.Stop();
        }

        public void Reset()
        {
            aTimer.Stop();
        }
    }
}