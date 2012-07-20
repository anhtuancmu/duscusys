using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Discussions.RTModel.Model;
using Discussions.RTModel.Operations;
using DistributedEditor;
using ExitGames.Logging;
using Lite;
using Photon.SocketServer;

namespace Discussions.RTModel
{
    public class Edge
    {
        public Linkable curr;
        public Linkable next;
        public int linkShapeId;
        public bool forward;

        public Edge(Linkable curr, Linkable next, int linkShapeId, bool forward)
        {
            this.curr = curr;
            this.next = next;
            this.linkShapeId = linkShapeId;
            this.forward = forward;
        }
    }

    public class Linkable 
    {
        readonly int _id;
        List<Edge> _edges = new List<Edge>();

        public Linkable(int id)
        {
            _id = id; 
        }
        
        public int GetId()
        {
            return _id;
        }

        public IEnumerable<Edge> GetEdge()
        {
            return _edges;
        }

        public void AddEdge(Edge next)
        {
            _edges.Add(next);
        }

        public void RemoveEdge(Edge next)
        {
            _edges.Remove(next);
        }

        public Edge RemoveEdge(Linkable next)
        {
            Edge toRemove = null;
            foreach (var e in _edges)
                if (e.next == next)
                    toRemove = e;

            _edges.Remove(toRemove);
            return toRemove;
        }

        public bool HasAdjacent(Linkable next)
        {
            return _edges.FirstOrDefault(edge=>edge.next==next)!=null;            
        }
    }

    public class Clusterable : Linkable
    {
        public const int UNDEFINED_CLUSTER = -1;

        Cluster _cluster = null;
        
        public Clusterable(int id) :
            base(id)
        {
        }

        public Cluster GetCluster()
        {
            if (!IsInCluster())
                throw new NotSupportedException("clusterable is out of clusters");

            return _cluster;
        }

        public bool IsInCluster()
        {
            return _cluster != null;
        }

        public void Cluster(Cluster cluster)
        {
            if (cluster == null)
                throw new NotSupportedException("undefined cluster");

            _cluster = cluster;
        }

        public void Uncluster()
        {
            if (_cluster == null)
                throw new NotSupportedException("already unclustered");

            _cluster = null;
        }
    }   

    public class Cluster : Linkable
    {
        List<Clusterable> _badges = new List<Clusterable>();

        public Cluster(int id):
            base(id)
        {
        }

        public IEnumerable<Clusterable> GetClusterables()
        {
            return _badges;
        }

        public void Add(Clusterable badge)
        {
            _badges.Add(badge);
        }

        public void Remove(Clusterable badge)
        {
            _badges.Remove(badge);
        }

        public bool IsEmpty()
        {
            return _badges.Count() == 0;
        }
    }

    public class ClusterTopology
    {
        static readonly ILogger _log = LogManager.GetCurrentClassLogger();
        
        //Id to clusters
        Dictionary<int, Cluster> _clusters = new Dictionary<int, Cluster>();

        //badge id to Clusterables
        Dictionary<int, Clusterable> _badges = new Dictionary<int, Clusterable>();

        //maps IDs of link shapes to forward edges
        Dictionary<int, Edge> _forwardEdges = new Dictionary<int, Edge>();

        readonly DiscussionRoom _room;

        public ClusterTopology(DiscussionRoom room, BinaryReader reader)
        {
            _room = room;

            Read(reader);
        }

        public delegate void OnLinkableDeleted(Linkable end, int usrId);
        public OnLinkableDeleted onLinkableDeleted;

        public delegate void OnLinkRemove(Linkable end1, Linkable end2, int linkShapeId, int usrId);
        public OnLinkRemove onLinkRemove;

        public delegate void OnUnclusterBadge(Clusterable badge, Cluster cluster, int userId);
        public OnUnclusterBadge onUnclusterBadge;

        #region linking

        public Edge GetForwardEdge(int linkShapeId)
        {
            return _forwardEdges[linkShapeId];
        }

        public Cluster GetCluster(int id)
        {
            return _clusters[id];
        }

        public Linkable GetLinkable(int id)
        {
            //IDs of clusters and badges don't intersect

            if (_badges.ContainsKey(id))
                return _badges[id];
            else
                return _clusters[id];
        }
     
