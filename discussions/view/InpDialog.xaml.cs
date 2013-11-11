using System.Windows;
using AbstractionLayer;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for URLDialog.xaml
    /// </summary>
    public partial class InpDialog : PortableWindow
    {
        public string Answer = null;

        public InpDialog()
        {
            InitializeComponent();
        }

        public InpDialog(string title, string initialText)
        {
            InitializeComponent();

            Title2 = title;
            txtBxURL.Text = initialText;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Answer = txtBxURL.Text; //regexp ? 
            Close();
        }
    }
}