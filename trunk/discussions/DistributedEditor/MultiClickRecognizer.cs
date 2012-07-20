using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows.Input;

namespace Discussions
{
    class MultiClickRecognizer
    {
        DispatcherTimer aTimer = new DispatcherTimer();

        int numConseqClicks = 0;

        public delegate void OnMultiClick(object sender, InputEventArgs e);

        OnMultiClick _onDoubleClick = null;
        OnMultiClick _onTripleClick = null; 

        public MultiClickRecognizer(OnMultiClick OnDoubleClick, OnMultiClick OnTripleClick)
        {
            _onDoubleClick = OnDoubleClick;
            _onTripleClick = OnTripleClick;

            aTimer.Tick += OnTimer;
            aTimer.Interval = TimeSpan.FromSeconds(0.5);
            aTimer.Start();
        }

        void OnTimer(object sender, EventArgs e)
        {
            numConseqClicks = 0;
        }

        public void Click(object sender, InputEventArgs e)
        {
            aTimer.Stop();
            aTimer.Start();
            numConseqClicks++;

            Console.WriteLine("click {0}",numConseqClicks);

            if (numConseqClicks == 2)
            {
                if (_onDoubleClick != null)
                {
                    _onDoubleClick(sender, e);
                    Console.WriteLine("calling 2 tap");
                }
            }
            else if (numConseqClicks == 3)
            {
                if (_onTripleClick != null)
                    _onTripleClick(sender, e);
            }
        }      
    }
}
