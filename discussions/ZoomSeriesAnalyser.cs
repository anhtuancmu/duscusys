using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Discussions.model;
using Discussions.rt;

namespace Discussions
{
    public enum ZoomDirection 
    {
        In,
        Out
    }
    
    public class ZoomSeriesAnalyser
    {
        DispatcherTimer _timer;

        public delegate void ZoomSeriesEnd(ZoomDirection direction);
        ZoomSeriesEnd _end;

        int _numSteps = 0;
        ZoomDirection _currentDirection;

        public ZoomSeriesAnalyser(ZoomSeriesEnd end)
        {
            _end = end;            

            _timer = new DispatcherTimer();
            _timer.Tick += OnTick;
            _timer.Interval = TimeSpan.FromMilliseconds(2500);
            _timer.Start();
        }

        public void SubmitStep(ZoomDirection direction)
        {
            _timer.Stop();
            _timer.Start();
            
            if (SeriesNonEmpty() && direction != _currentDirection)
            {
                _numSteps = 0;
                _end(_currentDirection);
            }
            else
            {
                _numSteps++;
            }

            if (direction != _currentDirection)
            {
                _currentDirection = direction;
            }
        }        

        bool SeriesNonEmpty()
        {
            return _numSteps > 0; 
        }

        void OnTick(object sender, EventArgs e)
        {
            if (SeriesNonEmpty())
            {
                _numSteps = 0;
                _end(_currentDirection);
            }
        }
    }
}
