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
using System.ComponentModel;
using System.Data;
using System.Collections.ObjectModel;
using System.Data.Objects.DataClasses;
using Discussions.DbModel;
using Discussions.rt;

namespace Discussions
{   
    public partial class SessionViewerDashboard : SurfaceWindow
    {
        ObservableCollection<SeatUserViewModel> _seatUsers = null;
        public ObservableCollection<SeatUserViewModel> SeatUsers
        {
            get
            {
                if (_seatUsers == null)
                {
                    _seatUsers = new ObservableCollection<SeatUserViewModel>();                     
                }
                return _seatUsers;
            }
            set
            {
                _seatUsers = value; 
            }
        }

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

        public SessionViewerDashboard()
        {
            InitializeComponent();

            DataContext = this;
        }

        private void lstSesions_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {                 
            var session = lstSessions.SelectedItem as Session;
            if (session == null)
                return;

            var sessionId = session.Id;

            var ctx = CtxSingleton.Get();
            var personsOfSession = ctx.Person.Where(p0=>p0.Session!=null && p0.Session.Id == sessionId);
                       
            SeatUsers.Clear();
            foreach (var pers in personsOfSession.ToArray())
            {
                if (pers.Seat == null)
                    continue;

                //find discussion of person in this session 
                var persId = pers.Id;
                var discussionsOfPerson =
                           (from t in ctx.Topic
                            where t.Person.Any(p0 => p0.Id == persId)
                            select t.Discussion).Distinct();

                if (discussionsOfPerson.Count() > 0)
                {
                    SeatUsers.Add(new SeatUserViewModel(pers.Seat.SeatName, pers.Seat.Color,
                                                       pers.Name, discussionsOfPerson.First().Subject));
                }
            }
        }

        private void btnOk_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnRefresh_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CtxSingleton.DropContext();

            _sessions = new ObservableCollection<Session>(CtxSingleton.Get().Session);
           
           //trigger update of seat/users 
           var prevSelSession = lstSessions.SelectedItem;
           lstSessions.SelectedItem = null;
           lstSessions.SelectedItem = prevSelSession;
        }
    }
}
