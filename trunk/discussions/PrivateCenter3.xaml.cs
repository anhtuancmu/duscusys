using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Objects;
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
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for PrivateCenter.xaml
    /// </summary>
    public partial class PrivateCenter3 : SurfaceWindow
    {
        Discussions.Main.OnDiscFrmClosing _closing;
        ObservableCollection<Topic> _topicsOfDiscussion = new ObservableCollection<Topic>();
        public ObservableCollection<Topic> TopicsOfDiscussion
        {
            get
            {
                return _topicsOfDiscussion;
            }
            set
            {
                _topicsOfDiscussion = value;
            }
        }

        ObservableCollection<ArgPoint> _ownArgPoints = new ObservableCollection<ArgPoint>();
        public ObservableCollection<ArgPoint> OwnArgPoints
        {
            get
            {
                return _ownArgPoints;
            }
            set
            {
                _ownArgPoints = value;
            }
        }

        ObservableCollection<Person> _otherUsers = new ObservableCollection<Person>();
        public ObservableCollection<Person> OtherUsers
        {
            get
            {
                return _otherUsers;
            }
            set
            {
                _otherUsers = value;
            }
        }
        
        ObservableCollection<ArgPoint> _argPointsOfOtherUser = new ObservableCollection<ArgPoint>();
        public ObservableCollection<ArgPoint> ArgPointsOfOtherUser
        {
            get
            {
                return _argPointsOfOtherUser;
            }
            set
            {
                _argPointsOfOtherUser = value;
            }
        }

        UISharedRTClient _sharedClient;
        ContactTimer dragTapRecognizer = new ContactTimer(null);

        bool initializing;


        class PointRemoveRecord
        {
            public ArgPoint point;
            public Topic  topic;
            public Person person;

            public void Restore()
            {
                point.Person = person;
                point.Topic = topic;
            }
        } 
        
        PointRemoveRecord recentlyDeleted = null;

        public PrivateCenter3(UISharedRTClient sharedClient, Discussions.Main.OnDiscFrmClosing closing)
        {
            InitializeComponent();

            _closing = closing;
            _sharedClient = sharedClient;

            theBadge.Hide();

            Ctx2.DropContext();

            SetListeners(true);

            Title = string.Format("{0} on {1} - private dashboard", 
                                   SessionInfo.Get().person.Name, 
                                   SessionInfo.Get().discussion.Subject);

            lstTopics.ItemsSource = TopicsOfDiscussion;
            lstPoints.ItemsSource = OwnArgPoints;          
            lstOtherUsers.ItemsSource = OtherUsers;
            lstBadgesOfOtherUser.ItemsSource = ArgPointsOfOtherUser;

            lblWelcome.Content = SessionInfo.Get().person.Name;

            initializing = true;
            DiscussionSelectionChanged();
            initializing = false;  
        }

        Discussion selectedDiscussion()
        {
            int currentDiscId = SessionInfo.Get().discussion.Id;
            return Ctx2.Get().Discussion.FirstOrDefault(d0 => d0.Id == currentDiscId);
        }

        void SetListeners(bool doSet)
        {
            //if (doSet)
            //    _sharedClient.clienRt.onStructChanged += onStructChanged;
            //else
            //    _sharedClient.clienRt.onStructChanged -= onStructChanged;
        }

        void ForgetDBDiscussionState()
        {
            //forget cached state
            Ctx2.DropContext();
            //////////////////////
        }

        void UpdateTopicsOfDiscussion(Discussion d)
        {
            TopicsOfDiscussion.Clear();

            if (d == null)
                return;

            int selfId = SessionInfo.Get().person.Id;
            var topicsOfDiscussion = d.Topic;

            foreach (Topic t in topicsOfDiscussion)
            {
                if(t.Person.Any(p0=>p0.Id==selfId))
                    TopicsOfDiscussion.Add(t);
            }

            if (TopicsOfDiscussion.Count > 0)
                lstTopics.SelectedIndex = 0;
        }

        void UpdatePointsOfTopic(Topic t)
        {
            OwnArgPoints.Clear();
         
            if (t == null)
            {
                theBadge.DataContext = null;
                return;
            }

            int selfId = SessionInfo.Get().person.Id;
            foreach (var ap in t.ArgPoint.Where(ap0=>ap0.Person.Id==selfId).OrderBy(ap0=>ap0.OrderNumber))                                    
                OwnArgPoints.Add(ap);

            if (OwnArgPoints.Count > 0)
            {
                lstPoints.SelectedItem = OwnArgPoints.First();
                theBadge.DataContext = OwnArgPoints.First();
            }
            else
                theBadge.DataContext = null;
        }

        private void SurfaceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetListeners(false);
            
            if (_closing != null)
                _closing();
        }

        bool otherUserSelectedManually = true; 
        void UpdateOtherUsers(int discussionId, int ownId)
        {
            OtherUsers.Clear();
            
            var q = from p in Ctx2.Get().Person
                    where p.Topic.Any(t0 => t0.Discussion.Id == discussionId) &&
                          p.Id != ownId
                    select p;
            
            foreach (var p in q)
                OtherUsers.Add(p);

            if (OtherUsers.Count > 0)
            {
                otherUserSelectedManually = false;
                lstOtherUsers.SelectedIndex = 0;
                otherUserSelectedManually = true;
            }
        }

        private void lstUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pers = lstOtherUsers.SelectedItem as Person;
            var topic = lstTopics.SelectedItem as Topic;
            //if(!initializing)
            //    theBadge.DataContext = null;
            if (pers != null && topic != null)
            {
                UpdateBadgesOfOtherUser(pers.Id, topic.Id);                    
            }
        }

        void UpdateBadgesOfOtherUser(int personId, int topicId)
        {
            ArgPointsOfOtherUser.Clear();

            var q = from ap in Ctx2.Get().ArgPoint
                    where ap.Person.Id == personId &&
                          ap.Topic.Id == topicId
                    select ap;

            foreach (var ap in q)
                ArgPointsOfOtherUser.Add(ap);

            if (otherUserSelectedManually && ArgPointsOfOtherUser.Count > 0)
                theBadge.DataContext = ArgPointsOfOtherUser.First();
        }

        private void lstOtherUserBadges_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lstPoints.SelectedItem = null; 
            selectBigBadge(lstBadgesOfOtherUser.SelectedItem as ArgPoint);    
        }

        private void lstTopics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OwnArgPoints.Clear();
            theBadge.DataContext = null;
            Topic st = lstTopics.SelectedItem as Topic;

            SessionInfo.Get().currentTopicId = (st == null) ? -1 : st.Id;

            if (st == null)
                return;

            UpdateOtherUsers(st.Discussion.Id,SessionInfo.Get().person.Id);

            //update points of other users
            var otherPers = lstOtherUsers.SelectedItem as Person;
            if(otherPers!=null)
                UpdateBadgesOfOtherUser(otherPers.Id,st.Id);

            UpdatePointsOfTopic(st);
        }

        void DiscussionSelectionChanged()
        {
            OwnArgPoints.Clear();
            TopicsOfDiscussion.Clear(); 
            if(!initializing)
                theBadge.DataContext = null;

            var dis = selectedDiscussion();
            if (dis == null)
                return;

            //badges of other users
            ArgPointsOfOtherUser.Clear();

            UpdateTopicsOfDiscussion(dis);

            UpdateOtherUsers(dis.Id, SessionInfo.Get().person.Id);
        }

        private void lstPoints_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           // lstBadgesOfOtherUser.SelectedItem = null;
            lstPoints.ScrollIntoView(lstPoints.SelectedItem as ArgPoint);  
            selectBigBadge(lstPoints.SelectedItem as ArgPoint);          
        }

        void selectBigBadge(ArgPoint ap)
        {   
            if (ap == null)
                return;

            theBadge.EditingMode = ap.Person.Id == SessionInfo.Get().person.Id;          
            theBadge.DataContext = ap;
        }

        void getPointAndTopic(out ArgPoint ap, out Topic t)
        {
            t = null;
            ap = null;

            t = lstTopics.SelectedItem as Topic;
            if (t == null)
                return;

            ap = theBadge.DataContext as ArgPoint;
            if (ap == null)
                return;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnAddPoint_Click(object sender, RoutedEventArgs e)
        {
            Topic t = lstTopics.SelectedItem as Topic;
            if (t == null)
                return;

            BusyWndSingleton.Show("Creating new argument...");
            try
            {
                int orderNumber = OwnArgPoints.Count() > 0 ? OwnArgPoints.Last().OrderNumber + 1 : 0;                    

                var np = DaoUtils.NewPoint(lstTopics.SelectedItem as Topic, orderNumber);
                if (np != null)
                {
                    theBadge.DataContext = np;
                    t.ArgPoint.Add(np);
                }
                UpdatePointsOfTopic(lstTopics.SelectedItem as Topic);
                lstPoints.SelectedItem = np;

                saveProcedure(null, -1);
            }
            finally
            {
                BusyWndSingleton.Hide();
            }
        }

        DateTime _recentRemovalStamp = DateTime.Now;
        private void btnRemovePoint_Click(object sender, RoutedEventArgs e)
        {
            if(DateTime.Now.Subtract(_recentRemovalStamp).TotalSeconds < 2.0)
                return;
            _recentRemovalStamp = DateTime.Now;

            ArgPoint ap;
            Topic t;
            getPointAndTopic(out ap, out t);
            if (ap == null)
                return;
            if (ap.Person.Id != SessionInfo.Get().person.Id)
                return;

            BusyWndSingleton.Show("Removing argument...");
            try
            {
                if (ap.Topic != null)
                {
                    Topic t1 = ap.Topic;

                    recentlyDeleted = new PointRemoveRecord();
                    recentlyDeleted.person = ap.Person;
                    recentlyDeleted.point = ap;
                    recentlyDeleted.topic = ap.Topic;

                    CtxSingleton.DropContext();
                    DaoUtils.UnattachPoint(ap);

                    RenumberPointsAfterDeletion(t1, SessionInfo.Get().person.Id);

                    UpdatePointsOfTopic(t1);

                    saveProcedure(ap, t1.Id);
                }                
            }
            finally
            {
                BusyWndSingleton.Hide();
            }
        }

        void RenumberPointsAfterDeletion(Topic t, int selfId)
        {
            //points follow in order of OrderNumber
            int nextOrderNr = 0;
            foreach (var ap in t.ArgPoint.Where(ap0=>ap0.Person.Id==selfId).OrderBy(ap0=>ap0.OrderNumber))
            {
                ap.OrderNumber = nextOrderNr++;                               
            }
        }    

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            BusyWndSingleton.Show("Saving argument, please wait...");
            try
            {
                Topic t;
                ArgPoint editedPoint;
                getPointAndTopic(out editedPoint, out t);
                if (t == null || editedPoint == null)
                    return;

                if (!t.ArgPoint.Contains(editedPoint))
                    t.ArgPoint.Add(editedPoint);

                if (editedPoint.Description == null)
                    editedPoint.Description = new RichText();
                editedPoint.Description.Text = theBadge.plainDescription.Text;

                saveProcedure(null, -1);
            }
            finally
            {
                BusyWndSingleton.Hide();
            }
        }

        //we can only process one deleted point at a time
        void saveProcedure(ArgPoint deletedPt, int TopicOfDeletedPointId)
        {
            //extract modification lists
            List<ArgPoint> created = null;
            List<ArgPoint> edited = null;
            List<ArgPoint> deleted = null;
            GetChangeLists(out created, out edited, out deleted);

            //save changes 
            Ctx2.SaveChangesIgnoreConflicts();

            //first notify about deleted point!
            if (deletedPt!=null)
                _sharedClient.clienRt.SendArgPointDeleted(deletedPt.Id, TopicOfDeletedPointId);   

            //notify about changes
            foreach (var createdPt in created)
                _sharedClient.clienRt.SendArgPointCreated(createdPt.Id, createdPt.Topic.Id);

            foreach (var editedPt in edited)
                _sharedClient.clienRt.SendArgPointChanged(editedPt.Id, editedPt.Topic.Id);      
        }

        //returns lists of IDs of those of own points, which have been changed (added, removed, edited)
        void GetChangeLists(out List<ArgPoint> created, out List<ArgPoint> edited, out List<ArgPoint> deleted)
        {
            created = new List<ArgPoint>();
            edited = new List<ArgPoint>();
            deleted = new List<ArgPoint>();
            
            var ctx = Ctx2.Get();
            foreach (var ap in OwnArgPoints)
            {
                //if point is unnatached (removed in UI but preserved for UNDO), 
                //for this method it's removed
                if (ap.Person == null || ap.Topic == null)
                {
                    deleted.Add(ap);
                    continue;
                }
    
                switch (ctx.ObjectStateManager.GetObjectStateEntry(ap).State)
                {
                    case EntityState.Added:
                        created.Add(ap);
                        break;
                    case EntityState.Deleted:
                        deleted.Add(ap);
                        break;
                    case EntityState.Modified:
                        edited.Add(ap);
                        break;
                } 
            }
            
            foreach (var ap in ArgPointsOfOtherUser)
            {
                //if point is unnatached (removed in UI but preserved for UNDO), 
                //for this method it's removed
                if (ap.Person == null || ap.Topic == null)
                {
                    deleted.Add(ap);
                    continue;
                }

                switch (ctx.ObjectStateManager.GetObjectStateEntry(ap).State)
                {
                    case EntityState.Added:
                        created.Add(ap);
                        break;
                    case EntityState.Deleted:
                        deleted.Add(ap);
                        break;
                    case EntityState.Modified:
                        edited.Add(ap);
                        break;
                }
            }
        }

        void onStructChanged(int activeTopic, int initiaterId, DeviceType devType)
        {
            if (initiaterId == SessionInfo.Get().person.Id && devType == DeviceType.Wpf)
                return;

            BusyWndSingleton.Show("Fetching changes...");
            try
            { 
                //save selected topic 
                int topicUnderSelectionId = -1;
                Topic sel = lstTopics.SelectedItem as Topic;
                if (sel != null)
                    topicUnderSelectionId = sel.Id;
               
                //save selected list of points 
                var selectedAp = theBadge.DataContext as ArgPoint;
 
                ForgetDBDiscussionState();
                DiscussionSelectionChanged();

                //select previously selected topic
                if (topicUnderSelectionId != -1)
                    lstTopics.SelectedItem = Ctx2.Get().Topic.FirstOrDefault(t0 => t0.Id == topicUnderSelectionId); 
               
                //select previously selected point
                if (selectedAp != null)
                {
                    //own list
                    if (selectedAp.Person.Id == SessionInfo.Get().person.Id)
                    {
                        lstPoints.SelectedItem = null;
                        lstPoints.SelectedItem = OwnArgPoints.FirstOrDefault(ap0 => ap0.Id == selectedAp.Id);
                    }
                    else
                    {
                        lstOtherUsers.SelectedItem = OtherUsers.FirstOrDefault(u0 => u0.Id == selectedAp.Person.Id);
                        lstBadgesOfOtherUser.SelectedItem = ArgPointsOfOtherUser.FirstOrDefault(ap0 => ap0.Id == selectedAp.Id);
                    }
                }
            }
            finally
            {
                BusyWndSingleton.Hide();
            }
        }

        private void btnDiscInfo_Click(object sender, RoutedEventArgs e)
        {
            Discussion d = selectedDiscussion();
            if (d == null)
                return;

            var diz = new DiscussionInfoZoom(d);
            diz.ShowDialog();            
        }

        private void btnDiscu_Click(object sender, RoutedEventArgs e)
        {
            var wnd = DiscWindows.Get();
            if (wnd.discDashboard != null)
            {
                wnd.discDashboard.Activate();
                return;
            }

            wnd.discDashboard = new PublicCenter(_sharedClient, () => { wnd.discDashboard = null; }, -1, -1);
            wnd.discDashboard.Show();

           // Close();
        }

        //publish/unpublish                        
        private void SurfaceCheckBox_Click(object sender, RoutedEventArgs e)
        {            
            ArgPoint ap = (((SurfaceCheckBox)sender).DataContext) as ArgPoint;
            if(ap==null)
                return;

            Topic t;
            ArgPoint ap1;
            getPointAndTopic(out ap1, out t);
            if (t == null)
                return;

            if (((SurfaceCheckBox)sender).IsChecked.Value)
                ap.SharedToPublic = true;
            else
            {
                if (Ctx2.Get().ObjectStateManager.GetObjectStateEntry(ap).State == EntityState.Modified ||
                    Ctx2.Get().ObjectStateManager.GetObjectStateEntry(ap).State == EntityState.Unchanged)
                {
                    Ctx2.Get().Refresh(RefreshMode.StoreWins, ap);
                    DaoUtils.UnpublishPoint(ap);
                }
            }

            saveProcedure(null, -1);            
        }

        private void btnCopy_Click_1(object sender, RoutedEventArgs e)
        {
            var selectedTopic = lstTopics.SelectedItem as Topic;               
            var dlg = new CopyDlg(_sharedClient, selectedTopic!=null ? selectedTopic.Id : -1);
            dlg.ShowDialog();
            if (dlg.operationPerformed)
                onStructChanged(-1,-1,DeviceType.Wpf);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            onStructChanged(-1,-1,DeviceType.Wpf);               
        }

        private void SurfaceWindow_KeyDown_1(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Z && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                TryUndo();

            if (e.Key == Key.S && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                btnSave_Click(null, null);
        }

        //returns back recently deleted point (own), if there is one and its topic still exists
        void TryUndo()
        {
            if (recentlyDeleted == null)
            {
                Console.Beep();
                return;
            }

            var np = DaoUtils.clonePoint(Ctx2.Get(), 
                                         recentlyDeleted.point, 
                                         recentlyDeleted.topic,
                                         recentlyDeleted.person, 
                                         recentlyDeleted.point.Point);
            DaoUtils.DeleteArgPoint(Ctx2.Get(), recentlyDeleted.point);
            recentlyDeleted = null;
            if (np == null)
            {
                return;
            }

            lstTopics.SelectedItem = null;  
            lstTopics.SelectedItem = np.Topic;
            lstPoints.SelectedItem = np;

            saveProcedure(null, -1);
        }

        private void btnHome_Click_1(object sender, RoutedEventArgs e)
        {
            DiscWindows.Get().mainWnd.Activate();
            //this.Close();
        }
    }
}
