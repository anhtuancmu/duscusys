using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace DistributedEditor
{
    internal struct CursorApprovalData
    {
        //recently clicked object, awaits cursor approval
        public Shape resizeNode;

        //recent contact point, awaits cursor approval
        public Point pos;

        public TouchDevice td;
    }
}