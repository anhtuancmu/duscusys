using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace DistributedEditor
{
    public enum CaptionType
    {
        Text,
        FreeDraw
    }
    
    public class ClusterCaptions
    {
        public VdCluster cluster;
        
        VdFreeForm _freeDraw = null;
        public VdFreeForm FreeDraw
        {
            get
            {
                return _freeDraw;
            }
            set
            {
                _freeDraw = value;
            }
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

        public delegate void CaptionCreationRequested(CaptionType type, VdCluster cluster);
        CaptionCreationRequested _captionCreationRequested;

        public ClusterCaptions(VdCluster cluster, CaptionCreationRequested captionCreationRequested)
        {
            this.cluster = cluster;

            _captionCreationRequested = captionCreationRequested;

            btnDraw = new ClusterButton();
            btnDraw.btn.Click += __bntDraw;
            btnDraw.SetBrush((System.Windows.Media.Brush)Application.Current.TryFindResource("editBrush"));

            btnType = new ClusterButton();
            btnType.btn.Click += __bntType;
            btnType.SetBrush((System.Windows.Media.Brush)Application.Current.TryFindResource("typeBrush"));
        }

        void __bntDraw(object sender, RoutedEventArgs e)
        {
            _captionCreationRequested(CaptionType.FreeDraw, cluster);
        }

        void __bntType(object sender, RoutedEventArgs e)
        {
            _captionCreationRequested(CaptionType.Text, cluster);        
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
            var clustOrg = cluster.boundsProvider().TopLeft;
            
            if (text != null)
            {
                text.SetPosForCluster(clustOrg.X + textX,
                                      clustOrg.Y + textY);
            }

            if (_freeDraw != null)
            {
                _freeDraw.SetPosForCluster(clustOrg.X + freeDrawX,
                                          clustOrg.Y + freeDrawY);
            }

            //btn draw 
            Canvas.SetLeft(btnDraw, clustOrg.X + 20);
            Canvas.SetTop(btnDraw,  clustOrg.Y - 20);

            //btn type
            Canvas.SetLeft(btnType, clustOrg.X + 60);
            Canvas.SetTop(btnType, clustOrg.Y  - 20);
        }

        //caption can be moved independently from cluster. when cluster beings movements,
        //we save relative positions of caption to update them accordingly
        public void UpdateRelatives()
        {
            var clustOrg = cluster.boundsProvider().TopLeft;

            if (text != null)
            {
                var textOrg = text.GetOrigin();
                textX = textOrg.X - clustOrg.X;
                textY = textOrg.Y - clustOrg.Y;             
            }

            if (_freeDraw != null)
            {
                var freeDrawOrg = _freeDraw.GetOrigin();
                freeDrawX = freeDrawOrg.X - clustOrg.X;
                freeDrawY = freeDrawOrg.Y - clustOrg.Y;             
            }
        }

        public void InitialResizeOfFreeForm()
        {
            var clustBounds = cluster.boundsProvider();
            if (_freeDraw != null)
            {
                var fdWidth  = clustBounds.Width * 0.6;
                var fdHeight = fdWidth * 0.4;
                _freeDraw.SetWH(fdWidth, fdHeight);

                var fdX = (clustBounds.X + clustBounds.Width / 2) - fdWidth  / 2;
                var fdY = clustBounds.Y - (fdHeight + 20);
                _freeDraw.SetPosForCluster(fdX, fdY);
            }
        }
    }
}
