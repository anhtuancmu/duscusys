using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Discussions.DbModel;
using Discussions.d_editor;

namespace DistributedEditor
{
    internal class DocTools
    {
        public const int TAG_UNDEFINED = -1;

        public static ICaptionHost GetCaptionHost(IEnumerable<IVdShape> shapes, IVdShape captionShape)
        {
            foreach (var s in shapes)
            {
                var capHost = s as ICaptionHost;
                if (capHost == null)
                    continue;

                if (capHost.CapMgr().text == captionShape || capHost.CapMgr().FreeDraw == captionShape)
                    return capHost;
            }
            return null;
        }

        public static ClientLinkable RequestLinkable(IVdShape sh)
        {
            var badge = sh as LinkableHost;
            if (badge != null)
                return badge.GetLinkable();

            return null;
        }

        //forces all badges to rebuild their views. but current instance of DB context is used (need to update it manually)
        public static void ToggleBadgeContexts(DiscCtx ctx, IEnumerable<IVdShape> badges)
        {
            foreach (var v in badges)
                ((VdBadge) v).RecontextBadge(ctx);
        }

        public static int[] ShapesToIds(IEnumerable<IVdShape> shapes)
        {
            var res = new List<int>(shapes.Count());
            foreach (var sh in shapes)
                res.Add(sh.Id());
            return res.ToArray();
        }

        public static void UnfocusAll(IEnumerable<IVdShape> shapes)
        {
            foreach (var v in shapes)
                v.RemoveFocus();
        }

        //after return:
        //1. any arrow, line, text tool are behind any badge
        //2. relative order of simple shapes preserves
        //3. palette is behind any simple shape 
        //4. badges are behind their clusters
        public static void SortScene(Canvas scene)
        {
            //accumulate list of shapes 
            var shapes = new List<IVdShape>();
            Palette palette = null;
            foreach (var child in scene.Children)
            {
                var sh = ((FrameworkElement) child).Tag as IVdShape;
                if (sh != null)
                    shapes.Add(sh);
                else if (palette == null)
                    palette = child as Palette;
            }

            //stable!
            var sorted = shapes.OrderBy(sh => sh, new ShapeZComparer());

            //apply sorted indices 
            var zIdx = 0;
            foreach (var sh in sorted)
            {
                sh.SetZIndex(zIdx);
                zIdx += 7; //up to 5 index positions for nodes, one for user cursor, and one for shape itself 
            }

            //palette on top
            if (palette != null)
                Canvas.SetZIndex(palette, zIdx);
        }

        public class ShapeZComparer : IComparer<IVdShape>
        {
            int IComparer<IVdShape>.Compare(IVdShape x, IVdShape y)
            {
                return x.ShapeZLevel() - y.ShapeZLevel();
            }
        }

        public static IVdShape MakeShape(VdShapeType shapeType,
                                         int owner,
                                         int shapeId,
                                         double startX,
                                         double startY,
                                         int tag)
        {
            switch ((VdShapeType) shapeType)
            {
                case VdShapeType.Segment:
                    return new VdSegment(startX, startY, startX + 1, startY + 1, owner, shapeId);
                case VdShapeType.Arrow:
                    return new VdArrow(startX, startY, startX + 1, startY + 1, owner, shapeId);
                case VdShapeType.FreeForm:
                    return new VdFreeForm(shapeId, owner);
                case VdShapeType.Badge:
                    return new VdBadge(startX, startY, owner, shapeId, tag);
                default:
                    throw new NotSupportedException();
            }
        }

        //IVdShape
        public static object DetectSelectedShape(VdDocument doc, Point pos,
                                                 TouchDevice d,
                                                 out Shape resizeNode)
        {
            resizeNode = null;

            HitTestResult htr = VisualTreeHelper.HitTest(doc.Scene, pos);
            if (htr == null)
                return null;

            var cluster = ShapeHitTester.findVdCluster(htr.VisualHit);
            if (cluster != null)
                return cluster;

            var text = ShapeHitTester.findVdText(htr.VisualHit);
            if (text != null)
                return text;

            var badge = ShapeHitTester.findVdBadge(htr.VisualHit);
            if (badge != null)
                return badge;

            var img = ShapeHitTester.findVdImg(htr.VisualHit);
            if (img != null)
                return img;

            //is it resize node? 
            resizeNode = htr.VisualHit as Shape;
            if (resizeNode != null)
            {
                var vdShape = resizeNode.Tag as IVdShape;
                if (vdShape != null && vdShape.IsVisible())
                    return vdShape;
            }

            //empty area, find nearest shape
            return NearestShape(doc, pos);
        }

        private static IVdShape NearestShape(VdDocument doc, Point point)
        {
            IVdShape res = null;
            double minDist = double.MaxValue;
            foreach (var sh in doc.GetShapes())
            {
                if (!sh.IsVisible())
                    continue;

                if (sh.ShapeCode() == VdShapeType.Badge)
                    continue;

                double d_i = sh.distToFigure(point);

                if (sh.ShapeCode() == VdShapeType.Arrow)
                    d_i /= 2.0;

                if (d_i < minDist)
                {
                    minDist = d_i;
                    res = sh;
                }
            }

            const double MAX_NEIGHBOURHOOD = 70;
            if (minDist > MAX_NEIGHBOURHOOD)
                res = null;

            return res;
        }

        /// <summary>
        /// owner can have only one cursor, this implies owner=>cursor=>shape        
        /// </summary>
        /// <returns>
        /// if given owner holds cursor, returns cursored shape.
        /// returns null otherwise
        /// </returns>
        public static IVdShape CursorOwnerToShape(int ownerId, IEnumerable<IVdShape> shapes)
        {
            foreach (var sh in shapes)
            {
                var c = sh.GetCursor();
                if (c != null && c.OwnerId == ownerId)
                    return sh;
            }
            return null;
        }
    }
}