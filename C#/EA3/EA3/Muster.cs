using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA3
{
    public class Muster
    {
        private static readonly int POPULATION_SIZE = 20;
        private static readonly int MAXSIGNAL = 10;
        private static readonly int PAUSEEND = 100;
        private static readonly int PAUSE2 = 200;

        private static readonly int KURZSTANDARD = 150;
        private static readonly int MITTELSTANDARD = 300;
        private static readonly int LANGSTANDARD = 600;

        private static readonly SignalStrength KURZSTRENGTH = SignalStrength.STRONG;
        private static readonly SignalStrength MITTELSTRENGTH = SignalStrength.STRONG;
        private static readonly SignalStrength LANGSTRENGTH = SignalStrength.STRONG;

        private List<List<Signal>> listOfMuster3;
        private List<List<Signal>> listOfMusterStandard3;
        private List<List<Signal>> listOfMusterGeneriert3;
        private List<List<Signal>> listOfMuster4;
        private List<List<Signal>> listOfMusterStandard4;
        private List<List<Signal>> listOfMusterGeneriert4;
        private List<List<Signal>> listOfMuster5;
        private List<List<Signal>> listOfMusterStandard5;
        private List<List<Signal>> listOfMusterGeneriert5;
        private List<int> indexStandard = new List<int>();
        private List<int> indexGeneriert = new List<int>();
        private int[] KTime;
        private int[] MTime;
        private int[] LTime;
        private int[] KStrength;
        private int[] MStrength;
        private int[] LStrength;
        private int TimeK;
        private int TimeM;
        private int TimeL;
        private int StrengthK;
        private int StrengthM;
        private int StrengthL;


        public Muster(int[] timeZone, int[] strongZone)
        {
            // definition von Pools / Listen von Mustern
            listOfMuster3 = new List<List<Signal>>();
            listOfMusterStandard3 = new List<List<Signal>>();
            listOfMusterGeneriert3 = new List<List<Signal>>();
            listOfMuster4 = new List<List<Signal>>();
            listOfMusterStandard4 = new List<List<Signal>>();
            listOfMusterGeneriert4 = new List<List<Signal>>();
            listOfMuster5 = new List<List<Signal>>();
            listOfMusterStandard5 = new List<List<Signal>>();
            listOfMusterGeneriert5 = new List<List<Signal>>();

            // inizialisert die Variablen und setzt den wert der uebergeben wurde
            initialize(timeZone, strongZone);

            // erstelle eine Population von den Signalen.
            createCollection();
            // kopiere jetzt in zufälliger Reihenfolge, Elemente in den Standard Pool hinein.
            listOfMusterStandard3 = copyIntoStandardPool(listOfMuster3);
            listOfMusterStandard4 = copyIntoStandardPool(listOfMuster4);
            listOfMusterStandard5 = copyIntoStandardPool(listOfMuster5);
            //copyIntoStandardPool();

            // kopiere jetzt in zufälliger Reihenfolge, Elemente in den Generierten Pool hinein.
            listOfMusterGeneriert3 = copyIntoGeneriertPool(listOfMuster3);
            listOfMusterGeneriert4 = copyIntoGeneriertPool(listOfMuster4);
            listOfMusterGeneriert5 = copyIntoGeneriertPool(listOfMuster5);

            // 
            //createRandomMuster(POPULATION_SIZE); // createRandomMuster
        }

        private void initialize(int[] timeZone, int[] strongZone)
        {
            // Intervalle der Kurzen / Mittleren / Langen Zeit und Stärke speichern
            KTime = new int[2];
            MTime = new int[2];
            LTime = new int[2];
            KStrength = new int[2];
            MStrength = new int[2];
            LStrength = new int[2];

            KTime[0] = timeZone[0];
            KTime[1] = timeZone[1];
            MTime[0] = timeZone[2];
            MTime[1] = timeZone[3];
            LTime[0] = timeZone[4];
            LTime[1] = timeZone[5];

            KStrength[0] = strongZone[0];
            KStrength[1] = strongZone[1];
            MStrength[0] = strongZone[2];
            MStrength[1] = strongZone[3];
            LStrength[0] = strongZone[4];
            LStrength[1] = strongZone[5];

            // den Median der jeweiliden Intervalle bestimmen 
            TimeK = (KTime[0] + KTime[1]) / 2;
            TimeM = (MTime[0] + MTime[1]) / 2;
            TimeL = (LTime[0] + LTime[1]) / 2;

            // TODO Stärke muss bei X,5 zufällig ausgewählt werden.
            StrengthK = (KStrength[0] + KStrength[1]) / 2;
            StrengthM = (MStrength[0] + MStrength[1]) / 2;
            StrengthL = (LStrength[0] + LStrength[1]) / 2;
        }

        // erzeuge eine Sammlung von Mustern 
        private void createCollection()
        {
            int kurz = (int)  SignalTyp.KURZ;
            int mittel = (int) SignalTyp.MITTEL;
            int lang = (int) SignalTyp.LANG;

            int pauseKurz1   = 100;
            int pauseKurz2   = 200;
            int pauseMittel1 = 100;
            int pauseMittel2 = 200;
            int pauseMittel3 = 300;
            int pauseLang1   = 100;
            int pauseLang2   = 200;
            int pauseLang3   = 300;
            int pauseLang4   = 400;

            #region definition Muster mit 3 signalen
            listOfMuster3.Add(generateSignals(kurz, kurz, kurz, pauseKurz1, pauseKurz1));
            listOfMuster3.Add(generateSignals(kurz, mittel, mittel, pauseKurz1, pauseKurz2));
            listOfMuster3.Add(generateSignals(kurz, mittel, kurz, pauseKurz2, pauseKurz2));
            listOfMuster3.Add(generateSignals(kurz, lang, lang, pauseKurz1, pauseKurz2));
            listOfMuster3.Add(generateSignals(kurz, lang, kurz, pauseKurz2, pauseKurz2));

            listOfMuster3.Add(generateSignals(mittel, mittel, mittel, pauseKurz1, pauseKurz1));
            listOfMuster3.Add(generateSignals(mittel, kurz, kurz, pauseKurz1, pauseKurz2));
            listOfMuster3.Add(generateSignals(mittel, kurz, mittel, pauseKurz2, pauseKurz2));
            listOfMuster3.Add(generateSignals(mittel, lang, lang, pauseKurz1, pauseKurz2));
            listOfMuster3.Add(generateSignals(mittel, lang, kurz, pauseKurz2, pauseKurz2));

            listOfMuster3.Add(generateSignals(lang, lang, lang, pauseKurz1, pauseKurz1));
            listOfMuster3.Add(generateSignals(lang, kurz, kurz, pauseKurz1, pauseKurz2));
            listOfMuster3.Add(generateSignals(lang, kurz, lang, pauseKurz2, pauseKurz2));
            listOfMuster3.Add(generateSignals(lang, mittel, mittel, pauseKurz1, pauseKurz2));
            listOfMuster3.Add(generateSignals(lang, mittel, kurz, pauseKurz2, pauseKurz2));
            #endregion

            #region definition Muster mit 4 Signalen
            listOfMuster4.Add(generateSignals(kurz, kurz, kurz, kurz, pauseMittel1, pauseMittel1, pauseMittel1));
            listOfMuster4.Add(generateSignals(kurz, mittel, kurz, mittel, pauseMittel1, pauseMittel3, pauseMittel2));
            listOfMuster4.Add(generateSignals(kurz, mittel, mittel, mittel, pauseMittel3, pauseMittel1, pauseMittel2));
            listOfMuster4.Add(generateSignals(kurz, lang, mittel, kurz, pauseMittel1, pauseMittel3, pauseMittel2));
            listOfMuster4.Add(generateSignals(kurz, lang, lang, kurz, pauseMittel3, pauseMittel1, pauseMittel2));

            listOfMuster4.Add(generateSignals(mittel, mittel, mittel, mittel, pauseMittel1, pauseMittel1, pauseMittel1));
            listOfMuster4.Add(generateSignals(mittel, lang, kurz, lang, pauseMittel1, pauseMittel3, pauseMittel2));
            listOfMuster4.Add(generateSignals(mittel, kurz, kurz, mittel, pauseMittel1, pauseMittel2, pauseMittel1));
            listOfMuster4.Add(generateSignals(mittel, lang, kurz, lang, pauseMittel1, pauseMittel1, pauseMittel2));
            listOfMuster4.Add(generateSignals(mittel, lang, lang, mittel, pauseMittel1, pauseMittel2, pauseMittel3));

            listOfMuster4.Add(generateSignals(lang, lang, lang, lang, pauseMittel1, pauseMittel1, pauseMittel1));
            listOfMuster4.Add(generateSignals(lang, kurz, mittel, kurz, pauseMittel1, pauseMittel2, pauseMittel3));
            listOfMuster4.Add(generateSignals(lang, kurz, lang, kurz, pauseMittel3, pauseMittel2, pauseMittel1));
            listOfMuster4.Add(generateSignals(lang, mittel, lang, lang, pauseMittel1, pauseMittel2, pauseMittel3));
            listOfMuster4.Add(generateSignals(lang, mittel, kurz, mittel, pauseMittel2, pauseMittel3, pauseMittel1));
            #endregion

            #region definition Muster mit 5 Signalen
            listOfMuster5.Add(generateSignals(kurz, kurz, kurz, kurz, kurz, pauseLang1, pauseLang1, pauseLang1, pauseLang1));
            listOfMuster5.Add(generateSignals(kurz, mittel, lang, kurz, mittel, pauseLang2, pauseLang1, pauseLang4, pauseLang3));
            listOfMuster5.Add(generateSignals(kurz, mittel, lang, mittel, kurz, pauseLang3, pauseLang2, pauseLang1, pauseLang1));
            listOfMuster5.Add(generateSignals(kurz, lang, lang, mittel, kurz, pauseLang2, pauseLang1, pauseLang4, pauseLang3));
            listOfMuster5.Add(generateSignals(kurz, lang, lang, mittel, lang, pauseLang1, pauseLang2, pauseLang3, pauseLang1));

            listOfMuster5.Add(generateSignals(mittel, mittel, mittel, mittel, mittel, pauseLang1, pauseLang1, pauseLang1, pauseLang1));
            listOfMuster5.Add(generateSignals(mittel, mittel, kurz, lang, lang, pauseLang1, pauseLang2, pauseLang3, pauseLang1));
            listOfMuster5.Add(generateSignals(mittel, kurz, mittel, kurz, lang, pauseLang3, pauseLang2, pauseLang4, pauseLang1));
            listOfMuster5.Add(generateSignals(mittel, lang, kurz, lang, mittel,  pauseLang1, pauseLang2, pauseLang1, pauseLang2));
            listOfMuster5.Add(generateSignals(mittel, kurz, lang, lang, mittel, pauseLang3, pauseLang1, pauseLang4, pauseLang1));

            listOfMuster5.Add(generateSignals(lang, lang, lang, lang, lang, pauseLang1, pauseLang1, pauseLang1, pauseLang1));
            listOfMuster5.Add(generateSignals(lang, kurz, mittel, lang, kurz, pauseLang1, pauseLang4, pauseLang2, pauseLang1));
            listOfMuster5.Add(generateSignals(lang, kurz, mittel, lang, kurz, pauseLang2, pauseLang1, pauseLang3, pauseLang2));
            listOfMuster5.Add(generateSignals(lang, mittel, lang, kurz, lang, pauseLang2, pauseLang4, pauseLang2, pauseLang4));
            listOfMuster5.Add(generateSignals(lang, mittel, mittel, lang, lang, pauseLang1, pauseLang3, pauseLang4, pauseLang1));
            #endregion

            /*for (int typeA = 1; typeA < 4; typeA++)
            {
                for (int typeB = 1; typeB < 4; typeB++)
                {
                    for (int typeC = 1; typeC < 4; typeC++)
                    {
                        
                        Debug.WriteLine("{0},{1},{2}", (SignalTyp)typeA, (SignalTyp)typeB, (SignalTyp)typeC);
                        listOfMuster.Add(generateSignals(typeA,typeB,typeC));
                    }
                }
            }*/
        }


        // Diese methode erstellt Muster mit 5 Signalen
        private List<Signal> generateSignals(int typeA, int typeB, int typeC, int typeD, int typeE, int pause1, int pause2, int pause3, int pause4)
        {
            List<Signal> signalsAndPause = new List<Signal>();
            Signal s = new Signal();

            // geriere Signal von dem Typ A und füge es in die Liste hinzu
            s = generateSignal(typeA);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen A und füge es in die Liste hinzu
            s = geratePause(pause1);
            signalsAndPause.Add(s);
            
            // geriere Signal von dem Typ B und füge es in die Liste hinzu
            s = generateSignal(typeB);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen B und füge es in die Liste hinzu
            s = geratePause(pause2);
            signalsAndPause.Add(s);
            
            // geriere Signal von dem Typ C und füge es in die Liste hinzu
            s = generateSignal(typeC);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen C und füge es in die Liste hinzu
            s = geratePause(pause3);
            signalsAndPause.Add(s);

            // geriere Signal von dem Typ D und füge es in die Liste hinzu
            s = generateSignal(typeD);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen D und füge es in die Liste hinzu
            s = geratePause(pause4);
            signalsAndPause.Add(s);

            // geriere Signal von dem Typ E und füge es in die Liste hinzu
            s = generateSignal(typeE);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen E und füge es in die Liste hinzu
            s = geratePause(PAUSEEND);
            signalsAndPause.Add(s);

            return signalsAndPause;
        }


        // Diese Methode erstellt Muster mit 4 Signalen
        private List<Signal> generateSignals(int typeA, int typeB, int typeC, int typeD, int pause1, int pause2, int pause3)
        {
            List<Signal> signalsAndPause = new List<Signal>();
            Signal s = new Signal();

            // geriere Signal von dem Typ A und füge es in die Liste hinzu
            s = generateSignal(typeA);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen A und füge es in die Liste hinzu
            s = geratePause(pause1);
            signalsAndPause.Add(s);
            
            // geriere Signal von dem Typ B und füge es in die Liste hinzu
            s = generateSignal(typeB);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen B und füge es in die Liste hinzu
            s = geratePause(pause2);
            signalsAndPause.Add(s);
            
            // geriere Signal von dem Typ C und füge es in die Liste hinzu
            s = generateSignal(typeC);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen C und füge es in die Liste hinzu
            s = geratePause(pause3);
            signalsAndPause.Add(s);

            // geriere Signal von dem Typ D und füge es in die Liste hinzu
            s = generateSignal(typeD);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen D und füge es in die Liste hinzu
            s = geratePause(PAUSEEND);
            signalsAndPause.Add(s);

            return signalsAndPause;
        }


        // Diese Methode erstellt Muster mit 3 Signalen
        private List<Signal> generateSignals(int typeA, int typeB, int typeC, int pause1, int pause2)
        {
            // Initialisierung
            List<Signal> signalsAndPause = new List<Signal>();
            Signal s = new Signal();

            // geriere Signal von dem Typ A und füge es in die Liste hinzu
            s = generateSignal(typeA);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen A und füge es in die Liste hinzu
            s = geratePause(pause1);
            signalsAndPause.Add(s);


            // geriere Signal von dem Typ B und füge es in die Liste hinzu
            s = generateSignal(typeB);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen B und füge es in die Liste hinzu
            s = geratePause(pause2);
            signalsAndPause.Add(s);


            // geriere Signal von dem Typ C und füge es in die Liste hinzu
            s = generateSignal(typeC);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen C und füge es in die Liste hinzu
            s = geratePause(PAUSEEND);
            signalsAndPause.Add(s);
            

            return signalsAndPause;
        }

        // generiert ein generiertes Signal anhand dem Typen 
        private Signal generateSignal(int typ)
        {
            SignalTyp type = (SignalTyp) typ;
            Signal s = new Signal();

            switch (type)
            {
                case SignalTyp.KURZ:
                    // Typ, Random Zeit, Random Strength, iBegin, iEnd
                    s = new Signal(SignalTyp.KURZ, TimeK, (SignalStrength) StrengthK, KTime[0], KTime[1]);
                break;
                case SignalTyp.MITTEL:
                    // Typ, Random Zeit, Random Strength, iBegin, iEnd
                    s = new Signal(SignalTyp.MITTEL, TimeM, (SignalStrength) StrengthM, MTime[0], MTime[1]);
                break;
                case SignalTyp.LANG:
                    // Typ, Random Zeit, Random Strength, iBegin, iEnd
                    s = new Signal(SignalTyp.LANG, TimeL, (SignalStrength) StrengthL, LTime[0], LTime[1]);
                break;
                default:
                    Debug.WriteLine("Error in der generateSignal Methode in Muster.cs");
                break;
            }
            
            return s;
        }

        // diese Methode generiert ein Pause Signal
        private Signal geratePause(int time)
        {
            Signal s = new Signal(SignalTyp.KURZ, time, (SignalStrength)StrengthK, time - 20, time + 20);
            return s;
        }

        // diese Methode kopiert die Signale aus dem listOfMuster in zufälliger Reihenfolge heraus und fügt diese dann in die Standard liste hinein.
        private List<List<Signal>> copyIntoStandardPool(List<List<Signal>> listMuster)
        {
            int randomIndex = -1;
            int startIndex = 0;
            List<List<Signal>> standardMuster = new List<List<Signal>>();

            do
            {
                // gehe das Array zufaellig durch und frage nach (daher die grenzen 0 bis n-1)
                randomIndex = getRandom(0, (listMuster.Count - 1));
                if (!indexStandard.Contains(randomIndex))
                {
                    // füge das Element in die Liste hinzu
                    indexStandard.Add(randomIndex);

                    // lese das noch nicht genommene element aus der Liste 
                    List<Signal> l = new List<Signal>(listMuster[randomIndex]);
                    // setze die Werte für die Standard werte
                    List<Signal> c = setAllValuesStandard(l);
                    // füge es in der neuen Liste hinzu
                    standardMuster.Add(c);

                    // erhöhe den Index
                    startIndex++;
                }
                // es muss überprüft werden, dass man nicht über die Anzahl der Elemente drauf zugreift, sonst endet man in der While Schleife 
                // und man kommt nicht mehr raus.
            } while (startIndex != listMuster.Count);

            return standardMuster;
        }

        // setzt alle Signale der Liste mit Standardwerten
        private List<Signal> setAllValuesStandard(List<Signal> list)
        {
            List<Signal> newList = new List<Signal>();
            Signal s = new Signal();
            for (int i = 0; i < list.Count; i++)
            { 
                if ((i % 2) == 0) // wenn es ein Signal ist, dann soll der Wert angepasst werden
                {
                    SignalTyp type = list[i].getType();
                    switch (type)
                    {
                        case SignalTyp.KURZ:
                            s = new Signal(SignalTyp.KURZ, KURZSTANDARD, KURZSTRENGTH, KURZSTANDARD - 20, KURZSTANDARD + 20);
                            //list[i].setTime(KURZSTANDARD);
                            //list[i].setStrength(SignalStrength.STRONG);
                        break;
                        case SignalTyp.MITTEL:
                            s = new Signal(SignalTyp.MITTEL, MITTELSTANDARD, MITTELSTRENGTH, MITTELSTANDARD - 20, MITTELSTANDARD + 20);
                            //list[i].setTime(MITTELSTANDARD);
                            //list[i].setStrength(SignalStrength.STRONG);
                        break;
                        case SignalTyp.LANG:
                            s = new Signal(SignalTyp.LANG, LANGSTANDARD, LANGSTRENGTH, LANGSTANDARD - 20, LANGSTANDARD + 20);
                            //list[i].setTime(LANGSTANDARD);
                            //list[i].setStrength(SignalStrength.STRONG);
                        break;
                        default:
                            Debug.WriteLine("Error in der setAllValuesStandard Methode in Muster.cs");
                        break;
                    }
                }
                else // Pause 
                {
                    int t = list[i].getTime();
                    s = new Signal(SignalTyp.KURZ, t, list[i].getStrength(), t - 20, t + 20);
                }
                newList.Add(s);
            }
            return newList;
        }

        // diese Methode kopiert die Signale aus dem listOfMuster in zufälliger Reihenfolge heraus und fügt diese dann in die Standard liste hinein.
        private List<List<Signal>> copyIntoGeneriertPool(List<List<Signal>> listMuster)
        {
            int randomIndex = -1;
            int startIndex = 0;

            List<List<Signal>> genMuster = new List<List<Signal>>();
            do
            {
                // gehe das Array zufaellig durch und frage nach (daher die grenzen 0 bis n-1)
                randomIndex = getRandom(0, (listMuster.Count - 1));
                if (!indexGeneriert.Contains(randomIndex))
                {
                    // füge das Element in die Liste hinzu
                    indexGeneriert.Add(randomIndex);

                    // lese das noch nicht genommene element aus der Liste 
                    List<Signal> l = new List<Signal>(listMuster[randomIndex]);
                    // setze die Werte für die Standard werte
                    List<Signal> c = setAllValuesGeneriert(l);
                    // füge es in der neuen Liste hinzu
                    genMuster.Add(c);

                    // erhöhe den Index
                    startIndex++;
                }
                // es muss überprüft werden, dass man nicht über die Anzahl der Elemente drauf zugreift, sonst endet man in der While Schleife 
                // und man kommt nicht mehr raus.
            } while (startIndex != listMuster.Count);

            return genMuster;
        }

        // setzt alle Signale der Liste mit Generierten Werten
        private List<Signal> setAllValuesGeneriert(List<Signal> list)
        {
            List<Signal> newList = new List<Signal>();
            Signal s = new Signal();

            for (int i = 0; i < list.Count; i++)
            {
                if ((i % 2) == 0) // wenn es ein Signal ist, dann soll der Wert angepasst werden
                {
                    SignalTyp type = list[i].getType();
                    switch (type)
                    {
                        case SignalTyp.KURZ:
                            s = new Signal(SignalTyp.KURZ, TimeK, (SignalStrength)StrengthK, KTime[0], KTime[1]);
                            //list[i].setTime(TimeK);
                            //list[i].setStrength((SignalStrength)StrengthK);
                        break;
                        case SignalTyp.MITTEL:
                            s = new Signal(SignalTyp.MITTEL, TimeM, (SignalStrength)StrengthM, MTime[0], MTime[1]);
                            //list[i].setTime(TimeM);
                            //list[i].setStrength((SignalStrength)StrengthM);
                        break;
                        case SignalTyp.LANG:
                            s = new Signal(SignalTyp.LANG, TimeL, (SignalStrength)StrengthL, LTime[0], LTime[1]);
                            //list[i].setTime(TimeL);
                            //list[i].setStrength((SignalStrength)StrengthL);
                        break;
                        default:
                            Debug.WriteLine("Error in der setAllValuesGeneriert Methode in Muster.cs");
                        break;
                    }
                } 
                else // Signal der Pause
                {
                    int t = list[i].getTime();
                    s = new Signal(SignalTyp.KURZ, t, list[i].getStrength(), t - 20, t + 20);
                }
                newList.Add(s);
            }

            return newList;
        }

        /*
        private void createRandomMuster(int iN)
        {
            for (int i = 0; i < iN; i++)
            {
                createRandomSignal(MAXSIGNAL);
            }
        }
        
        private void createRandomSignal(int iN)
        {
            Signal s = new Signal();
            List<Signal> fiveSignalsAndPause = new List<Signal>();

            for (int i = 0; i < iN; i++)
            {
                switch (getRandomType())
                {
                    case SignalTyp.KURZ:
                                        // Typ, Random Zeit, Random Strength, iBegin, iEnd
                        s = new Signal(SignalTyp.KURZ, getRandom(KTime[0], KTime[1]), getRandomStrength(KStrength[0], KStrength[1]), KTime[0], KTime[1]);
                    break;
                    case SignalTyp.MITTEL:
                                        // Typ, Random Zeit, Random Strength, iBegin, iEnd
                        s = new Signal(SignalTyp.MITTEL, getRandom(MTime[0], MTime[1]), getRandomStrength(MStrength[0], MStrength[1]), MTime[0], MTime[1]);
                    break;
                    case SignalTyp.LANG:
                                        // Typ, Random Zeit, Random Strength, iBegin, iEnd
                        s = new Signal(SignalTyp.LANG, getRandom(LTime[0], LTime[1]), getRandomStrength(LStrength[0], LStrength[1]), LTime[0], LTime[1]);
                    break;
                    default:
                        Debug.WriteLine("Error in der createRandom Methode in Muster.cs");
                    break;
                }
                fiveSignalsAndPause.Add(s);
            }
            listOfMuster.Add(fiveSignalsAndPause);
        }*/

        private SignalTyp getRandomType()
        {
            SignalTyp sT;

            int temp = getRandom((int) SignalTyp.KURZ, (int) SignalTyp.LANG);

            sT = (SignalTyp)temp;
            return sT;
        }

        private SignalStrength getRandomStrength(int min, int max)
        {
            int result = getRandom(min, max);

            return (SignalStrength)result;
        }

        private int getRandom(int min, int max)
        {
            Random r = new Random();
            int res = r.Next((max - min) + 1) + min;
            return res;
        }

        public List<List<Signal>> getListOfMuster3()
        {
            return this.listOfMuster3;
        }
        public List<List<Signal>> getListOfMuster4()
        {
            return this.listOfMuster4;
        }
        public List<List<Signal>> getListOfMuster5()
        {
            return this.listOfMuster5;
        }

        public List<List<Signal>> getListStandard3()
        {
            return this.listOfMusterStandard3;
        }
        public List<List<Signal>> getListStandard4()
        {
            return this.listOfMusterStandard4;
        }
        public List<List<Signal>> getListStandard5()
        {
            return this.listOfMusterStandard5;
        }

        public List<List<Signal>> getListGeneriert3()
        {
            return this.listOfMusterGeneriert3;
        }
        public List<List<Signal>> getListGeneriert4()
        {
            return this.listOfMusterGeneriert4;
        }
        public List<List<Signal>> getListGeneriert5()
        {
            return this.listOfMusterGeneriert5;
        }
    }
}
