﻿using System;
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
using Lumi;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.Foundation;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Windows;
using Windows.Security.Cryptography;



// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Lumi
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

        SerialBuffer serialBuffer = new SerialBuffer();

        public enum uiSetTo: int
        {
            none,
            Init,
            Connect,
            SearchBLE,
            SearchingBLE,
            SearchedBLE,
            ConnectingBLE,
            SerialConnectToTsb,
            BleConnectToTsb,
            ConnectedToTsb,
            TsbError,
            ConnectError,
            Disconnected,
            NoDevice,
            Paired
        }

        public MainPage()
        {
            this.InitializeComponent();

            // Add version information to main view.
            string appVersion = string.Format("Version: {0}.{1}.{2}.{3}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor,
                    Package.Current.Id.Version.Build,
                    Package.Current.Id.Version.Revision);

            ApplicationView appView = ApplicationView.GetForCurrentView();
            appView.Title = "Lumi " + appVersion;

            // Setup a thread dispatcher, used to set thread during IO callbacks.
            dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

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

            // Default view to serial devices.    
            tsb.setDevice(TSB.device.serial);
            tsb.populateResetPinCmbBox(cmbResetPin);

            // Have the serialPortsExtended object populate the combo boxes.
            serialPorts.populateComboBoxesWithPortSettings(cmbBaud, cmbDataBits, cmbStopBits, cmbParity, cmbHandshaking);
            serialPorts.init(theOneParagraph);

            // Initialize the TSB object.  This is necessary to start SerialDeviceWatcher.
            tsb.init(serialPorts, mainDisplayScroll, rtbMainDisplay, theOneParagraph, pbSys, serialBuffer, txbOpenFilePath);
            
            // Delegate callback for TSB updates.
            tsb.TsbUpdatedCommand += new TSB.TsbUpdateCommand(tsbStatusUpdate);

            // Get BluetoothLE up and going.
            blue.init();

            serialBuffer.RXbufferUpdated += new SerialBuffer.CallBackEventHandler(RXbufferUpdated);
            serialBuffer.TXbufferUpdated += new SerialBuffer.CallBackEventHandler(TXbufferUpdated);            
        }

        private void tsbStatusUpdate(TSB.statuses tsbConnectionStatus, Run message)
        {
            // This method is called when the TSB object sends a UI update.
            //
            // 1. Change to the UI thread.
            // 2. Switch on the status passed by the TSB object.

            var ignored = dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                switch (tsbConnectionStatus)
                {
                    case TSB.statuses.connected:
                        btnTsbConnect.Content = "Disconnect";                                       // Connect button switches to Disconnect.
                        connectionLabelBackGround.Background = getColoredBrush(Colors.LawnGreen);   // Change to green background for connection label
                        labelConnectionStatus.Text = "Connected to TSB";                            // 
                        mainPivotTable.SelectedIndex = 2;
                        tabTSB.IsEnabled = true;
                        appendRunToMainDisplay(message);
                        mainPivotTable.SelectedIndex = 3;
                        break;
                    case TSB.statuses.error:
                        tabTSB.IsEnabled = false;
                        btnTsbConnect.Content = "Connect";
                        btnTsbConnect.IsEnabled = true;
                        connectionLabelBackGround.Background = getColoredBrush(Colors.Crimson);
                        labelConnectionStatus.Text = "Error";
                        break;
                    case TSB.statuses.uploadSuccessful:
                        await reset();
                        break;
                    case TSB.statuses.displayMessage:
                        appendRunToMainDisplay(message);
                        break;
                    case TSB.statuses.bootloaderDisconnected:
                        tabTSB.IsEnabled = false;
                        btnTsbConnect.Content = "Connect to TSB";
                        btnTsbConnect.IsEnabled = true;
                        connectionLabelBackGround.Background = getColoredBrush(Colors.Yellow);
                        labelConnectionStatus.Text = "TSB Disconnected";
                        break;
                    case TSB.statuses.wirelessReleaseSuccess:
                        blue.closeBleDevice();
                        tabTSB.IsEnabled = false;
                        btnTsbConnect.Content = "Connect to TSB";
                        btnTsbConnect.IsEnabled = true;
                        connectionLabelBackGround.Background = getColoredBrush(Colors.Yellow);
                        labelConnectionStatus.Text = "TSB Disconnected";
                        break;
                }
            });

        }

        public void setUI(uiSetTo uiSetTo)
        {
            switch (uiSetTo)
            {
                case uiSetTo.none:
                    break;
                case uiSetTo.Init:
                    switch (cmbDeviceSelector.SelectedIndex)
                    {
                        case 0: // Serial
                            btnConnect.IsEnabled = true;
                            pvtPortSettings.IsEnabled = true;
                            cmbFoundDevices.IsEnabled = true;
                            cmbOTADevice.SelectedIndex = 0;
                            break;
                        case 1: // HM-1X
                            btnWirelessSearch.IsEnabled = true;
                            btnWirelessSearch.Content = "Search";
                            cmbDeviceSelector.IsEnabled = true;
                            break;
                    }
                    break;
                case uiSetTo.Connect:
                    switch (cmbDeviceSelector.SelectedIndex)
                    {
                        case 0: // Serial
                            cmbFoundDevices.Items.Clear();
                            pvtPortSettings.Visibility = Visibility.Visible;
                            btnWirelessSearch.Visibility = Visibility.Collapsed;
                            cmbFoundDevices.Visibility = Visibility.Visible;
                            cmbFoundDevices.IsEnabled = false;
                            break;
                        case 1: // HM-1X
                            cmbFoundDevices.Items.Clear();
                            pvtPortSettings.Visibility = Visibility.Collapsed;
                            btnWirelessSearch.Visibility = Visibility.Visible;
                            cmbFoundDevices.Visibility = Visibility.Visible;
                            cmbFoundDevices.IsEnabled = false;

                            break;
                    }
                    break;
                case uiSetTo.ConnectingBLE:
                    btnConnect.IsEnabled = false;
                    cmbDeviceSelector.IsEnabled = false;
                    cmbFoundDevices.IsEnabled = false;
                    btnBleSearch.IsEnabled = false;
                    break;
                case uiSetTo.SerialConnectToTsb:
                    btnBleSearch.IsEnabled = false;
                    btnTsbConnect.IsEnabled = true;
                    connectionLabelBackGround.Background = getColoredBrush(Colors.Yellow);
                    labelConnectionStatus.Text = "Connected";
                    btnConnect.Content = "Disconnect";
                    btnConnect.IsEnabled = true;
                    pvtPortSettings.IsEnabled = false;
                    cmbFoundDevices.IsEnabled = false;
                    cmbDeviceSelector.IsEnabled = false;
                    btnTsbConnect.IsEnabled = true;
                    break;
                case uiSetTo.ConnectedToTsb:
                    break;
                case uiSetTo.SearchBLE:
                    break;
                case uiSetTo.SearchingBLE:
                    btnWirelessSearch.Content = "Searching";
                    btnWirelessSearch.IsEnabled = false;
                    btnConnect.IsEnabled = false;
                    cmbFoundDevices.IsEnabled = false;
                    cmbDeviceSelector.IsEnabled = false;
                    break;
                case uiSetTo.SearchedBLE:
                    cmbFoundDevices.SelectedIndex = 0;
                    cmbFoundDevices.IsEnabled = true;
                    btnConnect.IsEnabled = true;
                    // Make sure the OTA is set to BLE, since, yanno, we are searching for a BLE device.
                    mainPivotTable.SelectedIndex = 2;
                    cmbOTADevice.SelectedIndex = 1;
                    break;
                case uiSetTo.ConnectError:
                    break;
                case uiSetTo.TsbError:
                    break;
                case uiSetTo.Disconnected:
                    btnTsbConnect.IsEnabled = false;
                    btnConnect.IsEnabled = false;
                    connectionLabelBackGround.Background = getColoredBrush(Colors.Crimson);
                    labelConnectionStatus.Text = "Disconnected";
                    btnConnect.Content = "Connect";
                    cmbFoundDevices.IsEnabled = true;
                    cmbDeviceSelector.IsEnabled = true;
                    pvtPortSettings.IsEnabled = true;
                    cmbFoundDevices.Items.Clear();
                    cmbFoundDevices.IsEnabled = false;
                    switch (cmbDeviceSelector.SelectedIndex)
                    {
                        case 0:
                            //populatePortComboBox();
                            break;
                        case 1:
                            //wirelessSearch();
                            break;
                    }
                    break;
                case uiSetTo.Paired:
                    connectionLabelBackGround.Background = getColoredBrush(Colors.CornflowerBlue);
                    labelConnectionStatus.Text = "Paired";
                    break;
                case uiSetTo.BleConnectToTsb:
                    btnTsbConnect.IsEnabled = true;
                    btnConnect.IsEnabled = true;
                    btnConnect.Content = "Disconnect";
                    connectionLabelBackGround.Background = getColoredBrush(Colors.Yellow);
                    labelConnectionStatus.Text = "Connected";
                    pvtPortSettings.IsEnabled = false;
                    cmbFoundDevices.IsEnabled = false;
                    cmbDeviceSelector.IsEnabled = false;
                    break;
                case uiSetTo.NoDevice:
                    cmbFoundDevices.IsEnabled = false;
                    break;
                
            }
        }

        public void RXbufferUpdated(object sender, EventArgs args)
        {
            if(tsb.commandInProgress == TSB.commands.hello)
            {
                byte[] data = serialBuffer.readAllBytesFromRXBuffer();
            }

            if(tsb.commandInProgress == TSB.commands.none)
            {
                byte[] data = serialBuffer.readAllBytesFromRXBuffer();

                IAsyncAction ignored;
                // Callback from serialPort thread.

                ignored = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        string dataStr = tsb.getAsciiStringFromByteArray(data);
                        appendText(dataStr, Colors.LimeGreen);
                    }
                    catch (Exception ex)
                    {
                        appendText("\nBad data recieved\n", Colors.Crimson);
                    }
                });
            }
            //Debug.WriteLine("main Callback for RX bufferUpdated");
        }

        public void TXbufferUpdated(object sender, EventArgs args)
        {
            //Debug.WriteLine("main Callback for TX bufferUpdated");
        }

        public void close()
        {
            blue.closeBleDevice();
        }

        public void serialPortCallback(object sender, serialPortsExtended.serialPortStatuses serialPortStatus)
        {
            IAsyncAction ignored;
            // Callback from serialPort thread.

                ignored = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (cmbDeviceSelector.SelectedIndex == 0)
                    {
                        switch (serialPortStatus)
                        {
                            case serialPortsExtended.serialPortStatuses.foundDevices:
                                populatePortComboBox();
                                break;
                            case serialPortsExtended.serialPortStatuses.didNotFindDevices:
                                setUI(uiSetTo.NoDevice);
                                cmbFoundDevices.Items.Clear();
                                break;
                        }
                    }
                });
        }

        public void blueCallback(object sender, blue.BlueEvent blueEvent, string message)
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
                            setUI(uiSetTo.SearchedBLE);

                        }
                        else
                        {
                            cmbFoundDevices.IsEnabled = false;
                            setUI(uiSetTo.Init);
                        }
                        setUI(uiSetTo.Init);
                    });
                    break;
                case blue.BlueEvent.paired:
                    setUI(uiSetTo.Paired);
                    break;
                case blue.BlueEvent.connected:
                    ignored = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        setUI(uiSetTo.BleConnectToTsb);
                        blue.attachSerialBuffer(serialBuffer);
                    });
                    break;
                case blue.BlueEvent.updateMessage:
                    appendText(message, Colors.CornflowerBlue);
                    break;
            }
        }


        public SolidColorBrush getColoredBrush(Color color)
        {
            return new SolidColorBrush(color);
        }

        private void cmbPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbFoundDevices.SelectedItem = cmbPort.SelectedItem;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            clearDisplay();
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            switch (cmbDeviceSelector.SelectedIndex)
            {
                case 0: // Serial Port
                    if (!portOpen)
                    {
                        assignCOMPort();                       
                        if (serialPorts.openPort())
                        {
                            setUI(uiSetTo.SerialConnectToTsb);
                            portOpen = true;
                            /////////////////////////////
                            serialPorts.AlwaysListening();
                            /////////////////////////////
                            serialPorts.attachSerialBuffer(serialBuffer);
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
                            setUI(uiSetTo.Disconnected);
                            serialPorts.CloseDevice();

                        } catch (Exception exArgs)
                        {
                            Debug.WriteLine(exArgs.Message);
                        }
                        portOpen = false;                        
                    }
                    break;
                case 1: // Bluetooth LE
                    if(cmbFoundDevices.SelectedItem != null)
                    {
                        if(true != blue.connected)
                        {
                            setUI(uiSetTo.ConnectingBLE);
                            var success = blue.connect(blue.bleDeviceAddresses[cmbFoundDevices.SelectedItem.ToString()]);
                        } else
                        {
                            tsb.remoteResetInit();
                            setUI(uiSetTo.Disconnected);
                        }
                        
                    }
                    break;
            }   
        }

        private async void btnTsbConnect_Click(object sender, RoutedEventArgs e)
        {
            tsb.printCommandInProgress();
            if(btnTsbConnect.Content == "Disconnect")
            {
                setUI(uiSetTo.SerialConnectToTsb);
                await reset();
                btnTsbConnect.Content = "Connect to TSB";
                tsb = new TSB();
                // Initialize the TSB object.  This is necessary to start SerialDeviceWatcher.
                tsb.init(serialPorts, mainDisplayScroll, rtbMainDisplay, theOneParagraph, pbSys, serialBuffer, txbOpenFilePath);
            } else
            {
                btnConnect.IsEnabled = false;
                await reset();
                try
                {
                    tsb.hello();
                }
                catch (Exception ex)
                {
                    //await serialPorts.disposeStream();
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void btnWirelessSearch_Click(object sender, RoutedEventArgs e)
        {
            wirelessSearch();
        }

        public async void wirelessSearch()
        {
            switch (cmbDeviceSelector.SelectedIndex)
            {
                case 0:
                    await serialPorts.ListAvailablePorts();
                    break;
                case 1:
                    setUI(uiSetTo.SearchingBLE);
                    await blue.startBLEWatcher(5);
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
                setUI(uiSetTo.Connect);
                switch (cmbDeviceSelector.SelectedIndex)
                {
                    case 0: // Serial
                        populatePortComboBox();
                        setUI(uiSetTo.Init);
                        tsb.setDevice(TSB.device.serial);
                        break;
                    case 1: // HM-1X
                        btnWirelessSearch.Focus(FocusState.Keyboard);
                        tsb.setDevice(TSB.device.hm1x);
                        wirelessSearch();
                        break;
                    case 3: // ESP8266
                        break;
                    default:
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
                        setUI(uiSetTo.Init);
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

        #region oldcode

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

        private void btnSendText_Click(object sender, RoutedEventArgs e)
        {
            string sendString = "";
            rtbSendTextBlock.Document.GetText(Windows.UI.Text.TextGetOptions.NoHidden, out sendString);
            serialBuffer.txBuffer = blue.GetBytes(sendString);
        }

        public void displayText(string text, Color color)
        {
            appendLine(text, color);
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

        public void appendRunToMainDisplay(Run r)
        {
            if(r != null)
            {
                theOneParagraph.Inlines.Add(r);
            }
        }

        public void clearDisplay()
        {
            theOneParagraph.Inlines.Clear();
        }

        private void Button_ReadFlash(object sender, RoutedEventArgs e)
        {
            tsb.readFlash();
        }

        public async Task reset()
        {
            switch (cmbDeviceSelector.SelectedIndex)
            {
                case 0:
                    await serialPorts.dtrToggle();
                    break;
                case 1:
                    tsb.remoteResetInit();
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

        private async void btnReset_Click(object sender, RoutedEventArgs e)
        {

            tsb = new TSB();
            // Initialize the TSB object.  This is necessary to start SerialDeviceWatcher.
            tsb.init(serialPorts, mainDisplayScroll, rtbMainDisplay, theOneParagraph, pbSys, serialBuffer, txbOpenFilePath);
            
            await reset();
            tsbStatusUpdate(TSB.statuses.bootloaderDisconnected, null);
            
        }

        private void cmbResetPinSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(blue != null)
            {
                tsb.setResetPin(cmbResetPin.SelectedIndex);
            }
        }

        private void btnBleSearch_Click(object sender, RoutedEventArgs e)
        {

        }
    }// End MainPage
} // End Namespace

