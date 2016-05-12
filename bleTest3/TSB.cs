using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using System.Threading;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Documents;
using Windows.UI;
using bleTest3;
using lumi;
using Windows.UI.Xaml;
using System.Collections;
using Windows.Storage;
using System.IO;
using Windows.Storage.Pickers;

namespace bleTest3
{
    class TSB
    {
        #region devices
        enum DEVICE_SIGNATURE
        {
            ATTINY_13A = 0x1E9007,
            ATTINY_13 = 0x1E9007,
            ATTINY_1634 = 0x1E9412,
            ATTINY_167 = 0x1E9487,
            ATTINY_2313A = 0x1E910A,
            ATTINY_2313 = 0x1E910A,
            ATTINY_24A = 0x1E910B,
            ATTINY_24 = 0x1E910B,
            ATTINY_25 = 0x1E910B,
            ATTINY_261A = 0x1E910C,
            ATTINY_261 = 0x1E910C,
            ATTINY_4313 = 0x1E920D,
            ATTINY_44A = 0x1E9207,
            ATTINY_44 = 0x1E9207,
            ATTINY_441 = 0x1E9215,
            ATTINY_45 = 0x1E9206,
            ATTINY_461A = 0x1E9208,
            ATTINY_461 = 0x1E9208,
            ATTINY_48 = 0x1E9209,
            ATTINY_84A = 0x1E930C,
            ATTINY_84 = 0x1E930C,
            ATTINY_841 = 0x1E9315,
            ATTINY_85 = 0x1E930B,
            ATTINY_861A = 0x1E930D,
            ATTINY_861 = 0x1E930D,
            ATTINY_87 = 0x1E9387,
            ATTINY_88 = 0x1E9311,
            ATMEGA_162 = 0x1E9403,
            ATMEGA_164A = 0x1E940F,
            ATMEGA_164PA = 0x1E940A,
            ATMEGA_164P = 0x1E940A,
            ATMEGA_165A = 0x1E9410,
            ATMEGA_165PA = 0x1E9407,
            ATMEGA_165P = 0x1E9407,
            ATMEGA_168A = 0x1E9406,
            ATMEGA_168 = 0x1E9406,
            ATMEGA_168PA = 0x1E940B,
            ATMEGA_168P = 0x1E940B,
            ATMEGA_169A = 0x1E9411,
            ATMEGA_169PA = 0x1E9405,
            ATMEGA_169P = 0x1E9405,
            ATMEGA_16A = 0x1E9403,
            ATMEGA_16 = 0x1E9403,
            ATMEGA_16HVA = 0x1E940C,
            ATMEGA_16HVB = 0x1E940D,
            ATMEGA_16ATMEGA_1 = 0x1E9484,
            ATMEGA_16U2 = 0x1E9489,
            ATMEGA_16U4 = 0x1E9488,
            ATMEGA_324A = 0x1E9515,
            ATMEGA_324PA = 0x1E9511,
            ATMEGA_324P = 0x1E9508,
            ATMEGA_3250A = 0x1E950E,
            ATMEGA_3250 = 0x1E9506,
            ATMEGA_3250PA = 0x1E950E,
            ATMEGA_3250P = 0x1E950E,
            ATMEGA_325A = 0x1E9505,
            ATMEGA_325 = 0x1E9505,
            ATMEGA_325PA = 0x1E9505,
            ATMEGA_325P = 0x1E950D,
            ATMEGA_328 = 0x1E9514,
            ATMEGA_328P = 0x1E950F,
            ATMEGA_3290A = 0x1E950C,
            ATMEGA_3290 = 0x1E9504,
            ATMEGA_3290PA = 0x1E950C,
            ATMEGA_3290P = 0x1E950C,
            ATMEGA_329A = 0x1E9503,
            ATMEGA_329 = 0x1E9503,
            ATMEGA_329PA = 0x1E950B,
            ATMEGA_329P = 0x1E950B,
            ATMEGA_32A = 0x1E9502,
            ATMEGA_32C1 = 0x1E9586,
            ATMEGA_32 = 0x1E9502,
            ATMEGA_32HVB = 0x1E9510,
            ATMEGA_32ATMEGA_1 = 0x1E9584,
            ATMEGA_32U2 = 0x1E958A,
            ATMEGA_32U4 = 0x1E9587,
            ATMEGA_406 = 0x1E9507,
            ATMEGA_48A = 0x1E9205,
            ATMEGA_48 = 0x1E9205,
            ATMEGA_48PA = 0x1E920A,
            ATMEGA_48P = 0x1E920A,
            ATMEGA_640 = 0x1E9608,
            ATMEGA_644A = 0x1E9609,
            ATMEGA_644 = 0x1E9609,
            ATMEGA_644PA = 0x1E960A,
            ATMEGA_644P = 0x1E960A
        }
        #endregion devices

