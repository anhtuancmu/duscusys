using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon;
using Discussions.RTModel.Operations;
using System.Collections;
using Discussions.RTModel.Model;
using Discussions.RTModel;
using Discussions.model;

namespace DiscussionsClientRT
{
    class Program
    {
        static void Main(string[] args)
        {            
            ClientRT c = new ClientRT(0,"localhost", 
                                     "usr_"+(new Random()).Next(400),
                                     -1,
                                     DeviceType.Sticky); 

            while (true)
            {
                c.Service();
                System.Threading.Thread.Sleep(300);
            }
        }
    }
}
