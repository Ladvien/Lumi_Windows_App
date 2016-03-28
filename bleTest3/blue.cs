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

namespace bleTest3
{


    public class blue
    {
        public enum BlueEvent
        {
            finishedConnecting = 0,
        }

        public BlueEvent blueEvent = new BlueEvent();
        public delegate void CallBackEventHandler(object sender, BlueEvent blueEvent);
        public event CallBackEventHandler Callback;

        BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();
        BluetoothLEDevice bleDevice;
        GattCharacteristic readWriteCharacteristic;
        DataWriter writer = new DataWriter();
        DeviceWatcher deviceWatcher;

        RichTextBlock mainDisplay = new RichTextBlock();

        int gattAddedCounter = 0;

        public bool pairedUnpairedThisSession = false;

        private byte[] _txBuffer;
        byte[] txBuffer
        {
            set
            {
                IBuffer buffer = CryptographicBuffer.CreateFromByteArray(value);
                CryptographicBuffer.CopyToByteArray(buffer, out _txBuffer);
            }
            get { return _txBuffer; }

        }

        public byte[] bytesFromTxBuffer(int numberOfBytes)
        {
            // 1. Get the characters to return: Range<0, numberOfBytes>
            // 2. Remove the number of bytes from the buffer.
            // 3. Return the wanted bytes.
            if (txBuffer.Length > 0)
            {
                byte[] returnBytes = txBuffer.Take(numberOfBytes).ToArray();
                txBuffer = txBuffer.Skip(numberOfBytes).Take(txBuffer.Length - numberOfBytes).ToArray();
                return returnBytes;
            }
            else
            {
                byte[] empty = { 0x00 };
                return empty;
            }

        }

        public async void init()
        {
            // Create and initialize a new watcher instance.
            watcher = new BluetoothLEAdvertisementWatcher();

            // Part 1B: Configuring the signal strength filter for proximity scenarios

            // Configure the signal strength filter to only propagate events when in-range
            // Please adjust these values if you cannot receive any advertisement 
            // Set the in-range threshold to -70dBm. This means advertisements with RSSI >= -70dBm 
            // will start to be considered "in-range".
            //watcher.SignalStrengthFilter.InRangeThresholdInDBm = -70;

            // Set the out-of-range threshold to -75dBm (give some buffer). Used in conjunction with OutOfRangeTimeout
            // to determine when an advertisement is no longer considered "in-range"
            //watcher.SignalStrengthFilter.OutOfRangeThresholdInDBm = -75;

            // Set the out-of-range timeout to be 2 seconds. Used in conjunction with OutOfRangeThresholdInDBm
            // to determine when an advertisement is no longer considered "in-range"
            watcher.SignalStrengthFilter.OutOfRangeTimeout = TimeSpan.FromMilliseconds(4000);

            // By default, the sampling interval is set to zero, which means there is no sampling and all
            // the advertisement received is returned in the Received event

            // End of watcher configuration. There is no need to comment out any code beyond this point.
            deviceWatcher = DeviceInformation.CreateWatcher();
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            //deviceWatcher.
            deviceWatcher.Start();
            watcher = new BluetoothLEAdvertisementWatcher();
            //watcher.ScanningMode = BluetoothLEScanningMode.Active;

            watcher.Received += OnAdvertisementReceived;
            watcher.Stopped += OnAdvertisementWatcherStopped;


        }

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
                        hmsoftFinishedEnumerating();
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
            
            Debug.WriteLine("Enumerated");
        }

