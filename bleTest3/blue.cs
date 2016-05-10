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
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth.Advertisement;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Devices.Bluetooth;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using lumi;

namespace bleTest3
{
    public class blue
    {
        #region credits
        // https://github.com/marknotgeorge/SpeedAndCadence/blob/master/SpeedAndCadence/Model/CsacService.cs
        // https://social.msdn.microsoft.com/Forums/sqlserver/en-US/b98d77f2-bf5e-45fc-9495-1c444b54450e/uwpreconnecting-to-a-ble-csac-device-causes-exception?forum=wpdevelop
        // https://github.com/Microsoft/Windows-universal-samples/tree/master/Samples/BluetoothAdvertisement/cs
        //
        //
        #endregion credits

        #region properties_and_methods

        SerialBuffer serialBuffer = new SerialBuffer();
        
        // Used for UI callback.
        public enum BlueEvent
        {
            none = 0,
            finishedConnecting = 1,
            searchFinished = 2
        }

        // Callback to the main UI.
        public BlueEvent blueEvent = new BlueEvent();
        public delegate void CallBackEventHandler(object sender, BlueEvent blueEvent);
        public event CallBackEventHandler Callback;

        // Bluetooth LE Discovery
        BluetoothLEAdvertisementWatcher bleAdvertWatcher = new BluetoothLEAdvertisementWatcher();
        public Dictionary<string, ulong> bleDevices = new Dictionary<string, ulong>();
        public Dictionary<string, short> bleDevicesRSSI = new Dictionary<string, short>();
        public DispatcherTimer bleSearchTimer = new DispatcherTimer();

        // Bluetooth LE Connection
        BluetoothLEDevice bleDevice;

        GattCharacteristic readWriteCharacteristic;
        DataWriter writer = new DataWriter();

        // Used for enumeration information.
        public DeviceWatcher deviceWatcher;

        DevicePicker devicePicker = new DevicePicker();
        double height = 0;
        double width = 0;

        // Used to determine discovery of BLE services.  
        int gattAddedCounter = 0;
        // Used to workaround MS' crappy API.
        public bool pairedUnpairedThisSession = false;
        private int deviceCounter;
        #endregion properties_and_methods

        public void init(double appHeight, double appWidth)
        {
            // Create and initialize a new watcher instance.
            bleAdvertWatcher = new BluetoothLEAdvertisementWatcher();
            
            height = appHeight;
            width = appWidth;

            // Watcher created for determination of enumeration of connected BLE.  This is needed
            // for the workaround to Microsoft's crappy BLE API.
            deviceWatcher = DeviceInformation.CreateWatcher();
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Start();

            
            bleAdvertWatcher.Received += OnAdvertisementReceived;
            bleAdvertWatcher.Stopped += OnAdvertisementWatcherStopped;

            bleAdvertWatcher.ScanningMode = BluetoothLEScanningMode.Active;

            bleAdvertWatcher.Start();

            bleSearchTimer.Tick += BleSearchTimer_Tick;

            serialBuffer.RXbufferUpdated += new SerialBuffer.CallBackEventHandler(RXbufferUpdated);
            serialBuffer.TXbufferUpdated += new SerialBuffer.CallBackEventHandler(TXbufferUpdated);
        }

        private void RXbufferUpdated(object sender, EventArgs args)
        {
            Debug.WriteLine("blue Callback for RX bufferUpdated");
        }

