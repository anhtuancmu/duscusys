using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Discussions.ctx;
using Discussions.DbModel;
using Discussions.webkit_host;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for Background.xaml
    /// </summary>
    public partial class Background : UserControl
    {
        public delegate void OnInitNewDiscussion();

        public OnInitNewDiscussion onInitNewDiscussion = null;

        private ObservableCollection<Source> sources = new ObservableCollection<Source>();

        public ObservableCollection<Source> Sources
        {
            get { return sources; }
        }

        //if source order or data context changes, we update 
        private void UpdateOrderedSources()
        {
            Sources.Clear();
            var disc = DataContext as Discussion;
            if (disc == null)
                return;

            foreach (var orderedSrc in disc.Background.Source.OrderBy(s => s.OrderNumber))
            {
                Sources.Add(orderedSrc);
            }
        }

        public Background()
        {
            this.InitializeComponent();

            this.Visibility = Visibility.Hidden;

            lstBxSources.DataContext = this;

            srcMover = new SourceMover(srcRepositionPopup);
        }

        public void ToViewMode()
        {
            txtAttachmentURL.Visibility = Visibility.Hidden;
            mediaButtons.Visibility = Visibility.Hidden;
            srcButtons.Visibility = Visibility.Hidden;

            btnEditBg.Visibility = Visibility.Collapsed;
        }

        private void txtBxBackground_GotFocus(object sender, RoutedEventArgs e)
        {
            if (onInitNewDiscussion != null)
                onInitNewDiscussion();
        }

        private void txtBxBackground_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveChanges();
        }

        public void SaveChanges()
        {
            Discussion discussion = DataContext as Discussion;

            if (discussion != null)
            {
                DaoUtils.EnsureBgExists(discussion);
                //discussion.Background.Text = txtBxBackground.Text;
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AttachmentManager.RunViewer(((FrameworkElement) sender).DataContext as Attachment);
        }

        private void Image_TouchDown(object sender, TouchEventArgs e)
        {
            AttachmentManager.RunViewer(((FrameworkElement) sender).DataContext as Attachment);
        }

        private void lstBxAttachments_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            //var a = lstBxAttachments.SelectedItem as Attachment;
            //if (a == null)
            //    return;

            //if (a.Link != null)
            //    txtAttachmentURL.Text = a.Link;
        }

        private void btnAttachFromUrl_Click_1(object sender, RoutedEventArgs e)
        {
            var discussion = DataContext as Discussion;
            if (discussion == null)
                return;

            InpDialog dlg = new InpDialog();
            dlg.ShowDialog();
            string URL = dlg.Answer;
            if (URL == null)
                return;

            Attachment a = new Attachment();
            if (AttachmentManager.ProcessAttachCmd(null, URL, ref a) != null)
            {
                a.Discussion = discussion;
                a.Person = getFreshPerson();
            }
        }

        private void btnAttachScreenshot_Click_1(object sender, RoutedEventArgs e)
        {
            var d = DataContext as Discussion;
            if (d == null)
                return;

            var screenshotWnd = new ScreenshotCaptureWnd((System.Drawing.Bitmap b) =>
                {
                    var attach = AttachmentManager.AttachScreenshot(null, b);
                    if (attach != null)
                    {
                        attach.Person = getFreshPerson();
                        attach.Discussion = d;
                    }
                });
            screenshotWnd.ShowDialog();
        }

        private void txtAttachmentURL_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;

            if (txtAttachmentURL.Text.Trim() == "")
                return;

            var d = DataContext as Discussion;
            if (d == null)
                return;

            //ap.RecentlyEnteredMediaUrl = txtAttachmentURL.Text;

            Attachment a = new Attachment();
            if (AttachmentManager.ProcessAttachCmd(null, txtAttachmentURL.Text, ref a) != null)
            {
                a.Person = getFreshPerson();
                a.Discussion = d;
            }
        }

        private void chooseImgClick(object sender, RoutedEventArgs e)
        {
            AttachFile(DataContext as ArgPoint, "Image");
        }

        //attachments from files
        private void AttachFile(ArgPoint ap, string command)
        {
            var d = DataContext as Discussion;
            if (d == null)
                return;

            Attachment a = new Attachment();
            if (AttachmentManager.ProcessAttachCmd(null, AttachCmd.ATTACH_IMG_OR_PDF, ref a) != null)
            {
                a.Discussion = d;
                a.Person = getFreshPerson();
            }
        }

        private Person getFreshPerson()
        {
            var ownId = SessionInfo.Get().person.Id;
            return PublicBoardCtx.Get().Person.FirstOrDefault(p0 => p0.Id == ownId);
        }

        private void removeMedia_Click(object sender, RoutedEventArgs e)
        {
            var at = ((ContentControl) sender).DataContext as Attachment;

            at.Discussion = null;
            var mediaData = at.MediaData;
            at.MediaData = null;
            if (mediaData != null)
                PublicBoardCtx.Get().DeleteObject(mediaData);
            PublicBoardCtx.Get().Attachment.DeleteObject(at);
        }

        private void btnAddSrc_Click(object sender, RoutedEventArgs e)
        {
            var d = DataContext as Discussion;
            if (d == null)
                return;

            DaoUtils.AddSource(txtSource.Text, d.Background);
            BeginDeferredItemTemplateHandle();
            UpdateOrderedSources();
        }

        private void onSourceRemoved(object sender, RoutedEventArgs e)
        {
            srcRepositionPopup.IsOpen = false;

            //report event 
            var ap = (Discussion) DataContext;

            (((FrameworkElement) e.OriginalSource).DataContext as Source).RichText = null;

            BeginDeferredItemTemplateHandle();
            UpdateOrderedSources();
        }

        private void txtSource_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                btnAddSrc_Click(null, null);
        }

        private void UserControl_DataContextChanged_1(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                this.Visibility = Visibility.Visible;
            else
                this.Visibility = Visibility.Hidden;

            txtAttachmentURL.Text = "URL or path here";
            txtSource.Text = "Source here";
            var d = DataContext as Discussion;
            if (d != null)
            {
                if (d.Attachment.Count() > 0)
                    txtAttachmentURL.Text = d.Attachment.Last().Link;

                if (d.Background.Source.Count > 0)
                    txtSource.Text = d.Background.Source.Last().Text;

                if (d.HtmlBackground != null)
                    htmlBackground.NavigateToString(d.HtmlBackground);
                else
                    htmlBackground.NavigateToString(NoBackground());
            }
            else if (htmlBackground.IsLoaded)
                htmlBackground.NavigateToString(NoBackground());

            BeginDeferredItemTemplateHandle();
            UpdateOrderedSources();
        }

        #region source up/down

        private SourceMover srcMover = null;

        private void processSrcUpDown(bool up, Source current)
        {
            if (current == null)
                return;

            if (srcMover.swapWithNeib(up, current))
            {
                BeginDeferredItemTemplateHandle();
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

        private bool _canReorder = true;

        public void SetCanReorderItems(bool canReorder)
        {
            _canReorder = canReorder;
            BeginDeferredItemTemplateHandle();
        }

        private void BeginDeferredItemTemplateHandle()
        {
            Dispatcher.BeginInvoke(new Action(_deferredItemTemplateHandle),
                                   System.Windows.Threading.DispatcherPriority.Background, null);
        }

        private void _deferredItemTemplateHandle()
        {
            var d = DataContext as Discussion;
            if (d == null)
                return;

            for (int i = 0; i < d.Background.Source.Count(); i++)
            {
                var item = lstBxSources.ItemContainerGenerator.ContainerFromIndex(i);
                var srcUC = Utils.FindChild<SourceUC>(item);
                if (srcUC != null)
                {
                    srcUC.CanReorder = _canReorder;
                    srcUC.SrcNumber = i + 1;
                }
            }
        }

        #endregion

        private void btnEditBg_Click(object sender, RoutedEventArgs e)
        {
            DiscWindows.Get().htmlBackgroundWnd = new HtmlEditWnd(DataContext as Discussion,
                                                                  () =>
                                                                      {
                                                                          DiscWindows.Get().htmlBackgroundWnd = null;

                                                                          var d = DataContext as Discussion;
                                                                          if (d != null)
                                                                          {
                                                                              if (d.HtmlBackground != null)
                                                                                  htmlBackground.NavigateToString(
                                                                                      d.HtmlBackground);
                                                                          }
                                                                      }
                );
            DiscWindows.Get().htmlBackgroundWnd.Show();
        }

        private void btnInternalBrowser_Click_1(object sender, RoutedEventArgs e)
        {
            var d = DataContext as Discussion;
            if (d != null)
            {
                if (d.HtmlBackground != null)
                {
                    var browser = new WebKitFrm(getDiscussionBackgroundUrl(d), 0);//0 TopicId for background
                    browser.ShowDialog();
                }
            }
        }

        private void btnSystemBrowserClick(object sender, RoutedEventArgs e)
        {
            var d = DataContext as Discussion;
            if (d != null)
            {
                if (d.HtmlBackground != null)
                {
                    System.Diagnostics.Process.Start(getDiscussionBackgroundUrl(d));
                }
            }
        }

        public static string getDiscussionBackgroundUrl(Discussion d)
        {
            return string.Format("http://{0}/discsvc/bgpage?id={1}", ConfigManager.ServiceServer, d.Id);
        }

        private void UserControl_Unloaded_1(object sender, RoutedEventArgs e)
        {
            htmlBackground.Dispose();
        }

        private static string noBackgroundHtml = null;

        private static string NoBackground()
        {
            if (noBackgroundHtml == null)
            {
                var htmlTemplatePathName = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "NoBackgroundPage.html");

                noBackgroundHtml = System.IO.File.ReadAllText(htmlTemplatePathName);
            }
            return noBackgroundHtml;
        }
    }
}