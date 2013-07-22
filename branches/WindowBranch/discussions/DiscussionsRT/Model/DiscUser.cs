using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Discussions.RTModel.Model
{
    public class DiscUser
    {
        public string Name { get; set; }
        public int ActNr = -1;
        public int usrDbId = -1;

        public DiscUser()
        {
        }

        public DiscUser(string Name, int ActNr)
        {
            this.Name = Name;
            this.ActNr = ActNr;
        }
    }
}