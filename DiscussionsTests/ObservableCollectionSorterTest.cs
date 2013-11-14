using System.Collections.ObjectModel;
using Discussions.util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiscussionsTests
{
    [TestClass]
    public class ObservableCollectionSorterTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var unordered = new ObservableCollection<int>{-5,0,6,3,10,-4};           
            ObservableCollectionSorter.Sort(unordered, (x, y) => x.CompareTo(y));

            Assert.IsTrue(unordered[0] == -5);
            Assert.IsTrue(unordered[1] == -4);
            Assert.IsTrue(unordered[2] == 0);
            Assert.IsTrue(unordered[3] == 3);
            Assert.IsTrue(unordered[4] == 6);
            Assert.IsTrue(unordered[5] == 10);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var unordered = new ObservableCollection<int> { 0, -5};
            ObservableCollectionSorter.Sort(unordered, (x, y) => x.CompareTo(y));

            Assert.IsTrue(unordered[0] == -5);
            Assert.IsTrue(unordered[1] == 0);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var unordered = new ObservableCollection<int> { 0 };
            ObservableCollectionSorter.Sort(unordered, (x, y) => x.CompareTo(y));

            Assert.IsTrue(unordered[0] == 0);
        }

        [TestMethod]
        public void TestMethod4()
        {
            var unordered = new ObservableCollection<int>();
            ObservableCollectionSorter.Sort(unordered, (x, y) => x.CompareTo(y));
        }
    }
}
