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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace bleTest3
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        serialPortsExtended serialPorts = new serialPortsExtended();

        public MainPage()
        {
            this.InitializeComponent();

            // Add the callback handlers for serialPortsExtended isntance
            serialPorts.Callback += new serialPortsExtended.CallBackEventHandler(updatePortComboBox);
            
            // Until other thread reports COM port discovered
            btnConnect.IsEnabled = false;
            btnTsbConnect.IsEnabled = false;
            pvtPortSettings.IsEnabled = false;

            // Let the user know to wait while device thread returns.
            clearMainDisplay();
            rtbMainDisplay.Blocks.Add(getParagraph("Please wait while COM ports load...", Colors.White));

            // Start the port discovery.
            serialPorts.ListAvailablePorts();
            // Have the serialPortsExtended object populate the combo boxes.
            serialPorts.populateComboBoxesWithPortSettings(cmbBaud, cmbDataBits, cmbStopBits, cmbParity, cmbHandshaking);
            serialPorts.loadMainDisplay(rtbMainDisplay);

        }

        public void updatePortComboBox()
        {
            // Callback from serialPort thread.
            populatePortComboBox();
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
            connectionLabelBackGround.Background = getColoredBrush(Colors.LawnGreen);
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

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (serialPorts.openPort())
            {
                btnTsbConnect.IsEnabled = true;
            } else
            {
                btnTsbConnect.IsEnabled = false;
            }
            
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                //serialPorts.newWriter();
               uint bytesWritten = await serialPorts.write("@@@");
            } catch
            {
                //await serialPorts.disposeStream();
                Debug.WriteLine("Shit");
            }
            
        }
    }
}
