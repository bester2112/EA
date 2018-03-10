using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace EA3
{
    public class DNA
    {

        private Signal signal;
//        private int input;
        private int inputType;
        private double fitness;

        public DNA(Signal signal)
        {
            this.signal = signal;
        }

        public void calculateSignalType()
        {
            // Diese Methode befragt den Benuter
            chooseType();
            validateType();
        }

        /** 
        ** Der Benutzer muss hier jetzt eine Auswahl treffen, bevor das Programm weiter fortfahren kann. 
        **/
        public void chooseType()
        {
            // TODO SIGNAL Abspielen
            Debug.WriteLine("-------------------------------------------");
            Debug.WriteLine("Das Signal ist : ");
            signal.printString();
            Debug.WriteLine("Bitte bewerten Sie das Signal \n\t 1 fuer kurz \n\t 2 fuer mittel \n\t 3 fuer lang");

            Debug.Write("Ihre Eingabe : ");
            //int n = Convert.ToInt32(Debug.ReadLine());
            // TODO
            inputType = 0; // n
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
                    Debug.WriteLine("ERROR in der validateType Funktion");
                    break;
            }
        }

        // die Fitness Funktion wird von dem Probanden evaluieren lassen.
        /*public void calcFitness()
        {
            // Aufrufen der Benutzer Eingabe 
            userInput();
            calculateFitness();
        }*/

        /*public void userInput()
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
                    Debug.WriteLine("ERROR in der userInput Funktion");
                    break;
            }

            Debug.WriteLine("-------------------- ");
            Debug.WriteLine("Das Signal ist : ");
            signal.printString();
            Debug.WriteLine("Das folgende Signal ist " + stype + ".");
            Debug.WriteLine("Bewerten Sie bitte mit den Zahlen \n\t1 gar nicht \n\t2 schlecht \n\t3 ok / geht so \n\t4 gut \n\t5 sehr gut");

            Debug.Write("Ihre Eingabe : ");
            int n;
            //int.TryParse(Debug.ReadLine(), out n);
            // TODO 
            input = 0;//n;
        }*/

        /*public void calculateFitness()
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
                    Debug.WriteLine("ERROR in der calculateFitness Funktion");
                    break;
            }
        }*/

        public void calculateFitnessValue()
        {
            SignalRating rating = signal.getRating();
            switch (rating)
            {
                case SignalRating.VERYBAD:     // Eingabe wurde 'gar nicht' erkannt
                    fitness = (int)rating;
                    break;
                case SignalRating.BAD: // Eingabe wurde 'schlecht' erkannt
                    fitness = (int)rating;
                    break;
                case SignalRating.OK: // Eingabe wurde 'ok / geht so' erkannt
                    fitness = (int)rating;
                    break;
                case SignalRating.GOOD: // Eingabe wurde 'gut' erkannt
                    fitness = (int)rating;
                    break;
                case SignalRating.VERYGOOD: // Eingabe wurde 'sehr gut' erkannt
                    fitness = (int)rating;
                    break;
                default:
                    Debug.WriteLine("ERROR in der calculateFitnessValue Funktion");
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
            Debug.Write("RANDOM 0 : " + resNull);
            Debug.Write(" RANDOM 1 : " + resEins);
            Debug.WriteLine("");

            if ((resNull < mutationRate) && (resEins < mutationRate))
            {
                // die Stelle 0 und die Stelle 1 war zufall, das beide geaendert werden sollten
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

        public void setSignalType(SignalTyp signalTyp, long time)
        {
            signal.setType(signalTyp);
            signal.setTimeToRecognize(time);
        }

        public void setSignalRating(SignalRating rating)
        {
            signal.setRating(rating);
        }
    }
}
