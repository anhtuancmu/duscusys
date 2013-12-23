using Discussions.YouViewer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiscussionsTests
{
    [TestClass]
    public class YouTubeProviderTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            YouTubeProvider.LoadVideoData("-RWgjtEwQYQ?v=2");
        }
    }
}
