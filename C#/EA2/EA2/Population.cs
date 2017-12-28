using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA2
{
    public class Population
    {

        private DNA[] population;
        private int numOfPopulation;

        private List<DNA> poolK;                               // Starte mit einem leeren pool
        private List<DNA> poolM;                               // 
        private List<DNA> poolL;

        private List<DNA> pool;

        private int[] arithmetikMedian;                             // beinhaltet das arithmetische mittel von allen Kurzen (index 0), allen mittleren (index 1) und allen langen (index 2)
        private int[] zones;                                            // beinhaltet die intervall grenzen in der folgenden Reihenfolge [minK, maxK, minM, macM, minL, maxL]	

        /**
         * Erstellt eine Population, bei dem die Elemente gleich verteilt sind
         * @param n anzahl der Elemente
         * @param b 
         */
        public Population(int n, bool b)
        {

            numOfPopulation = n;
            population = new DNA[numOfPopulation];
            pool = new List<DNA>();
            Signal s = null;

            int iX = MainProgram.MAXTIME / numOfPopulation;
            for (int i = 1; i <= numOfPopulation; i++)
            {
                Console.WriteLine(" ----  i = " + i);
                s = new Signal(iX * i);
                population[i - 1] = new DNA(s);
            }

            List<int> array = new List<int>();
            int index = 0;
            do
            {
                // gehe das Array zufÃ¤llig durch und frage nach (daher die grenzen 0 bis n-1)
                int i = getRandom(0, (numOfPopulation - 1));
                if (!array.Contains(i))
                {
                    array.Add(i);
                    population[i].calculateSignalType();
                    index++;
                }
            } while (index != numOfPopulation);

            if (!calculateZone())
            {
                // erneut benutzer abfragen
            }
        }

        public Population(int n, int[] x)
        {
            arithmetikMedian = new int[3];
            zones = new int[6];

            numOfPopulation = n;
            population = new DNA[numOfPopulation];
            pool = new List<DNA>();
            Signal s = null;

            int iX = MainProgram.MAXTIME / numOfPopulation;
            for (int i = 1; i <= numOfPopulation; i++)
            {
                Console.WriteLine(" ----  i = " + i);
                s = new Signal(iX * i);
                population[i - 1] = new DNA(s);
            }

            List<int> array = new List<int>();
            int index = 0;
            do
            {
                // gehe das Array zufÃ¤llig durch und frage nach (daher die grenzen 0 bis n-1)
                int i = getRandom(0, (numOfPopulation - 1));
                if (!array.Contains(i))
                {
                    array.Add(i);
                    //population[i].calculateSignalType();
                    population[i].setInputType(x[i]);
                    population[i].validateType();
                    index++;
                }
            } while (index != numOfPopulation);

            if (!calculateZone())
            {
                // erneut benutzer abfragen
            }
            calculateArithmeticMedian();
            calculateNewZones();
        }

        /**
         * erstelle eine Population mit n Signalen fuer jeden Signaltypen
         * @param n Anzahl zufÃ¤lliger Signale
         */
        public Population(int n)
        {
            numOfPopulation = n * 3;
            population = new DNA[numOfPopulation];
            poolK = new List<DNA>();
            poolM = new List<DNA>();
            poolL = new List<DNA>();

            int signalIndex = -1;
            for (int i = 0; i < numOfPopulation; i++)
            {
                Signal s = null;
                Console.WriteLine("i = " + i + "signalIndex = " + signalIndex);
                if (i % (numOfPopulation / 3) == 0)
                {
                    signalIndex++;
                }

                switch (signalIndex)
                {
                    case 0:
                        s = new Signal(SignalTyp.KURZ, getRandom(MainProgram.MINKURZTIME, MainProgram.MAXKURZTIME));
                        break;
                    case 1:
                        s = new Signal(SignalTyp.MITTEL, getRandom(MainProgram.MINMITTELTIME, MainProgram.MAXMITTELTIME));
                        break;
                    case 2:
                        s = new Signal(SignalTyp.LANG, getRandom(MainProgram.MINLANGTIME, MainProgram.MAXLANGTIME));
                        break;
                    default:
                        Console.WriteLine("ERROR in Population Konstruktor");
                        break;
                }


                population[i] = new DNA(s);
            }

            calculate();
        }

        /**
         * berechnet die Grenzen der Signaltypen
         */
        public bool calculateZone()
        {
            bool res = false;
            int minK = 0;
            int maxK = 0;
            int minM = 0;
            int maxM = 0;
            int minL = 0;
            int maxL = 0;


            for (int i = 0; i < numOfPopulation; i++)
            {
                Signal s = population[i].getSignal();
                int time = s.getTime();
                switch (s.getType())
                {
                    case SignalTyp.KURZ: // kurz
                        if (minK == 0)
                        {
                            minK = time;
                        }
                        if (maxK < time)
                        {
                            maxK = time;
                        }
                        break;
                    case SignalTyp.MITTEL: // mittel
                        if (maxM < time)
                        {
                            maxM = time;
                        }
                        if (minM < maxK)
                        {
                            minM = maxM;
                        }
                        break;
                    case SignalTyp.LANG:  // lang
                        if (maxL < time)
                        {
                            maxL = time;
                        }
                        if (minL < maxM)
                        {
                            minL = time;
                        }
                        break;
                    default:
                        Console.WriteLine("ERROR in der calculateZone Funktion");
                        break;
                }
            }
            if ((minK < maxK) && (maxK < minM) && (minM < maxM) && (maxM < minL) && (minL < maxL))
            {
                res = true;
                Console.WriteLine(minK);
                Console.WriteLine(maxK);
                Console.WriteLine(minM);
                Console.WriteLine(maxM);
                Console.WriteLine(minL);
                Console.WriteLine(maxL);
                zones[0] = minK;
                zones[1] = maxK;
                zones[2] = minM;
                zones[3] = maxM;
                zones[4] = minL;
                zones[5] = maxL;
            }

            return res;
        }

        public void calculateArithmeticMedian()
        {
            int midK = 0;
            int indexK = 0;
            int midM = 0;
            int indexM = 0;
            int midL = 0;
            int indexL = 0;

            for (int i = 0; i < numOfPopulation; i++)
            {
                Signal s = population[i].getSignal();
                switch (s.getType())
                {
                    case SignalTyp.KURZ:
                        midK = midK + s.getTime();
                        indexK++;
                        break;
                    case SignalTyp.MITTEL:
                        midM = midM + s.getTime();
                        indexM++;
                        break;
                    case SignalTyp.LANG:
                        midL = midL + s.getTime();
                        indexL++;
                        break;
                    default:
                        Console.WriteLine("ERROR in der calculateArithmeticMedian Funktion");
                        break;
                }
            }

            arithmetikMedian[0] = midK / indexK;
            arithmetikMedian[1] = midM / indexM;
            arithmetikMedian[2] = midL / indexL;
        }

        public void calculateNewZones()
        {
            // die minimale und maximale grenze von einem Signal kann nur 50 / 1000 sein, daher duerfen diese nicht weiter angepasst werden
            // daher wird zuerst das kurzen und danach das Lange Signal so angepasst, dass der median ungefÃ¤hr passt
            int minK = zones[0];
            int maxK = zones[1];
            int minM = zones[2];
            int maxM = zones[3];
            int minL = zones[4];
            int maxL = zones[5];
            int newMaxK = -1;
            int newMinL = -1;

            int medK = (minK + maxK) / 2;
            int medM = -1;
            int medL = (minL + maxL) / 2;

            if (medK > arithmetikMedian[0])
            {
                // berechne eine neue obere grenze fuer K
                newMaxK = (arithmetikMedian[0] * 2) - minK;
            }

            if (medL > arithmetikMedian[2])
            {
                // bereche eine neue untere grenze fuer L 
                newMinL = (arithmetikMedian[2] * 2) - maxL;
            }

            if (maxK > newMaxK)
            {
                maxK = newMaxK;
                minM = newMaxK + 50; // plus mindestabstand
            }

            if (minL > newMinL)
            {
                minL = newMinL;
                maxM = newMinL - 50; // minus mindestandtand
            }

            int indexLeft = minM;
            int indexRight = maxM;
            int diff = 0;

            int minDiff = 1000;
            List<int> bestLeft = new List<int>();
            List<int> bestRight = new List<int>();
            List<int> bestMid = new List<int>();
            int arrayIndex = 0;

            medM = (indexLeft + indexRight) / 2;
            do
            {
                do
                {
                    if (medM > arithmetikMedian[1])
                    {
                        // TODO berechne besten Median
                        // berechne kleinsten abstand zum median
                        diff = medM - arithmetikMedian[1];
                        if (diff < minDiff)
                        {
                            minDiff = diff;
                            bestLeft.Add(indexLeft);
                            bestRight.Add(indexRight);
                            bestMid.Add(minDiff);
                            arrayIndex++;
                        }
                    }
                    indexLeft++;
                    medM = (indexLeft + indexRight) / 2;
                } while (indexLeft <= arithmetikMedian[1]);
                indexRight--;
                if (indexLeft >= arithmetikMedian[1])
                {
                    indexLeft = minM;
                }
            } while (indexRight >= arithmetikMedian[1]);

            for (int i = 0; i < bestLeft.Count; i++)
            {
                Console.Write(i + ". Best Left  => " + bestLeft[i]);
                Console.Write("\t Best Right  => " + bestRight[i]);
                Console.WriteLine("\t Best Mid  => " + bestMid[i]);
            }
        }

        public void calculate()
        {
            calculateFitness();
            selection();
            generate(poolK, 0);
            generate(poolM, 1);
            generate(poolL, 2);
            print();
        }

        private void calculateFitness()
        {
            for (int i = 0; i < numOfPopulation; i++)
            {
                population[i].calcFitness();
            }
        }

        /**
         * erzeugt eine Population also einen pool von Signalen
         */
        public void selection()
        {
            poolK.Clear();
            poolM.Clear();
            poolL.Clear();

            // Bestimme die maximale Population von der population fuer ein Signal
            double[] maxFitness = new double[3];
            int index = -1;
            for (int i = 0; i < numOfPopulation; i++)
            {
                if (i % (numOfPopulation / 3) == 0)
                {
                    index++;
                }
                if (population[i].getFitness() > maxFitness[index])
                {
                    maxFitness[index] = population[i].getFitness();
                }

            }

            // Basierend auf der Fitness, jedes Element der Population wird eine bestimmte Anzahl an malen in den pool hinzugefuegt
            // eine hohe fitness = mehr eintrÃ¤ge im pool = grÃ¶ÃŸere Wahrscheinlichkeit als Eltern ausgewÃ¤hlt zu werden
            // eine kleine Fitness = weniger EintrÃ¤ge in pool = kleinere Wahrscheinlichkeit, als Eltern ausgewÃ¤hlt zu werden

            index = -1;

            for (int i = 0; i < numOfPopulation; i++)
            {

                if (i % (numOfPopulation / 3) == 0)
                {
                    index++;
                }

                // berechne das intervall zwischen dem 0 und der maximalen Fitness auf das Interwall zwischen 0 und 1
                double fitness = map(population[i].getFitness(), 0, maxFitness[index], 0, 1);
                int n = (int)(fitness * 100);           // wandle das ergebnis zwischen 0 und 100 statt 0 und 1
                switch (index)
                {
                    case 0:
                        for (int j = 0; j < n; j++)
                        {            // fuege die Anzahl der Fitness oft der population in den pool herein
                            poolK.Add(population[i]);
                        }
                        break;
                    case 1:
                        for (int j = 0; j < n; j++)
                        {            // fuege die Anzahl der Fitness oft der population in den pool herein
                            poolM.Add(population[i]);
                        }
                        break;
                    case 2:
                        for (int j = 0; j < n; j++)
                        {            // fuege die Anzahl der Fitness oft der population in den pool herein
                            poolL.Add(population[i]);
                        }
                        break;
                    default:
                        Console.WriteLine("ERROR !!!!! 1234");
                        break;
                }
            }
        }

        public double map(double n, double start1, double stop1, double start2, double stop2)
        {
            double newval = (n - start1) / (stop1 - start1) * (stop2 - start2) + start2;
            if (start2 < stop2)
            {
                return constrain(newval, start2, stop2);
            }
            else
            {
                return constrain(newval, stop2, start2);
            }
        }

        private double constrain(double n, double low, double high)
        {
            return Math.Max(Math.Min(n, high), low);
        }

        /**
         * erzeuge die neue Gernation
         */
        public void generate(List<DNA> pool, int index)
        {
            int min = index * (numOfPopulation / 3);
            int max = (index + 1) * (numOfPopulation / 3);
            for (int i = min; i < max; i++)
            {
                int randA;
                int randB;
                do
                {
                    randA = getRandom(0, (pool.Count - 1));
                    randB = getRandom(0, (pool.Count - 1));
                } while (randA == randB);

                DNA parentA = pool[randA];
                DNA parentB = pool[randB];

                DNA kind = parentA.crosover(parentB);
                kind.mutate(MainProgram.MUTATIONRATE);
                population[i] = kind;
            }
        }

        public void print()
        {
            int signalIndex = -1;
            for (int i = 0; i < numOfPopulation; i++)
            {
                Console.WriteLine("i = " + i + " signalIndex = " + signalIndex);
                if (i % (numOfPopulation / 3) == 0)
                {
                    signalIndex++;
                }

                Console.WriteLine("--------------------------");
                switch (signalIndex)
                {
                    case 0:
                        Console.WriteLine("KURZ");
                        population[i].getSignal().printString();
                        Console.WriteLine("Population INDEX = " + i + " Fitness " + population[i].getFitness());
                        break;
                    case 1:
                        Console.WriteLine("Mittel");
                        population[i].getSignal().printString();
                        Console.WriteLine("Population INDEX = " + i + " Fitness " + population[i].getFitness());
                        break;
                    case 2:
                        Console.WriteLine("LANG");
                        population[i].getSignal().printString();
                        Console.WriteLine("Population INDEX = " + i + " Fitness " + population[i].getFitness());
                        break;
                    default:
                        Console.WriteLine("ERROR in Population Konstruktor");
                        break;
                }
            }
        }

        private int getRandom(int min, int max)
        {
            Random r = new Random();
            int res = r.Next((max - min) + 1) + min;
            return res;
        }
    }
}
