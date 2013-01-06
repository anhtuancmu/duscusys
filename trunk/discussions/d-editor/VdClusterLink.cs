using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using Discussions;
using Petzold.Media2D;
using Discussions.RTModel.Model;
using Discussions.d_editor;

namespace DistributedEditor
{
    public class VdClusterLink : VdBaseShape, IVdShape, ICaptionHost
    {
        private ArrowLine line;
        private System.Windows.Shapes.Ellipse selMarker1;
        private System.Windows.Shapes.Ellipse selMarker2;
        private Canvas scene;

        private ClientLinkable _end1 = null;
        private ClientLinkable _end2 = null;

        //defined only if linkables != null
        public AnchorPoint anchor1;
        public AnchorPoint anchor2;

        private LinkHeadType _headType;

        private VdDocument _doc;
        private ShapeCaptionsManager _captions;

        public ShapeCaptionsManager CapMgr()
        {
            return _captions;
        }

        public VdClusterLink(ClientLinkable end1, ClientLinkable end2,
                             int shapeId, int owner,
                             VdDocument doc,
                             LinkHeadType headType) :
                                 base(owner, shapeId)
        {
            _doc = doc;

            _end1 = end1;
            _end2 = end2;

            _headType = headType;

            initLine(DaoUtils.UserIdToColor(owner), headType);

            //wait until actual size is set 
            line.Dispatcher.BeginInvoke(new Action(() => { NotifyLinkableMoved(); }),
                                        System.Windows.Threading.DispatcherPriority.Background);

            RemoveFocus();
            updateUserCursor();
        }

        private void initLine(Color c, LinkHeadType headType)
        {
            line = new ArrowLine();
            line.ArrowAngle = 40;
            line.Stroke = new SolidColorBrush(c);
            switch (headType)
            {
                case LinkHeadType.DoubleHeaded:
                    line.ArrowEnds = ArrowEnds.Both;
                    break;
                case LinkHeadType.SingleHeaded:
                    line.ArrowEnds = ArrowEnds.End;
                    break;
                default:
                    throw new NotSupportedException();
            }
            line.StrokeThickness = ShapeUtils.LINE_WIDTH;
            line.Effect = ShapeUtils.ShadowProvider();
            line.Tag = this;

            selMarker1 = ShapeUtils.MakeLinkEnd();
            selMarker1.Tag = this;

            selMarker2 = ShapeUtils.MakeLinkEnd();
            selMarker2.Tag = this;

            line.MouseWheel += MouseWheel;
        }

        public void InitCaptions(ShapeCaptionsManager.CaptionCreationRequested captionCreationRequested)
        {
            _captions = new ShapeCaptionsManager(this, captionCreationRequested);
        }

        public void Hide()
        {
            _cursorView.Visibility = Visibility.Hidden;
            line.Visibility = Visibility.Hidden;
            VdSegmentUtil.HideMarkers(selMarker1, selMarker2);
            if (_captions != null)
            {
                _captions.btnDraw.Visibility = Visibility.Hidden;
                _captions.btnType.Visibility = Visibility.Hidden;
            }
        }

        public void Show()
        {
            _cursorView.Visibility = Visibility.Visible;
            line.Visibility = Visibility.Visible;
            VdSegmentUtil.ShowMarkers(selMarker1, selMarker2);
            if (_captions != null)
            {
                _captions.btnDraw.Visibility = Visibility.Visible;
                _captions.btnType.Visibility = Visibility.Visible;
            }
        }

        public bool IsVisible()
        {
            return line.Visibility == Visibility.Visible;
        }

        public UIElement UnderlyingControl()
        {
            return line;
        }

        public override void ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            base.ManipulationStarting(sender, e);
            TouchManip = true;
        }

