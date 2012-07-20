using System.Collections.ObjectModel;
using System.Windows.Controls;
using Discussions.DbModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Discussions.rt;
using Discussions.RTModel.Model;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for SideStatusBar.xaml
    /// </summary>
    public partial class SideStatusBar : UserControl
    {
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

        public bool hidden = true;

        UISharedRTClient _sharedRt;
                
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

        void SetListeners(UISharedRTClient sharedClient, bool doSet)
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

        void RefreshUsersStatus()
        {
            UsersStatus.Clear();

            //fresh each time!
            var discCtx = new DiscCtx(Discussions.ConfigManager.ConnStr);
            foreach (var p in discCtx.Person)
                if (p.Online)
                    UsersStatus.Add(p);

            lblOnlinePlayers.Content = "Online now:" + UsersStatus.Count;
        }

        void SmbdLeaved()
        {
            RefreshUsersStatus();
        }
   
        void onlineListChanged(IEnumerable<DiscUser> newOnlineUsers)
        {
            RefreshUsersStatus();           
        }

        void userJoins(DiscUser u)
        {
            chatArea.Text += string.Format("{0} joins\n", u.Name);
        }

        void userLeaves(DiscUser u)
        {
            chatArea.Text += string.Format("{0} leaves\n", u.Name);
        }
    }
}
