using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Discussions;

namespace DistributedEditor
{
    public class ShapeHitTester
    {
        public static IVdShape findVdText(object originalSrc)
        {
            DependencyObject findSource = originalSrc as FrameworkElement;
            TextUC vdText = null;
               
            while (vdText == null && findSource != null)
            {
                if ((vdText = findSource as TextUC) == null)
                {
                    findSource = VisualTreeHelper.GetParent(findSource);
                }
            }

            if(vdText==null)
                return null;
            else
                return vdText.Tag as IVdShape;
        }
      
        public static IVdShape findVdCluster(object originalSrc)
        {
            DependencyObject findSource = originalSrc as FrameworkElement;
            System.Windows.Shapes.Path vdCluster = null;

            while (vdCluster == null && findSource != null)
            {
                if ((vdCluster = findSource as System.Windows.Shapes.Path) == null)
                {
                    findSource = VisualTreeHelper.GetParent(findSource);
                }
            }

            if (vdCluster == null)
                return null;
            else
                return vdCluster.Tag as IVdShape;
        }

        public static IVdShape findVdBadge(object originalSrc)
        {
            DependencyObject findSource = originalSrc as FrameworkElement;
            Badge4 vdBadge = null;

            while (vdBadge == null && findSource != null)
            {
                if ((vdBadge = findSource as Badge4) == null)
                {
                    findSource = VisualTreeHelper.GetParent(findSource);
                }
            }

            if (vdBadge == null)
                return null;
            else
                return vdBadge.Tag as IVdShape;
        }
        
        public static IVdShape findVdImg(object originalSrc)
        {
            DependencyObject findSource = originalSrc as FrameworkElement;
            System.Windows.Controls.Image vdImg = null;

            while (vdImg == null && findSource != null)
            {
                if ((vdImg = findSource as System.Windows.Controls.Image) == null)
                {
                    findSource = VisualTreeHelper.GetParent(findSource);
                }
            }

            if (vdImg == null)
                return null;
            else
                return vdImg.Tag as IVdShape;
        }

        public static bool IsPaletteHit(VdDocument doc, Point pos)
        {
            HitTestResult htr = VisualTreeHelper.HitTest(doc.Scene, pos);
            if (htr == null)
                return false;
            Ellipse paletteEllipse = htr.VisualHit as Ellipse;
            return paletteEllipse != null;
        }
    }
}
