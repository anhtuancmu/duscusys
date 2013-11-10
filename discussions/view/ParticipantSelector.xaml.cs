using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Discussions.DbModel;
using Discussions.model;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions.view
{
    /// <summary>
    /// Логика взаимодействия для ParticipantUC.xaml
    /// </summary>
    public partial class ParticipantSelector : UserControl
    {
        public delegate void ParticipantChanged();

        public ParticipantChanged changed { get; set; }

        private bool manualEnteringDetected = false;
        private bool blockTextChangeHandlers = false;

        private SurfaceListBox suggestions = new SurfaceListBox();

        public Person SelectedPerson
        {
            get
            {
                if (DataContext == null)
                    DataContext = Ctors.NewPerson(txtBxName.Text, txtBxEmail.Text);
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
                p = Ctors.NewPerson(txtBxName.Text, txtBxEmail.Text);
        }

        public ParticipantSelector()
        {
            InitializeComponent();

            suggestions.SelectionChanged += SuggestionSelectionChanged;
            suggestions.KeyDown += SuggestionKey;
            suggestions.DisplayMemberPath = "Name";
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
            blockTextChangeHandlers = true;
            txtBxName.Text = p.Name;
            txtBxEmail.Text = p.Email;

            txtBxName.IsReadOnly = true;
            txtBxEmail.IsReadOnly = true;

            blockTextChangeHandlers = false;

            SelectedPerson = p;

            if (changed != null)
                changed();

            HideSuggestions();
        }

        private void HideSuggestions()
        {
            contentAssistStack.Children.Remove(suggestions);
        }

        private void LookupName(string Name, string Email)
        {
            suggestions.Items.Clear();

            DiscCtx ctx = PublicBoardCtx.Get();
            if (ctx == null)
                return;

            if (Name == null && Email == null)
            {
                foreach (Person p in ctx.Person)
                    suggestions.Items.Add(p);
            }
            else if (Name != null && Name != "")
            {
                IQueryable<Person> lookup =
                    from p in ctx.Person
                    where p.Name.IndexOf(Name) != -1
                    select p;

                foreach (Person p in lookup)
                    suggestions.Items.Add(p);
            }
            else if (Email != null && Email != "")
            {
                IQueryable<Person> lookup =
                    from p in ctx.Person
                    where p.Email.IndexOf(Email) != -1
                    select p;

                foreach (Person p in lookup)
                    suggestions.Items.Add(p);
            }

            if (suggestions.Items.Count > 1)
            {
                if (!contentAssistStack.Children.Contains(suggestions))
                    contentAssistStack.Children.Add(suggestions);
            }
            else if (suggestions.Items.Count == 1)
            {
                if ((Name != null && Name == ((Person) suggestions.Items[0]).Name) ||
                    (Email != null && Email == ((Person) suggestions.Items[0]).Email))
                {
                    SelectPerson((Person) suggestions.Items[0]);
                }
                else
                {
                    if (!contentAssistStack.Children.Contains(suggestions))
                        contentAssistStack.Children.Add(suggestions);
                }
            }
            else
                HideSuggestions();
        }

        private void txtBxName_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            EnsureNonNullPerson();
            if (txtBxName != null && manualEnteringDetected && !blockTextChangeHandlers)
            {
                LookupName(txtBxName.Text, null);
            }
            if (SelectedPerson != null)
            {
                SelectedPerson.Name = txtBxName.Text;
            }
        }

        private void txtBxEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnsureNonNullPerson();
            if (txtBxEmail != null && manualEnteringDetected && !blockTextChangeHandlers)
                LookupName(null, txtBxEmail.Text);
            if (SelectedPerson != null)
            {
                SelectedPerson.Email = txtBxEmail.Text;
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            FullSelector selector = new FullSelector();
            selector.Set(PublicBoardCtx.Get().Person, "Name");
        }

        private void txtBxName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            manualEnteringDetected = true;
        }

        private void txtBxEmail_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            manualEnteringDetected = true;
        }
    }
}