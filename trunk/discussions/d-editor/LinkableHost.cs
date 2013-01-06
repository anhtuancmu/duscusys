using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedEditor
{
    internal interface LinkableHost
    {
        ClientLinkable GetLinkable();
    }
}