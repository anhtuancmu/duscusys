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
using System.Windows.Shapes;
using Discussions.model;
using Discussions.DbModel;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for SpeakerLogin.xaml
    /// </summary>
    public partial class SpeakerLogin : Microsoft.Surface.Presentation.Controls.SurfaceWindow
    {
        private Person _person = null;

        public Person SelectedPerson
        {
            get
            {
                Person p = _person;
                return p;
            }
        }

        public bool ok = false;

        public SideCode SelectedSide
        {
            get { return (SideCode) sideSelector.SelectedSide; }
        }

        private Discussion _discussion = null;

        public Discussion SelectedDiscussion
        {
            get { return _discussion; }
        }

        public void EnableSideSelector(bool value)
        {
            sideSelector.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        public SpeakerLogin()
        {
            InitializeComponent();

            participantSelector.onSelected += ParticipantChanged;

            var persons =
                from p in CtxSingleton.Get().Person
                where p.Name != "Name"
                select p;

            participantSelector.Set(persons, "Name");

            discSelector.onSelected += DiscussionChanged;
        }

        private void ParticipantChanged(object selected)
        {
            _discussion = null;
            _person = selected as Person;

            //enum all discussions of the person
            DiscCtx ctx = CtxSingleton.Get();
            IQueryable<Discussion> lookup =
                (from t in ctx.Topic
                 where t.Person.Any(p0 => p0.Id == _person.Id)
                 select t.Discussion).Distinct();

            discSelector.Set(lookup, "Subject");
            discSelector.IsEnabled = true;
        }

        private void DiscussionChanged(object selected)
        {
            _discussion = selected as Discussion;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            ok = true;
            Close();
        }
    }
}