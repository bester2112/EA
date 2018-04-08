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
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace EA3
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        public MainProgram setup;
        private SignalTyp untypedSignal;
        private SignalRating signalRating;
        private int generation = 0;
        private bool newGeneration = false;
        private Person user;
        private List<Population> initPopulation = new List<Population>();
        private static readonly int MAX_POPULATION = 5;
        private List<Population> allAlgoPopulations = new List<Population>();
        private List<int[]> rangeTime = new List<int[]>();
        private new List<int[]> rangeStrength = new List<int[]>();


        // BLE VARS
        private Int16[] lengthSignal = new Int16[MAX_POINTS - 1];
        private const Byte MAX_POINTS = 20; // ~ BLE-Buffersize


        private static readonly string DEVICE_NAME = "EA 3";
        private static readonly string SERVICE_UUID = "5A2D3BF8-F0BC-11E5-9CE9-5E5517507E66";
        private static readonly string CHARACTERISTIC_UUID = "5a2d40ee-f0bc-11e5-9ce9-5e5517507e66";

        private static readonly string LENGTH_SERVICE_UUID = "713D0000-503E-4C75-BA94-3148F18D941E";
        private static readonly string START_SERVICE_UUID = "813D0000-503E-4C75-BA94-3148F18D941E";
        private static readonly string STRENGTH_SERVICE_UUID = "913D0000-503E-4C75-BA94-3148F18D941E";
        private static readonly string LENGTH_CHARACTERISTIC_UUID = "713D0003-503E-4C75-BA94-3148F18D941E";
        private static readonly string START_CHARACTERISTIC_UUID = "813D0003-503E-4C75-BA94-3148F18D941E";
        private static readonly string STRENGTH_CHARACTERISTIC_UUID = "913D0003-503E-4C75-BA94-3148F18D941E";
        private List<DeviceInformation> devices = new List<DeviceInformation>();
        private byte[] bytes;
        private byte[] strengthBytes;
        //private BLEVisualizer visualizer = new BLEVisualizer();

        private DeviceWatcher deviceWatcher;
        private bool connecting;
        private GattCharacteristic lengthCharacteristic;
        private GattCharacteristic startCharacteristic;
        private GattCharacteristic strengthCharacteristic;

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;

            Loaded += (s, e) =>
            {
                var coreTitleBar1 = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar1.ExtendViewIntoTitleBar = true;
            };

            var appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            appView.FullScreenSystemOverlayMode = FullScreenSystemOverlayMode.Minimal;

            initCode();
            setup = new MainProgram();
            user = new Person();
            deviceConnected();


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
            //listViewDevices.ItemsSource = BTDevices;
            listViewDevices.DisplayMemberPath = "Name";
            //StartBleDeviceWatcher();
            QueryDevices();

            DataContext = this;

            // Starte die Hauptseite 
            myFrame.Navigate(typeof(IntroPage));
            hideMenuButtons();
        }
        

        private void initCode()
        {
            untypedSignal = SignalTyp.NODATA;
        }

        #region previousMenusButtons
        private void removeElementsButton_Click(object sender, RoutedEventArgs e)
        {
            removeStartElements();
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

        private void moveElementsButton_Click(object sender, RoutedEventArgs e)
        {
            moveElements();
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

        #endregion

        #region relativePanelStart
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
                setup.saveSignalTyp(untypedSignal, 0, 0);
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

        #endregion

        #region radioButtons
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
        #endregion

        #region relativePanelAlgo
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
                setup.saveSignalRating(signalRating);
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

                //setup.SaveInFileAlgo();

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
        #endregion


        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {
            /*lock (this)
            {
                if (connecting)
                {
                    return;
                }
                else
                {
                    connecting = true;
                }
            }*/
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
                        Debug.WriteLine("Length Service found!");
                        GattCharacteristicsResult characteristicsResult = await service.GetCharacteristicsAsync();
                        if (characteristicsResult.Status == GattCommunicationStatus.Success)
                        {
                            IReadOnlyList<GattCharacteristic> characteristics = characteristicsResult.Characteristics;
                            foreach (GattCharacteristic characteristic in characteristics)
                            {
                                if (characteristic.Uuid.Equals(new Guid(LENGTH_CHARACTERISTIC_UUID))) // CHARACTERISTIC_UUID
                                {
                                    Debug.WriteLine("Length Characteristic found!");
                                    lengthCharacteristic = characteristic;
                                    connectedImage.Visibility = Visibility.Visible;
                                    notConnectedImage.Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                    }
                    else if (service.Uuid.Equals(new Guid(STRENGTH_SERVICE_UUID))) //SERVICE_UUID
                    {
                        Debug.WriteLine("Strength Service found!");
                        GattCharacteristicsResult characteristicsResult = await service.GetCharacteristicsAsync();
                        if (characteristicsResult.Status == GattCommunicationStatus.Success)
                        {
                            IReadOnlyList<GattCharacteristic> characteristics = characteristicsResult.Characteristics;
                            foreach (GattCharacteristic characteristic in characteristics)
                            {
                                if (characteristic.Uuid.Equals(new Guid(STRENGTH_CHARACTERISTIC_UUID))) // CHARACTERISTIC_UUID
                                {
                                    Debug.WriteLine("Strength Characteristic found!");
                                    strengthCharacteristic = characteristic;
                                }
                            }
                        }
                    }
                    else if (service.Uuid.Equals(new Guid(START_SERVICE_UUID))) //SERVICE_UUID
                    {
                        Debug.WriteLine("Start Service found!");
                        GattCharacteristicsResult characteristicsResult = await service.GetCharacteristicsAsync();
                        if (characteristicsResult.Status == GattCommunicationStatus.Success)
                        {
                            IReadOnlyList<GattCharacteristic> characteristics = characteristicsResult.Characteristics;
                            foreach (GattCharacteristic characteristic in characteristics)
                            {
                                if (characteristic.Uuid.Equals(new Guid(START_CHARACTERISTIC_UUID))) // CHARACTERISTIC_UUID
                                {
                                    Debug.WriteLine("Start Characteristic found!");
                                    startCharacteristic = characteristic;
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
            /*lock (this)
            {
                connecting = false;
            }*/

        }

        #region BLE Device discovery
        public bool deviceConnected()
        {
            bool res = false;

            res = TactPlayFound();
            if (res) // wenn es verbunden ist
            {
                connectedImage.Visibility    = Visibility.Visible;
                notConnectedImage.Visibility = Visibility.Collapsed;
            }
            else // wenn es nicht verbunden ist
            {
                Popup("Das Armband ist derzeit nicht verbunden, bitte rufen Sie um Hilfe.");
                connectedImage.Visibility    = Visibility.Collapsed;
                notConnectedImage.Visibility = Visibility.Visible;
            }

            return res;
        }

        private async void Popup(string text)
        {
            var dialog = new MessageDialog(text);
            await dialog.ShowAsync();
        }

        public bool TactPlayFound()
        {
            devices = new List<DeviceInformation>();
            foreach (DeviceInformation bleDeviceInfo in devices)
            {
                if (bleDeviceInfo.Name.Equals(DEVICE_NAME))
                {
                    return true;
                }
            }
            return false;
        }

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
        #endregion

        

        // berechnet die Werte der Signale fuer das tactile Geraet.
        private void calculateSignalsForTactile(int time, SignalStrength signalStrength)
        {
            String hexString = "LEER";
            hexString = time.ToString("X");
            Debug.WriteLine("Test ausgabe HEX STRING " + hexString);

            hexString = timeToHexString(time, 0); //  versuch auf 1 zu ändern ///////////////////////////////////////////////  TODO

            strengthBytes = CalculateStrength(signalStrength);

            /*
			if (hexString.Length % 2 != 0) {
                hexString = "0" + hexString;
            }*/
            byte[] myByteTest = StringToByteArray(hexString + "000000000000000000000000000000000000");


            Debug.WriteLine(hexString + "000000000000000000000000000000000000");
            hexString = addMissingZeros(hexString);
            Debug.WriteLine(hexString);

            bytes = StringToByteArray(hexString);
            
            //lengthSignal = StringToByteArrayInt16(hexString);
            //int tempZahl = int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
            //int tempZahl2 = Convert.ToInt32(hexString, 16);

            //String newHex = BitConverter.ToString(myByteTest);
            //Debug.WriteLine(" newHex = " + newHex);
            //newHex.Replace("-", "");
            //Debug.WriteLine(" newHex 2 = " + newHex);
            //newHex = newHex.Replace("-", "");
            //Debug.WriteLine(" newHex 3 = " + newHex);

            //int a = (int)((myByteTest[0]) << 8 | (myByteTest[1]));
            //int b = (int)(myByteTest[0] * 256) + (myByteTest[1]);
            //int a2 = a / 4096;
            //int a3 = a / 8192;

            //byte[] myTestByte = { 0x14, 0x00, 0x24, 0x00, 0x13, 0x00, 0x23, 0x00, 0x12, 0x00, 0x22, 0x00, 0x11, 0x00, 0x21, 0x00, 0x14, 0x00, 0x24, 0x00 };

            /*for (int i = 0; i < myTestByte.Length; i++)
            {
                Debug.WriteLine(i + ". Element = " + myTestByte[i]);
            }*/
        }

        public string timeToHexString(int time, int modus)
        {
            // modus 1   => signal
            // modus 2   => pause
            string hexString = time.ToString("X");

            switch (hexString.Length)
            {
                case 0:
                    Debug.WriteLine("FEHLER: Der HexString in der calculateSignalsForTactile()  Methode ist leer");
                    hexString = modus + "000"; //"0000"; // es wird ein Signal mit nur nullen übertragen
                    
                    break;
                case 1:
                    Debug.WriteLine("Der Hexstring hat nur eine Länge von max 9");
                    hexString = modus + "00" + hexString; //"000" + hexString;
                    break;
                case 2:
                    Debug.WriteLine("der HexString hat eine Länge von 2 Zeichen");
                    hexString = modus + "0" + hexString; //"00" + hexString;
                    break;
                case 3:
                    Debug.WriteLine("der Hexstring hat eine Länge von 3 Zeichen");
                    hexString = modus + hexString; //"0" + hexString;
                    break;
                case 4:
                    Debug.WriteLine("der HexString hat eine länge von 4 Zeichen");
                    hexString = "" + hexString;
                    break;
                default:
                    Debug.WriteLine("FEHLER : der HexString hat eine Länge von " + hexString.Length + " Zeichen");
                    break;
            }

            return hexString;
        }
        public String AddPadding(string hexString)
        {
            return addMissingZeros(hexString);
        }


        private string addMissingZeros(string hexstring)
        {
            string res = hexstring;
            for (int i = res.Length; i < 40; i++)
            {
                res += "0";
            }

            return res;
        }
        public static Int16[] StringToByteArrayInt16(String hex)
        {
            int NumberChars = hex.Length;
            Int16[] array = new Int16[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                array[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return array;
        }

        /*public static byte[] StringToByteArray(String hex)
        {
          int NumberChars = hex.Length;
          byte[] bytes = new byte[NumberChars / 2];
          for (int i = 0; i < NumberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
          return bytes;
        }*/

        public void playSignalNow(Signal signal) 
        {
            

            calculateSignalsForTactile(signal.getTime(), signal.getStrength());
            var writerLength = new Byte[MAX_POINTS];
            var writerMode = new DataWriter();

            for (int i = 0; i < lengthSignal.Length; i++)
            {
                writerLength[i] = (Byte)lengthSignal.ElementAt(i);
            }

            //byte[] myTestByte = { 0x14, 0x00, 0x24, 0x00, 0x13, 0x00, 0x23, 0x00, 0x12, 0x00, 0x22, 0x00, 0x11, 0x00, 0x21, 0x00, 0x14, 0x00, 0x24, 0x00 };

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

                // bytes = myBytes ist schon in calculateTactileLength gemacht worden.
                WriteBytes();

                //var status1 = await CurrentLengthCharacteristic.characteristic.WriteValueAsync(writerLength.AsBuffer());
                //Debug.WriteLine(status1);
                //await CurrentModeCharacteristic.characteristic.WriteValueAsync(writerMode.DetachBuffer());
            } catch {
                Debug.WriteLine("Something went wrong by sending BLE DATA !!!!!!!");
            }
        }

        /*
         * signalAndStrengthHexString[0] = Muster Hex Zeit String
         * signalAndStrengthHexString[1] = Muster Hex Staerke String
         */
        public void playMuster(string[] signalAndStrengthHexString)
        {
            createByteForMuster(signalAndStrengthHexString);
            // send values to tactile device
            try
            {
                WriteBytes();
            }
            catch
            {
                Debug.WriteLine("Something went wrong by sending BLE DATA !!!!!!!");
            }
        }
        
        public void createByteForMuster(string[] tmp)
        {
            byte[] myByteTime = StringToByteArray(tmp[0]);
            byte[] myByteStrength = StringToByteArray(tmp[1]);

            bytes = myByteTime;
            strengthBytes = myByteStrength;
        }

        public async void WriteBytes()
        {
            //byte[] startBytes = { 0x55 };
            //byte[] strengthBytes = { 0xFF };

            // visualizer.AddValue(bytes);
            if (lengthCharacteristic == null || startCharacteristic == null || strengthCharacteristic == null)
            {
                return;
            }

            var lengthWriter = new DataWriter();
            var strengthWriter = new DataWriter();
            var startWriter = new DataWriter();

            long startTime = Environment.TickCount;

            Debug.WriteLine(startTime + " Sending " + ByteArrayToString(bytes));

            lengthWriter.WriteBytes(bytes);
            strengthWriter.WriteBytes(strengthBytes);
            //startWriter.WriteBytes(startBytes);

            GattCommunicationStatus statusLength = await
            lengthCharacteristic.WriteValueAsync(lengthWriter.DetachBuffer()); //TODO catch Exception after disconnect
            GattCommunicationStatus statusStrength = await
            strengthCharacteristic.WriteValueAsync(strengthWriter.DetachBuffer()); //TODO catch Exception after disconnect
            //GattCommunicationStatus statusStart = await
            //startCharacteristic.WriteValueAsync(startWriter.DetachBuffer()); //TODO catch Exception after disconnect

            GattCommunicationStatus status = GattCommunicationStatus.ProtocolError; // DEFAULT IST PROTOKOLL FEHLER 
            if (statusLength == GattCommunicationStatus.Success && statusStrength == GattCommunicationStatus.Success) // && statusStart == GattCommunicationStatus.Success)
            {
                status = GattCommunicationStatus.Success;
            }

            long endTime = Environment.TickCount;
            Debug.WriteLine(endTime + " Status for " + ByteArrayToString(bytes) + ": " + status + ". Time: " + (endTime - startTime) + " ms");
        }

        public string strengthToHexString(int strength, int modus)
        {
            string hexString = "";

            if (modus == 2) // wenn eine Pause ist, ist die Stärke gleich 0 
            {
                hexString = "0000";
            }
            else
            {
                switch (strength)
                {
                    case (int)SignalStrength.VERYWEAK:
                        hexString = "7FFF";
                        break;
                    case (int)SignalStrength.WEAK:
                        hexString = "9FFF";
                        break;
                    case (int)SignalStrength.OK:
                        hexString = "BFFF";
                        break;
                    case (int)SignalStrength.STRONG:
                        hexString = "DFFF";
                        break;
                    case (int)SignalStrength.VERYSTRONG:
                        hexString = "FFFF";
                        break;
                    default:
                        Debug.WriteLine("Es gibt einen Fehler in der strengthToHexStringMethode() !!! ");
                        break;
                }
            }

            return hexString;
        }

        /**
         * diese methode ist fuer die Berechnung von den normalen Signalen.
         */
        private byte[] CalculateStrength(SignalStrength strength)
        {
            byte[] temp = new byte[2];
            switch (strength)
            {
                case SignalStrength.VERYWEAK:
                    temp[0] = 0x7F;
                    temp[1] = 0x00;
                break;
                case SignalStrength.WEAK:
                    
                    temp[0] = 0x9F;
                    temp[1] = 0x00;
                break;
                case SignalStrength.OK:
                    
                    temp[0] = 0xBF;
                    temp[1] = 0x00;
                break;
                case SignalStrength.STRONG:                    
                    temp[0] = 0xDF;
                    temp[1] = 0x00;
                break;
                case SignalStrength.VERYSTRONG:
                    temp[0] = 0xFF;
                    temp[1] = 0x00;
                    break;
                default:

                break;
            }

            return temp;
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            testButton.Content = "Test Button!!!!";

            //setup.testWritingFile();
            //setup.newSaveInFileinCSharp();
            //testWritingFile2222("");
            //testWritingFile222();
            
            string time     = "1032";
            string strength = "7FFF";

            string[] temp = new string[2];
            temp[0] = time;
            temp[1] = strength;

            playMuster(temp);


            testButton.Content = "TestButton DONE!";
        }

        FileOpenPicker picker = new FileOpenPicker();
        StorageFile result;
        public void selectFolder(object sender, RoutedEventArgs e)
        {
            pickFile();
        }

        private async void pickFile()
        {
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add(".txt");

            // Show picker enabling user to pick one file.
            result = await picker.PickSingleFileAsync();
        }

        public void saveErkennungsData(string data)
        {
            testWritingFile2222(data); 
        }

        public async void testWritingFile2222(string input)
        {
            if(result == null)
            {
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.ViewMode = PickerViewMode.List;
                picker.FileTypeFilter.Add(".txt");

                // Show picker enabling user to pick one file.
                result = await picker.PickSingleFileAsync();
            }

            if (result != null)
            {
                try
                {
                    // Use FileIO to replace the content of the text file
                    // await FileIO.WriteTextAsync(result, textBlock.Text);
                    await FileIO.AppendTextAsync(result, input); //string.Format("{0}{1}", "OK", Environment.NewLine));

                    // Display a success message
                    Debug.WriteLine("Status: File saved successfully");
                }
                catch (Exception ex)
                {
                    // Display an error message
                    Debug.WriteLine("Status: error saving the file - " + ex.Message);
                }
            }
            else
            {
                Debug.WriteLine("Status: User cancelled save operation");
            }
        }

        /*public void testWritingFile222()
        {

            string pathDir = Path.GetDirectoryName(@"c:") + "c:\\settings";
            string settingsFile = "settings.txt";
            StreamWriter w;

            // Create the parent directory if it doesn't exist
            if (!Directory.Exists(pathDir))
                Directory.CreateDirectory(pathDir);
            settingsFile = Path.Combine(pathDir, settingsFile);
            w = new StreamWriter(settingsFile, true);


            // Get the directories currently on the C drive.
            DirectoryInfo[] cDirs = new DirectoryInfo(@"c:\").GetDirectories();

            // Write each directory name to a file.
            using (StreamWriter sw = new StreamWriter("CDriveDirs.txt"))
            {
                foreach (DirectoryInfo dir in cDirs)
                {
                    sw.WriteLine(dir.Name);

                }
            }

            // Read and show each line from the file.
            string line = "";
            using (StreamReader sr = new StreamReader("CDriveDirs.txt"))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
        }*/

        private void testButton2(object sender, RoutedEventArgs e)
        {
            myFrame.Navigate(typeof(IntroPage));
        }
        
        private void testButton_21(object sender, RoutedEventArgs e)
        {
            myFrame.Navigate(typeof(IntroPage2));
        }

        private void testButton3(object sender, RoutedEventArgs e)
        {
            myFrame.Navigate(typeof(InitSignalPage));
        }
        private void testButton4(object sender, RoutedEventArgs e)
        {
            myFrame.Navigate(typeof(AlgoSignalPage));
        }
        private void testButton5(object sender, RoutedEventArgs e)
        {
            myFrame.Navigate(typeof(EmotionPage));
        }
        private void testButton6(object sender, RoutedEventArgs e)
        {
            myFrame.Navigate(typeof(ErkennungPage));
        }

        public int[] getMousePosition(string pageType)
        {
            int []pos = new int[2];

            switch (pageType)
            {
                case "IntroPage":
                    pos[0] = 100;
                    pos[1] = 100;
                break;
                case "InitSignalPage":
                    pos[0] = 200;
                    pos[1] = 200;
                break;
                case "AlgoSignalPage":
                    pos[0] = 500;
                    pos[1] = 500;
                break;
                case "EmotionPage":
                    pos[0] = 700;
                    pos[1] = 700;
                break;
                case "ErkennungPage":
                    pos[0] = 900;
                    pos[1] = 900;
                break;
                default:
                    pos[0] = 1000;
                    pos[1] = 1000;
                break;
            }

            return pos;
        }

        public void changeToFrame(Type frameType) 
        {
            myFrame.Navigate(frameType);
        }

        private void TestButtonMoveCursor(object sender, RoutedEventArgs e)
        {
            var p  = Window.Current.CoreWindow.PointerCursor;
            Debug.WriteLine(" PointerCursor ID = " + p.Id + "; type = " + p.Type + "; to string =" + p.ToString());
            var q = Window.Current.CoreWindow.PointerPosition;
            Debug.WriteLine("1- PointerPosition  X = " + q.X + "; Y = " + q.Y);

            Window.Current.CoreWindow.PointerPosition = new Point(1000, 1000);
            var z = Window.Current.CoreWindow.PointerPosition;
            Debug.WriteLine("2- PointerPosition  X = " + z.X + "; Y = " + z.Y);
        }

        private async void radioButtonMittel_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Es wurde mittel gedrückt.");
            await dialog.ShowAsync();
            radioButtonKurz.IsChecked = true;
            radioButtonLang.IsChecked = true;
            radioButtonMittel.IsChecked = false;
        }

        public void setPerson(Person person)
        {
            this.user = person;
            Debug.WriteLine("TEST TEST TEST");
            Debug.WriteLine("this User =  " + user.getAge() + " " + user.getGender() + " musikalisch ? " + user.isMusically());
        }

        public void setOtherPersonValues(bool usedTactil, bool usedWatch, bool playedGames)
        {
            this.user.setTactile(usedTactil);
            this.user.setWatch(usedWatch);
            this.user.setGames(playedGames);

            Debug.WriteLine("TEST TEST TEST");
            Debug.WriteLine("this user =  used Tactile ? " + user.usedTactile() + " ; used Watch ? " + user.usedWatch() + " ; played Games ? " + user.isMusically());
        }

        public void setEmotion(Emotion emote)
        {
            this.user.addEmotion(emote);
        }

        public void setCursorPositionOnDefault(int iX, int iY)
        {
            Window.Current.CoreWindow.PointerPosition = new Point(iX, iY);
            // TODO die richtige Position herausfinden
        }

        public int getGeneration()
        {
            return this.generation;
        }

        public void countGeneration()
        {
            this.generation += 1;
        }

        public void saveInitPopulationBeforeNextStep()
        {
            initPopulation.Add(setup.getInitPupulation());
        }

        public void prepareAlgoForNextGeneration()
        {
            // speicher zuerst die Daten 
            //this.allAlgoPopulations.Add(setup.getAlgoPopulation());
            // anschließend ruf die methode die alle Daten wieder auf default setzt.
            setup.prepareForNextGeneration();
            // zähl eine Generation hoch
            countGeneration();
        }

        public void saveAlgoData()
        {
            // speicher zuerst die Daten 
            this.allAlgoPopulations.Add(setup.getAlgoPopulation());
            calculateNewZones();
        }

        public bool createAlgoDataTextFile()
        {
            bool res = false;
            string algoData = "empty";

            string temp = "";
            #region Algo Signal
            temp = "Algorithmus Signal" + Environment.NewLine;
            temp += line();
            for (int iX = 0; iX < allAlgoPopulations.Count; iX++)
            {
                temp += allAlgoPopulations[iX].createStringAlgoSignal();
            }
            temp += line();
            algoData += temp;
            #endregion
            return res;
        }

        public void saveAllPersonData()
        {
            string temp = "";
            #region Angaben zur Person und Emotion
            temp = "Angaben zur Person" + Environment.NewLine;
            temp += line();

            temp += this.user.ToString();

            temp += line();
            testWritingFile2222(temp);
            #endregion
        }

        public void saveInitSignal()
        {
            #region Init Signal
            string temp = "";
            temp = "Init Signal" + Environment.NewLine;
            temp += line();
            for (int iX = 0; iX < initPopulation.Count; iX++)
            {
                temp += initPopulation[iX].createStringInitialSignal();
            }

            temp += line();
            testWritingFile2222(temp);
            #endregion
        }

        bool firstTimeWritten = false;
        public bool saveAllData()
        {
            bool res = false;
            string allInput = "empty";
            string temp = "";
            #region create String
            /*
            if (!firstTimeWritten)
            {
                firstTimeWritten = true;
                // erstelle den String 
                #region create String
            }*/
            
            #region Algo Signal
            temp = "Algorithmus Signal" + Environment.NewLine;
            temp += line();
            temp += "Emotion: " + this.user.getEmotion()[generation - 1] + Environment.NewLine;
            temp += line();
            temp += "Grenzen: " + Environment.NewLine;
            for(int i = 0; i < 6; i += 2)
            {
                if (i == 0)
                {
                    temp += " Kurz: " + Environment.NewLine;
                }
                else if (i == 2)
                {
                   temp += " Mittel: " + Environment.NewLine;
                }
                else if (i == 4)
                {
                    temp += " Lang: " + Environment.NewLine;
                }
                temp += string.Format("  Beginn :   {0}ms", this.rangeTime[generation - 1][i]) + Environment.NewLine;
                temp += string.Format("  Ende   :   {0}ms", this.rangeTime[generation - 1][i + 1]) + Environment.NewLine;
                temp += string.Format("  Min    :   {0}", ((SignalStrength) this.rangeStrength[generation - 1][i]).ToString("F")) + Environment.NewLine;
                temp += string.Format("  Max    :   {0}", ((SignalStrength)this.rangeStrength[generation - 1][i + 1]).ToString("F")) + Environment.NewLine;
                temp += string.Format("{0},{1},{2},{3}", this.rangeTime[generation - 1][i],
                    this.rangeTime[generation - 1][i + 1], ((SignalStrength)this.rangeStrength[generation - 1][i]).ToString("F"),
                    ((SignalStrength)this.rangeStrength[generation - 1][i + 1]).ToString("F")) + Environment.NewLine;
            }
            temp += line();

            temp += allAlgoPopulations[generation - 1].createStringAlgoSignal();
            
            temp += line();
            allInput += temp;
            #endregion
            #region Erkennung Signal
            // TODO
            #endregion
            #endregion
            
            Debug.WriteLine(allInput);
            // speichere den String in der Datei
            this.testWritingFile2222(allInput);

            // gib das ergebnis zurück ob es in der datei gespeichert worden ist.


            return res; 
        }

        public void calculateNewZones()
        {
            Population lastPopulatioin = setup.getAlgoPopulation();
            int[] newS = lastPopulatioin.calculateNewIntervall();
            int[] newZone = new int[6];
            int[] newStr  = new int[6];
            for (int i = 0; i < 6; i++)
            {
                newZone[i] = newS[i];
            }
            for (int i = 6; i < 12; i++)
            {
                newStr[i - 6] = newS[i]; 
            }
            this.rangeTime.Add(newZone);
            this.rangeStrength.Add(newStr);
        }

        private string line()
        {
            string temp = "";
            for (int i = 0; i < 30; i++)
            {
                temp = temp + "*";
            }
            temp = temp + Environment.NewLine;
            return temp;
        }
        
        public List<int[]> getZones()
        {
            List<int[]> temp = new List<int[]>();
            temp.Add(this.rangeTime[this.generation - 1]);
            temp.Add(this.rangeStrength[this.generation - 1]);

            return temp;
        }

        bool visibleButtons = false;
        private void checkBoxHideButtons_Checked(object sender, RoutedEventArgs e)
        {
            hideMenuButtons();
        }

        private void hideMenuButtons()
        {
            if (!visibleButtons)
            {
                visibleButtons = true;
                testButton.Visibility = Visibility.Collapsed;
                testButton_2.Visibility = Visibility.Collapsed;
                testButton_3.Visibility = Visibility.Collapsed;
                testButton_4.Visibility = Visibility.Collapsed;
                testButton_5.Visibility = Visibility.Collapsed;
                testButton_6.Visibility = Visibility.Collapsed;
                testButton_7.Visibility = Visibility.Collapsed;
                testButton_21_Copy.Visibility = Visibility.Collapsed;
            }
            else
            {
                visibleButtons = false;
                testButton.Visibility = Visibility.Visible;
                testButton_2.Visibility = Visibility.Visible;
                testButton_3.Visibility = Visibility.Visible;
                testButton_4.Visibility = Visibility.Visible;
                testButton_5.Visibility = Visibility.Visible;
                testButton_6.Visibility = Visibility.Visible;
                testButton_7.Visibility = Visibility.Visible;
                testButton_21_Copy.Visibility = Visibility.Visible;
            }

        }
    }
}