        #region enumerations
        // TSB Command Variables
        public enum commands : int
        {
            none = 0,
            hello = 1,
            request = 2,
            confirm = 3,
            readFlash = 4,
            writeFlash = 5,
            readEEPROM = 6,
            writeEEPROM = 7,
            readUserData = 8,
            writeUserData = 9,
            error = 10
        }

        public static string[] commandsAsStrings =
        {
            "",
            "@@@",
            "?",
            "!",
            "f",
            "F",
            "e",
            "E",
            "c",
            "C",
        };

        public enum displayFlash
        {
            none = 0,
            dataOnly = 1,
            addressAndData = 2,
            asIntelHexFile = 3
        }

        public enum statuses : int
        {
            uknown = 0,
            connected = 1,
            writeFail = 2,
            readFail = 3,
            writeSuccessful = 4,
            readSuccessful = 5,
            downloadSuccessful = 6,
            uploadSuccessful = 7,
            x = 8,
            y = 9,
            error = 10
        }
        #endregion enumerations

        #region properties
        public delegate void TsbUpdateCommand(statuses tsbConnectionStatus);
        public event TsbUpdateCommand TsbUpdatedCommand;

        public SerialBuffer readFlashBuffer = new SerialBuffer();
        public SerialBuffer readFlashBufferTmp = new SerialBuffer();

        public IntelHexFile intelHexFileHandler = new IntelHexFile();
        public List<byte> intelHexFileToUpload;

        const int commandAttempts = 3;

        private string rxBuffer = "";

        string filePath = @"C:\Users\cthom\Documents\myFileTest.txt";
        //string filePath = "ms-appx:///thedata.txt";
        string fileName = "";

        StorageFile hexFileToRead;

        // Firmware date.
        public string firmwareDateString;
        long firmwareDate;

        // Atmel device signature.
        string deviceSignature;
        // The size is in words, make it bytes.
        int pageSize;
        // Get flash size.
        int flashSize;
        // Get EEPROM size.
        int fullEepromSize;
        // Number of pages
        int numberOfPages = 0;


        // Properties used for ReadFlash()
        List<byte> rxByteArray;
        int pageIndex = 0;

        DEVICE_SIGNATURE deviceSignatureValue = new DEVICE_SIGNATURE();
        public commands commandInProgress = new commands();
        displayFlash displayFlashType = displayFlash.asIntelHexFile;

        serialPortsExtended serialPorts;
        Paragraph theOneParagraph;
        ProgressBar progressBar;
        TextBlock txbOpenFilePath;

        #endregion properties

        public delegate void CallBackEventHandler(object sender, EventArgs args);
        public event CallBackEventHandler Callback;
        // 
        public SerialBuffer serialBuffer = new SerialBuffer();
        // When a write command is sent, then timeout timer is started.
        public DispatcherTimer writeTimer = new DispatcherTimer();

        public void init(serialPortsExtended serialPortMain, Paragraph mainDisplayMain, ProgressBar mainProgressBar, SerialBuffer _serialBuffer, TextBlock _openFilePath)
        {
            rxByteArray = new List<byte>();
            intelHexFileToUpload = new List<byte>();
            serialPorts = serialPortMain;
            theOneParagraph = mainDisplayMain;
            progressBar = mainProgressBar;
            serialBuffer = _serialBuffer;
            txbOpenFilePath = _openFilePath;

            // Write timeout timer.
            writeTimer.Tick += writeTimer_Tick;

            serialBuffer.RXbufferUpdated += new SerialBuffer.CallBackEventHandler(RXbufferUpdated);
            serialBuffer.TXbufferUpdated += new SerialBuffer.CallBackEventHandler(TXbufferUpdated);
            readFlashBuffer.bufferUpdated += new SerialBuffer.CallBackEventHandler(ReadFlashBuffer_bufferUpdated);
        }

        private void ReadFlashBuffer_bufferUpdated(object sender, EventArgs args)
        {
            
        }

        public void startWriteTimeoutTimer(int seconds)
        {
            writeTimer.Interval = new TimeSpan(0, 0, 0, seconds);
            writeTimer.Start();
        }

