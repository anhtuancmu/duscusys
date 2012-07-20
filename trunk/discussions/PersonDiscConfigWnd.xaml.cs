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
using Microsoft.Surface.Presentation.Controls;
using Discussions.DbModel;
using Discussions.model;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for PersonDiscConfigWnd.xaml
    /// </summary>
    public partial class PersonDiscConfigWnd : SurfaceWindow
    {
        Discussion _d;

        Person _p;
        public Person person {
            get
            {
                return _p;
            }
            set
            {
                if (value != null)
                {
                    lblName.Content = value.Name;
                    lblEmail.Content = value.Email;
                }
                 _p=value;
            }
        }

        void onPersonSelected(object selected)
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
                selector1.SelectedSide = (int)SideCode.Neutral;

            personSelector.Set(CtxSingleton.Get().Person, "Name");
        }

        private void SurfaceButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DaoUtils.SetGeneralSide(person, _d, selector1.SelectedSide);
            CtxSingleton.Get().SaveChanges();
            Close();
        }
    }
}
