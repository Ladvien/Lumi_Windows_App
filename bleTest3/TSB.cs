﻿using System;
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
using Lumi;
using Windows.UI.Xaml;
using System.Collections;
using Windows.Storage;
using System.IO;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.Foundation;

namespace Lumi
{
    public class TSB
    {

        int millsecondsDelayForBLEReset = 400;

        public void printCommandInProgress()
        {
            Debug.WriteLine(commandInProgress.ToString());
        }


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
            none,
            hello,
            request,
            confirm,
            readFlash,
            writeFlash,
            readEEPROM,
            writeEEPROM,
            readUserData,
            writeUserData,
            bleReset,
            bleResetSuccess,
            bleRelease,
            bleReleaseSuccess,
            blePrepareHello,
            bleHello,
            helloProcessing,
            error
        }

        public static string[] commandsAsStrings =
        {
            "",           // 0
            "@@@",        // 1
            "?",          // 2
            "!",          // 3
            "f",          // 4
            "F",          // 5
            "e",          // 6
            "E",          // 7
            "c",          // 8
            "C",          // 9
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
            uknown,
            connected,
            writeFail,
            readFail,
            writeSuccessful,
            readSuccessful,
            downloadSuccessful,
            uploadSuccessful,
            displayMessage,
            bootloaderDisconnected,
            wirelessReleaseSuccess,
            error
        }

        
        public enum OTAType: int
        {
            none = 0,
            hm1x = 1,
            esp = 2
        }
        OTAType OTASelected = new OTAType();

        public enum device: int
        {
            none = 0,
            serial = 1,
            hm1x = 2,
            esp8266 = 3
        }
        device deviceSelected = new device();

        #endregion enumerations

        #region properties
        public delegate void TsbUpdateCommand(statuses tsbConnectionStatus, Run message = null);
        public event TsbUpdateCommand TsbUpdatedCommand;

        public SerialBuffer readFlashBuffer = new SerialBuffer();
        public SerialBuffer readFlashBufferTmp = new SerialBuffer();

        public IntelHexFile intelHexFileHandler = new IntelHexFile();
        public List<byte> intelHexFileToUpload;
        int uploadPageIndex = 0;

        const int commandAttempts = 3;

        string filePath = @"C:\Users\cthom\Documents\myFileTest.txt";
        string fileName = "";

        StorageFile hexFileToRead;

        // Firmware date.
        public string firmwareDateString;

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

        DEVICE_SIGNATURE deviceSignatureValue = new DEVICE_SIGNATURE();
        public commands commandInProgress = new commands();
        displayFlash displayFlashType = displayFlash.asIntelHexFile;

        serialPortsExtended serialPorts;
        Paragraph theOneParagraph;
        ProgressBar progressBar;
        TextBlock txbOpenFilePath;
        RichTextBlock mainDisplay;
        ScrollViewer mainDisplayScrollView;


        public enum HM1X_Pin : int
        {
            PIO0 = 0,
            PIO1 = 1,
            PIO2 = 2,
            PIO3 = 3,
            PIO4 = 4,
            PIO5 = 5,
            PIO6 = 6,
            PIO7 = 7,
            PIO8 = 8,
            PIO9 = 9,
            PIOA = 10,
            PIOB = 11
        }
        public HM1X_Pin pin = new HM1X_Pin();
        private string resetPinStr = "";

        Dictionary<HM1X_Pin, string> resetPinDictionary = new Dictionary<HM1X_Pin, string>
            {
                {HM1X_Pin.PIO0, "PIO0"},
                {HM1X_Pin.PIO1, "PIO1"},
                {HM1X_Pin.PIO2, "PIO2"},
                {HM1X_Pin.PIO3, "PIO3"},
                {HM1X_Pin.PIO4, "PIO4"},
                {HM1X_Pin.PIO5, "PIO5"},
                {HM1X_Pin.PIO6, "PIO6"},
                {HM1X_Pin.PIO7, "PIO7"},
                {HM1X_Pin.PIO8, "PIO8"},
                {HM1X_Pin.PIO9, "PIO9"},
                {HM1X_Pin.PIOA, "PIOA"},
                {HM1X_Pin.PIOB, "PIOB"}
            };

        #endregion properties

        public delegate void CallBackEventHandler(object sender, EventArgs args);
        public event CallBackEventHandler Callback;
        // 
        public SerialBuffer serialBuffer = new SerialBuffer();
        // When a write command is sent, then timeout timer is started.
        public DispatcherTimer writeTimer = new DispatcherTimer();
        private DispatcherTimer resetTimer = new DispatcherTimer();

        

