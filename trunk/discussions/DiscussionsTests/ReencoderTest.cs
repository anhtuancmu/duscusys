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
            var reencoded = Reencoder.ShiftJisToUtf8(str);
        }
     
    }
}
