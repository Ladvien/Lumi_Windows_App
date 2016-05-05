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
using Windows.Security.Cryptography;
using lumi;

namespace bleTest3
{
    #region credits
    // A lot of the async serial handling code was pulled from MS-IoT samples.  
    // The differences I see from serialPortsExtended for the Desktop.  The SerialDevice
    // object no longer handles read and write operations.  Instead, it has a DataStream object
    // which is then passed to a DataWriter or DataReader object.  It's a better design
    // in that it forces decoupling of the Read / Write operations.  Now, it doesn't matter
    // if it is a Bluetooth or WiFi device, as long as they all should have DataReader and DataWriter
    // objects.
    // https://github.com/ms-iot/samples/blob/develop/SerialSample/CS/MainPage.xaml.cs

    #endregion credits

    public class serialPortsExtended
    {
        #region fields
        
        public delegate void CallBackEventHandler(object sender, EventArgs args);
        public event CallBackEventHandler Callback;

        public SerialBuffer serialBuffer = new SerialBuffer();

        public Paragraph theOneParagraph;

        //DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;

        private Dictionary<string, DeviceInformation> listOfDevices = new Dictionary<string, DeviceInformation>();
        private Dictionary<string, SerialDevice> listOfPorts= new Dictionary<string, SerialDevice>();
        private CancellationTokenSource ReadCancellationTokenSource;

        // public ObservableCollection<DeviceAccessInformation> aqsList = new ObservableCollection<DeviceAccessInformation>();
        public DeviceInformationCollection dis;
        #endregion fields

        #region properties
        public SerialDevice _selectedSerialDevice;
        public SerialDevice selectedSerialDevice
        {
            set { _selectedSerialDevice = value; }
            get { return _selectedSerialDevice; }
        }

        public class selectedSerialDeviceAttributes
        {
            public string comPort;
            public UInt32 baudRate;
            public ushort dataBits;
            public SerialStopBitCount stopBits;
            public SerialParity parity;
            public SerialHandshake handshake;
        }
        public selectedSerialDeviceAttributes selectedDeviceAttributes = new selectedSerialDeviceAttributes();

        private SerialDevice _serialDeviceItem;
        public SerialDevice serialDeviceItem
        {
            set { _serialDeviceItem = value; }
            get { return _serialDeviceItem; }
        }

        private byte[] _rxBuffer;
        byte[] rxBuffer
        {
            set {
                    IBuffer buffer = CryptographicBuffer.CreateFromByteArray(value);
                    CryptographicBuffer.CopyToByteArray(buffer, out _rxBuffer);
                }
            get { return _rxBuffer; }

        }

        public byte[] getBytes(int numberOfBytes)
        {
            // 1. Get the characters to return: Range<0, numberOfBytes>
            // 2. Remove the number of bytes from the buffer.
            // 3. Return the wanted bytes.
            if(rxBuffer.Length > 0)
            {
                byte[] returnBytes = rxBuffer.Take(numberOfBytes).ToArray();
                rxBuffer = rxBuffer.Skip(numberOfBytes).Take(rxBuffer.Length - numberOfBytes).ToArray();
                return returnBytes;
            } else
            {
                byte[] empty =  {0x00};
                return empty;
            }

        }

        public int numberBufferedBytes()
        {
            if(rxBuffer != null)
            {
                return rxBuffer.Length;
            } else
            {
                return 0;
            }
            
        }

        #endregion properties

        #region methods

        #region interface methods

        public void init(Paragraph theParagraph, SerialBuffer buffer)
        {
            serialBuffer = buffer;
            theOneParagraph = theParagraph;
            serialBuffer.RXbufferUpdated += new SerialBuffer.CallBackEventHandler(RXbufferUpdated);
            serialBuffer.TXbufferUpdated += new SerialBuffer.CallBackEventHandler(TXbufferUpdated);
        }

        public void RXbufferUpdated(object sender, EventArgs args)
        {
            Debug.WriteLine("serialPorts Callback for TX bufferUpdated");
        }

        public async void TXbufferUpdated(object sender, EventArgs args)
        {
            int numberOfBytes = serialBuffer.bytesInTxBuffer();
            await writeBytes(serialBuffer.ReadFromTxBuffer(numberOfBytes));
        }

