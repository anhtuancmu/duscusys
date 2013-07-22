using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    /// <summary>
    /// Interaction logic for TimelineView.xaml
    /// </summary>
    public partial class TimelineView : UserControl
    {
        private Timeline _model;

        private CurrentMarker _currentMarker = null;

        private double zoom = 1.0;

        public double Zoom
        {
            get { return zoom; }
        }

        public Canvas Scene
        {
            get { return eventScene; }
        }

        public TimelineView()
        {
            InitializeComponent();
        }

        public void SetModel(Timeline model)
        {
            _model = model;
            model.Events.CollectionChanged += notifyCollectionChangedEventHandler;

            _currentMarker = new CurrentMarker(this, model);

            ChangeZoom(zoom);
        }

        private void notifyCollectionChangedEventHandler(Object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                {
                    var model = item as TimelineEvent;
                    new TimelineEventView(this.eventScene, TimeScale.TIMELINE_HEIGHT, model, this);
                }

            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                {
                    var model = item as TimelineEvent;

                    if (model.view != null)
                        model.view.RemoveFromScene();
                }
        }

        private void setWidthByRange(double zoo)
        {
            //set actual width based on time range
            var rangeWidth = TimeScale.TimeToPosition(_model.Range, zoo);
            this.Width = rangeWidth; //rightmost tick is not visible
        }

        public void ChangeZoom(double newZoom)
        {
            if (_model == null)
                return;

            zoom = newZoom;
            TimeScale.drawTicks(eventScene, newZoom, _model.Range, TimeScale.TIMELINE_HEIGHT);
            setWidthByRange(newZoom);

            foreach (var ev in _model.Events)
            {
                ev.view.UpdatePositionByModel(ev);
            }

            _currentMarker.updatePositionByModel();
        }
    }
}