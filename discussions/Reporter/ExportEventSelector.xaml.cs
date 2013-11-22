using System.Windows;
using AbstractionLayer;

namespace Reporter
{
    /// <summary>
    /// Interaction logic for ExportEventSelector.xaml
    /// </summary>
    public partial class ExportEventSelector : PortableWindow
    {
        private readonly ExportEventSelectorVM _model;

        public ExportEventSelectorVM Result
        {
            get
            {
                return _model;
            }
        }

        public ExportEventSelector()
        {
            InitializeComponent();

            DataContext = _model = new ExportEventSelectorVM();
        }

        private void BtnOk_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
