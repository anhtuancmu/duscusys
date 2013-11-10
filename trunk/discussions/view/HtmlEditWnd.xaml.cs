using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using Discussions.DbModel;
using Discussions.model;

namespace Discussions.view
{
    public partial class HtmlEditWnd : Window
    {
        private Discussion _d;

        private Action _close = null;

        public HtmlEditWnd(Discussion d, Action close)
        {
            InitializeComponent();

            _close += close;

            _d = d;

            if (d.HtmlBackground == null)
            {
                var htmlTemplatePathName = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "DiscussionHTMLPage.html");

                plainHtml.Text = System.IO.File.ReadAllText(htmlTemplatePathName);
            }
            else
            {
                plainHtml.Text = d.HtmlBackground;
            }

            UpdateWebView();
        }

        private void UpdateWebView()
        {
            webView.NavigateToString(plainHtml.Text);
        }

        private void SurfaceButton_Click_1(object sender, RoutedEventArgs e)
        {
            webView.NavigateToString(plainHtml.Text);
        }

        private void AttachFile(ArgPoint ap, string command)
        {
            if (_d == null)
                return;

            Attachment a = new Attachment();
            if (AttachmentManager.ProcessAttachCmd(null, AttachCmd.ATTACH_IMG_OR_PDF, ref a) != null)
            {
                a.Discussion = _d;
                a.Person = getFreshPerson();

                insertMedia(a);
            }
        }

        private Person getFreshPerson()
        {
            var ownId = SessionInfo.Get().person.Id;
            return PublicBoardCtx.Get().Person.FirstOrDefault(p0 => p0.Id == ownId);
        }

        private void btnAttachFromUrl_Click_1(object sender, RoutedEventArgs e)
        {
            if (_d == null)
                return;

            InpDialog dlg = new InpDialog();
            dlg.ShowDialog();
            string URL = dlg.Answer;
            if (URL == null)
                return;

            Attachment a = new Attachment();
            if (AttachmentManager.ProcessAttachCmd(null, URL, ref a) != null)
            {
                a.Discussion = _d;
                a.Person = getFreshPerson();

                insertMedia(a);
            }
        }

        private void btnAttachScreenshot_Click_1(object sender, RoutedEventArgs e)
        {
            if (_d == null)
                return;

            var screenshotWnd = new ScreenshotCaptureWnd((System.Drawing.Bitmap b) =>
                {
                    var attach = AttachmentManager.AttachScreenshot(null, b);
                    if (attach != null)
                    {
                        attach.Person = getFreshPerson();
                        attach.Discussion = _d;

                        insertMedia(attach);
                    }
                });
            screenshotWnd.ShowDialog();
        }

        private void chooseImgClick(object sender, RoutedEventArgs e)
        {
            AttachFile(DataContext as ArgPoint, "Image");
        }

        private void SurfaceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveDiscussion();

            if (_close != null)
                _close();
        }

        private void insertMedia(Attachment a)
        {
            var html = plainHtml.Text;
            var bodyCloses = html.IndexOf("</body>");
            if (bodyCloses == -1)
                return;

            SaveDiscussion(); //first save, we need Id of attachment

            html = html.Insert(bodyCloses, GetPastableHtml(a));
            plainHtml.Text = html;

            UpdateWebView();
        }

        private void SaveDiscussion()
        {
            if (_d == null)
                return;

            DaoUtils.EnsureBgExists(_d);
            _d.Background.Text = "";
            _d.HtmlBackground = plainHtml.Text;
            PublicBoardCtx.Get().SaveChanges();
        }

        private static string GetPastableHtml(Attachment a)
        {
            var imgThumbUrl = string.Format("http://{0}/discsvc/discsvc.svc/Attachment({1})/$value",
                                            ConfigManager.ServiceServer, a.Id);
            if (AttachmentManager.IsGraphicFormat(a))
            {
                return string.Format("<p>\n <img src=\"{0}\"/> \n</p>\n", imgThumbUrl);
            }
            else if (a.Format == (int) AttachmentFormat.Youtube)
            {
                return
                    string.Format(
                        "<iframe width=\"640\" height=\"360\" src=\"{0}\" frameborder=\"0\" allowfullscreen></iframe>",
                        a.VideoEmbedURL);
            }

            return "";
        }

        private void UserControl_Unloaded_1(object sender, RoutedEventArgs e)
        {
            webView.Dispose();
        }
    }
}