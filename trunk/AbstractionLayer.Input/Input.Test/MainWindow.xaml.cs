using System.Windows;

namespace AbstractionLayer.Input.Input.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var wnd = new InputWindow();
            wnd.Show();

            var isw = new InputSurfaceWindow();
            isw.Show();
        }
    }
}
