using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface.Presentation.Controls;

namespace DistributedEditor
{
	/// <summary>
	/// Interaction logic for InkPalette.xaml
	/// </summary>
	public partial class InkPalette : UserControl
	{
		public InkPalette()
		{
			this.InitializeComponent();
		}

        Action _finishDrawing;

        public void Init(Action finishDrawing)
        {
            _finishDrawing = finishDrawing;
        }

        public SurfaceInkCanvas InkCanvas
        {
            get;
            set;
        }

        private void penSize_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rad = sender as RadioButton;
            var inkDA = new DrawingAttributes();
            inkDA.Width  = rad.FontSize;
            inkDA.Height = rad.FontSize;
            inkDA.Color = this.InkCanvas.DefaultDrawingAttributes.Color;
            inkDA.IsHighlighter = this.InkCanvas.DefaultDrawingAttributes.IsHighlighter;
            this.InkCanvas.DefaultDrawingAttributes = inkDA;                 
        }

        private void rad_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rad = sender as RadioButton;
            this.InkCanvas.EditingMode = (SurfaceInkEditingMode)rad.Tag;
        }

        private void btnFinishDrawing_Click_1(object sender, RoutedEventArgs e)
        {
            _finishDrawing();
        }
	}
}