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
using Discussions.DbModel;
using Discussions.model;
using System.Data;
using Discussions.rt;
using Discussions;

namespace Reporter
{
    public partial class SessionTopicDlg : SurfaceWindow
    {
        private List<int> _users;

        public List<int> Users
        {
            get { return _users; }
        }

        private Topic _topic = null;

        public Topic topic
        {
            get { return _topic; }
        }

        private Session _session = null;

        public Session session
        {
            get { return _session; }
        }

        private DiscCtx _ctx;

        public SessionTopicDlg()
        {
            InitializeComponent();

            AddWindowAvailabilityHandlers();

            this.WindowState = WindowState.Normal;

            _ctx = new DiscCtx(Discussions.ConfigManager.ConnStr);
            lstSessions.ItemsSource = _ctx.Session;
        }

        private ReportParameters _reportParameters = null;

        public ReportParameters reportParameters
        {
            get { return _reportParameters; }
        }

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

        private void SurfaceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void btnOk_Click_1(object sender, RoutedEventArgs e)
        {
            if (lstSessions.SelectedItem == null || lstTopics.SelectedItem == null)
            {
                MessageDlg.Show("Please select session and topic to generate report");
                return;
            }

            _session = lstSessions.SelectedItem as Session;
            _topic = lstTopics.SelectedItem as Topic;
            _users = new List<int>();
            foreach (var pers in _session.Person)
                _users.Add(pers.Id);

            _reportParameters = new ReportParameters(Users, session, topic, lstDiscussions.SelectedItem as Discussion);

            Close();
        }

        private void lstSessions_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var s = (e.AddedItems[0] as Session);

                var discussions = new List<Discussion>();

                foreach (var pers in s.Person)
                    foreach (var topic in pers.Topic)
                        if (!discussions.Contains(topic.Discussion))
                            discussions.Add(topic.Discussion);

                lstDiscussions.ItemsSource = discussions;
            }
        }

        private void lstDiscussions_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var d = (e.AddedItems[0] as Discussion);

                var topics = new List<Topic>();

                foreach (var topic in d.Topic)
                    if (!topics.Contains(topic))
                        topics.Add(topic);

                lstTopics.ItemsSource = topics;
            }
        }
    }
}