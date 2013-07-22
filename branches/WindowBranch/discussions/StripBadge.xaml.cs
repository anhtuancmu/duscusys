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
using Discussions.model;
using System.IO;
using Microsoft.Surface;
using Microsoft.Surface.Presentation.Input;
using System.Diagnostics;
using VE2;

namespace Discussions
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