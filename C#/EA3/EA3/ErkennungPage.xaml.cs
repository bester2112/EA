using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class ErkennungPage : Page
    {
        private Muster m;
        List<List<Signal>> listListSignal;
        private MainPage rootPage;
        private long startTime;
        private int countButtonClicks;
        private List<SignalTyp> sList;
        private List<List<SignalTyp>> allSignalList;
        private List<long> sTime;
        private int index;
        private List<Signal> signalList;

        public ErkennungPage()
        {
            this.InitializeComponent();
            
            // Erklärungstext aufrufen und Zeit starten
            initialize();

            #region UI Initialisierung 
            FirstLineText.Text = "Erkennung";
            TextBlockFrage.Text = "Was für Signale haben Sie erkannt? (in erkannter Reihenfolge)";
            #endregion
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            rootPage = MainPage.Current;
        }
        private async void initialize()
        {
            // Erklärung darstellen
            var dialog = new MessageDialog("Im Folgenden wird für Sie eine Folge von Signalen abgespielt." + Environment.NewLine 
                + "Sie sollen die Signale wie Sie die in der Reihenfolge erkannt haben angeben. " + Environment.NewLine
                + "Dabei bedeuten die Anfangsbuchstaben " + Environment.NewLine
                + "   'K' für Kurz" + Environment.NewLine 
                + "   'M' für Mittel" + Environment.NewLine
                + "   'L' für Lang");
            await dialog.ShowAsync();

            countButtonClicks = 0;
            index = 0;
            signalList = new List<Signal>();
            listListSignal = new List<List<Signal>>();
            sList = new List<SignalTyp>();
            allSignalList = new List<List<SignalTyp>>();
            sTime = new List<long>();
            // Cursor auf Startposition setzen 
            int[] temp = rootPage.getMousePosition("ErkennungPage");
            rootPage.setCursorPositionOnDefault(temp[0], temp[1]);

            createMuster(); // erstelle Muster
            string tmp = createString(); // erstelle aus den ersten Muster jetzt einen String, fuer das Abspielen des Signals
            playSignal(tmp); // Signal abspielen

            // Warte eine Zeit, bis das Signal abgespielt wurde

            // verstecken des Commit buttons
            commitButton.Visibility = Visibility.Collapsed;

            // Starten der Zeit 
            this.startTime = Environment.TickCount;
        }

        // erzeugt eine Liste Von mehreren Mustern
        private void createMuster()
        {
            List<int[]> temp = rootPage.getZones();
            m = new Muster(temp[0], temp[1]);
            listListSignal = m.getListOfMuster();
            signalList = listListSignal[index];
        }

        public void playSignal(string tmp)
        {
            
            // warte, bis das Signal abgespielt worden ist. 
            // addiere alle zeiten zusammen

        }

        public string createString()
        {
            // erstelle den String / Hex, der benötigt wird um das Signal abzuspielen
            string res = "";
                // TODO gehe hier die Liste mit den 10 signalen durch und erstelle den String von den Zeiten & einen weiteren fuer die Staerke
            return res;
        }
        
        // Loescht die letzte Eingabe
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            sList = new List<SignalTyp>();
            countButtonClicks = 0;
            printOnScreen();

            activateComittButton();
        }
       
        // bestaetige die Eingabe
        private void CommitButton_Click(object sender, RoutedEventArgs e)
        {
            activateComittButton();
            //es gespeichert (usw werden)

            countButtonClicks = 0;
            index++;
            signalList = listListSignal[index];

            allSignalList.Add(sList);
            sList = new List<SignalTyp>();
            printOnScreen();

        }

        // Hilfsmethode, die Buttons wieder aktiviert
        private void activateComittButton()
        {
            // Comitt Button daktiviern
            commitButton.Visibility = Visibility.Collapsed;
            // die 3 Buttons aktivieren
            ButtonKurz.Visibility = Visibility.Visible;
            ButtonMittel.Visibility = Visibility.Visible;
            ButtonLang.Visibility = Visibility.Visible;
        }


        #region UI 
        #region Eingabe Buttons
        private void ButtonKurz_Click(object sender, RoutedEventArgs e)
        {
            afterClick(SignalTyp.KURZ);
        }

        private void ButtonMittel_Click(object sender, RoutedEventArgs e)
        {
            afterClick(SignalTyp.MITTEL);
        }

        private void ButtonLang_Click(object sender, RoutedEventArgs e)
        {
            afterClick(SignalTyp.LANG);
        }
        #endregion

        // zeigt den Text an, nach dem auf ein Button gedrueckt wurde
        private void afterClick(SignalTyp type)
        {
            countButtonClicks++;
            sList.Add(type);
            long tempTime = Environment.TickCount;
            sTime.Add(tempTime - this.startTime);
            printOnScreen();

            if (countButtonClicks < 5)
            {
                // Wenn es kleiner als 5 Zeichen ist, dann soll erst noch weiter bewertet werden
                commitButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                // wenn es 5 Zeichen sind, dann sollen die Buttons verschwinden und der Commit Button erscheinen
                // erst wenn auf den Comitt Button gedrück wird, dann soll es gespeichert werden

                // Committ Button aktivieren
                commitButton.Visibility = Visibility.Visible;
                // die 3 Buttons disablen
                ButtonKurz.Visibility = Visibility.Collapsed;
                ButtonMittel.Visibility = Visibility.Collapsed;
                ButtonLang.Visibility = Visibility.Collapsed;

            }
        }


        // gibt das derzeitig grdrückte auf dem Bildschirm aus
        private void printOnScreen()
        {
            string temp = "";
            for (int i = 0; i < sList.Count; i++)
            {
                temp += getStringFrom(sList[i]);
                if (i < sList.Count - 1)
                {
                    temp += " - ";
                }
            }
            pressedButtonText.Text = temp;
        }

        // gibt an welcher Knopf gedrueckt wird und gibt den string zurueck
        private string getStringFrom(SignalTyp type)
        {
            string res = "";
            switch (type)
            {
                case SignalTyp.KURZ:
                    res = "K";
                    break;
                case SignalTyp.MITTEL:
                    res = "M";
                    break;
                case SignalTyp.LANG:
                    res = "L";
                    break;
                default:
                    Debug.WriteLine("Error in ErrkennungsPage - getStringFrom() Methode ");
                    break;
            }
            return res;
        }
        #endregion

    }
}
