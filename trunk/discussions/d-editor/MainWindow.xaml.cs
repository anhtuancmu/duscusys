using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Discussions;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Discussions.RTModel.Model;
using LoginEngine;
using Microsoft.Surface.Presentation.Controls;

namespace DistributedEditor
{  
    public partial class MainWindow : SurfaceWindow
    {
        UISharedRTClient rt = UISharedRTClient.Instance;

        string recentLog { get; set; }

        public EditorWndCtx wndCtx {get;set;}

        LoginResult loginInfo;        

        public MainWindow()
        {
            InitializeComponent();

            //var loginInfo = LoginDriver.Run(LoginFlow.Regular);
            loginInfo = testLoginStub();

            if (loginInfo == null)
            {
                Application.Current.Shutdown();
                return;
            }
            
            palette._ownerId = loginInfo.person.Id;
            inkPalette.InkCanvas = inkCanv;            

            rt.start(loginInfo, DbCtx.Get().Connection.DataSource, DeviceType.Wpf);
            setListeners(true);                         
        }

        LoginResult testLoginStub()
        {
            var loginRes = new LoginResult();        
            loginRes.discussion = DbCtx.Get().Discussion.FirstOrDefault(d0=>d0.Subject.StartsWith("d-editor"));
            loginRes.person = DbCtx.Get().Person.FirstOrDefault(p0=>p0.Name.StartsWith("moder"));
            if (loginRes.person.Online)
                loginRes.person = DbCtx.Get().Person.FirstOrDefault(p0 => p0.Name=="usr");
            if (loginRes.person.Online)
                loginRes.person = DbCtx.Get().Person.FirstOrDefault(p0 => p0.Name == "usr2");

            return loginRes;
        }
        
        void setListeners(bool doSet)
        {
            if (rt.clienRt == null)
                return;

            if (doSet)
                rt.clienRt.userJoins += userJoins;
            else
                rt.clienRt.userJoins -= userJoins; 
        }

        void userJoins(DiscUser usr)
        {
            if (usr.usrDbId == loginInfo.person.Id && wndCtx == null)
            {
                var topic = DbCtx.Get().Topic.FirstOrDefault(t0 => t0.Name.StartsWith("d-editor"));            
                wndCtx = new EditorWndCtx(scene,
                                          inkCanv,
                                          palette,
                                          inkPalette,  
                                          this,//surface window for focus fix                                      
                                          topic.Id,
                                          topic.Discussion.Id);
                DataContext = this;   
                rt.clienRt.SendInitialSceneLoadRequest(topic.Id);     
            }
        }

        //void OnFinishDrawingSelected()
        //{
        //    if (inkPalette.Visibility != Visibility.Visible)
        //        inkPalette.Visibility = Visibility.Visible;
        //    else
        //        inkPalette.Visibility = Visibility.Hidden;
        //}

        private void Window_Closed_1(object sender, EventArgs e)
        {
            setListeners(false);

            rt.clienRt.SendLiveRequest();
            rt.clienRt.SendLiveRequest();
            rt.clienRt.SendLiveRequest();
        }      
    }
}
