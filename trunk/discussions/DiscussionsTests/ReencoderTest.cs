using System.IO;
using System.Text;
using Discussions.webkit_host;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiscussionsTests
{
    [TestClass]
    public class ReencoderTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            string str = Reencoder.GetUrlContent("http://www.shinmai.co.jp/olympic/jouhou/shochi.htm");

            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            bw.Write(str);
            bw.Flush();

            ms.Position = 0;

            using (TextReader rdr = new StreamReader(ms, Encoding.GetEncoding("shift-jis")))
            {
                using (
                    TextWriter wrtr =
                        new StreamWriter(
                            @"C:\Users\User\Documents\Visual Studio 2013\Projects\tds3\discussions\bin\x86\Debug\qwe.html",
                            false,
                            Encoding.UTF8))
                {
                    wrtr.Write(rdr.ReadToEnd());
                }
            }

            //var writer = new BinaryWriter(
            //        new FileStream(@"C:\Users\User\Documents\Visual Studio 2013\Projects\tds3\discussions\bin\x86\Debug\qwe.html",
            //                       FileMode.OpenOrCreate)
            // );

            //writer.Write(reencoded);
            //writer.Close();
        }
    }
}
