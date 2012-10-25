using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using Reporter;

namespace EventGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EventGenWnd : SurfaceWindow
    {
        LoginResult login = null;

        Timeline _timelineModel;

        UISharedRTClient sharedClient = new UISharedRTClient();

        DispatcherTimer _timer;

        EventGen.timeline.Session _session;

        EventTotalsReport _totalsReport = new EventTotalsReport();

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

        public EventGenWnd()
        {
            InitializeComponent();

            _timelineModel = new Timeline(TimeSpan.FromMinutes(2));
            timelineView.SetModel(_timelineModel);
            timelineView.ChangeZoom(zoomSlider.Value);
            currentTime.DataContext   = _timelineModel;
            videoProgress.Maximum     = _timelineModel.Range.TotalSeconds;
            videoProgress.DataContext = _timelineModel;

            _timelineModel.PropertyChanged += TimelinePropertyChanged;

            _session = new EventGen.timeline.Session(_timelineModel);

            LoginProcedure();

            DataContext = this;

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _timer.Tick += onUpdateCurrentTimeFromVideo;
            
            myMediaElement.MediaEnded += mediaEnded;
        }

        void ensureCurrentTimeInView()
        {
            if (needsScroll())
                scrollCurrentTimeIntoView();
        }

        void scrollCurrentTimeIntoView()
        {
            var currentTimePos = TimeScale.TimeToPosition(_timelineModel.CurrentTime, timelineView.Zoom);
            timelineScroller.ScrollToHorizontalOffset(currentTimePos - 50);
        }

        bool needsScroll()
        {
            var currentTimePos = TimeScale.TimeToPosition(_timelineModel.CurrentTime, timelineView.Zoom);           
            return (currentTimePos < timelineScroller.ContentHorizontalOffset ||
                    currentTimePos > timelineScroller.ViewportWidth + timelineScroller.ContentHorizontalOffset);                 
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

        bool FireStatsEvent(StEvent e, int personId = -2)
        {
            if (cbxTopics.SelectedItem == null)
            {
                MessageBox.Show("Please first select topic");
                return false;
            }

            if (personId == -2)
            {
                if (lstUsers.SelectedItem == null)
                {
                    MessageBox.Show("Please first select user who initiates the event");
                    return false;
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
                                            _timelineModel.CurrentTime, 
                                            ((Topic)cbxTopics.SelectedItem).Id, 
                                            login.devType);
            EventGen.timeline.CommandManager.Instance.RegisterDoneCommand(new CreateEventCommand(newEvent, true));
            UpdateEventCounts();
            return true;
        }

        #region eventgen handlers
        private void SurfaceWindow_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                switch (e.Key)
                {
                    case Key.Z:
                        menuUndo_Click_1(null,null);                       
                        break;
                    case Key.Y:
                        menuRedo_Click_1(null, null);   
                        break;
                    case Key.S:
                        menuSave_Click_1(null, null);
                        break;
                    case Key.O:
                        menuOpen_Click_1(null, null);
                        break;
                }                            
            }
            else switch (e.Key)
            {
                case Key.D1:
                case Key.NumPad1:
                    btnRecordingStarted_Click_1(null, null);                    
                    break;
                case Key.D2:
                case Key.NumPad2:
                    btnRecordingStopped_Click_1(null, null);                      
                    break;
                case Key.D3:
                case Key.NumPad3:
                    btnBadgeCreated_Click_1(null, null);                      
                    break;
                case Key.D4:
                case Key.NumPad4:
                    btnBadgeEdited_Click_1(null, null);                          
                    break;
                case Key.D5:
                case Key.NumPad5:
                    btnBadgeMoved_Click_1(null, null);                      
                    break;
                case Key.D6:
                case Key.NumPad6:
                    btnBadgeZoomIn_Click_1(null, null);                     
                    break;
                case Key.D7:
                case Key.NumPad7:
                    btnClusterCreated_Click_1(null,null);                    
                    break;
                case Key.D8:
                case Key.NumPad8:
                    btnClusterRemoved_Click_1(null, null);                        
                    break;
                case Key.D9:
                case Key.NumPad9:
                    btnClusterIn_Click_1(null, null);                           
                    break;
                case Key.Q:                    
                    btnClusterOut_Click_1(null,null);
                    break;
                case Key.W:
                    btnClusterMoved_Click_1(null, null);                    
                    break;
                case Key.E:
                    btnLinkCreated_Click_1(null, null);                        
                    break;
                case Key.R:
                    btnLinkRemoved_Click_1(null, null);                    
                    break;
                case Key.T:
                    btnFreeDrawingCreated_Click_1(null, null);                    
                    break;
                case Key.Y:
                    btnFreeDrawingRemoved_Click_1(null, null);                           
                    break;
                case Key.U:
                    btnFreeDrawingResize_Click_1(null, null);                            
                    break;
                case Key.I:
                    btnFreeDrawingMoved_Click_1(null, null);                         
                    break;
                case Key.O:
                    btnSceneZoomIn_Click_1(null, null);                        
                    break;
                case Key.P:
                    btnSceneZoomOut_Click_1(null, null);
                    break;
                case Key.A:
                    btnArgPointTopicChanged_Click_1(null, null);                   
                    break;
                case Key.S:
                    btnSourceAdded_Click_1(null,null);                    
                    break;
                case Key.D:
                    btnSourceRemoved_Click_1(null, null);                        
                    break;
                case Key.F:
                    btnImageAdded_Click_1(null, null);                          
                    break;
                case Key.H:
                    btnPdfAdded_Click_1(null, null);                         
                    break;
                case Key.K:
                    btnYoutubeAdded_Click_1(null,null);                    
                    break;
                case Key.L:
                    btnScreenshotAdded_Click_1(null,null);
                    break;
                case Key.Z:
                    btnMediaRemoved_Click_1(null, null);                    
                    break;
                case Key.X:
                    btnCommentAdded_Click_1(null, null);                          
                    break;
                case Key.C:
                    btnCommentRemoved_Click_1(null, null);                         
                    break;
                case Key.V:
                    btnImageOpened_Click(null, null);                         
                    break;
                case Key.B:
                    btnVideoOpened_Click_1(null, null);                    
                    break;
                case Key.N:
                    btnScreenshotOpened_Click_1(null, null);                     
                    break;
                case Key.M:
                    btnPdfOpened_Click_1(null, null);                     
                    break;
                case Key.D0:
                case Key.NumPad0:
                    btnSourceOpened_Click_1(null, null);                          
                    break;
                case Key.Delete:
                    btnDeleteClick(null, null);                            
                    break;
                case Key.Space:
                    if (IsPlaying)
                        Pause();
                    else
                        Play();
                    break; 
                case Key.Left:
                    btnTimeLeftClick(null,null);
                    break;
                case Key.Right:
                    btnTimeRightClick(null, null);
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
            FireStatsEvent(StEvent.ScreenshotOpened);
        }

        private void btnPdfOpened_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.PdfOpened);
        }

        private void btnSourceOpened_Click_1(object sender, RoutedEventArgs e)
        {
            FireStatsEvent(StEvent.SourceOpened);
        }

        static string countToString(int cnt)
        {
            if (cnt == 0)
                return "";
            else
                return "+" + cnt.ToString();
        }

        void UpdateEventCounts()
        {
            //reset prec values
            _totalsReport = new EventTotalsReport();

            var fakeEventId = 0;
            foreach (var te in _timelineModel.Events)
            {
                _totalsReport.CountEvent(te.e, fakeEventId++);
            }
            btnArgPointTopicChanged.eventCount.Text = countToString(_totalsReport.TotalArgPointTopicChanged);
            btnBadgeCreated.eventCount.Text = countToString(_totalsReport.TotalBadgeCreated);
            btnBadgeEdited.eventCount.Text = countToString(_totalsReport.TotalBadgeEdited);
            btnBadgeMoved.eventCount.Text = countToString(_totalsReport.TotalBadgeMoved);
            btnBadgeZoomIn.eventCount.Text = countToString(_totalsReport.TotalBadgeZoomIn);
            btnClusterCreated.eventCount.Text = countToString(_totalsReport.TotalClusterCreated);
            btnClusterRemoved.eventCount.Text = countToString(_totalsReport.TotalClusterDeleted);
            btnClusterIn.eventCount.Text = countToString(_totalsReport.TotalClusterIn);
            btnClusterMoved.eventCount.Text = countToString(_totalsReport.TotalClusterMoved);
            btnClusterOut.eventCount.Text = countToString(_totalsReport.TotalClusterOut);
            btnCommentAdded.eventCount.Text = countToString(_totalsReport.TotalCommentAdded);
            btnCommentRemoved.eventCount.Text = countToString(_totalsReport.TotalCommentRemoved);
            btnFreeDrawingCreated.eventCount.Text = countToString(_totalsReport.TotalFreeDrawingCreated);
            btnFreeDrawingMoved.eventCount.Text = countToString(_totalsReport.TotalFreeDrawingMoved);
            btnFreeDrawingRemoved.eventCount.Text = countToString(_totalsReport.TotalFreeDrawingRemoved);
            btnFreeDrawingResize.eventCount.Text = countToString(_totalsReport.TotalFreeDrawingResize);
            btnImageAdded.eventCount.Text = countToString(_totalsReport.TotalImageAdded);
            btnImageOpened.eventCount.Text = countToString(_totalsReport.TotalImageOpened);
            btnLinkCreated.eventCount.Text = countToString(_totalsReport.TotalLinkCreated);
            btnLinkRemoved.eventCount.Text = countToString(_totalsReport.TotalLinkRemoved);
            btnMediaRemoved.eventCount.Text = countToString(_totalsReport.TotalMediaRemoved);
            btnPdfAdded.eventCount.Text = countToString(_totalsReport.TotalPdfAdded);
            btnPdfOpened.eventCount.Text = countToString(_totalsReport.TotalPdfOpened);
            btnRecordingStarted.eventCount.Text = countToString(_totalsReport.TotalRecordingStarted);
            btnRecordingStopped.eventCount.Text = countToString(_totalsReport.TotalRecordingStopped);
            btnSceneZoomIn.eventCount.Text = countToString(_totalsReport.TotalSceneZoomedIn);
            btnSceneZoomOut.eventCount.Text = countToString(_totalsReport.TotalSceneZoomedOut);
            btnScreenshotAdded.eventCount.Text = countToString(_totalsReport.TotalScreenshotAdded);
            btnScreenshotOpened.eventCount.Text = countToString(_totalsReport.TotalScreenshotOpened);
            btnSourceAdded.eventCount.Text = countToString(_totalsReport.TotalSourceAdded);
            btnSourceOpened.eventCount.Text = countToString(_totalsReport.TotalSourceOpened);
            btnSourceRemoved.eventCount.Text = countToString(_totalsReport.TotalSourceRemoved);
            btnSourceOpened.eventCount.Text = countToString(_totalsReport.TotalSourceOpened);
            btnYoutubeAdded.eventCount.Text = countToString(_totalsReport.TotalYoutubeAdded);
            btnVideoOpened.eventCount.Text = countToString(_totalsReport.TotalVideoOpened);
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
            _timelineModel.CurrentTime = TimeSpan.FromSeconds(0);
            _timelineModel.Range = myMediaElement.NaturalDuration.TimeSpan;
            videoLen.Text = _timelineModel.Range.ToString("hh\\:mm\\:ss");
            timelineView.ChangeZoom(timelineView.Zoom);
            videoProgress.Maximum = _timelineModel.Range.TotalSeconds;            
        }

        private void btnUpload_Click_1(object sender, MouseButtonEventArgs e)
        {
            var dlg = new System.Windows.Forms.OpenFileDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                HandleOnMediaOpened(dlg.FileName);
            }
        }

        void HandleOnMediaOpened(string mediaPathName)
        {
            try
            {
                myMediaElement.Source = new Uri(mediaPathName);
                Play();
                _session.videoPathName = mediaPathName;
            }
            catch (Exception e1)
            {
                myMediaElement.Source = null;
                MessageBox.Show("Problem with video: " + e1.ToString() + "  Ensure that selected video file can be played with Windows Media Player",
                                 "Error",
                                 MessageBoxButton.OK,
                                 MessageBoxImage.Error);
            }
        }

        #endregion media player

        void setPostLoginInfo()
        {
            Title += "  |  " + login.discussion.Subject + "  |  " + login.session.Name;

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
            if (Mouse.LeftButton == MouseButtonState.Released && Mouse.RightButton == MouseButtonState.Released)
            {
                _timelineModel.CurrentTime = myMediaElement.Position;
                ensureCurrentTimeInView();
            }
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

        private void btnUndo_Click_1(object sender, RoutedEventArgs e)
        {
            EventGen.timeline.CommandManager.Instance.Undo();
        }

        private void btnRedo_Click_1(object sender, RoutedEventArgs e)
        {
            EventGen.timeline.CommandManager.Instance.Redo();
        }

        private void menuOpen_Click_1(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.Filter = "MEG timeline file |*.meg";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _session.Read(dlg.FileName);

                if (!File.Exists(_session.videoPathName))
                {
                    MessageBox.Show("Cannot find video file " + _session.videoPathName + ". Selected MEG timeline references it");
                    return;
                }

                HandleOnMediaOpened(_session.videoPathName);
            }
        }

        void btnOpenClick(object sender, RoutedEventArgs e)
        {
            menuOpen_Click_1(null,null);
        }

        void SaveTimeline(bool saveAs)
        {
            if (_session.videoPathName == "")
            {
                MessageBox.Show("Cannot save timeline, no video selected");
                return;
            }

            var megPathName = _session.megFilePathName;
            if (megPathName == "" || saveAs)
            {
                var dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.CheckFileExists = false;
                dlg.Filter = "MEG timeline file |*.meg";
                // dlg.DefaultExt = ".meg";
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    megPathName = dlg.FileName;
                }
            }

            if (megPathName != "")
                _session.Write(megPathName);
        }

        private void menuSave_Click_1(object sender, RoutedEventArgs e)
        {
            SaveTimeline(false);
        }

        void btnSaveClick(object sender, RoutedEventArgs e)
        {
            menuSave_Click_1(null, null);
        }

        private void menuSaveAs_Click_1(object sender, RoutedEventArgs e)
        {
            SaveTimeline(true);
        }

        void btnSaveAsClick(object sender, RoutedEventArgs e)
        {
            menuSaveAs_Click_1(null, null);
        }

        private void menuSubmit_Click_1(object sender, RoutedEventArgs e)
        {            
            var baseStamp = login.session.EstimatedDateTime;
            (new SubmitionWnd(_timelineModel, baseStamp)).ShowDialog();
        }

        void btnSubmitClick(object sender, RoutedEventArgs e)
        {
            menuSubmit_Click_1(null, null);
        }

        private void menuDelete_Click_1(object sender, RoutedEventArgs e)
        {
            _timelineModel.RemoveSelectedEvents(EventGen.timeline.CommandManager.Instance);
            UpdateEventCounts();
        }

        void btnDeleteClick(object sender, RoutedEventArgs e)
        {
            menuDelete_Click_1(null, null);
        }

        private void menuUndo_Click_1(object sender, RoutedEventArgs e)
        {
            EventGen.timeline.CommandManager.Instance.Undo();
            UpdateEventCounts();
        }
        
        void btnUndoClick(object sender, RoutedEventArgs e)
        {
            menuUndo_Click_1(null, null);
        }

        private void menuRedo_Click_1(object sender, RoutedEventArgs e)
        {
            EventGen.timeline.CommandManager.Instance.Redo();
            UpdateEventCounts();
        }

        void btnRedoClick(object sender, RoutedEventArgs e)
        {
            menuRedo_Click_1(null, null);
        }

        void btnTimeLeftClick(object sender, RoutedEventArgs e)
        {
            _timelineModel.CurrentTime -= TimeSpan.FromSeconds(0.2);
        }

        void btnTimeRightClick(object sender, RoutedEventArgs e)
        {
            _timelineModel.CurrentTime += TimeSpan.FromSeconds(0.2);
        }
    }
}
