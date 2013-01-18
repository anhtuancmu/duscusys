using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Discussions.model;
using Discussions.DbModel;
using Discussions.rt;
using LoginEngine;
using Discussions.RTModel.Model;
using System.Collections.ObjectModel;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for EditableBadge.xaml
    /// </summary>
    public partial class LargeBadgeView : UserControl
    {
        private MultiClickRecognizer mediaDoubleClick;

        private UISharedRTClient _sharedClient;

        //large badge view was open, explanation mode was enabled, another client closed badge, but badge
        //on current client wasn't closed as a comment was being edited on local client. this flag remembers 
        //missed close event.
        public bool MissedCloseRequest = false;

        public static readonly RoutedEvent RequestSmallViewEvent = EventManager.RegisterRoutedEvent(
            "RequestSmallView", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (LargeBadgeView));

        public event RoutedEventHandler RequestSmallView
        {
            add { AddHandler(RequestSmallViewEvent, value); }
            remove { RemoveHandler(RequestSmallViewEvent, value); }
        }

        private ObservableCollection<Source> sources = new ObservableCollection<Source>();

        public ObservableCollection<Source> Sources
        {
            get { return sources; }
        }

        private ObservableCollection<Attachment> attachments = new ObservableCollection<Attachment>();

        public ObservableCollection<Attachment> Attachments
        {
            get { return attachments; }
        }

        //if source order or data context changes, we update 
        private void UpdateOrderedSources()
        {
            Sources.Clear();
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            foreach (var orderedSrc in ap.Description.Source.OrderBy(s => s.OrderNumber))
            {
                Sources.Add(orderedSrc);
            }
        }

        private void UpdateOrderedMedia()
        {
            Attachments.Clear();
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            foreach (var orderedAtt in ap.Attachment.OrderBy(a0 => a0.OrderNumber))
            {
                Attachments.Add(orderedAtt);
            }
        }

        private void BeginAttachmentNumberInjection()
        {
            Dispatcher.BeginInvoke(new Action(_injectMediaNumbers),
                                   System.Windows.Threading.DispatcherPriority.Background, null);
        }

        private void _injectMediaNumbers()
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            for (int i = 0; i < ap.Attachment.Count(); i++)
            {
                var item = lstBxAttachments.ItemContainerGenerator.ContainerFromIndex(i);

                var video = Utils.FindChild<LargeVideoUC>(item);
                if (video != null)
                    video.number.Text = (i + 1).ToString();
                else
                {
                    var image = Utils.FindChild<LargeImageUC>(item);
                    if (image != null)
                        image.number.Text = (i + 1).ToString();
                }
            }
        }


        //during comment editing large badge view cannot be closed
        private bool _isEditingComment = false;

        public bool IsEditingComment
        {
            get { return _isEditingComment; }
        }

        public LargeBadgeView()
        {
            InitializeComponent();

            //  RichTextBoxFormatBarManagerStatic.SetFormatBar(rtb, fmtBar);

            //  Drawing.dataContextHandled += DrawingDataContextHandled; 

            mediaDoubleClick = new MultiClickRecognizer(badgeDoubleTap, null);

            _commentDismissalRecognizer = new CommentDismissalRecognizer(scrollViewer, OnDismiss);

            lstBxSources.DataContext = this;
            lstBxAttachments.DataContext = this;

            BeginAttachmentNumberInjection();
        }

        public void SetRt(UISharedRTClient sharedClient)
        {
            _sharedClient = sharedClient;
            SetListeners(true);
        }

        private void badgeDoubleTap(object sender, InputEventArgs e)
        {
            SetListeners(false);

            RaiseEvent(new RoutedEventArgs(RequestSmallViewEvent));
            //if (CloseRequest != null)
            //    CloseRequest();
        }

        private void SetListeners(bool doSet)
        {
            if (doSet)
                _sharedClient.clienRt.argPointChanged += ArgPointChanged;
            else
                _sharedClient.clienRt.argPointChanged -= ArgPointChanged;

            if (doSet)
                _sharedClient.clienRt.onCommentRead += OnCommentRead;
            else
                _sharedClient.clienRt.onCommentRead -= OnCommentRead;
        }

        private void ArgPointChanged(int ArgPointId, int topicId, PointChangedType change)
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            if (ArgPointId != ap.Id)
                return; //not our point

            if (change != PointChangedType.Modified)
                return;

            //using db ctx here from login engine, not to spoil others
            DbCtx.DropContext();
            var ap2 = DbCtx.Get().ArgPoint.FirstOrDefault(p0 => p0.Id == ArgPointId);            

            BadgesCtx.DropContext();

            DataContext = null;
            DataContext = ap2;
        }

        private void BeginSrcNumberInjection()
        {
            Dispatcher.BeginInvoke(new Action(_injectSourceNumbers),
                                   System.Windows.Threading.DispatcherPriority.Background, null);
        }

        private void _injectSourceNumbers()
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            for (int i = 0; i < ap.Description.Source.Count(); i++)
            {
                var item = lstBxSources.ItemContainerGenerator.ContainerFromIndex(i);
                var srcUC = Utils.FindChild<SourceUC>(item);
                if (srcUC != null)
                    srcUC.SrcNumber = i + 1;
            }
        }

        private void UserControl_Initialized_1(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(
                new Action(setMaxWidthOfDescription),
                System.Windows.Threading.DispatcherPriority.Background);
        }

        private void setMaxWidthOfDescription()
        {
            plainDescription.MaxWidth = this.ActualWidth - 10;
        }

        private bool _editingMode = false;

        public bool EditingMode
        {
            get { return _editingMode; }
            set
            {
                _editingMode = value;
                ///rtbDescription.IsHitTestVisible = _editingMode;
                plainDescription.IsHitTestVisible = _editingMode;
                //Drawing.EditingMode = _editingMode;
                txtPoint.IsHitTestVisible = _editingMode;
            }
        }

        public void HandleRecontext()
        {
            SetStyle();

            if (DataContext == null)
            {
                Opacity = 0;
            }
            else
            {
                Opacity = 1;
            }

            //Drawing.HandleRecontext();

            var ap = DataContext as ArgPoint;
            _commentDismissalRecognizer.Reset(ap);
            UpdateLocalReadCounts(DbCtx.Get(), ap);
            UpdateRemoteReadCounts(DbCtx.Get(), ap);

            if (DataContext == null)
                EditingMode = false;
            else
            {
                EditingMode = SessionInfo.Get().person.Id == ap.Person.Id;
            }

            BeginSrcNumberInjection();
            UpdateOrderedSources();
            BeginAttachmentNumberInjection();
            UpdateOrderedMedia();

            ///commentsViewer.ScrollToBottom();
        }

        private void SetStyle()
        {
            if (DataContext != null && DataContext is ArgPoint)
            {
                ArgPoint p = (ArgPoint) DataContext;
                lblPerson.Background = new SolidColorBrush(Utils.IntToColor(p.Person.Color));
                switch ((SideCode) p.SideCode)
                {
                    case SideCode.Pros:
                        ///  stkHeader.Background = DiscussionColors.prosBrush;
                        break;
                    case SideCode.Cons:
                        ///   stkHeader.Background = DiscussionColors.consBrush;
                        break;
                    case SideCode.Neutral:
                        ///    stkHeader.Background = DiscussionColors.neutralBrush;
                        break;
                    default:
                        throw new NotSupportedException();
                }
                lblPerson.Content = p.Person.Name;
            }
        }

        private void disableAll()
        {
            //removeSketch.Visibility = Visibility.Hidden;
            //finishDrawing.Visibility = Visibility.Hidden;
        }

        private void EnableDrawingControls()
        {
            //finishDrawing.Visibility = Visibility.Hidden;
            //removeSketch.Visibility = Visibility.Hidden;
            //if (EditingMode)
            //{
            //    ArgPoint ap = DataContext as ArgPoint;
            //    if (Drawing.IsInDrawingMode())
            //        finishDrawing.Visibility = Visibility.Visible;              
            //    else
            //        removeSketch.Visibility = Visibility.Visible; 
            //}
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            HandleRecontext();
        }

        private void DrawingDataContextHandled()
        {
            EnableDrawingControls();
        }

        public ArgPoint GetArgPoint()
        {
            ArgPoint p = (ArgPoint) DataContext;
            return p;
        }

        private void lstBxComments_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void btnAddComment_Click(object sender, RoutedEventArgs e)
        {
            /// addCommentRequest(true);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            /// addCommentRequest(false);
        }

        private void btnAddSource_Click(object sender, RoutedEventArgs e)
        {
            ArgPoint ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            DaoUtils.AddSource("New source", ap.Description);
        }

        #region drawing

        public void SaveDrawing()
        {
            //Drawing.SaveDrawing();
            EnableDrawingControls();
        }

        private void ResetSketch()
        {
            //Drawing.ResetSketch();
            EnableDrawingControls();
        }

        private void btnRemoveSketch_Click(object sender, RoutedEventArgs e)
        {
            ResetSketch();
        }

        private void btnFinishDrawing_Click(object sender, RoutedEventArgs e)
        {
            SaveDrawing();
        }

        #endregion

        private void ScrollContentPresenter_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //to ignore clicks inside SurafecScrollViewer
        }

        private void finishDrawing_Click(object sender, RoutedEventArgs e)
        {
            SaveDrawing();
        }

        #region media highlight

        private Brush mediaBg = null;

        private void lstBxAttachments_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            HighlightMediaPointDown();
        }

        private void lstBxAttachments_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            HighlightMediaPointUp();
        }

        private void HighlightMediaPointDown()
        {
            if (mediaBg != null)
                return;
            mediaBg = lstBxAttachments.Background.Clone();
            lstBxAttachments.Background = new SolidColorBrush(Colors.LightGreen);
        }

        private void HighlightMediaPointUp()
        {
            if (mediaBg == null)
                return;
            lstBxAttachments.Background = mediaBg;
            mediaBg = null;
        }

        private void lstBxAttachments_PreviewTouchDown_1(object sender, TouchEventArgs e)
        {
            HighlightMediaPointDown();
        }

        private void lstBxAttachments_PreviewTouchUp_1(object sender, TouchEventArgs e)
        {
            HighlightMediaPointUp();
        }

        #endregion media highlight

        #region comment highlight

        private Brush commentBg = null;

        private void HighlightCommentPointDown()
        {
            if (commentBg != null)
                return;
            commentBg = lstBxComments1.Background.Clone();
            lstBxComments1.Background = new SolidColorBrush(Colors.LightGreen);
        }

        private void HighlightCommentPointUp()
        {
            if (commentBg == null)
                return;
            lstBxComments1.Background = commentBg;
            commentBg = null;
        }

        private void lstBxComments_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            HighlightCommentPointDown();
        }

        private void lstBxComments_PreviewMouseUp_1(object sender, MouseButtonEventArgs e)
        {
            HighlightCommentPointUp();
        }

        private void lstBxComments_PreviewTouchDown_1(object sender, TouchEventArgs e)
        {
            HighlightCommentPointDown();
        }

        private void lstBxComments_PreviewTouchUp_1(object sender, TouchEventArgs e)
        {
            HighlightCommentPointUp();
        }

        #endregion comment highlight

        #region comments        

        private void btnComment_Click(object sender, RoutedEventArgs e)
        {
            btnSave_Click(null, null);
        }

        private void onCommentEditabilityChanged(bool edited)
        {
            _isEditingComment = edited;
        }

        private void placeholderFocus(Comment comment)
        {
            new VisualCommentsHelper(this.Dispatcher, lstBxComments1.ItemContainerGenerator, comment);
        }

        private void commentSave()
        {
            btnSave_Click(null, null); //focus lost matters
        }

        private void possibilityToClose()
        {
            if (MissedCloseRequest)
                badgeDoubleTap(null, null);
        }

        #endregion comments

        private void btnZoom_Click(object sender, RoutedEventArgs e)
        {
            badgeDoubleTap(sender, null);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            BusyWndSingleton.Show("Saving argument, please wait...");
            try
            {
                saveProcedure();
            }
            finally
            {
                BusyWndSingleton.Hide();
            }
        }

        private void saveProcedure()
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            if (!ap.ChangesPending)
                return;

            ap.ChangesPending = false;

            //save changes 
            try
            {
                DbCtx.Get().SaveChanges();
            }
            catch (Exception)
            {
            }

            if (_sharedClient != null)
            {
                _sharedClient.clienRt.SendStatsEvent(StEvent.BadgeEdited,
                                                     SessionInfo.Get().person.Id,
                                                     ap.Topic.Discussion.Id,
                                                     ap.Topic.Id,
                                                     DeviceType.Wpf);

                _sharedClient.clienRt.SendArgPointChanged(ap.Id, ap.Topic.Id);
            }
        }

        private void stkHeader_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Image)
                return;
            mediaDoubleClick.Click(sender, e);
            e.Handled = true;
        }

        private void stkHeader_PreviewTouchDown_1(object sender, TouchEventArgs e)
        {
            if (e.OriginalSource is Image)
                return;
            mediaDoubleClick.Click(sender, e);
            e.Handled = true;
        }

        private void LargeBadgeView_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                btnSave_Click(null,null);
            }
        }

        #region comment notificatinos

        readonly CommentDismissalRecognizer _commentDismissalRecognizer;

        private void UpdateLocalReadCounts(DiscCtx ctx, ArgPoint ap)
        {
            if (ap == null)
                return;

            SetNumUnreadComments(DaoUtils.NumCommentsUnreadBy(ctx, ap.Id));
        }

        void UpdateRemoteReadCounts(DiscCtx ctx, ArgPoint ap)
        {
            if (ap == null)
                return;

            largeBadgeView.Text = DaoUtils.RecentCommentReadBy(ctx, ap.Id);
        }

        private void SetNumUnreadComments(IEnumerable<NewCommentsFrom> newCommentBins)
        {
            notifications.ItemsSource = newCommentBins;
            lblComments.Content = CommentDismissalRecognizer.FormatNumUnreadComments(newCommentBins.Total());
        }

        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _commentDismissalRecognizer.CheckScrollState();
        }

        void OnDismiss(ArgPoint ap)
        {
            Console.Beep();

            CommentDismissalRecognizer.pushDismissal(ap, DbCtx.Get());
        }

        private void OnCommentRead(CommentsReadEvent ev)
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            if (ev.ArgPointId != ap.Id)
                return;

            if (ev.PersonId == SessionInfo.Get().person.Id)
            {
                //we are only interested in comment read callbacks from ourselves, to update local label 

                UpdateLocalReadCounts(new DiscCtx(ConfigManager.ConnStr), ap);
            }
            else
            {
                UpdateRemoteReadCounts(new DiscCtx(ConfigManager.ConnStr), ap);
            }
        }     
        #endregion
    }
}