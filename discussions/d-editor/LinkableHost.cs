using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedEditor
{
    interface LinkableHost
    {
        ClientLinkable GetLinkable();   
    }
}
