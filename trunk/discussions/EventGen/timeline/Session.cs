using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EventGen.timeline
{
    public class Session
    {
        public string videoPathName = "";
        public string megFilePathName = "";
        Timeline _timeline;

        public Session(Timeline timeline)
        {
            _timeline = timeline;
        }

        public void Write(string pathName)
        {
            megFilePathName = pathName;
            using (var writer = new BinaryWriter(new FileStream(pathName, FileMode.Create)))
            {
                writer.Write(videoPathName);
                writer.Write(megFilePathName);
                _timeline.Write(writer);
            }
        }

        public void Read(string pathName)
        {
            megFilePathName = pathName;
            using (var reader = new BinaryReader(new FileStream(pathName, FileMode.Open)))
            {
                videoPathName   = reader.ReadString();
                megFilePathName = reader.ReadString();
                _timeline.Read(reader);
            }
        }
    }
}
