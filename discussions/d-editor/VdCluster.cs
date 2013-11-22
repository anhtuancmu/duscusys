using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Discussions;
using System.Windows.Threading;
using VectorEngine;
using Discussions.RTModel.Model;
using Discussions.d_editor;

namespace DistributedEditor
{
    public class VdCluster : VdBaseShape, IVdShape, LinkableHost, ICaptionHost
    {
        private const int UNDEFINED_CLUSTER = -1;

        //used to show border being drawn
        private Polygon drawingBorder;
        private Polygon convexHull;
        private System.Windows.Shapes.Path bezierBorder;

        private Canvas scene;

        private ClientCluster _endpoint;

        private VdDocument _doc;

        private ShapeCaptionsManager _captions;

        public ShapeCaptionsManager CapMgr()
        {
            return _captions;
        }

        //start/reset drawing
        //finish drawing   
        ///  ClusterButton btnSwitchDrawing;
        //EditorWndCtx _edtCtx;
        //SceneManager _mgr;
        //accumulated offset of cluster since its last rebuild (+/- badge) 
        private Point _offset;

        //offsets from most recent move operation
        private double _dx;
        private double _dy;

        //approx., top left point of cluster, used for user cursor placing
        private Point _topLeft;

        private DispatcherTimer cleanerTimer;

        public delegate void OnClusterUncluster(ClientCluster cluster,
                                                ClientClusterable clusterable,
                                                bool plus,
                                                bool playImmidiately);

        private OnClusterUncluster _onClusterUncluster;


        public delegate void OnClusterCleanup(int id);

        private OnClusterCleanup _OnClusterCleanup;

        //ctor for drawing 
        public VdCluster(int owner, int shapeId,
                         VdDocument doc,
                         OnClusterUncluster onClusterUncluster,
                         OnClusterCleanup OnClusterCleanup) :
                             base(owner, shapeId)
        {
            _doc = doc;

            _endpoint = new ClientCluster(shapeId, boundsProvider);

            _onClusterUncluster = onClusterUncluster;

            _OnClusterCleanup = OnClusterCleanup;

            init(DaoUtils.UserIdToColor(owner));
        }

        public void InitCaptions(ShapeCaptionsManager.CaptionCreationRequested captionCreationRequested)
        {
            _captions = new ShapeCaptionsManager(this, captionCreationRequested);
        }

        public ShapeZ ShapeZLevel()
        {
            return ShapeZ.CLUSTER_Z_LEVEL;
        }

        public void SetZIndex(int z)
        {
            Canvas.SetZIndex(bezierBorder, z);
            if (_captions != null)
            {
                Canvas.SetZIndex(_captions.btnDraw, z + 1);
                Canvas.SetZIndex(_captions.btnType, z + 2);
            }
        }

        //public VdFreeForm GetAnnotaion()
        //{
        //    return freeDrawCaption;
        //}

        private void init(Color c)
        {
            convexHull = new System.Windows.Shapes.Polygon();
            //convexHull.Stroke = new SolidColorBrush(Colors.Red);
            //convexHull.StrokeThickness = ShapeUtils.LINE_WIDTH;
            // hull.Fill = new SolidColorBrush(Color.FromArgb(30, 10, 50, 10));
            //hull.Effect = ShapeUtils.ShadowProvider();
            convexHull.Tag = this;

            drawingBorder = new System.Windows.Shapes.Polygon();
            drawingBorder.Stroke = new SolidColorBrush(c);
            drawingBorder.StrokeThickness = ShapeUtils.LINE_WIDTH;
            drawingBorder.Fill = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0));
            drawingBorder.Tag = this;

