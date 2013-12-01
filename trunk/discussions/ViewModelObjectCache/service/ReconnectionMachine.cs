using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Discussions.service
{
    /// <summary>
    /// While this class exists, it tries to connect to service. Once it connects, it provides 
    /// connected proxy. 
    /// </summary>
    //public class ReconnectionMachine : IDisposable
    //{
    //    TdsSvcRef.TdsServiceClient _svcClient;
    //    TdsSvcRef.TdsServiceClient _trialSvcClient;

    //    //when fired with false, gives dead instance of proxy so that listeners can unsubsribe        
    //    Action<bool, TdsSvcRef.TdsServiceClient> _svcConnection;

    //    private readonly Dispatcher _dispatcher;

    //    public TdsSvcRef.TdsServiceClient SvcClient
    //    {
    //        get
    //        {
    //            if (_svcClient != null && _svcClient.State == CommunicationState.Opened)
    //                return _svcClient;
    //            return null;
    //        }
    //    }

    //    public ReconnectionMachine(Dispatcher dispatcher,
    //                               Action<bool, TdsSvcRef.TdsServiceClient> svcConnection)
    //    {
    //        _dispatcher = dispatcher;
    //        _svcConnection = svcConnection;
    //        ConnectionLoop();
    //    }

    //    async void ConnectionLoop()
    //    {
    //        Debug.WriteLine("NetworkManager: ConnectionLoop");

    //        while (_svcClient == null)
    //        {
    //            try
    //            {
    //                //run pre-trial client 
    //                bool preTrialOk = false;
    //                try
    //                {
    //                    var antiDeadlockTrial = new TdsSvcRef.TdsServiceClient();

    //                    //the task doesn't propagate exceptions like TLS/SSL security 
    //                    await Task.Factory.StartNew(antiDeadlockTrial.Open);
    //                    if (antiDeadlockTrial.State == CommunicationState.Opened ||
    //                        antiDeadlockTrial.State == CommunicationState.Opening)
    //                    {
    //                        preTrialOk = true;
    //                        antiDeadlockTrial.Close();
    //                    }
    //                }
    //                catch
    //                {
    //                }

    //                if (preTrialOk)
    //                {
    //                    _trialSvcClient = new TdsSvcRef.TdsServiceClient();

    //                    _trialSvcClient.Open();

    //                    //here we can have _trialSvcClient.State==CommunicationState.Opening (ARM)
    //                    int i = 0;
    //                    while (_trialSvcClient.State != CommunicationState.Opened && ++i < 20)
    //                        await Utils.Delay(100);

    //                    if (_trialSvcClient.State == CommunicationState.Opened)
    //                    {
    //                        _svcClient = _trialSvcClient;
    //                        _trialSvcClient = null;
    //                        _svcClient.InnerChannel.Faulted += InnerChannelOnFaulted;
    //                        //CONNECTION OK
    //                        _svcConnection(true, _svcClient);
    //                        break;
    //                    }
    //                }
    //            }
    //            catch (Exception e)
    //            {
    //                //cannot reference ServiceModel from the store .net profile...
    //                if (e.GetType().FullName == "System.ServiceModel.Security.SecurityNegotiationException")
    //                {
    //                    MessageDlg.Show(
    //                        "Please ensure your system clock is reasonably close to the current time.\n\n" +
    //                        "The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel.\n",
    //                        "Secure connection");
    //                }
    //            }

    //            await Utils.Delay(10 * 1000);//10 sec
    //        }
    //    }

    //    //non-UI thread
    //    private void InnerChannelOnFaulted(object sender, EventArgs args)
    //    {
    //        Debug.WriteLine("RM: InnerChannelOnFaulted");
    //        _dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
    //        {
    //            if (_svcClient != null)
    //            {
    //                //CONNECTION FAILED
    //                _svcConnection(false, _svcClient);//order, let listeners to unlisten
    //                Dispose();
    //            }
    //        }));
    //    }

    //    public void Dispose()
    //    {
    //        if (_svcClient != null)
    //        {
    //            _svcClient.InnerChannel.Faulted -= InnerChannelOnFaulted;
    //            _svcClient.Abort();
    //            _svcClient = null;
    //        }
    //        if (_trialSvcClient != null)
    //        {
    //            _trialSvcClient.Abort();
    //            _trialSvcClient = null;
    //        }
    //        _svcConnection = null;
    //    }
    //}
}