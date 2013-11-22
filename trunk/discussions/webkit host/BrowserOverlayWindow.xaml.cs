using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
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
                WinAPI.SetHitTestVisible(this, visible: true);
            else
                WinAPI.SetHitTestVisible(this, visible: false);
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

        private void BrowserOverlayWindow_OnSourceInitialized(object sender, EventArgs e)
        {
            if(ExplanationModeMediator.Inst.LasersEnabled)
                WinAPI.SetHitTestVisible(this, visible:true);
            else
                WinAPI.SetHitTestVisible(this, visible: false);
        }
    }

}
