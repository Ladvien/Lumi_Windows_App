using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Reflection;
using System.IO;
using Windows.ApplicationModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;

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
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///IntelHexFileTest1.hex"));
                Stream stream = await file.OpenStreamForReadAsync();
                Debug.WriteLine(stream.ReadByte());
            } catch
            {
                Debug.WriteLine(Package.Current.InstalledLocation.Path);
            }

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
