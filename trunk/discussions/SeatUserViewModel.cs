using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Discussions
{
    public class SeatUserViewModel
    {
        public SeatUserViewModel(string seat, int color, string user, string discussion)
        {            
            SeatName  = seat;
            _brush = new SolidColorBrush(Utils.IntToColor(color));
            UserName  = user;
            Discussion = discussion;
        }

        public string SeatName
        {
            get;
            set;
        }

        SolidColorBrush _brush = null;
        public SolidColorBrush SeatColor
        {
            get
            {
                return _brush;
            }
        }

        public string UserName
        {
            get;
            set;
        }

        public string Discussion
        {
            get;
            set;
        }
    }
}
