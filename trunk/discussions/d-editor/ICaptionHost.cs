using System.Windows;
using DistributedEditor;

namespace Discussions.d_editor
{
    public interface ICaptionHost : IVdShape
    {
        Rect boundsProvider();
        Point capOrgProvider();
        Point btnOrgProvider();
        void InitCaptions(ShapeCaptionsManager.CaptionCreationRequested captionCreationRequested);
        ShapeCaptionsManager CapMgr();
    }
}