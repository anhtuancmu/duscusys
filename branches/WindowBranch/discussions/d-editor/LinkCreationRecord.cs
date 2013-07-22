using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedEditor
{
    internal struct LinkCreationRecord
    {
        public ClientLinkable end1;
        public ClientLinkable end2;
        public LinkHeadType headType;

        //when we sent begin create link request and expect link play event, we keep id of the link in this field 
        public int linkId;
    }
}