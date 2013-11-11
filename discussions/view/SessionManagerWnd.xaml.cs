using System;
using System.Collections.ObjectModel;
using System.Windows;
using AbstractionLayer;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;

namespace Discussions.view
{
    public partial class SessionManagerWnd : PortableWindow
    {
        private UISharedRTClient _rtClient = null;

        private ObservableCollection<Session> _sessions = null;

        public ObservableCollection<Session> Sessions
        {
            get
            {
                if (_sessions == null)
                {
                    _sessions = new ObservableCollection<Session>(PublicBoardCtx.Get().Session);
                }
                return _sessions;
            }
            set { _sessions = value; }
        }

        public SessionManagerWnd(UISharedRTClient rtClient)
        {
            InitializeComponent();

            DataContext = this;

            this.WindowState = WindowState.Normal;

            _rtClient = rtClient;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var s = new Session();
            s.Name = "<Session>";
            s.EstimatedDateTime = DateTime.Now;
            s.EstimatedEndDateTime = DateTime.Now.Add(TimeSpan.FromHours(1));
            s.EstimatedTimeSlot = (int) TimeSlot.Morning;

            Sessions.Add(s);
            PublicBoardCtx.Get().AddToSession(s);
        }

        public Session SelectedSession
        {
            get { return lstBxSessions.SelectedItem as Session; }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var ss = SelectedSession;

            if (ss == null)
                return;

            if (ss.Person.Count > 0)
            {
                MessageDlg.Show("Cannot remove session as it includes users");
                return;
            }

            Sessions.Remove(ss);
            PublicBoardCtx.Get().DeleteObject(ss);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PublicBoardCtx.Get().SaveChanges();
        }
    }
}