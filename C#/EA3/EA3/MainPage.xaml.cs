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
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Core;
using System.Threading.Tasks;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace EA3
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        SignalTyp untypedSignal;
        SignalRating signalRating;
        MainProgram setup;
        int generation = 0;
        bool newGeneration = false;

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

            Loaded += (s, e) =>
            {
                var coreTitleBar1 = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar1.ExtendViewIntoTitleBar = true;
            };


            var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            appView.FullScreenSystemOverlayMode = FullScreenSystemOverlayMode.Minimal;

            initCode();
            setup = new MainProgram();

            //CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            //coreTitleBar.IsVisibleChanged += OnIsVisibleChanged;

            /*var coreTitleBar1 = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar1.ExtendViewIntoTitleBar = true;*/

            //CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
           // Window.Current.SetTitleBar(null);


            // Setzt die Farbe der Leiste auf Weiß
           /* ApplicationViewTitleBar titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.White;
            titleBar.ButtonForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonHoverBackgroundColor = Windows.UI.Colors.White;
            titleBar.ButtonHoverForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.White;
            titleBar.ButtonInactiveForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonPressedBackgroundColor = Windows.UI.Colors.White;
            titleBar.ButtonPressedForegroundColor = Windows.UI.Colors.White;
            titleBar.InactiveBackgroundColor = Windows.UI.Colors.White;
            titleBar.InactiveForegroundColor = Windows.UI.Colors.White;
            */
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

        /**
        *** entfernt alle StartElemente und plaziert die neuen Elemente für den Algorithmus.
        **/
        private void removeStartElements()
        {
            relativePanelStart.Children.Clear();
            playSignalButton.Visibility = Visibility.Collapsed;
            /*
            relativePanelStart.Children.Remove((UIElement)this.FindName("NextSignalButtonStart"));
            relativePanelStart.Children.Remove((UIElement)this.FindName("radioButtonKurz"));
            relativePanelStart.Children.Remove((UIElement)this.FindName("radioButtonMittel"));
            relativePanelStart.Children.Remove((UIElement)this.FindName("radioButtonLang"));
            relativePanelStart.Children.Remove((UIElement)this.FindName("playSignalButtonStart"));
            */
        }

        private void moveElements()
        {
            borderAlgo.Margin = new Thickness(0,0,0,0);
            textBlockAlgo.Margin = new Thickness(54, 40, 0, 0);
            radioButtonVeryBad.Margin = new Thickness(27, 267, 0, 0);
            radioButtonBad.Margin = new Thickness(152, 267, 0, 0);
            radioButtonOK.Margin = new Thickness(272, 267, 0, 0);
            radioButtonGood.Margin = new Thickness(361, 267, 0, 0);
            radioButtonVeryGood.Margin = new Thickness(426, 267, 0, 0);
            replaySignalButtonAlgo.Margin = new Thickness(387, 141, -1324, -141);
            nextButtonAlgo.Margin = new Thickness(387, 89, -1324, -89);
            playSignalButtonAlgo.Margin = new Thickness(387, 41, -1324, -41);
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
            if (untypedSignal == SignalTyp.NODATA)
            {
                var dialog = new MessageDialog("Sie haben Keine Eingabe getätigt \n" +
                    "Bitte waehlen Sie Ihre Auswahl und bestaetigen Sie Ihre Eingabe.");
                await dialog.ShowAsync();
            }
            else
            {
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
                        Debug.WriteLine("ERROR in der  NextSignalButtonStart_Click_1 Methode");
                        break;
                }

                NextSignalButton.IsEnabled = false;
                playSignalButton.IsEnabled = true;
                untypedSignal = SignalTyp.NODATA;
            }
        }

        private async void playSignalButton_Click(object sender, RoutedEventArgs e)
        {
            if (setup.isElementAvailable())
            {
                String s = setup.playStartSignal();
                textBlock.Text = s;
                NextSignalButton.IsEnabled = true;
                playSignalButton.IsEnabled = false;
            }
            else
            {
                if (setup.calculateStartZones())
                {
                    NextSignalButton.IsEnabled = false;
                    playSignalButton.IsEnabled = false;

                    removeStartElements();
                    // Eingabe des Benutzers ist fertig für die validierung der gleichverteilten Population. 
                    // Es kann jetzt die Berechnung der Grenzen für Kurz, Mittel und Lang erfolgen.
                    // Der Benutzer hat alle Daten korrekt eingegeben und die Berechnung ist erfolgt und nun kann der Algorithmus Starten
                    var dialog = new MessageDialog("Ihre Eingabe wurde erfolgreich evaluiert\n" +
                                                 "Bitte drücken Sie auf den Knopf 'Schritt 2' um mit den Programm fortzufahren.");
                    await dialog.ShowAsync();
                }
                else
                {
                    NextSignalButton.IsEnabled = false;
                    playSignalButton.IsEnabled = true;
                    // Der Benutzer hat Eingabe Falsch Evaluiert und es sind keine validen Daten herausgekommen, mit
                    // dem das Programm weiter rechnen kann.
                    var dialog = new MessageDialog("Ihre Daten die Sie eingegeben haben sind leider zu sehr verfälscht.\n" +
                                                 "Sie müssen die Daten erneut eingeben ");
                    await dialog.ShowAsync();
                }
            }
        }
        
        private async void replaySignalButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO Signal muss hier noch abgespielt werden
            replaySignalButton.IsEnabled = false;
            var temp = replaySignalButton.Content;
            replaySignalButton.Content = "Warten 5s";
            await Task.Delay(1000);
            replaySignalButton.Content = "Warten 4s";
            await Task.Delay(1000);
            replaySignalButton.Content = "Warten 3s";
            await Task.Delay(1000);
            replaySignalButton.Content = "Warten 2s";
            await Task.Delay(1000);
            replaySignalButton.Content = "Warten 1s";
            await Task.Delay(1000);
            replaySignalButton.Content = temp;
            replaySignalButton.IsEnabled = true;
        }

        private void removeElementsButton_Click(object sender, RoutedEventArgs e)
        {
            removeStartElements();
        }

        private void moveElementsButton_Click(object sender, RoutedEventArgs e)
        {
            moveElements();
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

        private void radioButtonVeryBad_Checked(object sender, RoutedEventArgs e)
        {
            signalRating = SignalRating.VERYBAD;
        }

        private void radioButtonBad_Checked(object sender, RoutedEventArgs e)
        {
            signalRating = SignalRating.BAD;
        }

        private void radioButtonOK_Checked(object sender, RoutedEventArgs e)
        {
            signalRating = SignalRating.OK;
        }

        private void radioButtonGood_Checked(object sender, RoutedEventArgs e)
        {
            signalRating = SignalRating.GOOD;
        }

        private void radioButtonVeryGood_Checked(object sender, RoutedEventArgs e)
        {
            signalRating = SignalRating.VERYGOOD;
        }

        private async void nextButtonAlgo_Click(object sender, RoutedEventArgs e)
        {
            /*MediaElement mediaElement = new MediaElement();
            var synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            Windows.Media.SpeechSynthesis.SpeechSynthesisStream stream =
                await synth.SynthesizeTextToStreamAsync("Hello, World!");
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();*/

            // Falls nicht auf Next gedrückt worden ist, dann soll eine Fehler meldung erscheinen.
            if (signalRating == SignalRating.NODATA)
            {
                var dialog = new MessageDialog("Sie haben Keine Eingabe getätigt \n" +
                    "Bitte waehlen Sie Ihre Auswahl und bestaetigen Sie Ihre Eingabe.");
                await dialog.ShowAsync();
            }
            else
            {
                setup.nextSignalRating(signalRating);
                switch (signalRating)
                {
                    case SignalRating.VERYBAD:
                        radioButtonVeryBad.IsChecked = false;
                        break;
                    case SignalRating.BAD:
                        radioButtonBad.IsChecked = false;
                        break;
                    case SignalRating.OK:
                        radioButtonOK.IsChecked = false;
                        break;
                    case SignalRating.GOOD:
                        radioButtonGood.IsChecked = false;
                        break;
                    case SignalRating.VERYGOOD:
                        radioButtonVeryGood.IsChecked = false;
                        break;
                    default:
                        Debug.WriteLine("ERROR in der  NextSignalButton_Click_1 Methode");
                        break;
                }

                nextButtonAlgo.IsEnabled = false;
                playSignalButtonAlgo.IsEnabled = true;
                signalRating = SignalRating.NODATA;
            }
        }

        private async void playSignalButtonAlgo_Click(object sender, RoutedEventArgs e)
        {
            if (setup.isNextElementAvailable())
            {
                if (newGeneration)
                {
                    playSignalButtonAlgo.Content = "Signal abspielen";
                    newGeneration = false;
                }
                String s = setup.playSignal();
                textBlockAlgo.Text = s;
                nextButtonAlgo.IsEnabled = true;
                playSignalButtonAlgo.IsEnabled = false;
                replaySignalButtonAlgo.IsEnabled = true;
            }
            else
            {
                // Alle Daten wurden evalutiert, jetzt wird der Algorithmus ausgefuehrt.
                // die Daten werden im gleichen Datensatz überschrieben, d.h. auf Daten von vorherigen Generationen kann man nicht mehr zurueckgreifen
                // TODO Daten muessen gespeichert werden.
                var dialog = new MessageDialog("Es wird Anhand Ihrer Eingaben die nächsten Signale erstellt. \n" +
                                               "Bitte warten Sie einen Augenblick.");

                playSignalButtonAlgo.IsEnabled = false;
                replaySignalButtonAlgo.IsEnabled = false;
                nextButtonAlgo.IsEnabled = false;
                playSignalButtonAlgo.Content = "Warten";

                setup.calculateFitness();

                await dialog.ShowAsync();
                await Task.Delay(1000);

                textBlockAlgo.Text = "Zum Beginn der naechsten Session bitte auf Beginnen druecken.";
                playSignalButtonAlgo.Content = "Beginnen";

                playSignalButtonAlgo.IsEnabled = true;
                newGeneration = true;
                generation += 1;

                setup.prepareForNextGeneration();
            }
        }

        private async void replaySignalButtonAlgo_Click(object sender, RoutedEventArgs e)
        {
            // TODO Signal muss hier noch abgespielt werden
            replaySignalButtonAlgo.IsEnabled = false;
            var temp = replaySignalButtonAlgo.Content;
            replaySignalButtonAlgo.Content = "Warten 5s";
            await Task.Delay(1000);
            replaySignalButtonAlgo.Content = "Warten 4s";
            await Task.Delay(1000);
            replaySignalButtonAlgo.Content = "Warten 3s";
            await Task.Delay(1000);
            replaySignalButtonAlgo.Content = "Warten 2s";
            await Task.Delay(1000);
            replaySignalButtonAlgo.Content = "Warten 1s";
            await Task.Delay(1000);
            replaySignalButtonAlgo.Content = temp;
            replaySignalButtonAlgo.IsEnabled = true;
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
