using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using AbstractionLayer;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Discussions.RTModel.Model;
using Discussions.stats;

namespace Discussions.view
{
    public partial class Main : PortableWindow
    {
        private readonly UISharedRTClient sharedClient = UISharedRTClient.Instance;

        private readonly DiscWindows discWindows = DiscWindows.Get();

        private ObservableCollection<Person> _usersStatus = new ObservableCollection<Person>();

        public ObservableCollection<Person> UsersStatus
        {
            get { return _usersStatus; }
            set { _usersStatus = value; }
        }

        private ObservableCollection<EventViewModel> _recentEvents = new ObservableCollection<EventViewModel>();

        public ObservableCollection<EventViewModel> RecentEvents
        {
            get { return _recentEvents; }
            set { _recentEvents = value; }
        }

        public delegate void OnDiscFrmClosing();

        //Metro 
        private DoubleAnimationUsingKeyFrames anim;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Main()
        {
            InitializeComponent();

            //special case of screenshot mode
            if (SessionInfo.Get().ScreenshotMode)
            {
                var discId = SessionInfo.Get().screenDiscId;
                PrivateCenterCtx.sharedClient = sharedClient;
                PublicBoardCtx.sharedClient = sharedClient;
                SessionInfo.Get().discussion = PrivateCenterCtx.Get().Discussion.FirstOrDefault(d => d.Id == discId);
                SessionInfo.Get().setPerson(PrivateCenterCtx.Get().Person.FirstOrDefault(p => p.Name == "moderator"));
                var loginRes = new LoginResult();
                loginRes.devType = DeviceType.Wpf;
                loginRes.discussion = SessionInfo.Get().discussion;
                loginRes.person = SessionInfo.Get().person;
                sharedClient.start(loginRes, ConfigManager.ServiceServer, DeviceType.Wpf);

                this.Hide();

                sharedClient.clienRt.onJoin += () =>
                    {
                        PublicCenter pubCenter = new PublicCenter(UISharedRTClient.Instance,
                                                                  () => { },
                                                                  SessionInfo.Get().screenTopicId,
                                                                  SessionInfo.Get().screenDiscId
                            );

                        pubCenter.Show();
                        pubCenter.Hide();

                        Task<PublicCenter.ScreenshoReports> t = pubCenter.FinalSceneScreenshots();
                        t.GetAwaiter().OnCompleted(() =>
                            {
                                pubCenter.Close();
                                pubCenter = null;

                                var reports = t.Result;
                                Utils.ScreenshotPackToMetaInfo(reports, SessionInfo.Get().screenMetaInfo);
                                Application.Current.Shutdown();                                   
                            });
                    };
                return;
            }

            lblVersion.Content = Utils2.VersionString();

            DataContext = this;

            PrivateCenterCtx.sharedClient = sharedClient;
            PublicBoardCtx.sharedClient = sharedClient;

            avatar.pointDown = AvatarPointDown;

            foreach (EventViewModel evm in DaoUtils.GetRecentEvents())
                RecentEvents.Insert(0, evm);

            LoginProcedures();

            lstBxPlayers.ItemsSource = UsersStatus;
        }

        private void LoginProcedures()
        {
            DaoUtils.EnsureModerExists();

            LoginResult login = SessionInfo.Get().ExperimentMode
                                    ? LoginDriver.Run(LoginFlow.ForExperiment)
                                    : LoginDriver.Run(LoginFlow.Regular);
            if (login == null)
            {
                Application.Current.Shutdown();
                return;
            }

            if (login.session != null && login.discussion != null)
                lblSessionInfo.Content = SessionStr(login.session, login.discussion);
            else
                lblSessionInfo.Content = "";

            SessionInfo.Get().discussion = login.discussion;
            SessionInfo.Get().setPerson(login.person);

            discWindows.mainWnd = this;

            avatar.DataContext = login.person;

            //start rt client
            sharedClient.start(login, ConfigManager.ServiceServer, DeviceType.Wpf);

            SetListeners(sharedClient, true);
        }

        private void LogoutProcedures()
        {
            if (sharedClient.clienRt != null)
                sharedClient.clienRt.SendLiveRequest();

            SetListeners(sharedClient, false);

            discWindows.CloseAndDispose();

            SessionInfo.Reset();
            PublicBoardCtx.DropContext();
            PrivateCenterCtx.DropContext();
        }

        private static string SessionStr(Session s, Discussion d)
        {
            var tsConv = new TimeslotConverter();

            return s.Name + " (" +
                   s.EstimatedDateTime.ToString("D") + " " +
                   (string) tsConv.Convert(s.EstimatedTimeSlot, null, null, null) + "), " + d.Subject;
        }

