using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Discussions.DbModel;
using Discussions.model;
using Discussions.view;
using Discussions.webkit_host;

namespace Discussions.YouViewer
{
    /// <summary>
    /// Event delegate
    /// </summary>
    public delegate void SelectedEventHandler(object sender, YouTubeResultEventArgs e);


    /// <summary>
    /// Shows a single image, and when Mouse is over and the mode is
    /// not in drag mode, then show a play button, which when
    /// clicked will notify the YouViewerMainWindow to show a
    /// Viewer control
    /// </summary>
    public partial class YouTubeResultControl : UserControl
    {
        #region Data

        public event SelectedEventHandler SelectedEvent;

        #endregion

        #region Ctor

        public YouTubeResultControl()
        {
            InitializeComponent();

            //MouseEnter
            this.MouseEnter += delegate
                {
                    Storyboard sb = this.TryFindResource("OnMouseEnter") as Storyboard;
                    if (sb != null)
                        sb.Begin(this);
                };
            //MouseLeave
            this.MouseLeave += delegate
                {
                    Storyboard sb = this.TryFindResource("OnMouseLeave") as Storyboard;
                    if (sb != null)
                        sb.Begin(this);
                };
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when this control btnPlay is clicked
        /// </summary>
        protected virtual void OnSelectedEvent(YouTubeResultEventArgs e)
        {
            if (SelectedEvent != null)
            {
                //Invokes the delegates.
                SelectedEvent(this, e);
            }

            //wnd = new VideoWindow();            
            //wnd.viewer.ClosedEvent += OnBrowserClosed;
            //wnd.viewer.Video = e.Info;
            //wnd.ShowDialog();            

            RaiseEvent(new RoutedEventArgs(SourceUC.SourceViewEvent));

            var attachment = DataContext as Attachment;
            var browser = new WebkitBrowserWindow(e.Info.EmbedUrl, 
                                        attachment!=null ? attachment.ArgPoint.Topic.Id : (int?)null);
            browser.ShowDialog();
        }

        private void OnBrowserClosed(object sender, EventArgs e)
        {
            //if (wnd != null)
            //{
            //    wnd.Close();
            //    wnd = null; 
            //}
        }

        #endregion

        #region Properties

        public YouTubeInfo Info
        {
            get
            {
                if (DataContext != null && DataContext is Attachment)
                    return AttachmentToVideoConvertor.AttachToYtInfo((Attachment) DataContext);
                else
                    return null;
            }
        }

        #endregion

        #region Private Methods

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            var a = DataContext as Attachment;
            if (a != null)
                Utils.ReportMediaOpened(StEvent.VideoOpened, a);

            OnSelectedEvent(new YouTubeResultEventArgs(Info));
        }

        #endregion
    }
}