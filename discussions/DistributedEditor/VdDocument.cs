using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Forms;
using Discussions.rt;
using Discussions.RTModel;
using Discussions.RTModel.Model;

namespace DistributedEditor
{
    //provides access to shape collection
    public class VdDocument : IDisposable
    {
        UISharedRTClient _rt = UISharedRTClient.Instance;
       
        //shape id to shape
        Dictionary<int,IVdShape> shapes = new Dictionary<int,IVdShape>();

        //selection, copy clone
        CursorMgr _volatileCtx = null;
        public CursorMgr VolatileCtx
        {
            get
            {
                if(_volatileCtx==null)
                    _volatileCtx = new CursorMgr(this, _palette);
                return _volatileCtx;
            }
        }
        
        Canvas _scene;
        public Canvas Scene
        {
            get { return _scene; }
        }
        
        IPaletteOwner _palette;
        public int OwnerId
        {
            get
            {
                return _palette.GetOwnerId();
            }
        }

        int _topicId;
        public int TopicId
        {
            get { return _topicId; }
        }

        int _discId;
        public int DiscussionId
        {
            get { return _discId; }
        }

        public delegate void ShapePostCtor(IVdShape sh, VdShapeType shapeType);
        ShapePostCtor _shapePostHandler = null;
     
        //expecting cluster rebuild from server 
        public bool clusterRebuildPending = false;

        public VdDocument(IPaletteOwner owner, Canvas scene, ShapePostCtor shapePostHandler, 
                         int topicId, int discussionId)
        {
            _palette = owner;
            _scene = scene;
            _shapePostHandler = shapePostHandler;
            _topicId = topicId;
            _discId = discussionId; 

            setListeners(true);
        }

        public void Dispose()
        {
            setListeners(false);
        }

        public void setListeners(bool doSet)
        {
            if (doSet)
                _rt.clienRt.createShapeEvent += createShapeEvent;
            else
                _rt.clienRt.createShapeEvent -= createShapeEvent;

            if (doSet)
                _rt.clienRt.deleteSingleShape += _removeSingleShape;
            else
                _rt.clienRt.deleteSingleShape -= _removeSingleShape;

            if (doSet)
                _rt.clienRt.onLinkCreateEvent += onLinkCreateEvent;
            else
                _rt.clienRt.onLinkCreateEvent -= onLinkCreateEvent;

            if (doSet)
                _rt.clienRt.onUnclusterBadgeEvent += __unclusterBadge; 
            else
                _rt.clienRt.onUnclusterBadgeEvent -= __unclusterBadge;

            if (doSet)
                _rt.clienRt.onClusterBadgeEvent += __clusterBadge; 
            else
                _rt.clienRt.onClusterBadgeEvent -= __clusterBadge;
        }

        public IVdShape IdToShape(int id)
        {
            if (shapes.ContainsKey(id))
                return shapes[id];
            else
                return null;
        }

        public IEnumerable<IVdShape> GetShapes()
        {
            return shapes.Values;
        }

        public bool Contains(IVdShape sh)
        {
            return shapes.ContainsKey(sh.Id());
        }

        #region adding/removing shapes 

        //returns array of removed shapes
        public void BeginClearShapesOfOwner()
        {
            _rt.clienRt.SendDeleteShapesRequest(OwnerId, OwnerId, TopicId);                 
        }

        int recentLocallyRemovedShapeId = -1;
        public void BeginRemoveSingleShape(int shapeId)
        {
            recentLocallyRemovedShapeId = shapeId;
            _rt.clienRt.SendDeleteSingleShapeRequest(OwnerId, shapeId, TopicId);                       
        }

        void _removeSingleShape(DeleteSingleShapeEvent ev)
        {
            if (ev.topicId != TopicId)
                return;
            
            PlayRemoveSingleShape(ev.shapeId, ev.indirectOwner);
        }

        //player for single shape deletion
        void PlayRemoveSingleShape(int shapeId, int indirectOwner)
        {
            if (!shapes.ContainsKey(shapeId))
                return;
            var sh = shapes[shapeId];
            shapes.Remove(sh.Id());
            sh.DetachFromCanvas(_scene);

            switch (sh.ShapeCode())
            {
                case VdShapeType.ClusterLink:
                    var link = (VdClusterLink)sh;
                    link.GetLinkable1().RemoveEdge(link);
                    link.GetLinkable2().RemoveEdge(link); 
                    break;
                case VdShapeType.FreeForm:
                    CleanupClusterCaptions(sh, indirectOwner);                                     
                    break;
                case VdShapeType.Text:
                    CleanupClusterCaptions(sh, indirectOwner);                                         
                    break;
                case VdShapeType.Cluster:                    
                    CleanupClusterCaptions(sh, indirectOwner);   
                    break;
            }                 
        }

