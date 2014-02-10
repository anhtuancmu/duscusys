﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Discussions.RTModel
{
    internal class ScreenshotHandler
    {
        private string ClientLocation()
        {
            var client1 = @"C:\Program Files (x86)\Discussion system\Discussions.exe";
            if (File.Exists(client1))
                return client1;
            return @"C:\Program Files\Discussion system\Discussions.exe";
        }

        private string TempDir()
        {
            string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "discusys");
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
            return tempPath;
        }

        private string RandomFilePath(string extension)
        {
            return Path.Combine(TempDir(), Guid.NewGuid().ToString() + extension);
        }

        public string RunClientAndWait(int topicId, int discId)
        {
            var metaInfoPath1 = RandomFilePath(".txt");
            var parameters = string.Format("{0} {1} \"{2}\"", topicId, discId, metaInfoPath1);

            var psi = new ProcessStartInfo(ClientLocation(), parameters) {UseShellExecute = false};
            var pro = Process.Start(psi);

            pro.WaitForExit();
            return metaInfoPath1;
        }

        public Dictionary<int, string> MetaInfoToDict(string metaPathName)
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
    }
}