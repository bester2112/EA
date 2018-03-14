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
        private List<List<Signal>> listOfMuster;
        private int[] KTime;
        private int[] MTime;
        private int[] LTime;
        private int[] KStrength;
        private int[] MStrength;
        private int[] LStrength;


        public Muster(int[] timeZone, int[] strongZone)
        {
            // definition von 
            listOfMuster = new List<List<Signal>>();

            // inizialisert die Variablen und setzt den wert der uebergeben wurde
            initialize(timeZone, strongZone);

            // 
            createRandomMuster(POPULATION_SIZE); // createRandomMuster
        }

        private void initialize(int[] timeZone, int[] strongZone)
        {
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
    }
}
