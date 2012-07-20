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
    /// Interaction logic for SourceUC.xaml
    /// </summary>
    public partial class LargeVideoUC : UserControl
    {
        public LargeVideoUC()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkTarget.Text);
        }

        private void Hyperlink_TouchDown_1(object sender, TouchEventArgs e)
        {
            System.Diagnostics.Process.Start(linkTarget.Text);
        }
    }
}
