using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

using static TactileGraphDemo.BTZeugs;

namespace TactileGraphDemo
{
    public partial class MainWindow : Window
    {
        #region Variables

        // BLE VARS
        private ObservableCollection<DeviceInformation> BTDevices = new ObservableCollection<DeviceInformation>();
        private DeviceWatcher deviceWatcher;

        private BluetoothLEDevice CurrentBTDevice       { get; set; }
        private DeviceInformation CurrentBTDeviceInfo   { get; set; }

        // TODO: Firmware -> get FREQ/w and MODE/w into one control service
        private BLEAttributeDisplayContainer CurrentFreqService         { get; set; }
        private BLEAttributeDisplayContainer CurrentFreqCharacteristic  { get; set; }
        private BLEAttributeDisplayContainer CurrentModeService         { get; set; }
        private BLEAttributeDisplayContainer CurrentModeCharacteristic  { get; set; }
        private ObservableCollection<BLEAttributeDisplayContainer> currentServiceCollection
          = new ObservableCollection<BLEAttributeDisplayContainer>();

        private readonly Guid FREQ_SERVICE_UUID        = new Guid("713D0000-503E-4C75-BA94-3148F18D941E");
        private readonly Guid MODE_SERVICE_UUID        = new Guid("813D0000-503E-4C75-BA94-3148F18D941E");
        private readonly Guid FREQ_CHARACTERISTIC_UUID = new Guid("713D0003-503E-4C75-BA94-3148F18D941E");
        private readonly Guid MODE_CHARACTERISTIC_UUID = new Guid("813D0003-503E-4C75-BA94-3148F18D941E");


        // GRAPH VARS
        private SolidColorBrush GraphColor   = Brushes.CornflowerBlue;
        private SolidColorBrush SegmentColor = Brushes.Yellow;

        //public  ChartValues<ObservablePoint> MyValues           { get; set; }
        //public  SeriesCollection             SeriesCollection   { get; set; }
        private Byte                         NumPoints          { get; set; }
        private Byte                         Mode               { get; set; }
        private Boolean                      Animate            { get; set; }

        private readonly double[] defPoints = {  2,  5,  -2,  7, -2,
                                                -1, -1,  3, -2, -3,
                                                 6,  7,  0, -3, -7,
                                                 0,  5, -4,  2, 10 };

        private const Byte  MAX_POINTS             =   20; // ~ BLE-Buffersize

        private const Byte  IA_TACTILE_MAXFREQ     =   10;
        private const Byte  IA_TACTILE_NULFREQ     = 0x0A;
        private const Byte  IA_TACTILE_EOD         = 0xFF; // end of data
 
        private const Byte  IA_TACTILE_MODE_STOP   = 0x00;
        private const Byte  IA_TACTILE_MODE_DEF    = 0x01;
        private const Byte  IA_TACTILE_MODE_ALT    = 0x02;

        private const Int16 IA_TACTILE_DELAY       =  265;
        private const Int16 IA_TACTILE_TIMESTEP    = 2000; 

        private Int16[] frequencies = new Int16[MAX_POINTS - 1];

        #endregion


        public MainWindow()
        {
            InitializeComponent();

            UpdateUI(SysStatus.NOCONNECT);

            // BLE watcher init.
            listViewDevices.ItemsSource       = BTDevices;
            listViewDevices.DisplayMemberPath = "Name";
            StartBleDeviceWatcher();

            // Graph init
            cartesianChart.DisableAnimations = true;
   
            //CalcFrequenciesForTactile();

            Mode      = IA_TACTILE_MODE_DEF;
            Animate   = false;
            NumPoints = (Byte) defPoints.Count<double>();

            DataContext = this;
        }


        #region Device discovery

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
            deviceWatcher.Added   += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;

            // Start over with an empty collection.
            BTDevices.Clear();

