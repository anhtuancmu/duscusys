using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedEditor
{
    public enum ShapeInputMode
    {
        CreationExpected,
        LinkedObj1Expected,
        LinkedObj2Expected,
        FreeDrawing,
        ManipulationExpected,
        CursorApprovalExpected,
        Manipulating
    };
}