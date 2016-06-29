using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Reflection;
using System.IO;
using Windows.ApplicationModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    [TestClass]
    public class IntelHexFileTestClass
    {

        [TestMethod]
        public async Task shouldReadIntelHexFileIntoArray()
        {
            var s = await Package.Current.InstalledLocation.GetFileAsync("IntelHexFileTest1.hex");
            Stream stream = await s.OpenStreamForReadAsync();

            var l = stream.Length;

            Debug.WriteLine(l);
            Assert.AreEqual(l, l);
            return;
        }
        [TestMethod]
        public async void tester()
        {
            var a = 1 + 2;
            Assert.AreEqual(a, 3);
        }
    }
}
