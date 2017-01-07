using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.Security.Cryptography;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth.Advertisement;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.UI.Xaml;
using System;

namespace Lumi
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

        GattDeviceService _service;

        List<BluetoothLEDevice> bleDevices;
        List<GattDeviceService> gattServices;
        List<GattCharacteristic> gattCharacteristics;

        public bool connected = false;

        GattCharacteristic readWriteCharacteristic;
        // Used for UI callback.
        public enum BlueEvent
        {
            none = 0,
            finishedConnecting = 1,
            searchFinished = 2,
            connected = 3,
            updateMessage = 4,
            paired = 5
        }

        // Callback to the main UI.
        public BlueEvent blueEvent = new BlueEvent();
        public delegate void CallBackEventHandler(object sender, BlueEvent blueEvent, string message = null);
        public event CallBackEventHandler Callback;

        // Bluetooth LE Discovery
        BluetoothLEAdvertisementWatcher bleAdvertWatcher = new BluetoothLEAdvertisementWatcher();
        public Dictionary<string, ulong> bleDeviceAddresses = new Dictionary<string, ulong>();
        public Dictionary<string, short> bleDevicesRSSI = new Dictionary<string, short>();
        public DispatcherTimer bleSearchTimer = new DispatcherTimer();
        public DispatcherTimer gattDelayPopulateTimer = new DispatcherTimer();

        // Bluetooth LE Connection
        BluetoothLEDevice bleDevice;

        DataWriter writer = new DataWriter();

        // Used for enumeration information.
        public DeviceWatcher deviceWatcher;

        // Used to determine discovery of BLE services.  
        int gattAddedCounter = 0;
        // Used to workaround MS' crappy API.
        public bool pairedUnpairedThisSession = false;
        private int deviceCounter;

        public ulong bleAddress = 0;

        private List<byte> writeBleBuffer = new List<byte>();

        #endregion properties_and_methods

        public void init()
        {
            try
            {
                rxBuffer = new List<byte>();

                bleDevices = new List<BluetoothLEDevice>();
                gattServices = new List<GattDeviceService>();
                gattCharacteristics = new List<GattCharacteristic>();

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
            } catch
            {
                Debug.WriteLine("blue.init() failed");
            }
        }

        public void attachSerialBuffer(SerialBuffer _serialBuffer)
        {
            try
            {
                serialBuffer = _serialBuffer;
                serialBuffer.RXbufferUpdated += new SerialBuffer.CallBackEventHandler(RXbufferUpdated);
                serialBuffer.TXbufferUpdated += new SerialBuffer.CallBackEventHandler(TXbufferUpdated);
            } catch
            {
                Debug.WriteLine("blue.attachSerialBuffer() failed");
            }

        }

        public void detachSerialBuffer()
        {
            try
            {
                serialBuffer.RXbufferUpdated -= new SerialBuffer.CallBackEventHandler(RXbufferUpdated);
                serialBuffer.TXbufferUpdated -= new SerialBuffer.CallBackEventHandler(TXbufferUpdated);
                serialBuffer = null;
            } catch
            {
                Debug.WriteLine("blue.detachSerialBuffer() failed");
            }

        }

        private void RXbufferUpdated(object sender, EventArgs args)
        {
            Debug.WriteLine("blue Callback for RX bufferUpdated");
        }

        public async void TXbufferUpdated(object sender, EventArgs args)
        {
            try
            {
                int numberOfBytes = serialBuffer.bytesInTxBuffer();
                var bytesToWrite = serialBuffer.ReadFromTxBuffer(numberOfBytes);
                List<byte> listBA = new List<byte>();
                listBA.AddRange(bytesToWrite);
                if (listBA.Contains(0x21)) { Debug.WriteLine("Contains Confirm");

                }
                foreach (var i in listBA)
                {
                    //Debug.Write(i.ToString("X2"));
                }
                Debug.WriteLine("");
                await writeByteArrayToBle(bytesToWrite);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        #region devicewatcher

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {

            if (bleDevice != null)
            {

                if (bleDevice.DeviceInformation.Id == args.Id)
                {
                    Debug.WriteLine("Total Gatts added: " + gattAddedCounter);
                    gattAddedCounter++;
                    // This would be better if BluetoothAdvertisementWatcher discovered Gatt services
                    // rather than presuming them.
                    if (gattAddedCounter == 7)
                    {
                        Callback(this, BlueEvent.none);
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
            if (bleDevice != null)
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
            Debug.WriteLine("DeviceWatcher_EnumerationCompleted");

        }

        #endregion devicewatcher

        #region BLEadvertisementWatcher

        public async Task startBLEWatcher(int seconds)
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
            for (int i = 0; i < devices.Count; i++)
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

            // discoverChars(eventArgs.BluetoothAddress);

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

        private void BleSearchTimer_Tick(object sender, object e)
        {
            // 1. Stop the BLE watcher, stop the search timer.
            // 2. Set the command in  process.
            // 3. Pass it to the UI.
            bleAdvertWatcher.Stop();
            bleSearchTimer.Stop();
            Callback(this, BlueEvent.searchFinished);
        }

        #endregion BLEadvertisementWatcher

        #region BLEdevice

        private void BleDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            Debug.WriteLine("Conn. Changed: " + sender.ConnectionStatus);
        }

        public async Task<bool> discoverChars(ulong address)
        {
            try
            {
                var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);
                bleDevice = device;
                var services = device.GattServices;

                foreach (GattDeviceService service in services)
                {
                    service.Device.ConnectionStatusChanged += OnConnectionStatusChanged;
                    gattServices.Add(service);
                    var characteristics = service.GetAllCharacteristics();
                    foreach (GattCharacteristic characteristic in characteristics)
                    {
                        try
                        {
                            Debug.WriteLine(characteristic.CharacteristicProperties);
                            Debug.WriteLine(characteristic.ProtectionLevel);
                            var status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                            characteristic.ValueChanged += Oncharacteristic_ValueChanged;
                            gattCharacteristics.Add(characteristic);
                            writeToChar("AT+AFTC200");
                            Callback(this, BlueEvent.connected);

                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
                return true;
            } catch
            {
                Debug.WriteLine("blue.discoverChars() failed");
                return false;
            }

        }

        public async void writeToChar(string str)
        {
            try
            {
                byte[] tmpBfr = GetBytes(str);
                await writeByteArrayToBle(tmpBfr);
            } catch
            {
                Debug.WriteLine("blue.writeToChar() failed");
            }
        }

        private async void GattDelayPopulateTimer_Tick(object sender, object e)
        {
            try

            {
                await discoverChars(bleAddress);
                connected = true;
                gattDelayPopulateTimer.Stop();
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Did not finish populating Gatt options");
                gattDelayPopulateTimer.Start();
            }

        }

        private async Task<bool> connectToBLEDevice(DeviceInformation device)
        {
            // 1. Attempt to connect, if fail, try again.
            // 2. After 3 times, provide error message.
            // 3. If connected, provide connection status to main UI.
            // 4. Return the pairing status.

            DevicePairingResult pairingStatus = null; ;
            int pairingAttemptCounter = 0;
            try
            {
                do
                {
                    pairingStatus = await device.Pairing.PairAsync();
                    pairingAttemptCounter++;
                } while (pairingStatus.Status != DevicePairingResultStatus.Paired && pairingAttemptCounter < 3);

               
                switch (pairingStatus.Status)
                {
                    case DevicePairingResultStatus.AccessDenied:
                        break;
                    case DevicePairingResultStatus.AlreadyPaired:
                        break;
                    case DevicePairingResultStatus.AuthenticationFailure:
                        break;
                    case DevicePairingResultStatus.AuthenticationNotAllowed:
                        break;
                    case DevicePairingResultStatus.AuthenticationTimeout:
                        break;
                    case DevicePairingResultStatus.ConnectionRejected:
                        break;
                    case DevicePairingResultStatus.Failed:
                        break;
                    case DevicePairingResultStatus.HardwareFailure:
                        break;
                    case DevicePairingResultStatus.InvalidCeremonyData:
                        break;
                    case DevicePairingResultStatus.NoSupportedProfiles:
                        break;
                    case DevicePairingResultStatus.NotPaired:
                        break;
                    case DevicePairingResultStatus.NotReadyToPair:
                        break;
                    case DevicePairingResultStatus.OperationAlreadyInProgress:
                        break;
                    case DevicePairingResultStatus.Paired:
                        break;
                    case DevicePairingResultStatus.PairingCanceled:
                        break;
                    case DevicePairingResultStatus.ProtectionLevelCouldNotBeMet:
                        break;
                    case DevicePairingResultStatus.RejectedByHandler:
                        break;
                    case DevicePairingResultStatus.RemoteDeviceHasAssociation:
                        break;
                    case DevicePairingResultStatus.RequiredHandlerNotRegistered:
                        break;
                    case DevicePairingResultStatus.TooManyConnections:
                        break;
                }
                Callback(this, BlueEvent.paired, pairingStatus.Status.ToString()+"\n");               
                return bleDevice.DeviceInformation.Pairing.IsPaired;
            } catch
            {
                Debug.WriteLine("blue.connectToBLEDevice() failed");
                return false;
            }

        }

        public async Task connect(ulong bluetoothAddress)
        {
            // 1. Get bluetoothLE address from passed address.
            // 2. If device is not paired, pair it.
            // 3. Enumerate Gatt services.

            try
            {
                // Get a BLE device from address.
                bleDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);
                bleAddress = bluetoothAddress;
                DeviceInformation bleDeviceInfo = bleDevice.DeviceInformation;

                // Assert existing connection.
                if (bleDevice.DeviceInformation.Pairing.IsPaired == false)
                {
                    // Attempt to pair the BLE device
                    await connectToBLEDevice(bleDeviceInfo);
                    gattDelayPopulateTimer.Interval = new TimeSpan(0, 0, 8);
                    gattDelayPopulateTimer.Start();
                }
            } catch
            {
                Debug.WriteLine("blue.connect() failed");
            }


        }
        
        private void Oncharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try
            {
                byte[] charValueByteArray = new byte[args.CharacteristicValue.Length];
                
                Debug.WriteLine(CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, args.CharacteristicValue));
                CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out charValueByteArray);
                serialBuffer.RxBuffer = charValueByteArray;
                
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                byte[] charValueByteArray = new byte[args.CharacteristicValue.Length];
                CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out charValueByteArray);
                serialBuffer.RxBuffer = charValueByteArray;
            }
            
        }

        private void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
       
        }

        public async void writeStringToBle(string sendStr)
        {
            try
            {
                writer = new DataWriter();

                byte[] sendPacket = GetBytes(sendStr);
                writer.WriteBytes(sendPacket);

                foreach (GattCharacteristic gattCharacteristic in gattCharacteristics)
                {
                    var pairStatus = await gattCharacteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("blue.writeStringToBle() failed");
            }
        }

        public async Task<bool> writeByteArrayToBle(byte[] sendPacket)
        {
            // 1. Convert string to bytes.
            // 2. Determine if bytes than CC254X buffer (20 bytes).
            // 3. Store excess bytes in txBuffer
            // 4. await Send 20 or less bytes.
            // 5. Loop through await send until txBuffer is empty.
            // 6. Return true if all bytes are written successfully.

            try
            {
                writeBleBuffer.AddRange(sendPacket);
                //if(writeBleBuffer.Contains(0x21)){ Debug.WriteLine("Contains Confirm"); }

                foreach (GattCharacteristic gattCharacteristic in gattCharacteristics)
                {
                    Debug.WriteLine(gattCharacteristic.AttributeHandle);
                    while (writeBleBuffer.Count > 0)
                    {
                        writer = new DataWriter();
                        byte[] data = writeBleBuffer.Take(20).ToArray();
                        writer.WriteBytes(data);
                        try
                        {
                            var pairStatus = await gattCharacteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithoutResponse);
                            if (writeBleBuffer.Count > 20)
                            {
                                writeBleBuffer.RemoveRange(0, 20);
                            }
                            else
                            {
                                writeBleBuffer.RemoveRange(0, writeBleBuffer.Count);
                            }
                            //return true;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                    Debug.WriteLine("BLE leftover bytes: " + writeBleBuffer.Count);
                    return true;
                }
                return false;
            } catch
            {
                Debug.WriteLine("blue.writeByteArrayToBle");
                return false;
            }

        }


        private void BleDevice_GattServicesChanged(BluetoothLEDevice sender, object args)
        {
            Debug.WriteLine("Gatt Services Changed");
        }

        public void closeBleDevice()
        {
            try
            {
                connected = false;
                if (bleDevice != null)
                {
                    var services = bleDevice.GattServices;

                    foreach (GattDeviceService service in services)
                    {
                        service.Device.ConnectionStatusChanged -= OnConnectionStatusChanged;
                        var characteristics = service.GetAllCharacteristics();
                        foreach (GattCharacteristic characteristic in characteristics)
                        {
                            try
                            {
                                characteristic.ValueChanged -= Oncharacteristic_ValueChanged;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }
                    }

                    bleDevice.Dispose();
                    detachSerialBuffer();
                    gattCharacteristics.Clear();
                }
            } catch
            {
                Debug.WriteLine("blue.closeBleDevice() failed");
            }

        }

        #endregion BLEdevice

        public static byte[] GetBytes(string str)
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
                Debug.WriteLine("blue.GetBytes() failed");
                return null;
            }
        }

        static string GetString(byte[] bytes)
        {
            try
            {
                char[] chars = new char[bytes.Length / sizeof(uint)];
                System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
                return new string(chars);
            } catch
            {
                Debug.WriteLine("blue.GetString() failed");
                return "";
            }

        }

        public string byteArrayToReadableString(byte[] byteArray)
        {
            try
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
            } catch
            {
                Debug.WriteLine("blue.byteArrayToReadableString() failed");
                return "";
            }

        }


    } // End Blue

} // End Namespace