        public List<byte> readFlashBfr = new List<byte>();

        public void init(serialPortsExtended serialPortMain, ScrollViewer _mainDisplayScrollView,RichTextBlock _rtbMainDisplay, Paragraph _theOneParagraph, ProgressBar mainProgressBar, SerialBuffer _serialBuffer, TextBlock _openFilePath)
        {
            try
            {
                rxByteArray = new List<byte>();
                intelHexFileToUpload = new List<byte>();
                serialPorts = serialPortMain;
                theOneParagraph = _theOneParagraph;
                progressBar = mainProgressBar;
                serialBuffer = _serialBuffer;
                txbOpenFilePath = _openFilePath;
                mainDisplay = _rtbMainDisplay;
                mainDisplayScrollView = _mainDisplayScrollView;

                // Write timeout timer.
                writeTimer.Tick += writeTimer_Tick;

                serialBuffer.RXbufferUpdated += new SerialBuffer.CallBackEventHandler(RXbufferUpdated);
                serialBuffer.TXbufferUpdated += new SerialBuffer.CallBackEventHandler(TXbufferUpdated);
                readFlashBuffer.bufferUpdated += new SerialBuffer.CallBackEventHandler(ReadFlashBuffer_bufferUpdated);
            } catch
            {
                Debug.WriteLine("TSB.init() failed");
            }

        }



        private void ReadFlashBuffer_bufferUpdated(object sender, EventArgs args)
        {
            
        }

        public void startWriteTimeoutTimer(int seconds)
        {

            //var ignored = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            //() =>
            //{
            //    writeTimer.Interval = new TimeSpan(0, 0, 0, seconds);
            //    writeTimer.Start();
            //});
        }

        public void writeTimerStop()
        {
            //IAsyncAction ignored;
            //ignored = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() =>
            //{
            //    writeTimerStop();
            //});

        }


        public void startResetTimer(int seconds, int milliseconds)
        {

            //var ignored = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            //() =>
            //{
            //    resetTimer.Interval = new TimeSpan(0, 0, 0, seconds, milliseconds);
            //    resetTimer.Start();
            //});

        }

        private void writeTimer_Tick(object sender, object e)
        {
            //commandInProgress = commands.error;
            //RXbufferUpdated(this, null);
            //writeTimerStop();
        }

        private void TXbufferUpdated(object sender, EventArgs args)
        {
            
        }

        public void setResetPin(int _pin)
        {
            try
            {
                pin = (HM1X_Pin)_pin;
                if ((int)pin > -1)
                {
                    resetPinStr = resetPinDictionary[pin];
                }
                else
                {
                    resetPinStr = "";
                }
            }
            catch
            {
                Debug.WriteLine("TSB.setResetPin failed");
            }

        }

        public ComboBox populateResetPinCmbBox(ComboBox _comboBox)
        {
            try
            {
                _comboBox.ItemsSource = resetPinDictionary.Values;
                _comboBox.SelectedIndex = 0;
                return _comboBox;
            }
            catch
            {
                Debug.WriteLine("TSB.populateResetPinCmbBox failed");
                return null;
            }

        }

        public string getResetPinAsString()
        {
            try
            {
                return resetPinStr;
            } catch
            {
                Debug.WriteLine("TSB.getResetPinAsString failed");
                return null;
            }
        }

