using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        public static readonly RoutedEvent CustSelectionEvent = EventManager.RegisterRoutedEvent(
            "CustSelection", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (FileEntryControl));

        public event RoutedEventHandler CustSelection
        {
            add { AddHandler(CustSelectionEvent, value); }
            remove { RemoveHandler(CustSelectionEvent, value); }
        }

        public FileEntryControl()
        {
            InitializeComponent();
        }

        private void OnSingleClick(object sender, InputEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CustSelectionEvent));
            e.Handled = true;
        }

        private void BtnView_OnClick(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(RequestViewEvent));
            e.Handled = true;
        }
    }
}