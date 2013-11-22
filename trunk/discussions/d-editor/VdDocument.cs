using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Discussions.DbModel.model;
using Discussions.d_editor;
using Discussions.model;
using Discussions.rt;
using Discussions.RTModel.Model;
using Discussions;

namespace DistributedEditor
{
    //provides access to shape collection
    public class VdDocument : IDisposable
    {
        private readonly UISharedRTClient _rt = UISharedRTClient.Instance;

        //shape id to shape
        private readonly Dictionary<int, IVdShape> shapes = new Dictionary<int, IVdShape>();

        //selection, copy clone
        private CursorMgr _volatileCtx = null;

        public CursorMgr VolatileCtx
        {
            get
            {
                if (_volatileCtx == null)
                    _volatileCtx = new CursorMgr(this, _palette);
                return _volatileCtx;
            }
        }

        private readonly Canvas _scene;

        public Canvas Scene
        {
            get { return _scene; }
        }

        private readonly IPaletteOwner _palette;

        public int OwnerId
        {
            get { return _palette.GetOwnerId(); }
        }

        private bool _isLoaded = false;

        public bool IsLoaded
        {
            get { return _isLoaded; }
        }

        private readonly int _topicId;

        public int TopicId
        {
            get { return _topicId; }
        }

        private readonly int _discId;

        public int DiscussionId
        {
            get { return _discId; }
        }

        public delegate void ShapePostCtor(IVdShape sh, VdShapeType shapeType);

        private ShapePostCtor _shapePostHandler = null;

        //expecting cluster rebuild from server 
        public bool clusterRebuildPending = false;

        private bool _shapeVisibility;

        public bool ShapeVisibility
        {
            get { return _shapeVisibility; }
        }

        public VdDocument(IPaletteOwner owner, Canvas scene, ShapePostCtor shapePostHandler,
                          int topicId, int discussionId, bool shapeVisibility)
        {
            _palette = owner;
            _scene = scene;
            _shapePostHandler = shapePostHandler;
            _topicId = topicId;
            _shapeVisibility = shapeVisibility;
            _discId = discussionId;

            setListeners(true);
        }

        public void Dispose()
        {
            setListeners(false);
        }

        private void shapeLoadingDone()
        {
            _isLoaded = true;

            if (!_shapeVisibility)
                HideShapes();
        }

