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
        private static readonly int PAUSE1 = 100;
        private static readonly int PAUSE2 = 200;

        private static readonly int KURZSTANDARD = 150;
        private static readonly int MITTELSTANDARD = 300;
        private static readonly int LANGSTANDARD = 600;

        private static readonly SignalStrength KURZSTRENGTH = SignalStrength.STRONG;
        private static readonly SignalStrength MITTELSTRENGTH = SignalStrength.STRONG;
        private static readonly SignalStrength LANGSTRENGTH = SignalStrength.STRONG;

        private List<List<Signal>> listOfMuster;
        private List<List<Signal>> listOfMusterStandard;
        private List<List<Signal>> listOfMusterGeneriert;
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
            listOfMuster = new List<List<Signal>>();
            listOfMusterStandard = new List<List<Signal>>();
            listOfMusterGeneriert = new List<List<Signal>>();

            // inizialisert die Variablen und setzt den wert der uebergeben wurde
            initialize(timeZone, strongZone);

            // erstelle eine Population von den Signalen.
            createCollection();
            // kopiere jetzt in zufälliger Reihenfolge, Elemente in den Standard Pool hinein.
            copyIntoStandardPool();
            // kopiere jetzt in zufälliger Reihenfolge, Elemente in den Generierten Pool hinein.
            copyIntoGeneriertPool();

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
            for (int typeA = 1; typeA < 4; typeA++)
            {
                for (int typeB = 1; typeB < 4; typeB++)
                {
                    for (int typeC = 1; typeC < 4; typeC++)
                    {
                        Debug.WriteLine("{0},{1},{2}", (SignalTyp)typeA, (SignalTyp)typeB, (SignalTyp)typeC);
                        listOfMuster.Add(generateSignals(typeA,typeB,typeC));
                    }
                }
            }
        }

        private List<Signal> generateSignals(int typeA, int typeB, int typeC)
        {
            // Initialisierung
            List<Signal> signalsAndPause = new List<Signal>();
            Signal s = new Signal();

            // geriere Signal von dem Typ A und füge es in die Liste hinzu
            s = generateSignal(typeA);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen A und füge es in die Liste hinzu
            s = geratePause(PAUSE1);
            signalsAndPause.Add(s);


            // geriere Signal von dem Typ B und füge es in die Liste hinzu
            s = generateSignal(typeB);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen B und füge es in die Liste hinzu
            s = geratePause(PAUSE1);
            signalsAndPause.Add(s);


            // geriere Signal von dem Typ C und füge es in die Liste hinzu
            s = generateSignal(typeC);
            signalsAndPause.Add(s);

            // generiere Pause nach dem Typen C und füge es in die Liste hinzu
            s = geratePause(PAUSE1);
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
        private void copyIntoStandardPool()
        {
            int randomIndex = -1;
            int startIndex = 0;
            do
            {
                // gehe das Array zufaellig durch und frage nach (daher die grenzen 0 bis n-1)
                randomIndex = getRandom(0, (listOfMuster.Count - 1));
                if (!indexStandard.Contains(randomIndex))
                {
                    // füge das Element in die Liste hinzu
                    indexStandard.Add(randomIndex);

                    // lese das noch nicht genommene element aus der Liste 
                    List<Signal> l = new List<Signal>(listOfMuster[randomIndex]);
                    // setze die Werte für die Standard werte
                    List<Signal> c = setAllValuesStandard(l);
                    // füge es in der neuen Liste hinzu
                    listOfMusterStandard.Add(c);

                    // erhöhe den Index
                    startIndex++;
                }
                // es muss überprüft werden, dass man nicht über die Anzahl der Elemente drauf zugreift, sonst endet man in der While Schleife 
                // und man kommt nicht mehr raus.
            } while (startIndex != listOfMuster.Count);
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
        private void copyIntoGeneriertPool()
        {
            int randomIndex = -1;
            int startIndex = 0;
            do
            {
                // gehe das Array zufaellig durch und frage nach (daher die grenzen 0 bis n-1)
                randomIndex = getRandom(0, (listOfMuster.Count - 1));
                if (!indexGeneriert.Contains(randomIndex))
                {
                    // füge das Element in die Liste hinzu
                    indexGeneriert.Add(randomIndex);

                    // lese das noch nicht genommene element aus der Liste 
                    List<Signal> l = new List<Signal>(listOfMuster[randomIndex]);
                    // setze die Werte für die Standard werte
                    List<Signal> c = setAllValuesGeneriert(l);
                    // füge es in der neuen Liste hinzu
                    listOfMusterGeneriert.Add(c);

                    // erhöhe den Index
                    startIndex++;
                }
                // es muss überprüft werden, dass man nicht über die Anzahl der Elemente drauf zugreift, sonst endet man in der While Schleife 
                // und man kommt nicht mehr raus.
            } while (startIndex != listOfMuster.Count);
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
        }

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

        public List<List<Signal>> getListOfMuster()
        {
            return this.listOfMuster;
        }

        public List<List<Signal>> getListStandard()
        {
            return this.listOfMusterStandard;
        }

        public List<List<Signal>> getListGeneriert()
        {
            return this.listOfMusterGeneriert;
        }
    }
}
