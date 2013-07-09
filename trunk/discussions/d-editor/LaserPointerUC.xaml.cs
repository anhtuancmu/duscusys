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

        public void SetModel(Color clr, string name)
        {
            gradStop1.Color = gradStop3.Color = clr;
            userLines.BorderBrush = new SolidColorBrush(clr);
            //txtName.Foreground = new SolidColorBrush(clr);
            txtName.Text = name;
        }
    }
}