        public void Link(Linkable end1, Linkable end2, int linkShapeId)
        {
            if (end1 == end2)
                throw new NotSupportedException("no self loops");

            if (end1.HasAdjacent(end2))
                throw new NotSupportedException("no multiedges");

            var fwd = new Edge(end1, end2, linkShapeId, true);
            _forwardEdges.Add(linkShapeId, fwd);
            end1.AddEdge(fwd);           
            end2.AddEdge(new Edge(end2, end1, linkShapeId, false));                   
        }

        public void Link(int end1, int end2, int linkShapeId)
        {
            Link(GetLinkable(end1), GetLinkable(end2), linkShapeId);
        }

        public void Unlink(Linkable end1, Linkable end2, int usrId)
        {
            if (end1 == end2)
                throw new NotSupportedException("no self loops");
            
            if (!end1.HasAdjacent(end2))
                throw new NotSupportedException("no link");

            var edge1 = end1.RemoveEdge(end2);
            if (edge1.forward)
                _forwardEdges.Remove(edge1.linkShapeId);
           
            var edge2 = end2.RemoveEdge(end1);
            if (edge2.forward)
                _forwardEdges.Remove(edge2.linkShapeId);

            if (edge1.linkShapeId != edge2.linkShapeId)
                throw new NotSupportedException("?");

            if (onLinkRemove != null)
                onLinkRemove(end1, end2, edge1.linkShapeId, usrId);            
        }

        public void Unlink(int end1, int end2, int usrId)
        {
            Unlink(GetLinkable(end1), GetLinkable(end2), usrId);
        }

        public void UnlinkFromAll(Linkable unlinked, int usrId)
        {
            foreach (var edge in unlinked.GetEdge().ToArray())
                Unlink(unlinked, edge.next, usrId);
        }

        public void UnlinkFromAll(int unlinkedId, int usrId)
        {
            UnlinkFromAll(GetLinkable(unlinkedId), usrId);         
        }
        #endregion linking

        #region creation/removal

        //clusterId is generated on initiating client 
        public void CreateCluster(int clusterId)
        {
            _clusters.Add(clusterId, new Cluster(clusterId));
        }
        
        public void CreateBadge(int badgeId)
        {
            _badges.Add(badgeId, new Clusterable(badgeId));
        }

        //badge is deleted (not moved out of cluster)  
        public void DeleteBadge(Clusterable deleted, int usrId)
        {
            //1 remove all links 
            UnlinkFromAll(deleted, -1);
            
            //2 remove from cluster, if it's in cluster
            UnclusterBadge(deleted, -1);

            //3 remove badge itself
            _badges.Remove(deleted.GetId()); 
            if (onLinkableDeleted != null)
                onLinkableDeleted(deleted, usrId);
        }
        public void DeleteBadge(int badgeId, int usrId)
        {
            var deleted = _badges[badgeId];
            DeleteBadge(deleted, usrId);
        }
        public void DeleteCluster(Cluster cluster, int usrId)
        {
            //1 unlink from all
            UnlinkFromAll(cluster, -1);

            //2 remove all clusterables from cluster 
            foreach (var badge in cluster.GetClusterables().ToArray())
                UnclusterBadge(badge, -1);

            //3 remove cluster
            _clusters.Remove(cluster.GetId());
            if (onLinkableDeleted != null)
                onLinkableDeleted(cluster, usrId);
        }
        #endregion creation/removal

        #region clustering/unclustering
        public void UnclusterBadge(Clusterable badge, int usrId)
        {
            if (!badge.IsInCluster())
            {
                _log.Warn("badge " + badge.GetId() + " is already unclustered");
                return;
            }
            
            //1 remove from cluster            
            var affectedCluster = badge.GetCluster();
            affectedCluster.Remove(badge);
            badge.Uncluster();
            if (onUnclusterBadge != null)
                onUnclusterBadge(badge, affectedCluster, usrId);

            //2 if cluster is empty, delete cluster
            if (affectedCluster.IsEmpty())
            {
                DeleteCluster(affectedCluster, usrId);
            }            
        }
        public void UnclusterBadge(int badgeId, int usrId)
        {
            UnclusterBadge(_badges[badgeId], usrId);
        }

