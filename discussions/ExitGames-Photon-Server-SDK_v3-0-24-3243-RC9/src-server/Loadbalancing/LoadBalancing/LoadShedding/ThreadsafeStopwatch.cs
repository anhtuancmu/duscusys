namespace Photon.LoadBalancing.LoadShedding
{

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    public sealed class ThreadSafeStopwatch 
    {
        Thread worker;
        EventWaitHandle x = new AutoResetEvent(false);
        EventWaitHandle y = new AutoResetEvent(false);

        long ticks = 0;
        bool isStopped = false;
        
        public ThreadSafeStopwatch()
        {
            worker = new Thread(new ThreadStart(WorkThreadFunction));
            worker.Start();
            x.WaitOne();
        }

        private void WorkThreadFunction()
        {
            Thread.BeginThreadAffinity();
            while (!isStopped)
            {
                //Console.WriteLine("WorkThreadFunction() b - " + ticks);
                WaitHandle.SignalAndWait(x, y);
                if (!isStopped)
                {
                    ticks = Stopwatch.GetTimestamp();
                    //Console.WriteLine("WorkThreadFunction() a - " + ticks);
                }
                //else
                //    Console.WriteLine("WorkThreadFunction() a - " + "stopped!");
            }
        }

        public long GetTimestamp()
        {
            //Console.WriteLine("GetTimestamp() b - " + ticks);
            WaitHandle.SignalAndWait(y, x);
            //Console.WriteLine("GetTimestamp() a - " + ticks);
            return ticks;
        }

        public void Stop()
        {
            isStopped = true;
            y.Set();
        }
    }
}