        private void writeTimer_Tick(object sender, object e)
        {
            commandInProgress = commands.error;
            RXbufferUpdated(this, null);
            writeTimer.Stop();
        }

        private void TXbufferUpdated(object sender, EventArgs args)
        {
            
        }

        private void RXbufferUpdated(object sender, EventArgs args)
        {
            // 1. Route to command-in-progress.

            switch (commandInProgress)
            {
                case commands.error:
                    appendText("Uh-oh. Bad stuff happened.\n", Colors.Crimson);
                    TsbUpdatedCommand(statuses.error);
                    break;
                case commands.hello:
                    bool outcome = helloProcessing();
                    writeTimer.Stop();
                    if (outcome)
                    {
                        TsbUpdatedCommand(statuses.connected);
                        // Whatever says we are successful.
                    } else
                    {
                        appendText("Failed to connect to TinySafeBoot", Colors.Crimson);
                    } 
                    break;
                case commands.readFlash:
                    processFlashRead();
                    break;
                default:
                    Debug.WriteLine("Defaulted in RXbuffer switch\n");
                    break;
            }
        }

        public void scrollToBottomOfTerminal()
        {
            //mainDisplay.SelectionStart = mainDisplay.Text.Length;
            //mainDisplay.ScrollToCaret();
        }

        public void setFilePath(string path)
        {
            filePath = path;
        }

        public void setFileName(string name)
        {
            fileName = name;
        }

        public string commandString(commands commandNumber)
        {
            // 1. Return the command string.
            return commandsAsStrings[(int)commandNumber];
        }

        public void updateActionInProgress(commands commandNumber)
        {
            commandInProgress = commandNumber;
        }

        public void setFlashDisplay(displayFlash displayFlashTypeArgument)
        {
            displayFlashType = displayFlashTypeArgument;
        }

        public async void hello()
        {
            commandInProgress = commands.hello;
            startWriteTimeoutTimer(1);
            serialBuffer.txBuffer = getCommand(commands.hello);
//            await serialPorts.write(commandsAsStrings[(int)commands.hello]);
        }

        public bool helloProcessing()
        {
            
            // 1. Try handshake ("@@@") three times; or continue if successful.
            // 2. Check if reply seems valid(ish).
            // 3. Chop up the reply into useful device data.
            // 4. Save the device data for later.
            // 5. If not reply, let the user know it was a fail.

            if(serialBuffer.bytesInRxBuffer() > 16)
            {
                byte[] data = serialBuffer.readAllBytesFromRXBuffer();

                int[] firmwareDatePieces = { 0x00, 0x00 };
                int firmwareStatus = 0x00;
                int[] signatureBytes = { 0x00, 0x00, 0x00 };
                int pagesizeInWords = 0x00;
                int[] freeFlash = { 0x00, 0x00 };
                int[] eepromSize = { 0x00, 0x00 };

                string tsbString = "";
                tsbString += (char)data[0];
                tsbString += (char)data[1];
                tsbString += (char)data[2];

                // ATtiny have all lower case, ATMega have upper case.  Not sure if it's expected.
                if (tsbString.Contains("tsb") || tsbString.Contains("TSB") && data.Length == 17)
                {
                    if (data.Length > 16)
                    {
                        firmwareDatePieces[0] = data[3];
                        firmwareDatePieces[1] = data[4];
                        firmwareStatus = data[5];
                        signatureBytes[0] = data[6];
                        signatureBytes[1] = data[7];
                        signatureBytes[2] = data[8];
                        pagesizeInWords = data[9];
                        freeFlash[0] = data[10];
                        freeFlash[1] = data[11];
                        eepromSize[0] = data[12];
                        eepromSize[1] = data[13];
                    }

                    // Date of firmware.
                    int day = firmwareDatePieces[0];
                    int month = ((firmwareDatePieces[1] & 0xF0) >> 1);
                    int year = (firmwareDatePieces[1] & 0x0F);

                    firmwareDateString = (month + " " + day + " " + "20" + year);

                    // Atmel device signature.
                    deviceSignature = signatureBytes[0].ToString("X2") + " " + signatureBytes[1].ToString("X2") + " " + signatureBytes[2].ToString("X2");
                    Int32 combinedDeviceSignature = (Int32)(((signatureBytes[0] << 16) | signatureBytes[1] << 8) | signatureBytes[2]);
                    deviceSignatureValue = (DEVICE_SIGNATURE)combinedDeviceSignature;

                    // The size is in words, make it bytes.
                    pageSize = (pagesizeInWords * 2);
                    string pageSizeString = (pagesizeInWords * 2).ToString();

                    // Get flash size.
                    flashSize = ((freeFlash[1] << 8) | freeFlash[0]) * 2;
                    string flashLeft = flashSize.ToString();

                    // REPLACE WITH DEVICE INFO
                    numberOfPages = flashSize / pageSize;
                    //numberOfPages = 16;

                    // Get EEPROM size.
                    fullEepromSize = ((eepromSize[1] << 8) | eepromSize[0]) + 1;
                    string eeprom = fullEepromSize.ToString();

                    string tsbHanshakeInfo = 
                              deviceSignatureValue.ToString()
                         + "\nFirmware Date:\t" + firmwareDateString
                         + "\nStatus:\t\t" + firmwareStatus.ToString("X2")
                         + "\nSignature:\t" + deviceSignature
                         + "\nPage Size\t" + pageSizeString
                         + "\nFlash Free:\t" + flashLeft
                         + "\nEEPROM size:\t" + eeprom + "\n";
                    appendText(tsbHanshakeInfo, Colors.White);
                    commandInProgress = commands.none;
                    //setTsbConnectionSafely(true);
                    return true;
                }
            }
            else
            {
                string error = "Could not handshake with TSB. Please reset and try again.\n";
                appendText(error, Colors.Crimson);
                return false;
            }
            return false;
        }