        public void setListeners(bool doSet)
        {
            if (doSet)
                _rt.clienRt.loadingDoneEvent += shapeLoadingDone;
            else
                _rt.clienRt.loadingDoneEvent -= shapeLoadingDone;

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

        private int recentLocallyRemovedShapeId = -1;

        public void BeginRemoveSingleShape(int shapeId)
        {
            recentLocallyRemovedShapeId = shapeId;
            _rt.clienRt.SendDeleteSingleShapeRequest(OwnerId, shapeId, TopicId);
        }

        private void _removeSingleShape(DeleteSingleShapeEvent ev)
        {
            if (ev.topicId != TopicId)
                return;

            PlayRemoveSingleShape(ev.shapeId, ev.indirectOwner);
        }

        //player for single shape deletion
        private void PlayRemoveSingleShape(int shapeId, int indirectOwner)
        {
            if (!shapes.ContainsKey(shapeId))
                return;
            var sh = shapes[shapeId];
            shapes.Remove(sh.Id());
            sh.DetachFromCanvas(_scene);

            switch (sh.ShapeCode())
            {
                case VdShapeType.ClusterLink:
                    var link = (VdClusterLink) sh;
                    CleanupClusterCaptions(sh, indirectOwner);
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
        private void CleanupClusterCaptions(IVdShape shape, int indirectOwner)
        {
            ICaptionHost capHost = null;
            switch (shape.ShapeCode())
            {
                case VdShapeType.FreeForm:
                    capHost = DocTools.GetCaptionHost(GetShapes(), shape);
                    if (capHost != null)
                        capHost.CapMgr().InvalidateCaption(shape);

                    //caption removed locally, update cluster 
                    if (capHost != null && shape.Id() == recentLocallyRemovedShapeId)
                    {
                        _rt.clienRt.SendSyncState(capHost.Id(),
                                                  capHost.GetState(TopicId));
                    }
                    break;
                case VdShapeType.Text:
                    capHost = DocTools.GetCaptionHost(GetShapes(), shape);
                    if (capHost != null)
                        capHost.CapMgr().InvalidateCaption(shape);

                    //caption removed locally, update cluster 
                    if (capHost != null && shape.Id() == recentLocallyRemovedShapeId)
                    {
                        _rt.clienRt.SendSyncState(capHost.Id(),
                                                  capHost.GetState(TopicId));
                    }
                    break;
                case VdShapeType.Cluster:
                case VdShapeType.ClusterLink:
                    //cluster removed locally, remove captions
                    capHost = (ICaptionHost) shape;
                    if (indirectOwner == _palette.GetOwnerId())
                    {
                        if (capHost.CapMgr().text != null)
                        {
                            BeginRemoveSingleShape(capHost.CapMgr().text.Id());
                        }

                        if (capHost.CapMgr().FreeDraw != null)
                        {
                            BeginRemoveSingleShape(capHost.CapMgr().FreeDraw.Id());
                        }
                    }
                    break;
            }
        }

        private void Add(IVdShape shape)
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

        private void createShapeEvent(CreateShape ev)
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
                                        bool takeCursor,
                                        // for badge creation events, it's false, as badges are created in private board 
                                        int tag)
        {
            if (!shapes.ContainsKey(shapeId))
            {
                //update id generator
                if (shapeType != VdShapeType.Badge)
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

                if (!_shapeVisibility)
                    res.Hide();

                this.Add(res);
                DocTools.SortScene(_scene);
                if (takeCursor)
                    VolatileCtx.PlayTakeCursor(owner, shapeId);
                return res;
            }
            else
            {
                return shapes[shapeId];
            }
        }

        public int BeginCreateLink(int end1Id,
                                   int end2Id,
                                   LinkHeadType linkHead)
        {
            var end1 = ((LinkableHost) shapes[end1Id]).GetLinkable();
            var end2 = ((LinkableHost) shapes[end2Id]).GetLinkable();

            if (end1.HasAdjacent(end2))
            {
                MessageDlg.Show("Already linked",
                                "Info");
                return -1;
            }

            var shapeId = ShapeIdGenerator.Instance.NextId(_palette.GetOwnerId());
            _rt.clienRt.SendLinkCreateRequest(end1Id, end2Id,
                                              _palette.GetOwnerId(), shapeId,
                                              TopicId,
                                              true,
                                              linkHead);
            return shapeId;
        }

        private VdClusterLink PlayLinkCreate(ClientLinkable end1, ClientLinkable end2,
                                             int shapeId, int initOwnerId, bool takeCursor,
                                             LinkHeadType linkHead)
        {
            ShapeIdGenerator.Instance.CorrectLowBound(initOwnerId, shapeId);
            var res = new VdClusterLink(end1, end2, shapeId, initOwnerId, this, linkHead);
            _shapePostHandler(res, VdShapeType.ClusterLink);
            end1.AddEdge(res);
            end2.AddEdge(res);

            if (!_shapeVisibility)
                res.Hide();

            //no post handler for cluster link

            this.Add(res);

            DocTools.SortScene(_scene);
            //no initial lock, as link is created in free state (no pressed buttons)           
            return res;
        }

        private void onLinkCreateEvent(LinkCreateMessage ev)
        {
            if (ev.topicId != TopicId)
                return;

            PlayLinkCreate(((LinkableHost) shapes[ev.end1Id]).GetLinkable(),
                           ((LinkableHost) shapes[ev.end2Id]).GetLinkable(),
                           ev.shapeId,
                           ev.ownerId,
                           ev.takeCursor,
                           ev.HeadType);
        }

        //called only locally
        private void onClusterUncluster(ClientCluster cluster,
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

        private void OnTextCleanup(int id)
        {
            BeginRemoveSingleShape(id);
        }

        private void OnClusterCleanup(int id)
        {
            //remove cluster itself
            BeginRemoveSingleShape(id);
        }

        #endregion adding/removing shapes

        #region clustering 

        private void __clusterBadge(ClusterBadgeMessage ev)
        {
            if (ev.topicId != TopicId)
                return;

            clusterRebuildPending = false;
            PlayClusterBadge(ev.clusterId, ev.badgeId, ev.playImmidiately, ev.callToken);
        }

        private void PlayClusterBadge(int clusterId, int clusterableId, bool playImmidiately, int callToken)
        {
            var cluster = (VdCluster) shapes[clusterId];
            var badge = ((VdBadge) shapes[clusterableId]).GetClusterable();
            cluster.GetCluster().Add(badge);

            cluster.PlayBuildSmoothCurve();
        }

        private void __unclusterBadge(UnclusterBadgeMessage ev)
        {
            if (ev.topicId != TopicId)
                return;

            clusterRebuildPending = false;
            PlayUnclusterBadge(ev.clusterId, ev.badgeId, ev.playImmidiately, ev.callToken);
        }

        private void PlayUnclusterBadge(int clusterId, int clusterableId, bool playImmidiately, int callToken)
        {
            var cluster = (VdCluster) shapes[clusterId];
            var badge = ((VdBadge) shapes[clusterableId]).GetClusterable();
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

        public void ShowShapes()
        {
            foreach (var sh in GetShapes())
            {
                sh.Show();
            }
            DocTools.UnfocusAll(GetShapes());
            _shapeVisibility = true;
        }

        public void HideShapes()
        {
            foreach (var sh in GetShapes())
            {
                sh.Hide();
            }
            _shapeVisibility = false;
        }
    }
}