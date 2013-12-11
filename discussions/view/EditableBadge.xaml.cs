using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CloudStorage;
using CloudStorage.Model;
using Discussions.ctx;
using Discussions.DbModel;
using Discussions.DbModel.model;
using Discussions.model;
using Discussions.rt;
using Discussions.RTModel.Model;
using Discussions.util;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for EditableBadge.xaml
    /// </summary>
    public partial class EditableBadge : UserControl
    {
        private readonly MultiClickRecognizer _mediaDoubleClick;

        private StorageWnd _storageWnd = null;

        private readonly ObservableCollection<Source> _sources = new ObservableCollection<Source>();

        public ObservableCollection<Source> Sources
        {
            get { return _sources; }
        }

        private readonly ObservableCollection<Attachment> _attachments = new ObservableCollection<Attachment>();

        public ObservableCollection<Attachment> Attachments
        {
            get { return _attachments; }
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

        private readonly DispatcherTimer _selectAllTimer;

        public EditableBadge()
        {
            InitializeComponent();

            //  RichTextBoxFormatBarManagerStatic.SetFormatBar(rtb, fmtBar);

            //  Drawing.dataContextHandled += DrawingDataContextHandled;

            _mediaDoubleClick = new MultiClickRecognizer(MediaDoubleClick, null);

            lstBxSources.DataContext = this;
            lstBxAttachments.DataContext = this;

            srcMover = new SourceMover(srcRepositionPopup);
            mediaMover = new MediaMover(mediaRepositionPopup);

            _selectAllTimer = new DispatcherTimer();
            _selectAllTimer.Tick += _selectAllTimer_Tick;
            _selectAllTimer.Interval = TimeSpan.FromMilliseconds(500);
            _selectAllTimer.Start();
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

        private void placeholderFocus(Comment comment)
        {
            new VisualCommentsHelper(this.Dispatcher, lstBxComments.ItemContainerGenerator, comment);
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
                txtAttachmentURL.IsHitTestVisible = _editingMode;
                ///btnChooseFile.IsEnabled = _editingMode;
                btnAttachMedia.IsEnabled = _editingMode;
                btnAddSrc.IsEnabled = _editingMode;
                btnAttachScreenshot.IsEnabled = _editingMode;
                txtSource.IsHitTestVisible = _editingMode;
            }
        }

        public void Hide()
        {
            Opacity = 0;
        }

        public void RemoveFocusFromInputControls()
        {
            //remove focus from other controls
            focusInterceptor.Focus();
        }

        void HandleRecontext()
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
                if(ap!=null)
                    EditingMode = SessionInfo.Get().person.Id == ap.Person.Id;
            }

            UpdateOrderedSources();
            UpdateOrderedMedia();
            BeginSrcNumberInjection();
            BeginAttachmentNumberInjection();

            if (ap != null)
            {
                UpdateLocalReadCounts(PrivateCenterCtx.Get(), ap);
                UpdateRemoteReadCounts(PrivateCenterCtx.Get(), ap);
                new CommentNotificationDeferral(Dispatcher, PrivateCenterCtx.Get(), lstBxComments);
            }

            if (ap != null)
                DaoUtils.RemoveDuplicateComments(ap);

            if (CommentDismissalRecognizer != null)
            {
                CommentDismissalRecognizer.Reset(DataContext as ArgPoint);
                CommentDismissalRecognizer.CheckScrollState();
            }
        }

        private void Attach(object sender, ExecutedRoutedEventArgs args)
        {
            ArgPoint ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            //if ((string)args.Parameter == "Remove selected")
            //{
            //    Attachment a = lstBxAttachments.SelectedItem as Attachment;
            //    if (a != null)
            //        ap.Attachment.Remove(a);
            //}
            //else
            {
                Attachment a = new Attachment();
                ImageSource src = AttachmentManager.ProcessAttachCmd(ap, AttachCmd.ATTACH_IMG_OR_PDF, ref a);
                if (src != null)
                    a.Person = getFreshCurrentPerson();
            }
        }

        private static Person getFreshCurrentPerson()
        {
            var p = SessionInfo.Get().person;
            if (p == null)
                return null;

            return PrivateCenterCtx.Get().Person.FirstOrDefault(p0 => p0.Id == p.Id);
        }

        private void SetStyle()
        {
            if (DataContext != null && DataContext is ArgPoint)
            {
                ArgPoint p = (ArgPoint) DataContext;
                //root.Background = new SolidColorBrush(Utils.IntToColor(p.Person.Color)); 
                lblColor.Fill = new SolidColorBrush(Utils.IntToColor(p.Person.Color));
                switch ((SideCode) p.SideCode)
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

        private void MediaDoubleClick(object sender, InputEventArgs e)
        {
            AttachmentManager.RunViewer(((FrameworkElement) sender).DataContext as Attachment);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mediaDoubleClick.Click(sender, e);
        }

        private void Image_TouchDown(object sender, TouchEventArgs e)
        {
            _mediaDoubleClick.Click(sender, e);
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

        private Comment addCommentRequest()
        {
            ArgPoint ap = DataContext as ArgPoint;
            if (ap == null)
                return null;

            var c = new DbModel.Comment();
            c.Text = DaoUtils.NEW_COMMENT;
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
            UpdateOrderedSources();
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

        private void chooseImgClick(object sender, RoutedEventArgs e)
        {
            AttachFile(DataContext as ArgPoint, "Image");
        }

        //attachments from files
        private void AttachFile(ArgPoint ap, string command)
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
                    UpdateOrderedMedia();
                    BeginAttachmentNumberInjection();
                }
            }
        }

        //attachments from URL
        private void txtAttachmentURL_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;

            AttachMediaFromUrl();
        }

        private void AttachMediaFromUrl()
        {
            string url = txtAttachmentURL.Text.Trim();

            if (url == "")
                return;

            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            if (ap.Attachment.Any(at => at.Link == url))
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
                UpdateOrderedMedia();
                BeginAttachmentNumberInjection();
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
                    AttachmentToEvent(a, false),
                    ap.Person.Id,
                    ap.Topic.Discussion.Id,
                    ap.Topic.Id,
                    DeviceType.Wpf);
                UpdateOrderedMedia();
                BeginAttachmentNumberInjection();
            }
        }

        private static StEvent AttachmentToEvent(Attachment at, bool local)
        {
            switch ((AttachmentFormat) at.Format)
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
            var at = ((ContentControl) sender).DataContext as Attachment;
            var ap = DataContext as ArgPoint;

            ap.Attachment.Remove(at);

            var mediaData = at.MediaData;
            at.MediaData = null;
            if (mediaData != null)
                PrivateCenterCtx.Get().DeleteObject(mediaData);
            PrivateCenterCtx.Get().DeleteObject(at);

            ap.ChangesPending = true;
            UISharedRTClient.Instance.clienRt.SendStatsEvent(
                StEvent.MediaRemoved,
                ap.Person.Id,
                ap.Topic.Discussion.Id,
                ap.Topic.Id,
                DeviceType.Wpf);
            UpdateOrderedMedia();
            BeginAttachmentNumberInjection();
        }

        private void btnComment_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnAddSrc_Click(object sender, RoutedEventArgs e)
        {
            AttachSource();
        }

        void AttachSource()
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            string url = txtSource.Text.Trim();

            if (ap.Description.Source.FirstOrDefault(s => s.Text == url) != null)
                return;

            if (Validators.UriOk(url))
            {
                DaoUtils.AddSource(txtSource.Text, ap.Description);
                ap.ChangesPending = true;
                UISharedRTClient.Instance.clienRt.SendStatsEvent(
                    StEvent.SourceAdded,
                    ap.Person.Id,
                    ap.Topic.Discussion.Id,
                    ap.Topic.Id,
                    DeviceType.Wpf);

                UpdateOrderedSources();
                BeginSrcNumberInjection();
            }    
        }

        private void txtSource_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
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
                        attach.Person = PrivateCenterCtx.Get().Person.FirstOrDefault(p0 => p0.Id == seldId);

                        ap.ChangesPending = true;
                        UISharedRTClient.Instance.clienRt.SendStatsEvent(
                            StEvent.ScreenshotAdded,
                            ap.Person.Id,
                            ap.Topic.Discussion.Id,
                            ap.Topic.Id,
                            DeviceType.Wpf);

                        UpdateOrderedMedia();
                        BeginAttachmentNumberInjection();
                    }
                });
            screenshotWnd.ShowDialog();
        }

        //private void lstBxAttachments_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        //{
        //    var a = lstBxAttachments.SelectedItem as Attachment;
        //    if (a == null)
        //        return;

        //    if (a.Link != null)
        //        txtAttachmentURL.Text = a.Link;
        //}

        private void onSourceRemoved(object sender, RoutedEventArgs e)
        {
            srcRepositionPopup.IsOpen = false;

            //report event 
            var ap = (ArgPoint) DataContext;

            (((FrameworkElement) e.OriginalSource).DataContext as Source).RichText = null;

            BeginSrcNumberInjection();
            UpdateOrderedSources();

            ap.ChangesPending = true;
            UISharedRTClient.Instance.clienRt.SendStatsEvent(StEvent.SourceRemoved,
                                                             ap.Person.Id,
                                                             ap.Topic.Discussion.Id,
                                                             ap.Topic.Id,
                                                             DeviceType.Wpf);
        }

        private void BeginSrcNumberInjection()
        {
            Dispatcher.BeginInvoke(new Action(_injectSourceNumbers),
                                   System.Windows.Threading.DispatcherPriority.Background, null);
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

                var numberText = Utils.FindChild<Label>(item);
                if (numberText != null)
                    numberText.Content = (i + 1).ToString();
            }
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

        private void plainDescription_KeyDown_1(object sender, KeyEventArgs e)
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            ap.ChangesPending = true;
        }

        #region cloud storage 

        private void RunCloudStorageViewer(StorageType storageType)
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

        private void onWebViewerRequest(string Uri)
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            var browser = new WebkitBrowserWindow(Uri, ap.Topic.Id);
            browser.Show();
        }

        private void onStorageWndClosed(object sender, EventArgs e)
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            if (_storageWnd == null)
                return;

            if (_storageWnd.filesToAttach != null)
            {
                foreach (StorageSelectionEntry entry in _storageWnd.filesToAttach)
                {
                    try
                    {
                        var attach = AttachmentManager.AttachCloudEntry(ap, entry);
                        var seldId = SessionInfo.Get().person.Id;
                        attach.Person = PrivateCenterCtx.Get().Person.FirstOrDefault(p0 => p0.Id == seldId);

                        ap.ChangesPending = true;

                        var ev = StEvent.ArgPointTopicChanged;
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

                UpdateOrderedMedia();
                BeginAttachmentNumberInjection();
            }

            _storageWnd.fileViewCallback -= onCloudViewerRequest;
            _storageWnd.Closed -= onStorageWndClosed;
            _storageWnd = null;
        }

        private void onCloudViewerRequest(string pathName)
        {
            AttachmentManager.RunViewer(pathName, -1, null);
        }

        private void chooseDropboxFiles(object sender, RoutedEventArgs e)
        {
            RunCloudStorageViewer(StorageType.Undefined);
        }

        //private void chooseGDriveFiles(object sender, RoutedEventArgs e)
        //{
        //    RunCloudStorageViewer(StorageType.GoogleDrive);
        //}

        #endregion

        private void lstBxAttachments_PreviewMouseWheel_1(object sender, MouseWheelEventArgs e)
        {
            var scrollViwer = GetScrollViewer(lstBxAttachments) as ScrollViewer;

            if (scrollViwer != null)
            {
                var hOffset = -30.0;
                if (e.Delta < 0)
                    hOffset *= -1.0;

                scrollViwer.ScrollToHorizontalOffset(scrollViwer.HorizontalOffset + hOffset);
            }
            e.Handled = true;
        }

        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            {
                return o;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        #region source up/down

        private SourceMover srcMover = null;

        private void processSrcUpDown(bool up, Source current)
        {
            if (current == null)
                return;

            if (srcMover.swapWithNeib(up, current))
            {
                current.RichText.ArgPoint.ChangesPending = true;
                BeginSrcNumberInjection();
                UpdateOrderedSources();
            }
        }

        private void onSourceUpDown(object sender, RoutedEventArgs e)
        {
            srcMover.onSourceUpDown(sender, e);
        }

        private void btnSrcDown_Click_1(object sender, RoutedEventArgs e)
        {
            if (srcMover._srcToReposition == null)
                return;
            processSrcUpDown(false, srcMover._srcToReposition);
        }

        private void btnSrcUp_Click_1(object sender, RoutedEventArgs e)
        {
            if (srcMover._srcToReposition == null)
                return;
            processSrcUpDown(true, srcMover._srcToReposition);
        }

        private void btnClosePopup_Click_1(object sender, RoutedEventArgs e)
        {
            srcRepositionPopup.IsOpen = false;
        }

        #endregion

        #region media up/down

        private MediaMover mediaMover = null;

        private void processAttachmentUpDown(bool up, Attachment current)
        {
            if (current == null)
                return;

            if (mediaMover.swapWithNeib(up, current))
            {
                current.ArgPoint.ChangesPending = true;
                BeginAttachmentNumberInjection();
                UpdateOrderedMedia();
            }
        }

        private void btnReposition_Click_1(object sender, RoutedEventArgs e)
        {
            mediaMover.onAttachmentUpDown(sender, e);
        }

        private void btnMediaUp_Click_1(object sender, RoutedEventArgs e)
        {
            if (mediaMover._attachmentToReposition == null)
                return;
            processAttachmentUpDown(true, mediaMover._attachmentToReposition);
        }

        private void btnMediaDown_Click_1(object sender, RoutedEventArgs e)
        {
            if (mediaMover._attachmentToReposition == null)
                return;
            processAttachmentUpDown(false, mediaMover._attachmentToReposition);
        }

        private void btnCloseMediaRepositionPopup_Click_1(object sender, RoutedEventArgs e)
        {
            mediaRepositionPopup.IsOpen = false;
        }

        #endregion

        private void btnGoogle_Click_1(object sender, RoutedEventArgs e)
        {
            var ap = DataContext as ArgPoint;
            var browser = new WebkitBrowserWindow("http://google.com", ap.Topic.Id);
            browser.Show();
        }

        #region comment notificatinos

        public CommentDismissalRecognizer CommentDismissalRecognizer;

        public void HandleCommentRead(CommentsReadEvent ev)
        {
            var ap = DataContext as ArgPoint;
            if (ap == null)
                return;

            if (ev.ArgPointId != ap.Id)
                return;

            if (ev.PersonId == SessionInfo.Get().person.Id)
            {
                UpdateLocalReadCounts(new DiscCtx(ConfigManager.ConnStr), ap);
            }
            else
            {                
                UpdateRemoteReadCounts(new DiscCtx(ConfigManager.ConnStr), ap);
            }    
        }

        private void UpdateLocalReadCounts(DiscCtx ctx,  ArgPoint ap)
        {
            if (ap == null)
                return;

            SetNumUnreadComments(DaoUtils.NumCommentsUnreadBy(ctx, ap.Id).Total());
            new CommentNotificationDeferral(Dispatcher, ctx, lstBxComments);
        }

        void UpdateRemoteReadCounts(DiscCtx ctx, ArgPoint ap)
        {
            if (ap == null)
                return;

            txtCommentSeenBy.Text = DaoUtils.RecentCommentReadBy(ctx, ap.Id);
        }

        private void SetNumUnreadComments(int numUnread)
        {
            lblComments.Content = CommentDismissalRecognizer.FormatNumUnreadComments(numUnread);
        }

        public ItemsControl TestGetCommentsControl()
        {
            return lstBxComments;
        }
        #endregion

        private void TxtAttachmentURL_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            AttachMediaFromUrl();
        }

        private void TxtSource_OnGotFocus(object sender, RoutedEventArgs e)
        {
            txtSource.Text = "";
        }

        private void TxtSource_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var ap = DataContext as ArgPoint;
            txtSource.Text = ap.RecentlyEnteredSource;
        }

        private void TxtAttachmentURL_OnGotFocus(object sender, RoutedEventArgs e)
        {
            txtAttachmentURL.Text = "";
        }

        private void TxtAttachmentURL_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var ap = DataContext as ArgPoint;
            txtAttachmentURL.Text = ap.RecentlyEnteredMediaUrl;
        }


        private DateTime _recentSourceChange;
        private async void TxtSource_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _recentSourceChange = DateTime.Now;

            await Utils.Delay(700);

            if (DateTime.Now.Subtract(_recentSourceChange).TotalMilliseconds > 700)
                AttachSource();
        }

        void _selectAllTimer_Tick(object sender, EventArgs e)
        {
            if(txtPoint.Text == DaoUtils.NEW_POINT_NAME)
            {
                txtPoint.SelectAll();
            }
            if (plainDescription.Text == DaoUtils.NEW_DESCRIPTION)
            {
                plainDescription.SelectAll();
            }
        }

        private void BtnAttachMedia_OnClick(object sender, RoutedEventArgs e)
        {
            AttachMediaFromUrl();
        }

        private void TxtPoint_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (txtPoint.Text == DaoUtils.NEW_POINT_NAME)
                txtPoint.Text = "";
        }

        private void TxtPoint_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPoint.Text))
                txtPoint.Text = DaoUtils.NEW_POINT_NAME;
        }

        private void PlainDescription_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (plainDescription.Text == DaoUtils.NEW_DESCRIPTION)
                plainDescription.Text = "";
        }

        private void PlainDescription_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(plainDescription.Text))
                plainDescription.Text = DaoUtils.NEW_DESCRIPTION;
        }
    }
}