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
using Microsoft.Surface.Presentation.Controls;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for SurfaceCombobox.xaml
    /// </summary>
    public partial class SurfaceCombobox : UserControl
    {
        public delegate void Selected(object selected);

        public Selected OnSelected { get; set; }

        public object SelectedItem
        {
            get { return lstBxChoices.SelectedItem; }
        }

        public SurfaceCombobox()
        {
            InitializeComponent();
        }

        public void SetChoices(IEnumerable<object> choices, string displayMemberPath, object itemToSelect = null)
        {
            if (displayMemberPath != null)
                lstBxChoices.DisplayMemberPath = displayMemberPath;

            lstBxChoices.Items.Clear();
            foreach (var choice in choices)
            {
                lstBxChoices.Items.Add(choice);
            }

            if (itemToSelect != null)
            {
                if (lstBxChoices.Items.Contains(itemToSelect))
                {
                    lstBxChoices.SelectedItem = itemToSelect;
                    select(itemToSelect);
                }
            }
            else if (lstBxChoices.Items.Count > 0)
            {
                lstBxChoices.SelectedItem = lstBxChoices.Items[0];
                select(lstBxChoices.Items[0]);
            }
        }

        private void lstBxChoices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object selectedIt = null;

            if (e.AddedItems != null && e.AddedItems.Count > 0)
                selectedIt = e.AddedItems[0];

            select(selectedIt);
            lstBxChoices.Visibility = Visibility.Hidden;
        }

        private void select(object s)
        {
            if (OnSelected != null)
            {
                OnSelected(s);
            }
        }

        private void btnSelect_Click_1(object sender, RoutedEventArgs e)
        {
            if (lstBxChoices.Visibility == Visibility.Hidden)
                lstBxChoices.Visibility = Visibility.Visible;
            else
                lstBxChoices.Visibility = Visibility.Hidden;
        }

        private void lstBxChoices_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SurfaceListBoxItem item = Utils.findLBIUnderTouch(e);
            if (item != null && item.Content != null)
                select(item.Content);

            lstBxChoices.Visibility = Visibility.Hidden;
        }

        private void lstBxChoices_TouchUp(object sender, TouchEventArgs e)
        {
            SurfaceListBoxItem item = Utils.findLBIUnderTouch(e);
            if (item != null && item.Content != null)
                select(item.Content);

            lstBxChoices.Visibility = Visibility.Hidden;
        }
    }
}