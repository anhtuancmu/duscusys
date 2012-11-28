using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows;
using Discussions.model;
using DiscussionsClientRT;

namespace Discussions.rt
{
    public class UISharedRTClient
    {
        static UISharedRTClient _inst = null;

        public static UISharedRTClient Instance        
        {
            get
            {
                if (_inst == null)
                    _inst = new UISharedRTClient();
                return _inst;
            }
        }
                
        public ClientRT clienRt;
        public delegate void RtTickHandler();
        public RtTickHandler rtTickHandler;

        DispatcherTimer rtTimer;

        public void start(LoginResult login, string DbServer, DeviceType devType)
        {
            int discId;
            if (login.discussion != null)
                discId = login.discussion.Id;
            else
                discId = -1;

            var actorName = login.person!=null ? login.person.Name : "moderator"; 
            var actorDbId = login.person!=null ? login.person.Id   : 0; 
            
            clienRt = new ClientRT(discId,
                                   DbServer,
                                   actorName,
                                   actorDbId,
                                   devType);

            rtTimer = new DispatcherTimer();
            rtTimer.Tick += OnRtServiceTick;
            rtTimer.Interval = TimeSpan.FromMilliseconds(15);
            rtTimer.Start();
        }

        void OnRtServiceTick(object sender, EventArgs e)
        {
            if (rtTickHandler != null)
                rtTickHandler();

            //clienRt.Service();
            try
            {
                clienRt.Service();
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.StackTrace, "Photon error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
    }
}
