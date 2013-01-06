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
using Discussions.DbModel;
using Discussions.model;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for ReadOnlyParticipantSelector.xaml
    /// </summary>
    public partial class ReadOnlyParticipantSelector : UserControl
    {
        public ReadOnlyParticipantSelector()
        {
            InitializeComponent();

            suggestions.SelectionChanged += SuggestionSelectionChanged;
            suggestions.KeyDown += SuggestionKey;
            suggestions.DisplayMemberPath = "Name";
        }

        private SurfaceListBox suggestions = new SurfaceListBox();

        public Person SelectedPerson
        {
            get
            {
                if (DataContext == null)
                    DataContext = Ctors.NewPerson((string) txtBxName.Content, (string) txtBxEmail.Content);
                EnsureNonNullPerson();
                Person p = DataContext as Person;
                return p;
            }

            set { DataContext = value; }
        }

        private void EnsureNonNullPerson()
        {
            Person p = DataContext as Person;
            if (p == null)
                p = Ctors.NewPerson((string) txtBxName.Content, (string) txtBxEmail.Content);
        }

        private void SuggestionKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OnSelectedSuggestedItem((Person) suggestions.SelectedItem);
        }

        private void SuggestionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
                OnSelectedSuggestedItem((Person) e.AddedItems[0]);
        }

        public void SelectPerson(Person p)
        {
            OnSelectedSuggestedItem(p);
        }

        private void OnSelectedSuggestedItem(Person p)
        {
            txtBxName.Content = p.Name;
            txtBxEmail.Content = p.Email;

            SelectedPerson = p;

            HideSuggestions();
        }

        private void HideSuggestions()
        {
            contentAssistStack.Children.Remove(suggestions);
        }

        private void LookupName()
        {
            suggestions.Items.Clear();

            DiscCtx ctx = CtxSingleton.Get();
            if (ctx == null)
                return;

            foreach (Person p in ctx.Person)
                suggestions.Items.Add(p);

            if (!contentAssistStack.Children.Contains(suggestions))
                contentAssistStack.Children.Add(suggestions);
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            LookupName();
        }
    }
}