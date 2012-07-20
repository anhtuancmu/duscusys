using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using System.IO;
using System.Windows;
using System.Windows.Media.Effects;

namespace DistributedEditor
{
    public class ShapeUtils
    {
        public const double SZ = 10;
        public const double LINE_WIDTH = 4;
        public const double LINK_END_SZ = 10;

        public enum RectSide { None, BottomLeft, TopLeft, BottomRight, TopRight, TwoSided }
        
        static DropShadowEffect _dropShad = null;
        public static DropShadowEffect ShadowProvider()
        {
            if(_dropShad==null)
            {
                _dropShad = new DropShadowEffect();
                _dropShad.BlurRadius = 10;
                _dropShad.Opacity = 0.35;
                _dropShad.ShadowDepth = 3;
                _dropShad.RenderingBias = RenderingBias.Performance;                
            }
            return _dropShad;
        }

        public static void SetLinkEnd(System.Windows.Shapes.Ellipse end, double x, double y)
        {
            Canvas.SetLeft(end, x - LINK_END_SZ / 2);
            Canvas.SetTop(end, y - LINK_END_SZ / 2);
        }

        public static void SetMarker(System.Windows.Shapes.Rectangle rect, double x, double y) 
        {
            Canvas.SetLeft(rect, x - SZ / 2);
            Canvas.SetTop(rect,  y - SZ / 2);
        }

        public static System.Windows.Shapes.Rectangle MakeMarker()
        {
            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
            rect.Width = SZ;
            rect.Height = SZ;
            rect.Stroke = System.Windows.Media.Brushes.Blue;
            rect.Fill = System.Windows.Media.Brushes.Blue;
            rect.Opacity = 0.3;
            rect.StrokeThickness = 2;
            return rect;
        }

        public static System.Windows.Shapes.Ellipse MakeLinkEnd()
        {
            System.Windows.Shapes.Ellipse rect = new System.Windows.Shapes.Ellipse();
            rect.Width = LINK_END_SZ;
            rect.Height = LINK_END_SZ;
            rect.Stroke = System.Windows.Media.Brushes.Blue;
            rect.Fill = System.Windows.Media.Brushes.Blue;
            rect.Opacity = 0.3;
            rect.StrokeThickness = 2;
            return rect;
        }

        public static Matrix GetUnsolvedTransform(UIElement shape)
        {
            var transformation = shape.RenderTransform as MatrixTransform;
            if(transformation != null)
                return  transformation.Matrix;

            Matrix res = Matrix.Identity;
            return res;
        }

        public static void SetTransform(UIElement shape, double x, double y)
        {
            Matrix m = Matrix.Identity;
            m.Translate(x, y);
            shape.RenderTransform = new MatrixTransform(m);
        }

        public static void ApplyTransform(ManipulationDeltaEventArgs e, UIElement shape)
        {
            var matrix = GetUnsolvedTransform(shape);

            //matrix.RotateAt(e.DeltaManipulation.Rotation,
            //                e.ManipulationOrigin.X,
            //                e.ManipulationOrigin.Y);

            matrix.ScaleAt(e.DeltaManipulation.Scale.X,
                           e.DeltaManipulation.Scale.X,
                           e.ManipulationOrigin.X,
                           e.ManipulationOrigin.Y);

            matrix.Translate(e.DeltaManipulation.Translation.X,
                             e.DeltaManipulation.Translation.Y);

            shape.RenderTransform = new MatrixTransform(matrix);
        }

        public static void ApplyTransform(UIElement shape, Vector manipOrg, Vector translation, 
                                               double rotation, Vector scale)
        {
            var matrix = GetUnsolvedTransform(shape);

            //matrix.RotateAt(e.DeltaManipulation.Rotation,
            //                e.ManipulationOrigin.X,
            //                e.ManipulationOrigin.Y);

          //  matrix.Scale(scale.X, scale.Y);

            matrix.ScaleAt(scale.X,
                           scale.Y,
                           manipOrg.X,
                           manipOrg.Y);

            matrix.Translate(translation.X,
                             translation.Y);

            shape.RenderTransform = new MatrixTransform(matrix);
        }

