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
using Discussions.model;
using System.Reflection;
using System.IO;
using Discussions.DbModel;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for Viewer2.xaml
    /// </summary>
    public partial class ResultViewer : SurfaceWindow
    {
        Discussion discussion;

        Discussions.Main.OnDiscFrmClosing _closing;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ResultViewer(Discussion discussion, Discussions.Main.OnDiscFrmClosing closing)
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            _closing = closing;
            this.discussion  = discussion;
            this.DataContext = discussion;         

            List<ArgPoint> agreed    = new List<ArgPoint>();
            List<ArgPoint> disagreed = new List<ArgPoint>();
            List<ArgPoint> unsolved  = new List<ArgPoint>();

            lstBxAgreement.ItemsSource = agreed;
            lstBxDisagreement.ItemsSource = disagreed; 
            lstBxUnsolved.ItemsSource = unsolved;             
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

        private void btnBackToMain_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnToPDF_Click(object sender, RoutedEventArgs e)
        {
            BusyWndSingleton.Show("Generating report...");
            GenerateReport();
            BusyWndSingleton.Hide();
        }

        void GenerateReport()
        {
            string reportPathName = System.IO.Path.Combine(Utils.ReportsDir(), 
                                                           Utils.ValidateFileName(discussion.Subject + ".pdf")
                                                           );
            (new ReportGenerator()).Generate(discussion,reportPathName);     
        }

        private void SurfaceWindow_Activated(object sender, EventArgs e)
        {
           /// this.WindowState = WindowState.Normal;
        }

        private void SurfaceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closing != null)
                _closing();
        }

        private void btnDiscInfo_Click(object sender, RoutedEventArgs e)
        {
            var d = DataContext as Discussion;
            if (d == null)
                return;

            var diz = new DiscussionInfoZoom(d);
            diz.ShowDialog();       
        }
    }
}

