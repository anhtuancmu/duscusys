using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Ink;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using Discussions;
using Discussions.RTModel.Model;
using System.IO;

namespace DistributedEditor
{
    public class VdFreeForm : VdBaseShape, IVdShape
    {
        public const double MIN_SIZE = 30;

        DrawingGroup drawGrp;
        StrokeCollection _strokes = null;

        Image img;

        private Canvas scene;

        Rect _bounds;

        Color _clr;

        //only send strokes once (efficiency)
        bool strokesSent = false;
        public bool locallyJustCreated = false;

        System.Windows.Shapes.Rectangle blMark;
        System.Windows.Shapes.Rectangle tlMark;
        System.Windows.Shapes.Rectangle trMark;
        System.Windows.Shapes.Rectangle brMark;
        System.Windows.Shapes.Rectangle cMark;

        public VdFreeForm(int shapeId, int owner) :
            base(owner, shapeId)
        {
            _clr = DaoUtils.UserIdToColor(owner);
            init();

            RemoveFocus();
        }

        //local calls only 
        public void extractGeomtry(StrokeCollection strokes, Rect bounds)
        {
            _bounds = bounds;

            _strokes = strokes.Clone();
            foreach (Stroke strk in strokes)
            {                
                var brush = new SolidColorBrush(strk.DrawingAttributes.Color);             
                drawGrp.Children.Add(  new GeometryDrawing( brush, null, strk.GetGeometry())  );
            }

            SetMarkers();
            SetBounds();
        }

        public UIElement UnderlyingControl()
        {
            return img;
        }

        public StrokeCollection GetStrokes()
        {
            return _strokes;
        }

        public Image GetImage()
        {
            return img;
        }

        public Rect GetBounds()
        {
            return _bounds;
        }

        void init()
        {
            drawGrp = new DrawingGroup();

            initImg();

            blMark = ShapeUtils.MakeMarker();
            blMark.Tag = this;

            tlMark = ShapeUtils.MakeMarker();
            tlMark.Tag = this;

            trMark = ShapeUtils.MakeMarker();
            trMark.Tag = this;

            brMark = ShapeUtils.MakeMarker();
            brMark.Tag = this;

            cMark = ShapeUtils.MakeMarker();
            cMark.Tag = this;
        }

        void HideMarkers()
        {
            blMark.Visibility = Visibility.Hidden;
            tlMark.Visibility = Visibility.Hidden;
            trMark.Visibility = Visibility.Hidden;
            brMark.Visibility = Visibility.Hidden;
            cMark.Visibility  = Visibility.Hidden;
        }

        void ShowMarkers()
        {
            blMark.Visibility = Visibility.Visible;
            tlMark.Visibility = Visibility.Visible;
            trMark.Visibility = Visibility.Visible;
            brMark.Visibility = Visibility.Visible;
            cMark.Visibility = Visibility.Visible;
        }

        void SetMarkers()
        {
            ShapeUtils.SetMarker(blMark, _bounds.Left, _bounds.Bottom);
            ShapeUtils.SetMarker(tlMark, _bounds.Left, _bounds.Top);
            ShapeUtils.SetMarker(trMark, _bounds.Right, _bounds.Top);
            ShapeUtils.SetMarker(brMark, _bounds.Right, _bounds.Bottom);
            ShapeUtils.SetMarker(cMark, (_bounds.Left + _bounds.Right) / 2,
                                        (_bounds.Top + _bounds.Bottom) / 2);
        }

        public void Hide()
        {
            _cursorView.Visibility = Visibility.Hidden;
            img.Visibility = Visibility.Hidden;
            HideMarkers();
        }

        public void Show()
        {
            _cursorView.Visibility = Visibility.Visible;
            img.Visibility = Visibility.Visible;
            ShowMarkers();          
        }

        public bool IsVisible()
        {
            return img.Visibility == Visibility.Visible;
        }

        public void StartManip(Point p, object sender)
        {
            CurrentPoint = p;

            SetMarkers();
            SetFocus();

            activeMarker = null;
            if (sender == blMark)
            {
                activeMarker = sender as Rectangle;
                activeMarker.CaptureMouse();
            }
            else if (sender == tlMark)
            {
                activeMarker = sender as Rectangle;
                activeMarker.CaptureMouse();
            }
            else if (sender == trMark)
            {
                activeMarker = sender as Rectangle;
                activeMarker.CaptureMouse();
            }
            else if (sender == brMark)
            {
                activeMarker = sender as Rectangle;
                activeMarker.CaptureMouse();
            }
            else
            {
            }
        }

