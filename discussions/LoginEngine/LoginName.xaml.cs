﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AbstractionLayer;
using Discussions.DbModel;

namespace Discussions
{
    public partial class LoginName : PortableWindow
    {
        public bool BackClicked = false;

        private string _enteredName = "<User name>";

        public string EnteredName
        {
            get { return _enteredName; }
            set { _enteredName = value; }
        }

        public LoginName(Seat selectedSeat)
        {
            InitializeComponent();

            DataContext = this;

            //SkinManager.ChangeSkin("GreenSkin.xaml", this.Assets);
            SkinManager.ChangeSkin("Blue2Skin.xaml", this.Resources);

            if (selectedSeat != null && selectedSeat.Person != null)
            {
                var pers = selectedSeat.Person.LastOrDefault();
                if (pers != null)
                {
                    EnteredName = pers.Name;
                    tbxName.Text = pers.Name;
                }
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            BackClicked = true;
            Close();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (EnteredName.Trim() == "")
                EnteredName = null;

            Close();
        }

        private void tbxName_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            decorations.SetGreetingName(tbxName.Text);
        }
    }
}