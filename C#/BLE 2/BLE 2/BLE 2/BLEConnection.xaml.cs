using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using System.Diagnostics;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace BLE_2
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class BLEConnection : Page
    {
        public BLEConnection()
        {
            this.InitializeComponent();
            //visualizer.Show();
            QueryDevices();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private static readonly string DEVICE_NAME = "EA 3";
        private static readonly string SERVICE_UUID = "5A2D3BF8-F0BC-11E5-9CE9-5E5517507E66";
        private static readonly string CHARACTERISTIC_UUID = "5a2d40ee-f0bc-11e5-9ce9-5e5517507e66";

        private static readonly string LENGTH_SERVICE_UUID        = "713D0000-503E-4C75-BA94-3148F18D941E";
        private static readonly string MODE_SERVICE_UUID          = "813D0000-503E-4C75-BA94-3148F18D941E";
        private static readonly string LENGTH_CHARACTERISTIC_UUID = "713D0003-503E-4C75-BA94-3148F18D941E";
        private static readonly string MODE_CHARACTERISTIC_UUID   = "813D0003-503E-4C75-BA94-3148F18D941E";

        private List<DeviceInformation> devices = new List<DeviceInformation>();
        byte[] bytes;
        //private BLEVisualizer visualizer = new BLEVisualizer();

        private DeviceWatcher deviceWatcher;
        private bool connecting;
        private GattCharacteristic motorCharacteristic;


        public void QueryDevices()
        {
            // Query for extra properties you want returned
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            deviceWatcher = DeviceInformation.CreateWatcher(
                                BluetoothLEDevice.GetDeviceSelectorFromPairingState(false),
                                requestedProperties,
                                DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            // Added, Updated and Removed are required to get all nearby devices
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            // Start the watcher.
            deviceWatcher.Start();
        }

        public bool TactPlayFound()
        {
            foreach (DeviceInformation bleDeviceInfo in devices)
            {
                if (bleDeviceInfo.Name.Equals(DEVICE_NAME))
                {
                    return true;
                }
            }
            return false;
        }

        public async void WriteBytes()
        {

            // visualizer.AddValue(bytes);
            if (motorCharacteristic == null)
            {
                return;
            }
            var writer = new DataWriter();
            long startTime = Environment.TickCount;
            Debug.WriteLine(startTime + " Sending " + ByteArrayToString(bytes));
            writer.WriteBytes(bytes);
            GattCommunicationStatus status = await
            motorCharacteristic.WriteValueAsync(writer.DetachBuffer()); //TODO catch Exception after disconnect
            long endTime = Environment.TickCount;
            Debug.WriteLine(endTime + " Status for " + ByteArrayToString(bytes) + ": " + status + ". Time: " + (endTime - startTime) + " ms");
        }

        private void Write1(object sender, RoutedEventArgs e)
        {
            byte[] myBytes = { 0x14, 0x00, 0x24, 0x00, 0x13, 0x00, 0x23, 0x00, 0x12, 0x00, 0x22, 0x00, 0x11, 0x00, 0x21, 0x00, 0x14, 0x00, 0x24, 0x00 };
            bytes = myBytes;
            WriteBytes();
        }

        private void Write2(object sender, RoutedEventArgs e)
        {
            byte[] myBytes = { 0x14, 0x00, 0x24, 0x00, 0x14, 0x00, 0x24, 0x00, 0x14, 0x00, 0x24, 0x00, 0x14, 0x00, 0x24, 0x00, 0x14, 0x00, 0x24, 0x00 };
            bytes = myBytes;
            WriteBytes();
        }

        public async void WriteBytes(byte[] bytes)
        {
            // visualizer.AddValue(bytes);
            if (motorCharacteristic == null)
            {
                return;
            }
            var writer = new DataWriter();
            long startTime = Environment.TickCount;
            Debug.WriteLine(startTime + " Sending " + ByteArrayToString(bytes));
            writer.WriteBytes(bytes);
            GattCommunicationStatus status = await
            motorCharacteristic.WriteValueAsync(writer.DetachBuffer()); //TODO catch Exception after disconnect
            long endTime = Environment.TickCount;
            Debug.WriteLine(endTime + " Status for " + ByteArrayToString(bytes) + ": " + status + ". Time: " + (endTime - startTime) + " ms");
        }

        public async void ConnectTactPlay(object sender, RoutedEventArgs e)
        {
            lock (this)
            {
                if (connecting)
                {
                    return;
                }
                else
                {
                    connecting = true;
                }
            }
            DeviceInformation deviceInfo = GetDeviceByName(DEVICE_NAME);
            if (deviceInfo == null)
            {
                Debug.WriteLine("device info is null");
                return;
            }
            Debug.WriteLine("Connecting to " + deviceInfo.Id + "  " + deviceInfo.Name);
            BluetoothLEDevice bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id);
            Debug.WriteLine("Query service");
            GattDeviceServicesResult servicesResult = await bluetoothLeDevice.GetGattServicesAsync();
            Debug.WriteLine("Query service complete");
            if (servicesResult.Status == GattCommunicationStatus.Success)
            {
                IReadOnlyList<GattDeviceService> services = servicesResult.Services;

                foreach (GattDeviceService service in services)
                {
                    Debug.WriteLine("Service: " + service.Uuid);
                    GattCharacteristicsResult characteristicsResultOut = await service.GetCharacteristicsAsync();
                    if (characteristicsResultOut.Status == GattCommunicationStatus.Success)
                    {
                        IReadOnlyList<GattCharacteristic> characteristics = characteristicsResultOut.Characteristics;
                        foreach (GattCharacteristic characteristic in characteristics)
                        {
                            Debug.WriteLine("Characteristic: " + characteristic.Uuid);
                        }
                    }
                
                    if (service.Uuid.Equals(new Guid(LENGTH_SERVICE_UUID))) //SERVICE_UUID
                    {
                        Debug.WriteLine("Service found!");
                        GattCharacteristicsResult characteristicsResult = await service.GetCharacteristicsAsync();
                        if (characteristicsResult.Status == GattCommunicationStatus.Success)
                        {
                            IReadOnlyList<GattCharacteristic> characteristics = characteristicsResult.Characteristics;
                            foreach (GattCharacteristic characteristic in characteristics)
                            {
                                if (characteristic.Uuid.Equals(new Guid(LENGTH_CHARACTERISTIC_UUID))) // CHARACTERISTIC_UUID
                                {
                                    Debug.WriteLine("Characteristic found!");
                                    motorCharacteristic = characteristic;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Unknown service: " + service.Uuid);
                    }
                }

            }
            lock (this)
            {
                connecting = false;
            }
        }

        private DeviceInformation GetDeviceByID(string id)
        {
            foreach (DeviceInformation bleDeviceInfo in devices)
            {
                if (bleDeviceInfo.Id == id)
                {
                    return bleDeviceInfo;
                }
            }
            return null;
        }

        private DeviceInformation GetDeviceByName(string name)
        {
            foreach (DeviceInformation bleDeviceInfo in devices)
            {
                if (bleDeviceInfo.Name == name)
                {
                    return bleDeviceInfo;
                }
            }
            return null;
        }

        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            devices.Add(deviceInfo);
            Debug.WriteLine("Device added: " + deviceInfo.Id + "  " + deviceInfo.Name);
        }

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            if (GetDeviceByID(deviceInfoUpdate.Id) != null)
            {
                GetDeviceByID(deviceInfoUpdate.Id).Update(deviceInfoUpdate);
                Debug.WriteLine("Device Updated for " + deviceInfoUpdate.Id);
            }
        }

        public static byte[] StringToByteArray(String hex)
        {
            if (hex.StartsWith("0x"))
            {
                hex = hex.Substring(2);
            }
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

    }
}
