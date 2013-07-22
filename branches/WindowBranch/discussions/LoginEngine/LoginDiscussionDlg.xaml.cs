using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Discussions.DbModel;
using LoginEngine;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for LoginDiscussionDlg.xaml
    /// </summary>
    public partial class LoginDiscussionDlg : Window
    {
        private static Discussion _dummyDisc = null;

        public static Discussion DummyDiscussion
        {
            get
            {
                if (_dummyDisc == null)
                {
                    _dummyDisc = new Discussion();
                    _dummyDisc.Subject = "No discussion(moderator only)";
                }

                return _dummyDisc;
            }
        }

        public Discussion SelectedDiscussion = null;
        public bool BackClicked = false;

        private Person _selectedPerson = null;

        private ObservableCollection<Discussion> _discussions = null;

        public ObservableCollection<Discussion> Discussions
        {
            get
            {
                if (_discussions == null)
                {
                    _discussions = new ObservableCollection<Discussion>();

                    //enum all discussions of the person
                    if (_selectedPerson != null)
                    {
                        DiscCtx ctx = DbCtx.Get();
                        IQueryable<Discussion> myDiscussions =
                            (from t in ctx.Topic
                             where t.Person.Any(p0 => p0.Id == _selectedPerson.Id)
                             select t.Discussion).Distinct();

                        foreach (var d in myDiscussions)
                            _discussions.Add(d);

                        //add dummy discussion for moderator   
                        if (_selectedPerson.Name.StartsWith("moder"))
                        {
                            _discussions.Add(DummyDiscussion);
                        }
                    }
                    else
                    {
                        //we are parameterized by name, not person 
                        //show all discussions 
                        _discussions = new ObservableCollection<Discussion>(DbCtx.Get().Discussion);
                    }
                }

                return _discussions;
            }
            set { _discussions = value; }
        }

        public LoginDiscussionDlg(Person selectedPerson)
        {
            _selectedPerson = selectedPerson;

            InitializeComponent();

            DataContext = this;

            SkinManager.ChangeSkin("Blue2Skin.xaml", this.Resources);

            if (selectedPerson != null)
                decorations.SetGreetingName(selectedPerson.Name);
        }

        public LoginDiscussionDlg(string Name)
        {
            InitializeComponent();

            DataContext = this;

            SkinManager.ChangeSkin("Blue2Skin.xaml", this.Resources);

            decorations.SetGreetingName(Name);
        }

        private void lstBxDiscussions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedDiscussion = e.AddedItems[0] as Discussion;
            Close();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            BackClicked = true;
            Close();
        }
    }
}