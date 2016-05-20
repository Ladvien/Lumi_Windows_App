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

        DataReader dataReaderObject = null;

        List<byte> rxBuffer;

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
        public Dictionary<string, ulong> bleDeviceAddresses = new Dictionary<string, ulong>();
        public Dictionary<string, short> bleDevicesRSSI = new Dictionary<string, short>();
        public DispatcherTimer bleSearchTimer = new DispatcherTimer();
        public DispatcherTimer gattDelayPopulateTimer = new DispatcherTimer();

        // Bluetooth LE Connection
        BluetoothLEDevice bleDevice;

        GattCharacteristic readWriteCharacteristic;
        DataWriter writer = new DataWriter();

        // Used for enumeration information.
        public DeviceWatcher deviceWatcher;

        // Used to determine discovery of BLE services.  
        int gattAddedCounter = 0;
        // Used to workaround MS' crappy API.
        public bool pairedUnpairedThisSession = false;
        private int deviceCounter;

        public ulong bleAddress = 0;
        #endregion properties_and_methods

        public void init(SerialBuffer _serialBuffer)
        {
            serialBuffer = _serialBuffer;
            rxBuffer = new List<byte>();
            // Create and initialize a new watcher instance.
            bleAdvertWatcher = new BluetoothLEAdvertisementWatcher();

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
            gattDelayPopulateTimer.Tick += GattDelayPopulateTimer_Tick;

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

                if (bleDevice.DeviceInformation.Id == args.Id)
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

        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation device)
        {
            if(bleDevice != null)
            {
                
                if (bleDevice.Name == device.Name)
                {
                    try
                    {
                        var service = await GattDeviceService.FromIdAsync(device.Id);
                        Debug.WriteLine("Added");
                        //populateCharacteristics(service);
                    } catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                }
            }

        }

        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            Debug.WriteLine("HERE");
            
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
            bleDeviceAddresses.Clear();
            deviceCounter = 0;
        }


        private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            // 1. Clear discovered devices
            // 2. If no device name, make one.
            // 3. Add device name and adddress, if not in the dictionary.
            // 4. Add device name and rssi, if not in dictionary.

            string deviceName = eventArgs.Advertisement.LocalName;

            if (deviceName != "")
            {
                var address = eventArgs.BluetoothAddress;
                short rssi = eventArgs.RawSignalStrengthInDBm;

                //if (deviceName == "") { deviceName = "Unknown_" + deviceCounter; deviceCounter++; }

                if (!bleDeviceAddresses.ContainsKey(deviceName))
                {
                    bleDeviceAddresses.Add(deviceName, address);
                }

                if (!bleDeviceAddresses.ContainsKey(deviceName))
                {
                    bleDevicesRSSI.Add(deviceName, rssi);
                }
            }

            
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
        private async void BleDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            Debug.WriteLine("Conn. Changed: " + sender.ConnectionStatus);

        }

        private async void GattDelayPopulateTimer_Tick(object sender, object e)
        {
            bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(bleAddress);
            var services = bleDevice.GattServices;
            foreach (GattDeviceService service in services)
            {
                service.Device.ConnectionStatusChanged += OnConnectionStatusChanged;
                var characteristics = service.GetAllCharacteristics();
                foreach (GattCharacteristic characteristic in characteristics)
                {
                    try
                    {
                        await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                        characteristic.ValueChanged += Oncharacteristic_ValueChanged;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }                    
                }
            }
            gattDelayPopulateTimer.Stop();
        }

        private async Task<bool> connectToBLEDevice(DeviceInformation device)
        {
            // 1. Pair the device
            // 2. Add event handlers
            // 3. Returned the pairing status.
            var connectedBleDevice = await device.Pairing.PairAsync(DevicePairingProtectionLevel.None);
            return bleDevice.DeviceInformation.Pairing.IsPaired;
        }

        public async Task connect(ulong bluetoothAddress)
        {
            // 1. Get bluetoothLE address from passed address.
            // 2. If device is not paired, pair it.
            // 3. Enumerate Gatt services.

            // Get a BLE device from address.
            bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);

            bleAddress = bluetoothAddress;
            DeviceInformation bleDeviceInfo = bleDevice.DeviceInformation;

            bleDevice.Dispose();

            // Assert existing connection.
            if (bleDevice.DeviceInformation.Pairing.IsPaired == false)
            {
                // Attempt to pair the BLE device
                await connectToBLEDevice(bleDeviceInfo);
                gattDelayPopulateTimer.Interval = new TimeSpan(0, 0, 5);
                gattDelayPopulateTimer.Start();

            }

        }
        
        private void Oncharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try
            {
                byte[] charValueByteArray = new byte[args.CharacteristicValue.Length];
                Debug.Write(CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, args.CharacteristicValue));
                CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out charValueByteArray);
                serialBuffer.RxBuffer = charValueByteArray;
                
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }

        private void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
       
        }

        public async void writeToBleDevice(string sendStr)
        {
            writer = new DataWriter();

            sendStr = "Hey baby!";

            byte[] sendPacket = GetBytes(sendStr);
            writer.WriteBytes(sendPacket);

            try
            {
                GattDeviceService serviceID = bleDevice.GattServices[2];
                Guid serviceGUUID = serviceID.Uuid;

                var miliService = await GattDeviceService.FromIdAsync(serviceID.DeviceId);

                Debug.WriteLine("HEYHEY! " + miliService.GetAllCharacteristics().Count);
                readWriteCharacteristic = miliService.GetAllCharacteristics()[0];
                // Will tell you what the characteristic is about (Does it allow read? Write? Etc.)
                Debug.WriteLine("Characteristic value: " + readWriteCharacteristic.CharacteristicProperties);
                readWriteCharacteristic.ProtectionLevel = GattProtectionLevel.Plain;
                var pairCharacteristic = miliService.GetAllCharacteristics().FirstOrDefault();
                var pairStatus = await readWriteCharacteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);

                var read = await readWriteCharacteristic.ReadValueAsync();

                Debug.WriteLine(read.Value);

                
                Debug.WriteLine(CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, read.Value));

                try
                {
                    await readWriteCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                readWriteCharacteristic.ValueChanged += Oncharacteristic_ValueChanged;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
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
            rxBuffer.AddRange(tempByteArray);

            string fancyString = byteArrayToReadableString(rxBuffer.ToArray());

            if (bytesRead > 0)
            {
                serialBuffer.RxBuffer = tempByteArray;
                //appendText(fancyString, Colors.Red); 
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
