using System.Windows;
using System.Windows.Media;
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

            if (vdText == null)
                return null;
            else
            {
                var res = vdText.Tag as IVdShape;
                return res.IsVisible() ? res : null;
            }
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
            {
                var res = vdCluster.Tag as IVdShape;
                if (res == null)
                    return null;
                return res.IsVisible() ? res : null;
            }
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
            {
                var res = vdBadge.Tag as IVdShape;
                return res.IsVisible() ? res : null;
            }
        }

        public static IVdShape findVdImg(object originalSrc)
        {
            DependencyObject findSource = originalSrc as FrameworkElement;
            System.Windows.Controls.Border vdImg = null;

            while (vdImg == null && findSource != null)
            {
                if ((vdImg = findSource as System.Windows.Controls.Border) == null)
                {
                    findSource = VisualTreeHelper.GetParent(findSource);
                }
            }

            if (vdImg == null)
                return null;
            else
            {
                var res = vdImg.Tag as IVdShape;
                if (res == null)
                    return null;
                else
                    return res.IsVisible() ? res : null;
            }
        }
    }
}