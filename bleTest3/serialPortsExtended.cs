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
using Windows.Security.Cryptography;

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

        public delegate void CallBackEventHandler();
        public event CallBackEventHandler Callback;

        public RichTextBlock rtbMainDisplay;

        //DataWriter dataWriteObject = null;
        DataReader dataReaderObject = null;

        private ObservableCollection<DeviceInformation> listOfDevices = new ObservableCollection<DeviceInformation>();
        private ObservableCollection<SerialDevice> listOfPorts= new ObservableCollection<SerialDevice>();
        private CancellationTokenSource ReadCancellationTokenSource;

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
            set { _serialDeviceItem = value;
                listOfPorts.Add(value);
                Callback();
            }
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
        public void init(RichTextBlock paraMainDisplay)
        {
            rtbMainDisplay = paraMainDisplay;


        }


        public SolidColorBrush getColoredBrush(Color color)
        {
            return new SolidColorBrush(color);
        }

        public Paragraph getParagraph(string str, Color color)
        {
            // 1. Get new paragraph.
            // 2. Paint paragraph text with selected color.
            // 3. Create a new run
            // 4. Add stext to run.  
            // 5. Add run to paragraph
            // 6. Return paragraph.

            Paragraph p = new Paragraph();
            p.Foreground = getColoredBrush(color);
            Run r = new Run();
            r.Text = str;
            p.Inlines.Add(r);
            return p;
        }

        public void clearMainDisplay()
        {
            rtbMainDisplay.Blocks.Clear();
        }
        #endregion interface methods

        #region port setup
        public async void ListAvailablePorts()
        {
            // 1. Discover all device COM device IDs.
            // 2. Add all devices to a collection
            // 3. Add each port to the port field. Await asserts
            //    the discovered port is added(?).
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(SerialDevice.GetDeviceSelectorFromUsbVidPid(0x0000, 0xFFFF));

                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);
                    serialDeviceItem = await SerialDevice.FromIdAsync(dis[i].Id);
                }
            }
            catch
            {
                // Error handling.
            }
        }

        public string getPortNameAtIndex(int index)
        {
            string portName = listOfPorts[index].PortName;
            return portName;
        }

        public int numberOfPortsInList()
        {
            return listOfPorts.Count;
        }

        public SerialDevice getSerialDeviceByPortName(string portName)
        {
            // 1. Iterate through ports looking for matching name.
            // 2. Return SerialDevice if found.
            // 3. Or return null if not found.
            for(int i = 0; i < listOfPorts.Count; i++)
            {
                if(listOfPorts[i].PortName == portName) { return listOfPorts[i]; }
            }
            return null;
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
                Debug.WriteLine(selectedSerialDevice.PortName);
                Debug.WriteLine(selectedSerialDevice.BaudRate);
                Debug.WriteLine(selectedSerialDevice.DataBits);
                Debug.WriteLine(selectedSerialDevice.StopBits);
                Debug.WriteLine(selectedSerialDevice.Parity);
                Debug.WriteLine(selectedSerialDevice.Handshake);
                selectedSerialDevice.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                selectedSerialDevice.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                // Create cancellation token object to close I/O operations when closing the device
                ReadCancellationTokenSource = new CancellationTokenSource();
                //AlwaysListening();
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

            //string fancyString = byteArrayToReadableString(rxBuffer);

            if (bytesRead > 0)
            {
              //rtbMainDisplay.Blocks.Add(getParagraph(fancyString, Colors.Red));
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
                    //rtbMainDisplay.Blocks.Add(getParagraph("Timed out waiting for response.", Colors.Crimson));
                }
                else
                {
                    rtbMainDisplay.Blocks.Add(getParagraph(ex.Message, Colors.White));
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
                    rtbMainDisplay.Blocks.Add(getParagraph("Listening interrupted.  No longer listening.", Colors.Crimson));
                }
                else
                {
                    rtbMainDisplay.Blocks.Add(getParagraph(ex.Message, Colors.White));
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

        private void CloseDevice()
        {
            if (selectedSerialDevice != null)
            {
                selectedSerialDevice.Dispose();
            }
            selectedSerialDevice = null;

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

    }
    #endregion methods
}
