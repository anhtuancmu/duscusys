using System.Windows;
using System.Windows.Controls;

namespace CloudStorage
{
    public partial class FileEntryControl : UserControl
    {
        public static readonly RoutedEvent RequestViewEvent = EventManager.RegisterRoutedEvent(
            "RequestView", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (FileEntryControl));

        public event RoutedEventHandler RequestView
        {
            add { AddHandler(RequestViewEvent, value); }
            remove { RemoveHandler(RequestViewEvent, value); }
        }

        public FileEntryControl()
        {
            InitializeComponent();
        }

        private void BtnView_OnClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(RequestViewEvent));
            e.Handled = true;
        }
    }
}