        public static void ScalePrepend(UIElement shape, double sx, double sy)
        {
            var current = shape.RenderTransform.Value;
            current.ScalePrepend(sx, sy);
            shape.RenderTransform = new MatrixTransform(current);
        }

        public static Matrix GetTransform(ManipulationDeltaEventArgs e)
        {
            var matrix = Matrix.Identity;

            matrix.RotateAt(e.DeltaManipulation.Rotation,
                            e.ManipulationOrigin.X,
                            e.ManipulationOrigin.Y);

            matrix.ScaleAt(e.DeltaManipulation.Scale.X,
                           e.DeltaManipulation.Scale.X,
                           e.ManipulationOrigin.X,
                           e.ManipulationOrigin.Y);

            matrix.Translate(e.DeltaManipulation.Translation.X,
                             e.DeltaManipulation.Translation.Y);

            return matrix;
        }

        public static double Min(double d1, double d2, double d3, double d4)
        {
            double m1 = d1 < d2 ? d1 : d2;
            double m2 = d3 < d4 ? d3 : d4;
            double m3 = m1 < m2 ? m1 : m2;
            return m3;
        }

        public static double Max(double d1, double d2, double d3, double d4)
        {
            double m1 = d1 > d2 ? d1 : d2; 
            double m2 = d3 > d4 ? d3 : d4;
            double m3 = m1 > m2 ? m1 : m2;
            return m3;
        }

        public static double Min(double d1, double d2, double d3)
        {
            double m1 = d1 < d2 ? d1 : d2;
            double m2 = m1 < d3 ? m1 : d3;
            return m2;
        }

        public static Matrix InitTranslTransform(double x, double y)
        {
            var matrix = Matrix.Identity;
            matrix.Translate(x,y);
            return matrix;
        }

        public static void WriteTransform(BinaryWriter w, System.Windows.Media.Transform t)
        {
            w.Write(t.Value.M11);
            w.Write(t.Value.M12);
            w.Write(t.Value.M21);
            w.Write(t.Value.M22);
            w.Write(t.Value.OffsetX);
            w.Write(t.Value.OffsetY);
        }

        public static MatrixTransform ReadTransform(BinaryReader r)
        {
            Matrix m = new Matrix();
            m.M11 = r.ReadDouble();
            m.M12 = r.ReadDouble();
            m.M21 = r.ReadDouble();
            m.M22 = r.ReadDouble();
            m.OffsetX = r.ReadDouble();
            m.OffsetY = r.ReadDouble();                               
            return new MatrixTransform(m);
        }

        public static PointCollection MovePointCollection(PointCollection org, double dx, double dy)
        {
            var res = new PointCollection();

            foreach(var p1 in org)
            {
                var p2 = new Point(p1.X+dx, p1.Y+dy);
                res.Add(p2);
            }
            return res;
        }

        public static double Dist(Point p1, Point p2)
        {
            double dx = (p1.X - p2.X);
            double dy = (p1.Y - p2.Y);
            return Math.Sqrt(dx * dx + dy * dy); 
        }

        public static double Dist2(Point p1, Point p2)
        {
            double dx = (p1.X - p2.X);
            double dy = (p1.Y - p2.Y);
            return dx * dx + dy * dy;
        }

        public static double scaleFactor(bool plus)
        {
            return plus ? 1.1 : 0.9; 
        }

        public static double resizeDelta(bool plus)
        {
            return plus ? 7.2 : -7.2;
        }

        static bool PointInPolygon(Point p, List<Point> poly)
        {
            Point p1, p2;

            bool inside = false;

            if (poly.Count < 3)
            {
                return inside;
            }

            Point oldPoint = new Point(
            poly[poly.Count - 1].X, poly[poly.Count - 1].Y);

            for (int i = 0; i < poly.Count; i++)
            {
                Point newPoint = new Point(poly[i].X, poly[i].Y);

                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                && ((long)p.Y - (long)p1.Y) * (long)(p2.X - p1.X)
                 < ((long)p2.Y - (long)p1.Y) * (long)(p.X - p1.X))
                {
                    inside = !inside;
                }

                oldPoint = newPoint;
            }

            return inside;
        }

