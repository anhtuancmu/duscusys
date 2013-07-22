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
using Discussions.DbModel;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for PersonSelector.xaml
    /// </summary>
    public partial class PersonSelector : UserControl
    {
        public PersonSelector()
        {
            InitializeComponent();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void Rectangle_TouchDown(object sender, TouchEventArgs e)
        {
        }

        //void SelectedColorChanged(Color c)
        //{
        //    Person p = DataContext as Person;
        //    if (p == null)
        //        return;
        //}
    }
}