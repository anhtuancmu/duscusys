using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Discussions;
using Discussions.RTModel.Model;
using System.IO;
using System.ComponentModel;

namespace DistributedEditor
{
    public class VdText : VdBaseShape, IVdShape, INotifyPropertyChanged
    {
        public const double MIN_SIZE = 30;

        private System.Windows.Shapes.Rectangle blMark;
        private System.Windows.Shapes.Rectangle tlMark;
        private System.Windows.Shapes.Rectangle trMark;
        private System.Windows.Shapes.Rectangle brMark;

        private Canvas _scene;
        private TextUC _textEnterUC;

        private string _txt = "Text here";

        public string Text
        {
            get { return _txt; }
            set
            {
                if (value != _txt)
                {
                    _txt = value;
                    NotifyPropertyChanged("Text");
                }
            }
        }

        private string _textOnGotFocus;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public delegate void CleanupRequest(int id);

        private readonly CleanupRequest _cleanupRequest;

        public delegate void OnChanged(VdText text);

        public OnChanged onChanged;

        /// <summary>
        /// Only fires on focus lost  
        /// </summary>
        /// <param name="text"></param>
        public delegate void OnEdited(VdText text);

        public OnEdited onEdited;

        public VdText(double x, double y, int owner, int shapeId, CleanupRequest cleanupRequest) :
            base(owner, shapeId)
        {
            initText(DaoUtils.UserIdToColor(owner));

            _cleanupRequest = cleanupRequest;

            HandleMove(x, y);

            RemoveFocus();
        }

        private void initText(Color c)
        {
            _textEnterUC = new TextUC(this);
            _textEnterUC.txtLabel.Effect = ShapeUtils.ShadowProvider();
            _textEnterUC.txtLabel.Foreground = new SolidColorBrush(c);
            _textEnterUC.DataContext = this;
            _textEnterUC.Tag = this;
            _textEnterUC.field.Tag = this;
            _textEnterUC.handle.Tag = this;
            _textEnterUC.textChanged += TextChanged;
            _textEnterUC.MouseWheel += MouseWheel;

            blMark = ShapeUtils.MakeMarker();
            blMark.Tag = this;

            tlMark = ShapeUtils.MakeMarker();
            tlMark.Tag = this;

            trMark = ShapeUtils.MakeMarker();
            trMark.Tag = this;

            brMark = ShapeUtils.MakeMarker();
            brMark.Tag = this;
        }

        private void TextChanged(string text)
        {
            _txt = text;
            _serializeText = true;
            SetBounds();
            onChanged(this);
        }

        private void HideMarkers()
        {
            blMark.Visibility = Visibility.Hidden;
            tlMark.Visibility = Visibility.Hidden;
            trMark.Visibility = Visibility.Hidden;
            brMark.Visibility = Visibility.Hidden;
        }

        private void ShowMarkers()
        {
            blMark.Visibility = Visibility.Visible;
            tlMark.Visibility = Visibility.Visible;
            trMark.Visibility = Visibility.Visible;
            brMark.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            _cursorView.Visibility  = Visibility.Hidden;
            _textEnterUC.Visibility = Visibility.Hidden;
            HideMarkers();
        }

        public void Show()
        {
            _cursorView.Visibility  = Visibility.Visible;
            _textEnterUC.Visibility = Visibility.Visible;
            ShowMarkers();
        }

        private void SetMarkers()
        {
            Rect bounds = GetFieldBounds();

            ShapeUtils.SetMarker(blMark, bounds.Left, bounds.Bottom);
            ShapeUtils.SetMarker(tlMark, bounds.Left, bounds.Top);
            ShapeUtils.SetMarker(trMark, bounds.Right, bounds.Top);
            ShapeUtils.SetMarker(brMark, bounds.Right, bounds.Bottom);
        }

        public bool IsVisible()
        {
            return _textEnterUC.Visibility == Visibility.Visible;
        }

        public UIElement UnderlyingControl()
        {
            return _textEnterUC;
        }

        public override void RemoveFocus()
        {
            //if we have acquired focus and are now losing it
            if (_isFocused && Text != _textOnGotFocus)
            {
                //caption title edited?
                if (onEdited != null)
                    onEdited(this);
            }

            base.RemoveFocus();

            if (_txt == "")
            {
                _cleanupRequest(Id());
            }

            HideMarkers();

            _textEnterUC.RemoveFocus();
        }

        public override void SetFocus()
        {
            base.SetFocus();

            _textOnGotFocus = Text;

            _textEnterUC.SetFocus();

            //text field has just been shown, no sizes of field yet
            _scene.Dispatcher.BeginInvoke(DispatcherPriority.Background,
            (Action)(() =>
            {
                if (!_isFocused) return;
                SetMarkers();
                ShowMarkers();
            }));
        }

