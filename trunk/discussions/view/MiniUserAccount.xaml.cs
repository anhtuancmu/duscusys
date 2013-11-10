using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Discussions.view
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