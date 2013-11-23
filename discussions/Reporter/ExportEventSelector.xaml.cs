using System.Windows;
using AbstractionLayer;

namespace Reporter
{
    /// <summary>
    /// Interaction logic for ExportEventSelector.xaml
    /// </summary>
    public partial class ExportEventSelector : PortableWindow
    {
        private ExportEventSelectorVM _model;

        public ExportEventSelector(ExportEventSelectorVM vm)
        {
            InitializeComponent();

            DataContext = _model = vm;
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
