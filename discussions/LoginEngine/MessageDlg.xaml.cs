using AbstractionLayer;
using System.Windows;
using System.Media;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for BusyWindow.xaml
    /// </summary>
    public partial class MessageDlg : PortableWindow
    {
        public MessageDlg(string Message, string title="Message")
        {
            InitializeComponent();

            message.Text = Message;
            Title2 = title;
            Width = SystemParameters.PrimaryScreenWidth;

            SystemSounds.Asterisk.Play();
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void Show(string message, string title)
        {
            var dlg = new MessageDlg(message, title);
            dlg.ShowDialog();
        }

        public static void Show(string message)
        {
            var dlg = new MessageDlg(message);
            dlg.ShowDialog();
        }

        public static void Show(string message, MessageBoxButton i1, MessageBoxImage i2)
        {
            Show(message);            
        }

        public static void Show(string message, string title, MessageBoxButton i1, MessageBoxImage i2)
        {
            Show(message, title);
        }
    }
}