using System.Windows.Controls;
using System.Windows.Media;

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