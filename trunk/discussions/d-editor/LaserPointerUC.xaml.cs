using System.Windows.Controls;
using System.Windows.Media;

namespace CustomCursor
{
    /// <summary>
    /// Interaction logic for LaserPointerUC.xaml
    /// </summary>
    public partial class LaserPointerUC : UserControl
    {
        public LaserPointerUC()
        {
            InitializeComponent();
        }

        public void SetColor(Color clr)
        {
            gradStop1.Color = gradStop3.Color = clr;
        }
    }
}
