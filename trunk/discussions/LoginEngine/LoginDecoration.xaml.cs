using System;
using System.Collections.Generic;
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

namespace Discussions
{
    /// <summary>
    /// Interaction logic for LoginDecoration.xaml
    /// </summary>
    public partial class LoginDecoration : UserControl
    {
        public LoginDecoration()
        {
            this.InitializeComponent();
        }

        public void SetGreetingName(string name)
        {
            lblName.Content = name;
            lblName.Visibility = Visibility.Visible;
        }
    }
}