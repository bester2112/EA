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

using static TestApp.BLE;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Core;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace TestApp
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // BLE VARS
        private ObservableCollection<DeviceInformation> BTDevices = new ObservableCollection<DeviceInformation>();
        private DeviceWatcher deviceWatcher;

        private BluetoothLEDevice CurrentBTDevice { get; set; }
        private DeviceInformation CurrentBTDeviceInfo { get; set; }

        // TODO: Firmware -> get FREQ/w and MODE/w into one control service
        private BLEAttributeDisplayContainer CurrentFreqService { get; set; }
        private BLEAttributeDisplayContainer CurrentFreqCharacteristic { get; set; }
        private BLEAttributeDisplayContainer CurrentModeService { get; set; }
        private BLEAttributeDisplayContainer CurrentModeCharacteristic { get; set; }
        private ObservableCollection<BLEAttributeDisplayContainer> currentServiceCollection
          = new ObservableCollection<BLEAttributeDisplayContainer>();

        private readonly Guid FREQ_SERVICE_UUID = new Guid("713D0000-503E-4C75-BA94-3148F18D941E");
        private readonly Guid MODE_SERVICE_UUID = new Guid("813D0000-503E-4C75-BA94-3148F18D941E");
        private readonly Guid FREQ_CHARACTERISTIC_UUID = new Guid("713D0003-503E-4C75-BA94-3148F18D941E");
        private readonly Guid MODE_CHARACTERISTIC_UUID = new Guid("813D0003-503E-4C75-BA94-3148F18D941E");
        

        private Byte NumPoints { get; set; }
        private Byte Mode { get; set; }
        private Boolean Animate { get; set; }

        private readonly double[] defPoints = {  2,  5,  -2,  7, -2,
                                                -1, -1,  3, -2, -3,
                                                 6,  7,  0, -3, -7,
                                                 0,  5, -4,  2, 10 };

        private const Byte MAX_POINTS = 20; // ~ BLE-Buffersize

        private const Byte IA_TACTILE_MAXFREQ = 10;
        private const Byte IA_TACTILE_NULFREQ = 0x0A;
        private const Byte IA_TACTILE_EOD = 0xFF; // end of data

        private const Byte IA_TACTILE_MODE_STOP = 0x00;
        private const Byte IA_TACTILE_MODE_DEF = 0x01;
        private const Byte IA_TACTILE_MODE_ALT = 0x02;

        private const Int16 IA_TACTILE_DELAY = 265;
        private const Int16 IA_TACTILE_TIMESTEP = 2000;

        private Int16[] frequencies = new Int16[MAX_POINTS - 1];


        public MainPage()
        {
            this.InitializeComponent();

            // BLE watcher init.
            listViewDevices.ItemsSource = BTDevices;
            listViewDevices.DisplayMemberPath = "Name";
            StartBleDeviceWatcher();
        }

        /// <summary>
        /// Starts a device watcher that looks for all nearby BT devices (paired or unpaired).
        /// </summary>
        private void StartBleDeviceWatcher()
        {
            // Additional properties we would like about the device.
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            // BT_Code: Currently Bluetooth APIs don't provide a selector to get ALL devices that are both paired and non-paired.
            deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")",
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;

            // Start over with an empty collection.
            BTDevices.Clear();

            // Start the watcher.
            deviceWatcher.Start();
        }

        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Added {0} {1}", deviceInfo.Id, deviceInfo.Name));

                    if (sender == deviceWatcher)
                        if (deviceInfo.Name != String.Empty && !BTDevices.Any<DeviceInformation>(x => x.Id == deviceInfo.Id))
                            BTDevices.Add(deviceInfo);
                }
            });
        }

        private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {

            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Updated {0} {1}", deviceInfoUpdate.Id, " "));

                    if (sender == deviceWatcher)
                        if (BTDevices.Any<DeviceInformation>(x => x.Id == deviceInfoUpdate.Id))
                            BTDevices.First<DeviceInformation>(x => x.Id == deviceInfoUpdate.Id).Update(deviceInfoUpdate);
                }
            });
        }

        private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Removed {0} {1}", deviceInfoUpdate.Id, ""));

                    if (sender == deviceWatcher)
                        if (BTDevices.Any<DeviceInformation>(x => x.Id == deviceInfoUpdate.Id))
                            BTDevices.Remove(BTDevices.First<DeviceInformation>(x => x.Id == deviceInfoUpdate.Id));
                }
            });
        }

        /// <summary>
        ///    Disposes of everything associated with the currently connected device. </summary>
        ///    
        private void DisposeCurrentDevice()
        {
            try
            { /* CurrentBTDevice.ConnectionStatusChanged -= CurrentBTDevice_ConnectionStatusChanged; <- make */ }
            catch
            { }

            try
            {
                CurrentFreqService = null;
                CurrentModeService = null;
                CurrentFreqCharacteristic = null;
                CurrentModeCharacteristic = null;
            }
            catch { }

            CurrentBTDevice?.Dispose();
            CurrentBTDevice = null;
            CurrentBTDeviceInfo = null;
            currentServiceCollection = new ObservableCollection<BLEAttributeDisplayContainer>();
        }

        /// <summary>
        ///    Connect to a device. </summary>
        ///    
        /// <param name = "dev">
        ///    The device information instance. </param>
        ///    
        private async void Connect(DeviceInformation dev)
        {
            button_connect.IsEnabled = false;

            if (CurrentBTDevice != null) // disconnect pressed
            {
                DisposeCurrentDevice();
                return;
            }

            DisposeCurrentDevice();

            try { CurrentBTDevice = await BluetoothLEDevice.FromIdAsync(dev.Id); }
            catch { CurrentBTDeviceInfo = null; }

            if (CurrentBTDevice != null)
            {
                foreach (var service in CurrentBTDevice.GattServices)
                    currentServiceCollection.Add(new BLEAttributeDisplayContainer(service));

                // TODO: adding this made the device slower
                // or was it just the battery? :/

                // TODO: Error prompt
            }
            else
            {
                DisposeCurrentDevice();
            }
        }

        /// <summary>
        ///    Collects the wanted GATT services from the device. </summary>
        /// <returns>
        ///    True on success, false on fail. </returns>
        ///    
        private bool GetServices()
        {

            //CurrentFreqService = 
            //    currentServiceCollection.FirstOrDefault<BLEAttributeDisplayContainer>(x => x.service.Uuid.CompareTo(FREQ_SERVICE_UUID) == 0);
            //CurrentModeService = 
            //    currentServiceCollection.FirstOrDefault<BLEAttributeDisplayContainer>(x => x.service.Uuid.CompareTo(MODE_SERVICE_UUID) == 0);
            CurrentFreqService = GetSingleService(FREQ_SERVICE_UUID);
            CurrentModeService = GetSingleService(MODE_SERVICE_UUID);

            return (CurrentFreqService != null) &&
                   (CurrentModeService != null);
        }

        /// <summary>
        ///    Gets the service with the given UUID if possible. </summary>
        ///    
        /// <param name = "serviceUUID">
        ///    GUID/UUID of the wanted service. </param>
        /// 
        /// <returns>
        ///    The service. </returns>
        ///    
        private BLEAttributeDisplayContainer GetSingleService(Guid serviceUUID)
        {
            var service = currentServiceCollection.FirstOrDefault<BLEAttributeDisplayContainer>
                (x => x.service.Uuid.CompareTo(serviceUUID) == 0);

            return service;
        }

        /// <summary>
        ///    Collects the wanted GATT characteristics from the device. </summary>
        /// 
        /// <returns>
        ///    True on success, false on fail. </returns>
        ///     
        private bool GetCharacteristics()
        {
            //if (GetServices())
            //{
            //    IReadOnlyList<GattCharacteristic> freqCharacteristics = null;
            //    IReadOnlyList<GattCharacteristic> modeCharacteristics = null;

            //    try
            //    {
            //        freqCharacteristics = CurrentFreqService.service.GetAllCharacteristics();
            //        modeCharacteristics = CurrentModeService.service.GetAllCharacteristics();
            //    }
            //    catch
            //    {
            //        freqCharacteristics = null;
            //        modeCharacteristics = null;
            //        return false;
            //    }

            //    List<BLEAttributeDisplayContainer> freqCharCollection = new List<BLEAttributeDisplayContainer>();
            //    List<BLEAttributeDisplayContainer> modeCharCollection = new List<BLEAttributeDisplayContainer>();

            //    foreach (GattCharacteristic c in freqCharacteristics)
            //        freqCharCollection.Add(new BLEAttributeDisplayContainer(c));
            //    foreach (GattCharacteristic c in modeCharacteristics)
            //        modeCharCollection.Add(new BLEAttributeDisplayContainer(c));

            //    // now actually getting chars for later use
            //    CurrentFreqCharacteristic = 
            //        freqCharCollection.FirstOrDefault<BLEAttributeDisplayContainer>(x => x.characteristic.Uuid.CompareTo(FREQ_CHARACTERISTIC_UUID) == 0);
            //    CurrentModeCharacteristic = 
            //        modeCharCollection.FirstOrDefault<BLEAttributeDisplayContainer>(x => x.characteristic.Uuid.CompareTo(MODE_CHARACTERISTIC_UUID) == 0);
            //}
            //else
            //    return false;

            //return true;

            if (GetServices())
            {
                CurrentFreqCharacteristic = GetSingleCharacteristic(FREQ_CHARACTERISTIC_UUID, CurrentFreqService);
                CurrentModeCharacteristic = GetSingleCharacteristic(MODE_CHARACTERISTIC_UUID, CurrentModeService);
            }

            return (CurrentFreqCharacteristic != null) &&
                   (CurrentModeCharacteristic != null);
        }

        /// <summary>
        ///    Gets the characteristic with the given UUID from the
        ///    given service if possible. </summary>
        ///    
        /// <param name = "charUUID">
        ///    GUID/UUID of the wanted characteristic. </param>
        /// 
        /// <param name = "service">
        ///    The service to get the characteristic from. </param>
        /// 
        /// <returns>
        ///    The characteristic. </returns>
        /// 
        private BLEAttributeDisplayContainer GetSingleCharacteristic(Guid charUUID, BLEAttributeDisplayContainer service)
        {
            IReadOnlyList<GattCharacteristic> ServiceChars = null;
            try { ServiceChars = service.service.GetAllCharacteristics(); }
            catch { return null; }

            List<BLEAttributeDisplayContainer> CharCollection = new List<BLEAttributeDisplayContainer>();
            foreach (GattCharacteristic gattchar in ServiceChars)
                CharCollection.Add(new BLEAttributeDisplayContainer(gattchar));

            var characteristic = CharCollection.FirstOrDefault<BLEAttributeDisplayContainer>
                (x => x.characteristic.Uuid.CompareTo(charUUID) == 0);

            return characteristic;
        }


        private void ConnectOnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentBTDeviceInfo != null) // already connected -> disconnect
            {
                DisposeCurrentDevice();
                return;
            }

            if (listViewDevices.SelectedItem != null &&
                listViewDevices.SelectedItem is DeviceInformation)
            {
                var devInfo = listViewDevices.SelectedItem as DeviceInformation;
                Connect(devInfo);
            }
            else /* do nothing and */ return;
            
        }

        private async void sendData_Click(object sender, RoutedEventArgs e)
        {
            Byte[] writerValues = new Byte[NumPoints];
            Byte[] writerMode = { Mode };

            Byte[] myTestByte = { 0x14, 0x00, 0x24, 0x00, 0x13, 0x00, 0x23, 0x00, 0x12, 0x00, 0x22, 0x00, 0x11, 0x00, 0x21, 0x00, 0x14, 0x00, 0x24, 0x00 };


            for (int i = 0; i < frequencies.Length; i++)
                writerValues[i] = (Byte)frequencies.ElementAt(i);

            if (frequencies.Length < MAX_POINTS - 1) // add 'end of data' byte if needed
                writerValues[frequencies.Length] = IA_TACTILE_EOD;

            /* status1= */
            await CurrentFreqCharacteristic.characteristic.WriteValueAsync(myTestByte.AsBuffer());
            /* status2= */
            await CurrentModeCharacteristic.characteristic.WriteValueAsync(writerMode.AsBuffer());
        }
    }
}
