using System.ComponentModel;
using System.Windows;
using Discussions.RTModel.Model;
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

        private LaserPointerWndCtx _laserPointerWndCtx;

        public void ToggleLaserPointer()
        {
            if (_laserPointerWndCtx == null)
                _laserPointerWndCtx = new LaserPointerWndCtx(laserScene,
                    ExplanationModeMediator.Inst.CurrentTopicId != null ?
                    ExplanationModeMediator.Inst.CurrentTopicId.Value : -1,
                    LaserPointerTargetSurface.WebBrowser
                    );

            _laserPointerWndCtx.LocalLazerEnabled = ExplanationModeMediator.Inst.LaserPointersEnabled;
        }

        private void BrowserOverlayWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (_laserPointerWndCtx != null)
            {
                _laserPointerWndCtx.Dispose();
                _laserPointerWndCtx = null;
            }
        }
    }

}
