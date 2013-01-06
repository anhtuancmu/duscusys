using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Discussions.Metro
{
    public partial class TopicTile : UserControl
    {
        private MetroFiller.Launcher _launcher;

        public MetroFiller.Launcher LaunchDel
        {
            get { return _launcher; }
            set
            {
                _launcher = value;
                LayoutRoot.Opacity = value != null ? 1 : 0.3;
            }
        }

        public TopicTile()
        {
            InitializeComponent();
            LaunchDel = null;
        }

        public string BtnTitle
        {
            set { TileTxtBlck.Text = value; }
        }

        private void LayoutRoot_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LaunchDel == null)
                return;

            LaunchDel();
        }

        private void LayoutRoot_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            if (LaunchDel == null)
                return;

            LaunchDel();
        }
    }
}