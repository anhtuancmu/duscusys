using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EventGen.timeline
{
    public partial class CurrentMarker : UserControl
    {
        TimelineView _timelineView;
        Timeline _timeline;

        public CurrentMarker(TimelineView timelineView, Timeline timeline)
        {
            InitializeComponent();

            _timelineView = timelineView;
            _timeline = timeline;
            if (_timeline == null)
                throw new NotSupportedException("no timeline model");

            _timeline.PropertyChanged += propertyChanged;

            addToScene();
        }

        void propertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentTime")
            {
                updatePositionByModel();
            }
        }

        void CurrentMarker_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            //sets current marker at correct zero position 
            updatePositionByModel();
        }

        public void updatePositionByModel()
        {
            Canvas.SetLeft(this, TimeScale.TimeToPosition(_timeline.CurrentTime, _timelineView.Zoom)-this.ActualWidth*0.5);                
        }

        void addToScene()
        {
            if (!_timelineView.Scene.Children.Contains(this))
            {
                _timelineView.Scene.Children.Add(this);
                this.Height = TimeScale.TIMELINE_HEIGHT;
            }
        }

        private void CurrentMarker_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(this);
            e.Handled = true;
        }

        private void CurrentMarker_PreviewMouseMove_1(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            //change model's time
            var pos = e.MouseDevice.GetPosition(_timelineView.Scene);
            _timeline.CurrentTime = TimeScale.PositionToTime(pos.X, _timelineView.Zoom);

            e.Handled = true;
        }

        private void CurrentMarker_PreviewMouseUp_1(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            e.Handled = true;
        }
    }
}
