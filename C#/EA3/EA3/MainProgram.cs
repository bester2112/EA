using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EA3
{
    public class MainProgram
    {
        public static readonly int MUTATIONRATE = 5; // angabe in Prozent 5 ist das zwischen 0 und 4 also 5 % die wahrscheinlichkeit liegt, dass eine Zufaellige Zahl bestimmt wird

        //public static readonly int ;
        public static readonly int MINTIME = 0;
        public static readonly int MAXTIME = 1000;

        public static int MINKURZTIME = 100;
        public static int MAXKURZTIME = 300;

        public static int MINMITTELTIME = 400;
        public static int MAXMITTELTIME = 500;

        public static int MINLANGTIME = 600;
        public static int MAXLANGTIME = 700;

        private int startSize;
        Population p;
        Population pAlgo;
        int firstTime = 0;

        public MainProgram()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());



            /*Signal k1 = new Signal(SignalTyp.KURZ, 100);
             Signal k2 = new Signal(SignalTyp.KURZ, 200);
             Signal k3 = new Signal(SignalTyp.KURZ, 250);
             Signal m1 = new Signal(SignalTyp.MITTEL, 400);
             Signal m2 = new Signal(SignalTyp.MITTEL, 450);
             Signal m3 = new Signal(SignalTyp.MITTEL, 500);
             Signal l1 = new Signal(SignalTyp.LANG, 600);
             Signal l2 = new Signal(SignalTyp.LANG, 650);
             Signal l3 = new Signal(SignalTyp.LANG, 700);

             DNA dk1 = new DNA(k1);
             DNA dk2 = new DNA(k2);
             DNA dk3 = new DNA(k3);
             DNA dm1 = new DNA(m1);
             DNA dm2 = new DNA(m2);
             DNA dm3 = new DNA(m3);
             DNA dl1 = new DNA(l1);
             DNA dl2 = new DNA(l2);
             DNA dl3 = new DNA(l3);

             Console.WriteLine("child von k1 und k2");
             dk1.crosover(dk2);
             Console.WriteLine("child von k1 und k3");
             dk1.crosover(dk3);

             Console.WriteLine("child von m1 und m2");
             dm1.crosover(dm2);
             Console.WriteLine("child von m1 und m3");
             dm1.crosover(dm3);

             Console.WriteLine("child von l1 und l2");
             dl1.crosover(dl2);
             Console.WriteLine("child von l1 und l3");
             dl1.crosover(dl3);

             for (int i = 0; i < 100; i++) {
                 dk2.mutate(MUTATIONRATE);
             }*/

            // Gebe die Werte für die anfangspopulation ein.


            // Dies ist eine Test eingabe:
            //int[] x = { 1, 1, 1, 2, 1, 2, 1, 2, 1, 2, 2, 2, 3, 2, 2, 3, 3, 3, 3, 3 };
            //Population startPop = new Population(20, x);

            startSize = 10;
            p = new Population(startSize, true);
            //p.equallyDistibuted(); // gleich verteilt.

            /*for (int i = 0; i < startSize; i++)
            {
                p.calculate();
            }*/


            /*int min = 0;
            int max = 99;
            for (int i = 0; i < 100; i++) {
                Random r = new Random();
                int res = r.nextInt((max - min) + 1) + min;
                Console.Write("RANDOM : " + res);
                if (res < 5) {
                    Console.Write(" ______TRUEEEEEEEE______");
                }
                Console.WriteLine();
            }*/
        }


        public string playStartSignal()
        {
            // Signal abspielen.
            if (p.isElementAvailable())
            {
                String signalText = p.getNextSignal().getSignal().printString();
                return signalText;
            }
            return "NO ELEMENT AVALABLE IN playSignalStart() Methode!, Bitte frage vorher mit isStartElementAvailable() ab";
        }

        // Diese Methode berechnet fuer die Startpopulation die Bereiche der Zonen.
        // return true, wenn es richtig berechnet wurde
        // false, wenn die Daten nicht zu gebrauchen Sind, der Benutzer muss die Daten dann erneut eingeben
        public bool calculateStartZones()
        {
            bool res = false;
            if (!p.calculateZone())
            {
                // erneut benutzer abfragen
                p = new Population(startSize, true);

                res = false;
            } else {
                p.calculateArithmeticMedian(); // TODO muss noch ordentlich geprüft werden.
                p.calculateNewZones();

                res = true;
            }
            return res;
        }

        /**
        *** Diese Methode fragt ab ob vorhandene Elemente noch existieren, 
        *** falls ja, dann kann playSignalStart() aufgerufen werden.
        **/
        public bool isElementAvailable()
        {
            return p.isElementAvailable();
        }

        public string playSignal()
        {
            ////////////////////////////////////////TEST
            List<int> arr = new List<int>();
            for (int i = 1; i <= 100; i ++)
            {
                p.getUniqueRandomNumber(ref arr, 0, 101);
            }
            //arr.Sort();
            foreach (int x in arr)
            {
                Debug.WriteLine(x);
            }
            ////////////////////////////////////////END TEST

            if (firstTime == 0)
            {
                setMinMaxTime();
                pAlgo = new Population(startSize);
                firstTime++;
            }
            // Signal abspielen.
            if (pAlgo.isElementAvailable())
            {
                Signal s = pAlgo.getNextSignal().getSignal();
                String temp = "";
                String signalText = s.printString();
                switch (s.getType()) {
                    case SignalTyp.KURZ:
                        temp = "Das Folgende Signal ist ein KURZES Signal. \n" +
                            "Bitte bewerten Sie, wie gut Sie dieses KURZE Signal erkennen?";
                    break;
                    case SignalTyp.MITTEL:
                        temp = "Das Folgende Signal ist ein MITTLERES Signal. \n" +
                            "Bitte bewerten Sie, wie gut Sie dieses MITTLERE Signal erkennen?";
                    break;
                    case SignalTyp.LANG:
                        temp = "Das Folgende Signal ist ein LANGES Signal. \n" +
                            "Bitte bewerten Sie, wie gut Sie dieses LANGE Signal erkennen?";
                    break;
                    default:
                        temp = "ES ist leider ein Fehler in der playSignal() Methode unterlaufen. Bitte Rufen Sie Ihren Superviser zu Rate";
                    break;
                }
                return temp + "\n" + signalText;
            }
            return "NO ELEMENT AVALABLE IN playSignal() Methode!, Bitte frage vorher mit isElementAvailable() ab";
        }

        

        public bool calculateFitness()
        {
            pAlgo.calculate();

            return true; // TODO
        }

        public bool isNextElementAvailable()
        {
            if (firstTime == 0)
            {
                return true;
            }

            return pAlgo.isElementAvailable();
        }



        private void setMinMaxTime()
        {
            int[] zones = p.getZones();
            MINKURZTIME = zones[0];
            MAXKURZTIME = zones[1];
            MINMITTELTIME = zones[2];
            MAXMITTELTIME = zones[3];
            MINLANGTIME = zones[4];
            MAXLANGTIME = zones[5];

            bool res = false;
            
            for (int i = 0; i < zones.Length; i++)
            {
                if (zones[i] <= 0)
                {
                    res = true;
                } 
            }

            if (res)
            { 
                MINKURZTIME = 50;
                MAXKURZTIME = 350;

                MINMITTELTIME = 400;
                MAXMITTELTIME = 650;

                MINLANGTIME = 700;
                MAXLANGTIME = 1000;
        }
    }

        public int getValue()
        {
            return startSize;
        }

        // diese Methode liefert das nächste Signal
        public void nextSignal(SignalTyp signalTyp)
        {
            p.saveSignalType(signalTyp);
        }

        public void nextSignalRating(SignalRating signalRating)
        {
            pAlgo.saveSignalRating(signalRating);
        }
    }
}
