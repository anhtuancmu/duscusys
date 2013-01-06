using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedEditor
{
    public interface IPaletteOwner
    {
        int GetOwnerId();
    }
}