using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Discussions.model;
using iTextSharp.text;

namespace Discussions
{
    public class DiscussionColors
    {
        public static Brush prosBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x8D, 0xEF, 0xBD));
        public static Brush consBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xF8, 0x96, 0xDA));
        public static Brush neutralBrush = new SolidColorBrush(Color.FromArgb(0xFF, 127, 127, 127));
        public static Brush badgeHighlightBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xDB, 0x65));         
        public static Brush recycleBinBgBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xA0, 0xA0));

        public static BaseColor GetSideColor(int sideCode)
        {           
            switch ((model.SideCode)sideCode)
            {
                case model.SideCode.Pros:
                    return new BaseColor(0x8D, 0xEF, 0xBD);

                case model.SideCode.Cons:
                    return new BaseColor(0xF8, 0x96, 0xDA);

                case model.SideCode.Neutral:
                    return new BaseColor(220, 220, 220);

                default:
                    return new BaseColor(0,0,0);
            }
        }
    }
}
