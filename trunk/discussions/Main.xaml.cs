using System;
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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using Discussions.model;
using Discussions.DbModel;
using DiscussionsClientRT;
using System.Windows.Threading;
using Discussions.rt;
using Discussions.VectorEditor;
using Discussions.RTModel.Model;
using System.Collections.ObjectModel;
using System.Windows.Media.Animation;
using System.Threading;
using Discussions.stats;
using System.Runtime.InteropServices;
using System.IO;
using Discussions.pdf_reader;
using Reporter.pdf;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class Main : SurfaceWindow
    {      
        UISharedRTClient sharedClient = UISharedRTClient.Instance;

        DiscWindows discWindows = DiscWindows.Get();

        ObservableCollection<Person> _usersStatus = new ObservableCollection<Person>();
        public ObservableCollection<Person> UsersStatus
        {
            get
            {
                return _usersStatus;
            }
            set
            {
                _usersStatus = value;
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

        public delegate void OnDiscFrmClosing();

        //Metro 
        private DoubleAnimationUsingKeyFrames anim;          

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Main()
        {
            InitializeComponent();

            lblVersion.Content = Utils2.VersionString();
            
            DataContext = this;

            Ctx2.sharedClient = sharedClient;
            CtxSingleton.sharedClient = sharedClient;

            avatar.pointDown = AvatarPointDown;

            foreach (EventViewModel evm in DaoUtils.GetRecentEvents())
                RecentEvents.Insert(0,evm);

            LoginProcedures();                                  

            lstBxPlayers.ItemsSource = UsersStatus;
        }

        void LoginProcedures()
        {                        
            DaoUtils.EnsureModerExists();
             
            LoginResult login = SessionInfo.Get().ExperimentMode ? LoginDriver.Run(LoginFlow.ForExperiment) : 
                                                                   LoginDriver.Run(LoginFlow.Regular);                        
            if(login==null)
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
            sharedClient.start(login, CtxSingleton.Get().Connection.DataSource, DeviceType.Wpf);

            SetListeners(sharedClient, true);
        }

        void LogoutProcedures()
        {
            if (sharedClient.clienRt != null)
                sharedClient.clienRt.SendLiveRequest();

            SetListeners(sharedClient, false);

            discWindows.CloseAndDispose();

            SessionInfo.Reset();            
            CtxSingleton.DropContext();
            Ctx2.DropContext();
        }

        static string SessionStr(Session s, Discussion d)
        {
            var tsConv = new TimeslotConverter();

            return  s.Name + " (" +
                    s.EstimatedDateTime.ToString("D") + " " +
                    (string)tsConv.Convert(s.EstimatedTimeSlot, null, null, null) + "), " + d.Subject;         
        }

        void ValidateButtons(SessionInfo session)
        {                        
            if (session.discussion != null)
            {
                btnResults.LaunchDel = startResultViewer;
                btnDiscussionInfo.LaunchDel = ShowDiscussionInfo;              
                btnPrivate.LaunchDel = startPrivateBoard;
                btnPublic.LaunchDel  = startPublicBoard;
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

        void startDashboard()
        {
            if (discWindows.moderDashboard != null)
                return;

            discWindows.moderDashboard = new Dashboard(sharedClient, 
                                            () => {
                                                discWindows.moderDashboard = null;
                                                ValidateButtons(SessionInfo.Get()); 
                                            });

            discWindows.moderDashboard.Show();
        }

        async void startResultViewer()
        {
            if (discWindows.resViewer != null)
                return;
          
            if (SessionInfo.Get().discussion != null)
            {
                await (new PdfReportDriver()).Run();
            }
            else
                MessageBox.Show("No default discussion");
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

        void startReporter()
        {
            System.Diagnostics.Process.Start("Reporter.exe");
        }

        void startMeg()
        {
            System.Diagnostics.Process.Start("EventGen.exe");
        }

        void startSeatMgr()
        {
            var stMgr = new SeatManagerWnd();
            stMgr.ShowDialog();
        }

        void startSessionMgr()
        {
            var sesMgr = new SessionManagerWnd(sharedClient);
            sesMgr.ShowDialog();
        }

        void startSessionViewer()
        {
            var sesViewer = new SessionViewerDashboard();
            sesViewer.ShowDialog();
        }

        private void SurfaceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LogoutProcedures();
        }

        void startUserManager()
        {
            if (discWindows.persMgr != null)
                return;

            discWindows.persMgr = new PersonManagerWnd(sharedClient, () => { discWindows.persMgr = null; });
            discWindows.persMgr.Show();
        }       

        void startLogOut()
        {
            LogoutProcedures();

            this.Hide();

            LoginProcedures();

            this.Show();             
        }

        void startPublicBoard()
        {
            var wnd = DiscWindows.Get();
            if (wnd.discDashboard != null)
            {
                wnd.discDashboard.Activate();
            }
            else
            {
                wnd.discDashboard = new PublicCenter(UISharedRTClient.Instance,
                                                    () => { wnd.discDashboard = null; }
                                                    );
                wnd.discDashboard.Show();
            }            
        }

        void startPrivateBoard()
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

        void SetListeners(UISharedRTClient sharedClient, bool doSet)
        {
            var clienRt = sharedClient.clienRt;

            if (clienRt == null)
                return;

            if (doSet)
                clienRt.onlineListChanged += OnlineListChanged;
            else
                clienRt.onlineListChanged -= OnlineListChanged;   

            if(doSet)
                clienRt.smbdLeaved += SmbdLeaved;
            else
                clienRt.smbdLeaved -= SmbdLeaved;

            if (doSet)
                clienRt.userAccPlusMinus += UserAccPlusMinus;
            else
                clienRt.userAccPlusMinus -= UserAccPlusMinus;

            if(doSet)
                clienRt.onStatsEvent += OnStatsEvent;
            else
                clienRt.onStatsEvent -= OnStatsEvent;            
        }

        void RefreshUsersStatus()
        {
            UsersStatus.Clear();

            //fresh each time!
            var discCtx = new DiscCtx(Discussions.ConfigManager.ConnStr);
            foreach (var p in discCtx.Person)
            {
                UsersStatus.Insert(0,p);
            }

            Dispatcher.BeginInvoke(new Action(() => { ValidateButtons(SessionInfo.Get());}), 
                                   System.Windows.Threading.DispatcherPriority.Background, 
                                   null);
        }

        void OnlineListChanged(IEnumerable<DiscUser> onlineUsers)
        {
            RefreshUsersStatus();
        }

        void SmbdLeaved()
        {
            RefreshUsersStatus();
        }

        void UserAccPlusMinus()
        {
            RefreshUsersStatus();
        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {           
             anim = new DoubleAnimationUsingKeyFrames();
             anim.Duration = TimeSpan.FromMilliseconds(1800);
        }

        void setTiles()
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
        } 

        void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e )
        {
            setTiles();

            SkinManager.ChangeSkin("Blue2Skin.xaml", this.Resources);            
        }

        void MinimizeButton_Click(object sender, System.Windows.RoutedEventArgs e) 
        {            
            this.WindowState = WindowState.Minimized;
        }            

        void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
        }

        private void RedSkinButton_Click(object sender, RoutedEventArgs e)
        {
            SkinManager.ChangeSkin("RedSkin.xaml",this.Resources);
        }

        void PurpleSkinButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SkinManager.ChangeSkin("PurpleSkin.xaml",this.Resources);
        }

        private void GreenSkinButton_Click(object sender, RoutedEventArgs e)
        {
            SkinManager.ChangeSkin("GreenSkin.xaml",this.Resources);
        }

        private void BlueSkinButton_Click(object sender, RoutedEventArgs e)
        {
            SkinManager.ChangeSkin("Blue2Skin.xaml",this.Resources);
        }

        void CommitAvaNameChanges(Person pers)
        {
            var userId = pers.Id; 
            var usr = CtxSingleton.Get().Person.FirstOrDefault(p0 => p0.Id == userId);
            usr.Name = pers.Name;
            CtxSingleton.Get().SaveChanges();
        }

        void AvatarPointDown(bool name)
        {
            if (name)
            {
                var inpDlg = new InpDialog("Name","<User>");
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

        void SetNewAvatar()
        {
            Attachment avaAttachment = AttachmentManager.AttachPicture(null);
            if (avaAttachment == null)
                return;

            Person currentPers = SessionInfo.Get().person;

            currentPers.AvatarAttachment = avaAttachment;
            CtxSingleton.Get().SaveChanges();

            //refresh avatar            
            avatar.DataContext = null;
            avatar.DataContext = currentPers;                       
        }

        void ShowDiscussionInfo()
        {
            if (SessionInfo.Get().discussion == null)
                return;

            Discussion d = SessionInfo.Get().discussion;
            if (d == null)
                return;

            var diz = new DiscussionInfoZoom(CtxSingleton.Get().Discussion.FirstOrDefault(d0 => d0.Id == d.Id));
            diz.ShowDialog();
        }

        void OnStatsEvent(StEvent e, int userId, int discussionId, int topicId, DeviceType devType)
        {
            ShowRecentEvent(new EventViewModel(e, userId, DateTime.Now, devType));           
        }

        // keeps size-restricted list of n recent events
        void ShowRecentEvent(EventViewModel e)
        {           
            RecentEvents.Insert(0, e);          
        }     
    }
}
