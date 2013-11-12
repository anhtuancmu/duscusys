﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AbstractionLayer;
using Discussions.ctx;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Microsoft.Surface;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : PortableWindow
    {
        public Discussion EditedDiscussion
        {
            get
            {
                if (DataContext == null)
                    return null;
                else
                    return DataContext as Discussion;
            }
            set
            {
                DataContext = value;

                tmpPersons.Clear();

                if (value != null)
                {
                    lstBxParticipants.DataContext = this;
                    lstBxParticipants.ItemsSource = tmpPersons;

                    if (value.Topic.Count > 0)
                        lstBoxTopics.SelectedIndex = 0;
                }
            }
        }

        public Topic EditedTopic
        {
            get { return lstBoxTopics.SelectedItem as Topic; }
        }

        private ObservableCollection<Person> _tmpPersons = new ObservableCollection<Person>();

        public ObservableCollection<Person> tmpPersons
        {
            get { return _tmpPersons; }
            set { _tmpPersons = value; }
        }

        private void insertPersonOrdered(Person p)
        {
            tmpPersons.Add(p);
            tmpPersons = new ObservableCollection<Person>(tmpPersons.OrderBy(person => person.Name));
            lstBxParticipants.ItemsSource = null;
            lstBxParticipants.ItemsSource = tmpPersons;
        }

        public bool Confirmed = false;

        private UISharedRTClient _sharedClient = null;

        private Main.OnDiscFrmClosing _closing;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Dashboard(UISharedRTClient sharedClient, Main.OnDiscFrmClosing closing)
        {
            InitializeComponent();

            // Add handlers for window availability events
            AddWindowAvailabilityHandlers();

            _closing = closing;

            _sharedClient = sharedClient;

            // background.onInitNewDiscussion += onInitNewDiscussion;

            discussionSelector.Set(PublicBoardCtx.Get().Discussion, "Subject");

            discussionSelector.onSelected += ExistingDiscussionSelected;
        }

        private void onInitNewDiscussion()
        {
            // EnsureNonNullDiscussion();
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

        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        private void btnAddTopic_Click(object sender, RoutedEventArgs e)
        {
            if (EditedDiscussion == null)
                return;

            Topic top = new Topic();
            top.Name = "Topic name";
            EditedDiscussion.Topic.Add(top);
            lstBoxTopics.SelectedItem = top;

            foreach (var p in tmpPersons)
                top.Person.Add(p);
        }

        private void btnRemoveTopic_Click(object sender, RoutedEventArgs e)
        {
            Topic t = EditedTopic;
            if (t == null || EditedDiscussion == null)
                return;

            if (t != null)
            {
                EditedDiscussion.Topic.Remove(t);
                DaoUtils.removePersonsAndTopic(t);
            }
        }

        private void btnAddParticipant_Click(object sender, RoutedEventArgs e)
        {
            Topic t = EditedTopic;
            if (t == null || EditedDiscussion == null)
                return;

            PersonDiscConfigWnd wnd = new PersonDiscConfigWnd(EditedDiscussion, null);
            wnd.ShowDialog();

            if (wnd.person != null && !tmpPersons.Contains(wnd.person))
                insertPersonOrdered(wnd.person);
        }

        private void btnEditParticipant_Click(object sender, RoutedEventArgs e)
        {
            Topic t = EditedTopic;
            if (t == null || EditedDiscussion == null)
                return;

            Person p = lstBxParticipants.SelectedItem as Person;
            if (p == null)
                return;


            PersonDiscConfigWnd wnd = new PersonDiscConfigWnd(EditedDiscussion, p);
            wnd.ShowDialog();

            if (wnd.person != null && !tmpPersons.Contains(wnd.person))
            {
                tmpPersons.Remove(p); //remove previous person 
                insertPersonOrdered(wnd.person); //add new                 
            }
        }

        private void btnRemoveParticipant_Click(object sender, RoutedEventArgs e)
        {
            Person s = lstBxParticipants.SelectedItem as Person;
            if (s == null)
                return;

            var t = EditedTopic;
            if (t == null)
                return;

            t.Person.Remove(s);
            tmpPersons.Remove(s);
        }

        private void SaveDiscussion()
        {
            if (EditedDiscussion == null)
                return;

            SaveParticipants();

            if (!Ctors.DiscussionExists(EditedDiscussion))
                PublicBoardCtx.Get().AddToDiscussion(EditedDiscussion);

            background.SaveChanges();

            PublicBoardCtx.Get().SaveChanges();
        }

        private void ExistingDiscussionSelected(object selected)
        {
            background.SaveChanges();
            EditedDiscussion = selected as Discussion;
        }

        //void EnsureNonNullDiscussion()
        //{
        //    if (EditedDiscussion == null)
        //        btnAddDiscussion_Click(null,null);
        //}

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (EditedDiscussion != null && Ctors.DiscussionExists(EditedDiscussion))
            {
                BusyWndSingleton.Show("Deleting discussion...");
                try
                {
                    if (SessionInfo.Get().discussion != null)
                    {
                        if (SessionInfo.Get().discussion.Id == EditedDiscussion.Id)
                            SessionInfo.Get().discussion = null;
                    }

                    DaoUtils.DeleteDiscussion(EditedDiscussion);
                    discussionSelector.Set(PublicBoardCtx.Get().Discussion, "Subject");
                    EditedDiscussion = null;
                }
                finally
                {
                    BusyWndSingleton.Hide();
                }
            }
        }

        private void btnRemoveAttach_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //if (EditedDiscussion != null && background.lstBxAttachments.SelectedItem != null)
            //    EditedDiscussion.Attachment.Remove((Attachment)background.lstBxAttachments.SelectedItem);
        }

        private void btnAddDiscussion_Click(object sender, RoutedEventArgs e)
        {
            var newDisc = new Discussion();
            newDisc.Subject = txtBxDiscussion.Text;
            newDisc.Background = new RichText();
            newDisc.Background.Text = "";
            EditedDiscussion = newDisc;
            PublicBoardCtx.Get().AddToDiscussion(EditedDiscussion);
            PublicBoardCtx.Get().SaveChanges();
            discussionSelector.Set(PublicBoardCtx.Get().Discussion, "Subject");
            discussionSelector.Select(EditedDiscussion);
        }

        private void AllSources_Click(object sender, RoutedEventArgs e)
        {
            if (EditedDiscussion == null || EditedDiscussion.Background == null)
                return;

            allRefsPopup.SetModel(EditedDiscussion.Background, ReferenceEditor.Edit);
            allRefsPopup.IsOpen = true;
        }

        private void addSource_Click(object sender, RoutedEventArgs e)
        {
            if (EditedDiscussion == null)
                return;

            DaoUtils.EnsureBgExists(EditedDiscussion);
            DaoUtils.AddSource(EditedDiscussion.Background);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_closing != null)
                _closing();
        }

        private void SaveParticipants(Topic t = null)
        {
            if (t == null)
                t = EditedTopic;
            if (t == null || EditedDiscussion == null)
                return;

            foreach (Topic top in EditedDiscussion.Topic)
            {
                top.Person.Clear();

                foreach (var p in tmpPersons)
                {
                    if (p.Name == "Name")
                        continue;

                    bool prevExists;
                    Person prev = DaoUtils.PersonSingleton(p, out prevExists);

                    if (!top.Person.Contains(prev))
                        top.Person.Add(prev);
                }
            }

            PublicBoardCtx.Get().SaveChanges();
        }

        private void lstBoxTopics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Topic t = EditedTopic;
            if (t == null)
            {
                lblSpeakers.Content = "please select topic";
                return;
            }

            if (e.RemovedItems != null && e.RemovedItems.Count > 0)
                SaveParticipants((Topic) (e.RemovedItems[0]));

            tmpPersons.Clear();
            foreach (var p in t.Person)
                insertPersonOrdered(p);
            lstBxParticipants.ItemsSource = tmpPersons;

            lblSpeakers.Content = "Speakers of " + t.Name;
        }

        private void confirm()
        {
            try
            {
                BusyWndSingleton.Show("Uploading changes, please wait");
                SaveDiscussion();
            }
            finally
            {
                BusyWndSingleton.Hide();
            }

            //by selecting manual device type, we force all clients not to ignore the update as own (e.g. our private board will
            //update, not ignore this notification)
            _sharedClient.clienRt.SendNotifyStructureChanged(-1, SessionInfo.Get().person.Id, DeviceType.Sticky);

            Confirmed = true;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            confirm();
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            confirm();
        }

        private void btnBatch_Click_1(object sender, RoutedEventArgs e)
        {
            (new DiscussionAutoCreatorWnd()).ShowDialog();
            discussionSelector.Set(PublicBoardCtx.Get().Discussion, "Subject");
        }
    }
}