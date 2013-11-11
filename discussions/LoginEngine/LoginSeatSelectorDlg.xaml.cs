using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AbstractionLayer;
using Discussions.DbModel;
using LoginEngine;

namespace Discussions
{
    public partial class LoginSeatSelectorDlg : PortableWindow
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