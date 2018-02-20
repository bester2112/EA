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
using Windows.Storage.Streams;

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

        private BLEAttributeDisplayContainer CurrentLengthService { get; set; }
        private BLEAttributeDisplayContainer CurrentLengthCharacteristic { get; set; }
        private BLEAttributeDisplayContainer CurrentModeService { get; set; }
        private BLEAttributeDisplayContainer CurrentModeCharacteristic { get; set; }
        private ObservableCollection<BLEAttributeDisplayContainer> currentServiceCollection 
            = new ObservableCollection<BLEAttributeDisplayContainer>();

        private readonly Guid LENGTH_SERVICE_UUID        = new Guid("713D0000-503E-4C75-BA94-3148F18D941E");
        private readonly Guid MODE_SERVICE_UUID          = new Guid("813D0000-503E-4C75-BA94-3148F18D941E");
        private readonly Guid LENGTH_CHARACTERISTIC_UUID = new Guid("713D0003-503E-4C75-BA94-3148F18D941E");
        private readonly Guid MODE_CHARACTERISTIC_UUID   = new Guid("813D0003-503E-4C75-BA94-3148F18D941E");

        private const Byte MAX_POINTS = 20; // ~ BLE-Buffersize

        private const Byte IA_TACTILE_EOD       = 0xFF; // end of data

        private const Byte IA_TACTILE_MODE_STOP = 0x00;

        private Int16[] lengthSignal = new Int16[MAX_POINTS - 1];
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



            connectButton.IsEnabled = true;
            connectButton.Content = "Connect";

            // BLE watcher init.
            listViewDevices.ItemsSource = BTDevices;
            listViewDevices.DisplayMemberPath = "Name";
            StartBleDeviceWatcher();

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
                Signal signal = setup.getLastSignal();
                playSignalNow(signal); // spielt das aktullle Signal ab.
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

                //setup.newSaveInFileinCSharp();
                //setup.SaveInFileStart();
            }
            
        }
        
        private async void replaySignalButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO Signal muss hier noch abgespielt werden
            Signal signal = setup.getLastSignal();
            playSignalNow(signal); // spielt das aktullle Signal ab.

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

                Signal signal = setup.getLastSignal();
                playSignalNow(signal); // spielt das aktullle Signal ab.

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

                setup.SaveInFileAlgo();

                playSignalButtonAlgo.IsEnabled = true;
                newGeneration = true;
                generation += 1;

                setup.prepareForNextGeneration();
            }
        }

        private async void replaySignalButtonAlgo_Click(object sender, RoutedEventArgs e)
        {
            // TODO
            Signal signal = setup.getLastSignal(); // noch abfragen, ob das Signal vorhanden ist und ich nicht NULL zurueck erhalte 
            playSignalNow(signal); // spielt das aktullle Signal ab.

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
                connectButton.IsEnabled = true;
                connectButton.Content = "Connect";
                return;
            }

            if (listViewDevices.SelectedItem != null && listViewDevices.SelectedItem is DeviceInformation)
            {
                CurrentBTDeviceInfo = listViewDevices.SelectedItem as DeviceInformation;
                //var devInfo = listViewDevices.SelectedItem as DeviceInformation;
                //Connect(devInfo);
            }
            else {
                return; /* do nothing */ 
            }

            Connect(CurrentBTDeviceInfo);
        }

        #region BLE Device discovery
        // DONE
        /// <summary>
        /// Starts a device watcher that looks for all nearby BT devices (paired or unpaired).
        /// Attaches event handlers to populate the device collection.
        /// Folder BluethoothLE in File Scenario1_Discovery.xaml.cs
        /// </summary>
        private void StartBleDeviceWatcher()
        {
            // Additional properties we would like about the device.
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

            // BT_Code: Currently Bluetooth APIs don't provide a selector to get ALL devices that are both paired and non-paired.
            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            deviceWatcher =
                    DeviceInformation.CreateWatcher(
                        aqsAllBluetoothLEDevices,
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            // falls ich diese noch mal benoetigen sollte
            // TODO falls noch zeit vorhanden ist, kann man die weiteren methoden noch ergaenzen
            //deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            //deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start over with an empty collection.
            BTDevices.Clear();

            // Start the watcher.
            deviceWatcher.Start();
        }

        /// Folder BluethoothLE in File Scenario1_Discovery.xaml.cs
        //DONE
        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {

            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Added {0} {1}", deviceInfo.Id, deviceInfo.Name));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher) {
                        if (deviceInfo.Name != String.Empty && !BTDevices.Any<DeviceInformation>(x => x.Id == deviceInfo.Id)) {
                            // If device has a friendly name display it immediately.
                            BTDevices.Add(deviceInfo);
                        }
                    }
                }
            });
        }

        /// Folder BluethoothLE in File Scenario1_Discovery.xaml.cs
        //DONE
        private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Updated {0} {1}", deviceInfoUpdate.Id, " "));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher) {
                        if (BTDevices.Any<DeviceInformation>(x => x.Id == deviceInfoUpdate.Id)) {
                            BTDevices.First<DeviceInformation>(x => x.Id == deviceInfoUpdate.Id).Update(deviceInfoUpdate);
                        }
                    }
                }
            });
        }

        /// Folder BluethoothLE in File Scenario1_Discovery.xaml.cs
        //DONE
        private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    Debug.WriteLine(String.Format("Removed {0} {1}", deviceInfoUpdate.Id,""));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher) {
                        if (BTDevices.Any<DeviceInformation>(x => x.Id == deviceInfoUpdate.Id)) {
                            BTDevices.Remove(BTDevices.First<DeviceInformation>(x => x.Id == deviceInfoUpdate.Id));
                        }
                    }
                }
            });
        }
        #endregion

        /// <summary>
        ///    Disposes of everything associated with the currently connected device. 
        /// </summary>
        private void DisposeCurrentDevice()
        {
            try
            {
                CurrentLengthService = null;
                CurrentModeService = null;
                CurrentLengthCharacteristic = null;
                CurrentModeCharacteristic = null;
            }
            catch { }

            CurrentBTDevice?.Dispose();
            CurrentBTDevice = null;
            CurrentBTDeviceInfo = null;
            currentServiceCollection = new ObservableCollection<BLEAttributeDisplayContainer>();
        }

        /// <summary>
        ///    Connect to a device. 
        /// </summary>
        /// <param name = "dev">
        ///    The device information instance. 
        /// </param>
        private async void Connect(DeviceInformation dev)
        {
            connectButton.IsEnabled = false;

            if (CurrentBTDevice != null) // disconnect pressed
            {
                DisposeCurrentDevice();
                return;
            }

            DisposeCurrentDevice();

            try { CurrentBTDevice     = await BluetoothLEDevice.FromIdAsync(dev.Id); }
            catch { CurrentBTDeviceInfo = null; }


            if (CurrentBTDevice != null)
            {
                var gatt = await CurrentBTDevice.GetGattServicesAsync();

                foreach (var service in CurrentBTDevice.GattServices) {
                    currentServiceCollection.Add(new BLEAttributeDisplayContainer(service));
                }

                if (GetServices() && GetCharacteristics())
                {
                    // geraet ist verbunden
                }
                else
                {
                    // geraet ist nicht verbunden
                }

            }
            else
            {
                DisposeCurrentDevice();
                //CurrentBTDeviceInfo = null;
            }

            connectButton.Content = "Disconnect";
            connectButton.IsEnabled = true;

            GetServices();
            GetCharacteristics();
        }
   
        private bool GetServices()
        {
            bool resLENGTH = false;
            bool resMODE = false;

            if (currentServiceCollection.Any<BLEAttributeDisplayContainer>(x => x.service.Uuid == LENGTH_SERVICE_UUID)) {
                CurrentLengthService = currentServiceCollection.FirstOrDefault<BLEAttributeDisplayContainer>(x => x.service.Uuid.CompareTo(LENGTH_SERVICE_UUID) == 0);
                resLENGTH = true;
            } else {
                CurrentLengthService = null;
                resLENGTH = false;
            }
            
            if (currentServiceCollection.Any<BLEAttributeDisplayContainer>(x => x.service.Uuid == MODE_SERVICE_UUID)) {
                CurrentModeService = currentServiceCollection.FirstOrDefault<BLEAttributeDisplayContainer>(x => x.service.Uuid.CompareTo(MODE_SERVICE_UUID) == 0);
                resMODE = true;
            } else {
                CurrentModeService = null;
                resMODE = false;
            }
            return resLENGTH && resMODE;
        }

        private bool GetCharacteristics()
        {
            if (GetServices())
            {
                CurrentLengthCharacteristic = GetSingleCharacteristic(LENGTH_CHARACTERISTIC_UUID, CurrentLengthService);
                CurrentModeCharacteristic   = GetSingleCharacteristic(MODE_CHARACTERISTIC_UUID, CurrentModeService);
            }
          
            return ((CurrentLengthCharacteristic != null) && (CurrentModeCharacteristic != null)); 
        }

        private BLEAttributeDisplayContainer GetSingleCharacteristic(Guid UUID, BLEAttributeDisplayContainer currentService)
        {
            IReadOnlyList<GattCharacteristic> service_characteristics = null;

            try {
                service_characteristics = currentService.service.GetAllCharacteristics();
            }
            catch
            {
                return null;
            }

            List<BLEAttributeDisplayContainer> CharacteristicCollection = new List<BLEAttributeDisplayContainer>();

            // set currentModeCharacteristic variable
            foreach (GattCharacteristic gattCharacteristic in service_characteristics)
            {
                CharacteristicCollection.Add(new BLEAttributeDisplayContainer(gattCharacteristic));
            }

            var characteristic = CharacteristicCollection.FirstOrDefault<BLEAttributeDisplayContainer> (x => x.characteristic.Uuid.CompareTo(UUID) == 0);

            return characteristic;
        }

        // berechnet die Werte der Signale fuer das tactile Geraet.
        private void calculateSignalsForTactile(int time)
        {
            String hexString = "LEER";
            hexString = time.ToString("X");
            Debug.WriteLine("Test ausgabe HEX STRING " + hexString);

            switch (hexString.Length)
            {
                case 0:
                    Debug.WriteLine("FEHLER: Der HexString in der calculateSignalsForTactile()  Methode ist leer");
                    hexString = "0000"; // es wird ein Signal mit nur nullen übertragen
                    break;
                case 1:
                    Debug.WriteLine("Der Hexstring hat nur eine Länge von max 9");
                    hexString = "000" + hexString;
                    break;
                case 2:
                    Debug.WriteLine("der HexString hat eine Länge von 2 Zeichen");
                    hexString = "00" + hexString;
                    break;
                case 3:
                    Debug.WriteLine("der Hexstring hat eine Länge von 3 Zeichen");
                    hexString = "0" + hexString;
                    break;
                case 4:
                    Debug.WriteLine("der HexString hat eine länge von 4 Zeichen");
                    hexString = "" + hexString;
                    break;
                default:
                    Debug.WriteLine("FEHLER : der HexString hat eine Länge von " + hexString.Length + " Zeichen");
                    break;
            }
            /*
			if (hexString.Length % 2 != 0) {
                hexString = "0" + hexString;
            }*/
            byte[] myByteTest = StringToByteArray(hexString);

            lengthSignal = StringToByteArrayInt16(hexString);
            int tempZahl = int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
            int tempZahl2 = Convert.ToInt32(hexString, 16);

            String newHex = BitConverter.ToString(myByteTest);
            Debug.WriteLine(" newHex = " + newHex);
            newHex.Replace("-", "");
            Debug.WriteLine(" newHex 2 = " + newHex);
            newHex = newHex.Replace("-", "");
            Debug.WriteLine(" newHex 3 = " + newHex);

            int a = (int)((myByteTest[0]) << 8 | (myByteTest[1]));
            int b = (int)(myByteTest[0] * 256) + (myByteTest[1]);
            int a2 = a / 4096;
            int a3 = a / 8192;

            byte[] myTestByte = { 0x14, 0x00, 0x24, 0x00, 0x13, 0x00, 0x23, 0x00, 0x12, 0x00, 0x22, 0x00, 0x11, 0x00, 0x21, 0x00, 0x14, 0x00, 0x24, 0x00 };

            for (int i = 0; i < myTestByte.Length; i++)
            {
                Debug.WriteLine(i + ". Element = " + myTestByte[i]);
            }
        }
        public static Int16[] StringToByteArrayInt16(String hex)
        {
            int NumberChars = hex.Length;
            Int16[] array = new Int16[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                array[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return array;
        }

        public static byte[] StringToByteArray(String hex)
        {
          int NumberChars = hex.Length;
          byte[] bytes = new byte[NumberChars / 2];
          for (int i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
          return bytes;
        }

        private async void playSignalNow(Signal signal) 
        {
            calculateSignalsForTactile(signal.getTime());
            var writerLength = new Byte[MAX_POINTS];
            var writerMode = new DataWriter();

            for (int i = 0; i < lengthSignal.Length; i++)
            {
                writerLength[i] = (Byte)lengthSignal.ElementAt(i);
            }

            byte[] myTestByte = { 0x14, 0x00, 0x24, 0x00, 0x13, 0x00, 0x23, 0x00, 0x12, 0x00, 0x22, 0x00, 0x11, 0x00, 0x21, 0x00, 0x14, 0x00, 0x24, 0x00 };

            //foreach (var l in lengthSignal) {
            /*foreach (var l in myTestByte) {
                writerLength.WriteInt16(l);
            }*/

            // add end-value to avoid "garbage"-byte to be read
            /*if (myTestByte.Count() < 20) {
                writerLength.WriteInt16(0xFF);
            }*/

            //writerMode.WriteInt16(Mode);

            // send values to tactile device
            try {
                await CurrentLengthCharacteristic.characteristic.WriteValueAsync(writerLength.AsBuffer());
                //await CurrentModeCharacteristic.characteristic.WriteValueAsync(writerMode.DetachBuffer());
            } catch {
                Debug.WriteLine("Something went wrong by sending BLE DATA !!!!!!!");
            }
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            testButton.Content = "Test Button!!!!";


            setup.testWritingFile();
            //setup.newSaveInFileinCSharp();

            testButton.Content = "TestButton DONE!";
        }
    }
}
