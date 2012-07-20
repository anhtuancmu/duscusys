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
using Discussions.model;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for SideSelector.xaml
    /// </summary>
    public partial class SideSelector : UserControl
    {
        public int SelectedSide
        {
            get { return (int)GetValue(SideProperty); }
            set { SetValue(SideProperty, value); }
        }

        public static readonly DependencyProperty SideProperty =
        DependencyProperty.Register("SelectedSide",
                                    typeof(int),
                                    typeof(SideSelector),
                                    new FrameworkPropertyMetadata(OnSideChanged)
                                    );

        private static void OnSideChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SideSelector control = (SideSelector)d;
            
            if (e.NewValue == null)
                control.SelectedSide = (int)SideCode.Neutral;
            else
                control.SelectedSide = (int)e.NewValue;

            switch (control.SelectedSide)
            {
                case (int)SideCode.Neutral:
                    control.btnNeutral.IsChecked = true;
                    break;

                case (int)SideCode.Cons:
                    control.btnCons.IsChecked = true;
                    break;

                case (int)SideCode.Pros:
                    control.btnPros.IsChecked = true;
                    break;

                default:
                    throw new NotSupportedException();
            }           
        }

        public delegate void OnSelected(SideCode code);
        public OnSelected onSelected;

        public SideSelector()
        {
            InitializeComponent();
        }

        private void btnPros_Checked(object sender, RoutedEventArgs e)
        {
            SelectedSide = (int)SideCode.Pros;
            if (onSelected!=null)
                onSelected(SideCode.Pros);
        }

        private void btnCons_Checked(object sender, RoutedEventArgs e)
        {
            SelectedSide = (int)SideCode.Cons;
            if (onSelected != null)
                onSelected(SideCode.Cons);
        }

        private void btnNeutral_Checked(object sender, RoutedEventArgs e)
        {
            SelectedSide = (int)SideCode.Neutral;
            if (onSelected != null)
                onSelected(SideCode.Neutral);
        }
    }
}
