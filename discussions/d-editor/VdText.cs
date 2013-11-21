using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Discussions;
using Discussions.RTModel.Model;
using System.IO;
using System.ComponentModel;

namespace DistributedEditor
{
    public class VdText : VdBaseShape, IVdShape, INotifyPropertyChanged
    {
        public const double MIN_SIZE = 30;

        private Canvas scene;
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

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public delegate void CleanupRequest(int id);

        private CleanupRequest _cleanupRequest;

        public delegate void OnChanged(VdText text);

        public OnChanged onChanged;

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
        }

        private void TextChanged(string text)
        {
            _txt = text;
            serializeText = true;
            onChanged(this);
        }

        public void Hide()
        {
            _cursorView.Visibility = Visibility.Hidden;
            _textEnterUC.Visibility = Visibility.Hidden;
        }

        public void Show()
        {
            _cursorView.Visibility = Visibility.Visible;
            _textEnterUC.Visibility = Visibility.Visible;
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
            base.RemoveFocus();

            if (_txt == "")
            {
                _cleanupRequest(Id());
            }

            _textEnterUC.RemoveFocus();
        }

        public override void SetFocus()
        {
            base.SetFocus();

            _textEnterUC.SetFocus();
        }

        public ShapeZ ShapeZLevel()
        {
            return ShapeZ.SIMPLE_SHAPE_Z_LEVEL;
        }

        public void SetZIndex(int z)
        {
            Canvas.SetZIndex(_textEnterUC, z);
            Canvas.SetZIndex(_cursorView, z + 1);
        }

        public void AttachToCanvas(Canvas c)
        {
            scene = c;

            if (c.Children.Contains(_textEnterUC))
                return;

            c.Children.Add(_textEnterUC);
            c.Children.Add(_cursorView);

            SetBounds();
        }

        public void DetachFromCanvas(Canvas c)
        {
            c.Children.Remove(_textEnterUC);
            c.Children.Remove(_cursorView);
        }

        private void SetBounds()
        {
            updateUserCursor();
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
        private bool serializeText = false;

        public ShapeState GetState(int topicId)
        {
            var mat = _textEnterUC.RenderTransform.Value;

            byte[] textBytes = null;

            //encode string
            if (serializeText)
            {
                serializeText = false;
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

            Matrix m = new Matrix();
            m.M11 = st.doubles[2];
            m.M12 = st.doubles[3];
            m.M21 = st.doubles[4];
            m.M22 = st.doubles[5];
            m.OffsetX = st.doubles[6];
            m.OffsetY = st.doubles[7];

            _textEnterUC.RenderTransform = new MatrixTransform(m);

            updateUserCursor();
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

            updateUserCursor();
        }

        private void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScaleInPlace(e.Delta > 0);
            onChanged(this);
            e.Handled = true;
        }

        public void StartManip(Point p, object sender)
        {
            CurrentPoint.X = p.X;
            CurrentPoint.Y = p.Y;
            SetFocus();
        }

        public PointApplyResult ApplyCurrentPoint(Point p)
        {
            if (base.TouchManip)
                return PointApplyResult.None;

            double dx = p.X - CurrentPoint.X;
            double dy = p.Y - CurrentPoint.Y;
            HandleMove(dx, dy);
            CurrentPoint.X = p.X;
            CurrentPoint.Y = p.Y;
            updateUserCursor();

            return PointApplyResult.None;
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

        public Point GetOrigin()
        {
            if (_textEnterUC.RenderTransform == null)
                return new Point(0, 0);
            else
                return new Point(_textEnterUC.RenderTransform.Value.OffsetX,
                                 _textEnterUC.RenderTransform.Value.OffsetY);
        }

        private void HandleScale(double scale)
        {
            var manipOrg = _textEnterUC.RenderTransform.Transform(new Point(0, 0));
            ShapeUtils.ApplyTransform(_textEnterUC, new Vector(manipOrg.X, manipOrg.Y),
                                      new Vector(0, 0), 0, new Vector(scale, scale));
        }

        public void ScaleInPlace(bool plus)
        {
            var s = ShapeUtils.scaleFactor(plus);
            HandleScale(s);
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

            var Org = _textEnterUC.txtLabel.TranslatePoint(new Point(0, 0), scene);

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

            if (dMin > 170)
                dMin = double.MaxValue;

            return dMin;
        }

        public override Rect ReportingBoundsProvider()
        {
            var org = _textEnterUC.txtLabel.TranslatePoint(new Point(0, 0), scene);
            var w = _textEnterUC.txtLabel.ActualWidth;
            var h = _textEnterUC.txtLabel.ActualHeight;
            var bottomRight = _textEnterUC.txtLabel.TranslatePoint(new Point(w, h), scene);

            return new Rect(org, bottomRight);
        }

        public void MoveBy(double dx, double dy)
        {
            HandleMove(dx, dy);
        }
    }
}