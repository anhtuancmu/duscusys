using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Web;
using Discussions.youtube;

namespace Discussions.YouViewer
{
    /// Uses XLINQ to create a List<see cref="YouTubeInfo">YouTubeInfo</see> using an Rss feed.
    /// 
    /// The following links are useful information regarding how the YouTube API works 
    /// 
    /// Example url
    ///
    /// http://gdata.youtube.com/feeds/api/videos?q=football+-soccer&alt=rss&orderby=published&start-index=11&max-results=10&v=1
    ///
    ///
    /// API Notes
    /// http://code.google.com/apis/youtube/2.0/developers_guide_protocol_api_query_parameters.html
    /// </summary>
    public class YouTubeProvider
    {
        #region Data

        //private const string SEARCH = "http://gdata.youtube.com/feeds/api/videos?q={0}&alt=rss&&max-results=20&v=1";
        private const string SEARCH = "https://gdata.youtube.com/feeds/api/videos/{0}";
        

        #endregion

        #region Load Videos From Feed

        /// <summary>
        /// Returns a List<see cref="YouTubeInfo">YouTubeInfo</see> which represent
        /// the YouTube videos that matched the keyWord input parameter
        /// </summary>
        //public static List<YouTubeInfo> LoadVideosKey2(string keyWord)
        //{
        //    try
        //    {
        //        var xraw = XElement.Load(string.Format(SEARCH, keyWord));
        //        var xroot = XElement.Parse(xraw.ToString());
        //        var links = (from item in xroot.Element("channel").Descendants("item")
        //                     select new YouTubeInfo
        //                         {
        //                             LinkUrl = item.Element("link").Value,
        //                             EmbedUrl = GetEmbedUrlFromLink(item.Element("link").Value),
        //                             ThumbNailUrl = GetThumbNailUrlFromLink(item),
        //                             VideoTitle = GetTitleFromLink(item)
        //                         }).Take(20);

        //        return links.ToList<YouTubeInfo>();
        //    }
        //    catch (Exception e)
        //    {
        //        Trace.WriteLine(e.Message, "ERROR");
        //    }
        //    return null;
        //}

        public static YouTubeInfo LoadVideoData(string videoId)
        {
            try
            {
                XNamespace media = "http://search.yahoo.com/mrss/";
                XNamespace yt = "http://gdata.youtube.com/schemas/2007";

                var request = string.Format(SEARCH, videoId);
                var xraw = XElement.Load(request);
                var xroot = XElement.Parse(xraw.ToString());

                var playerUrl = xroot.Descendants(media + "player").First().Attribute("url").Value;

                var thumbnailNode = xroot.Descendants(media + "thumbnail").
                                    LastOrDefault(t => t.Attribute("height").Value == "360");
                if (thumbnailNode == null)
                    thumbnailNode = xroot.Descendants(media + "thumbnail").
                        FirstOrDefault(t => t.Attribute(yt + "height").Value == "90");
                if (thumbnailNode == null)
                    thumbnailNode = xroot.Descendants(media + "thumbnail").First();

                var embedUrl = string.Format("http://www.youtube.com/embed/{0}/?autoplay=1", videoId);
               
                var videoTitle = GetTitleFromLink(xroot);

                return new YouTubeInfo
                {
                    EmbedUrl = embedUrl,
                    LinkUrl = playerUrl,
                    ThumbNailUrl = thumbnailNode!=null ? thumbnailNode.Attribute("url").Value : "",
                    VideoTitle = videoTitle
                };
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message, "ERROR");
            }
            return null;
        }

        static readonly Regex _videoIdExtractor =
            new Regex(@"\?v=([^\&]*)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static YouTubeInfo LoadVideo(string url)
        {
            var match = _videoIdExtractor.Match(url);

            string videoId = null;
            if (match.Groups.Count >= 2)
            {
                if (!string.IsNullOrEmpty(match.Groups[match.Groups.Count - 1].Value))
                {
                    videoId = match.Groups[match.Groups.Count - 1].Value;
                }
            }

            if (string.IsNullOrEmpty(videoId))
                videoId = getIdFromShortYoutubeURL(url);
            if (string.IsNullOrEmpty(videoId))
                videoId = HttpUtility.ParseQueryString(url).Get(0);

            if (string.IsNullOrEmpty(videoId))
                return null;

            YouTubeInfo info = LoadVideoData(videoId);
            return info;
        }

        private static string getIdFromShortYoutubeURL(string url)
        {
            int i = url.IndexOf("http://youtu.be/");
            if (i == -1)
                return null;
            
            return url.Substring(i + 16);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Simple helper methods that tunrs a link string into a embed string
        /// for a YouTube item. 
        /// turns 
        /// http://www.youtube.com/watch?v=hV6B7bGZ0_E
        /// into
        /// http://www.youtube.com/embed/hV6B7bGZ0_E
        /// </summary>
        private static string GetEmbedUrlFromLink(string link)
        {
            try
            {
                string embedUrl = link.Substring(0, link.IndexOf("&")).Replace("watch?v=", "embed/");
                return embedUrl + "?autoplay=1";
            }
            catch
            {
                return link;
            }
        }

        private static string GetTitleFromLink(XElement element)
        {
            XElement group = null;
            var videoTitle = "";

            foreach (XElement desc in element.Descendants())
            {
                if (desc.Name.LocalName == "group")
                {
                    group = desc;
                    break;
                }
            }

            if (group != null)
            {
                foreach (XElement desc in group.Descendants())
                {
                    if (desc.Name.LocalName == "title")
                    {
                        videoTitle = desc.Value;
                        break;
                    }
                }
            }

            return videoTitle;
        }

        #endregion
    }
}