using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions.view
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    /// <remarks>
    /// The popup control is used as a child of the main scatterview and will display content in a window like UI.
    /// It's possible for rendered content to request an initial size by setting the InitialSizeRequest attached property (PopupWindow.InitialSizeRequest="www,hhh").
    /// 
    /// Written by: Isak Savo, isak.savo@gmail.com and included in a CodeProject Article
    /// </remarks>
    public partial class PopupWindow : UserControl
    {
        #region Attached Property InitialSizeRequest

        public static Size GetInitialSizeRequest(DependencyObject obj)
        {
            return (Size) obj.GetValue(InitialSizeRequestProperty);
        }

        public static void SetInitialSizeRequest(DependencyObject obj, Size value)
        {
            obj.SetValue(InitialSizeRequestProperty, value);
        }

        // Using a DependencyProperty as the backing store for InitialSizeRequest.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InitialSizeRequestProperty =
            DependencyProperty.RegisterAttached("InitialSizeRequest", typeof (Size), typeof (PopupWindow),
                                                new UIPropertyMetadata(Size.Empty));

        #endregion

        public PopupWindow()
        {
            InitializeComponent();
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                c_contentHolder.Loaded += new RoutedEventHandler(c_contentHolder_Loaded);
        }

        public PopupWindow(object content)
        {
            InitializeComponent();
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                c_contentHolder.Loaded += new RoutedEventHandler(c_contentHolder_Loaded);
            c_contentHolder.Content = content;
        }

        private static Size DefaultPopupSize = new Size(300, 200);

        /// <summary>
        /// Gets the size that the parent container should have to fully accomodate the PopupWindow and its child content
        /// based on the child's InitialSizeRequest.
        /// </summary>
        /// <returns>The size, which should be set to the parent container</returns>
        private Size CalculateScatterViewItemSize()
        {
            var presenter = GuiHelpers.GetChildObject<ContentPresenter>(c_contentHolder);
            if (presenter == null)
                return DefaultPopupSize;
            // It seems it's safe to assume the ContentPresenter will always only have one child and that child is the visual representation
            // of the content of c_contentHolder.
            var child = VisualTreeHelper.GetChild(presenter, 0);
            if (child == null)
                return DefaultPopupSize;
            var requestedSize = PopupWindow.GetInitialSizeRequest(child);
            if (!requestedSize.IsEmpty
                && requestedSize.Width != 0
                && requestedSize.Height != 0)
            {
                var borderHeight = this.ActualHeight - c_contentHolder.ActualHeight;
                var borderWidth = this.ActualWidth - c_contentHolder.ActualWidth;
                return new Size(requestedSize.Width + borderWidth, requestedSize.Height + borderHeight);
            }
            else
                // No requested size set, or the requested size was invalid
                return DefaultPopupSize;
        }

        private void c_contentHolder_Loaded(object sender, RoutedEventArgs e)
        {
            // By now we've got the visual tree for the content of the popup, check to see if it has a request for the size of the popup
            var newSize = CalculateScatterViewItemSize();

            AnimateEntry(newSize);
        }

        private void AnimateEntry(Size targetSize)
        {
            var svi = GuiHelpers.GetParentObject<ScatterViewItem>(this, false);
            if (svi != null)
            {
                // Easing function provide a more natural animation
                IEasingFunction ease = new BackEase {EasingMode = EasingMode.EaseOut, Amplitude = 0.3};
                var duration = new Duration(TimeSpan.FromMilliseconds(500));
                var w = new DoubleAnimation(0.0, targetSize.Width, duration) {EasingFunction = ease};
                var h = new DoubleAnimation(0.0, targetSize.Height, duration) {EasingFunction = ease};
                var o = new DoubleAnimation(0.0, 1.0, duration);

                // Remove the animation after it has completed so that its possible to manually resize the scatterviewitem
                w.Completed += (s, e) => svi.BeginAnimation(ScatterViewItem.WidthProperty, null);
                h.Completed += (s, e) => svi.BeginAnimation(ScatterViewItem.HeightProperty, null);
                // Set the size manually, otherwise once the animation is removed the size will revert back to the minimum size
                // Since animation has higher priority for DP's, this setting won't have effect until the animation is removed
                svi.Width = targetSize.Width;
                svi.Height = targetSize.Height;

                svi.BeginAnimation(ScatterViewItem.WidthProperty, w);
                svi.BeginAnimation(ScatterViewItem.HeightProperty, h);
                svi.BeginAnimation(ScatterViewItem.OpacityProperty, o);
            }
        }

        private void AnimateExit()
        {
            var svi = GuiHelpers.GetParentObject<ScatterViewItem>(this, false);
            if (svi != null)
            {
                IEasingFunction ease = new BackEase {EasingMode = EasingMode.EaseOut, Amplitude = 0.3};
                var duration = new Duration(TimeSpan.FromMilliseconds(500));
                var w = new DoubleAnimation(0.0, duration) {EasingFunction = ease};
                var h = new DoubleAnimation(0.0, duration) {EasingFunction = ease};
                var o = new DoubleAnimation(0.0, duration);
                svi.BeginAnimation(ScatterViewItem.WidthProperty, w);
                svi.BeginAnimation(ScatterViewItem.HeightProperty, h);
                svi.BeginAnimation(ScatterViewItem.OpacityProperty, o);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            var sv = GuiHelpers.GetParentObject<ScatterView>(this);
            if (sv != null)
                sv.Items.Remove(this);
        }
    }
}