using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Controls.Primitives;

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

        private Action _finishDrawing;

        public void Init(Action finishDrawing, DistributedInkCanvas inkCanvas)
        {
            _finishDrawing = finishDrawing;
            this.InkCanvas = inkCanvas;
        }

        public DistributedInkCanvas InkCanvas { get; set; }

        private void penSize_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rad = sender as RadioButton;
            var inkDA = new DrawingAttributes();
            this.InkCanvas.UsesTouchShape = false;
            inkDA.Width = rad.FontSize;
            inkDA.Height = rad.FontSize;
            inkDA.Color = this.InkCanvas.DefaultDrawingAttributes.Color;
            inkDA.IsHighlighter = this.InkCanvas.DefaultDrawingAttributes.IsHighlighter;
            this.InkCanvas.DefaultDrawingAttributes = inkDA;
        }

        private void rad_Click(object sender, RoutedEventArgs e)
        {
            var rad = sender as SurfaceToggleButton;
            this.InkCanvas.EditingMode = (SurfaceInkEditingMode) rad.Tag;

            if (rad == radInk)
                radErase.IsChecked = false;
            else
                radInk.IsChecked = false;
        }

        public void OnDrawingStarted()
        {
            b_8.IsChecked = true;
        }

        private void btnFinishDrawing_Click_1(object sender, RoutedEventArgs e)
        {
            _finishDrawing();
        }
    }
}