        public void appendText(string str, Color color)
        {
            Run r = new Run();
            r.Foreground = getColoredBrush(color);
            r.Text = str;
            theOneParagraph.Inlines.Add(r);
        }

        public void appendLine(string str, Color color)
        {
            Run r = new Run();
            r.Foreground = getColoredBrush(color);
            r.Text = str + '\n';
            theOneParagraph.Inlines.Add(r);
        }

        public void clearDisplay()
        {
            theOneParagraph.Inlines.Clear();
        }


        public SolidColorBrush getColoredBrush(Color color)
        {
            return new SolidColorBrush(color);
        }
        #endregion interface methods

        #region port setup
        public async Task ListAvailablePorts()
        {
            // 1. Discover all device COM device IDs.
            // 2. Create a new SerialDevice
            // 3. Iterate over found COM IDs
            // 4. Look for SerialDevice based on COM ID.
            // 5. When found, check if it is already in the dictionary.
            // 6. If not in dictionary, add COM ID to dictionary and 
            //    add SerialDevice to dictionary, keyed to portName.
            // 7. When each device is found, callback to UI with the update.

            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                dis = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelectorFromUsbVidPid(0x0000, 0xFFFF));

                SerialDevice newSerialDevice;
                
                for (int i = 0; i < dis.Count; i++)
                {
                    
                    if (!listOfDevices.ContainsKey(dis[i].Id))
                    {
                        newSerialDevice = await SerialDevice.FromIdAsync(dis[i].Id);
                        listOfDevices.Add(dis[i].Id, dis[i]);
                        listOfPorts.Add(newSerialDevice.PortName, newSerialDevice);
                    }
                    Callback(this, null);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("List ports caught: " + ex.Message);
            }
        }

        public string getPortNameAtIndex(int index)
        {
            // 1. Convert found SerialDevice portNames to string array.
            // 2. Return portName at index.

            string portName = "";
            if (listOfPorts.Count > 0)
            {
                string[] portNamesArray = listOfPorts.Keys.ToArray();
                portName = portNamesArray[index];
            }
            return portName;
        }

        public int numberOfPortsInList()
        {
            return listOfPorts.Count;
        }

        public SerialDevice getSerialDeviceByPortName(string portName)
        {
            return listOfPorts[portName]; 
        }

        public void populateComboBoxesWithPortSettings(ComboBox baud, ComboBox dataBits, ComboBox stopBits, ComboBox parity, ComboBox handshake)
        {
            // 1. Populate the parameter combo boxes with
            //    with standard serial port options.

            // Fills a referenced combobox with common baud rates.
            baud.Items.Add(300);
            baud.Items.Add(600);
            baud.Items.Add(1200);
            baud.Items.Add(2400);
            baud.Items.Add(9600);
            baud.Items.Add(14400);
            baud.Items.Add(19200);
            baud.Items.Add(38400);
            baud.Items.Add(57600);
            baud.Items.Add(115200);
            baud.Items.Add(230400);
            // Default to 
            baud.SelectedIndex = 5;

            // Fills a referenced combobox with data bits settings.
            dataBits.Items.Add(6);
            dataBits.Items.Add(7);
            dataBits.Items.Add(8);
            dataBits.Items.Add(9);
            dataBits.SelectedIndex = 3;

            // Fills a referenced combobox with stop bits settings.
            stopBits.Items.Add(1);
            stopBits.Items.Add(1.5);
            stopBits.Items.Add(2);
            stopBits.SelectedIndex = 1;

            parity.Items.Add("None");
            parity.Items.Add("Odd");
            parity.Items.Add("Even");
            parity.Items.Add("Mark");
            parity.Items.Add("Space");
            parity.SelectedIndex = 1;

            handshake.Items.Add("None");
            handshake.Items.Add("XOnXOff");
            handshake.Items.Add("RequestToSend");
            handshake.Items.Add("RequestToSendXOnXOff");
            handshake.SelectedIndex = 1;
        }