        private async void RXbufferUpdated(object sender, EventArgs args)
        {
            // 1. Route to command-in-progress.
            //IAsyncAction ignored;
            //ignored = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            //() =>
            //{
            //    writeTimerStop();
            //});
            switch (commandInProgress)
            {
                case commands.error:
                    try
                    {
                        byte[] rxData;
                        string str = "";
                        rxData = serialBuffer.readAllBytesFromRXBuffer();
                        str = getAsciiStringFromByteArray(rxData);
                        displayMessage("Uh-oh. Bad stuff happened.\n" + str, Colors.Crimson);
                        TsbUpdatedCommand(statuses.error);
                    } catch
                    {
                        Debug.WriteLine("RXBufferUpdated, case commands.error failed");
                    }
                    break; 
                case commands.hello:
                    try
                    {
                        bool outcome = await helloProcessing();
                        if (outcome)
                        {
                            TsbUpdatedCommand(statuses.connected);
                            // Whatever says we are successful.
                        }
                        else
                        {
                            displayMessage("Failed to connect to TinySafeBoot", Colors.Crimson);
                        }
                    } catch
                    {
                        Debug.WriteLine("RXBufferUpdated, case commands.hello failed");
                    }

                    break;
                case commands.readFlash:
                    try
                    {
                        processFlashRead();
                    } catch
                    {
                        Debug.WriteLine("RXBufferUpdated, case commands.readFlash failed");
                    }
                    
                    break;
                case commands.writeFlash:
                    try
                    {
                        var writeResponse = writeDataToFlash();
                        if (writeResponse)
                        {
                            Debug.WriteLine("Yay, there's much rejoicing.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("RXBufferUpdated, case commands.readFlash failed");
                    }
                    break;
                case commands.bleReset:
                    try
                    {
                        helloRouting();
                    } catch
                    {
                        Debug.WriteLine("RXBufferUpdated, case commands.bleReset failed");
                    }
                    
                    break;
                case commands.bleRelease:
                    try
                    {
                        checkReleaseSuccess();
                    } catch
                    {
                        Debug.WriteLine("RXBufferUpdated, case commands.bleReset failed");
                    }
                    break;
                case commands.bleResetSuccess:
                    //TsbUpdateCommand(statuses.)
                    break;
                case commands.bleReleaseSuccess:
                    try
                    {
                        TsbUpdatedCommand(statuses.wirelessReleaseSuccess);
                    } catch
                    {
                        Debug.WriteLine("RXBufferUpdated, case commands.bleReleaseSuccess failed");
                    }
                    break;
                case commands.bleHello:
                    
                    break;
                case commands.blePrepareHello:
                    try
                    {
                        helloRouting();
                    } catch
                    {
                        Debug.WriteLine("RXBufferUpdated, case commands.bleReleaseSuccess failed");
                    }
                    break;
                case commands.helloProcessing:
                    try
                    {
                        await helloProcessing();
                    } catch
                    {
                        Debug.WriteLine("RXBufferUpdated, case commands.helloProcessing failed");
                    }
                    break;
                default:
                    Debug.WriteLine("Defaulted in RXbuffer switch\n");
                    break;
            }
        }

        public void setOTADevice(OTAType device)
        {
            try
            {
                OTASelected = device;
            } catch
            {
                Debug.WriteLine("TSB.setOTADevice() failed");
            }
            
        }

        public void setDevice(device _device)
        {
            try
            {
                deviceSelected = _device;
            } catch
            {
                Debug.WriteLine("TSB.setDevice() failed");
            }
            
        }

        public void scrollToBottomOfTerminal()
        {
            try
            {
                mainDisplayScrollView.ScrollToVerticalOffset(mainDisplay.ContentEnd.Offset + 50);
            } catch
            {
                Debug.WriteLine("TSB.scrollToBottomOfTerminal() failed");
            }
            
        }

        public void setFilePath(string path)
        {
            try
            {
                filePath = path;
            } catch
            {
                Debug.WriteLine("TSB.setFilePath() failed");
            }
            
        }

        public void setFileName(string name)
        {
            try
            {
                fileName = name;
            } catch
            {
                Debug.WriteLine("TSB.setFileName() failed");
            }
            
        }

        public string commandString(commands commandNumber)
        {
            try
            {
                // 1. Return the command string.
                return commandsAsStrings[(int)commandNumber];
            } catch
            {
                Debug.WriteLine("TSB.commandString() failed");
                return "";
            }

        }

        public void updateActionInProgress(commands commandNumber)
        {
            try
            {
                commandInProgress = commandNumber;
            } catch
            {
                Debug.WriteLine("TSB.updateActionInProgress() failed");
            }
            
        }

        public void setFlashDisplay(displayFlash displayFlashTypeArgument)
        {
            try
            {
                displayFlashType = displayFlashTypeArgument;
            } catch
            {
                Debug.WriteLine("TSB.setFlashDisplay() failed");
            }
            
        }

        public string getAsciiStringFromByteArray(byte[] byteArray)
        {
            string str = "";
            try
            {
                for (int i = 0; i < byteArray.Length; i++)
                {
                    str += (char)byteArray[i];
                }
                return str;
            } catch
            {
                Debug.WriteLine("TSB.getAsciiStringFromByteArray() failed");
                return "";
            }
        }

        public void hello()
        {
            Debug.WriteLine(deviceSelected);
            switch (deviceSelected)
            {
                case device.serial:
                    try
                    {
                        helloRouting();
                    } catch
                    {
                        Debug.WriteLine("TSB.hello, device.serial failed");
                    }
                    break;
                case device.hm1x:
                    try
                    {
                        commandInProgress = commands.blePrepareHello;
                        serialBuffer.txBuffer = GetBytes("AT+" + getResetPinAsString() + "0");
                    }
                    catch
                    {
                        Debug.WriteLine("TSB.hello, device.serial failed");
                    }
                    break;
            }            
        }

        public void helloRouting()
        {
            switch (OTASelected)
            {
                case OTAType.none:
                    try
                    {
                        startWriteTimeoutTimer(1);
                        commandInProgress = commands.helloProcessing;
                        serialBuffer.txBuffer = getCommand(commands.hello);
                    } catch
                    {
                        Debug.WriteLine("TSB.hellRouting(), OTAType.none failed");
                    }
                    break;
                case OTAType.hm1x:
                    bool success = bleResetAssert();
                    switch (commandInProgress)
                    {
                        case commands.bleReset:
                            try
                            {
                                if (true == success)
                                {
                                    commandInProgress = commands.bleResetSuccess;
                                    TsbUpdatedCommand(statuses.wirelessReleaseSuccess);
                                }
                            } catch
                            {
                                Debug.WriteLine("TSB.hellRouting(), OTAType.hm1x, commands.Reset failed");
                            }                      

                            break;
                        case commands.blePrepareHello:
                            try
                            {
                                if (true == success)
                                {
                                    commandInProgress = commands.helloProcessing;
                                    serialBuffer.txBuffer = getCommand(commands.hello);
                                }
                            } catch
                            {
                                Debug.WriteLine("TSB.hellRouting(), OTAType.hm1x, commands.blePrepareHello failed");
                            }
                            break;
                    }
                    break;
                case OTAType.esp:
                    displayMessage("ESP8266 is not yet implemented.", Colors.Crimson);
                    break;
                default:
                    displayMessage("Uh-oh.  There was a problem selecting a device for TSB handshake.", Colors.Crimson);
                    break;
            }

        }

        public bool bleResetAssert()
        {
            try
            {
                byte[] rxData = serialBuffer.readAllBytesFromRXBuffer();
                string str = getAsciiStringFromByteArray(rxData);
                if (str.Contains("OK+" + getResetPinAsString() + ":0"))
                {
                    //startResetTimer(0, 50);
                    serialBuffer.txBuffer = GetBytes("AT+" + getResetPinAsString() + "1");
                    return false;
                }
                else if (str.Contains("OK+" + getResetPinAsString() + ":1"))
                {
                    return true;
                }
            } catch
            {
                Debug.WriteLine("TSB.bleResetAssert() failed");
            }
            return false;
        }


        public void wirelessReset()
        {
            try
            {
                commandInProgress = commands.bleRelease;
            } catch
            {
                Debug.WriteLine("TSB.wirelessReset() failed");
            }
        }

        public void remoteResetInit()
        {
            try
            {
                updateActionInProgress(TSB.commands.bleReset);
                serialBuffer.txBuffer = GetBytes("AT+" + getResetPinAsString() + "0");
                resetTimer.Interval = new TimeSpan(0, 0, 0, 0, millsecondsDelayForBLEReset);
                resetTimer.Tick += ResetTimer_Tick;
                resetTimer.Start();
            } catch
            {
                Debug.WriteLine("TSB.remoteResetInit() failed");
            }

        }
        private void ResetTimer_Tick(object sender, object e)
        {
            try
            {
                resetTimer.Stop();

                serialBuffer.txBuffer = GetBytes("AT+" + getResetPinAsString() + "1");
                updateActionInProgress(TSB.commands.none);
            } catch
            {
                Debug.WriteLine("TSB.ResetTimer_Tick() failed");
            }
            if(resetTimer.IsEnabled == true)
            {
                // Without this line then the timer ticks several more times before stopped.  Weird.
                resetTimer = null;
            }
            
        }

        public void checkReleaseSuccess()
        {
            byte[] rxData;
            string str = "";
            try
            {
                rxData = serialBuffer.readAllBytesFromRXBuffer();
                str = getAsciiStringFromByteArray(rxData);
                if (str.Contains("OK+" + getResetPinAsString() + ":0"))
                {

                }
                else if (str.Contains("OK+" + getResetPinAsString() + ":1"))
                {
                    TsbUpdatedCommand(statuses.wirelessReleaseSuccess, null);
                }
            } catch
            {
                Debug.WriteLine("TSB.checkReleaseSuccess() failed");
            }
            
        }

        public async Task<bool> helloProcessing()
        {

            // 1. Try handshake ("@@@") three times; or continue if successful.
            // 2. Check if reply seems valid(ish).  
            // 3. Chop up the reply into useful device data.
            // 4. Save the device data for later.
            // 5. If not reply, let the user know it was a fail.
            byte[] rxData;
            string str = "";
            rxData = serialBuffer.readAllBytesFromRXBuffer();
            str = getAsciiStringFromByteArray(rxData);
            if (serialBuffer.bytesInRxBuffer() > 16)
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

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        //displayMessage(tsbHanshakeInfo, Colors.White);
                        writeTimerStop();
                        TsbUpdatedCommand(statuses.connected, (getRun(tsbHanshakeInfo, Colors.White)));
                    });

                    commandInProgress = commands.none;
                    //setTsbConnectionSafely(true);
                    return true;
                }
            }
            else
            {
                string error = "Could not handshake with TSB. Please reset and try again.\n";
                Debug.WriteLine(error);
                return false;
            }
            return false;
        }

        public SolidColorBrush getColoredBrush(Color color)
        {
            try
            {
                return new SolidColorBrush(color);
            } catch
            {
                Debug.WriteLine("TSB.getColorBrush() failed");
                return null;
            }
            
        }
        

        public byte[] getCommand(commands command)
        {
            try
            {
                string cmdStr = commandsAsStrings[(int)command];
                byte[] commandAsByteArray = new byte[cmdStr.Length];
                for (int i = 0; i < cmdStr.Length; i++)
                {
                    commandAsByteArray[i] = (byte)cmdStr[i];
                }
                return commandAsByteArray;
            } catch
            {
                Debug.WriteLine("TSB.getCommand failed");
                return null;
            }

        }
        
        public void readFlash()
        {
            try
            {
                // 1. Set readFlash as the commandInProgress
                // 2. Write read Flash command.
                commandInProgress = commands.readFlash;
                //await serialPorts.write(commandsAsStrings[(int)commands.readFlash]);
                //await serialPorts.write(commandsAsStrings[(int)commands.confirm]);
                serialBuffer.txBuffer = getCommand(commands.readFlash);
                serialBuffer.txBuffer = getCommand(commands.confirm);
            } catch
            {
                Debug.WriteLine("TSB.readFlash() failed");
            }

        }

        public void processFlashRead() {

            // 1. Read RX buffer into a temporary buffer.
            // 2. If the temporary buffer is smaller than a page, exit method.
            // 3. If the larger than a page...
            // 4. Update the progress bar, using the main thread.
            // 5. Move the bytes received so far into the rxByteArray (full read-out buffer).
            // 6. Check if this is the last page, if so parse and print the read-out buffer.
            // 7. If not, request another page.
            // 8. Clear the temporary buffer.

            try
            {
                IAsyncAction ignored;

                readFlashBfr.AddRange(serialBuffer.readAllBytesFromRXBuffer());
                if (readFlashBfr.Count < pageSize) { return; }

                var currentProgressBarValue = 1000 * ((float)rxByteArray.Count / (float)flashSize);
                ignored = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    progressBar.Value = currentProgressBarValue;
                });

                rxByteArray.AddRange(readFlashBfr.ToArray());
                if (dogEarCheck(readFlashBfr.ToArray()))
                {
                    ignored = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        parseAndPrintRawRead(rxByteArray);
                        progressBar.Value = 100;
                    });
                    return;
                }
                else if (readFlashBfr.Count >= pageSize)
                {
                    serialBuffer.txBuffer = getCommand(commands.confirm);
                }
                readFlashBfr.Clear();
            } catch
            {
                Debug.WriteLine("TSB.processReadFlash() failed");
            }

        }

        private bool dogEarCheck(byte[] byteArray)
        {
            // 1. If null, return false.
            // 2. Check the last two bytes in a page to see if they are blank (0xFF).
            try
            {
                if (byteArray == null) { return false; }
                // Check to see what the if the page is dogeared.
                if (byteArray[byteArray.Length - 1] == 0xFF && byteArray[byteArray.Length - 2] == 0xFF)
                { return true; }
                else
                { return false; }
            } catch
            {
                Debug.WriteLine("TSB.dogEarCheck() failed");
                return false;
            }
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

            try
            {
                int numberOfPagesRead = (rawFlashRead.Count / pageSize);
                int pageDepth = (pageSize / 16);
                const int pageWidth = 16;
                string lineBuffer = "";

                byte[] byteArray = rawFlashRead.ToArray();

                displayMessage("\nFlash readout for " + deviceSignatureValue + "\n\n", Colors.White);

                for (int i = 0; i < numberOfPagesRead; i++)
                {
                    if (displayFlashType != displayFlash.none)
                    { displayMessage("\n\t Page #:" + i + "\n", Colors.Yellow); }
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
                    var currentProgressBarValue = 100 * ((float)i + 1 / (float)numberOfPagesRead);
                    //progressBar.Value = map(currentProgressBarValue, 0, 100, 50, 100);
                }

                scrollToBottomOfTerminal();
                //outputFile.Close();
            } catch
            {
                Debug.WriteLine("TSB.parseAndPrintRawRead() failed");
            }

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

            try
            {
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
                        displayMessage(data + "\n", Colors.LawnGreen);
                        break;
                    case displayFlash.addressAndData:
                        displayMessage(address + ": ", Colors.Yellow);
                        displayMessage(data + "\n", Colors.LawnGreen);
                        break;
                    case displayFlash.asIntelHexFile:
                        displayMessage(":", Colors.Yellow);                  // Start code
                        displayMessage(byteCount, Colors.Green);             // Byte count
                        displayMessage(address, Colors.Purple);              // Address
                        displayMessage(recordType, Colors.Pink);             // Record type.
                        displayMessage(data, Colors.CadetBlue);              // Data
                        displayMessage(checkSumString + "\n", Colors.Gray);  // Checksum
                        break;
                    default:
                        displayMessage(data + "\n", Colors.LawnGreen);
                        break;
                }
                scrollToBottomOfTerminal();
                return intelHexFileLine;
            } catch
            {
                Debug.WriteLine("TSB.getIntelFileHexString() failed");
                return "";
            }

            
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
            try
            {
                string str = "";
                byte[] byteArray = byteList.ToArray();
                for (int i = 0; i < byteArray.Length; i++)
                {
                    str += byteArray[i].ToString("X4");
                }

                return str;
            } catch
            {
                Debug.WriteLine("TSB.getStringFromByteList failed");
                return "";
            }

        }

        public int getCheckSumFromLine(string line)
        {
            // 1. Remove start character.
            // 2. Split the line into array of char pairs (e.g., "FFAC" -> { "FF", "AC" })
            // 3. Convert HEX string pairs to Int32, then cast as byte.
            // 4. Sum all bytes for the line.
            // 5. Take the two's complement.
            // 6. Return checksum.

            try
            {
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
            } catch
            {
                Debug.WriteLine("TSB.getCheckSumFromLine() failed");
                return 0;
            }

        }

        public async void openFileForWritingToFlash()
        {
            // 1. Open Intel HEX file.
            // 2. Read file into byte array.
            // 3. Print out the data.
            // 4. Write data to flash.

            try
            {
                //byte[] bytesFromFile = intelHexFileHandler.intelHexFileToArray(filePath, pageSize);

                //int[] intsFromFile = new int[intelHexFileToUpload.Length];
                //for (int i = 0; i < intelHexFileToUpload.Length; i++)
                //{
                //    intelHexFileToUpload[i] = intelHexFileToUpload[i];
                //}
                byte[] fileToUpload = await readHexFile();
                if (fileToUpload != null)
                {
                    intelHexFileToUpload.AddRange(fileToUpload);
                    parseAndPrintRawRead(intelHexFileToUpload);
                    //writeDataToFlash(intsFromFile);
                }
            } catch
            {
                Debug.WriteLine("TSB.openFileForWRitingToFlash() failed");
            }


        }

        public void writeToFlash()
        {
            try
            {
                prepareToWriteToFlash();
            } catch
            {
                Debug.WriteLine("TSB.writeToFlash() failed");
            }
        }

        private void prepareToWriteToFlash()
        {
            // 1. Make sure there are data to upload.
            // 2. Set the command to write flash.
            // 3. Send the command to write ("F").
            // 4. Update the user.
            // 5. Scroll display to last line.

            try
            {
                if (intelHexFileToUpload.Count != 0)
                {
                    uploadPageIndex = 0;
                    commandInProgress = commands.writeFlash;
                    serialBuffer.txBuffer = getCommand(commands.writeFlash);
                    displayMessage("\n\n\nWrite in progress: \nPlease do not disconnect device or exit the application.\n", Colors.Yellow);
                    scrollToBottomOfTerminal();
                }
                else
                {
                    displayMessage("No file to write.\n", Colors.Crimson);
                }
            } catch
            {
                Debug.WriteLine("TSB.prepareToWriteToFlash() failed");
            }


        }

        private bool writeDataToFlash()
        {
            // 1. Send Flash write character.
            // 2. Get response and check for RQ ('?').
            // 3. Write page of data.
            // 4. Wait and check for RQ ('?') or CF ('!').
            // 5. Repeat steps 3-4 until last page.
            // 6. Write RQ ('?').
            // 7. Wait and check for CF ('!').
            // 8. Return true if process successful.

            try
            {
                IAsyncAction ignored;

                int pagesToWrite = intelHexFileToUpload.Count / pageSize;

                byte[] rxByteArray = serialBuffer.readAllBytesFromRXBuffer();

                if (rxByteArray[0] == 0x3F) // ?
                {
                    if (uploadPageIndex < pagesToWrite)
                    {
                        // From byte array to string 
                        byte[] bytesToWrite = intelHexFileToUpload.Skip(uploadPageIndex * pageSize).Take(pageSize).ToArray();

                        serialBuffer.txBuffer = getCommand(commands.confirm);
                        serialBuffer.txBuffer = bytesToWrite;

                        ignored = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            displayMessage("Page #" + uploadPageIndex + " ", Colors.Yellow);
                            displayMessage("OK.\n", Colors.LawnGreen);
                            scrollToBottomOfTerminal();
                            var currentProgressBarValue = 100 * ((float)(uploadPageIndex + 1) / (float)pagesToWrite);
                            progressBar.Value = map(currentProgressBarValue, 0, 100, 0, 100);
                            uploadPageIndex++;
                        });

                    }
                    else
                    {
                        serialBuffer.txBuffer = getCommand(commands.request);
                    }
                    return true;
                }
                else if (rxByteArray[0] == 0x21) // !
                {
                    TsbUpdatedCommand(statuses.uploadSuccessful);
                    updateActionInProgress(commands.none);
                    ignored = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        scrollToBottomOfTerminal();
                        displayMessage("\nThe file ", Colors.LawnGreen);
                        displayMessage(hexFileToRead.Name, Colors.Yellow);
                        displayMessage(" written!\n", Colors.LawnGreen);
                        scrollToBottomOfTerminal();
                        
                    });
                    return true;
                }
                else
                {
                    // Error writing to flash.
                    ignored = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        displayMessage("ERROR writing Page #" + uploadPageIndex + "\n" + "RX: " + rxByteArray[0] + "\n", Colors.Crimson);
                    });
                    return false;
                }
            } catch
            {
                Debug.WriteLine("TSB.writeDataToFlash() failed");
                return false;
            }


        }

        public string getStringFromBytes(byte[] bytes)
        {
            try
            {
                string str = "";

                for (int i = 0; i < bytes.Length; i++)
                {
                    str += bytes[i].ToString("X2");
                }
                return str;
            } catch
            {
                Debug.WriteLine("TSB.getStringFromBytes() failed");
                return null;
            }
 
        }

        public string getStringFromIntBytes(int[] bytes)
        {
            try
            {
                string str = "";
                for (int i = 0; i < bytes.Length; i++)
                {
                    str += Convert.ToChar(bytes[i]);
                }
                return str;
            } catch
            {
                Debug.WriteLine("TSB.getStringFromIntBytes() failed");
                return "";
            }
 
        }

        static byte[] GetBytes(string str)
        {
            try
            {
                byte[] bytes = new byte[str.Length];
                for (int i = 0; i < str.Length; i++)
                {
                    bytes[i] = (byte)str[i];
                }
                return bytes;
            } catch
            {
                Debug.WriteLine("TSB.GetBytes() failed");
                return null;
            }

        }

        double map(double x, double in_min, double in_max, double out_min, double out_max)
        {
            try
            {
                return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
            } catch
            {
                Debug.WriteLine("TSB.map() failed");
                return 0;
            }
            
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
                    displayMessage("File selected to upload: ", Colors.Yellow);
                    displayMessage(hexFileToRead.Name + "\n", Colors.LawnGreen);
                    openFileForWritingToFlash();
                }
                else
                {
                    displayMessage("Open HEX File Operation cancelled.", Colors.Crimson);
                }
            } catch (NullReferenceException)
            {
                displayMessage("No file selected.\n", Colors.Crimson);
            }

        }

        public async Task<byte[]> readHexFile()
        {
            try
            {
                if (hexFileToRead == null)
                {
                    displayMessage("No HEX File selected.\n", Colors.Crimson);
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
            } catch
            {
                Debug.WriteLine("TSB.readHexFile failed");
                return null;
            }

        }

        public Run getRun(string str, Color color)
        {
            try
            {
                Run r = new Run();
                r.Foreground = getColoredBrush(color);
                r.Text = str;
                return r;
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return null;
        }

        private void displayMessage(string message, Color color)
        {
            Run r = getRun(message, color);
            TsbUpdatedCommand(statuses.displayMessage, r);
        }

    } // End TSB Class

    public class IntelHexFile
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

            try
            {
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

                int numberOfBytesUpToThisLine = 0;


                // Iterate
                for (int lineIndex = 0; lineIndex < numberOfLinesInFile; lineIndex++)
                {
                    lineOfDataAndAddress = readLineFromHexFile(fileStream);
                    if (lineOfDataAndAddress.Item1 != null)
                    {
                        Int16 startAddressOfLine = lineOfDataAndAddress.Item2;
                        for (int byteIndex = 0; byteIndex < lineOfDataAndAddress.Item1.Length; byteIndex++)
                        {
                            if ((byteIndex + numberOfBytesUpToThisLine) < numberOfBytesInFile)
                            {
                                dataFromFile[byteIndex + startAddressOfLine] = lineOfDataAndAddress.Item1[byteIndex];
                                indexOfLastDataLine = (byteIndex + startAddressOfLine) + 1;
                            }
                            else
                            {
                                dataFromFile[byteIndex + startAddressOfLine] = 0xFF;
                                indexOfLastDataLine = (byteIndex + startAddressOfLine) + 1;
                            }
                        }
                    }
                    numberOfBytesUpToThisLine += lineOfDataAndAddress.Item1.Length;
                }

                // Pad page.
                int blankBytesToFill = (neededPages * pageSize) - indexOfLastDataLine;
                for (int i = 0; i < blankBytesToFill; i++)
                {
                    dataFromFile[i + indexOfLastDataLine] = 0xFF;
                }
                return dataFromFile;
            } catch
            {
                Debug.WriteLine("IntelHexFile.IntelHexFileToArray() failed");
                return null;
            }
           
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
            
            try
            {
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
                /*
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
                   */
                return new Tuple<byte[], Int16>(bytesFromLine, fullAddressAsInt);
            } catch
            {
                Debug.WriteLine("IntelHexFile.readLineFromHexFile() failed");
                return null;
            }
            
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

            try
            {
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
            } catch
            {
                Debug.WriteLine("IntelHexFile.linesInFile() failed");
                return null;
            }
            
        }

        public byte getByteFrom2HexChar(string twoHexChars)
        {
            try
            {
                if(checkValidByteInString(twoHexChars))                    
                {
                    return (byte)Convert.ToInt32(twoHexChars, 16);
                } else
                {
                    return 0x00;
                }
                
            } catch
            {
                Debug.WriteLine("IntelHexFile.geyByteFrom2HexChar() failed");
                return 0x00;
            }
            
        }

        private bool checkValidByteInString(string twoChars)
        {
            if(twoChars.Length != 2) { return false; }
            if (twoChars[0] > 70) { return false; }                         // Greater than F
            if (twoChars[1] > 70) { return false; }
            if (twoChars[0] < 48) { return false; }                         // Less than 0
            if (twoChars[1] < 48) { return false; }
            if (twoChars[0] > 57 && twoChars[0] < 65) { return false; }     // Greater than 9 but less than A
            if (twoChars[1] > 57 && twoChars[1] < 65) { return false; }

            return true;
        }

        public int getNeededPagePadding(int byteCount, int pageSize)
        {
            // 1. Find out if pageSize divides byteCount with no remainder.  If so, return 0.
            // 2. Else, get the number of pages with padding.
            // 3. Get total bytes with padding.
            // 4. Find number of padding bytes needed.
            // 5. Return how many padding bytes are required to make the last page full.

            try
            {
                if (byteCount % pageSize == 0)
                {
                    return ((int)(byteCount / pageSize) + 1);
                }
                else
                {
                    return ((int)Math.Floor((float)byteCount / (float)pageSize) + 1);
                }
            } catch
            {
                Debug.WriteLine("IntelHexFile.getNeededPagePadding() failed");
                return 0;
            }

        }


    }// End Intel Hex File Class


}
