using System;
using System.Collections;
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
        private MainPage rootPage;                      // referenz zur StartSeite
        private Muster m;                               // Objekt das alle Muster generiert (usw)

        private List<List<Signal>> standardMuster;      // beinhaltet alle Muster von den Standartisierten Signalen
        private int indexStandard;                      // Indexvariable für standardMuster 

        private List<List<Signal>> geneticMuster;       // beinhaltet alle Muster von den generierten Signalen
        private int indexGenetic;                       // Indexvariable für geneticMuster 

        private List<Signal> signalList;                // ist das aktuell abgespielte Muster; es wird aus der standardMuster- oder geneticMuster-Liste genommen
        private List<List<Signal>> allMusterList;       // diese Liste beinhaltet alle Muster die abgefragt werden, d.h. alle in der reihenfolge, wie sie abgefragt wurden

        private List<SignalTyp> sList;                  // speichert die Eingabe, (K, M, L)-Reihenfolge, was der Benutzer erkannt hat.
        private List<List<SignalTyp>> allSignalList;    // speichert für jedes Muster, was der Benutzer als Muster erkannt hat.

        private List<long> sTime;                       // speichert die  Zeit, die für die Eingabe benötigt wurde, was der Benutzer erkannt hat.
        private List<List<long>> allTimeList;           // speichert für jedes Muster die Zeit, in der der Benutzer das Muster erkannt hat.

        private String genOrStan;                       // String, das speichert, ob ein generalisiertes oder standardtisiertes Muster aktuell abgespielt wird
        private List<String> allGenORStandList;         // die Liste speichert, was für ein Typ von Signal abgespielt wurde (ein generiertes oder ein standardtisiertes Muster)
        
        private int countReplays;                       // gibt die Anzahl an, wie oft der Replay Button gedrückt wurde.
        private List<int> allReplays;                   // speichert für jedes abgespielte Muster, wie oft Replay gedrückt wurde

        private string[] replayStrings;                 // speichert den Hex String von Time und Strength, wird verwendet um ohne neuberechnung ein Signal erneut abzuspielen.

        private int selectedMuster;                     // gibt an, wie viele Signale aktuell in dem Muster vorhanden sind.
        private int countButtonClicks;                  // gibt an, wie oft eine Eingabe pro Muster getätigt werden kann (Kurz, Mittel, Lang Button druck) 

        private int musterTime;                         // 

        private long startTime;                         // ist die Startzeit von der Zeitmessung der Bewertung vom Benuter

        private int totalMuster;                        // speichert, wie viele Muster bisher abgespielt wurden.

        int sendTime;                                   // Key, der für die Zeit benötigt wird, der asynchron in der MainPage gespeichert wird, dort wird die Zeit gespeichert, die gebötigt wurde um an das Gerät zu senden
        private List<int> allKeysToSendToDevice;        // Keys für alle Muster

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
            commitButton.Visibility = Visibility.Collapsed;
            ButtonKurz.Visibility = Visibility.Collapsed;
            ButtonMittel.Visibility = Visibility.Collapsed;
            ButtonLang.Visibility = Visibility.Collapsed;
            clearButton.Visibility = Visibility.Collapsed;
            TextBlockFrage.Visibility = Visibility.Collapsed;
            pressedButtonText.Text = "Bitte Warten, zur Errinnerung es gibt Süßigkeiten";

            // Erklärung darstellen
            var dialog = new MessageDialog("Im Folgenden wird für Sie eine Folge von Signalen abgespielt." + Environment.NewLine 
                + "Sie sollen die Signale wie Sie die in der Reihenfolge erkannt haben angeben. " + Environment.NewLine
                + "Dabei bedeuten die Anfangsbuchstaben " + Environment.NewLine
                + "   'K' für Kurz" + Environment.NewLine 
                + "   'M' für Mittel" + Environment.NewLine
                + "   'L' für Lang");
            await dialog.ShowAsync();

            // Variablen Initialisierung 
            standardMuster = new List<List<Signal>>();
            indexStandard = 0;

            geneticMuster = new List<List<Signal>>();
            indexGenetic = 0;

            signalList = new List<Signal>();
            allMusterList = new List<List<Signal>>();

            sList = new List<SignalTyp>();
            allSignalList = new List<List<SignalTyp>>();

            sTime = new List<long>();
            allTimeList = new List<List<long>>();

            genOrStan = "MUHH";
            allGenORStandList = new List<String>();

            sendTime = 0;
            allKeysToSendToDevice = new List<int>();

            countReplays = 0;
            allReplays = new List<int>();

            replayStrings = new string[2];

            selectedMuster = 0;
            countButtonClicks = 0;

            musterTime = 0;
            
            // DONE 1. erstelle 2 mal einen Pool 
            // DONE 2. ziehe in zufälliger Zeihenfolge ein Signal aus dem Pool 
            // bzw erstelle einen Pool der in gegebener Reihenfolge die gleichen Signale hat (standardwerte / generiert) 
            // danach nehme nach einander zufällig die Signale aus dem Pool in die 2 neuen Pools 
            // DONE Median berechnet : es soll der Mittelwert genommen werden und nicht der intervallbereich um die Signale zu erzeugen, d.h. es gibt nur einen mittel / kurz / langen wert das gleiche bei der stärke
            // es soll dann auswählbar sein, ob das zuerst die normalen signale alle abgespielt werden können, dann die generierten / andersrum oder gemischt

            // Muster wurden erzeugt
            createMuster(); // erstelle Muster

            playMusterOnceForInit();

            // Warte eine Zeit, bis das Signal abgespielt wurde

            // verstecken des Commit buttons
            pressedButtonText.Text = "";
            ButtonKurz.Visibility = Visibility.Visible;
            ButtonMittel.Visibility = Visibility.Visible;
            ButtonLang.Visibility = Visibility.Visible;
            clearButton.Visibility = Visibility.Visible;
            TextBlockFrage.Visibility = Visibility.Visible;

            // Cursor auf Startposition setzen 
            int[] temp = rootPage.getMousePosition("ErkennungPage");
            rootPage.setCursorPositionOnDefault(temp[0], temp[1]);

            // Starten der Zeit 
            this.startTime = Environment.TickCount;
        }

        // erzeugt eine Liste Von mehreren Mustern
        private void createMuster()
        {
            List<int[]> temp = new List<int[]>();

            if (rootPage.getGeneration() != 0)
            {
                temp = rootPage.getZones();
            }
            else
            {
                //TODO DELETE HARD FIX NUR ZUM ZEIGEN  
                // DIESER CODE WIRD NICHT AUSGEFÜHRT, IN DER EIGENTLICHEN STUDIE
                int[] temp2 = new int[6];
                for (int i = 0; i < 6; i += 2)
                {
                    temp2[i] = 100 * (i + 1);
                    temp2[i + 1] = 200 * (i + 1);
                }
                temp.Add(temp2);

                int[] temp3 = new int[6];
                for (int i = 0; i < 6; i += 2)
                {
                    temp3[i] = 1;
                    temp3[i + 1] = 5;
                }
                temp.Add(temp3);
                // ENDE TODO
            }

            m = new Muster(temp[0], temp[1]);

            // lade die 3er Muster / die Muster sind schon zufällig verteilt.
            geneticMuster = m.getListGeneriert3();
            standardMuster = m.getListStandard3();
            selectedMuster = 3;
        }

        public async void nextSignal()
        {
            if (nextSignalIsAvailable())
            {
                // wenn es verfügbar ist, dann soll ein nächstes Signal genommen werden.
                if (indexGenetic <= indexStandard)
                {
                    signalList = geneticMuster[indexGenetic];
                    genOrStan = "Genetisch";
                    indexGenetic++;
                }
                else // indexGenetic > indexStandard
                {
                    signalList = standardMuster[indexStandard];
                    genOrStan = "Standard";
                    indexStandard++;
                }
            }              
            else
            {
                // ansonsten muss das nächste paar gewählt werden also 4er / 5er 
                switch (selectedMuster)
                {
                    case 3:
                        // alle 3er Muster wurden schon abgearbeitet. 
                        geneticMuster = m.getListGeneriert4();
                        standardMuster = m.getListStandard4();
                        selectedMuster = 4;
                        var dialog4 = new MessageDialog("Nach dem nächsten Signal werden Muster mit 4 Signale abgespielt." + Environment.NewLine);
                        await dialog4.ShowAsync();
                        break;
                    case 4:
                        // alle 4er Muster wurden schon abgearbeitet
                        geneticMuster = m.getListGeneriert5();
                        standardMuster = m.getListStandard5();
                        selectedMuster = 5;
                        var dialog5 = new MessageDialog("Nach dem nächsten Signal werden Muster mit 5 Signale abgespielt." + Environment.NewLine);
                        await dialog5.ShowAsync();
                        break;
                    case 5:
                        // alle 5er Muster sind abgearbeitet worden,
                        // TODO Mache das, was gemacht werden soll, wenn alles fertig ist
                        var dialog = new MessageDialog("Die Studie ist nach der folgenden Bewertung erfolgreich beendet, bitte schließen Sie das Programm NICHT!");
                        await dialog.ShowAsync();
                        printEverything();
                        break;
                    default:
                        Debug.WriteLine("fehler in der nextSignal() ErrinnerungsPage.cs Methode \n");
                        break;
                }
                indexGenetic  = 0;
                indexStandard = 0;
            }
        }

        // überprüft, ob noch muster vorhanden sind, die noch nicht abgefragt wurden
        public bool nextSignalIsAvailable()
        {
            bool res = true;
            if (indexGenetic == indexStandard && indexStandard == geneticMuster.Count) // testen vllt Capacity - 1
            {
                res = false;
            }
            return res;
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
            rootPage.playMuster(tmp, sendTime);
            sendTime++;
            replayStrings = tmp;
        }

        private void Replay(object sender, RoutedEventArgs e)
        {
            rootPage.playMuster(replayStrings, sendTime);
            sendTime++;
            countReplays++;
        }


        // erstellt einen String für die aus der stärke und der zeit
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
                tempStrength[i] = (int)s.getStrength();
                
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
            sTime = new List<long>();
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

            playMuster();

            // Cursor auf Startposition setzen 
            int[] temp = rootPage.getMousePosition("ErkennungPage");
            rootPage.setCursorPositionOnDefault(temp[0], temp[1]);
        }

        private async void playMuster()
        {
            // Hole Signal das nächste Signal
            nextSignal();

            // erzeuge einen String vom Muster
            string[] tmp = createString(); // erstelle aus den ersten Muster jetzt einen String, fuer das Abspielen des Signals

            // Spiele Muster ab
            if (totalMuster <= 90)
            {
                playSignal(tmp); // Signal abspielen
            }
            else
            {
                Debug.WriteLine("BLA");
            }
            totalMuster++;

            // Speicher Daten vom Muster
            // Eingabe von der letzten Vibration speichern
            allReplays.Add(countReplays);
            allTimeList.Add(sTime);
            allSignalList.Add(sList);
            // und anschließend Variablen auf 0 setzen 
            countReplays = 0;
            sList = new List<SignalTyp>();
            sTime = new List<long>();

            if (totalMuster > 90)
            {
                var dialog = new MessageDialog("Die Studie ist erfolgreich beendet, bitte schließen Sie das Programm NICHT!");
                await dialog.ShowAsync();

                disableAllButtons();
                string allDataString = printEverything();
                saveAllData(allDataString);
                return;
            }

            // Variablen für die nächste Vibration speichern
            allGenORStandList.Add(genOrStan);
            allMusterList.Add(signalList);
            allKeysToSendToDevice.Add(sendTime);

            this.startTime = Environment.TickCount;
            printOnScreen();
        }

        private void disableAllButtons()
        {
            ButtonKurz.Visibility = Visibility.Collapsed;
            ButtonMittel.Visibility = Visibility.Collapsed;
            ButtonLang.Visibility = Visibility.Collapsed;
            commitButton.Visibility = Visibility.Collapsed;
            clearButton.Visibility = Visibility.Collapsed;
        }

        private void playMusterOnceForInit()
        {
            // Hole Signal das nächste Signal
            nextSignal();

            // erzeuge einen String vom Muster
            string[] tmp = createString(); // erstelle aus den ersten Muster jetzt einen String, fuer das Abspielen des Signals

            // Spiele Muster ab
            playSignal(tmp); // Signal abspielen
            totalMuster++; // zählt den counter hoch 

            // Speicher Daten vom Muster
            allGenORStandList.Add(genOrStan);
            allMusterList.Add(signalList);
            allKeysToSendToDevice.Add(sendTime);

            this.startTime = Environment.TickCount;
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


        private string printEverything()
        {
            string str = "";

            str += "ERKENNUNG" + Environment.NewLine;
            for (int i = 0; i < 90; i++)
            {
                str += "---------" + Environment.NewLine;
                str += string.Format(" --- {0}.Erkennungs  Muster-- -", i) + Environment.NewLine;

                string compromised = "";
                string sallMusterString = "";
                string pauseTimeTemp = "";
                List<Signal> sTemp = allMusterList[i];
                string sAllMusterTimeString = "";
                string tempAllMusterComp = "";
                string tempPauseComp = "";
                int musterTime = 0;
                for (int j = 0; j < sTemp.Count; j++)
                {
                    int theTime = sTemp[j].getTime();
                    if (j < (sTemp.Count - 1))
                    {
                        musterTime += theTime;
                    }
                    if (j%2 == 0)
                    {
                        sallMusterString += string.Format("{0} ", sTemp[j].getType());
                        compromised += string.Format("{0},", sTemp[j].getType());
                        sAllMusterTimeString += string.Format("  {0}.         :        {1}ms", (j + 1),
                                                        sTemp[j].getTime()) + Environment.NewLine;
                        tempAllMusterComp += string.Format("{0},", theTime);
                    }
                    else
                    {
                        pauseTimeTemp += string.Format("  {0}.         :        {1}ms", (j + 1),
                                                        sTemp[j].getTime()) + Environment.NewLine;
                        tempPauseComp += string.Format("{0},",theTime);
                    }
                }

                string sallSignalString = "";
                List<SignalTyp> sTypTemp = allSignalList[i];
                for (int j = 0; j < sTypTemp.Count; j++)
                {
                    sallSignalString += string.Format("{0} ", sTypTemp[j].ToString("F"));
                    compromised += string.Format("{0},", sTypTemp[j].ToString("F"));
                }

                string sallTimeString = "";
                List<long> sTimeTemp = allTimeList[i];
                sallTimeString += string.Format(" Zeit benötigt : ") + Environment.NewLine;
                for (int j = 0; j < sTimeTemp.Count; j++)
                {
                    sallTimeString += string.Format("  {0}.         :        {1}ms", (j + 1), sTimeTemp[j]) + Environment.NewLine;
                    compromised += string.Format("{0},", sTimeTemp[j]);
                }

                str += string.Format(" Muster Typ :         {0}", this.allGenORStandList[i]) + Environment.NewLine;
                str += string.Format(" Muster abgespielt :  {0}", sallMusterString) + Environment.NewLine;
                str += string.Format(" Muster erkannt :     {0}", sallSignalString) + Environment.NewLine;
                str += string.Format("{0}", sallTimeString);
                str += string.Format(" #Replays:            {0}", this.allReplays[i]) + Environment.NewLine;
                // ohne der Zeit von der letzten Pause
                str += string.Format(" Länge des Musters    {0}", musterTime) + Environment.NewLine;
                str += string.Format(" Länge der Pause:" + Environment.NewLine + "{0}", pauseTimeTemp);
                str += string.Format(" Länge der Signale:" + Environment.NewLine + "{0}", sAllMusterTimeString);

                Hashtable timesToSends = rootPage.getTimeToSend();
                str += string.Format(" Zeit zum übertragen: {0}ms", timesToSends[this.allKeysToSendToDevice[i]]) + Environment.NewLine;

                compromised += musterTime + ",";
                compromised += tempPauseComp;
                compromised += tempAllMusterComp;

                str += string.Format("!{0},{1}{2},{3}", this.allGenORStandList[i], compromised, timesToSends[this.allKeysToSendToDevice[i]], this.allReplays[i]) + Environment.NewLine;
            }
            
            return str;
        }

        private void saveAllData(string data)
        {
            rootPage.saveErkennungsData(data);
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
