using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedEditor
{
    public class ClientClusterable : ClientLinkable
    {
        private int _clusterId = -1;

        public int ClusterId
        {
            get { return _clusterId; }
        }

        //if true, local cluster operation pending, ignore it local interactions until it's reset by server response 
        private bool _busy = false;

        public bool Busy
        {
            get { return _busy; }
        }

        public ClientClusterable(int id, BoundsProvider boundsProvider) :
            base(id, boundsProvider)
        {
        }

        public void SetBusy()
        {
            if (_busy)
                throw new NotSupportedException("already busy");

            _busy = true;
        }

        public bool IsInCluster()
        {
            return _clusterId != -1;
        }

        public void Cluster(int clusterId)
        {
            this._clusterId = clusterId;
            _busy = false;
        }

        public void Uncluster()
        {
            this._clusterId = -1;
            _busy = false;
        }
    }
}