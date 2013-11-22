using System.Windows;
using System.Windows.Controls;
using Discussions.d_editor;

namespace DistributedEditor
{
    public enum CaptionType
    {
        Text,
        FreeDraw
    }

    public class ShapeCaptionsManager
    {
        private ICaptionHost _hostShape = null;

        public ICaptionHost HostShape
        {
            get { return _hostShape; }
            set { _hostShape = value; }
        }

        private VdFreeForm _freeDraw = null;

        public VdFreeForm FreeDraw
        {
            get { return _freeDraw; }
            set { _freeDraw = value; }
        }

        //origin relative to org of cluster 
        public double freeDrawX;
        public double freeDrawY;

        public VdText text = null;
        //origin relative to org of cluster 
        public double textX;
        public double textY;

        public ClusterButton btnDraw;
        public ClusterButton btnType;

        public delegate void CaptionCreationRequested(CaptionType type, ICaptionHost hostShape);

        private CaptionCreationRequested _captionCreationRequested;

        public ShapeCaptionsManager(ICaptionHost host, CaptionCreationRequested captionCreationRequested)
        {
            _hostShape = host;

            _captionCreationRequested = captionCreationRequested;

            btnDraw = new ClusterButton();
            btnDraw.btn.Click += __bntDraw;
            btnDraw.SetBrush((System.Windows.Media.Brush) Application.Current.TryFindResource("editBrush"));

            btnType = new ClusterButton();
            btnType.btn.Click += __bntType;
            btnType.SetBrush((System.Windows.Media.Brush) Application.Current.TryFindResource("typeBrush"));
        }

        private void __bntDraw(object sender, RoutedEventArgs e)
        {
            _captionCreationRequested(CaptionType.FreeDraw, _hostShape);
        }

        private void __bntType(object sender, RoutedEventArgs e)
        {
            _captionCreationRequested(CaptionType.Text, _hostShape);
        }

        //caption was removed
        public void InvalidateCaption(IVdShape caption)
        {
            if (caption == text)
                text = null;
            else if (caption == _freeDraw)
                _freeDraw = null;
        }

        public int GetTextId()
        {
            if (text == null)
                return -1;
            else
                return text.Id();
        }

        public int GetFreeDrawId()
        {
            if (_freeDraw == null)
                return -1;
            else
                return _freeDraw.Id();
        }

        public void SetBounds()
        {
            var capOrg = _hostShape.capOrgProvider();
            if (text != null)
            {
                text.SetPosForCluster(capOrg.X + textX,
                                      capOrg.Y + textY);
            }

            if (_freeDraw != null)
            {
                _freeDraw.SetPosForCluster(capOrg.X + freeDrawX,
                                           capOrg.Y + freeDrawY);
            }

            var btnOrg = HostShape.btnOrgProvider();
            //btn draw 
            Canvas.SetLeft(btnDraw, btnOrg.X);
            Canvas.SetTop(btnDraw, btnOrg.Y);

            //btn type
            Canvas.SetLeft(btnType, btnOrg.X + 50);
            Canvas.SetTop(btnType, btnOrg.Y);
        }

        //caption can be moved independently from cluster. when cluster begins movements,
        //we save relative positions of caption to update them accordingly
        public void UpdateRelatives()
        {
            var capOrg = _hostShape.capOrgProvider();

            if (text != null)
            {
                var textOrg = text.GetOrigin();
                textX = textOrg.X - capOrg.X;
                textY = textOrg.Y - capOrg.Y;
            }

            if (_freeDraw != null)
            {
                var freeDrawOrg = _freeDraw.GetOrigin();
                freeDrawX = freeDrawOrg.X - capOrg.X;
                freeDrawY = freeDrawOrg.Y - capOrg.Y;
            }
        }

        public void InitialResizeOfFreeForm()
        {
            if (_freeDraw != null)
            {
                var capOrg = _hostShape.capOrgProvider();
                var fdWidth = 200;
                var fdHeight = 100;
                _freeDraw.SetWH(fdWidth, fdHeight);

                var fdX = capOrg.X - fdWidth*0.5;
                var fdY = capOrg.Y - fdHeight*0.5;
                _freeDraw.SetPosForCluster(fdX, fdY);
            }
        }
    }
}