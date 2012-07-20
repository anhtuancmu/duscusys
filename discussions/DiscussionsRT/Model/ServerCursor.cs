using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributedEditor
{
    public class ServerCursor
    {
        public int OwnerId; //id of owner

        public ServerCursor(int UsrId)
        {
            this.OwnerId = UsrId;
        }
    }
}
