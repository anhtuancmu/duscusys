using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public struct UnselectAllEvent
    {
        public static UnselectAllEvent Read(Dictionary<byte, object> par)
        {
            var res = new UnselectAllEvent();                          
            return res;
        }

        public static Dictionary<byte, object> Write()
        {
            var res = new Dictionary<byte, object>();                   
            return res;      
        }
    }
}
