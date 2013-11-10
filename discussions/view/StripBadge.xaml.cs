using System.Windows;
using System.Windows.Controls;

namespace Discussions.view
{
    public partial class StripBadge : UserControl
    {
        public StripBadge()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged_1(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (DataContext == null)
            //    return;

            //var ap = (ArgPoint)DataContext;
            //if (ap.Person.Id == SessionInfo.Get().person.Id)
            //{
            //    txtPoint.Visibility = Visibility.Visible;
            //    lblPoint.Visibility = Visibility.Collapsed;
            //}
        }
    }
}