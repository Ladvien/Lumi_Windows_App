using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Reflection;
using System.IO;
using Windows.ApplicationModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Lumi;

namespace UnitTestProject1
{
    [TestClass]
    public class IntelHexFileTestClass
    {

        StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

        [TestMethod]
        public async Task shouldReadIntelHexFileIntoArray()
        {
            StorageFile sampleFile = null;
            try
            {
                sampleFile = await storageFolder.CreateFileAsync("output.txt");
            } catch
            {
                Debug.WriteLine("Couldn't open file");
            }
            
            // 1. Load test hex file
            // 2. Extract data using InelHexFileArray
            // 3. Compare to the manually extracted data.

            IntelHexFile intelHexFile = new IntelHexFile();
            Stream stream = null;
            try
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///IntelHexFileTest1.hex"));
                
                stream = await file.OpenStreamForReadAsync();
                //Debug.WriteLine(stream.ReadByte());
            } catch
            {
                Debug.WriteLine(Package.Current.InstalledLocation.Path);
            }

            byte[] intelHexFileArray = null;

            if(stream != null)
            {
                intelHexFileArray = intelHexFile.intelHexFileToArray(stream, 128);
            }
            
            try
            {
                Debug.WriteLine(sampleFile.Path);
                int fileLengthCounter = 0;
                int lineIndexCounter = 0;
                while (fileLengthCounter != intelHexFileArray.Length)
                {
                    
                    while (lineIndexCounter < 16)//&& fileLengthCounter < intelHexFileArray.Length)
                    {
                        await FileIO.AppendTextAsync(sampleFile, intelHexFileArray[fileLengthCounter].ToString("X2"));
                        fileLengthCounter++;
                        lineIndexCounter++;
                    }
                    await FileIO.AppendTextAsync(sampleFile, "\r\n");
                    lineIndexCounter = 0;
                }


                Debug.WriteLine(" END HEX FILE");
            } catch
            {
                Debug.WriteLine("Exception in printing IntelHexFileArray");
            }
            
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
