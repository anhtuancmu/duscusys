using System.Windows;
using AbstractionLayer;
using Discussions.ctx;
using Discussions.DbModel;
using Discussions.model;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for PersonDiscConfigWnd.xaml
    /// </summary>
    public partial class PersonDiscConfigWnd : PortableWindow
    {
        private Discussion _d;

        private Person _p;

        public Person person
        {
            get { return _p; }
            set
            {
                if (value != null)
                {
                    lblName.Content = value.Name;
                    lblEmail.Content = value.Email;
                }
                _p = value;
            }
        }

        private void onPersonSelected(object selected)
        {
            var p = selected as Person;
            if (p == null)
                return;

            person = p;
        }

        public PersonDiscConfigWnd(Discussion d, Person p)
        {
            InitializeComponent();

            this.WindowState = WindowState.Normal;
            this.Width = 336;

            personSelector.onSelected += onPersonSelected;

            _d = d;
            person = p;
            lblDiscussion.Content = "Discussion: " + d.Subject;

            int currentSide = DaoUtils.GetGeneralSide(p, d);
            if (currentSide != -1)
                selector1.SelectedSide = currentSide;
            else
                selector1.SelectedSide = (int) SideCode.Neutral;

            personSelector.Set(PublicBoardCtx.Get().Person, "Name");
        }

        private void SurfaceButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DaoUtils.SetGeneralSide(person, _d, selector1.SelectedSide);
            PublicBoardCtx.Get().SaveChanges();
            Close();
        }
    }
}