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

namespace DistributedEditor
{
    /// <summary>
    /// Interaction logic for ClusterButton.xaml
    /// </summary>
    public partial class ClusterButton : UserControl
    {
        public ClusterButton()
        {
            InitializeComponent();
        }

        public void SetBrush(Brush b)
        {
            btn.Background = b;
        }
    }
}