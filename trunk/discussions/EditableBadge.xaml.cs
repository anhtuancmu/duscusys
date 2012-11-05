using System;
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
using CloudStorage;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Discussions.webkit_host;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using VE2;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for EditableBadge.xaml
    /// </summary>
    public partial class EditableBadge : UserControl
    {
        MultiClickRecognizer mediaDoubleClick;

        StorageWnd _storageWnd = null; 

        public EditableBadge()
        {
            InitializeComponent();

            //  RichTextBoxFormatBarManagerStatic.SetFormatBar(rtb, fmtBar);

            //  Drawing.dataContextHandled += DrawingDataContextHandled;

            mediaDoubleClick = new MultiClickRecognizer(MediaDoubleClick, null);
        }        

        public SurfaceScrollViewer MainScroller
        {
            set
            {
              ///  System.Windows.Interactivity.Interaction.GetBehaviors(mediaGrid).Add(new VerticallyUnscrollableInnerList(value, lstBxAttachments));
            }
        }

        private void UserControl_Initialized_1(object sender, EventArgs e)
        {
            //Dispatcher.BeginInvoke(
            //new Action(setMaxWidthOfDescription),
            //System.Windows.Threading.DispatcherPriority.Background);
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
                ///txtAttachmentURL.IsHitTestVisible = _editingMode;
                ///btnChooseFile.IsEnabled = _editingMode;
                ///btnAttachFromUrl.IsEnabled = _editingMode;
                ///btnAddSrc.IsEnabled = _editingMode;
                ///btnAttachScreenshot.IsEnabled = _editingMode;
                ///txtSource.IsHitTestVisible = _editingMode;
            }
        }

        public void Hide()
        {
            Opacity = 0;
        }

        public void HandleRecontext()
        {
            var ap = DataContext as ArgPoint;

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
            {
                EditingMode = false;
            }
            else
            {
                EditingMode = SessionInfo.Get().person.Id == ap.Person.Id;
            }

            //if there are no comments, add placeholder
            var ap1 = DataContext as ArgPoint;
            if (ap1 != null)
            {
                var commentAuthor = SessionInfo.Get().getPerson(DataContext);
                DaoUtils.EnsureCommentPlaceholderExists(DataContext as ArgPoint, commentAuthor);
            }

            if (ap != null)
                DaoUtils.RemoveDuplicateComments(ap);

            BeginSrcNumberInjection();
        }

        void Attach(object sender, ExecutedRoutedEventArgs args)
        {
            ArgPoint ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            if ((string)args.Parameter == "Remove selected")
            {
                Attachment a = lstBxAttachments.SelectedItem as Attachment;
                if (a != null)
                    ap.Attachment.Remove(a);
            }
            else
            {
                Attachment a = new Attachment();                
                ImageSource src = AttachmentManager.ProcessAttachCmd(ap, AttachCmd.ATTACH_IMG_OR_PDF, ref a);
                if(src!=null)
                    a.Person = getFreshCurrentPerson();
            }
        } 

        static Person getFreshCurrentPerson()
        {
            var p = SessionInfo.Get().person;
            if (p == null)
                return null;

            return Ctx2.Get().Person.FirstOrDefault(p0 => p0.Id == p.Id);
        }

        void SetStyle()
        {
            if (DataContext != null && DataContext is ArgPoint)
            {
                ArgPoint p = (ArgPoint)DataContext;
                //root.Background = new SolidColorBrush(Utils.IntToColor(p.Person.Color)); 
                lblColor.Fill = new SolidColorBrush(Utils.IntToColor(p.Person.Color));
                switch ((SideCode)p.SideCode)
                {
                    case SideCode.Pros:
                        stkHeader.Background = DiscussionColors.prosBrush;
                        break;
                    case SideCode.Cons:
                        stkHeader.Background = DiscussionColors.consBrush;
                        break;
                    case SideCode.Neutral:
                        stkHeader.Background = DiscussionColors.neutralBrush;
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

        void MediaDoubleClick(object sender, InputEventArgs e)
        {
            AttachmentManager.RunViewer(((FrameworkElement)sender).DataContext as Attachment);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mediaDoubleClick.Click(sender, e);
        }

        private void Image_TouchDown(object sender, TouchEventArgs e)
        {
            mediaDoubleClick.Click(sender, e);
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

        Comment addCommentRequest()
        {
            ArgPoint ap = DataContext as ArgPoint;
            if (ap == null)
                return null;

            var c = new DbModel.Comment();
            c.Text = "New comment";
            c.Person = SessionInfo.Get().getPerson(ap);
            ap.Comment.Add(c);
            return c;
        }

        private void lstBxComments_MouseLeave(object sender, MouseEventArgs e)
        {
        }

        private void btnAddComment_Click(object sender, RoutedEventArgs e)
        {
            addCommentRequest();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {           
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

        private void chooseImgClick(object sender, RoutedEventArgs e)
        {
            AttachFile(DataContext as ArgPoint, "Image");
        }

        //attachments from files
        void AttachFile(ArgPoint ap, string command)
        {
            if (ap == null)
                return;

            if (command == "Remove selected")
            {
                throw new NotSupportedException();
                //Attachment a = lstBxAttachments.SelectedItem as Attachment;
                //if (a != null)
                //{
                //    ap.Attachment.Remove(a);    
                //    a.Person = getFreshCurrentPerson();
                //    ap.ChangesPending = true;
                //}
            }
            else
            {
                Attachment a = new Attachment();
                ImageSource src = AttachmentManager.ProcessAttachCmd(ap, AttachCmd.ATTACH_IMG_OR_PDF, ref a);
                if (src != null)
                {
                    a.Person = getFreshCurrentPerson();
                    ap.ChangesPending = true;
                    UISharedRTClient.Instance.clienRt.SendStatsEvent(
                                         AttachmentToEvent(a, true), 
                                         ap.Person.Id,
                                         ap.Topic.Discussion.Id,
                                         ap.Topic.Id,
                                         DeviceType.Wpf);
                }
            }
        }

        //attachments from URL
        private void txtAttachmentURL_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;

            if (txtAttachmentURL.Text.Trim() == "")
                return;

            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            ap.RecentlyEnteredMediaUrl = txtAttachmentURL.Text;

            Attachment a = new Attachment();
            var imgSrc = AttachmentManager.ProcessAttachCmd(ap, txtAttachmentURL.Text, ref a);
            if (imgSrc != null)
            {
                a.Person = getFreshCurrentPerson();                
               
                ap.ChangesPending = true;
                UISharedRTClient.Instance.clienRt.SendStatsEvent(
                     AttachmentToEvent(a, false), 
                     ap.Person.Id,
                     ap.Topic.Discussion.Id,
                     ap.Topic.Id,
                     DeviceType.Wpf);
            }
        }

        //attachment from URL, with dialog box
        private void btnAttachFromUrl_Click_1(object sender, RoutedEventArgs e)
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            InpDialog dlg = new InpDialog();
            dlg.ShowDialog();
            string URL = dlg.Answer;
            if (URL == null)
                return;

            Attachment a = new Attachment();
            var imgSrc = AttachmentManager.ProcessAttachCmd(ap, URL, ref a);
            if (imgSrc != null)
            {
                a.Person = ap.Person;

                ap.ChangesPending = true;                
                UISharedRTClient.Instance.clienRt.SendStatsEvent(
                     AttachmentToEvent(a,false),
                     ap.Person.Id,
                     ap.Topic.Discussion.Id,
                     ap.Topic.Id,
                     DeviceType.Wpf);
            }
        }

        static StEvent AttachmentToEvent(Attachment at, bool local)
        {
            switch ((AttachmentFormat)at.Format)
            {
                case AttachmentFormat.Bmp:
                    return local ? StEvent.ImageAdded : StEvent.ImageUrlAdded;                    
                case AttachmentFormat.Jpg:
                    return local ? StEvent.ImageAdded : StEvent.ImageUrlAdded; 
                case AttachmentFormat.Png:
                    return local ? StEvent.ImageAdded : StEvent.ImageUrlAdded; 
                case AttachmentFormat.Pdf:
                    return local ? StEvent.PdfAdded : StEvent.PdfUrlAdded;
                case AttachmentFormat.PngScreenshot:
                    return StEvent.ScreenshotAdded;
                case AttachmentFormat.Youtube:
                    return StEvent.YoutubeAdded;
                default:
                    return StEvent.LocalIgnorableEvent;
            }
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
            commentBg = lstBxComments.Background.Clone();
            lstBxComments.Background = new SolidColorBrush(Colors.LightGreen);
        }

        private void HighlightCommentPointUp()
        {
            if (commentBg == null)
                return;
            lstBxComments.Background = commentBg;
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

        private void removeMedia_Click(object sender, RoutedEventArgs e)
        {
            var at = ((ContentControl)sender).DataContext as Attachment;
            var ap = DataContext as ArgPoint;

            ap.Attachment.Remove(at);
            
            var mediaData = at.MediaData;
            at.MediaData = null;
            if (mediaData!=null)
                Ctx2.Get().DeleteObject(mediaData);
            Ctx2.Get().DeleteObject(at);
           
            ap.ChangesPending = true;
            UISharedRTClient.Instance.clienRt.SendStatsEvent(
                                 StEvent.MediaRemoved,
                                 ap.Person.Id,
                                 ap.Topic.Discussion.Id,
                                 ap.Topic.Id,
                                 DeviceType.Wpf);
        }

        Comment newComment = null;
        private void btnComment_Click(object sender, RoutedEventArgs e)
        {            
            var ap1 = DataContext as ArgPoint;
            if (ap1 != null)
            {
                var commentAuthor = SessionInfo.Get().getPerson(DataContext);
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
            var newItem = lstBxComments.ItemContainerGenerator.ContainerFromItem(newComment);
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

        private void btnAddSrc_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext == null)
                return;
            
            var ap=(ArgPoint)DataContext;

            DaoUtils.AddSource(txtSource.Text, ap.Description);
            ap.ChangesPending = true;
            UISharedRTClient.Instance.clienRt.SendStatsEvent(
                                StEvent.SourceAdded,
                                ap.Person.Id,
                                ap.Topic.Discussion.Id,
                                ap.Topic.Id,
                                DeviceType.Wpf);

            BeginSrcNumberInjection();
        }

        private void txtSource_KeyDown_1(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                btnAddSrc_Click(null, null);
        }

        private void btnAttachScreenshot_Click_1(object sender, RoutedEventArgs e)
        {
            var ap = DataContext as ArgPoint;            
            if (ap == null)
                return;
           
            var screenshotWnd = new ScreenshotCaptureWnd((System.Drawing.Bitmap b) => 
            { 
                var attach = AttachmentManager.AttachScreenshot(ap, b);
                if (attach != null)
                {
                    var seldId = SessionInfo.Get().person.Id;
                    attach.Person = Ctx2.Get().Person.FirstOrDefault(p0 => p0.Id == seldId);
                    
                    ap.ChangesPending = true;
                    UISharedRTClient.Instance.clienRt.SendStatsEvent(
                                          StEvent.ScreenshotAdded,
                                          ap.Person.Id,
                                          ap.Topic.Discussion.Id,
                                          ap.Topic.Id,
                                          DeviceType.Wpf);
                }
            });
            screenshotWnd.ShowDialog();
        }

        private void lstBxAttachments_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var a = lstBxAttachments.SelectedItem as Attachment;
            if (a == null)
                return;

            if (a.Link != null)
                txtAttachmentURL.Text = a.Link;
        }

        void onSourceRemoved(object sender, RoutedEventArgs e)
        {
            BeginSrcNumberInjection();

            //report event 
            var ap = (ArgPoint)DataContext;
            ap.ChangesPending = true;
            UISharedRTClient.Instance.clienRt.SendStatsEvent(StEvent.SourceRemoved,
                                                             ap.Person.Id,
                                                             ap.Topic.Discussion.Id,
                                                             ap.Topic.Id,
                                                             DeviceType.Wpf);
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
                if(srcUC!=null)
                    srcUC.SrcNumber = i + 1;                
            }
        }

        private void plainDescription_KeyDown_1(object sender, KeyEventArgs e)
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            ap.ChangesPending = true;
        }

        #region cloud storage 

        void RunCloudStorageViewer(StorageType storageType)
        {
            if (_storageWnd != null)
            {
                _storageWnd.Activate();
                return;
            }

            _storageWnd = new StorageWnd();
            _storageWnd.Show();
            _storageWnd.webViewCallback  += onWebViewerRequest;
            _storageWnd.fileViewCallback += onCloudViewerRequest;
            _storageWnd.Closed += onStorageWndClosed;
            _storageWnd.LoginAndEnumFiles(storageType);
        }

        void onWebViewerRequest(string Uri)
        {
            var browser = new WebKitFrm(Uri);
            browser.ShowDialog();
        }

        void onStorageWndClosed(object sender, EventArgs e)
        {
            ArgPoint ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            if (_storageWnd.filesToAttach!=null)
                foreach (CloudStorage.StorageWnd.StorageSelectionEntry entry in _storageWnd.filesToAttach)
                {
                    try
                    {
                        var attach = AttachmentManager.AttachCloudEntry(ap, entry);
                        var seldId = SessionInfo.Get().person.Id;
                        attach.Person = Ctx2.Get().Person.FirstOrDefault(p0 => p0.Id == seldId);

                        ap.ChangesPending = true;

                        StEvent ev = StEvent.ArgPointTopicChanged;
                        switch ((AttachmentFormat)attach.Format)
                        {
                            case AttachmentFormat.Bmp:
                            case AttachmentFormat.Jpg:
                            case AttachmentFormat.Png:
                                ev = StEvent.ImageAdded;
                                break;
                            case AttachmentFormat.Pdf:
                                ev = StEvent.PdfAdded;
                                break;
                        }
                        if (ev != StEvent.ArgPointTopicChanged)
                        {
                            UISharedRTClient.Instance.clienRt.SendStatsEvent(
                                                        ev,
                                                        ap.Person.Id,
                                                        ap.Topic.Discussion.Id,
                                                        ap.Topic.Id,
                                                        DeviceType.Wpf);
                        }
                    }
                    catch (Discussions.AttachmentManager.IncorrectAttachmentFormat)
                    {
                    }
                }

            _storageWnd.fileViewCallback -= onCloudViewerRequest;
            _storageWnd.Closed -= onStorageWndClosed;
            _storageWnd = null;
        }

        void onCloudViewerRequest(string pathName)
        {
            AttachmentManager.RunViewer(pathName);
        }

        private void chooseDropboxFiles(object sender, RoutedEventArgs e)
        {
            RunCloudStorageViewer(StorageType.Dropbox);            
        }

        private void chooseGDriveFiles(object sender, RoutedEventArgs e)
        {
            RunCloudStorageViewer(StorageType.GoogleDrive);
        }

        #endregion

        private void lstBxAttachments_PreviewMouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            var scrollViwer = GetScrollViewer(lstBxAttachments) as ScrollViewer;

	        if (scrollViwer != null)
	        {
                var hOffset = -30.0;
                if(e.Delta<0)
                    hOffset *= -1.0;   
             
                scrollViwer.ScrollToHorizontalOffset(scrollViwer.HorizontalOffset + hOffset);                
	        }
            e.Handled = true;
        }

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }
            return null;
        }
    }
}
