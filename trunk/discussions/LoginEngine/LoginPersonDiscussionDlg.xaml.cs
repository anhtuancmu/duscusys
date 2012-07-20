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
    /// Interaction logic for LoginPerson.xaml
    /// </summary>
    public partial class LoginPersonDiscussionDlg : Window
    {
        public bool BackClicked = false;
        
        public Person SelectedPerson = null;
        
        ObservableCollection<Person> _persons = null;
        public ObservableCollection<Person> Persons
        {
            get
            {
                return _persons;
            }
            set
            {
                _persons = value;
            }
        }


        static Discussion _dummyDisc = null;
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

        ObservableCollection<Discussion> _discussions = new ObservableCollection<Discussion>();
        public ObservableCollection<Discussion> Discussions
        {
            get
            {
                return _discussions;
            }
            set
            {
                _discussions = value;
            }
        }   

        //Session _selectedSession = null;

        public LoginPersonDiscussionDlg(Session s)
        {
            InitializeComponent();

            throw new NotSupportedException();

            //_selectedSession = s; 

            //DataContext = this;

            //lblVersion.Content = Utils2.VersionString();

            ////SkinManager.ChangeSkin("GreenSkin.xaml", this.Resources);
            //SkinManager.ChangeSkin("Blue2Skin.xaml", this.Resources);

            ////load persons in session 
            //_persons = new ObservableCollection<Person>();
            //if (s == LoginSessionDlg.TestSession)
            //{
            //    //for Test Session add all persons
            //    foreach (var p in DbCtx.Get().Person)
            //        _persons.Add(p);            
            //}
            //else
            //{
            //    //for all other sessions add users in selected session 
            //    foreach (var p in s.Person)
            //        _persons.Add(p);
            //}
        }

        private void lstBxPersons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedPerson = e.AddedItems[0] as Person;

            decoration.SetGreetingName(SelectedPerson.Name);
           
            //enum all discussions of the person
            if (SelectedPerson != null)
            {
                DiscCtx ctx = DbCtx.Get();
                IQueryable<Discussion> myDiscussions =
                                (from t in ctx.Topic
                                 where t.Person.Any(p0 => p0.Id == SelectedPerson.Id)
                                 select t.Discussion).Distinct();

                _discussions.Clear();
                foreach (var d in myDiscussions)
                    _discussions.Add(d);

                //add dummy discussion for moderator   
                if (SelectedPerson.Name.StartsWith("moder"))
                {
                    _discussions.Add(DummyDiscussion);
                }

                lblSelDiscussion.Visibility = Visibility.Visible;
            }
            else
            {
                lblSelDiscussion.Visibility = Visibility.Hidden;
            }
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
