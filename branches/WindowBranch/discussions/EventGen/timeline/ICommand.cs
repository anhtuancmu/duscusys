using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventGen.timeline
{
    //There are 3 trackable commands in timeline: 1. Create event 2. Delete event 3. Move event    
    public interface ICommand
    {
        void ToUndone();
        void ToDone();
    }
}