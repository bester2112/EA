package test;

/**
 * Die Klasse Signal repräsentiert ein Signal das nur aus drei Signalen zur verfügung Steht: 
 * kurz, mittel und lang
 * Jedes Signal hat eine mindest und maximal länge
 * @author thomas
 */
enum SignalTyp {
	KURZ,
	MITTEL,
	LANG,
	NODATA
}

public class Signal {
	
	private SignalTyp type;
	private int time;		  	// Zeit in ms
	private String signalCode;	// 
	private char[] cSignalCode;	// 
	private int iNull;			// Anzahl der Nullen
	private int iEins;			// Anzahl der Einsen
	private int begin;	  	 	// MinimalZeit für den Typ von Signal
	private int end;			  	// MaximalZeit für den Typ von Signal
	
	/**
	 * erzeugt ein Signal, dass mit der genannten Zeit
	 * @param sTime Zeit
	 */
	public Signal(int sTime) {
		type = SignalTyp.NODATA;
		time = sTime;
		init();
	}
	
	/**
	 * Konstruktor des Signals
	 * @param sType Signaltyp 
	 * @param sTime Zeit, die sich in dem Intervall des Signaltyps befinden soll
	 */
	public Signal(SignalTyp sType, int sTime) {
		type = sType; 
		time = sTime;
		//cSignalCode = new char [(end/5)];
		init();
	}
	
	/**
	 * Initialisierungsmethode
	 */
	private void init() {
		rightTime();
		calculate();
		createString(); 	// erzeuge den Code
	}
	
	/**
	 * es wird für jeden Signaltyp die Intervallgrenzen bestimmt
	 */
	private void rightTime() {
		switch (type) {
			case KURZ:
				begin = Main.MINKURZTIME;
				end   = Main.MAXKURZTIME;
			break;
			case MITTEL:
				begin = Main.MINMITTELTIME;
				end   = Main.MAXMITTELTIME;
			break;
			case LANG:
				begin = Main.MINLANGTIME;
				end   = Main.MAXLANGTIME;
			case NODATA:
				begin = Main.MINTIME;
				end   = Main.MAXTIME; 
			break;
		}
	}

	/**
	 * Berechnet die Anzahl der Einsen und Nullen
	 */
	private void calculate() {
		iEins = time / 5;
		iNull = (end / 5) - (time / 5);
	}

	private void createString() {
		char [] test = new char[(end/5)];
		for (int i = 0; i < iEins; i++) {
			test[i] = '1';
		}
		
		for (int i = iEins; i < (end/5); i++) {
			test[i] = '0';
		}
		
		cSignalCode = test;
		signalCode = String.copyValueOf(cSignalCode);
		// Ausgabe
		System.out.println("Ausgabe von caltulateString() Zeit : " + time + " berechnete einsen " + iEins + " daraus folgt "
				+ getEins() + " Code = ");
		System.out.println(signalCode);
	}
	
	/**
	 * erstellt den neuen String und gibt ihn Aus.
	 */
	public void printString() {
		createString(); 
	}
	
	/**
	 * Setze die Anzahl (n) an Nullen
	 * @param n Anzahl Einsen die gesetzt werden soll
	 */
	public void setEins(int n) {
		iEins = n;
	}
	
	/**
	 * setzt die Anzahl (n) an Nullen
	 * @param n Anzahl Nullen die gesetzt werden soll
	 */
	public void setNull(int n) {
		iNull = n;
	}
	
	/**
	 * setzt den Typen des Signals 
	 * @param sType ist der Typ der gesetzt werden soll
	 */
	public void setType(SignalTyp sType) {
		type = sType;
	}
	
	/**
	 * gibt die Anzahl der Einsen an
	 * @return
	 */
	public int getEins() {
		return iEins;
	}
	
	/**
	 * gibt die Anzahl der Nullen an 
	 * @return
	 */
	public int getNull() {
		return iNull;
	}
	
	/**
	 * gibt den Typ des Signals zurück
	 * @return
	 */
	public SignalTyp getType() {
		return type;
	}
	
	/**
	 * gibt die Zeit des Signals von dem Signaltyp zurück  
	 * @return
	 */
	public int getTime() {
		return time;
	}
	
	/**
	 * Gibt die Mindestzeit des Intervalls des Signalstypen an
	 * @return
	 */
	public int minTime() {
		return begin;
	}
	
	/**
	 * gibt den Endzeitpunkt des Intervalls dieses Signaltyps an
	 * @return
	 */
	public int maxTime() {
		return end;
	}
	
	// Anzahl Einsen
		/*public int countOnce() {
			//int temp = signalCode.length() - signalCode.replace("1", "").length();
			return iEins;
		}*/
}