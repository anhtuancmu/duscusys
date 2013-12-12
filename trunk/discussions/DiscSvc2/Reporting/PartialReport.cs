using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using Discussions.DbModel;

namespace DiscSvc.Reporting
{
    public partial class Report : IDisposable
    {
        public QueryParams QueryParams;
        public ReportParameters ReportParams;
        public IEnumerable<Tuple<Person, Person>> Participants;
        public readonly List<int> ProcessedArgPoints = new List<int>();
        public string ReportUrl;
        public string BaseUrl;
        public string MediaUrl
        {
            get { return ReportUrl.Replace("report", "media"); }
        }
        public string SourcesUrl
        {
            get { return ReportUrl.Replace("report", "sources"); }
        }

        public Reporter.ReportCollector ComplexReport;

        public struct ImgInfo
        {
            public string PhysPath;
            public string VPath;
            public string url;
        }

        private readonly Dictionary<int, ImgInfo> _screenshotFiles = new Dictionary<int, ImgInfo>();
        public Dictionary<int, ImgInfo> Screenshots
        {
            get { return _screenshotFiles; }
        }

        public void ReceiveScreenshots(Dictionary<int, byte[]> screenshots, HttpContext context)
        {
            foreach (var kv in screenshots)
            {
                var file = Path.GetRandomFileName() + ".png";
                var vPathName = @"~\img\" + file;
                var url = new System.Uri(context.Request.Url, @"img\" + file).AbsoluteUri;
                var physPathName = Path.Combine(context.Server.MapPath("~"), "img", file);

                var imgInfo = new ImgInfo { PhysPath = physPathName, VPath = vPathName, url = url };
              
                File.WriteAllBytes(physPathName, kv.Value);
                _screenshotFiles.Add(kv.Key, imgInfo);

                //resample screenshot of final scene
                if (kv.Key == -1)
                {
                    ResampleScreenshotOfFinalScene(imgInfo);
                }
            }
        }

        static void ResampleScreenshotOfFinalScene(ImgInfo imgInfo)
        {
            var screenshotStream = new FileStream(imgInfo.PhysPath, FileMode.Open);
            var bmp = new Bitmap(screenshotStream);
            screenshotStream.Close();

            int newWidth;
            int newHeight;
            if (bmp.Width > 2024)
            {
                newWidth = 2024;
                newHeight = newWidth * bmp.Height / bmp.Width;
            }
            else
            {
                newWidth  = bmp.Width;
                newHeight = bmp.Height;
            }
            var resized = ImageUtilities.ResizeImage(bmp, newWidth, newHeight);
            resized.Save(imgInfo.PhysPath, ImageFormat.Png);
        }

        public void Dispose()
        {
            foreach (var kv in _screenshotFiles)
            {
                File.Delete(kv.Value.PhysPath);             
            }
        }
    }
}