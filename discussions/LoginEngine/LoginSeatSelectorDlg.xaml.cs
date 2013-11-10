using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Discussions.DbModel;
using LoginEngine;

namespace Discussions
{
    public partial class LoginSeatSelectorDlg : Window
    {
        public Seat SelectedSeat = null;
        public bool BackClicked = false;

        private ObservableCollection<Seat> _seats = null;

        public ObservableCollection<Seat> Seats
        {
            get
            {
                if (_seats == null)
                {
                    _seats = new ObservableCollection<Seat>();
                    foreach (var s in DbCtx.Get().Seat)
                        _seats.Add(s);
                }
                return _seats;
            }
            set { _seats = value; }
        }

        public LoginSeatSelectorDlg()
        {
            InitializeComponent();

            DataContext = this;

            //SkinManager.ChangeSkin("GreenSkin.xaml", this.Assets);
            SkinManager.ChangeSkin("Blue2Skin.xaml", this.Resources);
        }

        private void lstBxSeats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedSeat = e.AddedItems[0] as Seat;
            Close();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            BackClicked = true;
            Close();
        }
    }
}