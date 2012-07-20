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
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation;
using Discussions.model;
using System.Windows.Ink;
using System.Diagnostics;
using System.Timers;
using DiscussionsClientRT;
using Discussions.RTModel;
using System.Windows.Threading;
using Discussions.RTModel.Model;
using Discussions.DbModel;
using Discussions.rt;
using System.IO;
using VectorEditor;
using System.Data.Objects;
using Discussions.VectorEditor;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for DiscussionDashboard.xaml
    /// </summary>
    public partial class DiscussionDashboard : SurfaceWindow
    {
        Discussion _discussion;
        public Discussion discussion { get { return _discussion; } }

        Topic selectedTopic = null;

        DragCursorState cursorState = null;
        UserCursor userCursor = null;
        
        ObservableCollection<ArgPoint> _allTopicsItems;
        public ObservableCollection<ArgPoint> allTopicsItems
        {
            get { 
                if(_allTopicsItems==null)
                    _allTopicsItems = new ObservableCollection<ArgPoint>();
                return _allTopicsItems; }
            set { _allTopicsItems = value; }
        }

        ObservableCollection<object> _unsolvedCurrentTopicItems = new ObservableCollection<object>();
        public ObservableCollection<object> unsolvedCurrentTopicItems
        {
            get {
                if (_unsolvedCurrentTopicItems == null)
                    _unsolvedCurrentTopicItems = new ObservableCollection<object>();
                return _unsolvedCurrentTopicItems; }
            set { _unsolvedCurrentTopicItems = value; }
        }

        ObservableCollection<object> _agreedAllTopicsItems = new ObservableCollection<object>();
        public ObservableCollection<object> agreedAllTopicsItems
        {
            get {
                if (_agreedAllTopicsItems == null)
                    _agreedAllTopicsItems = new ObservableCollection<object>();
                return _agreedAllTopicsItems; }
            set { _agreedAllTopicsItems = value; }
        }

        ObservableCollection<object> _disagreedAllTopicsItems = new ObservableCollection<object>();
        public ObservableCollection<object> disagreedAllTopicsItems
        {
            get {
                if (_disagreedAllTopicsItems==null)
                    _disagreedAllTopicsItems = new ObservableCollection<object>();
                return _disagreedAllTopicsItems; }
            set { _disagreedAllTopicsItems = value; }
        }

        BadgeFolder RecycleBin;
        BadgeFolder PrivIcon;

        ContactTimer unsolvedDragTapRecognizer   = new ContactTimer(null);
        ContactTimer agreedDragTapRecognizer     = new ContactTimer(null);
        ContactTimer disagreedDragTapRecognizer  = new ContactTimer(null);

        //items which changed geometry recently. 
        //they will be reported to server and reset during next timer tick
        List<ScatterViewItem> recentlyMovedItems = new List<ScatterViewItem>();

        //user id -> cursor UC
        Dictionary<int, UserCursorUC> userCursors = new Dictionary<int, UserCursorUC>();

        UISharedRTClient _sharedClient;

        Discussions.Main.OnDiscFrmClosing _closing;

        StatusWnd _stWnd;

        EditorWndCtx graphicsCtx = null;

        public DiscussionDashboard(UISharedRTClient sharedClient,
                                   StatusWnd stWnd,
                                   Discussions.Main.OnDiscFrmClosing closing)
        {
            this._discussion = SessionInfo.Get().discussion;
            _sharedClient = sharedClient;
            _closing = closing;
            _stWnd = stWnd;
            
            InitializeComponent();

            userCursor = new UserCursor(SessionInfo.Get().person.Name, CursorInputState.None);
            userCursor.usrId = SessionInfo.Get().person.Id;

            SetListeners(sharedClient, true);
            if(sharedClient.clienRt.IsConnected())
                OnJoin();

            ToLayerModeNoLayer();
        }  

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            DataContext = this;

            if (_discussion.Topic.Count > 0)
                selectedTopic = _discussion.Topic.First();

           InitCollections();

           cbxGroup.OnSelected += cbxGroup_SelectionChanged;

           UpdateTopics();
        }

        void UpdateTopics()
        {
            var selected = cbxGroup.SelectedItem as Topic;
            selectedTopic = _discussion.Topic.Count > 0 ? _discussion.Topic.First() : null;
           
            //restore seleced topic
            if(selected!=null)
            {
               Topic newSelected = null;
               newSelected = discussion.Topic.FirstOrDefault(t0=>t0.Id==selected.Id);
               cbxGroup.SetChoices(discussion.Topic, "Name",newSelected);
            }
            else
            {
                cbxGroup.SetChoices(discussion.Topic, "Name");
            }                                  
        }

        void InitCollections()
        {
            allTopicsItems.Clear();
            unsolvedCurrentTopicItems.Clear();
            agreedAllTopicsItems.Clear();
            disagreedAllTopicsItems.Clear();
            recentlyMovedItems.Clear();
            
            Dictionary<int,Group> groupCodeToAgreedGroups    = new Dictionary<int,Group>();
            Dictionary<int,Group> groupCodeToDisagreedGroups = new Dictionary<int,Group>();

            //fill agreed/disagreed points  
            foreach (Topic t in discussion.Topic)     
                foreach (ArgPoint p in t.ArgPoint)
                {
                    //this is public dashboard, we only know about publicly shared items
                    if (!p.SharedToPublic)
                        continue;
                    
                    allTopicsItems.Add(p);                                        

                    switch (p.AgreementCode)
                    {
                        case (int)AgreementCode.Agreed:
                            agreedAllTopicsItems.Add(p);
                            break;
                        case (int)AgreementCode.Disagreed:
                            disagreedAllTopicsItems.Add(p);
                            break;
                        case (int)AgreementCode.Unsolved:
                            if (p.Topic.Id == selectedTopic.Id)
                                unsolvedCurrentTopicItems.Add(p);
                            break;
                        case (int)AgreementCode.UnsolvedAndGrouped:            
                            break;
                        case (int)AgreementCode.AgreedAndGrouped:
                            if (p.Group!=null)
                                if (!groupCodeToAgreedGroups.ContainsKey(p.Group.Id))
                                    groupCodeToAgreedGroups.Add(p.Group.Id, p.Group);                                
                            break;
                        case (int)AgreementCode.DisagreedAndGrouped:
                            if (p.Group != null)
                                if (!groupCodeToDisagreedGroups.ContainsKey(p.Group.Id))
                                    groupCodeToDisagreedGroups.Add(p.Group.Id, p.Group);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }

           //create Agreed/Disagreed groups
           foreach (var group in groupCodeToAgreedGroups.Values)
               agreedAllTopicsItems.Add(CreateSmallContainer(GetBadgeFolder(group)));                              

           foreach (var group in groupCodeToDisagreedGroups.Values)               
               disagreedAllTopicsItems.Add(CreateSmallContainer(GetBadgeFolder(group)));

           HighlightOwnSmallBadges();
        }
       
        //when all ScatterViewItems are created, we block them from rotation
        //ScatterViewItems are created with delay, we have no event to hook it
        bool sviRotationBlocked = false;
        void BlockSVIRotation()
        {
            if (sviRotationBlocked)
                return;

            sviRotationBlocked = sviRotationBlocked || BlockSVIRotation(unsolved); 
            sviRotationBlocked = sviRotationBlocked || BlockSVIRotation(agreement);
            sviRotationBlocked = sviRotationBlocked || BlockSVIRotation(disagreement);
        }
        bool BlockSVIRotation(ScatterView sv)
        {
            if (sv == null)
                return false;

            Utils.EnumSVIs(sv, (ScatterViewItem svi)=>
            {
                svi.CanRotate = false;
                svi.Orientation = 0;
                svi.Deceleration = 10000000000;//we disable inertia as we don't keep track of objects moved by inertia 

                ColorAfterPerson(svi);

                svi.PreviewMouseLeftButtonDown += BadgeCanvasMouseDown;
                svi.PreviewTouchDown += BadgeCanvas_TouchDown;
            });

            return true;
        }

        bool HighlightOwnSmallBadges()
        {
            bool res = false;
            res |= HighlightOwnBadges(agreement);
            res |= HighlightOwnBadges(disagreement);
            return res;
        }
        bool HighlightOwnBadges(ScatterView sv)
        {
            if (sv == null)
                return false;

            Utils.EnumSVIs(sv, (ScatterViewItem svi) =>
            {
                 ColorAfterPerson(svi);
            });

            return true;
        }

        void ColorAfterPerson(ScatterViewItem svi)
        {            
            var ap = svi.DataContext as ArgPoint;
            if (ap == null)
                return;
            
            svi.Background = new SolidColorBrush( Utils.IntToColor(ap.Person.Color) );
        }

        void RestoreUnsolvedGroupsOfCurrentTopic(Topic t)
        {
            Dictionary<int, Group> groupCodeToUnsolvedGroups = new Dictionary<int, Group>();

            foreach (ArgPoint p in allTopicsItems)
            {
                if (p.AgreementCode== (int)AgreementCode.UnsolvedAndGrouped)
                {
                    if (p.Group != null)
                        if (!groupCodeToUnsolvedGroups.ContainsKey(p.Group.Id))
                            groupCodeToUnsolvedGroups.Add(p.Group.Id, p.Group); 
                }
            }

            foreach (var group in groupCodeToUnsolvedGroups.Values)
            {
                if (group.ArgPoint.Count > 0 && group.ArgPoint.First().Topic.Id == t.Id)                
                    putBadgeFolderTo(group, unsolvedCurrentTopicItems);
            }
        }

        void CreateSpecialIcons()
        {
            _CreateRecycleBin();
            _CreatePrivIconBin(); 
        }

        BadgeFolder makeSpecialBadgeFolder(string imgBrushName)
        {
            var res = new BadgeFolder();
            res.Width = 70;
            res.Height = 110;
            res.headerRect.Height = 0.1;
            res.headerText.Height = 0.1;
            res.argPointGroup.Width = res.Width;
            res.argPointGroup.Height = res.Height;
            res.headerText.Visibility = Visibility.Hidden;
            res.Background = (Brush)Application.Current.Resources[imgBrushName];
            res.scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            res.scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            return res;
        }

        void _CreateRecycleBin()
        {            
            //recreate the bin for every topic to avoid duplicate visual parents
            RecycleBin = makeSpecialBadgeFolder("RecBin"); 

            //we add the recycle bin to all topics  
            if (!unsolvedCurrentTopicItems.Contains(RecycleBin))
            {
                ScatterViewItem svi = GetSVI(RecycleBin, true);
                svi.CanMove = false;
                svi.CanRotate = false;                
                unsolvedCurrentTopicItems.Add(svi);
                svi.Center = new Point(RecycleBin.Width / 2, RecycleBin.Height / 2);
                svi.Orientation = 0;                
            }
        }

        void _CreatePrivIconBin()
        {
            //recreate the bin for every topic to avoid duplicate visual parents
            PrivIcon = makeSpecialBadgeFolder("PrivIcon"); 

            //we add the recycle bin to all topics  
            if (!unsolvedCurrentTopicItems.Contains(PrivIcon))
            {
                ScatterViewItem svi = GetSVI(PrivIcon, true);
                svi.CanMove = false;
                svi.CanRotate = false;
                unsolvedCurrentTopicItems.Add(svi);
                svi.Center = new Point(RecycleBin.Width + PrivIcon.Width / 2, PrivIcon.Height / 2);
                svi.Orientation = 0;
            }
        }

        BadgeFolder GetBadgeFolder(Group g)
        {
            BadgeFolder bf = new BadgeFolder();
            bf.model = g;
            bf.onAttemptToBeginDrag += OnAttemptToBeginDragFromBadgeFolder;
            bf.Width = 310;
            return bf;
        }

        void putBadgeFolderTo(Group g, ObservableCollection<object> c)
        {
            var bf = GetBadgeFolder(g);
            c.Add(GetSVI(bf, true));
        }

        void TargetChanged(object sender, TargetChangedEventArgs e)
        {
            ProcessTargetChanged(e.Cursor.Data, e.Cursor.CurrentTarget);
        }

        void ProcessTargetChanged(object DraggedContent, object DropTarget)
        {
            if(cursorState==null)
                return;            

            dragDropTooltip.Content = "";
            cursorState.SetOperation(DragCursorState.OperationType.None, null);

            if (DropTarget == null)
                return;

            if (DraggedContent is ArgPoint)
            {
                ArgPoint ap = (ArgPoint)DraggedContent;
                if (DropTarget is ScatterView)
                {
                    ScatterView sv = (ScatterView)DropTarget;
                    if (DropTarget != cursorState.DragSrc)
                    {
                        if (cursorState.DragSrc is ScatterView)
                        {
                            cursorState.SetOperation(DragCursorState.OperationType.ResolveAgreement, sv);
                            
                            string agreementZone = "";
                            if (sv == agreement)
                                agreementZone = "agreed";
                            else if (sv == disagreement)
                                agreementZone = "disagreed";
                            else if (sv == unsolved)
                                agreementZone = "unsolved";
                               
                            dragDropTooltip.Content = string.Format("Move [{0}] to [{1}]",
                                                                    ap.Point, agreementZone);                                                      
                        }
                        else if (cursorState.DragSrc is BadgeFolder && sv==unsolved)
                        {                            
                            dragDropTooltip.Content = string.Format("Move [{0}] from group", ap.Point);
                            cursorState.SetOperation(DragCursorState.OperationType.MoveFromGroup, sv);
                        }
                    }
                }
                else if (DropTarget is BadgeFolder)
                {
                    BadgeFolder bf = (BadgeFolder)DropTarget;                    
                    if (DropTarget != cursorState.DragSrc)
                    {
                        cursorState.SetOperation(DragCursorState.OperationType.MoveToGroup, bf);
                        
                        if(DropTarget==RecycleBin)
                            dragDropTooltip.Content = string.Format("Remove {0}",ap.Point);
                        else if (DropTarget == PrivIcon)
                            dragDropTooltip.Content = string.Format("Take {0} back to private", ap.Point);
                        else
                            dragDropTooltip.Content = string.Format("Move [{0}] to group", ap.Point);                                                
                    }
                }
                else if (DropTarget is Badge)
                {
                    Badge apuc = (Badge)DropTarget;
                    if (DropTarget != cursorState.DragSrc)
                    {
                        BadgeFolder bf = GetBadgeFolder(apuc);
                        if (bf != null)
                        {
                            dragDropTooltip.Content = string.Format("Move [{0}] to group",
                                                                    ap.Point);
                            cursorState.SetOperation(DragCursorState.OperationType.MoveToGroup, bf);
                        }
                        else if (cursorState.DragSrc == unsolved && 
                                (apuc.DataContext as ArgPoint).AgreementCode==(int)AgreementCode.Unsolved)
                        {   //merge operation is only possible over unsolved
                            
                            dragDropTooltip.Content = string.Format("Merge [{0}] with [{1}]",
                                                                    ap.Point,
                                                                    (apuc.DataContext as ArgPoint).Point);
                            
                            cursorState.SetOperation(DragCursorState.OperationType.MergeWith, 
                                                    (Badge)apuc);
                        }
                    }
                }
            }
            else if (DraggedContent is BadgeFolder &&  DropTarget != cursorState.DragSrc)
            {//badge folder is dragged

                if (DropTarget is ScatterView)
                {
                    ScatterView sv = DropTarget as ScatterView;
                                       
                    cursorState.SetOperation(DragCursorState.OperationType.ResolveAgreement,sv);
                    
                    string agreementZone = "";
                    if (sv == agreement)
                        agreementZone = "agreed";
                    else if (sv == disagreement)
                        agreementZone = "disagreed";
                    else if (sv == unsolved)
                        agreementZone = "unsolved";
                    
                    dragDropTooltip.Content = string.Format("Move group to [{0}]",agreementZone); 
                }
            }
        }

        BadgeFolder GetBadgeFolder(Badge apuc)
        {
            DependencyObject findSource = apuc;

            // Find the ScatterViewItem object that is being touched.
            while (findSource != null)
            {
                findSource = VisualTreeHelper.GetParent(findSource);
                if (findSource is BadgeFolder)
                    return (BadgeFolder)findSource;
            }

            return null;
        }
        
        void ProcessDropTarget(SurfaceDragCursor cursor, 
                               ObservableCollection<object> src, 
                               ObservableCollection<object> dst,
                               int NewResolutionCode)
        {
            if (cursorState == null)
                return;

            bool handled = false; 
            if (cursor.DragSource == cursor.CurrentTarget || src == null || dst==null)
            {
                DragCanceled();                
            }  
            else 
            {
                switch(cursorState.Operation)
                {
                    case DragCursorState.OperationType.None:
                        break;
                    case DragCursorState.OperationType.ResolveAgreement:
                        if (cursor.Data is ArgPoint)
                        {
                            ArgPoint ap = cursor.Data as ArgPoint;
                            if (src.Contains(ap))
                                src.Remove(ap);
                            dst.Add(ap);
                            ap.AgreementCode = NewResolutionCode;
                            handled = true;
                        }
                        else if (cursor.Data is BadgeFolder)
                        {
                            int ResolutionCode = -1;

                            if (cursor.CurrentTarget == unsolved)
                                ResolutionCode = (int)AgreementCode.UnsolvedAndGrouped;
                            else if(cursor.CurrentTarget == agreement)
                                ResolutionCode = (int)AgreementCode.AgreedAndGrouped;
                            else if(cursor.CurrentTarget == disagreement)
                                ResolutionCode = (int)AgreementCode.DisagreedAndGrouped;

                            if (ResolutionCode != -1)
                            {
                                BadgeFolder bf = cursor.Data as BadgeFolder;

                               // ScatterViewItem 
                                FrameworkElement itemContainer = (FrameworkElement)bf.Parent;
                                if (itemContainer != null && itemContainer.Parent is ScatterViewItem)
                                    itemContainer = (FrameworkElement)itemContainer.Parent;

                                if (itemContainer != null && itemContainer is ScatterViewItem)
                                {
                                    if (src.Contains(itemContainer))
                                    {
                                        src.Remove(itemContainer);
                                    }

                                    if (itemContainer is ContentControl)
                                    {
                                        if ((itemContainer as ContentControl).Content is Viewbox)
                                            ((itemContainer as ContentControl).Content as Viewbox).Child = null;

                                        (itemContainer as ContentControl).Content = null;
                                    }

                                    if(cursor.CurrentTarget==unsolved)
                                        dst.Add(CreateContainer(bf));
                                    else
                                        dst.Add(CreateSmallContainer(bf));

                                    foreach (ArgPoint ap in bf.model.ArgPoint)
                                        ap.AgreementCode = ResolutionCode;
                                    handled = true;
                                }
                            }
                        }
                        break;
                    case DragCursorState.OperationType.MoveToGroup:
                        if (cursorState.currentTarget is BadgeFolder && 
                            cursorState.draggedItem is ScatterViewItem &&
                            (cursorState.draggedItem as ScatterViewItem).Content is ArgPoint)
                        {
                            if(MoveToGroup((BadgeFolder)cursorState.currentTarget,
                                          (ArgPoint)(cursorState.draggedItem as ScatterViewItem).Content))
                                handled = true;
                        }
                        break;
                    case DragCursorState.OperationType.MoveFromGroup:
                        if (cursorState.DragSrc is BadgeFolder &&
                            cursorState.currentTarget == unsolved &&
                            (cursorState.draggedItem as ListBoxItem).Content is ArgPoint)
                        {
                            MoveFromGroup((BadgeFolder)cursorState.DragSrc, 
                                          (ArgPoint)(cursorState.draggedItem as ListBoxItem).Content);
                            handled = true;
                        }
                        break;
                    case DragCursorState.OperationType.MergeWith:
                        if ((cursorState.draggedItem as ScatterViewItem).Content is ArgPoint)
                        {
                            Badge target = (Badge)cursorState.currentTarget;
                            MergeTwoBadges((ArgPoint)target.DataContext, 
                                           (ArgPoint)(cursorState.draggedItem as ScatterViewItem).Content);
                            handled = true;
                        }
                        break;
                }
            }

            DragCanceled();

            if (handled)
            {
                CtxSingleton.SaveChangesIgnoreConflicts();
                
                //reset sviRotationBlocked to force-align new badges at next tick 
                sviRotationBlocked = false;

                HighlightOwnSmallBadges();

                _sharedClient.clienRt.SendNotifyStructureChanged(selectedTopic.Id);
            }

            dragDropTooltip.Content = "";
            cursorState = null;
        }

        ScatterViewItem CreateSmallContainer(BadgeFolder bf)
        {
            Viewbox vb = new Viewbox();
            vb.StretchDirection = StretchDirection.Both;
            vb.Stretch = Stretch.Uniform;
            vb.Child = bf;

            ScatterViewItem svi = GetSVI(vb, true);
            return svi;
        }

        ScatterViewItem CreateContainer(BadgeFolder bf)
        {
            ScatterViewItem svi = GetSVI(bf, true);
            return svi;
        }

        void MergeTwoBadges(ArgPoint p1, ArgPoint p2)
        {
            //merge operation is only possible over unsolved field 
            if (unsolvedCurrentTopicItems.Contains(p1) &&
                unsolvedCurrentTopicItems.Contains(p2))
            {
                p1.AgreementCode = (int)AgreementCode.UnsolvedAndGrouped;
                p2.AgreementCode = (int)AgreementCode.UnsolvedAndGrouped;

                unsolvedCurrentTopicItems.Remove(p1);
                unsolvedCurrentTopicItems.Remove(p2);

                BadgeFolder bf = new BadgeFolder();             
                
                p1.Group = bf.model;
                p2.Group = bf.model;              
                
                CtxSingleton.SaveChangesIgnoreConflicts();
                 
                bf.onAttemptToBeginDrag += OnAttemptToBeginDragFromBadgeFolder;
                bf.Width = 310;
                unsolvedCurrentTopicItems.Add(GetSVI(bf,true));
            }
        }

        ScatterViewItem GetSVI(FrameworkElement toWrap, bool CopySize)
        {
            ScatterViewItem svi = new ScatterViewItem();
            svi.Background = Brushes.Transparent;
            svi.BorderBrush = Brushes.Transparent;

            if (CopySize)
            {
                svi.Width = toWrap.Width;
                svi.Height = toWrap.Height;
                svi.CanRotate = false; 
                svi.Deceleration = 100000000;
            }
            svi.Content = toWrap;

            return svi;
        }

        bool MoveToGroup(BadgeFolder bf, ArgPoint ap)
        {
            if (unsolvedCurrentTopicItems.Contains(ap))
            {
                //rec. bin special case 
                if (bf == RecycleBin)
                {
                    //point moved to recycle bin, remove it
                    if (MessageBox.Show("Do you want to delete the point?",
                                        "About to delete",
                                        MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        ap.Group = null;
                        unsolvedCurrentTopicItems.Remove(ap);
                        CtxSingleton.Get().DeleteObject(ap);
                        CtxSingleton.SaveChangesIgnoreConflicts();
                        return true;
                    }
                }
                //move to private special case
                else if (bf == PrivIcon)
                {
                    if (ap.Person.Id == SessionInfo.Get().getPerson(null).Id)
                    {
                        ap.SharedToPublic = false;
                        CtxSingleton.SaveChangesIgnoreConflicts();
                        unsolvedCurrentTopicItems.Remove(ap);
                        _sharedClient.clienRt.SendNotifyStructureChanged(selectedTopic.Id);

                        return true;
                    }
                    else
                    {
                        MessageBox.Show("No permission");
                    }
                }
                else
                {
                    ap.AgreementCode = (int)AgreementCode.UnsolvedAndGrouped;
                    ap.Group = bf.model;
                    unsolvedCurrentTopicItems.Remove(ap);                                        
                    CtxSingleton.SaveChangesIgnoreConflicts();
                    return true;
                }  
            }

            return false;
        }

        void MoveFromGroup(BadgeFolder bf, ArgPoint ap)
        {
            if (!unsolvedCurrentTopicItems.Contains(ap) && bf.model.ArgPoint.Contains(ap))
            {
                ap.AgreementCode = (int)AgreementCode.Unsolved;
                bf.model.ArgPoint.Remove(ap);
                ap.Group = null;
                CtxSingleton.SaveChangesIgnoreConflicts();

                if (bf.model.ArgPoint.Count == 0)
                {
                    unsolvedCurrentTopicItems.Remove(bf.Parent); //todo: remove svi
                    bf = null;
                }
                unsolvedCurrentTopicItems.Add(ap);
            }
        }
    
        void FolderTargetDrop(object sender, SurfaceDragDropEventArgs e)
        {
            //Console.Beep();
            //Console.Beep();
        }

        void FolderTargetDrop2(object sender, DragEventArgs e)
        {
           // Console.Beep();
           // Console.Beep();
        }

        void GetUnsolvedPointsOfCurrentTopic(Topic topic)
        {
            unsolvedCurrentTopicItems.Clear();
            foreach (ArgPoint pt in allTopicsItems)
            {
                if (pt.Topic == topic && pt.AgreementCode == (int)AgreementCode.Unsolved)
                {
                    unsolvedCurrentTopicItems.Add(pt);
                }
            }
            RestoreUnsolvedGroupsOfCurrentTopic(topic);
        }

        public void OnAttemptToBeginDragFromBadgeFolder(InputEventArgs e,
                                                      BadgeFolder DragSource,
                                                      ListBoxItem draggedElement,
                                                      ObservableCollection<object> src)
        {
            if(cursorState==null)
                StartDragDrop(e, draggedElement, DragSource, src);
        }

        private void AttemptToBeginDragInside(InputEventArgs e,
                                              ScatterView dragSrc,
                                              ObservableCollection<object> srcCollection)
        {
            if (cursorState != null)
                return; //previous drag/drop not finished 

            ScatterViewItem draggedElement = Utils.findSVIUnderTouch(e);
            if (draggedElement == null)
                return;

            bool vbxBadgeFolder = (draggedElement.Content is Viewbox) && 
                    ((draggedElement.Content as Viewbox).Child is BadgeFolder);
            if (!(draggedElement.Content is ArgPoint) && !(draggedElement.Content is BadgeFolder) && !vbxBadgeFolder)
                return;

            StartDragDrop(e, draggedElement, dragSrc, srcCollection);
        }

        private void StartDragDrop(InputEventArgs e,
                                  ContentControl draggedElement,
                                  FrameworkElement dragSrc, //ScatterView or BadgeFolder
                                  ObservableCollection<object> srcCollection)
        {
            cursorState = new DragCursorState(draggedElement, dragSrc, srcCollection);

            ArgPoint argPtData = (draggedElement.DataContext as ArgPoint);
            BadgeFolder folderData = (draggedElement.Content as BadgeFolder);
            if (folderData == null)
            {
                if (draggedElement.Content is Viewbox)
                    folderData = (BadgeFolder)((Viewbox)draggedElement.Content).Child;                
            }

            // If the data has not been specified as draggable, 
            // or the ScatterViewItem cannot move, return.
            if (argPtData == null && folderData==null)
            {
                return;
            }

            // Set the dragged element. This is needed in case the drag operation is canceled.
            //data.DraggedElement = draggedElement;

            // Create the cursor visual.
            ContentControl cursorVisual = null;
            object data = null;
            if (argPtData != null)
            {
                data = argPtData;
                cursorVisual = new ContentControl()
                {
                    Content = argPtData,
                    Style = FindResource("BadgeCursorStyle") as Style
                };
            }
            else if (folderData != null)
            {
                data = folderData;
                cursorVisual = new ContentControl()
                {
                    Content = folderData.model,
                    Style = FindResource("FolderCursorStyle") as Style
                };
            }
                        
            SurfaceDragDrop.AddTargetChangedHandler(cursorVisual, TargetChanged);

            // Create a list of input devices, 
            // and add the device passed to this event handler.
            List<InputDevice> devices = new List<InputDevice>();
            devices.Add(e.Device);

            // If there are touch devices captured within the element,
            // add them to the list of input devices.
            foreach (InputDevice device in draggedElement.TouchesCapturedWithin)
            {
                if (device != e.Device)
                {
                    devices.Add(device);
                }
            }
            
            // Start the drag-and-drop operation.
            SurfaceDragCursor cursor =
                SurfaceDragDrop.BeginDragDrop(
                // The ScatterView object that the cursor is dragged out from.
                  dragSrc,
                // The ScatterViewItem object that is dragged from the drag source.
                  draggedElement,
                // The visual element of the cursor.
                  cursorVisual,
                // The data attached with the cursor.
                  data,
                // The input devices that start dragging the cursor.
                  devices,
                // The allowed drag-and-drop effects of the operation.
                  DragDropEffects.Move);

            // If the cursor was created, the drag-and-drop operation was successfully started.
            if (cursor != null)
            {
                // Hide the ScatterViewItem.
                draggedElement.Visibility = Visibility.Hidden;

                // This event has been handled.
                e.Handled = true;
            }
        }

        private void badgeFolder_DragEnter(object sender, DragEventArgs e)
        {
            MessageBox.Show("badgeFolder_DragEnter");
        }

        private void agreement_DragEnter(object sender, DragEventArgs e)
        {
            //MessageBox.Show("agreement_DragEnter");
        }

        private void agreement_DragLeave(object sender, DragEventArgs e)
        {
            //MessageBox.Show("agreement_DragLeave");
        }

        private void agreement_Drop(object sender, DragEventArgs e)
        {
            //MessageBox.Show("agreement_Drop");
        }

        private void DragCanceled()
        {
            if (cursorState == null)
                return;

            if (cursorState.draggedItem is ScatterViewItem)
                (cursorState.draggedItem as ScatterViewItem).Visibility = Visibility.Visible;
            else if (cursorState.draggedItem is ListBoxItem)
                (cursorState.draggedItem as ListBoxItem).Visibility = Visibility.Visible;
            
            cursorState = null;

            dragDropTooltip.Content = "";

            unsolvedDragTapRecognizer.Stop();
            agreedDragTapRecognizer.Stop();
            disagreedDragTapRecognizer.Stop();
        }

        private void AgreementDragCanceled(object sender, SurfaceDragDropEventArgs e)
        {
            DragCanceled();
        }

        private void AgreementDropTargetDrop(object sender, SurfaceDragDropEventArgs e)
        {
           // MessageBox.Show("AgreementDropTargetDrop");

            if (cursorState!=null)
            {
                ProcessDropTarget(e.Cursor,
                                 cursorState.srcCollection,
                                 agreedAllTopicsItems,
                                 (int)AgreementCode.Agreed);
            }
        }

        private void ScatViewDropTargetDrop(object sender, SurfaceDragDropEventArgs e)
        {
            //MessageBox.Show("ScatViewDropTargetDrop");

            if (cursorState != null)
            {
                ArgPoint pt = e.Cursor.Data as ArgPoint;
                BadgeFolder bf = e.Cursor.Data as BadgeFolder;
                if (pt != null && selectedTopic!=null && pt.Topic.Id == selectedTopic.Id)
                {
                    ProcessDropTarget(e.Cursor,
                                     cursorState.srcCollection,
                                     unsolvedCurrentTopicItems,
                                     (int)AgreementCode.Unsolved);
                }
                else if (bf != null)
                {
                    ProcessDropTarget(e.Cursor,
                                     cursorState.srcCollection,
                                     unsolvedCurrentTopicItems,
                                     (int)AgreementCode.UnsolvedAndGrouped);
                }
                else //cancel
                {
                    ProcessDropTarget(e.Cursor,
                                     null,
                                     null,
                                     (int)AgreementCode.Unsolved);
                }
            }
        }

        //////////////////////////////   unsolved  ////////////////////////////// 
        private void unsolved_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            unsolvedDragTapRecognizer.Stop();
            unsolvedDragTapRecognizer = new ContactTimer(                                   
                                                 (object source, EventArgs e1)=>
                                                 AttemptToBeginDragInside(e, unsolved, unsolvedCurrentTopicItems)
                                             );
            recentTouch = Utils.findSVIUnderTouch(e);        
        }
        ScatterViewItem recentTouch = null;
        private void unsolved_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            unsolvedDragTapRecognizer.Stop();
            if (recentTouch != null)
            {
                if (!recentlyMovedItems.Contains(recentTouch))
                    recentlyMovedItems.Add(recentTouch);
            }  
        }

        private void unsolved_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            unsolvedDragTapRecognizer.Stop();
        }

        private void unsolved_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
               e.RightButton == MouseButtonState.Pressed)
                AttemptToBeginDragInside(e, unsolved, unsolvedCurrentTopicItems);
        }

        private void unsolved_DragCanceled(object sender, SurfaceDragDropEventArgs e)
        {
            DragCanceled();
        }

        private void disagreement_DragCanceled(object sender, SurfaceDragDropEventArgs e)
        {
            DragCanceled();
        }

        //////////////////////////////  disagreement  ////////////////////////////// 
        private void disagreement_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            disagreedDragTapRecognizer.Stop();
            disagreedDragTapRecognizer = new ContactTimer(
                                        (object source, EventArgs e1) =>
                                        AttemptToBeginDragInside(e, disagreement, disagreedAllTopicsItems)
                                     );       
        }

        private void disagreement_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            disagreedDragTapRecognizer.Stop();
            e.Handled = false;
        }


        private void disagreement_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            disagreedDragTapRecognizer.Stop();
        }

        private void disagreement_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                e.RightButton == MouseButtonState.Pressed)
                AttemptToBeginDragInside(e, disagreement, disagreedAllTopicsItems);
        }

        private void disagreementDropTargetDrop(object sender, SurfaceDragDropEventArgs e)
        {
             if (cursorState != null)
             {                                  
                 ProcessDropTarget(e.Cursor,
                                  cursorState.srcCollection,
                                  disagreedAllTopicsItems,
                                  (int)AgreementCode.Disagreed);
             }
        }

        //////////////////////////////  agreed  //////////////////////////////// 
        private void agreement_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            agreedDragTapRecognizer.Stop();
            agreedDragTapRecognizer = new ContactTimer(
                        (object sender1, EventArgs e1) =>
                            AttemptToBeginDragInside(e, agreement, agreedAllTopicsItems)
                                     );

        }

        private void agreement_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            agreedDragTapRecognizer.Stop();
            e.Handled = false; 
        }


        private void agreement_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            agreedDragTapRecognizer.Stop();
        }

        private void agreement_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                e.RightButton == MouseButtonState.Pressed)
                AttemptToBeginDragInside(e, agreement, agreedAllTopicsItems);
        }

        private void cbxGroup_SelectionChanged(object selected)
        {
            selectedTopic = selected as Topic;
            if (selectedTopic == null)
                return;

            //we have pending changes
            ForgetDBDiscussionState();
            InitCollections();           
            CtxSingleton.SaveChangesIgnoreConflicts();
            GetUnsolvedPointsOfCurrentTopic(selectedTopic);
            CreateSpecialIcons();

            OnRefreshBadgeLayout();       
        }

        void OnRefreshBadgeLayout()
        {
            sviRotationBlocked = false;
            SendBadgeGeometryRequest();   
        }

        private void btnViewResults_Click(object sender, RoutedEventArgs e)
        {
            CtxSingleton.SaveChangesIgnoreConflicts();           
            new ResultViewer(discussion,null).Show();
            Close();
        }

        bool AnnotationsManipulation()
        {
            return layerMode == LayerMode.CreatedInPlaceEditing || layerMode == LayerMode.LoadedLayerEditing;
        }

        private void SurfaceWindow_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            if (AnnotationsManipulation())
                return;
            e.ManipulationContainer = this;
            e.Handled = true;
        }

        private void SurfaceWindow_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (AnnotationsManipulation())
                return;
            unsolved_ManipulationDelta(sender, e);
        }

        private void BadgeCanvas_TouchDown(object sender, TouchEventArgs e)
        {
            setUserCursorLocally(sender as DependencyObject);
            if (e.TouchDevice.GetTouchPoint(sender as IInputElement).Size.Width > 70)
            {
                Border b = sender as Border;
                ToggleHighlight(b);
            }
        }

        private void SmallBadge_TouchDown(object sender, TouchEventArgs e)
        {
            if (e.TouchDevice.GetTouchPoint(sender as IInputElement).Size.Width > 150)
            {
                Border b = sender as Border;
                ToggleHighlight(b);
            }
        }

        void ToggleHighlight(Border b)
        {            
            if (b != null)
            {
                if (b.BorderBrush == null)
                    b.BorderBrush = DiscussionColors.badgeHighlightBrush;
                else
                    b.BorderBrush = null;
            }
        }

        void Highlight(Border b)
        {
            if (b != null)
            {
                b.Background = DiscussionColors.badgeHighlightBrush;
            }
        }

        private void unsolved_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (AnnotationsManipulation())
                return;
            
            ScatterView element = unsolved;

            var matrix = GetUnsolvedTransform();

            matrix.ScaleAt(e.DeltaManipulation.Scale.X,
                            e.DeltaManipulation.Scale.Y,
                            e.ManipulationOrigin.X,
                            e.ManipulationOrigin.Y);

            //matrix.RotateAt(e.DeltaManipulation.Rotation,
            //                e.ManipulationOrigin.X,
            //                e.ManipulationOrigin.Y);

            matrix.Translate(e.DeltaManipulation.Translation.X,
                             e.DeltaManipulation.Translation.Y);

            element.RenderTransform = new MatrixTransform(matrix);

            e.Handled = true;
        }

        private void unsolved_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            if (AnnotationsManipulation())
                return;
            
            e.ManipulationContainer = this;
            e.Handled = true;
        }

        Matrix GetUnsolvedTransform()
        {
            var transformation = unsolved.RenderTransform
                                                 as MatrixTransform;
            var matrix = transformation == null ? Matrix.Identity :
                                           transformation.Matrix;
            return matrix;
        }

        private void unsolved_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScatterView element = unsolved;

            var matrix = GetUnsolvedTransform();

            Point mousePos = e.GetPosition(this);

            double factor = e.Delta > 0 ? 1.1 : 0.9;

            matrix.ScaleAt(factor,
                           factor,
                           mousePos.X,
                           mousePos.Y);

            element.RenderTransform = new MatrixTransform(matrix);
        }

        private void SurfaceWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            unsolved_MouseWheel(sender, e);
        }

        double PrevX, PrevY;
        private void SurfaceWindow_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(this);

            if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) !=
                    System.Windows.Forms.Keys.None)
            {
                ScatterView element = unsolved;

                var matrix = GetUnsolvedTransform();

                if (PrevX > 0 && PrevY > 0)
                    matrix.Translate(mousePos.X - PrevX, mousePos.Y - PrevY);

                element.RenderTransform = new MatrixTransform(matrix);
            }

            PrevX = mousePos.X;
            PrevY = mousePos.Y;

            //notify user cursor          
            userCursor.State = CursorInputState.Mouse;
            userCursor.x = mousePos.X;
            userCursor.y = mousePos.Y;
            _sharedClient.clienRt.SendNotifyUserCursorChanged(userCursor);
            /////////////
        }

        #region rt client 

        void SetListeners(UISharedRTClient sharedClient, bool doSet)
        {
            var clienRt = sharedClient.clienRt;
            
            if(doSet)
                clienRt.receivedBadgeGeometry += OnReceivedBadgeGeometry;
            else            
                clienRt.receivedBadgeGeometry -= OnReceivedBadgeGeometry;

            if (doSet)
                clienRt.onJoin += OnJoin;
            else
                clienRt.onJoin -= OnJoin;

            if (doSet)
                clienRt.receivedBadgeExpansion += receivedBadgeExpansion;
            else
                clienRt.receivedBadgeExpansion -= receivedBadgeExpansion;

            if (doSet)
                clienRt.onStructChanged += onStructChanged;
            else
                clienRt.onStructChanged -= onStructChanged;

            if (doSet)
                clienRt.geometryChangedOnSrv += SendBadgeGeometryRequest;
            else
                clienRt.geometryChangedOnSrv -= SendBadgeGeometryRequest;

            //if (doSet)
            //    clienRt.userCursorChanged += onUserCursorChanged;
            //else
            //    clienRt.userCursorChanged -= onUserCursorChanged;

            if (doSet)
                clienRt.userLeaves += UserLeave;
            else
                clienRt.userLeaves -= UserLeave;

            if (doSet)
                sharedClient.rtTickHandler += OnRtServiceTick;
            else
                sharedClient.rtTickHandler -= OnRtServiceTick;

            if (doSet)
                clienRt.argPointChanged += ArgPointChanged; 
            else
                clienRt.argPointChanged -= ArgPointChanged;

            if (doSet)
                clienRt.annotationChanged += OnAnnotationChanged; 
            else
                clienRt.annotationChanged -= OnAnnotationChanged; 
        }

        void onUserCursorChanged(UserCursor c)
        {
            UserCursorUC cuc = null;
            if (!userCursors.ContainsKey(c.usrId))
            {
                cuc = new UserCursorUC();
                cuc.DataContext = c;
                overlay.Children.Add(cuc);
                userCursors.Add(c.usrId, cuc);
            }
            else
            {
                cuc = userCursors[c.usrId];
            }

            Canvas.SetLeft(cuc, c.x);
            Canvas.SetTop(cuc, c.y);
        }

        public void UserLeave(DiscUser usr)
        {
            if (userCursors.ContainsKey(usr.usrDbId))
            {
                UserCursorUC cuc = userCursors[usr.usrDbId];
                userCursors.Remove(usr.usrDbId);
                overlay.Children.Remove(cuc);
                overlay.InvalidateVisual();
            }
        }

        bool ownHighlighted = false;
        public void OnRtServiceTick()
        {
            if (recentlyMovedItems.Count > 0)
            {
                notifyGeometryChanged(recentlyMovedItems,false,-1);
                recentlyMovedItems.Clear();
            }

            EnsureInitBadgeGeom();

            if(!ownHighlighted)
                ownHighlighted = HighlightOwnSmallBadges();

            BlockSVIRotation();       
        }

        public void notifyGeometryChanged(List<ScatterViewItem> updatedItems, bool withStruct, int activeTopicId)
        {
            var updatedBadges = new List<SharedView>();
            foreach (var svi in updatedItems)
            {
                bool viewType;
                int Id = SviContentToId(svi, out viewType);
                if (Id != -1)
                {
                    var sharedBadge = new SharedView(Id, viewType);
                    sharedBadge.badgeGeometry.CenterX = svi.Center.X;
                    sharedBadge.badgeGeometry.CenterY = svi.Center.Y;
                    sharedBadge.badgeGeometry.Orientation = svi.Orientation;
                    sharedBadge.viewType = viewType;
                    updatedBadges.Add(sharedBadge);
                }
            }

            userCursor.State = CursorInputState.Mouse;
            _sharedClient.clienRt.NotifyGeometryChanged(userCursor, updatedBadges.ToArray(), withStruct, activeTopicId);
        }

        public void OnJoin()
        {
            EnsureInitBadgeGeom();
        }
       
        bool badgeGeometryInitialized = false;
        void EnsureInitBadgeGeom()
        {
            if (!badgeGeometryInitialized)
            {
                SendBadgeGeometryRequest();
                badgeGeometryInitialized = true;
            }
        }

        void SendBadgeGeometryRequest()
        {
            if (_sharedClient.clienRt != null && unsolved != null && unsolved.ActualWidth > 0.0)
            {
                _sharedClient.clienRt.SendBadgeGeometryRequest(unsolved.ActualWidth, unsolved.ActualHeight);
            }
        }

        public static int SviContentToId(ScatterViewItem svi, out bool viewType)
        {
            int Id = -1;
            viewType = false;

            ArgPoint ap = svi.DataContext as ArgPoint;           
            if (ap != null)
            {
                Id = ap.Id;
                viewType = true;
            }
            else
            {
                BadgeFolder bf = Utils.FindChild<BadgeFolder>(svi);
                if (bf != null)
                {
                    Id = bf.model.Id;
                    viewType = false;
                }
            }
            return Id;
        }

        public void OnReceivedBadgeGeometry(UserCursor cursor, SharedView[] badges)
        {
            Dictionary<int, SharedView> argPointsDict = Serializers.ArrToDict(badges.Where(sv => sv.viewType));
            Dictionary<int, SharedView> groupDict = Serializers.ArrToDict(badges.Where(sv => !sv.viewType));

            Utils.EnumSVIs(unsolved, (ScatterViewItem svi) =>
            {
                bool viewType;
                int Id = SviContentToId(svi, out viewType);

                SharedView sv = null;
                if (viewType)
                {
                    if (argPointsDict.ContainsKey(Id))
                        sv = argPointsDict[Id];
                }
                else
                {
                    if (groupDict.ContainsKey(Id))
                        sv = groupDict[Id];
                }

                if (sv != null)
                {
                    svi.Center = new Point(sv.badgeGeometry.CenterX,
                                           sv.badgeGeometry.CenterY);
                    svi.Orientation = sv.badgeGeometry.Orientation;

                    SetUserCursor(svi, cursor);
                }
                else
                {
                    //hide previous cursors of the user 
                    HideUserCursor(svi, cursor.usrId);
                }
            });
        }

        //cc like ScatterViewItem
        void SetUserCursor(DependencyObject cc, UserCursor cursor)
        {
            UserCursorUC cursorUC = Utils.FindChild<UserCursorUC>(cc, "usrCursor");
            if (cursorUC != null)
            {
                cursorUC.Visibility = Visibility.Visible;
                cursorUC.DataContext = cursor;
            }
        }

        void HideUserCursor(DependencyObject cc, int usrIdToHide)
        {
            UserCursorUC cursorUC = Utils.FindChild<UserCursorUC>(cc, "usrCursor");
            if (cursorUC != null)
            {
                UserCursor uc = cursorUC.DataContext as UserCursor;
                if (uc != null)
                {
                    if (uc.usrId == usrIdToHide)
                    {
                        cursorUC.Visibility = Visibility.Hidden;
                        cursorUC.DataContext = null;
                    }
                }               
            }
        }
        
        void setUserCursorLocally(DependencyObject userCursorUCAncestor)
        {
            unsetPrevLocalCursors();
            if (userCursorUCAncestor != null)
            {
                SetUserCursor(userCursorUCAncestor, userCursor);
            }
        }

        void unsetPrevLocalCursors()
        {
            Utils.EnumSVIs(unsolved, (ScatterViewItem svi) =>
            {
                HideUserCursor(svi, SessionInfo.Get().getPerson(SessionInfo.Get().discussion).Id);
            });
        }

        private void unsolved_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ScatterViewItem svi = Mouse.DirectlyOver as ScatterViewItem;
                if (svi != null)
                {
                    if (!recentlyMovedItems.Contains(svi))
                        recentlyMovedItems.Add(svi);
                }
            }
        }

        private void BadgeCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            setUserCursorLocally(sender as DependencyObject);
        }

        private void expansionChanged(object sender, RoutedEventArgs e)
        {
            Badge badge = e.OriginalSource as Badge;
            _sharedClient.clienRt.SendBadgeExpansion(((ArgPoint)badge.DataContext).Id, 
                                       badge.IsExpanded());
            
            CtxSingleton.SaveChangesIgnoreConflicts();
        }

        void commentsChanged(object sender, RoutedEventArgs e)
        {
            if(selectedTopic==null)
                return; 
            CtxSingleton.SaveChangesIgnoreConflicts();
            _sharedClient.clienRt.SendNotifyStructureChanged(selectedTopic.Id);
        }
        
        public void receivedBadgeExpansion(int argPointId, bool expanded)
        {            
            ArgPoint pt = findArgPoint(argPointId);
            if (pt != null)
            {
                pt.Expanded = expanded;                
            }
        }

        ArgPoint findArgPoint(int pointId)
        {
            foreach (object ap in unsolvedCurrentTopicItems)
            {
                ArgPoint pt = ap as ArgPoint;                
                if (pt!=null && pt.Id == pointId)
                {
                    return pt;
                }
            }
            return null;
        }

        void ForgetDBDiscussionState()
        {
            //forget cached state
            CtxSingleton.DropContext();
            _discussion = SessionInfo.Get().discussion;
            //_discussion = CtxSingleton.Get().Discussion.FirstOrDefault(d1 => d1.Id == _discussion.Id);
            if (selectedTopic != null)
                selectedTopic = _discussion.Topic.FirstOrDefault(t1 => t1.Id == selectedTopic.Id);
            //////////////////////
        }

        /// <param name="activeTopic">db id  of topic that has been changed remotely</param>
        void onStructChanged(int activeTopic)
        {
            Console.WriteLine("discussion board: struct changed");
            
            BusyWndSingleton.Show("Fetching changes...");
            try
            {
                ForgetDBDiscussionState();

                UpdateTopics();
                InitCollections();

                if (selectedTopic != null)
                {
                    GetUnsolvedPointsOfCurrentTopic(selectedTopic);
                    CreateSpecialIcons();

                    OnRefreshBadgeLayout();
                }
            }
            finally
            {
                BusyWndSingleton.Hide();
            }
        }

        void ReportOwnGeomPostStruct()
        {   
            recentlyMovedItems.Clear();

            Utils.EnumSVIs(unsolved, (ScatterViewItem svi) =>
            {
                recentlyMovedItems.Add(svi);
            });

            if (recentlyMovedItems.Count > 0)
            {
                notifyGeometryChanged(recentlyMovedItems, true, 
                                     selectedTopic != null ? selectedTopic.Id : -1);
                recentlyMovedItems.Clear();
            }
        }

        private void SurfaceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetListeners(_sharedClient, false);
            if (_closing != null)
                _closing();
        }

        void ArgPointChanged(int ArgPointId)
        {
            //find the point
            //ArgPoint changedPoint = null;
            //foreach(var p in allTopicsItems)
            //    if(p.Id==ArgPointId) 
            //    {
            //        changedPoint = p;
            //        break;
            //    }

            //if(changedPoint==null)
            //    return;
           
            onStructChanged(-1);
        }

        #endregion rt client 

        private void btnStatus_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _stWnd.Toggle();
        }

        private void btnGoPrivate_Click(object sender, RoutedEventArgs e)
        {
            var wnd = DiscWindows.Get();
            if (wnd.privateDiscBoard != null)
            {
                wnd.privateDiscBoard.Activate();
                return;
            }

            wnd.privateDiscBoard = new PrivateCenter(_sharedClient, _stWnd, () => { wnd.privateDiscBoard = null; });
            wnd.privateDiscBoard.Show();

           // Close();
        }

        #region layer managment 
        enum LayerMode { NoLayer, ReadOnlyLoadedLayer, LoadedLayerEditing, CreatedInPlaceEditing, CreatedInPlaceReadonly } 
        LayerMode layerMode = LayerMode.NoLayer;

        //IsManipulationEnabled doesn't help in preventing manipulations from touch events 
        //we catch ink events here
        void inkPreviewTouchDown(object sender, TouchEventArgs e)
        {
           // e.Handled = true;
        }

        void inkPreviewTouchMove(object sender, TouchEventArgs e)
        {
            //e.Handled = true;
        }

        void inkPreviewTouchUp(object sender, TouchEventArgs e)
        {
            //e.Handled = true;
        } 

        void ToLayerModeNoLayer()
        {
            layerMode = LayerMode.NoLayer;
            lblAnnotations.Text = "Annotations";

            btnCreateInPlace.Visibility = Visibility.Visible;
            btnEditLayer.Visibility = Visibility.Collapsed;
            btnReadOnly.Visibility = Visibility.Collapsed;
            btnFinishFreeForm.Visibility = Visibility.Collapsed;
            btnCreateWithScreenshot.Visibility = Visibility.Visible;
            btnLoadAnnot.Visibility = Visibility.Visible;
            btnCloseAnnot.Visibility = Visibility.Collapsed;
            btnSaveAnnot.Visibility = Visibility.Collapsed;

            toolPanel.Visibility = Visibility.Collapsed;
            overlay.Visibility = Visibility.Collapsed;
            overlay.IsHitTestVisible = false;
            ink.IsHitTestVisible = false;
            ink.Visibility = Visibility.Collapsed;

            if (graphicsCtx != null)
                graphicsCtx.ToReadOnlyMode();
        }

        void ToReadOnlyLoadedLayer()
        {
            layerMode = LayerMode.ReadOnlyLoadedLayer;
            lblAnnotations.Text = "Annotations(read-only)";

            btnCreateInPlace.Visibility = Visibility.Collapsed;
            btnEditLayer.Visibility = Visibility.Visible;
            btnReadOnly.Visibility = Visibility.Collapsed;
            btnFinishFreeForm.Visibility = Visibility.Collapsed;
            btnCreateWithScreenshot.Visibility = Visibility.Collapsed;
            btnLoadAnnot.Visibility = Visibility.Collapsed;
            btnCloseAnnot.Visibility = Visibility.Visible;
            btnSaveAnnot.Visibility = Visibility.Collapsed;

            toolPanel.Visibility = Visibility.Collapsed;
            overlay.Visibility = Visibility.Visible;
            overlay.IsHitTestVisible = false;
            ink.IsHitTestVisible = false;
            ink.Visibility = Visibility.Collapsed;

            if (graphicsCtx != null)
                graphicsCtx.ToReadOnlyMode();
        }

        void ToLoadedLayerEditing()
        {
            layerMode = LayerMode.LoadedLayerEditing;
            lblAnnotations.Text = "Annotations(editing)";

            btnCreateInPlace.Visibility = Visibility.Collapsed;
            btnEditLayer.Visibility = Visibility.Collapsed;
            btnReadOnly.Visibility = Visibility.Visible;
            btnFinishFreeForm.Visibility = Visibility.Collapsed;
            btnCreateWithScreenshot.Visibility = Visibility.Collapsed;
            btnLoadAnnot.Visibility = Visibility.Collapsed;
            btnCloseAnnot.Visibility = Visibility.Visible;
            btnSaveAnnot.Visibility = Visibility.Visible;

            toolPanel.Visibility = Visibility.Visible;
            overlay.Visibility = Visibility.Visible;
            overlay.IsHitTestVisible = true;
        }

        void ToCreatedInPlaceEditing()
        {
            layerMode = LayerMode.CreatedInPlaceEditing;
            lblAnnotations.Text = "Annotations(editing)";

            btnCreateInPlace.Visibility = Visibility.Collapsed;
            btnEditLayer.Visibility = Visibility.Collapsed;
            btnReadOnly.Visibility = Visibility.Visible;
            btnFinishFreeForm.Visibility = Visibility.Collapsed;
            btnCreateWithScreenshot.Visibility = Visibility.Collapsed;
            btnLoadAnnot.Visibility = Visibility.Collapsed;
            btnCloseAnnot.Visibility = Visibility.Visible;
            btnSaveAnnot.Visibility = Visibility.Visible;

            toolPanel.Visibility = Visibility.Visible;
            overlay.Visibility = Visibility.Visible;
            overlay.IsHitTestVisible = true;
        }

        void ToCreatedInPlaceReadonly()
        {            
            layerMode = LayerMode.CreatedInPlaceReadonly;
            lblAnnotations.Text = "Annotations(read-only)";

            btnCreateInPlace.Visibility = Visibility.Collapsed;
            btnEditLayer.Visibility = Visibility.Visible;
            btnReadOnly.Visibility = Visibility.Collapsed;
            btnFinishFreeForm.Visibility = Visibility.Collapsed;
            btnCreateWithScreenshot.Visibility = Visibility.Collapsed;
            btnLoadAnnot.Visibility = Visibility.Collapsed;
            btnCloseAnnot.Visibility = Visibility.Visible;
            btnSaveAnnot.Visibility = Visibility.Collapsed;

            toolPanel.Visibility = Visibility.Collapsed;
            overlay.Visibility = Visibility.Visible;
            overlay.IsHitTestVisible = false;

            if (graphicsCtx != null)
                graphicsCtx.ToReadOnlyMode();
        }

        bool CreateGraphicContext(bool createNewGraphics)
        {          
            if(graphicsCtx!=null)
            {
                graphicsCtx.SetListeners(false);
                graphicsCtx = null;
            }

            int owner = SessionInfo.Get().person.Id;
            graphicsCtx = new EditorWndCtx(overlay, ink, toolPanel, btnFinishFreeForm, owner, this);
            if (createNewGraphics)
            {
                overlay.Children.Clear();
                graphicsCtx.CreateAnnotation(null, (int)this.ActualWidth, (int)this.ActualHeight, owner);                              
                return true;
            }                
            else
            {
                var aw = new AnnotationsWnd();
                aw.ShowDialog();

                if (aw.SelectedAnnotation() == null)
                    return false;
                
                if (aw.SelectedAnnotation().Bg != null)
                {
                    MessageBox.Show("Only can open in-place annotations in this form");
                    return false;
                }
                    
                graphicsCtx.LoadAnnotation(aw.SelectedAnnotation());
                btnEditLayer.Visibility = Visibility.Visible;
                btnEditLayer.Content = "Edit graphics";              
                return true;
            }
        }

        void ModeSwitch(LayerMode newMode)
        {
            switch (layerMode)
            {
                case LayerMode.NoLayer:
                    switch(newMode)
                    {
                        case LayerMode.NoLayer:
                            ToLayerModeNoLayer();
                            break;
                        case LayerMode.ReadOnlyLoadedLayer:
                            ToReadOnlyLoadedLayer();
                            break;
                        case LayerMode.LoadedLayerEditing:
                            ToLoadedLayerEditing();
                            break;
                        case LayerMode.CreatedInPlaceEditing:
                            ToCreatedInPlaceEditing();
                            break;
                        case LayerMode.CreatedInPlaceReadonly:
                            ToCreatedInPlaceReadonly();
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                case LayerMode.ReadOnlyLoadedLayer:
                    switch (newMode)
                    {
                        case LayerMode.NoLayer:
                            ToLayerModeNoLayer();
                            break;
                        case LayerMode.ReadOnlyLoadedLayer:
                            ToReadOnlyLoadedLayer();
                            break;
                        case LayerMode.LoadedLayerEditing:
                            ToLoadedLayerEditing();
                            break;
                        case LayerMode.CreatedInPlaceEditing:
                            ToCreatedInPlaceEditing();
                            break;
                        case LayerMode.CreatedInPlaceReadonly:
                            ToCreatedInPlaceReadonly();
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                case LayerMode.LoadedLayerEditing:
                    switch (newMode)
                    {
                        case LayerMode.NoLayer:
                            ToLayerModeNoLayer();
                            break;
                        case LayerMode.ReadOnlyLoadedLayer:
                            ToReadOnlyLoadedLayer();
                            break;
                        case LayerMode.LoadedLayerEditing:
                            ToLoadedLayerEditing();
                            break;
                        case LayerMode.CreatedInPlaceEditing:
                            ToCreatedInPlaceEditing();
                            break;
                        case LayerMode.CreatedInPlaceReadonly:
                            ToCreatedInPlaceReadonly();
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                case LayerMode.CreatedInPlaceEditing:
                    switch (newMode)
                    {
                        case LayerMode.NoLayer:
                            ToLayerModeNoLayer();
                            break;
                        case LayerMode.ReadOnlyLoadedLayer:
                            ToReadOnlyLoadedLayer();
                            break;
                        case LayerMode.LoadedLayerEditing:
                            ToLoadedLayerEditing();
                            break;
                        case LayerMode.CreatedInPlaceEditing:
                            ToCreatedInPlaceEditing();
                            break;
                        case LayerMode.CreatedInPlaceReadonly:
                            ToCreatedInPlaceReadonly();
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                case LayerMode.CreatedInPlaceReadonly:
                    switch (newMode)
                    {
                        case LayerMode.NoLayer:
                            ToLayerModeNoLayer();
                            break;
                        case LayerMode.ReadOnlyLoadedLayer:
                            ToReadOnlyLoadedLayer();
                            break;
                        case LayerMode.LoadedLayerEditing:
                            ToLoadedLayerEditing();
                            break;
                        case LayerMode.CreatedInPlaceEditing:
                            ToCreatedInPlaceEditing();
                            break;
                        case LayerMode.CreatedInPlaceReadonly:
                            ToCreatedInPlaceReadonly();
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void SurfaceWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (layerMode == LayerMode.CreatedInPlaceEditing || layerMode == LayerMode.LoadedLayerEditing)
                    graphicsCtx.RemoveShape();
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.C)
                {
                    if(graphicsCtx!=null)
                        graphicsCtx.CopySelected();                  
                }
                else if (e.Key == Key.V)
                {
                    if (graphicsCtx != null)
                        graphicsCtx.PasteSelected();
                    Keyboard.Focus(this);                   
                }
            }   
        }

        private void btnLayers_Click(object sender, RoutedEventArgs e)
        {
            if (layerControls.Visibility == Visibility.Collapsed)
                layerControls.Visibility = Visibility.Visible;
            else
                layerControls.Visibility = Visibility.Collapsed;
        }

        private void btnEditLayer_Click(object sender, RoutedEventArgs e)
        {
            if (layerMode == LayerMode.CreatedInPlaceReadonly)
                ModeSwitch(LayerMode.CreatedInPlaceEditing);
            else if (layerMode == LayerMode.ReadOnlyLoadedLayer)
                ModeSwitch(LayerMode.LoadedLayerEditing);                               
        }

        private void btnReadOnly_Click(object sender, RoutedEventArgs e)
        {
            if (layerMode == LayerMode.CreatedInPlaceEditing)
                ModeSwitch(LayerMode.CreatedInPlaceReadonly);
            else if (layerMode == LayerMode.LoadedLayerEditing)
                ModeSwitch(LayerMode.ReadOnlyLoadedLayer);               
        }

        private void btnCreateInPlace_Click(object sender, RoutedEventArgs e)
        {
            if (CreateGraphicContext(true))
            {                
                ModeSwitch(LayerMode.CreatedInPlaceEditing);
            }
        }

        private void btnCreateWithScreenshot_Click(object sender, RoutedEventArgs e)
        {
            var screenPath = Screenshot.Take(this);

            int owner = SessionInfo.Get().person.Id;
            VectorEditorWnd wnd = new VectorEditorWnd(null, owner);
            wnd.LoadBackground(screenPath, (int)this.ActualWidth, (int)this.ActualHeight, owner);
            wnd.Show();

            Close();
        }

        private void btnLoadAnnot_Click(object sender, RoutedEventArgs e)
        {
            if(CreateGraphicContext(false))
                ModeSwitch(LayerMode.ReadOnlyLoadedLayer);        
        }

        private void btnCloseAnnot_Click(object sender, RoutedEventArgs e)
        {
            ModeSwitch(LayerMode.NoLayer);               
        }       

        private void btnSaveAnnot_Click(object sender, RoutedEventArgs e)
        {
            if (graphicsCtx != null)
            {
                var annot = graphicsCtx.Save(this);
                if(annot!=null)
                    _sharedClient.clienRt.SendNotifyAnnotationChanged(annot.Id);
            }
        }

        void OnAnnotationChanged(int annotId)
        {
             if (graphicsCtx != null)
             {
                 var a = DaoUtils.GetUpdatedAnnotation(annotId);
                 if (a != null)
                     graphicsCtx.LoadAnnotation(a);
             }
        }

        #endregion layer managment
    }
}
