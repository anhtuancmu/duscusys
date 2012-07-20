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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Discussions.DbModel;

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
        VideoWindow wnd = null;
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

            wnd = new VideoWindow();            
            wnd.viewer.ClosedEvent += OnBrowserClosed;
            wnd.viewer.Video = e.Info;
            wnd.ShowDialog();            
        }

        void OnBrowserClosed(object sender, EventArgs e)
        {
            if (wnd != null)
            {
                wnd.Close();
                wnd = null; 
            }
        }
        #endregion

        #region Properties

        public YouTubeInfo Info
        {
            get
            {
                if (DataContext != null && DataContext is Attachment)
                    return AttachmentToVideoConvertor.AttachToYtInfo((Attachment)DataContext);
                else
                    return null;
            }
        }
        #endregion

        #region Private Methods

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            OnSelectedEvent(new YouTubeResultEventArgs(Info));
        }
        #endregion
    }
}
