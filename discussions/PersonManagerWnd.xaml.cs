using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Surface;
using System.Collections.ObjectModel;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for PersonManagerWnd.xaml
    /// </summary>
    public partial class PersonManagerWnd : Window
    {
        private bool changesExist = false;

        private UISharedRTClient _sharedClient = null;

        //persons are unique in the speakers
        private ObservableCollection<Person> _persons;

        public ObservableCollection<Person> persons
        {
            get { return _persons; }
            set { _persons = value; }
        }

        private Discussions.Main.OnDiscFrmClosing _closing;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PersonManagerWnd(UISharedRTClient sharedClient, Discussions.Main.OnDiscFrmClosing closing)
        {
            InitializeComponent();

            _sharedClient = sharedClient;

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            _closing = closing;
            persons = new ObservableCollection<Person>(PublicBoardCtx.Get().Person);

            DataContext = this;

            this.WindowState = WindowState.Normal;
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
            var ctx = PublicBoardCtx.Get();
            foreach (var p in persons)
            {
                bool prevExists;
                Person prev = DaoUtils.PersonSingleton(p, out prevExists);
                if (!prevExists)
                    try
                    {
                        ctx.Person.AddObject(prev);
                    }
                    catch (Exception)
                    {
                        //persons in modified are ignored
                    }
            }

            ctx.SaveChanges();

            if (changesExist)
                _sharedClient.clienRt.SendUserAccPlusMinus();

            Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            persons.Add(Ctors.NewPerson("New user", "email"));
            changesExist = true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            List<object> selected = new List<object>();
            foreach (var p in lstBxParticipants.SelectedItems)
                selected.Add(p);

            foreach (var p in selected)
            {
                Person pToDelete = p as Person;
                if (pToDelete == null)
                    return;

                BusyWndSingleton.Show("Deleting person...");
                try
                {
                    //remove the participant from UI
                    persons.Remove(pToDelete);
                    lstBxParticipants.Items.Refresh();
                    changesExist = true;
                    DaoUtils.deletePersonAndPoints(pToDelete);
                }
                finally
                {
                    BusyWndSingleton.Hide();
                }
            }
        }

        private void SurfaceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_closing != null)
                _closing();
        }
    }
}