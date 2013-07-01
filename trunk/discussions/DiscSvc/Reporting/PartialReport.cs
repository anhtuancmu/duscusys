using System;
using System.Collections.Generic;
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
            }
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