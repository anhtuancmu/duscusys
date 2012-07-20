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
    public partial class LoginSessionDlg : Window
    {
       // static Session _testSession = null; 
        //public static Session TestSession
        //{
        //    get
        //    {
        //        if(_testSession==null)
        //        {
        //            _testSession = new Session();
        //            _testSession.Name = "Test session";
        //        }
        //        return _testSession;
        //    }
        //}        
        
        public Session SelectedSession = null;
      
        ObservableCollection<Session> _sessions = null;
        public ObservableCollection<Session> Sessions
        {
            get
            {
                if (_sessions == null)
                {
                    _sessions = new ObservableCollection<Session>();

                    DiscCtx ctx = DbCtx.Get();                    
                    foreach (var d in ctx.Session)
                        _sessions.Add(d);

                  //  _sessions.Add(TestSession);                 
                }

                return _sessions;
            }
            set
            {
                _sessions = value;
            }
        }

        public LoginSessionDlg()
        {
            InitializeComponent();

            DataContext = this;

            //SkinManager.ChangeSkin("GreenSkin.xaml", this.Resources);
            SkinManager.ChangeSkin("Blue2Skin.xaml", this.Resources);

            lblVersion.Content = Utils2.VersionString();
        }

        private void lstBxDiscussions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedSession = e.AddedItems[0] as Session;
            Close();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
