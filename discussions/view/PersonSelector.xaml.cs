using System.Windows.Controls;
using System.Windows.Input;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for PersonSelector.xaml
    /// </summary>
    public partial class PersonSelector : UserControl
    {
        public PersonSelector()
        {
            InitializeComponent();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void Rectangle_TouchDown(object sender, TouchEventArgs e)
        {
        }

        //void SelectedColorChanged(Color c)
        //{
        //    Person p = DataContext as Person;
        //    if (p == null)
        //        return;
        //}
    }
}