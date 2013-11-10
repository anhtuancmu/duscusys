using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for TopicNavPanel.xaml
    /// </summary>
    public partial class TopicNavPanel : UserControl
    {
        private Discussion _discussion = null;

        public Discussion discussion
        {
            get { return _discussion; }
            set
            {
                _discussion = value;
                if (value.Topic.Count > 0)
                {
                    lstBxTopics.SelectedIndex = 0;
                }
            }
        }

        private bool Hidden = true;

        public delegate void TopicAnimate(bool hide);

        public TopicAnimate topicAnimate;

        public Topic selectedTopic
        {
            get { return lstBxTopics.SelectedItem as Topic; }
        }

        public delegate void SelectionChanged(SelectionChangedEventArgs e);

        public SelectionChanged topicChanged;

        public TopicNavPanel()
        {
            InitializeComponent();

            DataContext = this;
        }

        private List<TouchPoint> touchPoints = new List<TouchPoint>();

        public void SelectTopic(bool next)
        {
            if (discussion.Topic.Count == 0)
                return;

            if (next)
            {
                if (lstBxTopics.SelectedIndex + 1 < discussion.Topic.Count)
                    lstBxTopics.SelectedIndex = lstBxTopics.SelectedIndex + 1;
                else
                    lstBxTopics.SelectedIndex = 0;
            }
            else
            {
                if (lstBxTopics.SelectedIndex > 0)
                    lstBxTopics.SelectedIndex = lstBxTopics.SelectedIndex - 1;
                else
                    lstBxTopics.SelectedIndex = discussion.Topic.Count - 1;
            }

            if (lstBxTopics.SelectedItem != null)
                lstBxTopics.ScrollIntoView(lstBxTopics.SelectedItem);
        }

        private void lstBx_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            touchPoints.Add(e.GetTouchPoint(lstBxTopics));
        }

        private DateTime recentSwitch = DateTime.Now;

        private void lstBx_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            const double MIN_DELTA = 6;
            if (touchPoints.Count >= 2 && DateTime.Now.Subtract(recentSwitch).TotalMilliseconds > 200)
                foreach (var tp in touchPoints.ToArray())
                {
                    if (tp.TouchDevice == e.TouchDevice)
                    {
                        if (e.TouchDevice.GetPosition(lstBxTopics).X - tp.Position.X > MIN_DELTA)
                        {
                            SelectTopic(false);
                            recentSwitch = DateTime.Now;
                            break;
                        }
                        else if (tp.Position.X - e.TouchDevice.GetPosition(lstBxTopics).X > MIN_DELTA)
                        {
                            SelectTopic(true);
                            recentSwitch = DateTime.Now;
                            break;
                        }
                    }
                }

            //update touch points 
            var prevTouchPoints = touchPoints.ToArray();
            touchPoints.Clear();
            foreach (var tp in prevTouchPoints)
            {
                touchPoints.Add(tp.TouchDevice.GetTouchPoint(lstBxTopics));
            }
        }

        private void lstBx_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            foreach (var tp in touchPoints.ToArray())
                if (tp.TouchDevice == e.TouchDevice)
                {
                    touchPoints.Remove(tp);
                    break;
                }
        }

        private void topicSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                lstBxTopics.ScrollIntoViewCentered(e.AddedItems[0]);

            btnShow.Content = "";

            if (selectedTopic == null)
                return;

            btnShow.Content = selectedTopic.Name;

            if (topicChanged != null)
                topicChanged(e);
        }

        private void Animate(object sender, RoutedEventArgs e)
        {
            if (topicAnimate == null)
                return;

            topicAnimate(!Hidden);
            Hidden = !Hidden;
        }

        private void btnTriggerStats_Click_1(object sender, RoutedEventArgs e)
        {
        }

        private void HandleStartStop(RoutedEventArgs e)
        {
            var checkBx = e.OriginalSource as SurfaceCheckBox;
            var topic = checkBx.DataContext as Topic;
            if (topic == null)
                return;

            var ownId = SessionInfo.Get().person.Id;

            var freshTopic = PublicBoardCtx.Get().Topic.FirstOrDefault(t0 => t0.Id == topic.Id);
            freshTopic.Running = topic.Running;
            PublicBoardCtx.Get().SaveChanges();

            var evt = topic.Running ? StEvent.RecordingStarted : StEvent.RecordingStopped;
            UISharedRTClient.Instance.clienRt.SendStatsEvent(evt, ownId,
                                                             SessionInfo.Get().discussion.Id,
                                                             topic.Id,
                                                             DeviceType.Wpf);
        }

        private void running_Checked_1(object sender, RoutedEventArgs e)
        {
            HandleStartStop(e);
        }

        private void running_Unchecked_1(object sender, RoutedEventArgs e)
        {
            HandleStartStop(e);
        }
    }
}