using System;
using System.Windows;
using AbstractionLayer;
using Discussions.DbModel;
using Microsoft.Surface;

namespace Discussions.view
{
    public partial class TopicSelectionDlg : PortableWindow
    {
        private Topic _topic = null;

        public Topic topic
        {
            get { return _topic; }
        }

        private bool _html;
        public bool Html
        {
            get { return _html; }        
        }

        public TopicSelectionDlg(Discussion d)
        {
            InitializeComponent();

            AddWindowAvailabilityHandlers();

            this.WindowState = WindowState.Normal;

            lstTopics.ItemsSource = d.Topic;

            lstFormat.Items.Add("Export to HTML");
            lstFormat.Items.Add("Export to PDF");
            lstFormat.SelectedIndex = 0;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void btnOk_Click_1(object sender, RoutedEventArgs e)
        {
            if (lstTopics.SelectedItem == null)
            {
                MessageDlg.Show("Please select topic");
                return;
            }

            _topic = lstTopics.SelectedItem as Topic;

            _html = lstFormat.SelectedIndex == 0;

            Close();
        }

        private void TopicSelectionDlg_OnLoaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;            
        }
    }
}