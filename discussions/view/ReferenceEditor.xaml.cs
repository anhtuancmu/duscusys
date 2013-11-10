using System;
using System.Windows;
using Discussions.DbModel;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for ReferenceEditor.xaml
    /// </summary>
    public partial class ReferenceEditor : SurfaceWindow
    {
        public RichText richText { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ReferenceEditor(bool readOnly, RichText text)
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            DataContext = this;

            richText = text;

            if (readOnly)
            {
                btnAdd.Visibility = Visibility.Hidden;
                btnRemove.Visibility = Visibility.Hidden;
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

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (richText == null)
                return;

            DaoUtils.AddSource("new source", richText);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (richText == null)
                return;

            if (lstBxReferences.SelectedItem == null)
                return;

            richText.Source.Remove((Source) lstBxReferences.SelectedItem);
        }

        public static void Edit(RichText text, bool readOnly)
        {
            var refEdit = new ReferenceEditor(readOnly, text);
            refEdit.ShowDialog();
        }
    }
}