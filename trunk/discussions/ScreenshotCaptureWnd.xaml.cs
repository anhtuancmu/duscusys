using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions
{
    public partial class ScreenshotCaptureWnd : SurfaceWindow
    {
        private System.Windows.Point _topLeft;

        private enum CaptureState
        {
            SelectingWindow,
            SelectedWindow,
            SelectingCaptureArea
        }

        private CaptureState state = CaptureState.SelectingWindow;

        private Bitmap _screenshot;

        public Rect CaptureRect
        {
            get { return captureZone.Rect; }
        }

        public Bitmap GetScreenshot()
        {
            return _screenshot;
        }

        public ScreenshotCaptureWnd(Action<Bitmap> onCaptured)
        {
            InitializeComponent();

            HideOwnWindows();

            _onCaptured = onCaptured;
        }

        private void startDrawing(System.Windows.Point topLeft)
        {
            _topLeft = topLeft;
            captureZone.Rect = new Rect(topLeft.X, topLeft.Y, 1, 1);
            lblHelp.Visibility = Visibility.Hidden;
            updateSizeIndicator();
        }

        private void updateSizeIndicator()
        {
            Canvas.SetLeft(lblSizeIndicator, captureZone.Rect.Right + 10);
            Canvas.SetTop(lblSizeIndicator, captureZone.Rect.Bottom + 10);
            lblSizeIndicator.Content = string.Format("{0}x{1}", (int) captureZone.Rect.Width,
                                                     (int) captureZone.Rect.Height);
        }

        private void onDrawing(System.Windows.Point currentPen)
        {
            captureZone.Rect = new Rect(_topLeft.X, _topLeft.Y,
                                        Math.Abs(currentPen.X - _topLeft.X),
                                        Math.Abs(currentPen.Y - _topLeft.Y));

            updateSizeIndicator();
        }

        public System.Windows.Point ToDevicePixels(UIElement element, System.Windows.Point p)
        {
            Matrix transformToDevice;
            var source = PresentationSource.FromVisual(element);
            transformToDevice = source.CompositionTarget.TransformToDevice;

            return transformToDevice.Transform(p);
        }

        private void stopDrawing()
        {
            var rect = captureZone.Rect;

            this.Hide();

            var topLeftPx = ToDevicePixels(this, new System.Windows.Point(rect.X - 7, rect.Y - 7));
            var heightWidthPx = ToDevicePixels(this, new System.Windows.Point(rect.Width, rect.Height));
            _screenshot = Screenshot.CaptureDesktop((int) topLeftPx.X, (int) topLeftPx.Y,
                                                    (int) heightWidthPx.X, (int) heightWidthPx.Y);

            if (_onCaptured != null)
                _onCaptured(_screenshot);
        }

        private Action<Bitmap> _onCaptured = null;

        private void PointDown(System.Windows.Point pointDown)
        {
            switch (state)
            {
                case CaptureState.SelectingWindow:
                    break;
                case CaptureState.SelectedWindow:
                    startDrawing(pointDown);
                    state = CaptureState.SelectingCaptureArea;
                    break;
                case CaptureState.SelectingCaptureArea:
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void PointUp()
        {
            switch (state)
            {
                case CaptureState.SelectingWindow:
                    break;
                case CaptureState.SelectedWindow:
                    break;
                case CaptureState.SelectingCaptureArea:
                    stopDrawing();
                    ShowOwnWindows();
                    Close();
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void HideOwnWindows()
        {
            DiscWindows.Get().mainWnd.Hide();
            if (DiscWindows.Get().privateDiscBoard != null)
                DiscWindows.Get().privateDiscBoard.Hide();
            if (DiscWindows.Get().moderDashboard != null)
                DiscWindows.Get().moderDashboard.Hide();
            if (DiscWindows.Get().htmlBackgroundWnd != null)
                DiscWindows.Get().htmlBackgroundWnd.Hide();
        }

        private void ShowOwnWindows()
        {
            DiscWindows.Get().mainWnd.Show();
            if (DiscWindows.Get().privateDiscBoard != null)
                DiscWindows.Get().privateDiscBoard.Show();
            if (DiscWindows.Get().moderDashboard != null)
                DiscWindows.Get().moderDashboard.Show();
            if (DiscWindows.Get().htmlBackgroundWnd != null)
                DiscWindows.Get().htmlBackgroundWnd.Show();
        }

        //touch
        private void SurfaceWindow_TouchDown_1(object sender, TouchEventArgs e)
        {
            PointDown(e.GetTouchPoint(canv).Position);
        }

        private void SurfaceWindow_TouchMove_1(object sender, TouchEventArgs e)
        {
            if (state == CaptureState.SelectingCaptureArea)
                onDrawing(e.GetTouchPoint(canv).Position);
        }

        private void SurfaceWindow_TouchUp_1(object sender, TouchEventArgs e)
        {
            PointUp();
        }


        //mouse 
        private void SurfaceWindow_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            PointDown(e.GetPosition(canv));
        }

        private void SurfaceWindow_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (state == CaptureState.SelectingCaptureArea)
                onDrawing(e.GetPosition(canv));
        }

        private void SurfaceWindow_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            PointUp();
        }

        private void btnStartDrawing_Click_1(object sender, RoutedEventArgs e)
        {
            if (state == CaptureState.SelectingWindow)
            {
                state = CaptureState.SelectedWindow;
                this.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x5D, 0x5D, 0x5D));
                MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                this.WindowState = WindowState.Maximized;

                this.Opacity = 0.6;

                lblHelp.Visibility = Visibility.Visible;
                btnStartDrawing.Visibility = Visibility.Hidden;
                btnCancel.Visibility = Visibility.Collapsed;

                this.Hide();
                this.Show();
                this.Topmost = true;
            }
        }

        private void btnCancel_Click_1(object sender, RoutedEventArgs e)
        {
            ShowOwnWindows();
            Close();
        }
    }
}