        public PointApplyResult ApplyCurrentPoint(Point p)
        {
            if (TouchManip)
                return PointApplyResult.None;

            if (activeMarker == blMark)
            {
                return _applyCurrentPoint(p, ShapeUtils.RectSide.BottomLeft);
            }
            else if (activeMarker == tlMark)
            {
                return _applyCurrentPoint(p, ShapeUtils.RectSide.TopLeft);
            }
            else if (activeMarker == trMark)
            {
                return _applyCurrentPoint(p, ShapeUtils.RectSide.TopRight);
            }
            else if (activeMarker == brMark)
            {
                return _applyCurrentPoint(p, ShapeUtils.RectSide.BottomRight);
            }
            else
            {
                return _applyCurrentPoint(p, ShapeUtils.RectSide.None);
            }
        }

        public PointApplyResult _applyCurrentPoint(Point p, ShapeUtils.RectSide side)
        {
            var res = PointApplyResult.None;
            
            double dx = p.X - CurrentPoint.X;
            double dy = p.Y - CurrentPoint.Y;

            if (activeMarker == null)
            {
                HandleMove(dx, dy);
                if (dx != 0.0 || dy != 0.0)
                    res = PointApplyResult.Move;
            }
            else
            {
                HandleResize(dx, dy, side);
                if (dx != 0.0 || dy != 0.0)
                    res = PointApplyResult.Resize;
            }
            SetMarkers();
            SetBounds();
            CurrentPoint = p;

            return res;
        }

        void HandleMove(double deltaX, double deltaY)
        {
            if (scene == null)
                return;

            if (_bounds.X < deltaX && deltaX < 0)
                return;
            if (_bounds.Y < deltaY && deltaY < 0)
                return;
            if (_bounds.X + _bounds.Width + deltaX > scene.ActualWidth && deltaX > 0)
                return;
            if (_bounds.Y + _bounds.Height + deltaY > scene.ActualHeight && deltaY > 0)
                return;

            _bounds.X += deltaX;
            _bounds.Y += deltaY;
        }

        public override void ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            TouchManip = true;

            SetMarkers();
            //ShowMarkers();

            e.Handled = true;

            base.ManipulationStarting(sender, e);       
        }

        public void ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            HandleResizeMultiplicative(e.DeltaManipulation.Scale.X, e.DeltaManipulation.Scale.Y);
            HandleMove(e.DeltaManipulation.Translation.X, e.DeltaManipulation.Translation.Y);