            // Start the watcher.
            deviceWatcher.Start();
        }

        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            // code-lvl = wat
            this.Dispatcher.Invoke(
                new Action(
                () => { if (sender == deviceWatcher)
                          if (deviceInfo.Name != String.Empty && !BTDevices.Any<DeviceInformation>(x => x.Id == deviceInfo.Id))
                            BTDevices.Add(deviceInfo); }
                )
            );
        }

        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            this.Dispatcher.Invoke(
                new Action(
                () => { if (sender == deviceWatcher)
                          if (BTDevices.Any<DeviceInformation>(x => x.Id == deviceInfoUpdate.Id))
                            BTDevices.First<DeviceInformation>(x => x.Id == deviceInfoUpdate.Id).Update(deviceInfoUpdate); }
                )
            );
        }

        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            this.Dispatcher.Invoke(
                new Action(
                () => {
                    if (sender == deviceWatcher)
                        if (BTDevices.Any<DeviceInformation>(x => x.Id == deviceInfoUpdate.Id))
                          BTDevices.Remove(BTDevices.First<DeviceInformation>(x => x.Id == deviceInfoUpdate.Id)); }
                )
            );
        }

        #endregion


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
            CurrentBTDevice     = null;
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
                UpdateUI(SysStatus.NOCONNECT);
                return;
            }

            DisposeCurrentDevice();

            try   { CurrentBTDevice     = await BluetoothLEDevice.FromIdAsync(dev.Id); }
            catch { CurrentBTDeviceInfo = null; }

            if (CurrentBTDevice != null)
            {
                foreach (var service in CurrentBTDevice.GattServices)
                    currentServiceCollection.Add(new BLEAttributeDisplayContainer(service));
                
                // TODO: adding this made the device slower
                    // or was it just the battery? :/
                if (GetServices() && GetCharacteristics())
                    UpdateUI(SysStatus.READY);
                else
                    UpdateUI(SysStatus.NOCONNECT);

                    // TODO: Error prompt
            }
            else
            {
                UpdateUI(SysStatus.NOCONNECT);
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
            try   { ServiceChars = service.service.GetAllCharacteristics(); }
            catch { return null; }

            List<BLEAttributeDisplayContainer> CharCollection = new List<BLEAttributeDisplayContainer>();
            foreach (GattCharacteristic gattchar in ServiceChars)
                CharCollection.Add(new BLEAttributeDisplayContainer(gattchar));

            var characteristic = CharCollection.FirstOrDefault<BLEAttributeDisplayContainer>
                (x => x.characteristic.Uuid.CompareTo(charUUID) == 0);

            return characteristic;
        }

        /// <summary>
        ///    Calculates the frequency values for the tactile device. </summary>
        ///    
        /*private void CalcFrequenciesForTactile()
        {
            double   max  = double.MinValue;    // max gradient
            double   last = MyValues[0].Y;      // last    y-value
            double   cur  = MyValues[0].Y;                // current y-value
            double   next = 0;
            double[] grad = new double[MAX_POINTS - 1];     // list of all gradients

            for (int i = 0; i < MyValues.Count - 1; i++)
            {
                //cur     = MyValues[i].Y;
                //grad[i] = cur - last;
                //max     = Math.Abs(grad[i]) > max ? Math.Abs(grad[i]) : max;
                //last    = cur;

                next    = MyValues[i + 1].Y;
                grad[i] = next - cur;
                max     = Math.Abs(grad[i]) > max ? Math.Abs(grad[i]) : max;
                cur     = next;
            }

            if (max != 0)
            {
                for (int i = 0; i < MyValues.Count() - 1; i++)
                    // tl;dr: freq = [( m / m_max ) * freq_max] + freq_null
                    frequencies[i] = (Int16)
                      Math.Round(IA_TACTILE_NULFREQ + (grad[i] * IA_TACTILE_MAXFREQ / max));
            }
            else // constant graph
            {
                for (int i = 1; i < MyValues.Count(); i++)
                    frequencies[i - 1] = IA_TACTILE_NULFREQ;
            }

            return;
        }*/
        
        /// <summary>
        ///    Sends the frequency data based on the graph to
        ///    the connected device and starts the simulation. </summary>
        ///    
        private async void SendGraph()
        {
            Byte[] writerValues = new Byte[NumPoints];
            Byte[] writerValues2 = new Byte[NumPoints];
            Byte[] writerMode   = { Mode };

            Byte[] myTestByte = { 0x14, 0x00, 0x24, 0x00, 0x13, 0x00, 0x23, 0x00, 0x12, 0x00, 0x22, 0x00, 0x11, 0x00, 0x21, 0x00, 0x14, 0x00, 0x24, 0x00 };

            for (int i = 0; i < myTestByte.Length; i++)
                writerValues2[i] = (Byte)myTestByte.ElementAt(i);

            if (myTestByte.Length < MAX_POINTS - 1) // add 'end of data' byte if needed
                writerValues2[myTestByte.Length] = IA_TACTILE_EOD;


            for (int i = 0; i < frequencies.Length; i++)
                writerValues[i] = (Byte)frequencies.ElementAt(i);

            if (frequencies.Length < MAX_POINTS - 1) // add 'end of data' byte if needed
                writerValues[frequencies.Length] = IA_TACTILE_EOD;


            /* status1= */
            await CurrentFreqCharacteristic.characteristic.WriteValueAsync(writerValues2.AsBuffer());
            /* status2= */await CurrentModeCharacteristic.characteristic.WriteValueAsync(writerMode.AsBuffer());

            // if ((status1 & status2) != GattCommunicationStatus.Success)
            // TODO: Error prompt "simulation data transmission failed!"
        }

        /// <summary>
        ///    Stops the simulation on the device. </summary>
        ///    
        private async void SendStop()
        {
            Byte[] writerStop = { IA_TACTILE_MODE_STOP };
            await CurrentModeCharacteristic.characteristic.WriteValueAsync(writerStop.AsBuffer());
        }

        /// <summary>
        ///    Starts the animation. </summary>
        ///    
        /*private async void StartAnimation()
        {
            UpdateUI(SysStatus.ANIMATION);

            // GRAPH BEFORE MS
            SeriesCollection.Add(new LineSeries
            {
                Values            = { },
                Stroke            = SegmentColor,
                StrokeThickness   = 4,
                Fill              = Brushes.Transparent,
                PointGeometrySize = 0,
            });

            Animate = true;
            for (int i = 0; i < MyValues.Count() - 1; i++)
            {
                if (!Animate) // 'Stop' pressed
                    break;

                // adding NaN value between adjacent pts removes connecting segment
                SeriesCollection[0].Values.Add(new ObservablePoint(i + 0.5, double.NaN));

                if (i < MyValues.Count() - 2)
                    SeriesCollection[1].Values = new ChartValues<ObservablePoint>
                {
                    MyValues[i],
                    MyValues[i+1]
                };
                
                // wait for device
                await Task.Delay(IA_TACTILE_TIMESTEP + IA_TACTILE_DELAY);

                SeriesCollection[0].Values.RemoveAt(NumPoints);
            }

            SeriesCollection.RemoveAt(1);
            UpdateUI(SysStatus.READY);
        }*/

        /// <summary>
        ///    Stops the animation. </summary>
        ///    
        private void StopAnimation()
        {
            Animate = false;
        }


        #region Click events

        /// <summary>
        ///    Starts the animation and simulation on click. </summary>
        ///    
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void StartOnClick(object sender, RoutedEventArgs e)
        {
            SendGraph();
            //StartAnimation();
        }

        /// <summary>
        ///    Stops the animation and simulation on click. </summary>
        ///    
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void StopOnClick(object sender, RoutedEventArgs e)
        {
            SendStop();
            StopAnimation();
        }

        /// <summary>
        ///    Creates a new, random graph on click. </summary>
        ///    
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        /*private void RandomOnClick(object sender, RoutedEventArgs e)
        {
            var r = new Random();

            foreach (var observable in MyValues)
            {
                observable.Y = r.Next(-10, 10);
            }

            CalcFrequenciesForTactile();
        }*/

        /// <summary>
        ///    Switches the simulation mode on click. </summary>
        ///    
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void SwitchModeOnClick(object sender, RoutedEventArgs e)
        {
            if (Mode == IA_TACTILE_MODE_DEF)
            {
                Mode = IA_TACTILE_MODE_ALT;
                button_switch_mode.Content = "MODE 02";
            }
            else
            {
                Mode = IA_TACTILE_MODE_DEF;
                button_switch_mode.Content = "MODE 01";
            }
        }
        
        /// <summary>
        ///    Connects to a selected device on click. </summary>
        ///    
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void ConnectOnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentBTDeviceInfo != null) // already connected -> disconnect
            {
                DisposeCurrentDevice();
                UpdateUI(SysStatus.NOCONNECT);
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


        #endregion


        /// <summary>
        ///    Updates the user interface based on given system state. </summary>
        ///    
        /// <param name = "s">
        ///    New system state. </param>
        /// 
        private void UpdateUI(SysStatus s)
        {
            switch(s)
            {
                case SysStatus.NOCONNECT:
                    button_connect.IsEnabled     = true;
                    button_start.IsEnabled       = false;
                    button_stop.IsEnabled        = false;
                    button_random.IsEnabled      = true;
                    button_switch_mode.IsEnabled = true;
                    button_connect.Content       = "Connect";
                    break;
                case SysStatus.CONNECTING:
                    button_connect.IsEnabled     = false;
                    button_start.IsEnabled       = false;
                    button_stop.IsEnabled        = false;
                    button_random.IsEnabled      = false;
                    button_switch_mode.IsEnabled = false;
                    button_connect.Content       = "Connecting...";
                    break;
                case SysStatus.READY:
                    button_connect.IsEnabled     = true;
                    button_start.IsEnabled       = true;
                    button_stop.IsEnabled        = true;
                    button_random.IsEnabled      = true;
                    button_switch_mode.IsEnabled = true;
                    button_connect.Content       = "Disconnect";

                    break;
                case SysStatus.ANIMATION:
                    button_connect.IsEnabled     = false;
                    button_start.IsEnabled       = false;
                    button_stop.IsEnabled        = true;
                    button_random.IsEnabled      = false;
                    button_switch_mode.IsEnabled = false;
                    break;
                case SysStatus.PAUSE:
                    // TODO: implement
                    break;
            }
        }

        /// <summary>
        ///    Indicates the state of the program. Used
        ///    to update the user interface. </summary>
        ///    
        enum SysStatus
        {
            NOCONNECT,
            CONNECTING,
            READY,
            ANIMATION,
            PAUSE,       // not used yet
        }
    }
}
