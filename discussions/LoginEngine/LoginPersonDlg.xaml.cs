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
    /// <summary>
    /// Interaction logic for LoginPerson.xaml
    /// </summary>
    public partial class LoginPerson : Window
    {
        public Person SelectedPerson = null;

        private ObservableCollection<Person> _persons = null;

        public ObservableCollection<Person> Persons
        {
            get
            {
                if (_persons == null)
                {
                    _persons = new ObservableCollection<Person>();

                    foreach (var p in DbCtx.Get().Person)
                        _persons.Add(p);
                }

                return _persons;
            }
            set { _persons = value; }
        }

        public bool BackClicked = false;

        public LoginPerson(bool backAvailable)
        {
            InitializeComponent();

            DataContext = this;

            lblVersion.Content = Utils2.VersionString();

            //SkinManager.ChangeSkin("GreenSkin.xaml", this.Assets);
            SkinManager.ChangeSkin("Blue2Skin.xaml", this.Resources);

            if (backAvailable)
                btnBack.Visibility = Visibility.Visible;
            else
                btnBack.Visibility = Visibility.Collapsed;
        }

        private void lstBxPersons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedPerson = e.AddedItems[0] as Person;
            Close();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            BackClicked = true;
            Close();
        }
    }
}