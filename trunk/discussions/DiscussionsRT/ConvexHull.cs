using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace VectorEngine
{
    public class ConvexHull
    {
        public static PointCollection FindConvexPolygon(List<Point> points)
        {
            List<Point> top = new List<Point>();
            List<Point> bottom = new List<Point>();

            IEnumerable<Point> finalTop;
            IEnumerable<Point> finalBottom;

            points.Sort((x, y) => { return (int) (x.X - y.X); });

            double deltaX = points.Last().X - points.First().X;
            double deltaY = points.Last().Y - points.First().Y;
            double denominator = (Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));

            for (int i = 1; i < points.Count - 1; i++)
                if (MinimumDistanceBetweenPointAndLine2D(points.First(), points.Last(), points[i], deltaX, deltaY,
                                                         denominator))
                    bottom.Add(points[i]);
                else
                    top.Add(points[i]);

            top.Insert(0, points.First());
            top.Add(points.Last());
            bottom.Insert(0, points.First());
            bottom.Add(points.Last());

            finalTop = ConvexHullCore(top, true);
            finalBottom = ConvexHullCore(bottom, false);

            return new PointCollection(finalTop.Union(finalBottom.Reverse()));
        }

        private static IEnumerable<Point> ConvexHullCore(List<Point> points, bool isTop)
        {
            List<Point> result = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                result.Add(points[i]);
                if (result.Count > 2 && !IsAngleConvex(result, isTop))
                {
                    result.Remove(result[result.Count - 2]);
                    result = ConvexHullCore(result, isTop).ToList();
                }
            }
            return result;
        }

        private static bool IsAngleConvex(List<Point> result, bool isTop)
        {
            Point lastPoint = result.Last();
            Point middlePoint = result[result.Count - 2];
            Point firstPoint = result[result.Count - 3];

            double firstAngle = Math.Atan2((middlePoint - firstPoint).Y, (middlePoint - firstPoint).X)*180/Math.PI;
            double secondAngle = Math.Atan2((lastPoint - middlePoint).Y, (lastPoint - middlePoint).X)*180/Math.PI;

            return isTop ? secondAngle < firstAngle : secondAngle > firstAngle;
        }

        private static bool MinimumDistanceBetweenPointAndLine2D(Point P1, Point P2, Point P3, double deltaX,
                                                                 double deltaY, double denominator)
        {
            double u = ((P3.X - P1.X)*deltaX + (P3.Y - P1.Y)*deltaY)/denominator;
            //double x = P1.X + u * deltaX;
            double y = P1.Y + u*deltaY;
            return y - P3.Y > 0;
        }
    }
}