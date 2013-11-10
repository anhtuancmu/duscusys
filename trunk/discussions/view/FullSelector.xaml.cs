using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Discussions.view
{
    public partial class FullSelector : UserControl
    {
        public delegate void OnSelected(object selected);

        public OnSelected onSelected = null;

        private ObservableCollection<object> _choices = new ObservableCollection<object>();

        public ObservableCollection<object> Choices
        {
            get { return _choices; }
            set { _choices = value; }
        }

        public FullSelector()
        {
            InitializeComponent();

            DataContext = this;
        }

        public void Select(object Item)
        {
            if (lstBxChoices.Items.Contains(Item))
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
            if (e.AddedItems != null && e.AddedItems.Count > 0 && onSelected != null)
                onSelected(e.AddedItems[0]);
        }
    }
}