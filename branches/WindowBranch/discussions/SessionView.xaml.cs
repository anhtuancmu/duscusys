using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Discussions.DbModel;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for SessionView.xaml
    /// </summary>
    public partial class SessionView : UserControl
    {
        public SessionView()
        {
            InitializeComponent();
        }

        private void DatePicker_DateValidationError_1(object sender, DatePickerDateValidationErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void dateTime_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            checkDateTimes();
        }

        private void dateTime2_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            checkDateTimes();
        }

        private void checkDateTimes()
        {
            if (dateTime.Value == null || dateTime2.Value == null)
                return;

            var start = dateTime.Value.Value;
            var end = dateTime2.Value.Value;
            if (end.CompareTo(start) <= 0)
            {
                MessageDlg.Show("End date/time of session should be greater than start date/time");
                dateTime2.Value = start.Add(TimeSpan.FromHours(1));
            }
        }
    }
}