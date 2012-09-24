using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;
using Discussions;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Discussions.stats;
using EventGen.timeline;
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

        Timeline _timelineModel;

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

        public ObservableCollection<Person> Persons { get; set; }

        public ObservableCollection<Topic> Topics { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            _timelineModel = new Timeline(TimeSpan.FromMinutes(2));
            timelineView.SetModel(_timelineModel);
            currentTime.DataContext   = _timelineModel;
            videoProgress.Maximum     = _timelineModel.Range.TotalSeconds;
            videoProgress.DataContext = _timelineModel;

            _timelineModel.PropertyChanged += TimelinePropertyChanged; 

            LoginProcedure();

            DataContext = this;

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _timer.Tick += onUpdateCurrentTimeFromVideo;
            
            myMediaElement.MediaEnded += mediaEnded;
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

            var newEvent = new TimelineEvent(e, 
                                            personId, 
                                            login.discussion.Id,
                                            _timelineModel, 
                                            TimeSpan.FromSeconds(0), //taken from timeline 
                                            ((Topic)cbxTopics.SelectedItem).Id, 
                                            login.devType);
            _timelineModel.AddEvent(newEvent);       
        }

        #region eventgen handlers
        private void SurfaceWindow_KeyDown_1(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D1:
                case Key.NumPad1:
                    FireStatsEvent(StEvent.RecordingStarted);
                    break;
                case Key.D2:
                case Key.NumPad2:
                    FireStatsEvent(StEvent.RecordingStopped);
                    break;
                case Key.D3:
                case Key.NumPad3:
                    FireStatsEvent(StEvent.BadgeCreated);
                    break;
                case Key.D4:
                case Key.NumPad4:
                    FireStatsEvent(StEvent.BadgeEdited);
                    break;
                case Key.D5:
                case Key.NumPad5:
                    FireStatsEvent(StEvent.BadgeMoved);
                    break;
                case Key.D6:
                case Key.NumPad6:
                    FireStatsEvent(StEvent.BadgeZoomIn);
                    break;
                case Key.D7:
                case Key.NumPad7:
                    FireStatsEvent(StEvent.ClusterCreated);
                    break;
                case Key.D8:
                case Key.NumPad8:
                    FireStatsEvent(StEvent.ClusterDeleted);
                    break;
                case Key.D9:
                case Key.NumPad9:
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
                case Key.NumPad0:
                    FireStatsEvent(StEvent.SourceOpened);
                    break;
                case Key.Delete:
                    _timelineModel.RemoveSelectedEvents();    
                    break;
                case Key.Space:
                    if (IsPlaying)
                        Pause();
                    else
                        Play();
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
            _timelineModel.RemoveSelectedEvents();    
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
            _timelineModel.RemoveSelectedEvents();            
        }

        #endregion

        private void btnUpload_Click_1(object sender, RoutedEventArgs e)
        {
            var openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            if (openFileDialog1.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            myMediaElement.Source = new Uri(openFileDialog1.FileName);
        }

        #region media player

        bool _isPlaying = false;
        public bool IsPlaying
        {
            get
            {
                return _isPlaying;
            }
        }

        void mediaEnded(object sender, EventArgs e)
        {
            _isPlaying = false;
        }

        public void Play()
        {            
            myMediaElement.Play();
            _isPlaying = true;
            _timer.Start();
        }

        public void Pause()
        {
            _timer.Stop();
            _isPlaying = false;
            myMediaElement.Pause();
        }

        public void Stop()
        {
            _timer.Stop();
            _isPlaying = false;
            myMediaElement.Stop();
        }

        // Play the media.
        void OnMouseDownPlayMedia(object sender, MouseButtonEventArgs args)
        {
            Play();

            // Initialize the MediaElement property values.
            InitializePropertyValues();
        }

        // Pause the media.
        void OnMouseDownPauseMedia(object sender, MouseButtonEventArgs args)
        {
            Pause();
        }

        // Stop the media.
        void OnMouseDownStopMedia(object sender, MouseButtonEventArgs args)
        {
            Stop();
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

        // When the media playback is finished. Stop() the media to seek to media start.
        private void Element_MediaEnded(object sender, EventArgs e)
        {
            Stop();
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
            _timelineModel.Range = myMediaElement.NaturalDuration.TimeSpan;
            videoProgress.Maximum = _timelineModel.Range.TotalSeconds;            
        }

        private void btnUpload_Click_1(object sender, MouseButtonEventArgs e)
        {
            var dlg = new System.Windows.Forms.OpenFileDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    myMediaElement.Source = new Uri(dlg.FileName);
                    Play();
                }
                catch(Exception e1)
                {
                    myMediaElement.Source = null;
                    MessageBox.Show("Problem with video: " + e1.ToString() + "  Ensure that selected video file can be played with Windows Media Player",
                                     "Error",
                                     MessageBoxButton.OK,
                                     MessageBoxImage.Error);                    
                }
            }
        }

        #endregion media player

        void setPostLoginInfo()
        {
            lblSession.Text = "Session: " + login.session.Name;
            lblDiscussion.Text = "Discussion: " + login.discussion.Subject;

            Persons = new ObservableCollection<Person>(DaoHelpers.personsOfDiscussion(login.discussion));
         
            ///lblDiscDuration.Text = "Session duration: " + formatTimeSpan(timeline.MaxDateTime.Subtract(timeline.MinDateTime));
        }

        #region big timeline
        void TimelinePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if video is not playing and current time changed => it's changed due to manual drag, update video seek position
            if(!IsPlaying && e.PropertyName == "CurrentTime")                           
            {                  
                myMediaElement.Position = _timelineModel.CurrentTime;
            }
        }

        void onUpdateCurrentTimeFromVideo(object sender, EventArgs e)
        {
            if(Mouse.LeftButton==MouseButtonState.Released && Mouse.RightButton==MouseButtonState.Released)
                _timelineModel.CurrentTime = myMediaElement.Position;
        }

        private void timelineView_MouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            var delta = e.Delta < 0 ? 0.5 : -0.5;
            var newZoom = zoomSlider.Value + delta;
            if (zoomSlider.Minimum <= newZoom && newZoom <= zoomSlider.Maximum)
                zoomSlider.Value = newZoom;
        }

        private void zoomSlider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timelineView.ChangeZoom(e.NewValue);
        }

        private void timelineView_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            //stop the playback before manual manipulations with timeline 
            Pause();
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
            MessageBox.Show("Not implemented");
            //foreach (var te in _timelineModel.Events)
            //{
            //   // DaoHelpers.recordEvent(evInfo);
            //}
        }
    }
}
