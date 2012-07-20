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
using System.Windows.Shapes;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for URLDialog.xaml
    /// </summary>
    public partial class InpDialog : Window
    {
        public string Answer = null;

        public InpDialog()
        {
            InitializeComponent();
        }

        public InpDialog(string title, string initialText)
        {
            InitializeComponent();

            Title = title;
            txtBxURL.Text = initialText;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Answer = txtBxURL.Text; //regexp ? 
            Close();
        }
    }
}