        private void ValidateButtons(SessionInfo session)
        {
            if (session.discussion != null)
            {
                btnResults.LaunchDel = StartResultViewer;
                btnDiscussionInfo.LaunchDel = ShowDiscussionInfo;
                btnPrivate.LaunchDel = startPrivateBoard;
                btnPublic.LaunchDel = startPublicBoard;
            }
            else
            {
                btnResults.LaunchDel = null;
                btnPrivate.LaunchDel = null;
                btnPublic.LaunchDel = null;
                btnDiscussionInfo.LaunchDel = null;
                DiscWindows.Get().CloseUserDashboards();
            }

            if (session.IsModerator)
            {
                btnSeatManager.LaunchDel = startSeatMgr;
                btnModeratorBoard.LaunchDel = startDashboard;
                btnUserManager.LaunchDel = startUserManager;
                btnSessionManager.LaunchDel = startSessionMgr;
                btnSessionViewer.LaunchDel = startSessionViewer;
                btnReporter.LaunchDel = startReporter;
                btnMeg.LaunchDel = startMeg;
            }
            else
            {
                btnSeatManager.LaunchDel = null;
                btnModeratorBoard.LaunchDel = null;
                btnUserManager.LaunchDel = null;
                btnSessionManager.LaunchDel = null;
                btnSessionViewer.LaunchDel = null;
                btnReporter.LaunchDel = null;
                btnMeg.LaunchDel = null;
            }

            btnLogOut.LaunchDel = startLogOut;
        }

        private void startDashboard()
        {
            if (discWindows.moderDashboard != null)
                return;

            discWindows.moderDashboard = new Dashboard(sharedClient,
                                                       () =>
                                                           {
                                                               discWindows.moderDashboard = null;
                                                               ValidateButtons(SessionInfo.Get());
                                                           });

            discWindows.moderDashboard.Show();
        }

        private void StartResultViewer()
        {
            if (discWindows.resViewer != null)
                return;

            HtmlReportBrowsing.startResultViewer();
        }

        //void startMeg()
        //{
        //    if (discWindows.resViewer != null)
        //        return;

        //    if (SessionInfo.Get().discussion != null)
        //    {
        //        discWindows.resViewer = new ResultViewer(SessionInfo.Get().discussion, () => { discWindows.resViewer = null; });
        //        discWindows.resViewer.Show();
        //    }
        //    else
        //        MessageBox.Show("No default discussion");
        //}

        private void startReporter()
        {
            System.Diagnostics.Process.Start("Reporter.exe");
        }

        private void startMeg()
        {
            System.Diagnostics.Process.Start("EventGen.exe");
        }

        private void startSeatMgr()
        {
            var stMgr = new SeatManagerWnd();
            stMgr.ShowDialog();
        }

        private void startSessionMgr()
        {
            var sesMgr = new SessionManagerWnd(sharedClient);
            sesMgr.ShowDialog();
        }

        private void startSessionViewer()
        {
            var sesViewer = new SessionViewerDashboard();
            sesViewer.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LogoutProcedures();
        }

        private void startUserManager()
        {
            if (discWindows.persMgr != null)
                return;

            discWindows.persMgr = new PersonManagerWnd(sharedClient, () => { discWindows.persMgr = null; });
            discWindows.persMgr.Show();
        }

        private void startLogOut()
        {
            LogoutProcedures();

            this.Hide();

            LoginProcedures();

            this.Show();
        }

        private void startPublicBoard()
        {
            var wnd = DiscWindows.Get();
            if (wnd.discDashboard != null)
            {
                wnd.discDashboard.Activate();
            }
            else
            {
                wnd.discDashboard = new PublicCenter(UISharedRTClient.Instance,
                                                     () => { wnd.discDashboard = null; },
                                                     -1, -1
                    );
                wnd.discDashboard.Show();
            }
        }

        private void startPrivateBoard()
        {
            var wnd = DiscWindows.Get();
            if (wnd.privateDiscBoard != null)
            {
                wnd.privateDiscBoard.Activate();
            }
            else
            {
                wnd.privateDiscBoard = new PrivateCenter3(UISharedRTClient.Instance,
                                                          () => { wnd.privateDiscBoard = null; });
                wnd.privateDiscBoard.Show();
            }
        }

        private void SetListeners(UISharedRTClient sharedClient, bool doSet)
        {
            var clienRt = sharedClient.clienRt;

            if (clienRt == null)
                return;

            if (doSet)
                clienRt.onlineListChanged += OnlineListChanged;
            else
                clienRt.onlineListChanged -= OnlineListChanged;

            if (doSet)
                clienRt.smbdLeaved += SmbdLeaved;
            else
                clienRt.smbdLeaved -= SmbdLeaved;

            if (doSet)
                clienRt.userAccPlusMinus += UserAccPlusMinus;
            else
                clienRt.userAccPlusMinus -= UserAccPlusMinus;

            if (doSet)
                clienRt.onStatsEvent += OnStatsEvent;
            else
                clienRt.onStatsEvent -= OnStatsEvent;
        }

