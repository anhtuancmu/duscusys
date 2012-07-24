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

namespace Discussions
{
    /// <summary>
    /// Interaction logic for EditableBadge.xaml
    /// </summary>
    public partial class LargeBadgeView : UserControl
    {
        public Action closeRequest = null;

        MultiClickRecognizer mediaDoubleClick;

        UISharedRTClient _sharedClient;

        public static readonly RoutedEvent RequestSmallViewEvent = EventManager.RegisterRoutedEvent(
         "RequestSmallView", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LargeBadgeView));

        public event RoutedEventHandler RequestSmallView
        {
            add { AddHandler(RequestSmallViewEvent, value); }
            remove { RemoveHandler(RequestSmallViewEvent, value); }
        }

        public LargeBadgeView()
        {
            InitializeComponent();

            //  RichTextBoxFormatBarManagerStatic.SetFormatBar(rtb, fmtBar);

            //  Drawing.dataContextHandled += DrawingDataContextHandled; 

            mediaDoubleClick = new MultiClickRecognizer(badgeDoubleTap, null);
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
           // if (closeRequest != null)
           //     closeRequest();
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
            if (change == PointChangedType.Modified)
            {
                //using db ctx here from login engine, not to spoil others
                DbCtx.DropContext();
                var ap = DbCtx.Get().ArgPoint.FirstOrDefault(p0 => p0.Id == ArgPointId);

                DataContext = null;
                DataContext = ap;
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
            var commentAuthor = SessionInfo.Get().getPerson(DataContext);
            ///DaoUtils.EnsureCommentPlaceholderExists(DataContext as ArgPoint, commentAuthor);
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

            commentsViewer.ScrollToBottom();
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

        //Comment addCommentRequest(bool add)
        //{
        //    ArgPoint ap = DataContext as ArgPoint;
        //    if (ap == null)
        //        return null;

        //    if (add)
        //    {
        //        var c = new DbModel.Comment();
        //        c.Text = "New comment";

        //        c.Person = SessionInfo.Get().getPerson(ap);
        //        ap.Comment.Add(c);

        //        return c;
        //    }
        //    else
        //    {
        //        return null;

        //        //var c = lstBxComments.SelectedItem as Comment;
        //        //if (c == null)
        //        //    return;

        //        //ap.Comment.Remove(c);
        //        //CtxSingleton.Get().SaveChanges(); 
        //        //this.RaiseEvent(new RoutedEventArgs(DetailedArgPointBadge.CommentsEvent, this));
        //    }
        //}

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

       /// Comment newComment = null;
        private void btnComment_Click(object sender, RoutedEventArgs e)
        {
            //newComment = addCommentRequest(true);
            //if (newComment != null)
            //{
            //    Dispatcher.BeginInvoke(new Action(DefferedFocusSet), System.Windows.Threading.DispatcherPriority.Background, null);
            //}
        }

        //void DefferedFocusSet()
        //{
        //    var newItem = lstBxComments1.ItemContainerGenerator.ContainerFromItem(newComment);
        //    var commentText = Utils.FindChild<SurfaceTextBox>(newItem);
        //    if (commentText != null)
        //        commentText.Focus();
        //}

        private void btnZoom_Click(object sender, RoutedEventArgs e)
        {
            badgeDoubleTap(sender,null);
        }

        private void Border_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Image)
                return;
            mediaDoubleClick.Click(sender, e);
        }

        private void Border_PreviewTouchDown_1(object sender, TouchEventArgs e)
        {
            if (e.OriginalSource is Image)
                return;
            mediaDoubleClick.Click(sender, e);
        }
     
    }
}
