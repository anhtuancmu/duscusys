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
using System.Collections.ObjectModel;
using Discussions.DbModel;
using Discussions.model;
using System.Data;
using Discussions.rt;

namespace Discussions 
{
    public partial class SessionManagerWnd : SurfaceWindow
    {
        UISharedRTClient _rtClient = null;

        ObservableCollection<Session> _sessions = null;
        public ObservableCollection<Session> Sessions 
        {
            get 
            {
                if (_sessions == null)
                {
                    _sessions = new ObservableCollection<Session>(CtxSingleton.Get().Session);
                }
                return _sessions;
            }
            set
            {
                _sessions = value;
            }
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
            s.EstimatedTimeSlot = (int)TimeSlot.Morning;

            Sessions.Add(s);
            CtxSingleton.Get().Session.AddObject(s);
        }

        public Session SelectedSession
        {
            get
            {
                return lstBxSessions.SelectedItem as Session;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var ss = SelectedSession;
            
            if (ss == null)
                return;

            if (ss.Person.Count > 0)
            {
                MessageBox.Show("Cannot remove session as it includes users"); 
                return;
            }

            Sessions.Remove(ss);
            CtxSingleton.Get().DeleteObject(ss);
        }

        private void SurfaceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CtxSingleton.Get().SaveChanges();
        }
    }
}