        //shape can be either cluster, text, free draw
        void CleanupClusterCaptions(IVdShape shape, int indirectOwner)
        {
            VdCluster changedCluster = null;
            switch (shape.ShapeCode())
            {               
                case VdShapeType.FreeForm:
                    changedCluster = DocTools.GetCaptionHost(GetShapes(), shape);
                    if (changedCluster != null)
                        changedCluster.Captions.InvalidateCaption(shape);   
                   
                    //caption removed locally, update cluster 
                    if (changedCluster != null && shape.Id() == recentLocallyRemovedShapeId)
                    {                                
                        _rt.clienRt.SendSyncState(changedCluster.Id(),
                                                  changedCluster.GetState(TopicId));
                    }                     
                    break;
                case VdShapeType.Text:
                    changedCluster = DocTools.GetCaptionHost(GetShapes(), shape);
                    if (changedCluster != null)
                        changedCluster.Captions.InvalidateCaption(shape);    
                    
                    //caption removed locally, update cluster 
                    if (changedCluster != null && shape.Id() == recentLocallyRemovedShapeId)
                    {                                
                        _rt.clienRt.SendSyncState(changedCluster.Id(),
                                                  changedCluster.GetState(TopicId));
                    }  
                    break;
                case VdShapeType.Cluster:
                    //cluster removed locally, remove captions
                    changedCluster = (VdCluster)shape;
                    if (indirectOwner == _palette.GetOwnerId())                   
                    {
                        if (changedCluster.Captions.text!=null)
                        {
                            BeginRemoveSingleShape(changedCluster.Captions.text.Id());                       
                        }

                        if (changedCluster.Captions.FreeDraw != null)
                        {
                            BeginRemoveSingleShape(changedCluster.Captions.FreeDraw.Id());                      
                        }    
                    }
                    break;
            }   
        }

        void Add(IVdShape shape)
        {
            shape.AttachToCanvas(_scene);
            shapes.Add(shape.Id(), shape);
        }

        public IVdShape BeginCreateShape(VdShapeType shapeType,
                                         double startX, 
                                         double startY,
                                         bool takeCursor,
                                         int tag)
        {
            var shapeId = ShapeIdGenerator.Instance.NextId(_palette.GetOwnerId());
            _rt.clienRt.SendCreateShapeRequest(_palette.GetOwnerId(), shapeId, shapeType, takeCursor, 
                                               startX, startY, tag, TopicId); //always success 
            var sh = PlayCreateShape(shapeType,
                                     shapeId, 
                                     _palette.GetOwnerId(),
                                     startX, startY, takeCursor, tag);   
            return sh;
        }

        void createShapeEvent(CreateShape ev)
        {
            if (ev.topicId != TopicId)
                return;
            
            PlayCreateShape(ev.shapeType, ev.shapeId, ev.ownerId, ev.startX, ev.startY, ev.takeCursor, ev.tag);
        }

        //if it's real-time creation, shape is locked by its owner. 
        //if it's initial loading, we don't lock the shape,
        //lock requests will follow in this case 
        public IVdShape PlayCreateShape(VdShapeType shapeType,
                                        int shapeId, 
                                        int owner,
                                        double startX,
                                        double startY,
                                        bool takeCursor, // for badge creation events, it's false, as badges are created in private board 
                                        int tag)
        {
            if (!shapes.ContainsKey(shapeId))
            {
                //update id generator
                if (shapeType!=VdShapeType.Badge)
                    ShapeIdGenerator.Instance.CorrectLowBound(owner, shapeId);

                IVdShape res = null;
                switch (shapeType)
                {
                    case VdShapeType.Cluster:
                        res = new VdCluster(owner, shapeId, this, onClusterUncluster, OnClusterCleanup);
                        break;
                    case VdShapeType.Text:
                        res = new VdText(startX, startY, owner, shapeId, OnTextCleanup);
                        break;
                    default:
                        res = DocTools.MakeShape(shapeType, owner, shapeId, startX, startY, tag);
                        break;
                }                

                _shapePostHandler(res, shapeType);
                this.Add(res);
                DocTools.SortScene(_scene);
                if(takeCursor)
                    VolatileCtx.PlayTakeCursor(owner, shapeId);
                return res;
            }
            else
            {
                return shapes[shapeId];
            }
        }