        private void TXbufferUpdated(object sender, EventArgs args)
        {
            Debug.WriteLine("blue Callback for TX bufferUpdated");
        }
        #region devicewatcher

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            
            if (bleDevice != null)
            {
                if (bleDevice.DeviceId == args.Id)
                {
                    Debug.WriteLine("Total Gatts added: "+gattAddedCounter);
                    gattAddedCounter++;
                    // This would be better if BluetoothAdvertisementWatcher discovered Gatt services
                    // rather than presuming them.
                    if(gattAddedCounter == 7)
                    {
                        callback();
                    }
                }
            }

        }

        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            if (bleDevice != null)
            {
                if (bleDevice.DeviceId == args.Id)
                {
                    Debug.WriteLine("Removed");
                }
            }
        }

        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            if(bleDevice != null)
            {
                if (bleDevice.DeviceId == args.Id)
                {
                    Debug.WriteLine("Added");
                }
            }

        }

        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            
        }

        #endregion devicewatcher

        #region BLEadvertisementWatcher

        public async void startBLEWatcher(int seconds)
        {
            // 1. Setup search timer
            // 2. Loop through all paired devices and unpair them (BLAST YOU MS!)
            // 3. Clear discovered device lists.
            // 4. Start the search for unpaired devices.

            bleSearchTimer.Interval = new TimeSpan(0, 0, 0, seconds);
            var selector = BluetoothLEDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(selector);

            // Hacker fix.  Each time the app closes the "User Session Token" for the BLE connection
            // is lost.  To re-enable it for our new session, pairing and unpairing must be done.
            for (int i =0; i < devices.Count; i++)
            {
                await devices[i].Pairing.UnpairAsync();
            }

            clearBleSearchResults();

            bleSearchTimer.Start();
            bleAdvertWatcher.Start();
        }

        public void clearBleSearchResults()
        {
            // 1. Clear discovered BLE devices.
            // 2. Reset "Uknown" device counter.
            bleDevices.Clear();
            bleDevices.Clear();
            deviceCounter = 0;
        }


        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            // 1. Clear discovered devices
            // 2. If no device name, make one.
            // 3. Add device name and adddress, if not in the dictionary.
            // 4. Add device name and rssi, if not in dictionary.

            string deviceName = eventArgs.Advertisement.LocalName;
            var address = eventArgs.BluetoothAddress;
            short rssi = eventArgs.RawSignalStrengthInDBm;

            if(deviceName == "") { deviceName = "Unknown_" + deviceCounter; deviceCounter++; }
            
            if (!bleDevices.ContainsKey(deviceName))
            {
                bleDevices.Add(deviceName, address);
            }

            if (!bleDevices.ContainsKey(deviceName))
            {
                bleDevicesRSSI.Add(deviceName, rssi);
            }
            
            //blueEvent = BlueEvent.searchFinished;
            //callback();

            //int numberOfGattServices = eventArgs.Advertisement.ServiceUuids.Count;
            //var data = eventArgs.Advertisement.DataSections[1];
            //byte[] dataAsBA = new byte[data.Data.Length];
            //CryptographicBuffer.CopyToByteArray(data.Data, out dataAsBA);
            //string name = byteArrayToReadableString(dataAsBA);

            //Debug.WriteLine(name);


            //var = even

            //try
            //{
            //    BluetoothLEDevice device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
            //    var cnt = device.GattServices.Count;
            //    watcher.Stop();
            //    if (device.Name == "HMSoft")
            //    {
            //        DeviceInformation deviceIno = device.DeviceInformation;
            //        Debug.WriteLine(device.BluetoothAddress);
            //        device.Dispose();

            //        IDevicePairingSettings pairSettings;
            //        Debug.WriteLine(deviceIno.Pairing.ProtectionLevel);
            //        DevicePairingResult dpr = await deviceIno.Pairing.PairAsync(DevicePairingProtectionLevel.None);
            //        //var service = await GattDeviceService.FromIdAsync(eventArgs.Advertisement.ServiceUuids.ToString());
            //        Debug.WriteLine(dpr.Status);
            //        Debug.WriteLine(device.BluetoothAddress.ToString("X2"));
            //        Debug.WriteLine("Device name: " + device.Name);
            //        Debug.WriteLine("Connection status: " + device.ConnectionStatus);
            //        Debug.WriteLine(device.DeviceInformation.Kind);
            //        Debug.WriteLine(device.GattServices);
            //    } else
            //    {
            //        //watcher.Start();
            //    }
            //} catch (Exception ex)
            //{
            //    Debug.WriteLine(ex);
            //}

        }

        private void OnAdvertisementWatcherStopped(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementWatcherStoppedEventArgs eventArgs)
        {

        }

        private async void BleSearchTimer_Tick(object sender, object e)
        {
            // 1. Stop the BLE watcher, stop the search timer.
            // 2. Set the command in  process.
            // 3. Pass it to the UI.
            bleAdvertWatcher.Stop();
            bleSearchTimer.Stop();
            blueEvent = BlueEvent.searchFinished;
            callback();
        }

        #endregion BLEadvertisementWatcher

        #region BLEdevice
        private void BleDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            Debug.WriteLine("Conn. Changed: " + sender.ConnectionStatus);
        }

        public async Task connect(ulong bluetoothAddress)
        {
            // 1. Get bluetoothLE address from passed address.
            // 2. If device is not paired, pair it.
            // 3. Enumerate Gatt services.


            // Get a BLE device from address.
            bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);
            
            // Assert existing connection.
            if (bleDevice.DeviceInformation.Pairing.IsPaired == false)
            {
                // Attempt to pair the BLE device
                await connectToBLEDevice();
            }

            for (int i = 0; i < bleDevice.GattServices.Count; i++)
            {
                var characteristics = bleDevice.GattServices[i].GetAllCharacteristics();
                for (int j = 0; j < characteristics.Count; j++)
                {
                    Debug.WriteLine("Service UUID: " + characteristics[j].Service.Uuid.ToString() + "Gatt #: " + i.ToString() + " Characteristic #: " + j.ToString());
                }
            }

        }

        public async void writeToBleDevice(string sendStr)
        {
            writer = new DataWriter();

            sendStr = "Hey baby!";
            //writeToBleDevice(sendStr);

            byte[] sendPacket = GetBytes(sendStr);
            writer.WriteBytes(sendPacket);

            try
            {
                GattDeviceService serviceID = bleDevice.GattServices[2];
                Guid serviceGUUID = serviceID.Uuid;

                //Debug.WriteLine(serviceID.Uuid);
                //var service = await GattDeviceService.FromIdAsync(bleDevice.DeviceId);
                var miliService = await GattDeviceService.FromIdAsync(serviceID.DeviceId);

                Debug.WriteLine("HEYHEY! " + miliService.GetAllCharacteristics().Count);
                readWriteCharacteristic = miliService.GetAllCharacteristics()[0];
                // Will tell you what the characteristic is about (Does it allow read? Write? Etc.)
                Debug.WriteLine("Characteristic value: " + readWriteCharacteristic.CharacteristicProperties);
                readWriteCharacteristic.ProtectionLevel = GattProtectionLevel.Plain;


                //var miliService = await GattDeviceService.FromIdAsync(GatDevices[0].Id);
                var pairCharacteristic = miliService.GetAllCharacteristics().FirstOrDefault();
                var pairStatus = await readWriteCharacteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);

                //Debug.WriteLine(pairStatus);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<bool> connectToBLEDevice()
        {
            // 1. Pair the device
            // 2. Add event handlers
            // 3. Returned the pairing status.

            var connectedBleDevice = await bleDevice.DeviceInformation.Pairing.PairAsync(DevicePairingProtectionLevel.None);
            bleDevice.ConnectionStatusChanged += BleDevice_ConnectionStatusChanged;
            bleDevice.GattServicesChanged += BleDevice_GattServicesChanged;
            return bleDevice.DeviceInformation.Pairing.IsPaired;
        }

        private void BleDevice_GattServicesChanged(BluetoothLEDevice sender, object args)
        {
            Debug.WriteLine("Gatt Services Changed");
        }

        public void writeToBle(string message)
        {
            // 1. Convert string to bytes.
            // 2. Determine if bytes than CC254X buffer (20 bytes).
            // 3. Store excess bytes in txBuffer
            // 4. await Send 20 or less bytes.
            // 5. Loop through await send until txBuffer is empty.
            // 6. Return true if all bytes are written successfully.

            byte[] txBuffer = GetBytes(message);
            while (txBuffer.Length > 20)
            {
                byte[] messageAsByteArray = txBuffer.Skip(20).ToArray();
                messageAsByteArray = messageAsByteArray.Take(20).ToArray();

            }
            if (txBuffer.Length > 0)
            {

            }
        }

        public void closeBleDevice()
        {
            bleDevice.Dispose();
        }

        public void callback()
        {

            Callback(this, blueEvent);
        }

        #endregion BLEdevice

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length];
            for(int i = 0; i < str.Length; i++)
            {
                bytes[i] = (byte)str[i];
            }
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(uint)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public string byteArrayToReadableString(byte[] byteArray)
        {
            string charOrTwoCharHexString = "";

            for (int i = 0; i < byteArray.Length; i++)
            {
                //if ((int)byteArray[i] < 127)
                //{
                //    charOrTwoCharHexString += (char)byteArray[i];
                //}
                //else
                //{
                    charOrTwoCharHexString += " " + byteArray[i].ToString("X2");
                //}
            }

            //Debug.Write(bytesReadByteArray[i].ToString("X2"));
            return charOrTwoCharHexString;
        }

    } // End Blue

} // End Namespace
