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
    public sealed partial class InitSignalPage : Page
    {
        private MainPage rootPage;

        private SignalTyp untypedSignal;

        private long startTime;
        private long endTime;
        private List<long> times;
        private int countReplay;
        

        public InitSignalPage()
        {
            this.InitializeComponent();

            // Variablen initialisieren
            untypedSignal = SignalTyp.NODATA;
            times = new List<long>();
            countReplay = 0;

            // Erklärungstext aufrufen und Zeit starten
            initialize();

            /*
            long startTime = Environment.TickCount;
            // Do things
            long endTime = Environment.TickCount;
            Debug.WriteLine(endTime + " benötigte Zeit " + (endTime - startTime) + " ms");
            */

            #region UI Initialisierung 
            FirstLineText.Text = "Bitte bewerten Sie das Signal, wie Sie es empfunden haben.";
            #endregion
        }

        private async void initialize()
        {
            // Erklärung darstellen
            var dialog = new MessageDialog("Es wird Ihnen jetzt nacheinander Signale (einmalig) abgespielt, " +
                "wenn Sie das Signal bewertet haben wird Ihnen das nächste Signal abgespielt. " +
                "Sie können das Signal auch noch mal erneut abspielen lassen.");
            await dialog.ShowAsync();

            // Signal abspielen 
            playSignal();
            // Cursor auf Startposition setzen 
            int[] temp = rootPage.getMousePosition("InitSignalPage");
            rootPage.setCursorPositionOnDefault(temp[0], temp[1]); //800, 500);


            // Starten der Zeit 
            this.startTime = Environment.TickCount;
        }



        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
        }

        private void Replay(object sender, RoutedEventArgs e)
        {
            // TODO Replay Signal
            Signal signal = rootPage.setup.getLastSignal();
            rootPage.playSignalNow(signal); // spielt das aktullle Signal ab.
            countReplay++;
        }

        #region UI RadioButtons
        private void RadioButtonKurz_Clicked(object sender, RoutedEventArgs e)
        {
            this.endTime = Environment.TickCount;
            RadioButtonKurz.IsChecked = true;
            untypedSignal = SignalTyp.KURZ;
            afterClick(true);
        }

        private void RadioButtonMittel_Clicked(object sender, RoutedEventArgs e)
        {
            this.endTime = Environment.TickCount;
            RadioButtonMittel.IsChecked = true;
            untypedSignal = SignalTyp.MITTEL;
            afterClick(true);
        }

        private void RadioButtonLang_Clicked(object sender, RoutedEventArgs e)
        {
            this.endTime = Environment.TickCount;
            RadioButtonLang.IsChecked = true;
            untypedSignal = SignalTyp.LANG;
            afterClick(true);
        }
         #endregion

        // diese methode wird verwendet um das Signal abzuspielen
        private async void playSignal()
        {
            if (rootPage.setup.isElementAvailable())
            {
                String s = rootPage.setup.playStartSignal();
                Debug.WriteLine(s);
                Signal signal = rootPage.setup.getLastSignal();
                rootPage.playSignalNow(signal); // spielt das aktullle Signal ab.
            }
            else
            {
                if (rootPage.setup.calculateStartZones())
                {
                    // Eingabe des Benutzers ist fertig für die validierung der gleichverteilten Population. 
                    // Es kann jetzt die Berechnung der Grenzen für Kurz, Mittel und Lang erfolgen.
                    // Der Benutzer hat alle Daten korrekt eingegeben und die Berechnung ist erfolgt und nun kann der Algorithmus Starten
                    var dialog = new MessageDialog("Ihre Eingabe wurde erfolgreich evaluiert\n" +
                                                   "Bitte drücken Sie auf den 'Bestätigen' um mit den Programm fortzufahren.");
                    await dialog.ShowAsync();

                    rootPage.saveInitPopulationBeforeNextStep();

                    // Frame wird zu AlgoSignalPage gewechelt
                    rootPage.changeToFrame(typeof(AlgoSignalPage));
                }
                else
                {
                    // Der Benutzer hat Eingabe Falsch Evaluiert und es sind keine validen Daten herausgekommen, mit
                    // dem das Programm weiter rechnen kann.
                    var dialog = new MessageDialog("Ihre Daten die Sie eingegeben haben sind leider zu sehr verfälscht.\n" +
                                                   "Sie müssen die Daten erneut eingeben ");
                    await dialog.ShowAsync();
                    // falls ein Fehler aufgetreten ist, wird nach der Fehlermeldung die playSignal Methode erneut ausgeführt, da es keinen Ausloeser / Event mehr gibt um einen Signal zu starten.

                    afterClick(false);
                }
            }
        }
        
        // evaluiert, und spiel Signal ab. 
        // wenn es bei nach einem Durchlauf kein Ergebnis erfolgen konnte, muss false uebergeben werden, denn dann wird eine Neue Session gestartet.
        private void afterClick(bool evaluate)
        {
            if (evaluate)
            {
                evaluateClick();
            }

            playSignal();

            // Cursor auf Startposition setzen 
            int[] temp = rootPage.getMousePosition("InitSignalPage");
            rootPage.setCursorPositionOnDefault(temp[0], temp[1]);
            startTime = Environment.TickCount;
        }

        private async void evaluateClick()
        {
            // speicher den Zustand und die Zeit die benötigt wurde um das Signal zu erkennen
            rootPage.setup.saveSignalTyp(untypedSignal, this.endTime - this.startTime, countReplay);
            countReplay = 0;


            // kurzes Delay (in ms)
            await Task.Delay(400);

            // setze den Wert wieder auf ungesetzt (also leere die Eingabe auf dem Bildschirm)
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
                    Debug.WriteLine("ERROR in der  -EvaluateSignal-  Methode");
                    break;
            }
            
            // setzte den Wert wieder auf NODATA
            untypedSignal = SignalTyp.NODATA;
        }


    }
}
