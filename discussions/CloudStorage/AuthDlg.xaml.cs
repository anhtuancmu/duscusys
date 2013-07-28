using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CloudStorage.Model;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Collections.ObjectModel;

namespace CloudStorage
{
    /// <summary>
    /// Interaction logic for PersonManagerWnd.xaml
    /// </summary>
    public partial class AuthDlg : SurfaceWindow
    {
        public string AuthCode = "";

        private StorageType _storageType;

        public AuthDlg(StorageType storageType)
        {
            InitializeComponent();

            this.WindowState = WindowState.Normal;

            _storageType = storageType;

            switch (storageType)
            {
                case StorageType.Dropbox:
                    txtInfo.Content = "Authorize Discusys on Dropbox site and Continue";
                    gdriveAuthCode.Visibility = Visibility.Collapsed;
                    Title = "Dropbox login";
                    break;
                case StorageType.GDrive:
                    Title = "Google Drive login";
                    txtInfo.Visibility = Visibility.Collapsed;
                    break;
                default:
                    throw new NotSupportedException();
            }
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
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
        }

        private void gdriveAuthCode_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (!gdriveAuthCode.Text.Contains("AUTH CODE"))
            {
                AuthCode = gdriveAuthCode.Text;
            }
        }

        private void btnOk_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}