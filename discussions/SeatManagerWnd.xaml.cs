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
    public partial class SeatManagerWnd : SurfaceWindow
    {
        private ObservableCollection<Seat> _seats = null;

        public ObservableCollection<Seat> Seats
        {
            get
            {
                if (_seats == null)
                {
                    _seats = new ObservableCollection<Seat>(CtxSingleton.Get().Seat);
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
            CtxSingleton.Get().Seat.AddObject(s);
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
            CtxSingleton.Get().DeleteObject(ss);
        }

        private void SurfaceWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CtxSingleton.Get().SaveChanges();
        }

        private void lstBxSeats_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}