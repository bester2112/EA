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

        public static readonly int MINKURZTIME = 100;
        public static readonly int MAXKURZTIME = 300;

        public static readonly int MINMITTELTIME = 400;
        public static readonly int MAXMITTELTIME = 500;

        public static readonly int MINLANGTIME = 600;
        public static readonly int MAXLANGTIME = 700;

        private int startSize;
        Population p;

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

        /**
        *** Diese Methode fragt ab ob vorhandene Elemente noch existieren, 
        *** falls ja, dann kann playSignalStart() aufgerufen werden.
        **/
        public bool isStartElementAvailable()
        {
            return p.isElementAvailable();
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
        public void calculateStartZones()
        {
            if (!p.calculateZone())
            {
                // erneut benutzer abfragen
            }
            p.calculateArithmeticMedian();
            p.calculateNewZones();
            // Es kann zu Negativen Zahlen kommen in den Methoden, daher muss ich mir das noch mal angucken!
            // TODO
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
    }
}
