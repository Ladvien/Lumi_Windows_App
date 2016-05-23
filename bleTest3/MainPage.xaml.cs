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
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Windows;



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace bleTest3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        serialPortsExtended serialPorts = new serialPortsExtended();
        TSB tsb = new TSB();
        blue blue = new blue();
        private bool portOpen = false;
        private CoreDispatcher dispatcher;

        

        DevicePicker devicePicker = new DevicePicker();

        SerialBuffer serialBufffer = new SerialBuffer();

        public MainPage()
        {
            this.InitializeComponent();
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            ApplicationView appView = ApplicationView.GetForCurrentView();
            appView.Title = "Lumi";

            // Add the callback handlers for serialPortsExtended isntance
            serialPorts.Callback += new serialPortsExtended.CallBackEventHandler(serialPortCallback);
            blue.Callback += new blue.CallBackEventHandler(blueCallback);


            // Until other thread reports COM port discovered
            btnConnect.IsEnabled = false;
            btnTsbConnect.IsEnabled = false;
            pvtPortSettings.IsEnabled = false;
            tabTSB.IsEnabled = false;

            // Let the user know to wait while device thread returns.
            clearDisplay();
            appendLine("Please wait while COM ports load...", Colors.White);

            // Start the port discovery.
            serialPorts.ListAvailablePorts();

  

            // Have the serialPortsExtended object populate the combo boxes.
            serialPorts.populateComboBoxesWithPortSettings(cmbBaud, cmbDataBits, cmbStopBits, cmbParity, cmbHandshaking);
            serialPorts.init(theOneParagraph, serialBufffer);

            tsb.init(serialPorts, mainDisplayScroll, rtbMainDisplay, theOneParagraph, pbSys, serialBufffer, txbOpenFilePath);
            // Delegate callback for TSB updates.
            tsb.TsbUpdatedCommand += new TSB.TsbUpdateCommand(tsbcommandUpdate);
            blue.init(serialBufffer);

            //devicePicker.DeviceSelected += DevicePicker_DeviceSelected;

            serialBufffer.RXbufferUpdated += new SerialBuffer.CallBackEventHandler(RXbufferUpdated);
            serialBufffer.TXbufferUpdated += new SerialBuffer.CallBackEventHandler(TXbufferUpdated);
        }

        private void tsbcommandUpdate(TSB.statuses tsbConnectionStatus)
        {
            Debug.WriteLine("Insert command updates here");
            Debug.Write(tsbConnectionStatus);
            switch (tsbConnectionStatus)
            {
                case TSB.statuses.connected:
                    btnTsbConnect.Content = "Disconnect";
                    connectionLabelBackGround.Background = getColoredBrush(Colors.LawnGreen);
                    labelConnectionStatus.Text = "Connected to TSB";
                    mainPivotTable.SelectedIndex = 2;
                    tabTSB.IsEnabled = true;
                    break;
                case TSB.statuses.error:
                    tabTSB.IsEnabled = false;
                    btnTsbConnect.Content = "Connect";
                    btnTsbConnect.IsEnabled = true;
                    connectionLabelBackGround.Background = getColoredBrush(Colors.Crimson);
                    labelConnectionStatus.Text = "Error";
                    tabTSB.IsEnabled = false;
                    break;
                case TSB.statuses.uploadSuccessful:
                    reset();
                    break;
            }
        }

        public void RXbufferUpdated(object sender, EventArgs args)
        {
            if(tsb.commandInProgress == TSB.commands.hello)
            {
                byte[] data = serialBufffer.readAllBytesFromRXBuffer();
                //appendText(serialBufffer.ReadFromRxBuffer)
            }
            Debug.WriteLine("main Callback for RX bufferUpdated");
        }

        public void TXbufferUpdated(object sender, EventArgs args)
        {
            Debug.WriteLine("main Callback for TX bufferUpdated");
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

        public void serialPortCallback(object sender, serialPortsExtended.serialPortStatuses serialPortStatus)
        {
            IAsyncAction ignored;

            // Callback from serialPort thread.
            if(cmbDeviceSelector.SelectedIndex == 0)
            {
                ignored = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    switch (serialPortStatus)
                    {
                        case serialPortsExtended.serialPortStatuses.foundDevices:
                            populatePortComboBox();
                            break;
                        case serialPortsExtended.serialPortStatuses.didNotFindDevices:
                            appendLine("Did not find any serial devices.\n", Colors.Crimson);
                            break;
                    }                    
                });
            }
        }

   
        
        //public Paragraph getParagraph(string str, Color color)
        //{
        //    // 1. Get new paragraph.
        //    // 2. Paint paragraph text with selected color.
        //    // 3. Create a new run
        //    // 4. Add stext to run.  
        //    // 5. Add run to paragraph
        //    // 6. Return paragraph.

        //    Paragraph p = new Paragraph();
        //    p.Foreground = getColoredBrush(color);
        //    Run r = new Run();
        //    r.Text = str;
        //    p.Inlines.Add(r);
        //    return p;
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            clearDisplay();
        }

        public SolidColorBrush getColoredBrush(Color color)
        {
            return new SolidColorBrush(color);
        }

        private void cmbPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbFoundDevices.SelectedItem = cmbPort.SelectedItem;
        }

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            switch (cmbDeviceSelector.SelectedIndex)
            {
                case 0: // Serial Port
                    if (!portOpen)
                    {
                        assignCOMPort();                       
                        if (serialPorts.openPort())
                        {
                            btnTsbConnect.IsEnabled = true;
                            connectionLabelBackGround.Background = getColoredBrush(Colors.Yellow);
                            labelConnectionStatus.Text = "Connected";
                            btnConnect.Content = "Disconnect";
                            pvtPortSettings.IsEnabled = false;
                            cmbFoundDevices.IsEnabled = false;
                            cmbDeviceSelector.IsEnabled = false;
                            btnTsbConnect.IsEnabled = true;
                            portOpen = true;
                            /////////////////////////////
                            serialPorts.AlwaysListening();
                            /////////////////////////////
                        }
                        else
                        {
                            appendLine("Unable to open port " + serialPorts.selectedDeviceAttributes.comPort, Colors.Crimson);
                        }
                    }
                    else
                    {
                        try
                        {
                            serialPorts.CloseDevice();
                            btnTsbConnect.IsEnabled = false;
                            btnConnect.IsEnabled = false;
                            connectionLabelBackGround.Background = getColoredBrush(Colors.Crimson);
                            labelConnectionStatus.Text = "Disconnected";
                            btnConnect.Content = "Connect";
                            cmbFoundDevices.IsEnabled = true;
                            cmbDeviceSelector.IsEnabled = true;
                            pvtPortSettings.IsEnabled = true;
                            cmbFoundDevices.Items.Clear();
                        } catch (Exception exArgs)
                        {
                            Debug.WriteLine(exArgs.Message);
                        }
                        portOpen = false;                        
                   
                    }
                    break;
                case 1: // Bluetooth LE
                    if(cmbFoundDevices.SelectedItem != null) { var success = blue.connect(blue.bleDeviceAddresses[cmbFoundDevices.SelectedItem.ToString()]); }
                    
                    break;
            }   
        }


        private async void btnTsbConnect_Click(object sender, RoutedEventArgs e)
        {
            btnConnect.IsEnabled = false;
            await reset();
            try
            {
                tsb.hello();
            } catch (Exception ex)
            {
                //await serialPorts.disposeStream();
                Debug.WriteLine(ex.Message);
            }
            
        }

        public void displayText(string text, Color color)
        {
            appendLine(text, color);
        }

        private async void btnBleSearch_Click(object sender, RoutedEventArgs e)
        {
            switch (cmbDeviceSelector.SelectedIndex)
            {
                case 0:
                    await serialPorts.ListAvailablePorts();
                    break;
                case 1:
                    btnBleSearch.Content = "Searching";
                    btnBleSearch.IsEnabled = false;
                    btnConnect.IsEnabled = false;
                    cmbFoundDevices.IsEnabled = false;
                    cmbDeviceSelector.IsEnabled = false;
                    blue.startBLEWatcher(5);
                    //DeviceSelectorInfo bluetoothLESelectorPaired = DeviceSelectorChoices.BluetoothLEPairedOnly;
                    break;
            }

        }

        private void cmbFoundDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pvtPortSettings != null)
            {
                switch (cmbDeviceSelector.SelectedIndex)
                {
                    case 0:
                        cmbPort.SelectedItem = cmbFoundDevices.SelectedItem;
                        break;
                    case 1:
                        break;
                }
            }
        }

        private void cmbDeviceSelector_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (pvtPortSettings != null)
            {
                switch (cmbDeviceSelector.SelectedIndex)
                {
                    case 0:
                        cmbFoundDevices.Items.Clear();
                        pvtPortSettings.Visibility = Visibility.Visible;
                        btnBleSearch.Visibility = Visibility.Collapsed;
                        cmbFoundDevices.Visibility = Visibility.Visible;
                        cmbFoundDevices.IsEnabled = false;
                        populatePortComboBox();
                        break;
                    case 1:
                        cmbFoundDevices.Items.Clear();
                        pvtPortSettings.Visibility = Visibility.Collapsed;
                        btnBleSearch.Visibility = Visibility.Visible;
                        cmbFoundDevices.Visibility = Visibility.Visible;
                        cmbFoundDevices.IsEnabled = false;
                        break;
                }                
            }
        }

        public void assignCOMPort()
        {
            // 1. Assert one port in combobox.
            // 2. Get the SerialDevice by name.
            // 3. If SerialDevice is not null, set our serialPorts selectedSerialDevice.
            if (cmbFoundDevices.Items.Count > 0)
            {
                Debug.WriteLine(cmbFoundDevices.SelectedItem.ToString());
                SerialDevice selectedDevice = serialPorts.getSerialDeviceByPortName(cmbFoundDevices.SelectedItem.ToString());
                if (selectedDevice != null)
                {
                    serialPorts.selectedSerialDevice = selectedDevice;
                    serialPorts.setPortAttributesWithComboBoxs(
                        cmbDeviceSelector,
                        cmbBaud,
                        cmbDataBits,
                        cmbStopBits,
                        cmbParity,
                        cmbHandshaking
                    );
                }
            }
        }

        private void populatePortComboBox()
        {
            // 1. Clear port items to prevent duplicates.
            // 2. Add all ports discovered to combo box.
            // 3. If there is one port in comboboxes enable dependent UI. 
            IAsyncAction ignored;
            ignored = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Now COM ports are found, populate the cmbBox.
                if (cmbDeviceSelector.SelectedIndex == 0)
                {
                    cmbFoundDevices.Items.Clear();
                    cmbPort.Items.Clear();
                    for (int i = 0; i < serialPorts.numberOfPortsInList(); i++)
                    {
                        cmbFoundDevices.Items.Insert(i, serialPorts.getPortNameAtIndex(i));
                        cmbPort.Items.Insert(i, serialPorts.getPortNameAtIndex(i));
                    }
                    if (cmbFoundDevices.Items.Count > 0)
                    {
                        btnConnect.IsEnabled = true;
                        pvtPortSettings.IsEnabled = true;
                        cmbFoundDevices.IsEnabled = true;
                        appendLine("Ready", Colors.LawnGreen);
                        cmbFoundDevices.SelectedIndex = 0;
                    }
                    else
                    {
                        btnConnect.IsEnabled = false;
                        pvtPortSettings.IsEnabled = false;
                    }
                }
            });
        }

        public void blueCallback(object sender, blue.BlueEvent blueEvent)
        {
            IAsyncAction ignored;
            switch (blueEvent)
            {
                case blue.BlueEvent.finishedConnecting:
                    ignored = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        appendLine("Finished connecting to Bluetooth", Colors.CadetBlue);
                        labelConnectionStatus.Text = "Connected to Bluetooth LE";
                        connectionLabelBackGround.Background = getColoredBrush(Colors.CadetBlue);
                    });
                    break;
                case blue.BlueEvent.searchFinished:
                    ignored = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if (blue.bleDeviceAddresses.Count > 0)
                        {
                            int deviceCount = blue.bleDeviceAddresses.Count;
                            string[] key = new string[deviceCount];

                            cmbFoundDevices.Items.Clear();

                            for (int i = 0; i < deviceCount; i++)
                            {
                                blue.bleDeviceAddresses.Keys.CopyTo(key, 0);
                                cmbFoundDevices.Items.Insert(i, key[i]);
                            }
                            cmbFoundDevices.SelectedIndex = 0;
                            cmbFoundDevices.IsEnabled = true;
                            btnConnect.IsEnabled = true;
                        }
                        else
                        {
                            cmbFoundDevices.IsEnabled = false;
                        }
                        btnBleSearch.IsEnabled = true;
                        btnBleSearch.Content = "Search";
                        cmbDeviceSelector.IsEnabled = true;
                    });
                    break;
            }
        }
        #region oldcode


        //public void rootDeviceSelector(int selection)
        //{
        //    switch (selection)
        //    {
        //        case 0:
        //            break;
        //        case 1:
        //            //DeviceSelectorInfo bluetoothLESelectorUnpaired = DeviceSelectorChoices.BluetoothUnpairedOnly;
        //            //devicePicker.Filter.SupportedDeviceSelectors.Add(bluetoothLESelectorUnpaired.Selector);
        //            DeviceSelectorInfo bluetoothLESelectorPaired = DeviceSelectorChoices.BluetoothLEPairedOnly;
        //            devicePicker.Filter.SupportedDeviceSelectors.Add(bluetoothLESelectorPaired.Selector);
        //            devicePicker.Appearance.Title = "TinySafeBoot Link";
        //            devicePicker.Appearance.ForegroundColor = Colors.White;
        //            Rect devicePickerBox = new Rect(this.Height / 2, this.Width / 2, this.Width / 3, this.Height / 3);
        //            devicePicker.Show(devicePickerBox, Windows.UI.Popups.Placement.Default);
        //            break;
        //    }
        //}

        //private async void OnSuspending(object sender, SuspendingEventArgs e)
        //{
        //    //await blue.closeBleDevice();
        //}
        #endregion oldcode

        private async void btnSendText_Click(object sender, RoutedEventArgs e)
        {
            string sendString = "";
            rtbSendTextBlock.Document.GetText(Windows.UI.Text.TextGetOptions.NoHidden, out sendString);
            await serialPorts.write(sendString);            
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

        private void Button_ReadFlash(object sender, RoutedEventArgs e)
        {
            tsb.readFlash();
        }

        private async void btnTest_Click(object sender, RoutedEventArgs e)
        {
            blue.writeToBleDevice("Blah");
            //blue.connectToAlreadyPaired();
        }

        public async Task reset()
        {
            switch (cmbDeviceSelector.SelectedIndex)
            {
                case 0:
                    await serialPorts.dtrToggle();
                    break;
                case 1:
                    // Write PIOB low and high.
                    break;
            }
        }

        private void displayFlashType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tsb.setFlashDisplay((TSB.displayFlash)cmbFlashDisplay.SelectedIndex);
        }

        private async void OpenFile_ButtonClick(object sender, RoutedEventArgs e)
        {
            await tsb.selectFileToRead();
        }

        private void Button_WriteFlash(object sender, RoutedEventArgs e)
        {
            tsb.writeToFlash();
        }

        private void cmbOTADevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cmbOTADevice.SelectedIndex)
            {
                case 0:
                    tsb.setOTADevice(TSB.OTAType.none);
                    break;
                case 1:
                    tsb.setOTADevice(TSB.OTAType.hm1x);
                    break;
                case 2:
                    tsb.setOTADevice(TSB.OTAType.esp);
                    break;
                default:
                    tsb.setOTADevice(TSB.OTAType.none);
                    appendText("Problems selecting Over-the-Air device", Colors.Crimson);
                    break;
            }
        }
    }// End MainPage
} // End Namespace

