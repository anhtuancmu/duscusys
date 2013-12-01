using Badass.Model;
using Badass.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BadassTests
{
    [TestClass]
    public class SerializationTests
    {
        [TestMethod]
        public void TestHotkey()
        {
            string xmlStr = "";
            {
                var hk = new Hotkey {Alt = true, Key = System.Windows.Input.Key.B};
                xmlStr = ObjectSerializerHelper.SerializeObjectToString(hk);
            }

            {
                var hk2 = ObjectSerializerHelper.DeSerializeObjectFromString<Hotkey>(xmlStr);
                Assert.IsTrue(hk2.Alt==true);
                Assert.IsTrue(hk2.Ctrl == false);
                Assert.IsTrue(hk2.Shift == false);
                Assert.IsTrue(hk2.Key == System.Windows.Input.Key.B);
            }
        }
    }
}