        public void ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
        }

        private void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScaleInPlace(e.Delta > 0);
        }

        private void Manipulation(ManipulationDeltaEventArgs e)
        {
        }

        private void SetMarkers()
        {
            ShapeUtils.SetLinkEnd(selMarker1, line.X1, line.Y1);
            ShapeUtils.SetLinkEnd(selMarker2, line.X2, line.Y2);
        }

        public override void RemoveFocus()
        {
            base.RemoveFocus();

            VdSegmentUtil.HideMarkers(selMarker1, selMarker2);
        }

        public override void SetFocus()
        {
            base.SetFocus();

            VdSegmentUtil.ShowMarkers(selMarker1, selMarker2);
        }

        public override void SetCursor(Cursor c)
        {
            base.SetCursor(c);
        }

        public ShapeZ ShapeZLevel()
        {
            return ShapeZ.SIMPLE_SHAPE_Z_LEVEL;
        }

        public void SetZIndex(int z)
        {
            Canvas.SetZIndex(line, z);
            Canvas.SetZIndex(selMarker1, z + 1);
            Canvas.SetZIndex(selMarker2, z + 2);
            Canvas.SetZIndex(_cursorView, z + 3);
            if (_captions != null)
            {
                Canvas.SetZIndex(_captions.btnDraw, z + 4);
                Canvas.SetZIndex(_captions.btnType, z + 5);
            }
        }

        public void AttachToCanvas(Canvas c)
        {
            scene = c;

            if (!c.Children.Contains(line))
                c.Children.Add(line);

            c.Children.Add(selMarker1);
            c.Children.Add(selMarker2);
            c.Children.Add(_cursorView);

            c.Children.Add(_captions.btnDraw);
            c.Children.Add(_captions.btnType);
        }

        public void DetachFromCanvas(Canvas c)
        {
            scene = null;

            c.Children.Remove(line);

            c.Children.Remove(selMarker1);
            c.Children.Remove(selMarker2);
            c.Children.Remove(_cursorView);

            c.Children.Remove(_captions.btnDraw);
            c.Children.Remove(_captions.btnType);
        }

        public VdShapeType ShapeCode()
        {
            return VdShapeType.ClusterLink;
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
                                             _captions.textX, _captions.textY,
                                             _captions.freeDrawX, _captions.freeDrawY
                                         },
                                     topicId);
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
            _captions.textX = st.doubles[0];
            _captions.textY = st.doubles[1];
            _captions.freeDrawX = st.doubles[2];
            _captions.freeDrawY = st.doubles[3];
        }

        public void StartManip(Point p, object sender)
        {
            CurrentPoint = p;
        }

        public PointApplyResult ApplyCurrentPoint(Point p)
        {
            CurrentPoint = p;
            return PointApplyResult.None;
        }

        private void HandleMove(double deltaX, double deltaY)
        {
            if (_end1 == null)
            {
                line.X1 += deltaX;
                line.Y1 += deltaY;
            }

            if (_end2 == null)
            {
                line.X2 += deltaX;
                line.Y2 += deltaY;
            }
        }

        private void HandleResize(double deltaX, double deltaY, VdSegmentUtil.SegmentMarker side)
        {
            //switch (side)
            //{
            //    case VdSegmentUtil.SegmentMarker.Side1:
            //        line.X1 += deltaX;
            //        line.Y1 += deltaY;                    
            //        RefreshContactSide(VdSegmentUtil.SegmentMarker.Side1);
            //        break;
            //    case VdSegmentUtil.SegmentMarker.Side2:
            //        line.X2 += deltaX;
            //        line.Y2 += deltaY;                    
            //        RefreshContactSide(VdSegmentUtil.SegmentMarker.Side2);
            //        break;
            //}
        }

        private void HandleScale(double scale)
        {
        }

        public void ScaleInPlace(bool plus)
        {
        }

        public double distToFigure(Point from)
        {
            var d1 = ShapeUtils.Dist(new Point(line.X1, line.Y1), from);
            var d2 = ShapeUtils.Dist(new Point(line.X2, line.Y2), from);
            var d3 = ShapeUtils.Dist(new Point((line.X1 + line.X2)/2, (line.Y1 + line.Y2)/2), from);

            return ShapeUtils.Min(d1, d2, d3);
        }

        public void MoveBy(double dx, double dy)
        {
            //if (end1 != null)
            {
                line.X1 += dx;
                line.Y1 += dy;
            }

            //if (end2 != null)
            {
                line.X2 += dx;
                line.Y2 += dy;
            }
        }

        //void RefreshContactSide(VdSegmentUtil.SegmentMarker side)
        //{
        //    if (_end1 == null || _end2 == null)
        //        return;

        //    switch (side)
        //    {
        //        case VdSegmentUtil.SegmentMarker.Side1:
        //            double minDist;
        //            Point minAnchorPt;
        //            ShapeUtils.NearestAnchor(new Point(line.X2, line.Y2),
        //                                    _end1,
        //                                    out anchor1,
        //                                    out minAnchorPt,
        //                                    out minDist);
        //            line.X1 = minAnchorPt.X;
        //            line.Y1 = minAnchorPt.Y;
        //            break;
        //        case VdSegmentUtil.SegmentMarker.Side2:
        //            ShapeUtils.NearestAnchor(new Point(line.X1, line.Y1), 
        //                                     _end2,
        //                                     out anchor2,
        //                                     out minAnchorPt,
        //                                     out minDist);
        //            line.X2 = minAnchorPt.X;
        //            line.Y2 = minAnchorPt.Y;
        //            break;
        //    }
        //}

        private void RefreshLinkLayout()
        {
            if (_end1 == null || _end2 == null)
                return;

            double x1, y1, x2, y2;
            ShapeUtils.GetLinkPoints(_end1, _end2, out x1, out y1, out x2, out y2);
            line.X1 = x1;
            line.Y1 = y1;

            line.X2 = x2;
            line.Y2 = y2;
        }

        private void updateUserCursor()
        {
            Canvas.SetLeft(_cursorView, (line.X1 + line.X2)*0.5);
            Canvas.SetTop(_cursorView, (line.Y1 + line.Y2)*0.5);
        }

        public void NotifyLinkableMoved()
        {
            if (_end1 == null || _end2 == null)
                return;


            //for link with cluster at least on one side, use rect. anchors
            //if (_end1 is ClientCluster || _end2 is ClientCluster)
            //{
            //    RefreshContactSide(VdSegmentUtil.SegmentMarker.Side1);
            //    RefreshContactSide(VdSegmentUtil.SegmentMarker.Side2);
            //}
            //else
            //{
            //for link between badges, central aligned
            RefreshLinkLayout();
            //}

            _captions.SetBounds();

            SetMarkers();
            updateUserCursor();
        }

        public ClientLinkable GetLinkable1()
        {
            return _end1;
        }

        public ClientLinkable GetLinkable2()
        {
            return _end2;
        }

        public Rect boundsProvider()
        {
            return new Rect(new Point(line.X1, line.Y1), new Point(line.X2, line.Y2));
        }

        public Point capOrgProvider()
        {
            var bounds = boundsProvider();
            var res = bounds.Location;
            res.Offset(bounds.Width*0.5, bounds.Height*0.5 - 40);
            return res;
        }

        public Point btnOrgProvider()
        {
            var bounds = boundsProvider();
            var res = bounds.Location;
            res.Offset(bounds.Width*0.5, bounds.Height*0.5);
            return res;
        }

        public override Rect ReportingBoundsProvider()
        {
            return Rect.Union(_end1.GetBounds(), _end2.GetBounds());
        }
    }
}