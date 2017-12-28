using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA2
{
    
    public class DNA
    {

        private Signal signal;
        private int input;
        private int inputType;
        private double fitness;

        public DNA(Signal signal)
        {
            this.signal = signal;
        }

        public void calculateSignalType()
        {
            // benutzer erfragen
            chooseType();
            validateType();
        }

        public void chooseType()
        {
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Das Signal ist : ");
            signal.printString();
            Console.WriteLine("Bitte bewerten Sie das Signal \n\t 1 fuer kurz \n\t 2 fuer mittel \n\t 3 fuer lang");

            Console.Write("Ihre Eingabe : ");
            int n = Convert.ToInt32(Console.ReadLine());

            inputType = n;
        }

        public void validateType()
        {
            switch (inputType)
            {
                case 1: // kurz
                    signal.setType(SignalTyp.KURZ);
                    break;
                case 2: // mittel
                    signal.setType(SignalTyp.MITTEL);
                    break;
                case 3: // lang
                    signal.setType(SignalTyp.LANG);
                    break;
                default:
                    Console.WriteLine("ERROR in der validateType Funktion");
                    break;
            }
        }

        // die Fitness Funktion wird von dem Probanden evaluieren lassen.
        public void calcFitness()
        {
            // Aufrufen der Benutzer Eingabe 
            userInput();
            calculateFitness();
        }

        public void userInput()
        {
            string stype = " UNINIZIALISIERT ";
            switch (signal.getType())
            {
                case SignalTyp.KURZ:
                    stype = "Kurz";
                    break;
                case SignalTyp.MITTEL:
                    stype = "Mittel";
                    break;
                case SignalTyp.LANG:
                    stype = "Lang";
                    break;

                default:
                    Console.WriteLine("ERROR in der userInput Funktion");
                    break;
            }

            Console.WriteLine("-------------------- ");
            Console.WriteLine("Das Signal ist : ");
            signal.printString();
            Console.WriteLine("Das folgende Signal ist " + stype + ".");
            Console.WriteLine("Bewerten Sie bitte mit den Zahlen \n\t1 gar nicht \n\t2 schlecht \n\t3 ok / geht so \n\t4 gut \n\t5 sehr gut");

            Console.Write("Ihre Eingabe : ");
            int n;
            int.TryParse(Console.ReadLine(), out n);

            input = n;
        }

        public void calculateFitness()
        {
            switch (input)
            {
                case 1:     // Eingabe wurde 'gar nicht' erkannt
                    fitness = 1;
                    break;
                case 2: // Eingabe wurde 'schlecht' erkannt
                    fitness = 3;
                    break;
                case 3: // Eingabe wurde 'ok / geht so' erkannt
                    fitness = 1;
                    break;
                case 4: // Eingabe wurde 'gut' erkannt
                    fitness = 1;
                    break;
                case 5: // Eingabe wurde 'sehr gut' erkannt
                    fitness = 1;
                    break;
                default:
                    Console.WriteLine("ERROR in der calculateFitness Funktion");
                    break;
            }
        }


        // TODO durchgehen, was passiert bei ungeraden zahlen, also 151 oder 154 ms
        // crossover erzeugt ein neues Kind aus den beiden Eltern
        public DNA crosover(DNA partner)
        {
            Signal pSignal = partner.getSignal();
            int time = -1;

            int temp = Math.Abs(signal.getEins() - pSignal.getEins());
            int res = temp / 2;
            if (signal.getEins() > pSignal.getEins())
            {
                time = pSignal.getTime() + (res * 5);
            }
            else
            {
                time = signal.getTime() + (res * 5);
            }

            Signal childSignal = new Signal(signal.getType(), time);
            DNA child = new DNA(childSignal);

            return child;
        }

        // 
        public void mutate(int mutationRate)
        {
            int min = 0; // das ist fest, es beginn immer bei 0
            int max = 99;

            Random r = new Random();
            int resEins = r.Next((max - min) + 1) + min;
            int resNull = r.Next((max - min) + 1) + min;
            Console.Write("RANDOM 0 : " + resNull);
            Console.Write(" RANDOM 1 : " + resEins);
            Console.WriteLine();

            if ((resNull < mutationRate) && (resEins < mutationRate))
            {
                // die Stelle 0 und die Stelle 1 war zufall, das beide geÃ¤ndert werden sollten
                //  tt
                // 1100 => 1010 X geht nicht, erweitere dann einfach eine 0 zur 1 
                signal.setEins(signal.getEins() + 1);
                signal.setNull(signal.getNull() - 1);
                signal.printString();
            }
            else
            {
                // nur eine stelle wollte gewechselt werden
                if (resNull < mutationRate)
                { //  1100  => 1110
                    signal.setEins(signal.getEins() + 1);
                    signal.setNull(signal.getNull() - 1);
                }
                if (resEins < mutationRate)
                { //  1100  => 1000
                    signal.setEins(signal.getEins() - 1);
                    signal.setNull(signal.getNull() + 1);
                }
                signal.printString();
            }
        }

        public void setInputType(int iType)
        {
            inputType = iType;
        }

        public Signal getSignal()
        {
            return signal;
        }

        public double getFitness()
        {
            return fitness;
        }

        /*public int getInputType() {
            return inputType;
        }*/
    }

}
