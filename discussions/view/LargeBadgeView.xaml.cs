using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Discussions.DbModel;
using Discussions.DbModel.model;
using Discussions.model;
using Discussions.pdf_reader;
using Discussions.rt;
using Discussions.RTModel.Model;
using LoginEngine;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for EditableBadge.xaml
    /// </summary>
    public partial class LargeBadgeView : UserControl
    {
        private MultiClickRecognizer _mediaDoubleClick;

        private UISharedRTClient _sharedClient;

        private Random _rnd;

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

        private readonly ObservableCollection<Source> sources = new ObservableCollection<Source>();

        public ObservableCollection<Source> Sources
        {
            get { return sources; }
        }

        private readonly ObservableCollection<Attachment> attachments = new ObservableCollection<Attachment>();

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

            _rnd = new Random();

            _mediaDoubleClick = new MultiClickRecognizer(badgeDoubleTap, null);

            _commentDismissalRecognizer = new CommentDismissalRecognizer(scrollViewer, OnDismiss);

            txtNewComment.Text = DaoUtils.NEW_COMMENT;      

            lstBxSources.DataContext = this;
            lstBxAttachments.DataContext = this;

            BeginAttachmentNumberInjection();
        }

        public void SetRt(UISharedRTClient sharedClient)
        {
            _sharedClient = sharedClient;
            SetListeners(true);
        }

        public void Close()
        {
            badgeDoubleTap(null, null);
        }

        private void badgeDoubleTap(object sender, InputEventArgs e)
        {
            SetListeners(false);

            _stopBot = true;

            RaiseEvent(new RoutedEventArgs(RequestSmallViewEvent));
            //if (CloseRequest != null)
            //    CloseRequest();

            DataContext = null;
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

        private void ArgPointChanged(int ArgPointId, int topicId, PointChangedType change, int personId)
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            if (ArgPointId != ap.Id)
                return; //not our point

            if (change != PointChangedType.Modified)
                return;

            //save edited comment
            //string editedCommentText = null;
            //var editedComment = ap.Comment.FirstOrDefault(c => c.Person == null && 
            //                                                   c.Text != DaoUtils.NEW_COMMENT);
            //if (editedComment != null)
            //    editedCommentText = editedComment.Text;

            //using db ctx here from login engine, not to spoil others
            DbCtx.DropContext();
            var ap2 = DbCtx.Get().ArgPoint.FirstOrDefault(p0 => p0.Id == ArgPointId);            

            //BadgesCtx.DropContext();

            DataContext = null;
           
            //restore edited comment 
            //if (!string.IsNullOrWhiteSpace(editedCommentText))
            //{
            //    var placeholder = ap2.Comment.FirstOrDefault(c => c.Person == null);
            //    if (placeholder != null)
            //    {
            //        placeholder.Text = editedCommentText;
            //        placeholder.Person = null;

            //        ////restore focus
            //        Dispatcher.BeginInvoke(
            //            DispatcherPriority.Background,
            //            (Action) (() => FocusCommentTextBox(placeholder))
            //            );
            //    }
            //}

            DataContext = ap2;

            scrollViewer.ScrollToBottom();
        }

        void FocusCommentTextBox(Comment comment)
        {
            DependencyObject editedContainer =
                        lstBxComments1.ItemContainerGenerator.ContainerFromItem(comment);
            if (editedContainer != null)
            {
                var commentTextbox = Utils.FindChild<TextBox>(editedContainer);
                if (commentTextbox != null)
                    commentTextbox.Focus();
            }
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

            Opacity = DataContext == null ? 0 : 1;

            //Drawing.HandleRecontext();

            var ap = DataContext as ArgPoint;
            _commentDismissalRecognizer.Reset(ap);

            UpdateLocalReadCounts(DbCtx.Get(), ap);
            new CommentNotificationDeferral(Dispatcher, DbCtx.Get(), lstBxComments1);
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

        //private void onCommentEditabilityChanged(bool edited)
        //{
        //    _isEditingComment = edited;
        //    SaveProcedure();
        //}

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
                SaveProcedure();
            }
            finally
            {
                BusyWndSingleton.Hide();
            }
        }

        private string _lastCommentSubmitted;
        private DateTime _lastSave;
        void SaveProcedure()
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            if (DateTime.Now.Subtract(_lastSave).TotalMilliseconds < 100)
                return;

            _lastSave = DateTime.Now;

            //finalize edited comment
            if (!string.IsNullOrWhiteSpace(txtNewComment.Text) && 
                DaoUtils.NEW_COMMENT != txtNewComment.Text && 
                txtNewComment.Text!=_lastCommentSubmitted)
            {
                DaoUtils.HandleCommentCommit(txtNewComment.Text, ap);
                _lastCommentSubmitted = txtNewComment.Text;
                //txtNewComment.Text = DaoUtils.NEW_COMMENT;
                txtNewComment.Text = "";
            }

            if (!ap.ChangesPending)
                return;

            ap.ChangesPending = false;

            //save changes 
            try
            {
                DbCtx.Get().SaveChanges();
            }
            catch
            {
            }

            if (_sharedClient != null)
            {
                _sharedClient.clienRt.SendStatsEvent(StEvent.BadgeEdited,
                                                     SessionInfo.Get().person.Id,
                                                     ap.Topic.Discussion.Id,
                                                     ap.Topic.Id,
                                                     DeviceType.Wpf);

                _sharedClient.clienRt.SendArgPointChanged(ap.Id, ap.Topic.Id, SessionInfo.Get().person.Id);
            }
        }

        private void stkHeader_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Image)
                return;
            _mediaDoubleClick.Click(sender, e);
            e.Handled = true;
        }

        private void stkHeader_PreviewTouchDown_1(object sender, TouchEventArgs e)
        {
            if (e.OriginalSource is Image)
                return;
            _mediaDoubleClick.Click(sender, e);
            e.Handled = true;
        }

        private void LargeBadgeView_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                btnSave_Click(null,null);
            }
            else if (e.Key == Key.Return)
            {
                btnSave_Click(null, null);
            }
        }

        #region comment notifications

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

            txtCommentSeenBy.Text = DaoUtils.RecentCommentReadBy(ctx, ap.Id);
        }

        private void SetNumUnreadComments(List<NewCommentsFrom> newCommentBins)
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
            //Console.Beep();

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
                var ctx = new DiscCtx(ConfigManager.ConnStr);
                UpdateLocalReadCounts(ctx, ap);
                new CommentNotificationDeferral(Dispatcher, ctx, lstBxComments1);
            }
            else
            {
                UpdateRemoteReadCounts(new DiscCtx(ConfigManager.ConnStr), ap);
            }
        }     
        #endregion

        private void SetMaxWidthOfDescription()
        {
            textArea.MaxWidth = this.ActualWidth - 60;
        }

        private void LargeBadgeView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetMaxWidthOfDescription();
        }

        private void TxtNewComment_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                scrollViewer.ScrollToBottom();
            }
        }

        private void CommentUC_OnCommentRemovedEvent(Comment c)
        {
            SaveProcedure();
        }

        private void TxtNewComment_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (txtNewComment.Text == DaoUtils.NEW_COMMENT)
                txtNewComment.Text = "";
        }

        private void TxtNewComment_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewComment.Text))
                txtNewComment.Text = DaoUtils.NEW_COMMENT;
        }

        private async void BtnRunBot_OnClick(object sender, RoutedEventArgs e)
        {
            await BotRunAsync();
        }

        public async Task<Tuple<WebkitBrowserWindow, ImageWindow, ReaderWindow2>> 
            BotLaunchRandomAttachmentAsync(Random rnd)
        {
            var ap = ((ArgPoint)DataContext);
            if (ap.Attachment.Count < 0)
                return new Tuple<WebkitBrowserWindow, ImageWindow, ReaderWindow2>(null, null, null);

            int i = rnd.Next(ap.Attachment.Count);
            await Utils.DelayAsync(100);
            DependencyObject container = lstBxAttachments.ItemContainerGenerator
                                                        .ContainerFromIndex(i);

            var imageUc = Utils.FindChild<LargeImageUC>(container);
            if (imageUc != null)
            {
                var resultWnd = imageUc.BotLaunch();
                return new Tuple<WebkitBrowserWindow, ImageWindow, ReaderWindow2>(
                    null, 
                    resultWnd as ImageWindow,
                    resultWnd as ReaderWindow2);
            }

            var videoUc = Utils.FindChild<LargeVideoUC>(container);
            if (videoUc != null)
            {
                videoUc.BotLaunch();
                return new Tuple<WebkitBrowserWindow, ImageWindow, ReaderWindow2>(null, null, null);
            }

            return new Tuple<WebkitBrowserWindow, ImageWindow, ReaderWindow2>(null, null, null);
        }

        public async Task<WebkitBrowserWindow> BotLaunchRandomSource(Random rnd)
        {
            var ap = ((ArgPoint)DataContext);
            if (ap.Description.Source.Count > 0)
            {
                int i = rnd.Next(ap.Description.Source.Count);
                await Utils.DelayAsync(100);
                DependencyObject container = lstBxSources.ItemContainerGenerator
                                                         .ContainerFromIndex(i);
                var src = Utils.FindChild<SourceUC>(container);
                if (src != null)
                {
                    return src.Launch();
                }
            }
            return null;
        }

        private bool _stopBot;
        async Task BotRunAsync()
        {
            while (true)
            {
                if (_stopBot)
                    break;
                BotGenerateCommentChange();
                await Utils.DelayAsync(200);
            }
        }

        public void BotGenerateCommentChange()
        {
            var ap = (ArgPoint)DataContext;
            if (ap.Comment.Count > 10 || _rnd.Next(100) < 50)
                BotRemoveComment();
            else
                BotCreateBotComment();
        }

        public void BotCreateBotComment()
        {
            txtNewComment.Text = "Bot comment" + _rnd.Next();
            SaveProcedure();
        }

        public void BotRemoveComment()
        {
            var ap = (ArgPoint)DataContext;
            int commentIdxToRemove = _rnd.Next(ap.Comment.Count);
            DependencyObject container = lstBxComments1
                .ItemContainerGenerator.ContainerFromIndex(commentIdxToRemove);
            if (container == null)
                return;

            var commentUc = Utils.FindChild<CommentUC>(container);
            if (commentUc != null)
                commentUc.btnRemoveComment_Click(null, null);
        }
    }
}