        public void setPortAttributesWithComboBoxs(ComboBox cmbPort, ComboBox cmbBaud, ComboBox cmbDataBits, ComboBox cmbStopBits, ComboBox cmbParity, ComboBox cmbHandshaking)
        {
            // 1. Get serial attributes from combo box values.
            // 2. If a selectedDevice is not null, update its attributes.

            // 1
            selectedDeviceAttributes.comPort = cmbPort.SelectedItem.ToString();
            selectedDeviceAttributes.baudRate = UInt32.Parse(cmbBaud.SelectedItem.ToString());
            selectedDeviceAttributes.dataBits = ushort.Parse(cmbDataBits.SelectedItem.ToString());

            //Grr.Enum.Parse kept throwing errors on StopBits. I've no time for that crap.
            switch (cmbStopBits.SelectedIndex.ToString())
            {
                case "1":
                    selectedDeviceAttributes.stopBits = SerialStopBitCount.One;
                    break;
                case "1.5":
                    selectedDeviceAttributes.stopBits = SerialStopBitCount.OnePointFive;
                    break;
                case "2":
                    selectedDeviceAttributes.stopBits = SerialStopBitCount.Two;
                    break;
                default:
                    selectedDeviceAttributes.stopBits = SerialStopBitCount.One;
                    break;
            }


            //selectedDeviceAttributes.stopBits = (SerialStopBitCount)Enum.Parse(typeof(SerialStopBitCount), cmbStopBits.SelectedItem.ToString());

            selectedDeviceAttributes.parity = (SerialParity)Enum.Parse(typeof(SerialParity), cmbParity.SelectedItem.ToString());
            selectedDeviceAttributes.handshake = (SerialHandshake)Enum.Parse(typeof(SerialHandshake), cmbHandshaking.SelectedItem.ToString());

            // 2
            if(selectedSerialDevice != null)
            {
                selectedSerialDevice.BaudRate = selectedDeviceAttributes.baudRate;
                selectedSerialDevice.DataBits = selectedDeviceAttributes.dataBits;
                selectedSerialDevice.StopBits = selectedDeviceAttributes.stopBits;
                selectedSerialDevice.Parity = selectedDeviceAttributes.parity;
                selectedSerialDevice.Handshake = selectedDeviceAttributes.handshake;
            }
        }

        #endregion port handlers

        #region port use

        public bool openPort()
        {
            if(selectedSerialDevice != null)
            {
                selectedSerialDevice.WriteTimeout = TimeSpan.FromMilliseconds(100);
                selectedSerialDevice.ReadTimeout = TimeSpan.FromMilliseconds(100);
                // Create cancellation token object to close I/O operations when closing the device
                ReadCancellationTokenSource = new CancellationTokenSource();
                return true;
            }
            return false;     
        }

        public async Task<uint> write(string data)
        {
            // 1. Assert dataWriteObject is using selectedSerialDevice.
            // 2. Create the WriteString operation.
            // 3. Create the async Write task.
            // 4. Execute the write operation; await how many bytes are written.
            // 5. Return the number of bytes written.

            //DataWriter dataWriteObject = new DataWriter(selectedSerialDevice.OutputStream);
            //Task<uint> storeAsyncTask;
            //dataWriteObject.WriteString(data);
            //storeAsyncTask = dataWriteObject.StoreAsync().AsTask();

            
            DataWriter dataWriter = new DataWriter();       // Creates a new writing stream.
            dataWriter.WriteString(data);                   // Prepares the string into a byte[] buffered in
                                                            // the dataWriter object, which is ready to write
                                                            // back out as a string. Weird.
            // Here's where it get's tricky.  WriteAsync takes a IBuffer object, which as I understand it, is
            // simply a buffered string attached to an abstract object.  Now, DetachBuffer basically returns
            // the data, formatted in a streaming-ready format (ie, IBuffer), once the data is returned, it
            // deletes the buffered data.  Weird.
            uint bytesWritten = await selectedSerialDevice.OutputStream.WriteAsync(dataWriter.DetachBuffer());

            return bytesWritten;
        }

