using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Discussions.DbModel;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions.view
{
    public partial class SeatManagerWnd : SurfaceWindow
    {
        private ObservableCollection<Seat> _seats = null;

        public ObservableCollection<Seat> Seats
        {
            get
            {
                if (_seats == null)
                {
                    _seats = new ObservableCollection<Seat>(PublicBoardCtx.Get().Seat);
                }
                return _seats;
            }
            set { _seats = value; }
        }

        public SeatManagerWnd()
        {
            InitializeComponent();

            DataContext = this;

            this.WindowState = WindowState.Normal;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var s = new Seat();
            s.SeatName = "<Seat>";
            s.Color = Utils.ColorToInt(Colors.LawnGreen);

            Seats.Add(s);
            PublicBoardCtx.Get().AddToSeat(s);
        }

        public Seat SelectedSeat
        {
            get { return lstBxSeats.SelectedItem as Seat; }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var ss = SelectedSeat;

            if (ss == null)
                return;

            Seats.Remove(ss);
            PublicBoardCtx.Get().DeleteObject(ss);
        }

        private void SurfaceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PublicBoardCtx.Get().SaveChanges();
        }

        private void lstBxSeats_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}