        public SolidColorBrush getColoredBrush(Color color)
        {
            return new SolidColorBrush(color);
        }
        
        public void appendText(string str, Color color)
        {
            Run r = new Run();
            //r.FontFamily = new FontFamily("Courier New");
            r.Foreground = getColoredBrush(color);
            r.Text = str;
            theOneParagraph.Inlines.Add(r);
        }

        public void clearMainDisplay()
        {
            theOneParagraph = new Paragraph();
        }

        public byte[] getCommand(commands command)
        {
            string cmdStr = commandsAsStrings[(int)command];
            byte[] commandAsByteArray = new byte[cmdStr.Length];
            for(int i =0; i < cmdStr.Length; i++)
            {
                commandAsByteArray[i] = (byte)cmdStr[i];
            }
            return commandAsByteArray;
        }
        
        public async void readFlash()
        {
            // 1. Set readFlash as the commandInProgress
            // 2. Write read Flash command.
            commandInProgress = commands.readFlash;
            await serialPorts.write(commandsAsStrings[(int)commands.readFlash]);
            await serialPorts.write(commandsAsStrings[(int)commands.confirm]);
        }

        public async void processFlashRead()
        {
            // 1. Update progressBar based on pages read.
            // 2. Add new serialBuffer data (recently received) to rxByteArray List.
            // 3. Check the dogear of the page (bottom  corner bytes)
            // 3. If last two bytes are FF FF, then break, as end of Flash.
            // 3. If all pages are collected, process read.
            // 4. Continue to get data until buffer is full.
            // 5. If not the end, request another page.

            byte[] tmpRxByteArray = serialBuffer.readAllBytesFromRXBuffer();
            var currentProgressBarValue = 100*((float)rxByteArray.Count / (float)flashSize);
            progressBar.Value = map(currentProgressBarValue, 0, 100, 0, 50);
            rxByteArray.AddRange(tmpRxByteArray);
            if (dogEarCheck(tmpRxByteArray.ToArray())){
                progressBar.Value = 50;
                parseAndPrintRawRead(rxByteArray);
                progressBar.Value = 100;
                return;
            }
            else if (tmpRxByteArray.Length >= pageSize)
            {
                await serialPorts.write(commandsAsStrings[(int)commands.confirm]);
            }
        }

        private bool dogEarCheck(byte[] byteArray)
        {
            // 1. If null, return false.
            // 2. Check the last two bytes in a page to see if they are blank (0xFF).
            if(byteArray == null) { return false;}
            // Check to see what the if the page is dogeared.
            if(byteArray[byteArray.Length - 1] == 0xFF && byteArray[byteArray.Length - 2] == 0xFF)
            {return true;}
            else
            {return false;}

        }