        private void BleDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            Debug.WriteLine("Conn. Changed: "+ sender.ConnectionStatus);
        }

        private async void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {

            var address = eventArgs.BluetoothAddress;
            try
            {
                BluetoothLEDevice device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
                var cnt = device.GattServices.Count;
                watcher.Stop();
                if (device.Name == "HMSoft")
                {
                    DeviceInformation deviceIno = device.DeviceInformation;
                    Debug.WriteLine(device.BluetoothAddress);
                    device.Dispose();

                    IDevicePairingSettings pairSettings;
                    Debug.WriteLine(deviceIno.Pairing.ProtectionLevel);
                    DevicePairingResult dpr = await deviceIno.Pairing.PairAsync(DevicePairingProtectionLevel.None);
                    //var service = await GattDeviceService.FromIdAsync(eventArgs.Advertisement.ServiceUuids.ToString());
                    Debug.WriteLine(dpr.Status);
                    Debug.WriteLine(device.BluetoothAddress);
                    Debug.WriteLine("Device name: " + device.Name);
                    Debug.WriteLine("Connection status: " + device.ConnectionStatus);
                    Debug.WriteLine(device.DeviceInformation.Kind);
                    Debug.WriteLine(device.GattServices);
                } else
                {
                    //watcher.Start();
                }
            } catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            
        }

        /// <summary>
        /// Invoked as an event handler when the watcher is stopped or aborted.
        /// </summary>
        /// <param name="watcher">Instance of watcher that triggered the event.</param>
        /// <param name="eventArgs">Event data containing information about why the watcher stopped or aborted.</param>
        private void OnAdvertisementWatcherStopped(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementWatcherStoppedEventArgs eventArgs)
        {

        }

        public async Task connect(ulong bluetoothAddress)
        {
            // Get a BLE device from address.
            bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);

            // Assert existing connection.
            if (bleDevice.DeviceInformation.Pairing.IsPaired == false)
            {
                // Attempt to pair the BLE device
                await connectToBLEDevice();
            }
            else
            {
                // Hacker fix.  Each time the app closes the "User Session Token" for the BLE connection
                // is lost.  To re-enable it for our new session, pairing and unpairing must be done.
                if(pairedUnpairedThisSession == false)
                {
                    await closeBleDevice();
                    await connectToBLEDevice();
                    pairedUnpairedThisSession = true;
                }
                
                for (int i = 0; i < bleDevice.GattServices.Count; i++)
                {
                    var characteristics = bleDevice.GattServices[i].GetAllCharacteristics();
                    for(int j = 0; j < characteristics.Count; j++)
                    {
                        Debug.WriteLine("Service UUID: " + characteristics[j].Service.Uuid.ToString() + "Gatt #: " +i.ToString() + " Characteristic #: "+j.ToString());
                    }
                }
                //Debug.WriteLine(bleDevice.GattServices.Count);

                writer = new DataWriter();

                string sendStr = "Hey baby!";
                writeToBleDevice(sendStr);

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
                    //var pairCharacteristic = miliService.GetAllCharacteristics().FirstOrDefault();

                    
                    //var pairStatus = await readWriteCharacteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);
                    //Debug.WriteLine(pairStatus);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private async Task<bool> connectToBLEDevice()
        {
            var hmsoft = await bleDevice.DeviceInformation.Pairing.PairAsync(DevicePairingProtectionLevel.None);
            // Add event methods.
            bleDevice.ConnectionStatusChanged += BleDevice_ConnectionStatusChanged;
            bleDevice.GattServicesChanged += BleDevice_GattServicesChanged;
            return bleDevice.DeviceInformation.Pairing.IsPaired;
        }

        private void BleDevice_GattServicesChanged(BluetoothLEDevice sender, object args)
        {
            Debug.WriteLine("Gatt Services Changed");
        }

        public async Task closeBleDevice()
        {
            var pairingStatus = await bleDevice.DeviceInformation.Pairing.UnpairAsync();
            //Debug.WriteLine(pairingStatus);
        }

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

        public void writeToBleDevice(string message)
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
            if(txBuffer.Length > 0)
            {
                
            }
        }

        public void hmsoftFinishedEnumerating()
        {
            
            Callback(this, blueEvent);
        }

    } // End Blue


    

} // End Namespace