            bezierBorder = new System.Windows.Shapes.Path();
            bezierBorder.Stroke = new SolidColorBrush(c);
            bezierBorder.StrokeThickness = ShapeUtils.LINE_WIDTH;
            bezierBorder.Fill = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0));
            bezierBorder.Tag = this;
        }

        public UIElement UnderlyingControl()
        {
            return bezierBorder;
        }

        public void Hide()
        {
            _cursorView.Visibility = Visibility.Hidden;
            bezierBorder.Visibility = Visibility.Hidden;
            _captions.btnDraw.Visibility = Visibility.Hidden;
            _captions.btnType.Visibility = Visibility.Hidden;
        }

        public void Show()
        {
            _cursorView.Visibility = Visibility.Visible;
            bezierBorder.Visibility = Visibility.Visible;
            _captions.btnDraw.Visibility = Visibility.Visible;
            _captions.btnType.Visibility = Visibility.Visible;
        }

        public bool IsVisible()
        {
            return bezierBorder.Visibility == Visibility.Visible;
        }

        //callback from EditorWndCtx
        //public void FreeDrawingFinished(VdFreeForm ff)
        //{
        //    _edtCtx.freeDrawingFinished -= FreeDrawingFinished;
        //    freeDrawCaption = ff;
        //    btnSwitchDrawing.Visibility = Visibility.Visible;
        //    freeDrawCaption.ClusterId = _clusterId;

        //    RepositionCaption();
        //}

        //private void RepositionCaption()
        //{
        //    if (freeDrawCaption != null)
        //    {
        //        const double capWidth = 300;
        //        const double capHeight = 150;
        //        var captionBounds = new Rect(_bezierCurveVisualOrigin.X +
        //                                     bezierBorder.Data.Bounds.X +
        //                                     bezierBorder.Data.Bounds.Width / 2 - capWidth / 2,

        //                                     _bezierCurveVisualOrigin.Y +
        //                                     bezierBorder.Data.Bounds.Y -
        //                                     capHeight,

        //                                     capWidth,

        //                                     capHeight);

        //        freeDrawCaption.SetBounds(captionBounds);
        //    }
        //}

        void IVdShape.AttachToCanvas(Canvas c)
        {
            scene = c;

            if (c.Children.Contains(convexHull))
                return;

            c.Children.Add(drawingBorder);
            c.Children.Add(convexHull);
            c.Children.Add(bezierBorder);

            c.Children.Add(_cursorView);

            c.Children.Add(_captions.btnDraw);
            c.Children.Add(_captions.btnType);
        }

        void IVdShape.DetachFromCanvas(Canvas c)
        {
            scene = null;
            c.Children.Remove(drawingBorder);
            c.Children.Remove(convexHull);
            c.Children.Remove(bezierBorder);

            c.Children.Remove(_cursorView);

            c.Children.Remove(_captions.btnDraw);
            c.Children.Remove(_captions.btnType);
        }

        private bool clusterCreated = false;

        public bool ClusterCreated
        {
            get { return clusterCreated; }
        }

        void IVdShape.StartManip(Point p, object sender)
        {
            if (!clusterCreated)
            {
                if (_topLeft.X == 0.0)
                {
                    _topLeft = new Point(p.X - 50, p.Y - 50);
                    setUsrCursor();
                }

                drawingBorder.Points.Add(p);
            }
            else
            {
                //movement
            }

            CurrentPoint = p;

            cleanerTimer = new DispatcherTimer();
            cleanerTimer.Interval = TimeSpan.FromSeconds(3);
            cleanerTimer.Tick += cleanerTick;
            cleanerTimer.Start();
        }

        private void cleanerTick(object sender, EventArgs e)
        {
            if (!clusterCreated)
                _OnClusterCleanup(Id());

            if (cleanerTimer != null)
            {
                cleanerTimer.Stop();
                cleanerTimer = null;
            }
        }

        public PointApplyResult ApplyCurrentPoint(Point p)
        {
            PointApplyResult res = PointApplyResult.None;

            if (!clusterCreated)
            {
                drawingBorder.Points.Add(p);
            }
            else
            {
                //movement
                _dx = p.X - CurrentPoint.X;
                _dy = p.Y - CurrentPoint.Y;
                MoveBy(_dx, _dy);
                res = PointApplyResult.Move;
            }

            CurrentPoint = p;

            if (cleanerTimer != null)
            {
                cleanerTimer.Stop();
                cleanerTimer.Start();
            }

            return res;
        }

        public override void FinishManip()
        {
            if (!clusterCreated)
            {
                buildInitialCluster(drawingBorder);

                //if (scene != null)
                //    TryAddAnnotateBtn();          
            }

            if (scene != null)
                scene.Children.Remove(drawingBorder);

            base.FinishManip();
        }

        public void ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            e.Handled = true;
        }

        //convHull can be either corrected input set, or existing convex hull  
        private List<ClientClusterable> GetBadgeList(List<Point> convHull)
        {
            var res = new List<ClientClusterable>(_doc.GetShapes().Count()/2);

            var badges = _doc.GetShapes().Where(sh => sh.ShapeCode() == VdShapeType.Badge);
            foreach (IVdShape b in badges)
            {
                var clusterable = ((VdBadge) b).GetClusterable();

                if (clusterable.Busy)
                    continue;

                //if badge is in cluster and it's not in this cluster, ignore it
                if (clusterable.IsInCluster() && clusterable.ClusterId != Id())
                    continue;

                //if (!CoarseHitTest(clusterable))
                //    continue;

                if (ShapeUtils.FuzzyInside(clusterable.GetBounds(), convHull))
                    res.Add(clusterable);
            }

            return res;
        }

        //first takes any incorrect (concave, self-intersecting, unclosed etc) manually entered border of cluster and 
        //trasforms it to intermediate convex hull IH. 
        //then depending on coordinates of badges in IH builds final convex hull FH (containing badges fuzzily inside IH). 
        private void buildInitialCluster(Polygon drawnBorder)
        {
            if (drawnBorder.Points.Count <= 1)
            {
                //cannot build convex hull from 1 point 
                return;
            }

            //create corrected input polygon            
            var convexHull = ConvexHull.FindConvexPolygon(drawnBorder.Points.ToList()).ToList();

            //no badges, cluster is not created 
            var contents = GetBadgeList(convexHull);
            if (contents.Count() == 0)
            {
                return;
            }

            _offset = new Point(0, 0);

            //initial building of cluster always changes contents           
            for (int i = 0; i < contents.Count(); i++)
            {
                contents[i].SetBusy();
                _onClusterUncluster(GetCluster(), contents[i], true, i == contents.Count() - 1);
            }
        }

        //returns true if badge is probably inside cluster. 
        //returns false if badge cannot be inside
        private bool CoarseHitTest(ClientClusterable badge)
        {
            if (bezierBorder.Data == null)
                return true;

            var borders = bezierBorder.Data.Bounds;
            borders.Offset(_offset.X, _offset.Y);

            return borders.IntersectsWith(badge.GetBounds());
        }

        //given list of badges which comprise cluster, we build convex hull and then smooth border
        public void PlayBuildSmoothCurve()
        {
            var BadgesCorners = ShapeUtils.buildBadgeCorners(_endpoint.GetClusterables());

            if (BadgesCorners.Count <= 2)
            {
                //rebuilding cluster after unclustering last badge.
                //the cluster will soon be removed (by server event)
                return;
            }

            convexHull.Points = ConvexHull.FindConvexPolygon(BadgesCorners);
            var convexPoints = convexHull.Points;
            _topLeft = new Point(convexPoints.Min(pt => pt.X), convexPoints.Min(pt => pt.Y));

            //build bezier curve      
            var pathFigure = new PathFigure();
            Point[] firstControlPoints = null;
            Point[] secondControlPoints = null;

            ovp.BezierSpline.GetCurveControlPoints(convexPoints.ToArray(),
                                                   out firstControlPoints,
                                                   out secondControlPoints);
            pathFigure.StartPoint = convexPoints.ElementAt(0);
            for (int i = 0; i < convexPoints.Count - 1; i++)
            {
                pathFigure.Segments.Add(new BezierSegment(firstControlPoints[i],
                                                          secondControlPoints[i],
                                                          convexPoints.ElementAt(i + 1), true));
            }

            //closing segment 
            if (convexPoints.Count == 3)
            {
                Point[] firstLast = new Point[] {convexPoints[2], convexPoints[0], convexPoints[1]};
                ovp.BezierSpline.GetCurveControlPoints(firstLast, out firstControlPoints, out secondControlPoints);
                pathFigure.Segments.Add(new BezierSegment(firstControlPoints.First(),
                                                          secondControlPoints.First(),
                                                          convexPoints.First(), true));
            }
            else
            {
                Point[] firstLast = new Point[]
                    {
                        convexPoints[convexPoints.Count - 2],
                        convexPoints[convexPoints.Count - 1],
                        convexPoints[0], convexPoints[1]
                    };
                ovp.BezierSpline.GetCurveControlPoints(firstLast, out firstControlPoints, out secondControlPoints);
                pathFigure.Segments.Add(new BezierSegment(firstControlPoints[1],
                                                          secondControlPoints[1],
                                                          convexPoints[0], true));
            }

            var pathGeom = new PathGeometry();
            pathGeom.Figures.Add(pathFigure);
            bezierBorder.Data = pathGeom;
            _offset = new Point(0, 0);

            if (_endpoint.GetClusterables().Count() > 0)
            {
                clusterCreated = true;
                if (cleanerTimer != null)
                {
                    cleanerTimer.Stop();
                    cleanerTimer = null;
                }
            }

            SetBounds();
        }

        //PRODUCER
        //if badge is not in any cluster and new bounds are fuzzily inside the cluster, then include 
        //the badge into the cluster and rebuild cluster hull
        public void ClusterableMoved(ClientClusterable badge)
        {
            if (badge.Busy)
                return;

            //hull doesn't exist             
            if (convexHull.Points.Count <= 2)
                return;

            var contents = GetBadgeList(convexHull.Points.ToList());
            var added = contents.Except(_endpoint.GetClusterables());
            var removed = _endpoint.GetClusterables().Except(contents);
            foreach (var a in added)
            {
                if (a.Busy)
                    continue;

                if (a.IsInCluster())
                    continue;

                a.SetBusy();
                _onClusterUncluster(GetCluster(), a, true, /*a == added.Last()*/true);
            }
            foreach (var r in removed)
            {
                if (r.Busy)
                    continue;

                if (!r.IsInCluster())
                    continue;

                r.SetBusy();
                _onClusterUncluster(GetCluster(), r, false, /*r == removed.Last()*/true);
            }

            ////moved shape is in cluster, but not in our one          
            //if (badge.IsInCluster() && badge.ClusterId!= Id())
            //    return;

            ////if (!CoarseHitTest(badge))
            ////    return;

            //var willBeInTheCluster = ShapeUtils.FuzzyInside(badge.GetBounds(), convexHull.Points.ToList());
            //var nowInTheCluster = badge.ClusterId == Id();

            ////send cluster edit operation
            //if (nowInTheCluster && !willBeInTheCluster)
            //{
            //    badge.SetBusy();
            //    _onClusterUncluster(GetCluster(), badge, true, true);
            //}
            //else if (!nowInTheCluster && willBeInTheCluster)
            //{
            //    badge.SetBusy();
            //    _onClusterUncluster(GetCluster(), badge, false, true);
            //}

            //if (nowInTheCluster != willBeInTheCluster)
            //{
            //    badge.SetBusy();
            //    var contents = GetBadgeList(convexHull.Points.ToList()); 

            //    var added = contents.Except(_endpoint.GetClusterables());
            //    var removed = _endpoint.GetClusterables().Except(contents);

            //    foreach (var a in added)
            //    {
            //        _onClusterUncluster(GetCluster(), a, true, true);
            //    }
            //    foreach (var r in removed)                 
            //        _onClusterUncluster( GetCluster(), r, false, true);
            //}       
        }

        public override void RemoveFocus()
        {
            base.RemoveFocus();

            //if (freeDrawCaption != null)
            //    freeDrawCaption.RemoveFocus();
        }

        public override void SetFocus()
        {
            base.SetFocus();

            //if (freeDrawCaption != null)
            //    freeDrawCaption.SetFocus();
        }

        //public override void SetCursor(Cursor c)
        //{
        //    base.SetCursor(c);
        //}

        void IVdShape.ScaleInPlace(bool plus)
        {
            //throw new NotImplementedException();
        }

        private void setUsrCursor()
        {
            Canvas.SetLeft(_cursorView, _offset.X + _topLeft.X - 50);
            Canvas.SetTop(_cursorView, _offset.Y + _topLeft.Y - 50);
        }

        private void SetBounds()
        {
            Canvas.SetLeft(bezierBorder, _offset.X);
            Canvas.SetTop(bezierBorder, _offset.Y);

            setUsrCursor();

            GetLinkable().InvalidateLinks();

            _captions.SetBounds();
        }

        public ShapeState GetState(int topicId)
        {
            var res = new ShapeState(ShapeCode(),
                                     InitialOwner(),
                                     Id(),
                                     null,
                                     new int[] {_captions.GetTextId(), _captions.GetFreeDrawId()},
                                     new double[]
                                         {
                                             _dx, _dy, _captions.textX, _captions.textY,
                                             _captions.freeDrawX, _captions.freeDrawY
                                         },
                                     topicId);
            _dx = 0;
            _dy = 0;
            return res;
        }

        public void ApplyState(ShapeState st)
        {
            //bind caption shapes if not already bound 
            if (st.ints[0] != -1 && _captions.text == null)
                _captions.text = (VdText) _doc.IdToShape(st.ints[0]);

            if (st.ints[1] != -1 && _captions.FreeDraw == null)
                _captions.FreeDraw = (VdFreeForm) _doc.IdToShape(st.ints[1]);

            //update relative caption positions
            _captions.textX = st.doubles[2];
            _captions.textY = st.doubles[3];
            _captions.freeDrawX = st.doubles[4];
            _captions.freeDrawY = st.doubles[5];

            MoveBy(st.doubles[0], st.doubles[1]);
        }

        public void MoveBy(double dx, double dy)
        {
            //update convex hull
            for (int i = 0; i < convexHull.Points.Count; i++)
            {
                var p = convexHull.Points[i];
                p.Offset(dx, dy);
                convexHull.Points[i] = p;
            }

            //update badges 
            foreach (var badgeIntf in GetCluster().GetClusterables())
            {
                var badge = (VdBadge) _doc.IdToShape(badgeIntf.GetId());

                //badge was recently removed
                if (badge == null)
                    continue;

                var pos = badge.getBadgePos();
                pos.X += dx;
                pos.Y += dy;
                badge.setBadgePos(pos.X, pos.Y);
            }

            //update origin
            _offset.X += dx;
            _offset.Y += dy;

            SetBounds(); //set bezier border to org      
        }

        double IVdShape.distToFigure(Point from)
        {
            if (bezierBorder == null)
                return double.MaxValue;

            if (bezierBorder.Data == null)
                return double.MaxValue;

            var center = new Point(bezierBorder.Data.Bounds.X + bezierBorder.Data.Bounds.Width/2,
                                   bezierBorder.Data.Bounds.Y + bezierBorder.Data.Bounds.Height/2);

            return ShapeUtils.Dist(from, center);
        }

        public Point GetOrigin()
        {
            if (bezierBorder.Data == null)
                return new Point(0, 0);

            return bezierBorder.Data.Bounds.TopLeft;
        }

        public VdShapeType ShapeCode()
        {
            return VdShapeType.Cluster;
        }

        public ClientLinkable GetLinkable()
        {
            return _endpoint;
        }

        public ClientCluster GetCluster()
        {
            return _endpoint;
        }

        public Rect boundsProvider()
        {
            if (bezierBorder.Data == null)
                return new Rect(0, 0, 100, 100);
            var res = bezierBorder.Data.Bounds;
            res.Offset(_offset.X, _offset.Y);

            return res;
        }

        public Point capOrgProvider()
        {
            var bounds = boundsProvider();
            var res = bounds.Location;
            res.Offset(0, -70);
            return res;
        }

        public Point btnOrgProvider()
        {
            var bounds = boundsProvider();
            var res = bounds.Location;
            res.Offset(20, -45);
            return res;
        }

        public override Rect ReportingBoundsProvider()
        {
            var bounds = boundsProvider();
            if (_captions.text != null)
            {
                var captionBounds = _captions.text.ReportingBoundsProvider();
                bounds.Union(captionBounds);
            }
            else if (_captions.FreeDraw != null)
            {
                var captionBounds = _captions.FreeDraw.ReportingBoundsProvider();
                bounds.Union(captionBounds);
            }

            bounds.Inflate(20,20); //include border in screenshot

            return bounds;
        }
    }
}