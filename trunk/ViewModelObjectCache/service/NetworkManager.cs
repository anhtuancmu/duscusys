using System;
using System.Diagnostics;
using System.Windows.Threading;
using Discussions.Annotations;

namespace Discussions.service
{
    //connection/reconnection logic
    //NetworkManager lives for program life time    
    //Cases with SvcAvailable==True are subset of cases InternetAvailable==True
    public class NetworkManager : IDisposable
    {
        Action<NetworkManager> InternetStatus;

        public event Action<bool, TdsSvcRef.TdsServiceClient> SvcConnection;

        private bool _internetAvailable;
        public bool InternetAvailable
        {
            get { return _internetAvailable; }
        }

        public bool SvcAvailable
        {
            get { return SvcClient != null; }
        }

        //exists only when Internet is not available and we try to establish connection
        private ReconnectionMachine _machine;

        private readonly Dispatcher _dispatcher;

        public NetworkManager(Dispatcher dispatcher,
                              Action<bool, TdsSvcRef.TdsServiceClient> svcConnection)
        {
            _dispatcher = dispatcher;
            this.SvcConnection += svcConnection;
        }

        public void Dispose()
        {
            if (InternetAvailable)
                SwitchToOffline();

            DisposeMachine();

            SvcConnection = null;
        }

        async void svcConnection(bool ok, TdsSvcRef.TdsServiceClient client)
        {
            SvcConnection(ok, client);

            if (!ok)
            {
                DisposeMachine();

                //if channel faults, we'd like to avoid instant reconnection
                await Utils.Delay(3000);

                //start new machine
                StartMachine();
            }
        }

        void DisposeMachine()
        {
            if (_machine != null)
            {
                _machine.Dispose();
                _machine = null;
            }
        }

        /// <summary>
        /// Don't cache it, it can change any time when connection status changes.    
        /// </summary>
        [CanBeNull]
        public TdsSvcRef.TdsServiceClient SvcClient
        {
            get
            {
                return _machine != null ? _machine.SvcClient : null;
            }
        }

        /// <summary>
        /// Force status to update, e.g. after program resumes. 
        /// </summary>
        public void Refresh()
        {
            SwitchToOnline();
        }

        public void DisconnectFromService()
        {
            SvcConnection(false, SvcClient);
            DisposeMachine();
        }

        public void OnResumed()
        {
            StartMachine();
        }

        void SwitchToOffline()
        {
            Debug.WriteLine("NetworkManager: SwitchToOffline");

            DisconnectFromService();

            _internetAvailable = false;

            if (InternetStatus != null)
                InternetStatus(this);
        }

        void SwitchToOnline()
        {
            Debug.WriteLine("NetworkManager: SwitchToOnline");

            StartMachine();

            _internetAvailable = true;

            if (InternetStatus != null)
                InternetStatus(this);
        }

        private void StartMachine()
        {
            if (_machine == null)
                _machine = new ReconnectionMachine(_dispatcher, svcConnection);
        }
    }
}