        public bool ClusterBadge(Clusterable badge, Cluster cluster)
        {
            if (badge.IsInCluster())
            {                       
                //two or more clients sent clustering requests in the same time. first wins
                _log.Warn("cluster failed: badge=" + badge.GetId() + " cluster=" + cluster.GetId() +
                          " current cluster of badge=" + badge.GetCluster().GetId());
                return false;             
            }

            badge.Cluster(cluster);
            cluster.Add(badge);
            return true;
        }
        public bool ClusterBadge(int badgeId, int clusterId)
        {
            return ClusterBadge(_badges[badgeId], _clusters[clusterId]);
        }
        #endregion clustering/unclustering

        #region persistence
        /*read strategy
         *  -Register badges in topology 
         *  -Read list of clusters 
         *  -Cluster badges (list of cluster requests)
         *  -Register list of links
         */
        public void Write(BinaryWriter annotation)
        {
            //list of badges
            var badges = _badges.Values;
            annotation.Write(badges.Count());
            foreach (var b in badges)
                annotation.Write(b.GetId());  
 
            //list of clusters
            var clusters = _clusters.Values;
            annotation.Write(clusters.Count());
            foreach(var cl in clusters)
            {
                annotation.Write(cl.GetId());

                //list of clusterables
                var clusterables = cl.GetClusterables();
                annotation.Write(clusterables.Count());
                foreach (var clusterable in clusterables)
                    annotation.Write(clusterable.GetId()); 
            }

            //list of links
            var edges = _forwardEdges.Values;
            annotation.Write(edges.Count());
            foreach (var ed in edges)
            {
                annotation.Write(ed.linkShapeId);
                annotation.Write(ed.curr.GetId());
                annotation.Write(ed.next.GetId());
            }
        }

        public void Read(BinaryReader annotation)
        {
            if(annotation==null)
                return;

            //read badges
            var badges = _badges.Values;
            var numBadges = annotation.ReadInt32();
            for (int i = 0; i < numBadges; i++)
            {
                var badgeId = annotation.ReadInt32();
                this.CreateBadge(badgeId);                
            }
          
            //read clusters
            var numClusters = annotation.ReadInt32();
            for(var i=0;i<numClusters;i++)
            {
                var clustId = annotation.ReadInt32();
                this.CreateCluster(clustId);

                //list of clusterables
                var numClusterables = annotation.ReadInt32();
                for (int j = 0; j < numClusterables; j++)
                {
                    var badgeeId = annotation.ReadInt32();
                    this.ClusterBadge(badgeeId, clustId);
                }      
            }

            //list of links
            var numEdges = annotation.ReadInt32();
            for (int i = 0; i < numEdges; i++)
            {
                var linkShapeId = annotation.ReadInt32();
                var currId = annotation.ReadInt32();
                var nextId = annotation.ReadInt32();
                this.Link(currId, nextId, linkShapeId);
            }              
        }

        #endregion persistence

        #region reporting
        public DEditorStatsResponse CollectStats(DEditorStatsRequest req)
        {
            var res = default(DEditorStatsResponse);

            res.NumClusteredBadges = 0;
            var clusterIds = new int[_clusters.Values.Count()];
            int i = 0;
            foreach (var cluster in _clusters.Values)
            {
                res.NumClusteredBadges += cluster.GetClusterables().Count();
                clusterIds[i++] = cluster.GetId();
            }

            res.NumClusters = _clusters.Count();

            res.NumLinks    = _forwardEdges.Count();

            res.ListOfClusterIds = clusterIds;

            return res;
        }

        //returns list of badge IDs, not ArgPoint IDs!  Custer text is also undefined   
        //if returns false, cluster was already removed 
        public bool ReportCluster(int clusterId, out ClusterStatsResponse clusterResp)
        {
            clusterResp = default(ClusterStatsResponse);

            if(!_clusters.ContainsKey(clusterId))
               return false;
            
            var cluster = _clusters[clusterId];

            clusterResp.clusterId = clusterId;
            
            var badges = cluster.GetClusterables();
            var badgesArr = new int[badges.Count()];
            int i = 0;
            foreach (var badge in badges)
            {
                badgesArr[i++] = badge.GetId();
            }

            clusterResp.points = badgesArr;
            return true;       
        }
        #endregion reporting
    }
}
