using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discussions.YouViewer
{
    /// <summary>
    /// A simple data class
    /// </summary>
    public class YouTubeInfo
    {
        #region Data
        public string LinkUrl { get; set; }
        public string EmbedUrl { get; set; }
        public string ThumbNailUrl { get; set; }
        public string VideoTitle { get; set; } 
        #endregion
    }
}
