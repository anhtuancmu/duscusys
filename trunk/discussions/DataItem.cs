using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discussions
{
    class DataItem
    {
        private string name;
        private bool canDrop;

        public string Name
        {
            get { return name; }
        }

        public bool CanDrop
        {
            get { return canDrop; }
        }

        public DataItem(string name, bool canDrop)
        {
            this.name = name;
            this.canDrop = canDrop;
        }
    }
}
