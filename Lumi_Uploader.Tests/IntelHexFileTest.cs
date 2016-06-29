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

            try
            {
                var s = await Package.Current.InstalledLocation.GetFileAsync("IntelHexFileTest1.hex");
                Stream stream = await s.OpenStreamForReadAsync();
            } catch
            {
                Debug.WriteLine(Package.Current.InstalledLocation.Path);
            }



//            var l = stream.Length;
            
            Debug.WriteLine("Here" + 1);
            Assert.AreEqual(0, 1);
            return;
        }
        public async Task tester()
        {
            var a = 1 + 2;
            Assert.AreEqual(a, 3);
            return;
        }
    }
}
