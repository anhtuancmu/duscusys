using System;
using AbstractionLayer;
using Discussions.DbModel;
using Discussions.DbModel.model;
using Discussions.model;
using Discussions.rt;
using Microsoft.Surface;

namespace Discussions.view
{
    public partial class ZoomWindow : PortableWindow
    {
        public ZoomWindow(ArgPoint ap)
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            largeBadgeView.DataContext = ap;
            largeBadgeView.SetRt(UISharedRTClient.Instance);

            UISharedRTClient.Instance.clienRt.SendStatsEvent(StEvent.BadgeZoomIn,
                                                             SessionInfo.Get().person.Id,
                                                             SessionInfo.Get().discussion.Id,
                                                             ap.Topic.Id,
                                                             DeviceType.Wpf);
        }

        private void CloseRequest()
        {
            largeBadgeView = null;
            Close();
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

        private void Window_Activated(object sender, EventArgs e)
        {
            this.Height = largeBadgeView.ActualHeight;
            this.Width = largeBadgeView.ActualWidth;
        }

        private void OnToggleZoom()
        {
            Close();
        }
    }
}