        public void parseAndPrintRawRead(List<byte> rawFlashRead)
        {
            // 0. Greeting
            // 1. Get number of pages reads.
            // 2. Define page array, lineBuffer, lineSize, location.
            //    and get doc path and stream.
            // 3. Loop through each page...
            // 4. Loop through page depth (pageDepth * lineSize = page)
            // 5. Loop through line
            // 6. Write assemble a HEX string a byte at a time.
            // 7. Write the assembled HEX string to display and file.
            // 8. Clear line buffer.
            // 9. Repeat 1-8 until end of int array.

            //string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //System.IO.StreamWriter outputFile = new System.IO.StreamWriter(mydocpath + @"\Flash_Read_Output.hex");

            int numberOfPagesRead = (rawFlashRead.Count / pageSize);
            int pageDepth = (pageSize / 16);
            const int pageWidth = 16;
            string lineBuffer = "";

            byte[] byteArray = rawFlashRead.ToArray();

            appendText("\nFlash readout for " + deviceSignatureValue + "\n\n", Colors.White);

            for (int i = 0; i < numberOfPagesRead; i++)
            {
                if (displayFlashType != displayFlash.none)
                { appendText("\n\t Page #:" + i + "\n", Colors.Yellow); }
                for (int j = 0; j < pageDepth; j++)
                {
                    int location = ((i * pageSize) + (j * pageWidth));
                    for (int k = 0; k < pageWidth; k++)
                    {
                        lineBuffer += byteArray[location + k].ToString("X2");
                    }
                    getIntelFileHexString(location.ToString("X4"), lineBuffer.ToString());
                    lineBuffer = "";
                }
                var currentProgressBarValue = 100 * ((float)i / (float)numberOfPages);
                progressBar.Value = map(currentProgressBarValue, 0, 100, 50, 100);
            }

            scrollToBottomOfTerminal();
            //outputFile.Close();
        }

        public string getIntelFileHexString(string address, string data)
        {
            // 1. Get start code.
            // 2. Get and set byte count string.
            // 3. Set address string.
            // 4. Get record type.
            // 5. Set data
            // 6. Get and set checksum.
            // 7. Add newline at end.
            // 8. Return completed Intel HEX file line as string.

            string startCode = ":";
            string byteCount = (data.Length / 2).ToString("X2");
            // Address passed in.
            string recordType = "00"; // 00 = Data, 01 = EOF, 02 = Ext. Segment. Addr., 03 = Start Lin. Addr, 04 = Ext. Linear Addr., 05 = Start Linear Addr.
                                      // Checksum passed in

            string intelHexFileLine = startCode + byteCount + address + recordType + data;
            int checkSum = getCheckSumFromLine(intelHexFileLine);
            string checkSumString = checkSum.ToString("X2");
            intelHexFileLine += checkSumString;

            switch (displayFlashType)
            {
                case displayFlash.none:
                    // No display.
                    break;
                case displayFlash.dataOnly:
                    appendText(data + "\n", Colors.LawnGreen);
                    break;
                case displayFlash.addressAndData:
                    appendText(address + ": ", Colors.Yellow);
                    appendText(data + "\n", Colors.LawnGreen);
                    break;
                case displayFlash.asIntelHexFile:
                    appendText(":", Colors.Yellow);                  // Start code
                    appendText(byteCount, Colors.Green);             // Byte count
                    appendText(address, Colors.Purple);              // Address
                    appendText(recordType, Colors.Pink);             // Record type.
                    appendText(data, Colors.CadetBlue);              // Data
                    appendText(checkSumString + "\n", Colors.Gray);  // Checksum
                    break;
                default:
                    appendText(data + "\n", Colors.LawnGreen);
                    break;
            }
            return intelHexFileLine;
        }

        public async void writeHexFile(byte[] line)
        {
            string test = "AB33";

            StorageFile file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync("myData.txt", CreationCollisionOption.ReplaceExisting);
            Debug.WriteLine(file.Path.ToString());
            await FileIO.AppendTextAsync(file, test);

        }

        private string getStringFromByteList(List<byte> byteList)
        {
            string str = "";
            byte[] byteArray = byteList.ToArray();
            for(int i = 0; i < byteArray.Length; i++)
            {
                str += byteArray[i].ToString("X4");
            }

            return str;
        }

