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

        private int musterTime;
        private string[] replayStrings;

        public ErkennungPage()
        {
            this.InitializeComponent();
            
            // Erklärungstext aufrufen und Zeit starten
            initialize();

            // UI Initialisierung
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

            // Variablen Initialisierung 
            countButtonClicks = 0;
            index = 0;
            musterTime = 0;
            replayStrings = new string[2];
            signalList = new List<Signal>();
            listListSignal = new List<List<Signal>>();
            sList = new List<SignalTyp>();
            allSignalList = new List<List<SignalTyp>>();
            sTime = new List<long>();

            // Cursor auf Startposition setzen 
            int[] temp = rootPage.getMousePosition("ErkennungPage");
            rootPage.setCursorPositionOnDefault(temp[0], temp[1]);
            
            // TODO 1. erstelle 2 mal einen Pool 
            // TODO 2. ziehe in zufälliger Zeihenfolge ein Signal aus dem Pool 
            // bzw erstelle einen Pool der in gegebener Reihenfolge die gleichen Signale hat (standardwerte / generiert) 
            // danach nehme nach einander zufällig die Signale aus dem Pool in die 2 neuen Pools 
            // DONE Median berechnet : es soll der Mittelwert genommen werden und nicht der intervallbereich um die Signale zu erzeugen, d.h. es gibt nur einen mittel / kurz / langen wert das gleiche bei der stärke
            // es soll dann auswählbar sein, ob das zuerst die normalen signale alle abgespielt werden können, dann die generierten / andersrum oder gemischt

            createMuster(); // erstelle Muster
            string[] tmp = createString(); // erstelle aus den ersten Muster jetzt einen String, fuer das Abspielen des Signals

            playSignal(tmp); // Signal abspielen

            // Warte eine Zeit, bis das Signal abgespielt wurde

            // verstecken des Commit buttons
            commitButton.Visibility = Visibility.Collapsed;
            pressedButtonText.Text = "";

            // Starten der Zeit 
            this.startTime = Environment.TickCount;
        }

        // erzeugt eine Liste Von mehreren Mustern
        private void createMuster()
        {
            List<int[]> temp = rootPage.getZones();
            m = new Muster(temp[0], temp[1]);
            listListSignal = m.getListOfMuster();

            listListSignal = m.getListStandard();

            signalList = listListSignal[index];
        }

        /**
         * tmp[0] = Muster Hex Zeit String
         * tmp[1] = Muster Hex Staerke String
         */
        public void playSignal(string[] tmp)
        {
            // das hier passiert in der playMuster Methode:
            // die beiden Strings werden jetzt in der rootPage umgewandelt
            // umwandeln in byte[] (Arrays)
            // Nach dem Umwandeln, ruft man die PlayMethode auf.
            rootPage.playMuster(tmp);
            replayStrings = tmp;
        }

        private void Replay(object sender, RoutedEventArgs e)
        {
            rootPage.playMuster(replayStrings);
        }

        public string[] createString()
        {
            // erstelle den String / Hex, der benötigt wird um das Signal abzuspielen
            string []res = new string[2];
            int[] tempTime = new int[10];
            int[] tempStrength = new int[10];
            string hexTimeString = "";
            string hexStrengthString = "";
            musterTime = 0;
            // TODO gehe hier die Liste mit den 10 signalen durch und erstelle den String von den Zeiten & einen weiteren fuer die Staerke
            for (int i = 0; i < signalList.Count; i++)
            {
                Signal s = signalList[i];
                //s.getTime(), s.getStrength()
                tempTime[i] = s.getTime();
                tempStrength[i] = (int) s.getStrength();

                musterTime += tempTime[i];

                int modus;
                if ((i % 2) == 0)
                {
                    modus = 1; // signal 
                }
                else
                {
                    modus = 2; // pause 
                } 

                hexTimeString += rootPage.timeToHexString(tempTime[i], modus);

                hexStrengthString += rootPage.strengthToHexString(tempStrength[i], modus);

                /*
                //                         Signal----  Pause-----  
                byte    tempOfTesting[] = {0x14, 0x00, 0xFF, 0x00, 
                //                         Signal----  Pause-----
                                           0x13, 0x00, 0x23, 0x00, 
                //                         Signal----  Pause-----
                                           0x12, 0x00, 0x22, 0x00, 
                //                         Signal----  Pause-----
                                           0x11, 0x00, 0x21, 0x00, 
                //                         Signal----  Pause-----
                                           0x14, 0x00, 0x24, 0x00};*/
                                           
            }
            res[0] = rootPage.AddPadding(hexTimeString);
            res[1] = rootPage.AddPadding(hexStrengthString);

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
        private async void CommitButton_Click(object sender, RoutedEventArgs e)
        {
            activateComittButton();
            //es gespeichert (usw werden)

            countButtonClicks = 0;
            index++;
            if (index == listListSignal.Count)
            {
                var dialog = new MessageDialog("Die Studie ist erfolgreich beendet, bitte schließen Sie das Programm NICHT!");
                await dialog.ShowAsync();
            }
            signalList = listListSignal[index];

            string[] tmp = createString(); // erstelle aus den Muster jetzt einen String, fuer das Abspielen des Signals
            playSignal(tmp); // Signal abspielen

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

            if (countButtonClicks < (signalList.Count/2))//5)
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
