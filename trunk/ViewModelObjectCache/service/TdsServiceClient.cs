using System.Windows.Threading;
using Discussions.Annotations;

namespace Discussions.service
{
    public class TdsServiceClient
    {
        private readonly NetworkManager _netMgr;

        public TdsServiceClient(Dispatcher dispatcher)
        {
            _netMgr = new NetworkManager(dispatcher, SvcConnection);      
        }

        private void SvcConnection(bool connOk, TdsSvcRef.TdsServiceClient client)
        {
            if (connOk)
            {

            }
            else
            {
                
            }
        }

        [CanBeNull]
        public TdsSvcRef.TdsServiceClient Proxy
        {
            get
            {
                return _netMgr.SvcClient;
            }
        }
    }
}