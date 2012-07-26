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

        public delegate void ToolSelected(VdShapeType shape, int shapeTag, int owner);       
        public ToolSelected toolSelected;
        
        public delegate void RemoveShape(int owner);
        public RemoveShape removeShape;

        public delegate void NoTool(int owner);
        public NoTool noTool;

        public delegate void Reset(int owner);
        public Reset reset;

        public Palette()
        {
            InitializeComponent();

            ResetOvers();
        }

        public void ResetOvers()
        {
            //btnSegment.IsChecked = false;
            btnCluster.IsChecked = false;
            //btnArrow.IsChecked   = false;
            btnText.IsChecked    = false;
            btnClusterLink.IsChecked = false;
            btnClusterLink2.IsChecked = false;
            btnFreeForm.IsChecked = false;                                                            
        }

        private void btnSegment_Click(object sender, RoutedEventArgs e)
        {
            shapeType = VdShapeType.Segment;
            if (toolSelected != null)
                toolSelected(VdShapeType.Segment, -1, _ownerId);

            //ResetOvers();            
        }

        private void btnArrow_Click(object sender, RoutedEventArgs e)
        {
            shapeType = VdShapeType.Arrow;
            if (toolSelected != null)
                toolSelected(VdShapeType.Arrow, -1, _ownerId);

            //ResetOvers();            
        }

        public void SelectText()
        {
            shapeType = VdShapeType.Text;
            if (toolSelected != null)
                toolSelected(VdShapeType.Text, -1, _ownerId);

            //ResetOvers();            
        }

        private void btnText_Click(object sender, RoutedEventArgs e)
        {
            SelectText();
        }

        private void btnFreeForm_Click(object sender, RoutedEventArgs e)
        {
            shapeType = VdShapeType.FreeForm;
            if (toolSelected != null)
                toolSelected(VdShapeType.FreeForm, -1, _ownerId);

            //ResetOvers();           
        }

        private void btnCluster_Click(object sender, RoutedEventArgs e)
        {
            shapeType = VdShapeType.Cluster;
            if (toolSelected != null)
                toolSelected(VdShapeType.Cluster, -1, _ownerId);

            //ResetOvers();            
        }

        private void btnClusterLink_Click(object sender, RoutedEventArgs e)
        {
            shapeType = VdShapeType.ClusterLink;
            if (toolSelected != null)
                toolSelected(VdShapeType.ClusterLink, (int)LinkHeadType.SingleHeaded, _ownerId);

            //ResetOvers();            
        }

        private void btnClusterLink2_Click(object sender, RoutedEventArgs e)
        {
            shapeType = VdShapeType.ClusterLink;
            if (toolSelected != null)
                toolSelected(VdShapeType.ClusterLink, (int)LinkHeadType.DoubleHeaded, _ownerId);

            //ResetOvers();            
        }
        
        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (removeShape != null)
                removeShape(_ownerId);

            //ResetOvers();            
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

            //ResetOvers();      
        }
    }
}
