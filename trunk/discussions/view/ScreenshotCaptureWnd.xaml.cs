using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AbstractionLayer;

namespace Discussions.view
{
    public partial class ScreenshotCaptureWnd : PortableWindow
    {
        private System.Windows.Point _topLeft;

        private enum CaptureState
        {
            SelectingWindow,
            SelectedWindow,
            SelectingCaptureArea
        }

        private CaptureState _state = CaptureState.SelectingWindow;

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

           // ShowPreMessage();
        }

        static void ShowPreMessage()
        {
            MessageDlg.Show("Select windows screen to capture screenshot");
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

        private readonly Action<Bitmap> _onCaptured = null;

        private void PointDown(System.Windows.Point pointDown)
        {
            switch (_state)
            {
                case CaptureState.SelectingWindow:
                    break;
                case CaptureState.SelectedWindow:
                    startDrawing(pointDown);
                    _state = CaptureState.SelectingCaptureArea;
                    break;
                case CaptureState.SelectingCaptureArea:
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void PointUp()
        {
            switch (_state)
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
        private void Window_TouchDown_1(object sender, TouchEventArgs e)
        {
            PointDown(e.GetTouchPoint(canv).Position);
        }

        private void Window_TouchMove_1(object sender, TouchEventArgs e)
        {
            if (_state == CaptureState.SelectingCaptureArea)
                onDrawing(e.GetTouchPoint(canv).Position);
        }

        private void Window_TouchUp_1(object sender, TouchEventArgs e)
        {
            PointUp();
        }


        //mouse 
        private void Window_MouseDown_1(object sender, MouseButtonEventArgs e)
        {
            PointDown(e.GetPosition(canv));
        }

        private void Window_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (_state == CaptureState.SelectingCaptureArea)
                onDrawing(e.GetPosition(canv));
        }

        private void Window_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            PointUp();
        }

        private void btnStartDrawing_Click_1(object sender, RoutedEventArgs e)
        {
            if (_state == CaptureState.SelectingWindow)
            {
                _state = CaptureState.SelectedWindow;
                helpBg.Background = null;
                helpBg.VerticalAlignment = VerticalAlignment.Top;
                lblHelp.Content = "Draw capture area";

                this.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(78, 0, 0, 0));
                MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                this.WindowState = WindowState.Maximized;
              
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

        private void ScreenshotCaptureWnd_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                btnCancel_Click_1(sender, e);
        }
    }
}