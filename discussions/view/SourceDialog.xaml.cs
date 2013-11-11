using System.Windows;
using AbstractionLayer;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for URLDialog.xaml
    /// </summary>
    public partial class SourceDialog : PortableWindow
    {
        public string Source = null;

        public SourceDialog()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Source = txtBxSource.Text;
            Close();
        }
    }
}