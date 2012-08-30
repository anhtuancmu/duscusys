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
          
        public ObservableCollection<Person> Persons{get;set;}

        public ObservableCollection<Topic> Topics{get;set;}

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

            if (login.discussion==null)
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

        void FireStatsEvent(StEvent e)
        {
            if (cbxTopics.SelectedItem == null)
            {
                MessageBox.Show("Please first select topic");
                return;
            }

            if (lstUsers.SelectedItem == null)
            {
                MessageBox.Show("Please first select user who initiates the event");
                return;
            }

            var eventTimestamp = timeline.CurrentDateTime;
            var eventViewModel = new EventViewModel(e, ((Person)lstUsers.SelectedItem).Id, eventTimestamp, login.devType); 
            
            var te = new TimelineLibrary.TimelineEvent();
            te.StartDate = eventTimestamp;
            te.EndDate = eventTimestamp;
            te.IsDuration = false;            
            te.Title = eventViewModel.userName + " " + eventViewModel.evt;
            te.Description = eventViewModel.devType + ", " + eventViewModel.dateTime;
            te.Tag = new EventInfo(e, ((Person)lstUsers.SelectedItem).Id, login.discussion.Id,
                                   eventTimestamp, ((Topic)cbxTopics.SelectedItem).Id,
                                   login.devType);

            timeline.TimelineEvents.Add(te);
            timeline.ResetEvents(timeline.TimelineEvents);            
        }
    
        #region eventgen handlers 
        private void SurfaceWindow_KeyDown_1(object sender, KeyEventArgs e)
        {
            throw new NotSupportedException("broken by new event system");
            
            //switch (e.Key)
            //{
            //    case Key.D1:
            //        FireStatsEvent(StatsEvent.LinkCreated);
            //        break;
            //    case Key.D2:
            //        FireStatsEvent(StatsEvent.LinkRemoved);
            //        break;
            //    case Key.D3:
            //        FireStatsEvent(StatsEvent.BadgeCreated);
            //        break;
            //    case Key.D4:
            //        FireStatsEvent(StatsEvent.BadgeEdited);
            //        break;
            //    case Key.D5:
            //        FireStatsEvent(StatsEvent.ClusterCreated);
            //        break;
            //    case Key.D6:
            //        FireStatsEvent(StatsEvent.ClusterRemoved);
            //        break;
            //    case Key.D7:
            //        FireStatsEvent(StatsEvent.DiscussionSessionStarted);
            //        break;
            //    case Key.D8:
            //        FireStatsEvent(StatsEvent.DiscussionSessionStopped);
            //        break;
            //    case Key.D9:
            //        FireStatsEvent(StatsEvent.FreeDrawingCreated);
            //        break;
            //    case Key.D0:
            //        FireStatsEvent(StatsEvent.FreeDrawingRemoved);
            //        break;  
            //    case Key.Delete:
            //        DeleteSelectedEvents();
            //        break;
            //}
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
            throw new NotSupportedException();
            //FireStatsEvent(StatsEvent.ClusterRemoved);
        }

        private void btnDiscussionStarted_Click_1(object sender, RoutedEventArgs e)
        {
            throw new NotSupportedException();
            //FireStatsEvent(StatsEvent.DiscussionSessionStarted);
        }

        private void btnDiscussionStopped_Click_1(object sender, RoutedEventArgs e)
        {
            throw new NotSupportedException();
            //FireStatsEvent(StatsEvent.DiscussionSessionStopped);
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
            if(dlg.ShowDialog()== System.Windows.Forms.DialogResult.OK)
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
                lblVideoDuration.Content = "Video duration: " + formatTimeSpan(myMediaElement.NaturalDuration.TimeSpan);
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
            lblSession.Content = "Session: " + login.session.Name;
            lblDiscussion.Content = "Discussion: " + login.discussion.Subject;
            
            Persons = new ObservableCollection<Person>(DaoHelpers.personsOfDiscussion(login.discussion));

            var tr = new TimeRangeWnd();
            tr.ShowDialog();

            //var discSession = new DiscussionSession(login.discussion.Id);
            //DateTime startTime;
            //if (!discSession.GetStartTime(out startTime))
            //{
            //    MessageBox.Show("Cannot find start event for this discussion in DB. Start and end events are used to compute discussion duration. " +
            //                    "Will not be able to submit events",
            //                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    Application.Current.Shutdown();
            //    return;
            //}

            //DateTime endTime;
            //if (!discSession.GetEndTime(out endTime))
            //{
            //    MessageBox.Show("Cannot find start event for this discussion in DB. Start and end events are used to compute discussion duration. " +
            //                    "Will not be able to submit events",
            //                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    Application.Current.Shutdown();                
            //}

            //timeline.MinDateTime = startTime;
            //timeline.MaxDateTime = endTime;
            //timeline.CurrentDateTime = startTime;
            //timeline_CurrentDateChanged_1(null,null);

            //lblDiscStart.Content = "Discussion start: " + startTime.ToString();
            //lblDiscEnd.Content = "Discussion end: " + endTime.ToString();
            //lblDiscDuration.Content = "Discussion duration: " + formatTimeSpan(endTime.Subtract(startTime));
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
                MessageBox.Show(e1.ToString(),"Cannot submit events due to error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);                
            }

            Close();
        }

        void submitEvents()
        {
            foreach(var te in timeline.TimelineEvents)
            {
                var evInfo = (EventInfo)te.Tag;
                DaoHelpers.recordEvent(evInfo);
            }
        }
    }
}