        public ShapeZ ShapeZLevel()
        {
            return ShapeZ.SIMPLE_SHAPE_Z_LEVEL;
        }

        public void SetZIndex(int z)
        {
            Canvas.SetZIndex(_textEnterUC, z);
            Canvas.SetZIndex(_cursorView, z + 1);
            Canvas.SetZIndex(blMark, z + 2);
            Canvas.SetZIndex(tlMark, z + 3);
            Canvas.SetZIndex(trMark, z + 4);
            Canvas.SetZIndex(brMark, z + 5);
        }

        public void AttachToCanvas(Canvas c)
        {
            _scene = c;

            if (c.Children.Contains(_textEnterUC))
                return;

            c.Children.Add(_textEnterUC);
            c.Children.Add(_cursorView);
            
            if (!c.Children.Contains(blMark))
            {
                c.Children.Add(blMark);
                c.Children.Add(tlMark);
                c.Children.Add(trMark);
                c.Children.Add(brMark);                
            }

            SetBounds();
        }

        public void DetachFromCanvas(Canvas c)
        {
            c.Children.Remove(_textEnterUC);
            c.Children.Remove(_cursorView);

            c.Children.Remove(blMark);
            c.Children.Remove(tlMark);
            c.Children.Remove(trMark);
            c.Children.Remove(brMark);
        }

        private void SetBounds()
        {
            updateUserCursor();

            SetMarkers();
        }

        private void updateUserCursor()
        {
            var org = GetOrigin();

            Canvas.SetLeft(_cursorView, org.X - 50);
            Canvas.SetTop(_cursorView, org.Y - 50);
        }

        public void SetText(string text)
        {
            Text = text;
        }

        public VdShapeType ShapeCode()
        {
            return VdShapeType.Text;
        }

        //by default, text is not serialized. only enabled after text change event
        private bool _serializeText = false;

        public ShapeState GetState(int topicId)
        {
            var mat = _textEnterUC.RenderTransform.Value;

            byte[] textBytes = null;

            //encode string
            if (_serializeText)
            {
                _serializeText = false;
                using (var s = new MemoryStream())
                {
                    using (var bw = new BinaryWriter(s))
                    {
                        bw.Write(Text);
                    }
                    textBytes = s.ToArray();
                }
            }

            return new ShapeState(ShapeCode(),
                                  InitialOwner(),
                                  Id(),
                                  textBytes,
                                  null,
                                  new double[]
                                      {
                                          0, 0, mat.M11, mat.M12,
                                          mat.M21, mat.M22,
                                          mat.OffsetX, mat.OffsetY
                                      },
                                  topicId);
        }

        public void ApplyState(ShapeState st)
        {
            if (st.bytes != null)
            {
                using (var s = new MemoryStream(st.bytes))
                {
                    using (var br = new BinaryReader(s))
                    {
                        Text = br.ReadString();
                    }
                }
            }

            var m = new Matrix
            {
                M11 = st.doubles[2],
                M12 = st.doubles[3],
                M21 = st.doubles[4],
                M22 = st.doubles[5],
                OffsetX = st.doubles[6],
                OffsetY = st.doubles[7]
            };

            _textEnterUC.RenderTransform = new MatrixTransform(m);

            _scene.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                          (Action)SetBounds);
        }

        public override void ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            TouchManip = true;

