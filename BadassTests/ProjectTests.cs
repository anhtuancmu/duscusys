using Badass.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BadassTests
{
    [TestClass]
    public class ProjectTests
    {
        [TestMethod]
        public void TestTypeNameBahavior()
        {
            var p = new Project(name:"experiment");
            p.AddSessionType("type1");
            Assert.IsTrue(p.Sessions.Count == 0);

            p.AddSessionType("type2");
            Assert.IsTrue(p.Sessions.Count == 0);
           
            p.AddSessionName("name");            
            Assert.IsTrue(p.Sessions.Count==2);

            p.RemoveSessionType("type2");
            Assert.IsTrue(p.Sessions.Count == 1);
        }
    }
}
