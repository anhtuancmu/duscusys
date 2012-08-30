using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;
using Discussions;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Discussions.stats;
using LoginEngine;
using Microsoft.Surface.Presentation.Controls;
using TimelineLibrary;

namespace EventGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SurfaceWindow
    {
        LoginResult login = null;

        UISharedRTClient sharedClient = new UISharedRTClient();

        DispatcherTimer _timer;

        ObservableCollection<Topic> _topics = null;
        public ObservableCollection<Topic> topics
        {
            get
            {
                if (_topics == null)
                    _topics = new ObservableCollection<Topic>();
                return _topics;
            }
            set
            {
                _topics = value;
            }
        }

        ObservableCollection<EventViewModel> _recentEvents = new ObservableCollection<EventViewModel>();
        public ObservableCollection<EventViewModel> RecentEvents
        {
            get
            {
                return _recentEvents;
            }
            set
            {
                _recentEvents = value;
            }
        }

        // public ObservableCollection<Discussion> Discussions {get;set;}

        public ObservableCollection<Person> Persons { get; set; }

        public ObservableCollection<Topic> Topics { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            LoginProcedure();

            DataContext = this;

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            _timer.Tick += setUpdateTrackPos;
            _timer.Start();
        }

        void LoginProcedure()
        {
            login = LoginDriver.Run(LoginFlow.ForEventGen);
            if (login == null)
            {
                System.Windows.Application.Current.Shutdown();
                return;
            }

            if (login.discussion == null)
            {
                System.Windows.MessageBox.Show("In this application even moderator should select real, existing discussion");
                System.Windows.Application.Current.Shutdown();
                return;
            }

            // Discussions = new ObservableCollection<Discussion>(DaoHelpers.discussionsOfSession(login.session));

            Topics = new ObservableCollection<Topic>(login.discussion.Topic);

            Persons = new ObservableCollection<Person>(DaoHelpers.personsOfDiscussion(login.discussion));

            setPostLoginInfo();

            FillTopics(login.discussion);

            sharedClient.start(login, DbCtx.Get().Connection.DataSource, login.devType);
            sharedClient.clienRt.onStatsEvent += OnStatsEvent;
        }

        void LogoutProcedure()
        {
            if (sharedClient.clienRt != null)
            {
                sharedClient.clienRt.SendLiveRequest();
                sharedClient.clienRt.onStatsEvent -= OnStatsEvent;
            }
        }

        void FillTopics(Discussion d)
        {
            topics.Clear();
            foreach (var t in d.Topic)
            {
                topics.Add(t);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            LogoutProcedure();
            Close();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            LogoutProcedure();
            LoginProcedure();
        }

        void FireStatsEvent(StEvent e, int personId = -2)
        {
            if (cbxTopics.SelectedItem == null)
            {
                MessageBox.Show("Please first select topic");
                return;
            }

            if (personId == -2)
            {
                if (lstUsers.SelectedItem == null)
                {
                    MessageBox.Show("Please first select user who initiates the event");
                    return;
                }
                personId = ((Person)lstUsers.SelectedItem).Id;
            }
            else
            {
                //we use given personId
            }

            var eventTimestamp = timeline.CurrentDateTime;
            var eventViewModel = new EventViewModel(e, personId, eventTimestamp, login.devType);

            var te = new TimelineLibrary.TimelineEvent();
            te.StartDate = eventTimestamp;
            te.EndDate = eventTimestamp;
            te.IsDuration = false;
            te.Title = eventViewModel.userName + " " + eventViewModel.evt;
            te.Description = eventViewModel.devType + ", " + eventViewModel.dateTime;
            te.Tag = new EventInfo(e, personId, login.discussion.Id,
                                   eventTimestamp, ((Topic)cbxTopics.SelectedItem).Id,
                                   login.devType);

            timeline.TimelineEvents.Add(te);
            timeline.ResetEvents(timeline.TimelineEvents);
        }

        #region eventgen handlers
        private void SurfaceWindow_KeyDown_1(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D1:
                    FireStatsEvent(StEvent.RecordingStarted);
                    break;
                case Key.D2:
                    FireStatsEvent(StEvent.RecordingStopped);
                    break;
                case Key.D3:
                    FireStatsEvent(StEvent.BadgeCreated);
                    break;
                case Key.D4:
                    FireStatsEvent(StEvent.BadgeEdited);
                    break;
                case Key.D5:
                    FireStatsEvent(StEvent.BadgeMoved);
                    break;
                case Key.D6:
                    FireStatsEvent(StEvent.BadgeZoomIn);
                    break;
                case Key.D7:
                    FireStatsEvent(StEvent.ClusterCreated);
                    break;
                case Key.D8:
                    FireStatsEvent(StEvent.ClusterDeleted);
                    break;
                case Key.D9:
                    FireStatsEvent(StEvent.ClusterIn);
                    break;
                case Key.Q:
                    FireStatsEvent(StEvent.ClusterOut);
                    break;
                case Key.W:
                    FireStatsEvent(StEvent.ClusterMoved);
                    break;
                case Key.E:
                    FireStatsEvent(StEvent.LinkCreated);
                    break;
                case Key.R:
                    FireStatsEvent(StEvent.LinkRemoved);
                    break;
                case Key.T:
                    FireStatsEvent(StEvent.FreeDrawingCreated);
                    break;
                case Key.Y:
                    FireStatsEvent(StEvent.FreeDrawingRemoved);
                    break;
                case Key.U:
                    FireStatsEvent(StEvent.FreeDrawingResize);
                    break;
                case Key.I:
                    FireStatsEvent(StEvent.FreeDrawingMoved);
                    break;
                case Key.O:
                    FireStatsEvent(StEvent.SceneZoomedIn);
                    break;
                case Key.P:
                    FireStatsEvent(StEvent.SceneZoomedOut);
                    break;
                case Key.A:
                    FireStatsEvent(StEvent.ArgPointTopicChanged);
                    break;
                case Key.S:
                    FireStatsEvent(StEvent.SourceAdded);
                    break;
                case Key.D:
                    FireStatsEvent(StEvent.SourceRemoved);
                    break;
                case Key.F:
                    FireStatsEvent(StEvent.ImageAdded);
                    break;
                case Key.H:
                    FireStatsEvent(StEvent.PdfAdded);
                    break;
                case Key.K:
                    FireStatsEvent(StEvent.YoutubeAdded);
                    break;
                case Key.L:
                    FireStatsEvent(StEvent.ScreenshotAdded);
                    break;
                case Key.Z:
                    FireStatsEvent(StEvent.MediaRemoved);
                    break;
                case Key.X:
                    FireStatsEvent(StEvent.CommentAdded);
                    break;
                case Key.C:
                    FireStatsEvent(StEvent.CommentRemoved);
                    break;
                case Key.V:
                    FireStatsEvent(StEvent.ImageOpened);
                    break;
                case Key.B:
                    FireStatsEvent(StEvent.VideoOpened);
                    break;
                case Key.N:
                    FireStatsEvent(StEvent.ScreenshotOpened);
                    break;
                case Key.M:
                    FireStatsEvent(StEvent.PdfOpened);
                    break;
                case Key.D0:
                    FireStatsEvent(StEvent.SourceOpened);
                    break;
                case Key.Delete:
                    DeleteSelectedEvents();
                    break;
            }
        }

        private void btnLinkCreated_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.LinkCreated);
        }

        private void btnLinkRemoved_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.LinkRemoved);
        }

        private void btnBadgeCreated_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.BadgeCreated);
        }

        private void btnBadgeEdited_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.BadgeEdited);
        }

        private void btnClusterCreated_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.ClusterCreated);
        }

        private void btnClusterRemoved_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.ClusterDeleted);
        }

        private void btnFreeDrawingCreated_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.FreeDrawingCreated);
        }

        private void btnFreeDrawingRemoved_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.FreeDrawingRemoved);
        }

        private void btnChangedTopic_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.ArgPointTopicChanged);
        }

        void OnStatsEvent(StEvent e, int userId, int discussionId, int topicId, DeviceType devType)
        {
            RecentEvents.Add(new EventViewModel(e, userId, DateTime.Now, devType));
        }

        private void btnDeleteEvent_Click_1(object sender, RoutedEventArgs e)
        {
            DeleteSelectedEvents();
        }

        private void btnRecordingStarted_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.RecordingStarted);
        }

        private void btnRecordingStopped_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.RecordingStopped);
        }

        private void btnBadgeEdited_Click_2(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.BadgeEdited);
        }

        private void btnBadgeMoved_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.BadgeMoved);
        }

        private void btnBadgeZoomIn_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.BadgeZoomIn);
        }

        private void btnClusterIn_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.ClusterIn);
        }

        private void btnClusterOut_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.ClusterOut);
        }

        private void btnClusterMoved_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.ClusterMoved);
        }

        private void btnFreeDrawingResize_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.FreeDrawingResize);
        }

        private void btnFreeDrawingMoved_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.FreeDrawingMoved);
        }

        private void btnSceneZoomIn_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.SceneZoomedIn);
        }

        private void btnSceneZoomOut_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.SceneZoomedOut);
        }

        private void btnArgPointTopicChanged_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.ArgPointTopicChanged);
        }

        private void btnSourceAdded_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.SourceAdded);
        }

        private void btnSourceRemoved_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.SourceRemoved);
        }

        private void btnImageAdded_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.ImageAdded);
        }

        private void btnPdfAdded_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.PdfAdded);
        }

        private void btnYoutubeAdded_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.YoutubeAdded);
        }

        private void btnScreenshotAdded_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.ScreenshotAdded);
        }

        private void btnMediaRemoved_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.MediaRemoved);
        }

        private void btnCommentAdded_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.CommentAdded);
        }

        private void btnCommentRemoved_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.CommentRemoved);
        }

        private void btnImageOpened_Click(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.ImageOpened);
        }

        private void btnVideoOpened_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.VideoOpened);
        }

        private void btnScreenshotOpened_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.ScreenshotAdded);
        }

        private void btnPdfOpened_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.PdfOpened);
        }

        private void btnSourceOpened_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.SourceOpened);
        }

        private void btnDeleteEvent_Click_2(object sender, RoutedEventArgs e)
        {
            DeleteSelectedEvents();
        }

        #endregion

        void DeleteSelectedEvents()
        {
            foreach (var se in timeline.SelectedTimelineEvents)
            {
                timeline.TimelineEvents.Remove(se);
            }

            timeline.ResetEvents(timeline.TimelineEvents);
        }

        private void btnUpload_Click_1(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            myMediaElement.Source = new Uri(openFileDialog1.FileName);
        }

        #region media player

        // Play the media.
        void OnMouseDownPlayMedia(object sender, MouseButtonEventArgs args)
        {
            myMediaElement.Play();

            // Initialize the MediaElement property values.
            InitializePropertyValues();
        }

        // Pause the media.
        void OnMouseDownPauseMedia(object sender, MouseButtonEventArgs args)
        {
            myMediaElement.Pause();
        }

        // Stop the media.
        void OnMouseDownStopMedia(object sender, MouseButtonEventArgs args)
        {
            myMediaElement.Stop();
        }

        // Change the volume of the media.
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            myMediaElement.Volume = (double)volumeSlider.Value;
        }

        // Change the speed of the media.
        private void ChangeMediaSpeedRatio(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            myMediaElement.SpeedRatio = (double)speedRatioSlider.Value;
            lblSpeed.Text = string.Format("Speed {0:0.0}x", (double)speedRatioSlider.Value);
        }

        // When the media opens, initialize the "Seek To" slider maximum value
        // to the total number of miliseconds in the length of the media clip.
        private void Element_MediaOpened(object sender, EventArgs e)
        {
        }

        // When the media playback is finished. Stop() the media to seek to media start.
        private void Element_MediaEnded(object sender, EventArgs e)
        {
            myMediaElement.Stop();
        }

        void InitializePropertyValues()
        {
            // Set the media's starting Volume and SpeedRatio to the current value of the
            // their respective slider controls.
            myMediaElement.Volume = (double)volumeSlider.Value;
            myMediaElement.SpeedRatio = (double)speedRatioSlider.Value;
        }

        private void Element_MediaOpened(object sender, RoutedEventArgs e)
        {
        }

        private void btnUpload_Click_1(object sender, MouseButtonEventArgs e)
        {
            var dlg = new System.Windows.Forms.OpenFileDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                myMediaElement.Source = new Uri(dlg.FileName);
                _timer.Tick += setMaxTrackValue;
                myMediaElement.Play();
            }
        }

        void setMaxTrackValue(object sender, EventArgs e)
        {
            if (myMediaElement.NaturalDuration.HasTimeSpan)
            {
                _timer.Tick -= setMaxTrackValue;
                timelineSlider.Maximum = myMediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                lblVideoDuration.Text = "Video duration: " + formatTimeSpan(myMediaElement.NaturalDuration.TimeSpan);
            }
        }

        void setUpdateTrackPos(object sender, EventArgs e)
        {
            sliderBeingUpdatedFromPlayer = true;
            try
            {
                timelineSlider.Value = myMediaElement.Position.TotalSeconds;
                timeline.CurrentDateTime = timeline.MinDateTime.AddSeconds(myMediaElement.Position.TotalSeconds);
            }
            finally
            {
                sliderBeingUpdatedFromPlayer = false;
            }
        }

        static string formatTimeSpan(TimeSpan s)
        {
            return string.Format("{0:00.}:{1:00.}:{2:00.}", s.Hours, s.Minutes, s.Seconds);
        }

        bool sliderBeingUpdatedFromPlayer = false;
        bool miniTimelinePending = false;
        private void SeekToMediaPosition(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            if (sliderBeingUpdatedFromPlayer)
                return;

            miniTimelinePending = true;
            int SliderValue = (int)timelineSlider.Value;
            TimeSpan ts = new TimeSpan(0, 0, 0, SliderValue, 0);
            myMediaElement.Stop();
            myMediaElement.Position = ts;
        }

        void finalizeMiniTimelineChange()
        {
            if (miniTimelinePending)
            {
                miniTimelinePending = false;
                myMediaElement.Play();
            }
        }

        private void timelineSlider_PreviewMouseUp_1(object sender, MouseButtonEventArgs e)
        {
            finalizeMiniTimelineChange();
        }

        private void timelineSlider_PreviewTouchUp_1(object sender, TouchEventArgs e)
        {
            finalizeMiniTimelineChange();
        }

        #endregion media player

        void setPostLoginInfo()
        {
            lblSession.Text = "Session: " + login.session.Name;
            lblDiscussion.Text = "Discussion: " + login.discussion.Subject;

            Persons = new ObservableCollection<Person>(DaoHelpers.personsOfDiscussion(login.discussion));

            timeline.MinDateTime = login.session.EstimatedDateTime;
            timeline.MaxDateTime = login.session.EstimatedEndDateTime;
            timeline.CurrentDateTime = timeline.MinDateTime;
            timeline_CurrentDateChanged_1(null, null);

            lblDiscStart.Text = "Session start: " + timeline.MinDateTime.ToString();
            lblDiscEnd.Text = "Session end: " + timeline.MaxDateTime.ToString();
            lblDiscDuration.Text = "Session duration: " + formatTimeSpan(timeline.MaxDateTime.Subtract(timeline.MinDateTime));
        }

        #region big timeline

        bool bigTimelinePending = false;
        private void timeline_CurrentDateChanged_1(object sender, EventArgs e)
        {
            var timeSpanFromStart = timeline.CurrentDateTime.Subtract(timeline.MinDateTime);
            relCurrentTime.Text = formatTimeSpan(timeSpanFromStart); //update relative label 

            if (!sliderBeingUpdatedFromPlayer)
            {
                bigTimelinePending = true;
                myMediaElement.Stop();
                myMediaElement.Position = timeSpanFromStart;
            }
        }

        void finalizeTimelineChange()
        {
            if (bigTimelinePending)
            {
                bigTimelinePending = false;
                myMediaElement.Play();
            }
        }

        private void timeline_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            finalizeTimelineChange();
        }

        private void timeline_TouchUp_1(object sender, TouchEventArgs e)
        {
            finalizeTimelineChange();
        }

        #endregion  big timeline

        private void btnSubmit_Click_1(object sender, RoutedEventArgs e)
        {
            submitEvents();

            try
            {
                DbCtx.Get().SaveChanges();
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.ToString(), "Cannot submit events due to error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Close();
        }

        void submitEvents()
        {
            foreach (var te in timeline.TimelineEvents)
            {
                var evInfo = (EventInfo)te.Tag;
                DaoHelpers.recordEvent(evInfo);
            }
        }

        private void btnMoveEvent_Click_1(object sender, RoutedEventArgs e)
        {
            if (timeline.SelectedTimelineEvents.Count != 1)
            {
                MessageBox.Show("1. Select event to move;\n2. Move timeline to new position of event;\n3. Click this button");
                return;
            }
            var moved = timeline.SelectedTimelineEvents.First();
          

            DeleteSelectedEvents();
            var ei = moved.Tag as EventInfo;
            FireStatsEvent(ei.e, ei.userId);
        }
    }
}
