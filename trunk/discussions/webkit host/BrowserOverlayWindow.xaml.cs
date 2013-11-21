using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Discussions.RTModel.Model;
using Discussions.view;
using DistributedEditor;

namespace Discussions.webkit_host
{
    /// <summary>
    /// Interaction logic for BrowserOverlayWindow.xaml
    /// </summary>
    public partial class BrowserOverlayWindow : Window
    {
        public BrowserOverlayWindow()
        {
            InitializeComponent();
        }

        public WebkitBrowserWindow Window { get; set;}

        private LaserPointerWndCtx _laserPointerWndCtx;

        public void ToggleLocalLaserPointer()
        {
            if (_laserPointerWndCtx == null)
                _laserPointerWndCtx = new LaserPointerWndCtx(laserScene,
                    ExplanationModeMediator.Inst.CurrentTopicId != null ?
                    ExplanationModeMediator.Inst.CurrentTopicId.Value : -1,
                    LaserPointerTargetSurface.WebBrowser
                    );

            _laserPointerWndCtx.LocalLazerEnabled = ExplanationModeMediator.Inst.LasersEnabled;

            if (ExplanationModeMediator.Inst.LasersEnabled)
                SetHitTestVisible(visible: true);
            else
                SetHitTestVisible(visible: false);
        }

        private void BrowserOverlayWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (_laserPointerWndCtx != null)
            {
                _laserPointerWndCtx.Dispose();
                _laserPointerWndCtx = null;
            }
        }

        private void BrowserOverlayWindow_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Debug.WriteLine("BrowserOverlayWindow_OnMouseWheel");
            if (Window != null)
                Window.ScrollBrowser(e.Delta);
        }


        public void SetHitTestVisible(bool visible)
        {
            // Get this window's handle
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            // Change the extended window style to include WS_EX_TRANSPARENT
            int extendedStyle = WinAPI.GetWindowLong(hwnd, WinAPI.GWL_EXSTYLE);

            if (visible)
                WinAPI.SetWindowLong(hwnd, WinAPI.GWL_EXSTYLE, extendedStyle & ~WinAPI.WS_EX_TRANSPARENT);
            else
                WinAPI.SetWindowLong(hwnd, WinAPI.GWL_EXSTYLE, extendedStyle | WinAPI.WS_EX_TRANSPARENT);
        }

        private void BrowserOverlayWindow_OnSourceInitialized(object sender, EventArgs e)
        {
            if(ExplanationModeMediator.Inst.LasersEnabled)
                SetHitTestVisible(visible:true);
            else
                SetHitTestVisible(visible: false);
        }
    }

}