        //fuzzily decides whether clusterable is inside poly 
        public static bool FuzzyInside(Rect clusterable, List<Point> poly)
        {
            int i = 0;

            var rectCenter = new Point(clusterable.X + clusterable.Width / 2,
                                       clusterable.Y + clusterable.Height / 2);

            var leftCnt = new Point(clusterable.X,
                                    clusterable.Y + clusterable.Height / 2);

            var rightCnt = new Point(clusterable.X + clusterable.Width,
                                     clusterable.Y + clusterable.Height / 2);

            var topCnt = new Point(clusterable.X + clusterable.Width / 2,
                                   clusterable.Y);

            var botCnt = new Point(clusterable.X + clusterable.Width / 2,
                                   clusterable.Y + clusterable.Height);

            if (PointInPolygon(leftCnt, poly))
                ++i;
            if (PointInPolygon(rightCnt, poly))
                if (++i >= 2)
                    return true;
            if (PointInPolygon(topCnt, poly))
                if (++i >= 2)
                    return true;
            if (PointInPolygon(botCnt, poly))
                if (++i >= 2)
                    return true;
            if (PointInPolygon(clusterable.TopLeft, poly))
                if (++i >= 2)
                    return true;
            if (PointInPolygon(clusterable.TopRight, poly))
                if (++i >= 2)
                    return true;
            if (PointInPolygon(clusterable.BottomRight, poly))
                if (++i >= 2)
                    return true;
            if (PointInPolygon(clusterable.BottomLeft, poly))
                if (++i >= 2)
                    return true;
            if (PointInPolygon(rectCenter, poly))
                if (++i >= 2)
                    return true;

            return false;
        }

        public static Point ClusterCaptionPos(Point[] points1, Point[] points2, Point[] points3)
        {
            //find min Y, min and max X
            double minY = double.MaxValue;
            double minX = double.MaxValue;
            double maxX = double.MinValue;

            foreach (var p in points1)
            {
                if (p.X < minX)
                    minX = p.X;
                if (p.X > maxX)
                    maxX = p.X;
                if (p.Y < minY)
                    minY = p.Y;
            }
            foreach (var p in points2)
            {
                if (p.X < minX)
                    minX = p.X;
                if (p.X > maxX)
                    maxX = p.X;
                if (p.Y < minY)
                    minY = p.Y;
            }
            foreach (var p in points3)
            {
                if (p.X < minX)
                    minX = p.X;
                if (p.X > maxX)
                    maxX = p.X;
                if (p.Y < minY)
                    minY = p.Y;
            }

            return new Point((minX + maxX) * 0.5 - 80, minY - 60);
        }

        public static Point GetAnchorCoords(ClientLinkable end, AnchorPoint anchorPoint)
        {
            var border = end.GetBounds();
            border.Inflate(20,20);
            switch (anchorPoint)
            {
                case AnchorPoint.TopLeft:
                    return border.TopLeft;
                case AnchorPoint.TopCenter:
                    return new Point(border.X + border.Width / 2, border.Y);
                case AnchorPoint.TopRight:
                    return border.TopRight;
                case AnchorPoint.LeftCenter:
                    return new Point(border.X, border.Y + border.Height / 2);
                case AnchorPoint.BottomLeft:
                    return border.BottomLeft;
                case AnchorPoint.BottomCenter:
                    return new Point(border.X + border.Width / 2, border.Y + border.Height);
                case AnchorPoint.BottomRight:
                    return border.BottomRight;
                case AnchorPoint.RightCenter:
                    return new Point(border.X + border.Width, border.Y + border.Height / 2);
                default:
                    throw new NotSupportedException();
            }
        }