        private void RefreshUsersStatus()
        {
            UsersStatus.Clear();

            //fresh each time!
            var discCtx = new DiscCtx(Discussions.ConfigManager.ConnStr);
            foreach (var p in discCtx.Person)
            {
                UsersStatus.Insert(0, p);
            }

            Dispatcher.BeginInvoke(new Action(() => { ValidateButtons(SessionInfo.Get()); }),
                                   System.Windows.Threading.DispatcherPriority.Background,
                                   null);
        }

        private void OnlineListChanged(IEnumerable<DiscUser> onlineUsers)
        {
            RefreshUsersStatus();
        }

        private void SmbdLeaved()
        {
            RefreshUsersStatus();
        }

        private void UserAccPlusMinus()
        {
            RefreshUsersStatus();
        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            anim = new DoubleAnimationUsingKeyFrames();
            anim.Duration = TimeSpan.FromMilliseconds(1800);
        }

        private void setTiles()
        {
            btnModeratorBoard.BtnTitle = "Moderator dashboard";
            btnUserManager.BtnTitle = "User manager";
            btnPrivate.BtnTitle = "Private dashboard";
            btnPublic.BtnTitle = "Public dashboard";
            // btnResults.BtnTitle = "Results";
            btnSeatManager.BtnTitle = "Seat manager";
            btnLogOut.BtnTitle = "Log out";
            btnSessionManager.BtnTitle = "Session manager";
            btnSessionViewer.BtnTitle = "Session/user viewer";
            btnDiscussionInfo.BtnTitle = "About this discussion";
            btnReporter.BtnTitle = "Reports";
            btnMeg.BtnTitle = "MEG";
            btnResults.BtnTitle = "View results"; 
        }

        private void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            setTiles();

            SkinManager.ChangeSkin("Blue2Skin.xaml", this.Resources);
        }

        private void MinimizeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }

        private void RedSkinButton_Click(object sender, RoutedEventArgs e)
        {
            SkinManager.ChangeSkin("RedSkin.xaml", this.Resources);
        }

        private void PurpleSkinButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SkinManager.ChangeSkin("PurpleSkin.xaml", this.Resources);
        }

        private void GreenSkinButton_Click(object sender, RoutedEventArgs e)
        {
            SkinManager.ChangeSkin("GreenSkin.xaml", this.Resources);
        }

        private void BlueSkinButton_Click(object sender, RoutedEventArgs e)
        {
            SkinManager.ChangeSkin("Blue2Skin.xaml", this.Resources);
        }

        private void CommitAvaNameChanges(Person pers)
        {
            var userId = pers.Id;
            var usr = PublicBoardCtx.Get().Person.FirstOrDefault(p0 => p0.Id == userId);
            usr.Name = pers.Name;
            PublicBoardCtx.Get().SaveChanges();
        }

        private void AvatarPointDown(bool name)
        {
            if (name)
            {
                var inpDlg = new InpDialog("Name", "<User>");
                inpDlg.ShowDialog();
                if (inpDlg.Answer != null)
                {
                    SessionInfo.Get().person.Name = inpDlg.Answer;
                    CommitAvaNameChanges(SessionInfo.Get().person);

                    //refresh avatar            
                    var pers = SessionInfo.Get().person;
                    avatar.DataContext = null;
                    avatar.DataContext = pers;
                }
            }
            else
            {
                SetNewAvatar();
            }

            sharedClient.clienRt.SendAvaNameChanged();
        }

        private void SetNewAvatar()
        {
            Attachment avaAttachment = AttachmentManager.AttachPicture(null);
            if (avaAttachment == null)
                return;

            Person currentPers = SessionInfo.Get().person;

            currentPers.AvatarAttachment = avaAttachment;
            PublicBoardCtx.Get().SaveChanges();

            //refresh avatar            
            avatar.DataContext = null;
            avatar.DataContext = currentPers;
        }

        private void ShowDiscussionInfo()
        {
            if (SessionInfo.Get().discussion == null)
                return;

            Discussion d = SessionInfo.Get().discussion;
            if (d == null)
                return;

            var diz = new DiscussionInfoZoom(PublicBoardCtx.Get().Discussion.FirstOrDefault(d0 => d0.Id == d.Id));
            diz.ShowDialog();
        }

        private void OnStatsEvent(StEvent e, int userId, int discussionId, int topicId, DeviceType devType)
        {
            ShowRecentEvent(new EventViewModel(e, userId, DateTime.Now, devType));
        }

        // keeps size-restricted list of n recent events
        private void ShowRecentEvent(EventViewModel e)
        {
            RecentEvents.Insert(0, e);
        }

        private void SurfaceButton_Click_1(object sender, RoutedEventArgs e)
        {
            UISharedRTClient.Instance.clienRt.SendScreenshotRequest(1, 2);
        }
    }
}