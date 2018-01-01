using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using static EA3.BLE;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace EA3
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        SignalTyp untypedSignal;
        MainProgram setup;

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
        private Byte Mode { get; set; }
        private ObservableCollection<BLEAttributeDisplayContainer> currentServiceCollection
          = new ObservableCollection<BLEAttributeDisplayContainer>();

        private readonly Guid FREQ_SERVICE_UUID = new Guid("713D0000-503E-4C75-BA94-3148F18D941E");
        private readonly Guid MODE_SERVICE_UUID = new Guid("813D0000-503E-4C75-BA94-3148F18D941E");
        private readonly Guid FREQ_CHARACTERISTIC_UUID = new Guid("713D0003-503E-4C75-BA94-3148F18D941E");
        private readonly Guid MODE_CHARACTERISTIC_UUID = new Guid("813D0003-503E-4C75-BA94-3148F18D941E");

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
            initCode();
            setup = new MainProgram();

            //textBlock.Text = setup.playSignalStart();



            UpdateUI(SystemStatus.NOCONNECT);

            // BLE watcher init.
            listViewDevices.ItemsSource = BTDevices;
            listViewDevices.DisplayMemberPath = "Name";
            StartBleDeviceWatcher();

            Mode = IA_TACTILE_MODE_DEF;
            
            DataContext = this;
        }



        private void initCode()
        {
            untypedSignal = SignalTyp.NODATA;
        }

        private async void NextSignalButton_Click_1(object sender, RoutedEventArgs e)
        {
            /*MediaElement mediaElement = new MediaElement();
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream =
                await synth.SynthesizeTextToStreamAsync("Hello, World!");
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();*/

            // Falls nicht auf Next gedrückt worden ist, dann soll eine Fehler meldung erscheinen.
            if (untypedSignal == SignalTyp.NODATA) {
                var dialog = new MessageDialog("Sie haben Keine Eingabe getätigt \n" +
                    "Bitte waehlen Sie Ihre Auswahl und bestaetigen Sie Ihre Eingabe.");
                await dialog.ShowAsync();
            } else {
                setup.nextSignal(untypedSignal);
                switch (untypedSignal)
                {
                    case SignalTyp.KURZ:
                        radioButtonKurz.IsChecked = false; 
                    break;
                    case SignalTyp.MITTEL:
                        radioButtonMittel.IsChecked = false;
                    break;
                    case SignalTyp.LANG:
                        radioButtonLang.IsChecked = false;
                    break;
                    default:
                        Debug.WriteLine("ERROR in der  NextSignalButton_Click_1 Methode");
                    break;
                }

                NextSignalButton.IsEnabled = false;
                playSignalButton.IsEnabled = true;
                untypedSignal = SignalTyp.NODATA;
            }
        }

        private void playSignalButton_Click(object sender, RoutedEventArgs e)
        {
            if (setup.isStartElementAvailable())
            {
                String s = setup.playStartSignal();
                textBlock.Text = s;
                NextSignalButton.IsEnabled = true;
                playSignalButton.IsEnabled = false;
            } else {
                NextSignalButton.IsEnabled = true;
                // Eingabe des Benutzers ist fertig für die validierung der gleichverteilten Population. 
                // Es kann jetzt die Berechnung der Grenzen für Kurz, Mittel und Lang erfolgen.
                setup.calculateStartZones();
            }
        }

        private void radioButtonKurz_Checked(object sender, RoutedEventArgs e)
        {
            untypedSignal = SignalTyp.KURZ;
        }

        private void radioButtonMittel_Checked(object sender, RoutedEventArgs e)
        {
            untypedSignal = SignalTyp.MITTEL;
        }

        private void radioButtonLang_Checked(object sender, RoutedEventArgs e)
        {
            untypedSignal = SignalTyp.LANG;
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentBTDeviceInfo != null) // already connected -> disconnect
            {
                DisposeCurrentDevice();
                UpdateUI(SystemStatus.NOCONNECT);
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
            connectButton.IsEnabled = false;

            if (CurrentBTDevice != null) // disconnect pressed
            {
                DisposeCurrentDevice();
                UpdateUI(SystemStatus.NOCONNECT);
                return;
            }

            DisposeCurrentDevice();

            try { CurrentBTDevice = await BluetoothLEDevice.FromIdAsync(dev.Id); }
            catch { CurrentBTDeviceInfo = null; }

            if (CurrentBTDevice != null)
            {
                #pragma warning disable 0618
                foreach (var service in CurrentBTDevice.GattServices)
                    currentServiceCollection.Add(new BLEAttributeDisplayContainer(service));

                // TODO: adding this made the device slower
                // or was it just the battery? :/
                if (GetServices() && GetCharacteristics())
                    UpdateUI(SystemStatus.READY);
                else
                    UpdateUI(SystemStatus.NOCONNECT);

                // TODO: Error prompt
            }
            else
            {
                UpdateUI(SystemStatus.NOCONNECT);
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

        /// <summary>
        ///    Updates the user interface based on given system state. </summary>
        ///    
        /// <param name = "status">
        ///    New system state. </param>
        /// 
        private void UpdateUI(SystemStatus status)
        {
            switch (status)
            {
                case SystemStatus.NOCONNECT:
                    connectButton.IsEnabled = true;
                    connectButton.Content = "Connect";
                    break;
                case SystemStatus.CONNECTING:
                    connectButton.IsEnabled = false;
                    connectButton.Content = "Connecting...";
                    break;
                case SystemStatus.READY:
                    connectButton.IsEnabled = true;
                    connectButton.Content = "Disconnect";
                    break;
                case SystemStatus.ANIMATION:
                    connectButton.IsEnabled = false;
                    break;
                case SystemStatus.PAUSE:
                    // TODO: implement
                    break;
            }
        }

        /// Indicates the state of the program. Used
        /// to update the user interface.
        enum SystemStatus
        {
            NOCONNECT,
            CONNECTING,
            READY,
            ANIMATION,
            PAUSE,       // not used yet
        }

        
    }
}
