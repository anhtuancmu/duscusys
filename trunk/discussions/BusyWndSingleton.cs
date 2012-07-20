using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discussions
{
    public class BusyWndSingleton 
    {
        static BusyWindow wnd = null;

        public static void Show(string msg = null)
        {
            if (wnd != null)
                return;

            wnd = new BusyWindow();
            if (msg == null)
            {
                wnd.lblMsg.Content = "Please, wait...";
                //wnd.busy.BusyContent = "Please, wait...";
            }
            else
            {
                wnd.lblMsg.Content = msg;
                //wnd.busy.BusyContent = msg;
            }

            wnd.Show();
        }

        public static void Hide()
        {
            if (wnd != null)
            {                
                wnd.Close();
                wnd = null;
            }
        }
    }
}