        public void BeginCreateLink(int end1Id,
                                    int end2Id)
        {
            var end1 = ((LinkableHost)shapes[end1Id]).GetLinkable();
            var end2 = ((LinkableHost)shapes[end2Id]).GetLinkable();

            if (end1.HasAdjacent(end2))
            {
                MessageBox.Show("Already linked",
                                "Info",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                return;
            }
            
            var shapeId = ShapeIdGenerator.Instance.NextId(_palette.GetOwnerId());
            _rt.clienRt.SendLinkCreateRequest(end1Id, end2Id,
                                              _palette.GetOwnerId(), shapeId,
                                              TopicId,
                                              true);                     
        }

        VdClusterLink PlayLinkCreate(ClientLinkable end1, ClientLinkable end2, 
                                     int shapeId, int initOwnerId, bool takeCursor)
        {
            ShapeIdGenerator.Instance.CorrectLowBound(initOwnerId, shapeId);
            var res = new VdClusterLink(end1, end2, shapeId, initOwnerId);
            end1.AddEdge(res);
            end2.AddEdge(res);
            //no post handler for cluster link
            this.Add(res);
            DocTools.SortScene(_scene);
            //no initial lock, as link is created in free state (no pressed buttons)           
            return res;
        }
        
        void onLinkCreateEvent(LinkCreateMessage ev)
        {
            if (ev.topicId != TopicId)
                return;

            PlayLinkCreate(((LinkableHost)shapes[ev.end1Id]).GetLinkable(),
                           ((LinkableHost)shapes[ev.end2Id]).GetLinkable(),
                           ev.shapeId,
                           ev.ownerId,
                           ev.takeCursor);           
        }

        //called only locally
        void onClusterUncluster(ClientCluster cluster,
                                ClientClusterable badge,
                                bool plus, bool playImmidiately)
        {
            if (plus)
            {
                //cluster must exist      
                clusterRebuildPending = true;
                _rt.clienRt.SendClusterBadgeRequest(badge.GetId(), 
                                                    cluster.GetId(),
                                                    _palette.GetOwnerId(),
                                                    TopicId,
                                                    playImmidiately,
                                                    -1);
            }
            else
            {
                //server checks if affected clsuter is empty     
                clusterRebuildPending = true;
                _rt.clienRt.SendUnclusterBadgeRequest(badge.GetId(), 
                                                     cluster.GetId(),                                                      
                                                     TopicId, 
                                                     _palette.GetOwnerId(),
                                                     playImmidiately,
                                                     -1);
            }             
        }

        void OnTextCleanup(int id)
        {
            BeginRemoveSingleShape(id);     
        }
        void OnClusterCleanup(int id)
        {
            //remove cluster itself
            BeginRemoveSingleShape(id);
        }
        #endregion adding/removing shapes

        #region clustering 
        void __clusterBadge(ClusterBadgeMessage ev)
        {
            if (ev.topicId != TopicId)
                return;

            clusterRebuildPending = false;
            PlayClusterBadge(ev.clusterId, ev.badgeId, ev.playImmidiately, ev.callToken);
        }

        void PlayClusterBadge(int clusterId, int clusterableId, bool playImmidiately, int callToken)
        {
            var cluster = (VdCluster)shapes[clusterId];
            var badge = ((VdBadge)shapes[clusterableId]).GetClusterable();
            cluster.GetCluster().Add(badge);

            //we ignore playing own rebuild, as we already rebuilt cluster and by now local positions can be different
           // if (playImmidiately)
                cluster.PlayBuildSmoothCurve();
            //else
            //{
            //    Console.WriteLine("badges:");
            //    var badges = shapes.Values.Where(sh0 => sh0.ShapeCode() == VdShapeType.Badge);
            //    foreach(var b in badges)                
            //        Console.WriteLine("badge {0}, cluster {1}",b.Id(), ((VdBadge)b).GetClusterable().ClusterId);

            //    Console.WriteLine("");
            //    Console.WriteLine("clusters");
            //    var cl = shapes.Values.Where(sh0 => sh0.ShapeCode() == VdShapeType.Cluster);
            //    foreach (var c in cl)
            //        Console.WriteLine("cluster {0}", c.Id());
            //}
        }

        void __unclusterBadge(UnclusterBadgeMessage ev)
        {
            if (ev.topicId != TopicId)
                return;

            clusterRebuildPending = false;
            PlayUnclusterBadge(ev.clusterId, ev.badgeId, ev.playImmidiately, ev.callToken);
        }

        void PlayUnclusterBadge(int clusterId, int clusterableId, bool playImmidiately, int callToken)
        {
            var cluster = (VdCluster)shapes[clusterId];
            var badge = ((VdBadge)shapes[clusterableId]).GetClusterable();
            cluster.GetCluster().Remove(badge);

            if (playImmidiately)
                cluster.PlayBuildSmoothCurve();
        }
        #endregion clustering

        /// <summary>
        /// Detaches shapes of current document from canvas, but preserves all data. 
        /// Later document can be reattached again        
        /// </summary>
        public void DetachFromCanvas()
        {
            if (Scene == null)
                return;
           
            foreach (IVdShape sh in shapes.Values)                
                sh.DetachFromCanvas(Scene);
        }
    }
}
