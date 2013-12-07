using System;
using Discussions.DbModel.model;

namespace DiscussionsClientRT
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ClientRT c = new ClientRT(0, "localhost",
                                      "usr_" + (new Random()).Next(400),
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