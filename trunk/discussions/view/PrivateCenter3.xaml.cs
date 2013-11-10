using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Discussions.ctx;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Discussions.RTModel.Model;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for PrivateCenter.xaml
    /// </summary>
    public partial class PrivateCenter3 : SurfaceWindow
    {
        private readonly Main.OnDiscFrmClosing _closing;

        private ObservableCollection<Topic> _topicsOfDiscussion = new ObservableCollection<Topic>();
        public ObservableCollection<Topic> TopicsOfDiscussion
        {
            get { return _topicsOfDiscussion; }
            set { _topicsOfDiscussion = value; }
        }

        private ObservableCollection<ArgPointExt> _ownArgPoints = new ObservableCollection<ArgPointExt>();
        public ObservableCollection<ArgPointExt> OwnArgPoints
        {
            get { return _ownArgPoints; }
            set { _ownArgPoints = value; }
        }

        private ObservableCollection<PersonExt> _otherUsers = new ObservableCollection<PersonExt>();
        public ObservableCollection<PersonExt> OtherUsers
        {
            get { return _otherUsers; }
            set { _otherUsers = value; }
        }

        private ObservableCollection<ArgPointExt> _argPointsOfOtherUser = new ObservableCollection<ArgPointExt>();
        public ObservableCollection<ArgPointExt> ArgPointsOfOtherUser
        {
            get { return _argPointsOfOtherUser; }
            set { _argPointsOfOtherUser = value; }
        }

        private UISharedRTClient _sharedClient;

        private readonly bool initializing;

        private class PointRemoveRecord
        {
            public ArgPoint point;
            public Topic topic;
            public Person person;

            public void Restore()
            {
                point.Person = person;
                point.Topic = topic;
            }
        }

        private PointRemoveRecord recentlyDeleted = null;

        public PrivateCenter3(UISharedRTClient sharedClient, Main.OnDiscFrmClosing closing)
        {
            InitializeComponent();

            _closing = closing;
            _sharedClient = sharedClient;

            theBadge.Hide();

            PrivateCenterCtx.DropContext();
            TimingCtx.Drop();           

            SetListeners(true);

            Title = string.Format("{0} on {1} - private dashboard",
                                  SessionInfo.Get().person.Name,
                                  SessionInfo.Get().discussion.Subject);

            lstTopics.ItemsSource = TopicsOfDiscussion;
            lstPoints.ItemsSource = OwnArgPoints;
            lstOtherUsers.ItemsSource = OtherUsers;
            lstBadgesOfOtherUser.ItemsSource = ArgPointsOfOtherUser;

            lblWelcome.Content = SessionInfo.Get().person.Name;

            _commentDismissalRecognizer = new CommentDismissalRecognizer(bigBadgeScroller, OnDismiss);
            theBadge.CommentDismissalRecognizer = _commentDismissalRecognizer;

            initializing = true;
            DiscussionSelectionChanged();
            initializing = false;
        }

        private Discussion selectedDiscussion()
        {
            int currentDiscId = SessionInfo.Get().discussion.Id;
            return PrivateCenterCtx.Get().Discussion.FirstOrDefault(d0 => d0.Id == currentDiscId);
        }

        private void SetListeners(bool doSet)
        {
            //if (doSet)
            //    _sharedClient.clienRt.onStructChanged += onStructChanged;
            //else
            //    _sharedClient.clienRt.onStructChanged -= onStructChanged;

            if (doSet)
                _sharedClient.clienRt.onCommentRead += OnCommentRead;
            else
                _sharedClient.clienRt.onCommentRead -= OnCommentRead;

            if (doSet)
                _sharedClient.clienRt.argPointChanged += ArgPointChanged;
            else
                _sharedClient.clienRt.argPointChanged -= ArgPointChanged;
        }

        private void ForgetDBDiscussionState()
        {
            //forget cached state
            PrivateCenterCtx.DropContext();
            TimingCtx.Drop();
            //////////////////////
        }

        private void UpdateTopicsOfDiscussion(Discussion d)
        {
            TopicsOfDiscussion.Clear();

            if (d == null)
                return;

            int selfId = SessionInfo.Get().person.Id;
            var topicsOfDiscussion = d.Topic;

            foreach (Topic t in topicsOfDiscussion)
            {
                if (t.Person.Any(p0 => p0.Id == selfId))
                    TopicsOfDiscussion.Add(t);
            }

            if (TopicsOfDiscussion.Count > 0)
                lstTopics.SelectedIndex = 0;
        }

        private void UpdatePointsOfTopic(Topic t)
        {
            OwnArgPoints.Clear();

            if (t == null)
            {
                theBadge.DataContext = null;
                return;
            }

            int selfId = SessionInfo.Get().person.Id;
            foreach (var ap in t.ArgPoint.Where(ap0 => ap0.Person.Id == selfId).OrderBy(ap0 => ap0.OrderNumber))
                OwnArgPoints.Add(new ArgPointExt(ap));

            UpdateLocalUnreadCountsOwn(TimingCtx.GetFresh());

            if (OwnArgPoints.Count > 0)
            {
                lstPoints.SelectedItem = OwnArgPoints.First();
                theBadge.DataContext = OwnArgPoints.First().Ap;
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

        private bool _otherUserSelectedManually = true;
               
        private void UpdateOtherUsers(int discussionId, int ownId)
        {
            OtherUsers.Clear();

            var q = from p in PrivateCenterCtx.Get().Person
                    where p.Topic.Any(t0 => t0.Discussion.Id == discussionId) &&
                          p.Id != ownId
                    select p;

            foreach (var p in q)
                OtherUsers.Add(new PersonExt(p));

            if (OtherUsers.Count > 0)
            {
                _otherUserSelectedManually = false;
                lstOtherUsers.SelectedIndex = 0;
                _otherUserSelectedManually = true;

                UpdateOtherUsersDots(PrivateCenterCtx.Get());
            }
        }

        private void lstUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pers = lstOtherUsers.SelectedItem as PersonExt;
            var topic = lstTopics.SelectedItem as Topic;
            //if(!initializing)
            //    theBadge.DataContext = null;
            if (pers != null && topic != null)
            {
                UpdateBadgesOfOtherUser(pers.Pers.Id, topic.Id);
            }
        }

        private void UpdateBadgesOfOtherUser(int personId, int topicId)
        {
            ArgPointsOfOtherUser.Clear();

            var q = from ap in PrivateCenterCtx.Get().ArgPoint
                    where ap.Person.Id == personId &&
                          ap.Topic.Id == topicId
                    select ap;

            foreach (var ap in q)
                ArgPointsOfOtherUser.Add(new ArgPointExt(ap)); 

            if (_otherUserSelectedManually && ArgPointsOfOtherUser.Count > 0)
                theBadge.DataContext = ArgPointsOfOtherUser.First().Ap;

            UpdateLocalUnreadCountsOfOtherUser(TimingCtx.GetFresh());
        }

        private void lstOtherUserBadges_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstBadgesOfOtherUser.SelectedItem != null)
                lstPoints.SelectedItem = null;
      
            selectBigBadge(lstBadgesOfOtherUser.SelectedItem as ArgPointExt);
        }

        void triggerBadgesOfOtherUserSelectionChanged()
        {
            var selected = lstBadgesOfOtherUser.SelectedItem;
            lstBadgesOfOtherUser.SelectedItem = null;
            lstBadgesOfOtherUser.SelectedItem = selected;
        }

        private void LstBadgesOfOtherUser_OnTouchDown(object sender, TouchEventArgs e)
        {
            triggerBadgesOfOtherUserSelectionChanged();
        }

        private void LstBadgesOfOtherUser_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            triggerBadgesOfOtherUserSelectionChanged();
        }

        private void lstTopics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OwnArgPoints.Clear();
            theBadge.DataContext = null;
            Topic st = lstTopics.SelectedItem as Topic;

            SessionInfo.Get().currentTopicId = (st == null) ? -1 : st.Id;

            if (st == null)
                return;

            UpdateOtherUsers(st.Discussion.Id, SessionInfo.Get().person.Id);

            //update points of other users
            var otherPers = lstOtherUsers.SelectedItem as PersonExt;
            if (otherPers != null)
                UpdateBadgesOfOtherUser(otherPers.Pers.Id, st.Id);

            UpdatePointsOfTopic(st);            
        }

        private void DiscussionSelectionChanged()
        {
            OwnArgPoints.Clear();
            TopicsOfDiscussion.Clear();
            if (!initializing)
                theBadge.DataContext = null;

            var dis = selectedDiscussion();
            if (dis == null)
                return;

            //badges of other users
            ArgPointsOfOtherUser.Clear();

            UpdateTopicsOfDiscussion(dis);

            UpdateOtherUsers(dis.Id, SessionInfo.Get().person.Id);
        }


        private void LstPoints_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            triggerPointsSelectionChanged();
        }

        private void LstPoints_OnPreviewTouchDown(object sender, TouchEventArgs e)
        {
            triggerPointsSelectionChanged();
        }

        void triggerPointsSelectionChanged()
        {
            var selected = lstPoints.SelectedItem;
            lstPoints.SelectedItem = null;
            lstPoints.SelectedItem = selected;
        }

        private void lstPoints_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lstPoints.SelectedItem!=null)
                lstBadgesOfOtherUser.SelectedItem = null;

            var apExt = (ArgPointExt) lstPoints.SelectedItem;
            if (lstPoints.SelectedItem != null)
                lstPoints.ScrollIntoView(lstPoints.SelectedItem);
            selectBigBadge(apExt);
        }

        private void selectBigBadge(ArgPointExt ap)
        {
            if (ap == null)
                return;

            theBadge.EditingMode = ap.Ap.Person.Id == SessionInfo.Get().person.Id;
            theBadge.DataContext = ap.Ap;
        }

        private void getPointAndTopic(out ArgPoint ap, out Topic t)
        {
            t = null;
            ap = null;

            t = lstTopics.SelectedItem as Topic;
            if (t == null)
                return;

            ap = theBadge.DataContext as ArgPoint;
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
                int orderNumber = OwnArgPoints.Any() ? OwnArgPoints.Last().Ap.OrderNumber + 1 : 1;

                var np = DaoUtils.NewPoint(lstTopics.SelectedItem as Topic, orderNumber);
                if (np != null)
                {
                    theBadge.DataContext = np;
                    t.ArgPoint.Add(np);
                }
                UpdatePointsOfTopic(lstTopics.SelectedItem as Topic);
               
                ArgPointExt newArgPointExt = OwnArgPoints.FirstOrDefault(i => i.Ap == np);

                lstPoints.SelectedItem = newArgPointExt;

                saveProcedure(null, -1);
            }
            finally
            {
                BusyWndSingleton.Hide();
            }
        }

        private DateTime _recentRemovalStamp = DateTime.Now;

        private void btnRemovePoint_Click(object sender, RoutedEventArgs e)
        {
            if (DateTime.Now.Subtract(_recentRemovalStamp).TotalSeconds < 2.0)
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

                    PublicBoardCtx.DropContext();
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

        private void RenumberPointsAfterDeletion(Topic t, int selfId)
        {
            //points follow in order of OrderNumber
            int nextOrderNr = 1;
            foreach (var ap in t.ArgPoint.Where(ap0 => ap0.Person.Id == selfId).OrderBy(ap0 => ap0.OrderNumber))
            {
                ap.OrderNumber = nextOrderNr++;
            }
        }

        public void btnSave_Click(object sender, RoutedEventArgs e)
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
        private void saveProcedure(ArgPoint deletedPt, int TopicOfDeletedPointId)
        {
            //extract modification lists
            List<ArgPoint> created = null;
            List<ArgPoint> edited = null;
            List<ArgPoint> deleted = null;
            GetChangeLists(out created, out edited, out deleted);

            //save changes 
            PrivateCenterCtx.SaveChangesIgnoreConflicts();

            //first notify about deleted point!
            if (deletedPt != null)
                _sharedClient.clienRt.SendArgPointDeleted(deletedPt.Id, TopicOfDeletedPointId);

            //notify about changes
            foreach (var createdPt in created)
                _sharedClient.clienRt.SendArgPointCreated(createdPt.Id, createdPt.Topic.Id);

            foreach (var editedPt in edited)
                _sharedClient.clienRt.SendArgPointChanged(editedPt.Id, editedPt.Topic.Id);
        }

        //returns lists of IDs of those of own points, which have been changed (added, removed, edited)
        private void GetChangeLists(out List<ArgPoint> created, out List<ArgPoint> edited, out List<ArgPoint> deleted)
        {
            created = new List<ArgPoint>();
            edited  = new List<ArgPoint>();
            deleted = new List<ArgPoint>();

            var ctx = PrivateCenterCtx.Get();
            foreach (var ap in OwnArgPoints)
            {
                //if point is unnatached (removed in UI but preserved for UNDO), 
                //for this method it's removed
                if (ap.Ap.Person == null || ap.Ap.Topic == null)
                {
                    deleted.Add(ap.Ap);
                    continue;
                }

                switch (ctx.ObjectStateManager.GetObjectStateEntry(ap.Ap).State)
                {
                    case EntityState.Added:
                        created.Add(ap.Ap);
                        break;
                    case EntityState.Deleted:
                        deleted.Add(ap.Ap);
                        break;
                    case EntityState.Modified:
                        edited.Add(ap.Ap);
                        break;
                }
            }

            foreach (var ap in ArgPointsOfOtherUser)
            {
                //if point is unnatached (removed in UI but preserved for UNDO), 
                //for this method it's removed
                if (ap.Ap.Person == null || ap.Ap.Topic == null)
                {
                    deleted.Add(ap.Ap);
                    continue;
                }

                switch (ctx.ObjectStateManager.GetObjectStateEntry(ap.Ap).State)
                {
                    case EntityState.Added:
                        created.Add(ap.Ap);
                        break;
                    case EntityState.Deleted:
                        deleted.Add(ap.Ap);
                        break;
                    case EntityState.Modified:
                        edited.Add(ap.Ap);
                        break;
                }
            }
        }

        private void onStructChanged(int activeTopic, int initiaterId, DeviceType devType)
        {
            if (initiaterId == SessionInfo.Get().person.Id && devType == DeviceType.Wpf)
                return;

            BusyWndSingleton.Show("Fetching changes...");
            try
            {
                //save selected topic 
                int topicUnderSelectionId = -1;
                var sel = lstTopics.SelectedItem as Topic;
                if (sel != null)
                    topicUnderSelectionId = sel.Id;

                //save selected list of points 
                var selectedAp = theBadge.DataContext as ArgPoint;

                ForgetDBDiscussionState();
                DiscussionSelectionChanged();

                //select previously selected topic
                if (topicUnderSelectionId != -1)
                    lstTopics.SelectedItem = PrivateCenterCtx.Get().Topic.FirstOrDefault(t0 => t0.Id == topicUnderSelectionId);

                //select previously selected point
                if (selectedAp != null)
                {
                    //own list
                    if (selectedAp.Person.Id == SessionInfo.Get().person.Id)
                    {
                        lstPoints.SelectedItem = null;
                        lstPoints.SelectedItem = OwnArgPoints.FirstOrDefault(ap0 => ap0.Ap.Id == selectedAp.Id);
                    }
                    else
                    {
                        lstOtherUsers.SelectedItem = OtherUsers.FirstOrDefault(u0 => u0.Pers.Id == selectedAp.Person.Id);
                        
                        lstBadgesOfOtherUser.SelectedItem =
                            ArgPointsOfOtherUser.FirstOrDefault(ap0 => ap0.Ap.Id == selectedAp.Id);
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
            ArgPoint ap = (((SurfaceCheckBox) sender).DataContext) as ArgPoint;
            if (ap == null)
                return;

            Topic t;
            ArgPoint ap1;
            getPointAndTopic(out ap1, out t);
            if (t == null)
                return;

            if (((SurfaceCheckBox) sender).IsChecked.Value)
                ap.SharedToPublic = true;
            else
            {
                if (PrivateCenterCtx.Get().ObjectStateManager.GetObjectStateEntry(ap).State == EntityState.Modified ||
                    PrivateCenterCtx.Get().ObjectStateManager.GetObjectStateEntry(ap).State == EntityState.Unchanged)
                {
                    PrivateCenterCtx.Get().Refresh(RefreshMode.StoreWins, ap);
                    DaoUtils.UnpublishPoint(ap);
                }
            }

            saveProcedure(null, -1);
        }

        private void btnCopy_Click_1(object sender, RoutedEventArgs e)
        {
            var selectedTopic = lstTopics.SelectedItem as Topic;
            var dlg = new CopyDlg(_sharedClient, selectedTopic != null ? selectedTopic.Id : -1);
            dlg.ShowDialog();
            if (dlg.operationPerformed)
                onStructChanged(-1, -1, DeviceType.Wpf);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            thereAreNewComments.Visibility = Visibility.Collapsed;            
            onStructChanged(-1, -1, DeviceType.Wpf);
        }

        private void SurfaceWindow_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Z && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                TryUndo();

            if (e.Key == Key.S && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                btnSave_Click(null, null);
        }

        //returns back recently deleted point (own), if there is one and its topic still exists
        private void TryUndo()
        {
            if (recentlyDeleted == null)
            {
                Console.Beep();
                return;
            }

            var np = DaoUtils.clonePoint(PrivateCenterCtx.Get(),
                                         recentlyDeleted.point,
                                         recentlyDeleted.topic,
                                         recentlyDeleted.person,
                                         recentlyDeleted.point.Point);
            DaoUtils.DeleteArgPoint(PrivateCenterCtx.Get(), recentlyDeleted.point);
            recentlyDeleted = null;
            if (np == null)
            {
                return;
            }

            lstTopics.SelectedItem = null;
            lstTopics.SelectedItem = np.Topic;
            lstPoints.SelectedItem = new ArgPointExt(np);

            saveProcedure(null, -1);
        }

        private void btnHome_Click_1(object sender, RoutedEventArgs e)
        {
            DiscWindows.Get().mainWnd.Activate();
            //this.Close();
        }

        #region comment notifications

        private readonly CommentDismissalRecognizer _commentDismissalRecognizer;

        private void BigBadgeScroller_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _commentDismissalRecognizer.CheckScrollState();
        }
     
        void OnDismiss(ArgPoint ap)
        {
            //Console.Beep();

            CommentDismissalRecognizer.pushDismissal(ap, PrivateCenterCtx.Get());
        }

        private void OnCommentRead(CommentsReadEvent ev)
        {            
            theBadge.OnCommentRead(ev);

            var topic = lstTopics.SelectedItem as Topic;
            if (topic != null && topic.Id == ev.TopicId)
            {
                bool changedPointOwnedByOtherUser = false;
                var changedPointExt = OwnArgPoints.FirstOrDefault(ap => ap.Ap.Id == ev.ArgPointId);
                if (changedPointExt == null)
                {
                    changedPointExt = ArgPointsOfOtherUser.FirstOrDefault(ap => ap.Ap.Id == ev.ArgPointId);
                    changedPointOwnedByOtherUser = true;
                }

                TimingCtx.Drop();
                var ctx = TimingCtx.GetFresh();

                //if the changed point is our own point
                if (changedPointExt != null)
                {
                    changedPointExt.NumUnreadComments = DaoUtils.NumCommentsUnreadBy(
                        ctx,
                        changedPointExt.Ap.Id).Total();

                    if (changedPointOwnedByOtherUser)
                    {
                        UpdateOtherUsersDots(ctx, changedPointExt.Ap.Person.Id);
                    }
                    else
                    {
                        UpdateLocalUnreadCountsOwn(ctx);
                    }
                }
            }
        }

        private void ArgPointChanged(int argPointId, int topicId, PointChangedType change)
        {           
            if (change == PointChangedType.Modified)
            {
               //if a comment has been added by someone except us, update number of unread comments
               // onStructChanged(-1, -1, DeviceType.Wpf);

               //only show notification that there are new comments, not more. user will need to click Refresh
               var currTopic = lstTopics.SelectedItem as Topic;
               if (currTopic != null && topicId == currTopic.Id)
               {
                  TimingCtx.Drop();
                  if (DaoUtils.NumCommentsUnreadBy(TimingCtx.GetFresh(), argPointId).Total() > 0)
                      thereAreNewComments.Visibility = Visibility.Visible;                   
               }
            }
        }

        private void UpdateLocalUnreadCountsOwn(DiscCtx ctx)
        {            
            foreach(var ap in OwnArgPoints)
            {
                ap.NumUnreadComments = DaoUtils.NumCommentsUnreadBy(ctx, ap.Ap.Id).Total();
            }
        }

        private void UpdateLocalUnreadCountsOfOtherUser(DiscCtx ctx)
        {
            foreach (var ap in ArgPointsOfOtherUser)
            {
                ap.NumUnreadComments = DaoUtils.NumCommentsUnreadBy(ctx, ap.Ap.Id).Total();
            }
        }

        readonly DuplicateEventRecognizer _otherUsersDots = new DuplicateEventRecognizer(100);
        private void UpdateOtherUsersDots(DiscCtx ctx, int singleUserId = -1)
        {
            if (_otherUsersDots.IsDuplicate())
                return;
            _otherUsersDots.RecordEvent();

            var topic = lstTopics.SelectedItem as Topic;
            if (topic == null)
                return;

            List<int> usersWithUnreadComments;
            if (singleUserId == -1)
            {
                usersWithUnreadComments = DaoUtils.SubsetOfPersonsWithDots(ctx,
                                                                           OtherUsers.Select(u0 => u0.Pers.Id).ToArray(),
                                                                           topic.Id);
            }
            else
            {
                usersWithUnreadComments = DaoUtils.SubsetOfPersonsWithDots(ctx,
                                                                           new []{ singleUserId },
                                                                           topic.Id);
            }

            foreach (var otherUser in OtherUsers)
            {
                if (singleUserId != -1 && otherUser.Pers.Id != singleUserId)
                    continue;                

                otherUser.HasPointsWithUnreadComments =
                    usersWithUnreadComments != null &&
                    usersWithUnreadComments.Contains(otherUser.Pers.Id);
            }         
        }

        private void BtnStartEvents_OnClick(object sender, RoutedEventArgs e)
        {
            TestEventOrchestrater.Inst.Start(theBadge, this);
        }
        #endregion

    }
}