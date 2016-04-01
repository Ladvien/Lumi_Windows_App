using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;
using System.Threading;
using Windows.UI.Xaml.Documents;
using Windows.UI;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using bleTest3;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.Foundation;
using lumi;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace bleTest3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        serialPortsExtended serialPorts = new serialPortsExtended();
        tsb tsb = new tsb();
        blue blue = new blue();
        private bool portOpen = false;
        private CoreDispatcher dispatcher;

        DevicePicker devicePicker = new DevicePicker();
        

        public MainPage()
        {
            this.InitializeComponent();
            dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
            ApplicationView appView = ApplicationView.GetForCurrentView();
            appView.Title = "Lumi";

            // Add the callback handlers for serialPortsExtended isntance
            serialPorts.Callback += new serialPortsExtended.CallBackEventHandler(serialPortCallback);
            blue.Callback += new blue.CallBackEventHandler(blueCallback);

            // Until other thread reports COM port discovered
            btnConnect.IsEnabled = false;
            btnTsbConnect.IsEnabled = false;
            pvtPortSettings.IsEnabled = false;

            // Let the user know to wait while device thread returns.
            //clearMainDisplay();
            rtbMainDisplay.Blocks.Add(getParagraph("Please wait while COM ports load...", Colors.White));

            // Start the port discovery.
            serialPorts.ListAvailablePorts();
            
            // Have the serialPortsExtended object populate the combo boxes.
            serialPorts.populateComboBoxesWithPortSettings(cmbBaud, cmbDataBits, cmbStopBits, cmbParity, cmbHandshaking);
            serialPorts.init(rtbMainDisplay);

            tsb.init(serialPorts, rtbMainDisplay, pbSys);
            blue.init(this.Height, this.Width);

            devicePicker.DeviceSelected += DevicePicker_DeviceSelected;


            App.Current.Suspending += OnSuspending;

        }

        private async void DevicePicker_DeviceSelected(DevicePicker sender, DeviceSelectedEventArgs args)
        {
            Debug.WriteLine("Here's the ID: "+ args.SelectedDevice.Id);
            var paired = await args.SelectedDevice.Pairing.PairAsync(DevicePairingProtectionLevel.None);
            Debug.WriteLine(args.SelectedDevice.Id.Substring(15, 10)); 
            
        }

        public void close()
        {
            blue.closeBleDevice();
        }

        public void serialPortCallback(object sender, EventArgs args)
        {
            // Callback from serialPort thread.
            populatePortComboBox();
        }

        public void blueCallback(object sender, blue.BlueEvent blueEvent)
        {
            IAsyncAction ignored;
            switch (blueEvent)
            {
                case blue.BlueEvent.finishedConnecting:
                    ignored = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        rtbMainDisplay.Blocks.Add(getParagraph("Finished connecting to Bluetooth", Colors.CadetBlue));
                        labelConnectionStatus.Text = "Connected to Bluetooth LE";
                        connectionLabelBackGround.Background = getColoredBrush(Colors.CadetBlue);
                    });
                    break;
                case blue.BlueEvent.searchFinished:
                    ignored = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if(blue.bleDevices.Count > 0)
                        {
                            int deviceCount = blue.bleDevices.Count;
                            string[] key = new string[deviceCount];

                            cmbBleDevice.Items.Clear();
                            
                            for (int i = 0; i < deviceCount; i++)
                            {
                                blue.bleDevices.Keys.CopyTo(key, 0);
                                cmbBleDevice.Items.Insert(i,key[i]);
                            }
                            cmbBleDevice.SelectedIndex = 0;
                            cmbBleDevice.IsEnabled = true;
                            btnConnect.IsEnabled = true;
                        }
                        else
                        {
                            cmbBleDevice.IsEnabled = false;
                        }
                        btnBleSearch.IsEnabled = true;
                        btnConnect.IsEnabled = true;
                        cmbBleDevice.IsEnabled = true;
                        cmbDeviceSelector.IsEnabled = true;


                    });
                    break;
            }
        }
        

        private void populatePortComboBox()
        {
            // 1. Clear port items to prevent duplicates.
            // 2. Add all ports discovered to combo box.
            // 3. If there is one port in comboboxes enable dependent UI. 

            // Now COM ports are found, populate the cmbBox.
            cmbPort.Items.Clear();
            for(int i = 0; i < serialPorts.numberOfPortsInList(); i++)
            {
                cmbPort.Items.Add(serialPorts.getPortNameAtIndex(i));
            }
            if(cmbPort.Items.Count > 0)
            {
                btnConnect.IsEnabled = true;
                pvtPortSettings.IsEnabled = true;
                rtbMainDisplay.Blocks.Clear();
                rtbMainDisplay.Blocks.Add(getParagraph("Ready", Colors.LawnGreen));
                cmbPort.SelectedIndex = 0;
            } else
            {
                btnConnect.IsEnabled = false;
                pvtPortSettings.IsEnabled = false;
            }
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            clearMainDisplay();
        }

        public SolidColorBrush getColoredBrush(Color color)
        {
            return new SolidColorBrush(color);
        }


        private void cmbPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 1. Assert one port in combobox.
            // 2. Get the SerialDevice by name.
            // 3. If SerialDevice is not null, set our serialPorts selectedSerialDevice.
            if(cmbPort.Items.Count > 0) { 
                SerialDevice selectedDevice = serialPorts.getSerialDeviceByPortName(cmbPort.SelectedValue.ToString());
                if (selectedDevice != null)
                {
                    serialPorts.selectedSerialDevice = selectedDevice;
                    serialPorts.setPortAttributesWithComboBoxs(
                        cmbPort,
                        cmbBaud,
                        cmbDataBits,
                        cmbStopBits,
                        cmbParity,
                        cmbHandshaking
                    );
                }
            }
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            switch (cmbDeviceSelector.SelectedIndex)
            {
                case 0: // Serial Port
                    if (!portOpen)
                    {
                        if (serialPorts.openPort())
                        {
                            btnTsbConnect.IsEnabled = true;
                            connectionLabelBackGround.Background = getColoredBrush(Colors.LawnGreen);
                            labelConnectionStatus.Text = "Connected";
                            btnConnect.Content = "Disconnect";
                            pvtPortSettings.IsEnabled = false;
                            portOpen = true;
                        }
                        else
                        {
                            rtbMainDisplay.Blocks.Add(getParagraph("Unable to open port " + serialPorts.selectedDeviceAttributes.comPort, Colors.Crimson));
                        }

                    }
                    else
                    {
                        btnTsbConnect.IsEnabled = false;
                        connectionLabelBackGround.Background = getColoredBrush(Colors.Crimson);
                        labelConnectionStatus.Text = "Disconnected";
                        btnConnect.Content = "Connect";
                        pvtPortSettings.IsEnabled = true;
                        serialPorts.CloseDevice();
                        portOpen = false;
                    }
                    break;
                case 1: // Bluetooth LE
                    var success = blue.connect(blue.bleDevices[cmbBleDevice.SelectedItem.ToString()]);

                    break;

            }   
        }


        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                //serialPorts.newWriter();
                //uint bytesWritten = await serialPorts.write("@@@");
                await tsb.helloProcessing();
            } catch (Exception ex)
            {
                //await serialPorts.disposeStream();
                Debug.WriteLine(ex.Message);
            }
            
        }

        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {   
            //await blue.closeBleDevice();
        }

        public void displayText(string text, Color color)
        {
            rtbMainDisplay.Blocks.Add(getParagraph(text, color));
        }

        private void cmbDeviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(pvtPortSettings != null)
            {
                switch (cmbDeviceSelector.SelectedIndex)
                {
                    case 0:
                        pvtPortSettings.Visibility = Visibility.Visible;
                        btnBleSearch.Visibility = Visibility.Collapsed;
                        cmbBleDevice.Visibility = Visibility.Collapsed;
                        break;
                    case 1:
                        pvtPortSettings.Visibility = Visibility.Collapsed;
                        btnBleSearch.Visibility = Visibility.Visible;
                        cmbBleDevice.Visibility = Visibility.Visible;
                        break;
                }
            }
            
        }

        public void rootDeviceSelector(int selection)
        {
            switch (selection)
            {
                case 0:

                    break;

                case 1:
                    //DeviceSelectorInfo bluetoothLESelectorUnpaired = DeviceSelectorChoices.BluetoothUnpairedOnly;
                    //devicePicker.Filter.SupportedDeviceSelectors.Add(bluetoothLESelectorUnpaired.Selector);
                    DeviceSelectorInfo bluetoothLESelectorPaired = DeviceSelectorChoices.BluetoothLEPairedOnly;
                    devicePicker.Filter.SupportedDeviceSelectors.Add(bluetoothLESelectorPaired.Selector);
                    devicePicker.Appearance.Title = "TinySafeBoot Link";
                    devicePicker.Appearance.ForegroundColor = Colors.White;
                    Rect devicePickerBox = new Rect(this.Height / 2, this.Width / 2, this.Width / 3, this.Height / 3);
                    devicePicker.Show(devicePickerBox, Windows.UI.Popups.Placement.Default);
                    break;
            }
        }

        private void btnBleSearch_Click(object sender, RoutedEventArgs e)
        {
            btnBleSearch.IsEnabled = false;
            btnConnect.IsEnabled = false;
            cmbBleDevice.IsEnabled = false;
            cmbDeviceSelector.IsEnabled = false;
            blue.startBLEWatcher(5);
            DeviceSelectorInfo bluetoothLESelectorPaired = DeviceSelectorChoices.BluetoothLEPairedOnly;
            
        }
    }

}

