using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Discussions.RTModel.Model;
using Discussions.webkit_host;
using DistributedEditor;

namespace Discussions.pdf_reader
{
    public partial class ReaderOverlayWindow : Window
    {
        public ReaderOverlayWindow()
        {
            InitializeComponent();
        }

        public ReaderWindow2 Window { get; set; }

        private LaserPointerWndCtx _laserPointerWndCtx;

        public void ToggleLocalLaserPointer()
        {
            if (_laserPointerWndCtx == null)
                _laserPointerWndCtx = new LaserPointerWndCtx(laserScene,
                    ExplanationModeMediator.Inst.CurrentTopicId != null ?
                    ExplanationModeMediator.Inst.CurrentTopicId.Value : -1,
                    LaserPointerTargetSurface.PdfReader
                    );

            _laserPointerWndCtx.LocalLazerEnabled = ExplanationModeMediator.Inst.LasersEnabled;

            if (ExplanationModeMediator.Inst.LasersEnabled)
                WinAPI.SetHitTestVisible(this, visible: true);
            else
                WinAPI.SetHitTestVisible(this, visible: false);
        }

        private void ReaderOverlayWindow_OnSourceInitialized(object sender, EventArgs e)
        {
            if (ExplanationModeMediator.Inst.LasersEnabled)
                WinAPI.SetHitTestVisible(this, visible: true);
            else
                WinAPI.SetHitTestVisible(this, visible: false);
        }

        private void ReaderOverlayWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (_laserPointerWndCtx != null)
            {
                _laserPointerWndCtx.Dispose();
                _laserPointerWndCtx = null;
            }
        }

        private void ReaderOverlayWindow_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
        }
    }
}
