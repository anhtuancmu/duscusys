using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DiscSvc
{
    /// <summary>
    /// Summary description for ScreenshotHandler
    /// </summary>
    public class ScreenshotHandler : IHttpHandler
    {
        /// <summary>
        /// Keeps mapping of shape IDs to pathnames of temp files. 
        /// Each key has format requestId shapeId 
        /// </summary>
        private static Dictionary<string, string> _screens = new Dictionary<string, string>();

        static void CleanupMediaEntries(int requestId)
        {
            lock (_screens)
            {
                foreach (var kvp in _screens.ToArray())
                {                    
                    if (KeyToRequestId(kvp.Key) == requestId)
                    {
                        File.Delete(_screens[kvp.Key]);
                        _screens.Remove(kvp.Key);
                    }
                }               
            }
        }

        static void AddMediaEntry(string key, string pathName)
        {
            lock (_screens)
            {
                _screens.Add(key, pathName);
            }
        }

        static int NextRequestId = 0;
        static int GetRequestId()
        {
            lock (_screens)
            {
                return NextRequestId++;
            }
        }

        static string ClientLocation()
        {
            var client1 = @"C:\Program Files (x86)\Discussion system\ConsoleLauncher.exe";
            if (File.Exists(client1))
                return client1;
            else
                return @"C:\Program Files\Discussion system\Discussions.exe"; 
        }

        public static string TempDir()
        {
            string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "discusys");
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
            return tempPath;
        }

        public static string RandomFilePath(string extension)
        {
            return Path.Combine(TempDir(), Guid.NewGuid().ToString() + extension);
        }

        static string RunClientAndWait(int topicId, int discId)
        {        
           var metaInfoPath = RandomFilePath(".txt");
           var parameters = string.Format("{0} {1} {2}", topicId, discId, metaInfoPath);

           var psi = new ProcessStartInfo(ClientLocation(),"");
           psi.UseShellExecute = false;
           // psi.UserName = "disc";           
          // psi.CreateNoWindow = true;
           //Process pro = Process.Start(ClientLocation(), parameters);
           var pwd = new System.Security.SecureString();
           pwd.AppendChar('d'); 
           pwd.AppendChar('i'); 
           pwd.AppendChar('s'); 
           pwd.AppendChar('c');          
           //psi.Password = pwd;
           //psi.WorkingDirectory = @"C:\Program Files (x86)\Discussion system";
           //psi.Domain = Environment.UserDomainName;
           Process pro = Process.Start(psi);
           pro.WaitForExit();
           return metaInfoPath;
        }
        
        static Dictionary<int,string> MetaInfoToDict(string metaPathName)
        {
            var res = new Dictionary<int, string>(); 

            using (var fs = new BinaryReader(new FileStream(metaPathName, FileMode.Open)))
            {
                var count = fs.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    var shapeId = fs.ReadInt32();
                    var pathName = fs.ReadString();
                    res.Add(shapeId, pathName);
                }
            }

            return res;
        }

        static string RequestScreenshotToKey(int requestId, int screenId)
        {
            return string.Format("{0} {1}", requestId, screenId);
        }

        static int KeyToRequestId(string key)
        {
            string[] requestId_ShapeId = key.Split();
            return int.Parse(requestId_ShapeId[0]);                    
        }
            
        #region IHttpHandler
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/octet-stream";

            if (context.Request.Params["screenTopicId"] != null && context.Request.Params["screenDiscId"] != null)
            {
                var topicId = int.Parse(context.Request.Params["screenTopicId"]);
                var discId = int.Parse(context.Request.Params["screenDiscId"]);

                var requestId = GetRequestId();

                //launch client and make screens
                var metaInfoPathName = RunClientAndWait(topicId, discId);
                var screenDict = MetaInfoToDict(metaInfoPathName);
                File.Delete(metaInfoPathName);

                //cleanup task 
                var timer = new System.Timers.Timer(10000) { AutoReset = false };
                timer.Elapsed += delegate
                {
                    timer.Dispose();
                    CleanupMediaEntries(requestId);
                };
                timer.Start();

                //write response
                using (var bw = new BinaryWriter(context.Response.OutputStream))
                {
                    bw.Write(requestId);
                    bw.Write(screenDict.Count);
                    foreach (var kvp in screenDict)
                    {
                        bw.Write(kvp.Key);
                        bw.Write(kvp.Value);
                        AddMediaEntry(RequestScreenshotToKey(requestId, kvp.Key), kvp.Value);
                    }
                }
            }
            else if (context.Request.Params["requestId"] != null && context.Request.Params["screenId"] != null)
            {
                var requestId = int.Parse(context.Request.Params["requestId"]);
                var screenId = int.Parse(context.Request.Params["screenId"]);
             
                string screenPathName;
                lock (_screens)
                    screenPathName = _screens[RequestScreenshotToKey(requestId, screenId)];
                context.Response.WriteFile(screenPathName);
            }

            context.Response.StatusCode = 404;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        #endregion
    }
}