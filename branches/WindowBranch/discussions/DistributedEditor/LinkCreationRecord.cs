using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedEditor
{
    struct LinkCreationRecord
    {
        public ClientLinkable end1;
        public ClientLinkable end2;
        public LinkHeadType linkHead;
    }
}
