using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EA3
{
    /**
     * Die Klasse Signal repraesentiert ein Signal das nur aus drei Signalen zur verfuegung Steht: 
     * kurz, mittel und lang
     * Jedes Signal hat eine mindest und maximal laenge
     * @author thomas
     */
    public enum SignalTyp
    {
        KURZ,
        MITTEL,
        LANG,
        NODATA
    }

    public enum SignalRating
    {
        VERYBAD = 1,
        BAD = 2,
        OK = 3,
        GOOD = 4,
        VERYGOOD = 5,
        NODATA = -1
    }

    public enum SignalStrength
    {
        VERYWEAK,
        WEAK,
        OK,
        STRONG,
        VERYSTRONG, 
        NODATA 
    }

    public class Signal
    {
        private SignalTyp type;                 // Typ der vom Programm bestimmt wird 

        private SignalTyp recognizeType;        // Typ der vom Benutzer erkannt wurde
        private SignalRating rating;            // rating von dem Typ vom Programm, bewertung durch den Benutzer
        private SignalStrength strength;        // staerke vom Signal, der durch den Benutzer bestimt wurde
        private int time;                       // Zeit in ms
        private string signalCode;              // 
        private char[] cSignalCode;             // 
        private int iNull;                      // Anzahl der Nullen
        private int iEins;                      // Anzahl der Einsen
        private int begin;                      // MinimalZeit fuer den Typ von Signal
        private int end;                        // MaximalZeit fuer den Typ von Signal
        private long timeToRecognizeType;       // Zeit die benoetigt wurde um den Signal Typen zu erkennen 
        private long timeToRecognizeRating;     // Zeit die benoetigt wurde um das Signal zu bewerten 
        private long timeToRecognizeStrength;   // Zeit die benoetigt wurde um die Stärke zu bewerten

        /**
         * erzeugt ein Signal, dass mit der genannten Zeit
         * @param sTime Zeit in ms
         */
        public Signal(int sTime)
        {
            defaultVariables();
            type = SignalTyp.NODATA;
            
            time = sTime;
            init();
        }

        /**
         * Konstruktor des Signals
         * @param sType Signaltyp 
         * @param sTime Zeit in ms, die sich in dem Intervall des Signaltyps befinden soll
         */
        public Signal(SignalTyp sType, int sTime)
        {
            defaultVariables();
            type = sType;
            time = sTime;
            //cSignalCode = new char [(end/5)];
            init();
        }

        /**
         * initialisiert alle Werte
         */
        private void defaultVariables()
        {
            type = SignalTyp.NODATA;

            recognizeType = SignalTyp.NODATA;
            rating = SignalRating.NODATA;
            strength = SignalStrength.NODATA;
            time = -1;
            signalCode = "empty";
            iNull = -1;
            iEins = -1;
            begin = -1;
            end = -1;
            timeToRecognizeType = -1;
            timeToRecognizeRating = -1;
            timeToRecognizeStrength = -1;
        }

        /**
         * Initialisierungsmethode
         */
        private void init()
        {
            rightTime();
            calculate();
            createString();     // erzeuge den Code
        }

        /**
         * es wird fuer jeden Signaltyp die Intervallgrenzen bestimmt
         */
        private void rightTime()
        {
            switch (type)
            {
                case SignalTyp.KURZ:
                    begin = MainProgram.MINKURZTIME;
                    end = MainProgram.MAXKURZTIME;
                    break;
                case SignalTyp.MITTEL:
                    begin = MainProgram.MINMITTELTIME;
                    end = MainProgram.MAXMITTELTIME;
                    break;
                case SignalTyp.LANG:
                    begin = MainProgram.MINLANGTIME;
                    end = MainProgram.MAXLANGTIME;
                    break;
                case SignalTyp.NODATA:
                    begin = MainProgram.MINTIME;
                    end = MainProgram.MAXTIME;
                    break;
            }
        }

        /**
         * Berechnet die Anzahl der Einsen und Nullen
         */
        private void calculate()
        {
            iEins = time / 5;
            iNull = (end / 5) - (time / 5);
        }

        private void  createString()
        {
            char[] test = new char[(end / 5)];
            for (int i = 0; i < iEins; i++)
            {
                test[i] = '1';
            }

            for (int i = iEins; i < (end / 5); i++)
            {
                test[i] = '0';
            }

            cSignalCode = test;
            signalCode = new string(cSignalCode);
            // Ausgabe
            Debug.WriteLine("Ausgabe von caltulateString() Zeit : " + time + " berechnete einsen " + iEins + " daraus folgt "
                    + getEins() + " Code = ");
            Debug.WriteLine(signalCode);
            //return signalCode;
        }

        /**
         * erstellt den neuen String und gibt ihn Aus.
         */
        public String printString()
        {
            createString();
            return signalCode;
        }

        /**
         * Setze die Anzahl (n) an Nullen
         * @param n Anzahl Einsen die gesetzt werden soll
         */
        public void setEins(int n)
        {
            iEins = n;
        }

        /**
         * setzt die Anzahl (n) an Nullen
         * @param n Anzahl Nullen die gesetzt werden soll
         */
        public void setNull(int n)
        {
            iNull = n;
        }

        /**
         * setzt den Typen des Signals 
         * @param sType ist der Typ der gesetzt werden soll
         */
        public void setType(SignalTyp sType)
        {
            type = sType;
        }

        public void setRecognizeType(SignalTyp  recognizedTyp)
        {
            this.recognizeType = recognizedTyp;
        }

        /**
         * setzt die Zeit die benoetigt wurde um das Signal zu erkennen
         */
        public void setTimeToRecognizeType(long time)
        {
            this.timeToRecognizeType = time;
        }
        
        /**
         * setzt die Bewertung des Signals 
         * @param sRating ist der Rating-Wert vom Benutzer
         */
        public void setRating(SignalRating sRating)
        {
            this.rating = sRating;
        }

        /**
         * 
         */
        public void setTimeToRecognizeRating(long time)
        {
            this.timeToRecognizeRating = time;
        }

        /**
         * setzt den Typen 
         */
        public void setRecognizeStrength(SignalStrength strength)
        {
            this.strength = strength;
        }

        /**
         * 
         */
        public void setTimeToRecognizeStrength(long time)
        {
            this.timeToRecognizeStrength = time;
        }

        /**
         * gibt die Anzahl der Einsen an
         * @return
         */
        public int getEins()
        {
            return iEins;
        }

        /**
         * gibt die Anzahl der Nullen an 
         * @return
         */
        public int getNull()
        {
            return iNull;
        }

        /**
         * gibt den Typ des Signals zurueck
         * @return
         */
        public SignalTyp getType()
        {
            return type;
        }

        /**
         * gibt den Rating-Wert des Signals zurueck
         * @return
         */
        public SignalRating getRating()
        {
            return rating;
        }

        /**
         * gibt die Zeit des Signals von dem Signaltyp zurueck  
         * @return
         */
        public int getTime()
        {
            return time;
        }

        /**
         * Gibt die Mindestzeit des Intervalls des Signalstypen an
         * @return
         */
        public int minTime()
        {
            return begin;
        }

        /**
         * gibt den Endzeitpunkt des Intervalls dieses Signaltyps an
         * @return
         */
        public int maxTime()
        {
            return end;
        }

        // Anzahl Einsen
        /*public int countOnce() {
			//int temp = signalCode.length() - signalCode.replace("1", "").length();
			return iEins;
		}*/

        public override string ToString()
        {
            string str = "";

            str += "signalTyp, recognizedType, rating, strength, time, iNull, iEins, begin, end, timeToRecognizeType, timeToRecognizeRating, timeToRecognizeStrength" + Environment.NewLine;

            str += string.Format("{0},{1},{2},{3},", this.type.ToString("F"), this.recognizeType.ToString("F"), this.rating.ToString("F"), this.strength.ToString("F"));
            str += string.Format("{0},{1},", this.time, this.iNull);
            str += string.Format("{0},{1},{2},{3},", this.iEins, this.begin, this.end, this.timeToRecognizeType);
            str += string.Format("{0},{1}", this.timeToRecognizeRating, this.timeToRecognizeStrength) + Environment.NewLine;
            

            return str;
        }
    }
}