            SetMarkers();
            e.Handled = true;
        }

        void updateUserCursor()
        {
            Canvas.SetLeft(_cursorView, _bounds.X - 50);
            Canvas.SetTop(_cursorView,  _bounds.Y - 50);
        }

        void SetBounds()
        {
            updateUserCursor();
            img.Width  = _bounds.Width;
            img.Height = _bounds.Height;
            Canvas.SetLeft(img, _bounds.Left);
            Canvas.SetTop(img, _bounds.Top);
        }

        void HandleResizeMultiplicative(double scaleX, double scaleY)
        {
            double newWidth = _bounds.Width;
            double newHeight = _bounds.Height;

            newWidth *= scaleX;
            newHeight *= scaleY;

            if (newWidth < MIN_SIZE && newHeight < MIN_SIZE)
                return;

            double cx = (_bounds.Left + _bounds.Right) / 2;
            double cy = (_bounds.Top + _bounds.Bottom) / 2;

            _bounds.X = cx - newWidth / 2;
            _bounds.Width = newWidth;
            _bounds.Y = cy - newHeight / 2;
            _bounds.Height = newHeight;

            SetBounds();
        }

        void HandleResize(double deltaX, double deltaY, ShapeUtils.RectSide side)
        {
            var bounds = _bounds;

            try
            {
                switch (side)
                {
                    case ShapeUtils.RectSide.TopLeft:
                        bounds.X += deltaX;
                        bounds.Y += deltaY;
                        bounds.Width -= deltaX;
                        bounds.Height -= deltaY;
                        break;
                    case ShapeUtils.RectSide.TopRight:
                        bounds.Width += deltaX;

                        bounds.Y += deltaY;
                        bounds.Height -= deltaY;
                        break;
                    case ShapeUtils.RectSide.BottomLeft:
                        bounds.X += deltaX;
                        bounds.Width -= deltaX;

                        bounds.Height += deltaY;
                        break;
                    case ShapeUtils.RectSide.BottomRight:
                        bounds.Width += deltaX;
                        bounds.Height += deltaY;
                        break;
                    case ShapeUtils.RectSide.TwoSided:
                        double cx = (bounds.Left + bounds.Right) / 2;
                        double cy = (bounds.Top + bounds.Bottom) / 2;
                        bounds.Width += deltaX / 2;
                        bounds.Height += deltaY / 2;
                        bounds.X = cx - bounds.Width / 2;
                        bounds.Y = cy - bounds.Height / 2;
                        break;
                }
            }
            catch (Exception)
            {
            }

            if (bounds.Width > MIN_SIZE && bounds.Height > MIN_SIZE)
                _bounds = bounds;
        }

        public VdShapeType ShapeCode()
        {
            return VdShapeType.FreeForm;
        }

        public ShapeState GetState(int topicId)
        {
            byte[] isfData = null;
            if (!strokesSent && locallyJustCreated)
            {
                strokesSent = true;
                var isf = new MemoryStream();
                _strokes.Save(isf);
                isfData = isf.ToArray();
            }

            return new ShapeState(ShapeCode(),
                                  InitialOwner(),
                                  Id(),
                                  isfData,
                                  null,
                                  new double[] { _bounds.X, _bounds.Y, _bounds.Width, _bounds.Height },
                                  topicId);
        }

        public void ApplyState(ShapeState st)
        {
            _bounds.X = st.doubles[0];
            _bounds.Y = st.doubles[1];
            _bounds.Width  = st.doubles[2];
            _bounds.Height = st.doubles[3];
           
            if (st.bytes != null)
            {
                var s = new MemoryStream();
                s.Write(st.bytes, 0, st.bytes.Length);
                s.Position = 0;
                _strokes = new System.Windows.Ink.StrokeCollection(s);
                extractGeomtry(_strokes, _bounds);
            }

            SetBounds();
            SetMarkers();
        }

        void initImg()
        {
            if (img == null)
            {
                img = new Image();
                img.Source = new DrawingImage(drawGrp);
                img.Effect = ShapeUtils.ShadowProvider();
                img.Tag = this;
            }
        }

        public void AttachToCanvas(Canvas c)
        {
            scene = c;

            initImg();
            if (!c.Children.Contains(img))
            {
                c.Children.Add(img);

                Canvas.SetLeft(img, _bounds.X);
                Canvas.SetTop(img, _bounds.Y);
            }

            if (!c.Children.Contains(blMark))
            {
                c.Children.Add(blMark);
                c.Children.Add(tlMark);
                c.Children.Add(trMark);
                c.Children.Add(brMark);
                c.Children.Add(cMark);
            }

            c.Children.Add(_cursorView);
        }

        public void DetachFromCanvas(Canvas c)
        {
            scene = null;

            c.Children.Remove(img);

            c.Children.Remove(blMark);
            c.Children.Remove(tlMark);
            c.Children.Remove(trMark);
            c.Children.Remove(brMark);
            c.Children.Remove(cMark);

            c.Children.Remove(_cursorView);
        }

        public void GetWH(out double w, out double h)
        {
            w = img.ActualWidth;
            h = img.ActualHeight;
        }

        public override void SetFocus()
        {
            base.SetFocus();
            ShowMarkers();
        }

        public override void RemoveFocus()
        {
            base.RemoveFocus();
            HideMarkers();
        }

        public ShapeZ ShapeZLevel()
        {
            return ShapeZ.SIMPLE_SHAPE_Z_LEVEL;
        }

        public void SetZIndex(int z)
        {
            Canvas.SetZIndex(img, z);

            Canvas.SetZIndex(blMark, z + 1);
            Canvas.SetZIndex(tlMark, z + 2);
            Canvas.SetZIndex(trMark, z + 3);
            Canvas.SetZIndex(brMark, z + 4);
            Canvas.SetZIndex(cMark,  z + 5);
        }

        public object GetShape()
        {
            return img;
        }

        public void ScaleInPlace(bool plus)
        {
            var s = ShapeUtils.resizeDelta(plus);
            HandleResize(s, s, ShapeUtils.RectSide.TwoSided);
            SetMarkers();
            SetBounds();
        }

        public double distToFigure(Point from)
        {                                    
            var d1 = ShapeUtils.Dist(_bounds.TopLeft, from);
            var d2 = ShapeUtils.Dist(_bounds.TopRight, from);
            var d3 = ShapeUtils.Dist(_bounds.BottomLeft, from);
            var d4 = ShapeUtils.Dist(_bounds.BottomRight, from);

            var cx = (_bounds.Left + _bounds.Right) / 2;
            var cy = (_bounds.Top + _bounds.Bottom) / 2;
            var d5 = ShapeUtils.Dist(new Point(cx, cy), from);

            return Math.Min(ShapeUtils.Min(d1, d2, d3, d4), d5);
        }

        public void MoveBy(double dx, double dy)
        {
            _bounds.X += dx;
            _bounds.Y += dy;
        }

        //used by cluster
        public void SetPosForCluster(double x, double y)
        {
            _bounds.X = x;
            _bounds.Y = y;
            SetBounds();
            SetMarkers();
        }

        //used by cluster
        public void SetWH(double w, double h)
        {
            _bounds.Width = w;
            _bounds.Height = h;
            SetBounds();
            SetMarkers();
        }

        //used by cluster
        public Point GetOrigin()
        {
            return _bounds.TopLeft;
        }
    }
}
