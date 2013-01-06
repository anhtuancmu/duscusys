﻿using System;
using System.Collections.Generic;
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
    public partial class TimelineEventView : UserControl
    {
        private double recentMouseX;
        private double recentMouseY;

        private double _timelineBottomY;

        private TimelineView _timeline;

        private EventMoveCommand _currentMoveCommand = null;

        public TimelineEventView(Canvas scene, double timelineBottomY, TimelineEvent te, TimelineView timeline)
        {
            InitializeComponent();

            _timeline = timeline;

            _timelineBottomY = timelineBottomY;

            DataContext = te;
            te.view = this;
            AddToScene();

            UpdatePositionByModel(te);
        }

        public void AddToScene()
        {
            if (!_timeline.Scene.Children.Contains(this))
                _timeline.Scene.Children.Add(this);
        }

        public void RemoveFromScene()
        {
            _timeline.Scene.Children.Remove(this);
        }

        public void UpdatePositionByModel(TimelineEvent te)
        {
            var totalHeight = te.StickHeight + border.Height;
            Canvas.SetTop(this, _timelineBottomY - totalHeight);
            Canvas.SetLeft(this, TimeScale.TimeToPosition(te.Span, _timeline.Zoom));
        }

        private void TimelineEventView_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(_timeline.Scene);
            recentMouseX = pos.X;
            recentMouseY = pos.Y;

            var model = DataContext as TimelineEvent;
            if (model == null)
                return;
            model.IsEvSelected = !model.IsEvSelected;

            _currentMoveCommand = new EventMoveCommand(model);

            Mouse.Capture(this);

            e.Handled = true;
        }

        private void TimelineEventView_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var model = DataContext as TimelineEvent;
            if (model == null)
                return;

            //change model's time
            var pos = e.MouseDevice.GetPosition(_timeline.Scene);
            var xDelta = pos.X - recentMouseX;
            var yDelta = pos.Y - recentMouseY;
            recentMouseX = pos.X;
            recentMouseY = pos.Y;

            //x
            TimeSpan newTime = TimeScale.PositionToTime(Canvas.GetLeft(this) + xDelta, _timeline.Zoom);
            model.Span = newTime;

            //y
            var newStickHeight = model.StickHeight - yDelta;
            if (newStickHeight > 0)
            {
                model.StickHeight = newStickHeight;
            }

            //update position by model
            UpdatePositionByModel(model);

            e.Handled = true;
        }

        private void TimelineEventView_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            if (_currentMoveCommand != null && _currentMoveCommand.EndCommand())
            {
                CommandManager.Instance.RegisterDoneCommand(_currentMoveCommand);
            }
            _currentMoveCommand = null;

            ReleaseMouseCapture();
            e.Handled = true;
        }
    }
}