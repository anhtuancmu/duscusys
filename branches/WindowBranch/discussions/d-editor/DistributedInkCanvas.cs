using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Surface.Presentation.Controls;

namespace DistributedEditor
{
    public class DistributedInkCanvas : SurfaceInkCanvas
    {
        private InkInterceptorPlugin _interceptor = new InkInterceptorPlugin();

        public Action OnInkChanged;

        public DistributedInkCanvas()
            : base()
        {
            //this.StylusPlugIns.Add(interceptor);

            //this.StrokeCollected += strokeCollected;
            //this.StrokeErased += strokeErased;
        }

        private void strokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            if (OnInkChanged != null)
                OnInkChanged();
        }

        private void strokeErased(object sender, RoutedEventArgs e)
        {
            if (OnInkChanged != null)
                OnInkChanged();
        }
    }
}