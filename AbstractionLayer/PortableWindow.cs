using System.Windows;
using Microsoft.Surface.Presentation.Controls;

namespace AbstractionLayer
{
    public class PortableWindow :
#if SURFACE
 SurfaceWindow
#else
 Window
#endif
    {
        public bool IsSurfaceWindow
        {
            get
            {
#if SURFACE
                return true;
#else
                return false;
#endif
            }
        }

        string _title2;
        public string Title2
        {
            get
            {
                return _title2;
            }
            set
            {
                if (IsSurfaceWindow)
                    _title2 = value + " (Surface)";
                else
                    _title2 = value + " (Window)";

                Title = _title2;
            }
        }
    }
}
