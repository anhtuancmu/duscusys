using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using Discussions.DbModel;
using System.Windows.Input;
using System.IO;
using System.Reflection;

namespace Discussions
{
    public class Utils2
    {
        public static string VersionString()
        {
            return "v." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public static ScatterViewItem WrapWithScatterItem(UserControl uc)
        {
            return WrapWithScatterItem(uc, 300, 180);
        }

        public static ScatterViewItem WrapWithScatterItem(UserControl uc, int w, int h)
        {
            ScatterViewItem result = new ScatterViewItem();
            result.Content = uc;
            result.Width = w;
            result.Height = h;
            return result;
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T) child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T) child;
                    break;
                }
            }

            return foundChild;
        }

        public static T FindChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!(child is T))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement is T)
                    {
                        // if the child's name is of the request name
                        foundChild = (T) child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T) child;
                    break;
                }
            }

            return foundChild;
        }

        public static ScatterViewItem findSVIUnderTouch(InputEventArgs e)
        {
            DependencyObject findSource = e.OriginalSource as FrameworkElement;
            ScatterViewItem draggedElement = null;

            // Find the ScatterViewItem object that is being touched.
            while (draggedElement == null && findSource != null)
            {
                if ((draggedElement = findSource as ScatterViewItem) == null)
                {
                    findSource = VisualTreeHelper.GetParent(findSource);
                }
            }

            return draggedElement;
        }

        public static SurfaceListBoxItem findLBIUnderTouch(InputEventArgs e)
        {
            DependencyObject findSource = e.OriginalSource as FrameworkElement;
            SurfaceListBoxItem draggedElement = null;

            // Find the ScatterViewItem object that is being touched.
            while (draggedElement == null && findSource != null)
            {
                if ((draggedElement = findSource as SurfaceListBoxItem) == null)
                {
                    findSource = VisualTreeHelper.GetParent(findSource);
                }
            }

            return draggedElement;
        }

        private static DependencyObject GetScatterViewCanvas(ScatterView sv)
        {
            if (sv.Items.Count == 0)
                return null;

            //http://msdn.microsoft.com/en-us/library/ee804791%28v=surface.10%29.aspx for the hierarchy            
            Border b = VisualTreeHelper.GetChild(sv, 0) as Border;
            ItemsPresenter p = VisualTreeHelper.GetChild(b, 0) as ItemsPresenter;
            DependencyObject scatterCanvas = VisualTreeHelper.GetChild(p, 0) as DependencyObject; //ScatterCanvas 
            return scatterCanvas;
        }

        public delegate void VisitSVI(ScatterViewItem svi);

        public static void EnumSVIs(ScatterView sv, VisitSVI handler)
        {
            DependencyObject canv = GetScatterViewCanvas(sv);
            if (canv == null)
                return;

            int nChildren = VisualTreeHelper.GetChildrenCount(canv);
            for (int i = 0; i < nChildren; ++i)
            {
                DependencyObject child = VisualTreeHelper.GetChild(canv, i);
                var svi = child as ScatterViewItem;
                if (svi != null)
                    handler(svi);
            }
        }

        public static System.Windows.Media.Color IntToColor(int iCol)
        {
            return Color.FromArgb((byte) (iCol >> 24),
                                  (byte) (iCol >> 16),
                                  (byte) (iCol >> 8),
                                  (byte) (iCol));
        }

        public static int ColorToInt(System.Windows.Media.Color c)
        {
            int iCol = (c.A << 24) | (c.R << 16) | (c.G << 8) | c.B;
            return iCol;
        }

        public static string ScreenshotPathName()
        {
            string DiscDir = Path.Combine(TempDir(), "screenshots");
            if (!Directory.Exists(DiscDir))
                Directory.CreateDirectory(DiscDir);

            return Path.Combine(DiscDir, Guid.NewGuid().ToString() + ".png");
        }

        public static string ExeDir()
        {
            return System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        }

        public static string TempDir()
        {
            string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "discusys");
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
            return tempPath;
        }

        public static string ReportsDir()
        {
            string reportsDir = System.IO.Path.Combine(TempDir(), "reports");
            if (!Directory.Exists(reportsDir))
                Directory.CreateDirectory(reportsDir);
            return reportsDir;
        }

        public static string ValidateFileName(string inp)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                inp = inp.Replace(c, ' ');
            }
            return inp;
        }

        public static string RandomFilePath(string extension)
        {
            return Path.Combine(TempDir(), Guid.NewGuid().ToString() + extension);
        }
    }
}