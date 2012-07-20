using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.DbModel;
using Microsoft.Windows.Controls;

namespace Discussions
{
    public class TextRefsAggregater
    {               
        public static string PlainifyRichText(RichText text)
        {
            return text.Text;
        }
    }
}
