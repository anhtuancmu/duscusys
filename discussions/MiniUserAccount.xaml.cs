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

namespace Discussions
{
    /// <summary>
    /// Interaction logic for UserAccount.xaml
    /// </summary>
    public partial class MiniUserAccount : UserControl
    {
        public MiniUserAccount()
        {
            InitializeComponent();
        }

        public delegate void PointDown(bool name);

        public PointDown pointDown = null;

        public Brush NameColor
        {
            set { lblName.Foreground = value; }
        }

        private void avaTouchDown(object sender, TouchEventArgs e)
        {
            if (pointDown != null)
                pointDown(false);
        }

        private void imgAvatar_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            if (pointDown != null)
                pointDown(false);
        }

        private void nameTouchDown(object sender, TouchEventArgs e)
        {
            if (pointDown != null)
                pointDown(true);
        }

        private void nameMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (pointDown != null)
                pointDown(true);
        }
    }
}