        //returns anchor point on searchTarget nearest to pivot
        public static void NearestAnchor(Point pivot, ClientLinkable searchTarget,
                                         out AnchorPoint anchor, out Point anchorPoint,
                                         out double minDist)
        {
            anchor = AnchorPoint.RightCenter;
            minDist = double.MaxValue;

            //double d = Dist2(pivot, GetAnchorCoords(searchTarget, AnchorPoint.TopLeft));
            //if (d < minDist)
            //{
            //    minDist = d;
            //    anchor = AnchorPoint.TopLeft;
            //}

            var d = Dist2(pivot, GetAnchorCoords(searchTarget, AnchorPoint.TopCenter));
            if (d < minDist)
            {
                minDist = d;
                anchor = AnchorPoint.TopCenter;
            }

            //d = Dist2(pivot, GetAnchorCoords(searchTarget, AnchorPoint.TopRight));
            //if (d < minDist)
            //{
            //    minDist = d;
            //    anchor = AnchorPoint.TopRight;
            //}

            d = Dist2(pivot, GetAnchorCoords(searchTarget, AnchorPoint.LeftCenter));
            if (d < minDist)
            {
                minDist = d;
                anchor = AnchorPoint.LeftCenter;
            }

            //d = Dist2(pivot, GetAnchorCoords(searchTarget, AnchorPoint.BottomLeft));
            //if (d < minDist)
            //{
            //    minDist = d;
            //    anchor = AnchorPoint.BottomLeft;
            //}

            d = Dist2(pivot, GetAnchorCoords(searchTarget, AnchorPoint.BottomCenter));
            if (d < minDist)
            {
                minDist = d;
                anchor = AnchorPoint.BottomCenter;
            }

            //d = Dist2(pivot, GetAnchorCoords(searchTarget, AnchorPoint.BottomRight));
            //if (d < minDist)
            //{
            //    minDist = d;
            //    anchor = AnchorPoint.BottomRight;
            //}

            d = Dist2(pivot, GetAnchorCoords(searchTarget, AnchorPoint.RightCenter));
            if (d < minDist)
            {
                minDist = d;
                anchor = AnchorPoint.RightCenter;
            }

            minDist = Math.Sqrt(minDist);
            anchorPoint = GetAnchorCoords(searchTarget, anchor);
        }

        public static void GetLinkPoints(ClientLinkable s, ClientLinkable t, 
                                         out double x1, out double y1, 
                                         out double x2, out double y2)
        {
            x1 = 0;
            y1 = 0;
            x2 = 0;
            y2 = 0;

            var sBounds = s.GetBounds();
          //  sBounds.Inflate(10,10);
          //  sBounds.X += 5;
          //  sBounds.Y += 15;

            var tBounds = t.GetBounds();
           // tBounds.Inflate(10,10); 
           // tBounds.X += 5;
         //   tBounds.Y += 15;
         
            //c is point center of <s-t>, lambda is in [0,1]
            var xs = sBounds.X + sBounds.Width/2;
            var ys = sBounds.Y + sBounds.Height/2;
            
            var xt = tBounds.X + tBounds.Width / 2;
            var yt = tBounds.Y + tBounds.Height / 2;

            var xc = (xs + xt) * 0.5;
            var yc = (ys + yt) * 0.5;

            //s-anchor
            EdgeFinder((double x, double y)=>sBounds.Contains(x, y),
                       xs, ys, xc, yc, 5, out x1, out y1);

            //t-anchor
            EdgeFinder((double x, double y) => tBounds.Contains(x, y),
                       xt, yt, xc, yc, 5, out x2, out y2);
        }

        public delegate bool PointTester(double x, double y);
        public static void EdgeFinder(PointTester tester, double x0, double y0, double x1, double y1, 
                                      int recursionSteps, out double x, out double y)
        {
           var xc = (x0 + x1)*0.5;
           var yc = (y0 + y1)*0.5;
           x = xc;
           y = yc;
           if(recursionSteps==0)
               return;

           var val0 = tester(x0,y0);
           var valC = tester(xc,yc);
           if (val0 != valC)
               EdgeFinder(tester, x0, y0, xc, yc, recursionSteps - 1, out x, out y);
           else
           {
               var val1 = tester(x1, y1);
               if (val1 != valC)
                   EdgeFinder(tester, xc, yc, x1, y1, recursionSteps - 1, out x, out y);
               else
                   return;
           }
        }

        public static List<Point> buildBadgeCorners(List<ClientClusterable> badges)
        {
            var res = new List<Point>(4*badges.Count());

            foreach (var b in badges)
            {
                var bounds = b.GetBounds();
                bounds.Inflate(5, 5);
                res.Add(bounds.TopLeft);
                res.Add(bounds.TopRight);
                res.Add(bounds.BottomLeft);
                res.Add(bounds.BottomRight);
            }
            return res;
        }
    }
}