            base.ManipulationStarting(sender, e);
        }

        public void ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            ShapeUtils.ApplyTransform(e, _textEnterUC);

            e.Handled = true;

            SetBounds();
        }

        private void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScaleInPlace(e.Delta > 0);
            onChanged(this);
            e.Handled = true;
        }

        public void StartManip(Point p, object sender)
        {
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

            CurrentPoint.X = p.X;
            CurrentPoint.Y = p.Y;
            SetFocus();
        }

        public PointApplyResult ApplyCurrentPoint(Point p)
        {
            if (base.TouchManip)
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

        PointApplyResult _applyCurrentPoint(Point p, ShapeUtils.RectSide side)
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
            SetBounds();
            CurrentPoint.X = p.X;
            CurrentPoint.Y = p.Y;

            return res;
        }



        //for cluster only
        public void SetPosForCluster(double x, double y)
        {
            var org = GetOrigin();
            var dx = x - org.X;
            var dy = y - org.Y;
            HandleMove(dx, dy);
        }

        private void HandleMove(double dx, double dy)
        {
            var manipOrg = GetOrigin();
            ShapeUtils.ApplyTransform(_textEnterUC,
                                      new Vector(manipOrg.X, manipOrg.Y),
                                      new Vector(dx, dy),
                                      0,
                                      new Vector(1, 1));
        }

        private void HandleResize(double deltaX, double deltaY, ShapeUtils.RectSide side)
        {
            Rect bounds = GetShapeBounds();
            Rect boundsBefore = bounds;
            try
            {
                switch (side)
                {
                    case ShapeUtils.RectSide.TopLeft:
                        bounds.X += deltaX;
                        bounds.Y += deltaY;
                        bounds.Width  -= deltaX;
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
                        bounds.Width  += deltaX;
                        bounds.Height += deltaY;
                        break;
                    case ShapeUtils.RectSide.TwoSided:
                        break;
                }
            }
            catch (Exception)
            {
            }

            if (bounds.Width > MIN_SIZE && bounds.Height > MIN_SIZE)
            {
                var manipOrg = _textEnterUC.RenderTransform.Transform(
                    new Point(0,0)
                );

                double xScale = bounds.Width/boundsBefore.Width;
                double yScale = bounds.Height/boundsBefore.Height;
                double scaleCoeff = Math.Max(xScale, yScale);
                var scale = new Vector(yScale,
                                       yScale);

                var translate = new Vector(bounds.X - boundsBefore.X,
                                           bounds.Y - boundsBefore.Y);

                ShapeUtils.ApplyTransform(_textEnterUC,
                                          new Vector(manipOrg.X, manipOrg.Y),
                                          translate,
                                          0,
                                          scale);

                SetBounds();
            }
        }

        public Point GetOrigin()
        {
            if (_textEnterUC.RenderTransform == null)
                return new Point(0, 0);

            return new Point(_textEnterUC.RenderTransform.Value.OffsetX,
                             _textEnterUC.RenderTransform.Value.OffsetY);
        }

        public Rect GetFieldBounds()
        {
            Point org = _textEnterUC.field.TranslatePoint(new Point(0, 0), _scene);

            var w = _textEnterUC.GetFieldActualWidth();
            var h = _textEnterUC.GetFieldActualHeight();
            var bottomRight = _textEnterUC.field.TranslatePoint(new Point(w, h), _scene);

            return new Rect(org, bottomRight);
        }

        public Rect GetShapeBounds()
        {
            Point org = _textEnterUC.TranslatePoint(new Point(0, 0), _scene);

            var w = _textEnterUC.ActualWidth;
            var h = _textEnterUC.ActualHeight;
            var bottomRight = _textEnterUC.TranslatePoint(new Point(w, h), _scene);

            return new Rect(org, bottomRight);
        }

        private void HandleScale(double scale)
        {
            var manipOrg = _textEnterUC.RenderTransform.Transform(new Point(0, 0));
            ShapeUtils.ApplyTransform(_textEnterUC, 
                                      new Vector(manipOrg.X, manipOrg.Y),
                                      new Vector(0, 0), 
                                      0, 
                                      new Vector(scale, scale));
        }

        public void ScaleInPlace(bool plus)
        {
            var s = ShapeUtils.scaleFactor(plus);
            HandleScale(s);

            _scene.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                          (Action)SetBounds);          
        }

        public double distToFigure(Point from)
        {
            HitTestResult htr = VisualTreeHelper.HitTest(_textEnterUC.handle, from);
            if (htr != null)
            {
                var border = htr.VisualHit as Border;
                if (border != null)
                    return 0;
            }

            var Org = _textEnterUC.txtLabel.TranslatePoint(new Point(0, 0), _scene);

            var w = _textEnterUC.txtLabel.ActualWidth;
            var h = _textEnterUC.txtLabel.ActualHeight;
            var TopRight = new Point(Org.X + w, Org.Y);
            var BottomRight = new Point(Org.X + w, Org.Y + h);
            var BottomLeft = new Point(Org.X, Org.Y + h);
            var Center = new Point(Org.X + w/2, Org.Y + h/2);

            double d1 = ShapeUtils.Dist(Org, from);
            double d2 = ShapeUtils.Dist(TopRight, from);
            double d3 = ShapeUtils.Dist(BottomRight, from);
            double d4 = ShapeUtils.Dist(BottomLeft, from);
            double d5 = ShapeUtils.Dist(Center, from);

            double dMin = ShapeUtils.Min(d1, d2, d3);
            dMin = ShapeUtils.Min(dMin, d4, d5);

            if (dMin > 270)
                dMin = double.MaxValue;

            return dMin;
        }

        public override Rect ReportingBoundsProvider()
        {
            return GetFieldBounds();            
        }

        public void MoveBy(double dx, double dy)
        {
            HandleMove(dx, dy);
        }
    }
}