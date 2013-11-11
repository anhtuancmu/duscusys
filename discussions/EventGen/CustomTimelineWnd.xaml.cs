using System;
using System.Windows;
using System.Windows.Input;
using EventGen.timeline;
using Microsoft.Surface.Presentation.Controls;

namespace EventGen
{
    public partial class CustomTimelineWnd : SurfaceWindow
    {
        private Timeline _timelineModel;

        public CustomTimelineWnd()
        {
            InitializeComponent();

            _timelineModel = new Timeline(TimeSpan.FromMinutes(2));
            timelineView.SetModel(_timelineModel);
            currentTime.DataContext = _timelineModel;
        }

        private void AddEvent_Click_1(object sender, RoutedEventArgs e)
        {
            var ev = new TimelineEvent(Discussions.model.StEvent.BadgeEdited, 1, 1,
                                       _timelineModel, TimeSpan.FromSeconds(4), 1, Discussions.model.DeviceType.Wpf);
            _timelineModel.AddEvent(ev);
        }

        private void RemoveEvent_Click_1(object sender, RoutedEventArgs e)
        {
        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timelineView.ChangeZoom(e.NewValue);
        }

        private void timelineView_MouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            var delta = e.Delta < 0 ? 0.5 : -0.5;
            var newZoom = zoomSlider.Value + delta;
            if (zoomSlider.Minimum <= newZoom && newZoom <= zoomSlider.Maximum)
                zoomSlider.Value = newZoom;
        }
    }
}