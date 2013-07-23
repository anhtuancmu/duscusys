using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Discussions.DbModel;

namespace Discussions
{
    public partial class SessionViewerDashboard : Window
    {
        private ObservableCollection<SeatUserViewModel> _seatUsers = null;

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
            set { _seatUsers = value; }
        }

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

            var ctx = PublicBoardCtx.Get();
            var personsOfSession = ctx.Person.Where(p0 => p0.Session != null && p0.Session.Id == sessionId);

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
            PublicBoardCtx.DropContext();

            _sessions = new ObservableCollection<Session>(PublicBoardCtx.Get().Session);

            //trigger update of seat/users 
            var prevSelSession = lstSessions.SelectedItem;
            lstSessions.SelectedItem = null;
            lstSessions.SelectedItem = prevSelSession;
        }
    }
}