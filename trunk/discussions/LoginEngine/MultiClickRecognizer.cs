﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;

namespace Discussions
{
    public class MultiClickRecognizer
    {
        DispatcherTimer aTimer = new DispatcherTimer();

        int numConseqClicks = 0;

        public delegate void OnMultiClick(object sender, InputEventArgs e);

        OnMultiClick _onDoubleClick = null;
        OnMultiClick _onTripleClick = null;
        OnMultiClick _onSingleClick = null;

        public MultiClickRecognizer(OnMultiClick OnDoubleClick, OnMultiClick OnTripleClick, OnMultiClick onSingleClick = null)
        {
            _onDoubleClick = OnDoubleClick;
            _onTripleClick = OnTripleClick;
            _onSingleClick = onSingleClick;

            aTimer.Tick += OnTimer;
            aTimer.Interval = TimeSpan.FromSeconds(0.5);
            aTimer.Start();
        }

        void OnTimer(object sender, EventArgs e)
        {
            if (numConseqClicks == 1 && _onSingleClick != null)
                _onSingleClick(null, null);

            numConseqClicks = 0;            
        }

        public void Click(object sender, InputEventArgs e)
        {
            aTimer.Stop();
            aTimer.Start();
            numConseqClicks++;

            Console.WriteLine("click {0}", numConseqClicks);

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
