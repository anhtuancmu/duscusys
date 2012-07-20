using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedEditor
{
    public class ClientCluster : ClientLinkable
    {
        List<ClientClusterable> _badges = new List<ClientClusterable>();

        public ClientCluster(int id, BoundsProvider boundsProvider) :
            base(id, boundsProvider)
        {            
        }        

        public bool IsEmpty()
        {
            return _badges.Count() == 0;
        }

        public void SetClusterables(List<ClientClusterable> badges)
        {
            _badges = badges;
        }

        public List<ClientClusterable> GetClusterables()
        {
            return _badges;
        }

        public void Add(ClientClusterable badge)
        {
            if (_badges.Contains(badge))
            {
                throw new NotSupportedException("duplicate cluster entry");
            }

            _badges.Add(badge);
            badge.Cluster(GetId());
        }

        public void Remove(ClientClusterable badge)
        {
            _badges.Remove(badge);
            badge.Uncluster();
        }
    }
}
