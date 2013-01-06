using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions
{
    public static class ListBoxExtensions
    {
        /// <summary>
        /// Causes the object to scroll into view centered.
        /// </summary>
        /// <param name="listBox">ListBox instance.</param>
        /// <param name="item">Object to scroll.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Deliberately targeting ListBox.")]
        public static void ScrollIntoViewCentered(this SurfaceListBox listBox, object item)
        {
            Debug.Assert(!VirtualizingStackPanel.GetIsVirtualizing(listBox),
                         "VirtualizingStackPanel.IsVirtualizing must be disabled for ScrollIntoViewCentered to work.");
            Debug.Assert(!ScrollViewer.GetCanContentScroll(listBox),
                         "ScrollViewer.GetCanContentScroll must be disabled for ScrollIntoViewCentered to work.");

            // Get the container for the specified item
            var container = listBox.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
            if (null != container)
            {
                // Get the bounds of the item container
                var rect = new Rect(new Point(), container.RenderSize);

                // Find constraining parent (either the nearest ScrollContentPresenter or the ListBox itself)
                FrameworkElement constrainingParent = container;
                do
                {
                    constrainingParent = VisualTreeHelper.GetParent(constrainingParent) as FrameworkElement;
                } while ((null != constrainingParent) &&
                         (listBox != constrainingParent) &&
                         !(constrainingParent is ScrollContentPresenter));

                if (null != constrainingParent)
                {
                    // Inflate rect to fill the constraining parent
                    rect.Inflate(
                        Math.Max((constrainingParent.ActualWidth - rect.Width)/2, 0),
                        Math.Max((constrainingParent.ActualHeight - rect.Height)/2, 0));
                }

                // Bring the (inflated) bounds into view
                container.BringIntoView(rect);
            }
        }
    }
}