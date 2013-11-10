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
    public partial class AvatarBar : UserControl
    {
        public delegate void PaletteOwnerChanged(int owner);

        public PaletteOwnerChanged paletteOwnerChanged = null;

        private ObservableCollection<Person> _usersStatus = new ObservableCollection<Person>();

        public ObservableCollection<Person> UsersStatus
        {
            get { return _usersStatus; }
            set { _usersStatus = value; }
        }

        public bool hidden = true;

        private UISharedRTClient _sharedRt;

        public AvatarBar()
        {
            InitializeComponent();

            RefreshUsersStatus();
        }

        public void Init(UISharedRTClient sharedRt)
        {
            _sharedRt = sharedRt;

            SetListeners(sharedRt, true);

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
        }

        private void RefreshUsersStatus()
        {
            UsersStatus.Clear();

            //fresh each time!
            var discCtx = new DiscCtx(Discussions.ConfigManager.ConnStr);
            foreach (var p in discCtx.Person)
                if (p.Online)
                    UsersStatus.Add(p);
        }

        private void SmbdLeaved()
        {
            RefreshUsersStatus();
        }

        private void onlineListChanged(IEnumerable<DiscUser> newOnlineUsers)
        {
            RefreshUsersStatus();
        }

        private void lstBxPlayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int owner = -1;
            if (e.AddedItems.Count > 0)
            {
                var person = (Person) e.AddedItems[0];
                owner = person.Id;
            }

            if (paletteOwnerChanged != null)
                paletteOwnerChanged(owner);
        }

        public void SelectCurrentUser()
        {
            var currId = SessionInfo.Get().person.Id;
            foreach (Person pers in lstBxPlayers.Items)
            {
                if (pers.Id == currId)
                {
                    lstBxPlayers.SelectedItem = pers;
                    return;
                }
            }
        }
    }
}