﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using Discussions.model;
using Discussions.DbModel;
using Discussions.rt;
using LoginEngine;
using Discussions.RTModel.Model;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for EditableBadge.xaml
    /// </summary>
    public partial class LargeBadgeView : UserControl
    {
        MultiClickRecognizer mediaDoubleClick;

        UISharedRTClient _sharedClient;

        public Action CloseRequest = null;

        //large badge view was open, explanation mode was enabled, another client closed badge, but badge
        //on current client wasn't closed as a comment was being edited on local client. this flag remembers 
        //missed close event.
        public bool MissedCloseRequest = false;

        public static readonly RoutedEvent RequestSmallViewEvent = EventManager.RegisterRoutedEvent(
         "RequestSmallView", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LargeBadgeView));

        public event RoutedEventHandler RequestSmallView
        {
            add { AddHandler(RequestSmallViewEvent, value); }
            remove { RemoveHandler(RequestSmallViewEvent, value); }
        }

        ObservableCollection<Source> sources = new ObservableCollection<Source>();
        public ObservableCollection<Source> Sources
        {
            get
            {
                return sources;
            }
        }

        ObservableCollection<Attachment> attachments = new ObservableCollection<Attachment>();
        public ObservableCollection<Attachment> Attachments
        {
            get
            {
                return attachments;
            }
        }

        //if source order or data context changes, we update 
        void UpdateOrderedSources()
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

        void UpdateOrderedMedia()
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

        void BeginAttachmentNumberInjection()
        {
            Dispatcher.BeginInvoke(new Action(_injectMediaNumbers), System.Windows.Threading.DispatcherPriority.Background, null);
        }

        void _injectMediaNumbers()
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
        bool _isEditingComment = false;
        public bool IsEditingComment
        {
            get
            {
                return _isEditingComment;
            }
        }

        public LargeBadgeView()
        {
            InitializeComponent();

            //  RichTextBoxFormatBarManagerStatic.SetFormatBar(rtb, fmtBar);

            //  Drawing.dataContextHandled += DrawingDataContextHandled; 

            mediaDoubleClick = new MultiClickRecognizer(badgeDoubleTap, null);

            lstBxSources.DataContext = this;
            lstBxAttachments.DataContext = this;

            BeginAttachmentNumberInjection();         
        }

        public void SetRt(UISharedRTClient sharedClient)
        {
            _sharedClient = sharedClient;
            SetListeners(true);
        }

        void badgeDoubleTap(object sender, InputEventArgs e)
        {
            SetListeners(false);

            RaiseEvent(new RoutedEventArgs(RequestSmallViewEvent));
            //if (CloseRequest != null)
            //    CloseRequest();
        }

        void SetListeners(bool doSet)
        {
            if (doSet)
                _sharedClient.clienRt.argPointChanged += ArgPointChanged;
            else
                _sharedClient.clienRt.argPointChanged -= ArgPointChanged;
        }

        void ArgPointChanged(int ArgPointId, int topicId, PointChangedType change)
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

            DataContext = null;
            DataContext = ap2;
        }

        void BeginSrcNumberInjection()
        {
            Dispatcher.BeginInvoke(new Action(_injectSourceNumbers), System.Windows.Threading.DispatcherPriority.Background, null);
        }

        void _injectSourceNumbers()
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

        void setMaxWidthOfDescription()
        {
            plainDescription.MaxWidth = this.ActualWidth - 10;
        }

        void commentEdit(object sender, CommentRoutedEventArgs e)
        {
            var ownerId = SessionInfo.Get().person.Id;
            var commentAuthor = DbCtx.Get().Person.FirstOrDefault(p0 => p0.Id == ownerId);
            //var commentId = e.Comment.Id;
            //var comment = DbCtx.Get().Comment.FirstOrDefault(c0 => c0.Id == commentId);
            DaoUtils.InjectAuthorOfComment(e.Comment, commentAuthor);

            if (e.RequiresDataRecontext)
            {
                //trigger data context to force author refresh 
                e.CommentControl.DataContext = null;
                e.CommentControl.DataContext = e.Comment;
            }
        }

        bool _editingMode = false;
        public bool EditingMode
        {
            get
            {
                return _editingMode;
            }
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

            if (DataContext == null)
                EditingMode = false;
            else
            {
                var ap = (ArgPoint)DataContext;
                EditingMode = SessionInfo.Get().person.Id == ap.Person.Id;
            }

            btnComment_Click(null, null);

            BeginSrcNumberInjection();
            UpdateOrderedSources();
            BeginAttachmentNumberInjection();
            UpdateOrderedMedia();

            ///commentsViewer.ScrollToBottom();
        }

        void SetStyle()
        {
            if (DataContext != null && DataContext is ArgPoint)
            {
                ArgPoint p = (ArgPoint)DataContext;
                lblPerson.Background = new SolidColorBrush(Utils.IntToColor(p.Person.Color));
                switch ((SideCode)p.SideCode)
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

        void disableAll()
        {
            //removeSketch.Visibility = Visibility.Hidden;
            //finishDrawing.Visibility = Visibility.Hidden;
        }

        void EnableDrawingControls()
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

        void DrawingDataContextHandled()
        {
            EnableDrawingControls();
        }

        public ArgPoint GetArgPoint()
        {
            ArgPoint p = (ArgPoint)DataContext;
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

        void ResetSketch()
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
        Brush mediaBg = null;
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
        Brush commentBg = null;
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
        Comment newComment = null;
        private void btnComment_Click(object sender, RoutedEventArgs e)
        {
            var ap1 = DataContext as ArgPoint;            
            if (ap1 != null)
            {
                var ownerId = SessionInfo.Get().person.Id;
                var commentAuthor = DbCtx.Get().Person.FirstOrDefault(p0 => p0.Id == ownerId);
                newComment = DaoUtils.EnsureCommentPlaceholderExists(DataContext as ArgPoint, commentAuthor);
                if (newComment != null)
                {
                    Dispatcher.BeginInvoke(new Action(DeferredFocusSet),
                                            System.Windows.Threading.DispatcherPriority.Background, null);

                    var ap = (ArgPoint)DataContext;
                    ap.ChangesPending = true;
                    UISharedRTClient.Instance.clienRt.SendStatsEvent(
                                               StEvent.CommentAdded,
                                               SessionInfo.Get().person.Id,
                                               ap.Topic.Discussion.Id,
                                               ap.Topic.Id,
                                               DeviceType.Wpf);
                }
            }
        }

        void DeferredFocusSet()
        {
            var newItem = lstBxComments1.ItemContainerGenerator.ContainerFromItem(newComment);
            var commentText = Utils.FindChild<SurfaceTextBox>(newItem);
            if (commentText != null)
                commentText.Focus();
        }

        void onCommentEnd(object sender, CommentRoutedEventArgs e)
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            commentEdit(null, e);       //inject author

            bool needsEvent = false;
            if (DaoUtils.needCommentPlaceholder(ap))
            {
                btnComment_Click(null, null);//add new placeholder and focus it  
                needsEvent = true;
            }

            if (newComment != null)
                needsEvent = true;

            if (needsEvent)
            {
            }
        }

        void onCommentDelete(object sender, RoutedEventArgs e)
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            ap.ChangesPending = true;
            UISharedRTClient.Instance.clienRt.SendStatsEvent(
                                StEvent.CommentRemoved,
                                ap.Person.Id,
                                ap.Topic.Discussion.Id,
                                ap.Topic.Id,
                                DeviceType.Wpf);
        }

        void onCommentEditabilityChanged(object sender, CommentEditabilityChanged e)
        {
            _isEditingComment = e.IsBeingEdited;
            dbgText.Text = "editability " + e.IsBeingEdited;

            if(!e.IsBeingEdited && MissedCloseRequest)
                badgeDoubleTap(sender, null);
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
        
        void saveProcedure()
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

            _sharedClient.clienRt.SendStatsEvent(StEvent.BadgeEdited,
                                                SessionInfo.Get().person.Id,
                                                ap.Topic.Discussion.Id,
                                                ap.Topic.Id,
                                                DeviceType.Wpf);

            _sharedClient.clienRt.SendArgPointChanged(ap.Id, ap.Topic.Id);

            //update locally 
            ///ArgPointChanged(ap.Id, ap.Topic.Id, PointChangedType.Modified);
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
    }
}
