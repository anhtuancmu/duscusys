﻿using System.Linq;
using LoginEngine;

namespace DistributedEditor
{
    public class Cursor
    {
        public int OwnerId; //id of owner
        private string _name = "";

        public Cursor(int UsrId)
        {
            this.OwnerId = UsrId;
        }

        public string Name
        {
            get
            {
                if (_name == "")
                {
                    var pers = DbCtx.Get().Person.FirstOrDefault(p0 => p0.Id == OwnerId);
                    if (pers != null)
                        _name = pers.Name;
                }
                return _name;
            }
            set { _name = value; }
        }

        public int Color
        {
            get
            {
                if (OwnerId == 0)
                    return 255;
                
                return DbCtx.Get().Person.FirstOrDefault(p0 => p0.Id == OwnerId).Color;
            }
        }
    }
}