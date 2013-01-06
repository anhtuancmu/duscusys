using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using Microsoft.Expression.BlendSDK;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions
{
    public class VerticallyUnscrollableInnerList : Behavior<UIElement>
    {
        private SurfaceScrollViewer _scroller;
        private SurfaceListBox _media;

        public VerticallyUnscrollableInnerList(SurfaceScrollViewer scroller, SurfaceListBox media)
        {
            _scroller = scroller;
            _media = media;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseDown += PreviewMouseDown;
            AssociatedObject.PreviewMouseUp += PreviewMouseUp;
            AssociatedObject.PreviewMouseMove += PreviewMouseMove;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseDown -= PreviewMouseDown;
            AssociatedObject.PreviewMouseUp -= PreviewMouseUp;
            AssociatedObject.PreviewMouseMove -= PreviewMouseMove;
            base.OnDetaching();
        }


        // bool dragging = false;
        private void PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //   e.Handled = true;

            //var e2 = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left);
            //e2.RoutedEvent = UIElement.MouseDownEvent;
            //e2.Source = sender;
            //_scroller.RaiseEvent(e2);

            //  _media.RaiseEvent(e);
            //
            ///     
        }

        private void PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // e.Handled = true;
            ///dragging = false;

            var e2 = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left);
            e2.RoutedEvent = UIElement.MouseUpEvent;
            e2.Source = sender;
            _scroller.RaiseEvent(e2);

            //  _media.RaiseEvent(e);
            //
        }

        private void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // e.Handled = true;

            var e3 = new MouseEventArgs(e.MouseDevice, e.Timestamp, e.StylusDevice);
            e3.RoutedEvent = UIElement.MouseMoveEvent;
            e3.Source = sender;
            _scroller.RaiseEvent(e3);

            _media.RaiseEvent(e);

            //
        }
    }
}