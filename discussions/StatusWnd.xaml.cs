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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Collections.ObjectModel;
using Discussions.RTModel.Model;
using Discussions.rt;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for StatusWnd.xaml
    /// </summary>
    public partial class StatusWnd : SurfaceWindow
    {
        ObservableCollection<DiscUser> _onlineUsers = new ObservableCollection<DiscUser>();
        public ObservableCollection<DiscUser> onlineUsers
        {
            get
            {
                return _onlineUsers;
            }
            set
            {
                _onlineUsers = value;
            }
        }

        UISharedRTClient _sharedRt;
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public StatusWnd(UISharedRTClient sharedRt)
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            _sharedRt = sharedRt;

            SetListeners(sharedRt, true);

            if (SessionInfo.Get().discussion != null)
                lblDiscussion.Content = SessionInfo.Get().discussion.Subject;
            else
                lblDiscussion.Content = "<NO DISCUSSION>";

            lblPlayer.Content = SessionInfo.Get().person.Name;

            DataContext = this;
        }

        void SetListeners(UISharedRTClient sharedClient, bool doSet)
        {
            var clienRt = sharedClient.clienRt;

            if (clienRt == null)
                return;
           
            //if (doSet)
            //    clienRt.onlineListChanged += onlineListChanged;
            //else
            //    clienRt.onlineListChanged -= onlineListChanged;

            if (doSet)
                clienRt.userJoins += userJoins;
            else
                clienRt.userJoins -= userJoins;

            if (doSet)
                clienRt.userLeaves += userLeaves;
            else
                clienRt.userLeaves -= userLeaves;
        }

        void onlineListChanged(IEnumerable<DiscUser> newOnlineUsers)
        {
            onlineUsers.Clear();
            foreach (DiscUser usr in newOnlineUsers)
            {
                onlineUsers.Add(usr);
            }
            lblOnlinePlayers.Content = "Online players:" + onlineUsers.Count;
        }

        void userJoins(DiscUser u)
        {
            chatArea.Text += string.Format("player {0} joins\n", u.Name);
        }

        void userLeaves(DiscUser u)
        {
            chatArea.Text += string.Format("player {0} leaves\n", u.Name); 
        }

        #region slide-in/out
        bool expanded;
        static double delta = 13;
        public void SlideOut()
        {
            if (expanded)
                return;

            while (true)
            {
                if (Width > 250)
                    break;

                Width += delta;
            }
            expanded = true;
            btnToggle.Visibility = Visibility.Visible;
        }

        public void SlideIn()
        {
            if (!expanded)
                return;
            
            while(true)
            {
                if (getTogglePos().X < delta)
                    break;

                if (Width - delta > btnToggle.ActualWidth)
                    Width -= delta;
            }

            expanded = false;
            btnToggle.Visibility = Visibility.Hidden;
        }

        public void Toggle()
        {
            if (expanded)
                SlideIn();
            else
                SlideOut();
        }

        Point getTogglePos()
        {
            return btnToggle.PointToScreen(new Point(0, 0));
        }
        #endregion

        /// <summary>
        /// Occurs when the window is about to close. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Remove handlers for window availability events
            RemoveWindowAvailabilityHandlers();
        }

        /// <summary>
        /// Adds handlers for window availability events.
        /// </summary>
        private void AddWindowAvailabilityHandlers()
        {
            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;
        }

        /// <summary>
        /// Removes handlers for window availability events.
        /// </summary>
        private void RemoveWindowAvailabilityHandlers()
        {
            // Unsubscribe from surface window availability events
            ApplicationServices.WindowInteractive -= OnWindowInteractive;
            ApplicationServices.WindowNoninteractive -= OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable -= OnWindowUnavailable;
        }

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        private void SurfaceWindow_MouseEnter(object sender, MouseEventArgs e)
        {
           // SlideOut();
        }

        private void SurfaceWindow_MouseLeave(object sender, MouseEventArgs e)
        {
           // SlideIn();
        }

        private void SurfaceWindow_Activated(object sender, EventArgs e)
        {
            expanded = true;
            SlideIn();
        }

        private void SurfaceWindow_Deactivated(object sender, EventArgs e)
        {
            this.Topmost = false;
            this.Topmost = true;
        }

        private void SurfaceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetListeners(_sharedRt, false);
        }

        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            if (expanded)
                SlideIn();
            else
                SlideOut();
        }
    }
}