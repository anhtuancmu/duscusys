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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;

namespace Discussions
{
    public partial class FullSelector : UserControl
    {
        public delegate void OnSelected(object selected);
        public OnSelected onSelected = null;

        ObservableCollection<object> _choices = new ObservableCollection<object>(); 
        public ObservableCollection<object> Choices
        {
            get
            {
                return _choices;
            }
            set
            {
                _choices = value;
            }
        }

        public FullSelector()
        {
            InitializeComponent();

            DataContext = this; 
        }

        public void Select(object Item)
        {
            if(lstBxChoices.Items.Contains(Item))
                lstBxChoices.SelectedItem = Item;
        }

        public object SelectedItem()
        {
            return lstBxChoices.SelectedItem;
        }

        public void Set(IEnumerable<object> choices, string DisplayMemberPath)
        {
            this.Choices.Clear();
            foreach (var choice in choices)
                this.Choices.Add(choice);
            lstBxChoices.DisplayMemberPath = DisplayMemberPath;
        }

        public void Set(ObservableCollection<object> choices, string DisplayMemberPath)
        {
            lstBxChoices.DisplayMemberPath = DisplayMemberPath;
            Choices = choices; 
        }

        private void lstBxChoices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0 && onSelected!=null)
                onSelected(e.AddedItems[0]);
        }
    }
}