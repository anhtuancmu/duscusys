using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Discussions.DbModel;
using Discussions.rt;
using Discussions.RTModel.Model;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for SideStatusBar.xaml
    /// </summary>
    public partial class SideStatusBar : UserControl
    {
        private ObservableCollection<Person> _usersStatus = new ObservableCollection<Person>();

        public ObservableCollection<Person> UsersStatus
        {
            get { return _usersStatus; }
            set { _usersStatus = value; }
        }

        public bool hidden = true;

        private UISharedRTClient _sharedRt;

        public SideStatusBar()
        {
            InitializeComponent();

            RefreshUsersStatus();
        }

        public void Init(UISharedRTClient sharedRt)
        {
            _sharedRt = sharedRt;

            SetListeners(sharedRt, true);

            if (SessionInfo.Get().discussion != null)
                lblDiscussion.Content = SessionInfo.Get().discussion.Subject;
            else
                lblDiscussion.Content = "<NO DISCUSSION>";

            lblPlayer.Content = SessionInfo.Get().person.Name;

            DataContext = this;
        }

        public void Deinit()
        {
            SetListeners(_sharedRt, false);
        }

        private void SetListeners(UISharedRTClient sharedClient, bool doSet)
        {
            var clienRt = sharedClient.clienRt;

            if (clienRt == null)
                return;

            if (doSet)
                clienRt.onlineListChanged += onlineListChanged;
            else
                clienRt.onlineListChanged -= onlineListChanged;

            if (doSet)
                clienRt.smbdLeaved += SmbdLeaved;
            else
                clienRt.smbdLeaved -= SmbdLeaved;

            if (doSet)
                clienRt.userJoins += userJoins;
            else
                clienRt.userJoins -= userJoins;

            if (doSet)
                clienRt.userLeaves += userLeaves;
            else
                clienRt.userLeaves -= userLeaves;
        }

        private void RefreshUsersStatus()
        {
            UsersStatus.Clear();

            //fresh each time!
            var discCtx = new DiscCtx(Discussions.ConfigManager.ConnStr);
            foreach (var p in discCtx.Person)
                if (p.Online)
                    UsersStatus.Add(p);

            lblOnlinePlayers.Content = "Online now:" + UsersStatus.Count;
        }

        private void SmbdLeaved()
        {
            RefreshUsersStatus();
        }

        private void onlineListChanged(IEnumerable<DiscUser> newOnlineUsers)
        {
            RefreshUsersStatus();
        }

        private void userJoins(DiscUser u)
        {
            chatArea.Text += string.Format("{0} joins\n", u.Name);
        }

        private void userLeaves(DiscUser u)
        {
            chatArea.Text += string.Format("{0} leaves\n", u.Name);
        }
    }
}