        public async Task<uint> writeBytes(byte[] data)
        {
            // 1. Assert dataWriteObject is using selectedSerialDevice.
            // 2. Create the WriteString operation.
            // 3. Create the async Write task.
            // 4. Execute the write operation; await how many bytes are written.
            // 5. Return the number of bytes written.

            //DataWriter dataWriteObject = new DataWriter(selectedSerialDevice.OutputStream);
            //Task<uint> storeAsyncTask;
            //dataWriteObject.WriteString(data);
            //storeAsyncTask = dataWriteObject.StoreAsync().AsTask();


            DataWriter dataWriter = new DataWriter();       // Creates a new writing stream.
            dataWriter.WriteBytes(data);                   // Prepares the string into a byte[] buffered in
                                                            // the dataWriter object, which is ready to write
                                                            // back out as a string. Weird.
                                                            // Here's where it get's tricky.  WriteAsync takes a IBuffer object, which as I understand it, is
                                                            // simply a buffered string attached to an abstract object.  Now, DetachBuffer basically returns
                                                            // the data, formatted in a streaming-ready format (ie, IBuffer), once the data is returned, it
                                                            // deletes the buffered data.  Weird.
            uint bytesWritten = await selectedSerialDevice.OutputStream.WriteAsync(dataWriter.DetachBuffer());

            return bytesWritten;
        }

        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;

            uint ReadBufferLength = 1024;

            //dataReaderObject.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16BE;

            // If task cancellation was requested, comply
            cancellationToken.ThrowIfCancellationRequested();

            // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
            dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;

            // Create a task object to wait for data on the serialPort.InputStream
            loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
            byte[] tempByteArray = new byte[bytesRead];
            dataReaderObject.ReadBytes(tempByteArray);
            rxBuffer = tempByteArray;

            string fancyString = byteArrayToReadableString(rxBuffer);

            if (bytesRead > 0)
            {
                serialBuffer.RxBuffer = tempByteArray;
                //appendText(fancyString, Colors.Red); 
            }

        }

        public async Task Listen(int timeOut)
        {
            try
            {
                if (selectedSerialDevice != null)
                {
                    dataReaderObject = new DataReader(selectedSerialDevice.InputStream);
                    ReadCancellationTokenSource = new CancellationTokenSource(timeOut);

                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Name == "TaskCanceledException")
                {
                    appendLine("Timed out waiting for response.", Colors.Crimson);
                }
                else
                {
                    appendLine(ex.Message, Colors.White);
                }
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }

        public void stopListing()
        {
            ReadCancellationTokenSource.Cancel();
        }

        public async void AlwaysListening()
        {
            
            try
            {
                if (selectedSerialDevice != null)
                {
                    dataReaderObject = new DataReader(selectedSerialDevice.InputStream);

                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().Name == "TaskCanceledException")
                {
                    appendLine("Listening interrupted.  No longer listening.", Colors.Crimson);
                }
                else
                {
                    appendLine(ex.Message, Colors.White);
                }
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }

        public void CloseDevice()
        {
            if (selectedSerialDevice != null)
            {
                // 1. Remove the selectedSerialDevice from portList.
                // 2. Disconnect streams.
                // 3. Dispose of selected COM port.

                listOfPorts.Remove(selectedSerialDevice.PortName);
                selectedSerialDevice.OutputStream.Dispose();
                selectedSerialDevice.InputStream.Dispose();
                selectedSerialDevice.Dispose();
            }
            listOfDevices.Clear();
        }

        public string byteArrayToReadableString(byte[] byteArray)
        {
            string charOrTwoCharHexString = "";

            for (int i = 0; i < byteArray.Length; i++)
            {
             if((int)byteArray[i] < 127)
                {
                    charOrTwoCharHexString += (char)byteArray[i];
                }
                else
                {
                    charOrTwoCharHexString += " " + byteArray[i].ToString("X2");
                } 
            }

            //Debug.Write(bytesReadByteArray[i].ToString("X2"));
            return charOrTwoCharHexString;
        }

        public void stopListening()
        {
            ReadCancellationTokenSource.Cancel();
        }

        public async Task disposeStream()
        {
            //await dataWriteObject.FlushAsync();
            //dataWriteObject.DetachStream();
            //dataWriteObject.Dispose();
        }

        #endregion port use

        #endregion methods
    }
}
