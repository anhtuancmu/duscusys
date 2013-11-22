using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media;
using Discussions;
using Discussions.RTModel.Model;

namespace DistributedEditor
{
    internal class VdSegment : VdBaseShape, IVdShape
    {
        private System.Windows.Shapes.Line line;
        private System.Windows.Shapes.Rectangle selMarker1;
        private System.Windows.Shapes.Rectangle selMarker2;
        private Canvas scene;
        private VdSegmentUtil.SegmentMarker markerSide;

        public VdSegment(Double x1, Double y1, Double x2, Double y2, int owner, int shapeId) :
            base(owner, shapeId)
        {
            initLine(DaoUtils.UserIdToColor(owner));
            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;
            RemoveFocus();
        }

        private void initLine(Color c)
        {
            line = new Line();
            line.Stroke = new SolidColorBrush(c);
            line.StrokeThickness = ShapeUtils.LINE_WIDTH;
            line.Effect = ShapeUtils.ShadowProvider();
            line.Tag = this;

            selMarker1 = ShapeUtils.MakeMarker();
            selMarker1.Tag = this;

            selMarker2 = ShapeUtils.MakeMarker();
            selMarker2.Tag = this;
        }

        public UIElement UnderlyingControl()
        {
            return line;
        }

        public override void ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            TouchManip = true;

            SetMarkers();
            // VdSegmentUtil.ShowMarkers(selMarker1, selMarker2);

            base.ManipulationStarting(sender, e);
        }

        public void ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            Manipulation(e);

            updateUserCursor();

            SetMarkers();
            e.Handled = true;
        }

        private void updateUserCursor()
        {
            Canvas.SetLeft(_cursorView, line.X1 - 50);
            Canvas.SetTop(_cursorView, line.Y1 - 50);
        }

        private void Manipulation(ManipulationDeltaEventArgs e)
        {
            var mt = new MatrixTransform(ShapeUtils.GetTransform(e));

            var Point1 = mt.Transform(new Point(line.X1, line.Y1));
            var Point2 = mt.Transform(new Point(line.X2, line.Y2));
            line.X1 = Point1.X;
            line.Y1 = Point1.Y;
            line.X2 = Point2.X;
            line.Y2 = Point2.Y;
        }

        private void SetMarkers()
        {
            ShapeUtils.SetMarker(selMarker1, line.X1, line.Y1);
            ShapeUtils.SetMarker(selMarker2, line.X2, line.Y2);
        }

        public void Hide()
        {
            _cursorView.Visibility = Visibility.Hidden;
            line.Visibility = Visibility.Hidden;
            VdSegmentUtil.HideMarkers(selMarker1, selMarker2);
        }

        public void Show()
        {
            _cursorView.Visibility = Visibility.Visible;
            line.Visibility = Visibility.Visible;
            VdSegmentUtil.ShowMarkers(selMarker1, selMarker2);
        }

        public bool IsVisible()
        {
            return line.Visibility == Visibility.Visible;
        }

        public override void RemoveFocus()
        {
            base.RemoveFocus();

            line.Stroke = new SolidColorBrush(DaoUtils.UserIdToColor(_initialOwner));
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

            // SetFocus();
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
        }

        public void AttachToCanvas(Canvas c)
        {
            scene = c;

            if (!c.Children.Contains(line))
                c.Children.Add(line);

            c.Children.Add(selMarker1);
            c.Children.Add(selMarker2);

            c.Children.Add(_cursorView);
        }

        public void DetachFromCanvas(Canvas c)
        {
            scene = null;

            c.Children.Remove(line);

            c.Children.Remove(selMarker1);
            c.Children.Remove(selMarker2);

            c.Children.Remove(_cursorView);
        }

        public VdShapeType ShapeCode()
        {
            return VdShapeType.Segment;
        }

        public ShapeState GetState(int topicId)
        {
            return new ShapeState(ShapeCode(),
                                  InitialOwner(),
                                  Id(),
                                  null,
                                  null,
                                  new double[] {line.X1, line.Y1, line.X2, line.Y2},
                                  topicId);
        }

        public void ApplyState(ShapeState st)
        {
            line.X1 = st.doubles[0];
            line.Y1 = st.doubles[1];
            line.X2 = st.doubles[2];
            line.Y2 = st.doubles[3];
            SetMarkers();
            updateUserCursor();
        }

        public void StartManip(Point p, object sender)
        {
            CurrentPoint = p;

            double d1 = ShapeUtils.Dist(p, new Point(line.X1, line.Y1));
            double d2 = ShapeUtils.Dist(p, new Point(line.X2, line.Y2));
            double dc = ShapeUtils.Dist(p, new Point((line.X1 + line.X2)/2, (line.Y1 + line.Y2)/2));
            double dMin = ShapeUtils.Min(d1, d2, dc);

            if (dMin == d1)
            {
                markerSide = VdSegmentUtil.SegmentMarker.Side1;
            }
            else if (dMin == dc)
            {
                markerSide = VdSegmentUtil.SegmentMarker.Center;
            }
            else if (dMin == d2)
            {
                markerSide = VdSegmentUtil.SegmentMarker.Side2;
            }

            activeMarker = null;
            if (markerSide == VdSegmentUtil.SegmentMarker.Side1)
            {
                activeMarker = selMarker1;
                activeMarker.CaptureMouse();
            }
            else if (markerSide == VdSegmentUtil.SegmentMarker.Side2)
            {
                activeMarker = selMarker2;
                activeMarker.CaptureMouse();
            }
        }

        public PointApplyResult ApplyCurrentPoint(Point p)
        {
            if (TouchManip)
                return PointApplyResult.None;

            double dx = p.X - CurrentPoint.X;
            double dy = p.Y - CurrentPoint.Y;

            if (activeMarker == null)
            {
                HandleMove(dx, dy);
            }
            else
            {
                HandleResize(dx, dy, markerSide);
            }
            SetMarkers();
            updateUserCursor();

            CurrentPoint = p;

            return PointApplyResult.None; //todo
        }

        private void HandleMove(double deltaX, double deltaY)
        {
            line.X1 += deltaX;
            line.Y1 += deltaY;
            line.X2 += deltaX;
            line.Y2 += deltaY;
        }

        private void HandleResize(double deltaX, double deltaY, VdSegmentUtil.SegmentMarker side)
        {
            switch (side)
            {
                case VdSegmentUtil.SegmentMarker.Side1:
                    line.X1 += deltaX;
                    line.Y1 += deltaY;
                    break;
                case VdSegmentUtil.SegmentMarker.Side2:
                    line.X2 += deltaX;
                    line.Y2 += deltaY;
                    break;
            }
        }

        public void ScaleInPlace(bool plus)
        {
            var s = ShapeUtils.scaleFactor(plus);

            double cx = (line.X1 + line.X2)/2;
            double cy = (line.Y1 + line.Y2)/2;

            Vector c = new Vector(cx, cy);
            Vector v1 = new Vector(line.X1 - cx, line.Y1 - cy);
            Vector v2 = new Vector(line.X2 - cx, line.Y2 - cy);

            Vector newV1 = c + v1*s;
            line.X1 = newV1.X;
            line.Y1 = newV1.Y;

            Vector newV2 = c + v2*s;
            line.X2 = newV2.X;
            line.Y2 = newV2.Y;

            SetMarkers();
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
            line.X1 += dx;
            line.Y1 += dy;

            line.X2 += dx;
            line.Y2 += dy;
        }
    }
}