        public int getCheckSumFromLine(string line)
        {
            // 1. Remove start character.
            // 2. Split the line into array of char pairs (e.g., "FFAC" -> { "FF", "AC" })
            // 3. Convert HEX string pairs to Int32, then cast as byte.
            // 4. Sum all bytes for the line.
            // 5. Take the two's complement.
            // 6. Return checksum.

            byte checkSum = 0;
            int halfLength = (line.Length / 2);
            int[] returnBuffer = new int[halfLength];
            string[] splitByTwoData = new string[halfLength];

            line = line.Replace(":", "");
            for (int i = 0; i < halfLength; i++)
            {
                splitByTwoData[i] = line.Substring((i * 2), 2);
            }
            for (int i = 0; i < halfLength; i++)
            {
                checkSum += (byte)Convert.ToInt32(splitByTwoData[i], 16);
            }
            checkSum = (byte)(~checkSum + 1);

            return checkSum;
        }

        public async void uploadFileToChip()
        {
            // 1. Open Intel HEX file.
            // 2. Read file into byte array.
            // 3. Print out the data.
            // 4. Write data to flash.

            //byte[] bytesFromFile = intelHexFileHandler.intelHexFileToArray(filePath, pageSize);

            //int[] intsFromFile = new int[intelHexFileToUpload.Length];
            //for (int i = 0; i < intelHexFileToUpload.Length; i++)
            //{
            //    intelHexFileToUpload[i] = intelHexFileToUpload[i];
            //}
            byte[] fileToUpload = await readHexFile();
            if(fileToUpload != null)
            {
                intelHexFileToUpload.AddRange(fileToUpload);
                parseAndPrintRawRead(intelHexFileToUpload);
                //writeDataToFlash(intsFromFile);
            }


        }

        //public bool writeDataToFlash(List<byte> dataToWrite)
        //{
        //    // 1. Send Flash write character.
        //    // 2. Get response and check for RQ ('?').
        //    // 3. Write page of data.
        //    // 4. Wait and check for RQ ('?') or CF ('!').
        //    // 5. Repeat steps 3-4 until last page.
        //    // 6. Write RQ ('?').
        //    // 7. Wait and check for CF ('!').
        //    // 8. Return true if process successful.

        //    appendText("\n\n\nWrite in progress: \nPlease do not disconnect device or exit the application.\n", Color.Yellow);
        //    scrollToBottomOfTerminal();



        //    int pagesToWrite = dataToWrite.Length / pageSize;

        //    serialPorts.WriteData(commandsAsStrings[(int)commands.writeFlash]);
        //    Thread.Sleep(1200);

        //    string readyForData = serialPorts.ReadExistingAsString();

        //    if (readyForData.Contains("?"))
        //    {
        //        for (int i = 0; i < pagesToWrite; i++)
        //        {
        //            // From byte array to string
        //            string stringToWrite = getStringFromIntBytes(dataToWrite.Skip(i * pageSize).Take(pageSize).ToArray());
        //            serialPorts.WriteData(commandsAsStrings[(int)commands.confirm]);
        //            Thread.Sleep(100);
        //            serialPorts.WriteData(stringToWrite);
        //            Thread.Sleep(500);
        //            readyForData = serialPorts.ReadExistingAsString();
        //            if (readyForData.Contains("!"))
        //            {
        //                appendText("ERROR writing Page #" + i + "\n", Color.Red);
        //                return false;
        //            }
        //            appendText("Page #" + i + " ", Color.Yellow);
        //            appendText("OK.\n", Color.LawnGreen);
        //            scrollToBottomOfTerminal();
        //        }
        //        serialPorts.WriteData(commandsAsStrings[(int)commands.request]);
        //        Thread.Sleep(100);
        //        readyForData = serialPorts.ReadExistingAsString();
        //        if (readyForData.Contains("!"))
        //        {
        //            scrollToBottomOfTerminal();
        //            appendText("\nThe file ", Color.LawnGreen);
        //            appendText(fileName, Color.Yellow);
        //            appendText(" was written succesfully!", Color.LawnGreen);
        //            scrollToBottomOfTerminal();
        //            return true;
        //        }

        //    }

        //    return true;
        //}

        public string getStringFromBytes(byte[] bytes)
        {
            string str = "";

            for(int i = 0; i < bytes.Length; i++)
            {
                str += bytes[i].ToString("X2");
            }
            return str;
        }

        public string getStringFromIntBytes(int[] bytes)
        {

            string str = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                str += Convert.ToChar(bytes[i]);
            }

            return str;
        }

        double map(double x, double in_min, double in_max, double out_min, double out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        public async Task selectFileToRead()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".hex");

