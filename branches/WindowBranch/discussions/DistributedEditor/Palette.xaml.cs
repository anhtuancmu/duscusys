using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DistributedEditor
{
    /// <summary>
    /// Interaction logic for ToolPanel.xaml
    /// </summary>
    public partial class Palette : UserControl, IPaletteOwner
    {
        public VdShapeType shapeType = VdShapeType.None;
        public int _ownerId;
        public int GetOwnerId()
        {
            return _ownerId;
        }

        public delegate void ToolSelected(VdShapeType shape, int owner);       
        public ToolSelected toolSelected;
        
        public delegate void RemoveShape(int owner);
        public RemoveShape removeShape;

        public delegate void NoTool(int owner);
        public NoTool noTool;

        public delegate void Reset(int owner);
        public Reset reset;

        Point currPoint;

        bool isManipulated = false;
        public bool IsManipulated
        {
            get
            {
                return isManipulated;
            }
        }

        public Palette()
        {
            InitializeComponent();

            // for hit tester
            outerEllipse.Tag = this;

            ResetOvers();
        }

        //point is relative to scene
        //public bool IsPressed(Point pt)
        //{
        //    if (pt.X < Canvas.GetLeft(outerEllipse))
        //        return false;

        //    if (pt.X > Canvas.GetRight(outerEllipse))
        //        return false;

        //    if (pt.Y < Canvas.GetTop(outerEllipse))
        //        return false;

        //    if (pt.Y > Canvas.GetBottom(outerEllipse))
        //        return false;

        //    return true;
        //}

        public void StartManip(Point p, TouchDevice td)
        {
            currPoint = p;
            isManipulated = true;

            this.CaptureMouse();
            if (td!=null)
                this.CaptureTouch(td);
        }

        public void ApplyPoint(Point p)
        {
            var dx = p.X - currPoint.X;
            var dy = p.Y - currPoint.Y;

            currPoint = p;

            Canvas.SetLeft(this, Canvas.GetLeft(this) + dx);
            Canvas.SetTop(this, Canvas.GetTop(this) + dy);
        }

        public void StopManip()
        {
            isManipulated = false;
            this.ReleaseAllTouchCaptures();
            this.ReleaseMouseCapture();            
        }
        
        public void ResetOvers()
        {
            lineOver.Visibility = Visibility.Hidden;
            clusterOver.Visibility = Visibility.Hidden;
            arrowOver.Visibility = Visibility.Hidden;
            textOver.Visibility = Visibility.Hidden;
            linkOver.Visibility = Visibility.Hidden;
            freeFormOver.Visibility = Visibility.Hidden;
        }

        private void btnSegment_Click(object sender, RoutedEventArgs e)
        {
            shapeType = VdShapeType.Segment;
            if (toolSelected != null)
                toolSelected(VdShapeType.Segment, _ownerId);

            ResetOvers();
            lineOver.Visibility = Visibility.Visible;
        }

        private void btnArrow_Click(object sender, RoutedEventArgs e)
        {
            shapeType = VdShapeType.Arrow;
            if (toolSelected != null)
                toolSelected(VdShapeType.Arrow, _ownerId);

            ResetOvers();
            arrowOver.Visibility = Visibility.Visible;
        }

        public void SelectText()
        {
            shapeType = VdShapeType.Text;
            if (toolSelected != null)
                toolSelected(VdShapeType.Text, _ownerId);

            ResetOvers();
            textOver.Visibility = Visibility.Visible;
        }

        private void btnText_Click(object sender, RoutedEventArgs e)
        {
            SelectText();
        }

        private void btnFreeForm_Click(object sender, RoutedEventArgs e)
        {
            shapeType = VdShapeType.FreeForm;
            if (toolSelected != null)
                toolSelected(VdShapeType.FreeForm, _ownerId);

            ResetOvers();
            freeFormOver.Visibility = Visibility.Visible;
        }

        private void btnCluster_Click(object sender, RoutedEventArgs e)
        {
            shapeType = VdShapeType.Cluster;
            if (toolSelected != null)
                toolSelected(VdShapeType.Cluster, _ownerId);

            ResetOvers();
            clusterOver.Visibility = Visibility.Visible;
        }

        private void btnClusterLink_Click(object sender, RoutedEventArgs e)
        {
            shapeType = VdShapeType.ClusterLink;
            if (toolSelected != null)
                toolSelected(VdShapeType.ClusterLink, _ownerId);

            ResetOvers();
            linkOver.Visibility = Visibility.Visible;
        }
        
        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (removeShape != null)
                removeShape(_ownerId);

            ResetOvers();            
        }

        private void btnNOP_Click(object sender, RoutedEventArgs e)
        {
            shapeType = VdShapeType.None;
            if (noTool != null)
                noTool(_ownerId);
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (reset != null)
                reset(_ownerId);

            ResetOvers();      
        }
    }
}
