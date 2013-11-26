using System;
using System.ComponentModel;
using System.Windows;
using AbstractionLayer;
using Microsoft.Surface;

namespace Reporter
{
    public partial class NewSessionTopicDlg : PortableWindow
    {       
        private readonly NewSessionTopicDialogVm _model;
        public NewSessionTopicDialogVm Model
        {
            get
            {
                return _model;
            }
        }

        public NewSessionTopicDlg()
        {
            InitializeComponent();

            AddWindowAvailabilityHandlers();

            this.WindowState = WindowState.Normal;

            DataContext = _model = new NewSessionTopicDialogVm();
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
            Close();
        }

        private void NewSessionTopicDlg_OnClosing(object sender, CancelEventArgs e)
        {
            foreach (var reportTarget in lstReportTargets.SelectedItems)
                _model.SelectedReportTargets.Add((ReportParameters)reportTarget);
        }
    }
}