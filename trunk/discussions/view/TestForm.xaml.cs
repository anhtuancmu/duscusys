using System;
using System.Collections.Generic;
using System.Windows;
using Discussions.YouViewer;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for TestDiscussionDashboard.xaml
    /// </summary>
    public partial class TestForm : SurfaceWindow
    {
        private string rtf = "";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TestForm()
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            YouTubeInfo inf =
                YouTubeProvider.LoadVideo("http://www.youtube.com/watch?feature=player_detailpage&v=Osf0fLKKbZc");
            if (inf != null)
                youTubeResultControl1.DataContext = inf;

            List<object> cs = new List<object>(new string[]
                {
                    "1111111111",
                    "222222222",
                    "333333333",
                    "444444444",
                    "555555555",
                    "666666666",
                    "777777777",
                    "888888888",
                    "999999999"
                });
            surfaceCombobox1.SetChoices(cs, null);
        }

        private void control_SelectedEvent(object sender, YouTubeResultEventArgs e)
        {
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

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageDlg.Show(rtf);
        }
    }
}