            try
            {
                hexFileToRead = await openPicker.PickSingleFileAsync();
                txbOpenFilePath.TextTrimming = TextTrimming.CharacterEllipsis;
                txbOpenFilePath.Text = hexFileToRead.Path;
                if (hexFileToRead.Path != "")
                {
                    appendText("File selected to upload: ", Colors.Yellow);
                    appendText(hexFileToRead.Name + "\n", Colors.LawnGreen);
                }
                else
                {
                    appendText("Open HEX File Operation cancelled.", Colors.Crimson);
                }
            } catch (NullReferenceException)
            {
                appendText("No file selected.\n", Colors.Crimson);
            }

        }

        public async Task<byte[]> readHexFile()
        {
            if (hexFileToRead == null)
            {
                appendText("No HEX File selected.\n", Colors.Crimson);
            }
            else
            {
                // Code borrowed from SO: 
                // http://stackoverflow.com/questions/34583303/how-to-read-a-text-file-in-windows-universal-app
                using (var inputStream = await hexFileToRead.OpenReadAsync())
                using (var classicStream = inputStream.AsStreamForRead())
                {
                    byte[] intelHexFileAsByteArray = intelHexFileHandler.intelHexFileToArray(classicStream, pageSize);
                    return intelHexFileAsByteArray;
                }
            }
            return null;
        }

    } // End TSB Class

    class IntelHexFile
    {

        public byte[] intelHexFileToArray(Stream fileName, int pageSize)
        {
            // 1. Open file for file info.
            // 2. Get number of lines in file.
            // 3. Get number of bytes in file.
            // 3. Close and reopen the file for reading.
            // 4. Get the number of bytes needed so all pages to write are full.
            // 5. Get a byte array sized for number of needed pages (pageSize * neededPages).
            // 6. Open file for reading.
            // 7. Loop through lines in file, filling line buffer.
            // 8. If a line of data was found...
            // 9. Get the beginning address of the line.
            // 10. Loop through char in line buffer.
            // 11. If number of full lines is less than total bytes, then fill grab data from line.
            // 12. Otherwise, pad line in the byte array with 0xFF until end of line and get position.
            // 13. Find the difference between position and end of full page.
            // 14. If page is not filled, fill with 0xFF.
            // 15. Return the byte array filled with extracted data.

            // Peek.
            Stream fullReadStream = fileName;
            StreamReader fileToGetNumberOfLines = new StreamReader(fileName);
            Tuple<int, int> numberOfBytesAndLines = linesInFile(fileToGetNumberOfLines);
            int numberOfBytesInFile = numberOfBytesAndLines.Item1;
            int numberOfLinesInFile = numberOfBytesAndLines.Item2;
            fileToGetNumberOfLines.BaseStream.Seek(0, SeekOrigin.Begin);

            // Buffer.
            int neededPages = getNeededPagePadding(numberOfBytesInFile, pageSize);
            byte[] dataFromFile = new byte[neededPages * pageSize];

            // Read.
            StreamReader fileStream = new StreamReader(fullReadStream);
            byte[] bytesThisLine = new byte[16];
            Tuple<byte[], Int16> lineOfDataAndAddress = new Tuple<byte[], Int16>(null, 0);
            int indexOfLastDataLine = 0;



            // Iterate
            for (int lineIndex = 0; lineIndex < numberOfLinesInFile; lineIndex++)
            {
                lineOfDataAndAddress = readLineFromHexFile(fileStream);
                if (lineOfDataAndAddress.Item1 != null)
                {
                    Int16 startAddressOfLine = lineOfDataAndAddress.Item2;
                    for (int byteIndex = 0; byteIndex < lineOfDataAndAddress.Item1.Length; byteIndex++)
                    {
                        if ((byteIndex + lineIndex * 16) < numberOfBytesInFile)
                        { dataFromFile[byteIndex + startAddressOfLine] = lineOfDataAndAddress.Item1[byteIndex]; }
                        else
                        {
                            dataFromFile[byteIndex + startAddressOfLine] = 0xFF;
                            indexOfLastDataLine = (byteIndex + startAddressOfLine);
                        }
                    }
                }
            }

            // Pad page.
            int blankBytesToFill = (neededPages * pageSize) - indexOfLastDataLine;
            for (int i = 0; i < blankBytesToFill; i++)
            {
                dataFromFile[i + indexOfLastDataLine] = 0xFF;
            }
            return dataFromFile;
        }

        public Tuple<byte[], Int16> readLineFromHexFile(StreamReader fileStream)
        {
            // 1. Get a line from file.
            // 2. Remove the start character
            // 3. Get byte count and convert to byte, then to int.
            // 4. Get both address bytes, convert to Int16.
            // 5. Get data type, return null if not data.
            // 6. Loop through the data extracting a line of bytes.
            //    UNIMP: Checksum
            // 7. Return line of bytes and Int16 address a tuple.

            int parseLineIndex = 0;

            //To hold file hex values.
            int dataByteCount = 0;
            byte data_address1 = 0x00;
            byte data_address2 = 0x00;
            UInt16 fullDataAddress = 0x00;
            Int16 fullAddressAsInt = 0;
            byte data_record_type = 0x00;
            byte data_check_sum = 0x00;

            string line = "";
            line = fileStream.ReadLine();

            // Skip start code.
            parseLineIndex++;

            // Get byte count and convert to int.
            string byteCountStrBfr = line.Substring(parseLineIndex, 2);
            dataByteCount = getByteFrom2HexChar(byteCountStrBfr);
            parseLineIndex += 2;

            // Create the byte array for the read about to be read.
            byte[] bytesFromLine = new byte[dataByteCount];

            // Get data address and convert to memory address.
            string byteDataAddressStrBfr = line.Substring(parseLineIndex, 2);
            data_address1 = getByteFrom2HexChar(byteDataAddressStrBfr);
            parseLineIndex += 2;

            byteDataAddressStrBfr = line.Substring(parseLineIndex, 2);
            data_address2 = getByteFrom2HexChar(byteDataAddressStrBfr);
            parseLineIndex += 2;

            fullDataAddress = (UInt16)((data_address1 << 8) | data_address2);
            fullAddressAsInt = (Int16)fullDataAddress;

            // Data type.
            string dataRecordTypeStrBfr = line.Substring(parseLineIndex, 2);
            data_record_type = getByteFrom2HexChar(dataRecordTypeStrBfr);
            parseLineIndex += 2;

            // If not data, don't bother and return false.
            if (data_record_type != 0x00) { return new Tuple<byte[], Int16>(null, 0); }

            // Get the data.
            int dataIndex = 0;
            string dataStrBfr = "";
            while (dataIndex < dataByteCount)
            {
                dataStrBfr = line.Substring(parseLineIndex, 2);
                parseLineIndex += 2;
                bytesFromLine[dataIndex] = getByteFrom2HexChar(dataStrBfr);
                dataIndex++;
            }

            // Get checksum
            // IF CHECKSUM NEEDED, GET LATER.

            Debug.Write(
               "\nByte Count: " + dataByteCount.ToString("X2") +
               "  Full address: "+ fullAddressAsInt.ToString("X4") +
               "  Record type: " + data_record_type.ToString("X2") +
               "  Data: "
               );
               for (int i = 0; i < bytesFromLine.Length; i++)
               {
                   Debug.Write(bytesFromLine[i].ToString("X2"));
               }

            return new Tuple<byte[], Int16>(bytesFromLine, fullAddressAsInt);
        }

        private Tuple<int, int> linesInFile(StreamReader file)
        {
            // 1. Read initial line.
            // 2. If line is null return empty tuple.
            // 3. loop through lines
            // 4. If the line is data
            // 5. Get how many bytes of data and add it to a running count.
            // 6. Increment line counter
            // 7. Read next line.
            // 8. Continue until EOF.
            // 9. Return bytes of data and number of lines in file.

            string line = "";
            int lineCount = 0;
            int dataBytes = 0;

            line = file.ReadLine();
            while (line != null)
            {
                if (line.Substring(7, 2) == "00")
                {
                    dataBytes += getByteFrom2HexChar(line.Substring(1, 2));
                    lineCount++;
                }
                line = file.ReadLine();
            }
            return new Tuple<int, int>(dataBytes, lineCount);
        }

        public byte getByteFrom2HexChar(string twoHexChars)
        {
            return (byte)Convert.ToInt32(twoHexChars, 16);
        }

        public int getNeededPagePadding(int byteCount, int pageSize)
        {
            // 1. Find out if pageSize divides byteCount with no remainder.  If so, return 0.
            // 2. Else, get the number of pages with padding.
            // 3. Get total bytes with padding.
            // 4. Find number of padding bytes needed.
            // 5. Return how many padding bytes are required to make the last page full.

            if (byteCount % pageSize == 0)
            {
                return 0;
            }
            else
            {
                return ((int)Math.Floor((float)byteCount / (float)pageSize) + 1);
            }
            return 0;
        }


    }// End Intel Hex File Class


}
