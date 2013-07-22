using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input.StylusPlugIns;
using Discussions.rt;
using Microsoft.Surface.Presentation.Controls;

namespace DistributedEditor
{
    public class DistributedInkCanvas : SurfaceInkCanvas
    {
        InkInterceptorPlugin interceptor = new InkInterceptorPlugin();

        public Action OnInkChanged;

        public DistributedInkCanvas()
            : base()
        {
            //this.StylusPlugIns.Add(interceptor);

            //this.StrokeCollected += strokeCollected;
            //this.StrokeErased += strokeErased;
        }

        void strokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            if (OnInkChanged != null)
                OnInkChanged();           
        }

        void strokeErased(object sender, RoutedEventArgs e)
        {
            if (OnInkChanged != null)
                OnInkChanged();
        }
    }
}
