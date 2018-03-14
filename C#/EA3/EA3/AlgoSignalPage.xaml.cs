using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace EA3
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class AlgoSignalPage : Page
    {
        private MainPage rootPage;

        private SignalTyp untypedSignal;
        private SignalRating signalRating;
        private SignalStrength signalStrength;
        private long startTime;
        private long endTimeSignal;
        private long endTimeRating;
        private long endTimeStrength;
        private long diffTimeSignal;
        private long diffTimeRating;
        private long diffTimeStrength;
        private bool bType;
        private bool bRating;
        private bool bStrength;


        public AlgoSignalPage()
        {
            this.InitializeComponent();

            setVariablesDefault();

            // Erklärungstext aufrufen und Zeit starten
            initialize();

            #region UI Initialisierung 
            FirstLineText.Text = "Bitte bewerten Sie das Signal.";
            TextBlockFrage1.Text = "Was für einen Signaltypen haben Sie erkannt?";
            TextBlockFrage2.Text = "Wie gut haben Sie das Signal als X Signal erkannt?";
            TextBlockFrage3.Text = "Wie empfanden Sie die Stärke des Signals?";
            #endregion
        }
        private void setVariablesDefault()
        {
            untypedSignal    = SignalTyp.NODATA;
            signalRating     = SignalRating.NODATA;
            signalStrength   = SignalStrength.NODATA;
            bType            = false;
            bRating          = false;
            bStrength        = false;
            startTime        = Environment.TickCount;
            endTimeSignal    = -1;
            endTimeRating    = -1;
            endTimeStrength  = -1;
            diffTimeSignal   = -1;
            diffTimeRating   = -1;
            diffTimeStrength = -1;
        }

        private async void initialize()
        {
            // Erklärung darstellen
            var dialog = new MessageDialog("Im Folgenden werden Für Sie Personalisierte Signale gespielt. Bitte beantworten Sie die folgende Fragen.");
            await dialog.ShowAsync();

            // Cursor auf Startposition setzen 
            int[] temp = rootPage.getMousePosition("AlgoSignalPage");
            rootPage.setCursorPositionOnDefault(temp[0], temp[1]);

            // Signal abspielen 
            playSignal();

            // Starten der Zeit 
            this.startTime = Environment.TickCount;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
        }

        private void Replay(object sender, RoutedEventArgs e)
        {
            // TODO 
            Signal signal = rootPage.setup.getLastSignal(); // noch abfragen, ob das Signal vorhanden ist und ich nicht NULL zurueck erhalte 
            rootPage.playSignalNow(signal); // spielt das aktullle Signal ab.
        }

        #region UI Radiobuttons
        #region UI Radiobuttons Type
        private void RadioButtonKurz_Click(object sender, RoutedEventArgs e)
        {
            endTimeSignal = Environment.TickCount;
            RadioButtonKurz.IsChecked = true;
            untypedSignal = SignalTyp.KURZ;
            bType = true;

            afterClick();
        }

        private void RadioButtonMittel_Click(object sender, RoutedEventArgs e)
        {
            endTimeSignal = Environment.TickCount;
            RadioButtonMittel.IsChecked = true;
            untypedSignal = SignalTyp.MITTEL;
            bType = true;

            afterClick();
        }

        private void RadioButtonLang_Click(object sender, RoutedEventArgs e)
        {
            endTimeSignal = Environment.TickCount;
            RadioButtonLang.IsChecked = true;
            untypedSignal = SignalTyp.LANG;
            bType = true;

            afterClick();
        }
        #endregion

        #region UI Radiobuttons Rating
        private void RadioButtonVeryBad_Click(object sender, RoutedEventArgs e)
        {
            endTimeRating = Environment.TickCount;
            RadioButtonVeryBad.IsChecked = true;
            signalRating = SignalRating.VERYBAD;
            bRating = true;
            
            afterClick();
        }

        private void RadioButtonBad_Click(object sender, RoutedEventArgs e)
        {
            endTimeRating = Environment.TickCount;
            RadioButtonBad.IsChecked = true;
            signalRating = SignalRating.BAD;
            bRating = true;

            afterClick();
        }

        private void RadioButtonOK_Click(object sender, RoutedEventArgs e)
        {
            endTimeRating = Environment.TickCount;
            RadioButtonOK.IsChecked = true;
            signalRating = SignalRating.OK;
            bRating = true;

            afterClick();
        }

        private void RadioButtonGood_Click(object sender, RoutedEventArgs e)
        {
            endTimeRating = Environment.TickCount;
            RadioButtonGood.IsChecked = true;
            signalRating = SignalRating.GOOD;
            bRating = true;

            afterClick();
        }

        private void RadioButtonVeryGood_Click(object sender, RoutedEventArgs e)
        {
            endTimeRating = Environment.TickCount;
            RadioButtonVeryGood.IsChecked = true;
            signalRating = SignalRating.VERYGOOD;
            bRating = true;

            afterClick();
        }
        #endregion

        #region UI Radiobuttons Strength
        private void RadioButtonVeryWeak_Click(object sender, RoutedEventArgs e)
        {
            endTimeStrength = Environment.TickCount;
            RadioButtonVeryWeak.IsChecked = true;
            signalStrength = SignalStrength.VERYWEAK;
            bStrength = true;

            afterClick();
        }

        private void RadioButtonWeak_Click(object sender, RoutedEventArgs e)
        {
            endTimeStrength = Environment.TickCount;
            RadioButtonWeak.IsChecked = true;
            signalStrength = SignalStrength.WEAK;
            bStrength = true;

            afterClick();
        }

        private void RadioButtonPassend_Click(object sender, RoutedEventArgs e)
        {
            endTimeStrength = Environment.TickCount;
            RadioButtonPassend.IsChecked = true;
            signalStrength = SignalStrength.OK;
            bStrength = true;

            afterClick();
        }

        private void RadioButtonStrong_Click(object sender, RoutedEventArgs e)
        {
            endTimeStrength = Environment.TickCount;
            RadioButtonStrong.IsChecked = true;
            signalStrength = SignalStrength.STRONG;
            bStrength = true;

            afterClick();
        }

        private void RadioButtonVeryStrong_Click(object sender, RoutedEventArgs e)
        {
            endTimeStrength = Environment.TickCount;
            RadioButtonVeryStrong.IsChecked = true;
            signalStrength = SignalStrength.VERYSTRONG;
            bStrength = true;

            afterClick();
        }
        #endregion

        #endregion

        private void afterClick()
        {
            if (bType && bRating && bStrength)
            {
                evaluateSignal();
                
                playSignal();

                // TODO auskommentieren
                // Cursor auf Startposition setzen 
                //int[] temp = rootPage.getMousePosition("AlgoSignalPage");
                //rootPage.setCursorPositionOnDefault(temp[0], temp[1]);
                //startTime = Environment.TickCount;
            }
        }

        int counter = 0;

        private async void playSignal()
        {
            if (rootPage.setup.isNextElementAvailable())
            {
                counter++;
                String s = rootPage.setup.playSignal();
                Debug.WriteLine("Everything Counter = " + counter + " Signal = " + s);

                Signal signal = rootPage.setup.getLastSignal();
                rootPage.playSignalNow(signal); // spielt das aktullle Signal ab.
            }
            else
            {
                // Alle Daten wurden evalutiert, jetzt wird der Algorithmus ausgefuehrt.
                // die Daten werden im gleichen Datensatz überschrieben, d.h. auf Daten von vorherigen Generationen kann man nicht mehr zurueckgreifen
                // TODO Daten muessen gespeichert werden.
                var dialog = new MessageDialog("Es wird Anhand Ihrer Eingaben die nächsten Signale erstellt. \n" +
                                               "Bitte warten Sie einen Augenblick.");

                // hier muss es gespeichert werden, vor der berechnung
                // TODOOOOOOO
                // WICHTIG
                rootPage.saveAlgoData();

                rootPage.setup.calculateFitness();

                await dialog.ShowAsync();
                //await Task.Delay(1000);

                //setup.SaveInFileAlgo();

                rootPage.prepareAlgoForNextGeneration();
                rootPage.changeToFrame(typeof(EmotionPage));
            }

        }

        private async void evaluateSignal()
        {
            // berechne die Zeit, die benoetigt wurde
            diffTimeSignal   = endTimeSignal - startTime;
            diffTimeRating   = endTimeRating - startTime;
            diffTimeStrength = endTimeStrength - startTime;
            Debug.WriteLine("Time Type " + diffTimeSignal + " Time Rating " + diffTimeRating + " Time Strength " + diffTimeStrength);
            /** Zu übertragende Daten sind 
            /*  untypedSignal,  signalRating,   signalStrength
             *  diffTimeSignal, diffTimeRating, diffTimeStrength
             */
            rootPage.setup.saveSignalAlgo(untypedSignal,  signalRating, signalStrength, diffTimeSignal, diffTimeRating, diffTimeStrength);

            // kurzes Delay (in ms)
            await Task.Delay(400);

            switch (untypedSignal)
            {
                case SignalTyp.KURZ:
                    RadioButtonKurz.IsChecked = false;
                    break;
                case SignalTyp.MITTEL:
                    RadioButtonMittel.IsChecked = false;
                    break;
                case SignalTyp.LANG:
                    RadioButtonLang.IsChecked = false;
                    break;
                default:
                    Debug.WriteLine("ERROR in switch (untypedSignal) in der  -evaluateSignal-  Methode in Klasse AlgoSignalPage");
                    break;
            }

            switch (signalRating)
            {
                case SignalRating.VERYBAD:
                    RadioButtonVeryBad.IsChecked = false;
                    break;
                case SignalRating.BAD:
                    RadioButtonBad.IsChecked = false;
                    break;
                case SignalRating.OK:
                    RadioButtonOK.IsChecked = false;
                    break;
                case SignalRating.GOOD:
                    RadioButtonGood.IsChecked = false;
                    break;
                case SignalRating.VERYGOOD:
                    RadioButtonVeryGood.IsChecked = false;
                    break;
                default:
                    Debug.WriteLine("ERROR in switch (signalRating) in der  -evaluateSignal- Methode in Klasse AlgoSignalPage");
                    break;
            }

            switch (signalStrength)
            {
                case SignalStrength.VERYWEAK:
                    RadioButtonVeryWeak.IsChecked = false;
                    break;
                case SignalStrength.WEAK:
                    RadioButtonWeak.IsChecked = false;
                    break;
                case SignalStrength.OK:
                    RadioButtonPassend.IsChecked = false;
                    break;
                case SignalStrength.STRONG:
                    RadioButtonStrong.IsChecked = false;
                    break;
                case SignalStrength.VERYSTRONG:
                    RadioButtonVeryStrong.IsChecked = false;
                    break;
                default:
                    Debug.WriteLine("ERROR in switch (signalStrength) in der  -evaluateSignal-  Methode in Klasse AlgoSignalPage");
                    break;
            }

            setVariablesDefault();
        }
    }
}
