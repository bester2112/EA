package test;

import java.util.ArrayList;
import java.util.Random;

public class Population {
	
	private DNA[] population;	
	private int numOfPopulation;
	
	private ArrayList<DNA> poolK;								// Starte mit einem leeren pool
	private ArrayList<DNA> poolM;								// 
	private ArrayList<DNA> poolL;
	
	private ArrayList<DNA> pool;
	
	private int[] arithmetikMedian;								// beinhaltet das arithmetische mittel von allen Kurzen (index 0), allen mittleren (index 1) und allen langen (index 2)
	private int[] zones;											// beinhaltet die intervall grenzen in der folgenden Reihenfolge [minK, maxK, minM, macM, minL, maxL]	
	
	/**
	 * Erstellt eine Population, bei dem die Elemente gleich verteilt sind
	 * @param n anzahl der Elemente
	 * @param b 
	 */
	public Population(int n, boolean b) {
		
		numOfPopulation = n;
		population = new DNA[numOfPopulation];
		pool = new ArrayList<DNA>();
		Signal s = null;
		
		int iX = Main.MAXTIME / numOfPopulation;
		for (int i = 1; i <= numOfPopulation; i++) {
			System.out.println(" ----  i = " + i);
			s = new Signal(iX * i);
			population[i - 1] = new DNA(s);
		}

		ArrayList array = new ArrayList();
		int index = 0;
		do {
			// gehe das Array zufällig durch und frage nach (daher die grenzen 0 bis n-1)
			int i = getRandom(0, (numOfPopulation - 1));
			if (!array.contains(i)) {
				array.add(i);
				population[i].calculateSignalType();
				index++;
			}
		} while (index != numOfPopulation);

		if (!calculateZone()) {
			// erneut benutzer abfragen
		}
	}
	
	public Population(int n, int[] x) {
		arithmetikMedian = new int[3]; 
		zones = new int[6]; 
		
		numOfPopulation = n;
		population = new DNA[numOfPopulation];
		pool = new ArrayList<DNA>();
		Signal s = null;
		
		int iX = Main.MAXTIME / numOfPopulation;
		for (int i = 1; i <= numOfPopulation; i++) {
			System.out.println(" ----  i = " + i);
			s = new Signal(iX * i);
			population[i - 1] = new DNA(s);
		}

		ArrayList array = new ArrayList();
		int index = 0;
		do {
			// gehe das Array zufällig durch und frage nach (daher die grenzen 0 bis n-1)
			int i = getRandom(0, (numOfPopulation - 1));
			if (!array.contains(i)) {
				array.add(i);
				//population[i].calculateSignalType();
				population[i].setInputType(x[i]);
				population[i].validateType();
				index++;
			}
		} while (index != numOfPopulation);

		if (!calculateZone()) {
			// erneut benutzer abfragen
		}
		calculateArithmeticMedian();
		calculateNewZones();
	}
	
	/**
	 * erstelle eine Population mit n Signalen für jeden Signaltypen
	 * @param n Anzahl zufälliger Signale
	 */
	public Population(int n) {
		numOfPopulation = n * 3;
		population = new DNA[numOfPopulation];
		poolK = new ArrayList<DNA>();
		poolM = new ArrayList<DNA>();
		poolL = new ArrayList<DNA>();
		
		int signalIndex = -1;
		for (int i = 0; i < numOfPopulation; i++) {
			Signal s = null;
			System.out.println("i = " + i + "signalIndex = " + signalIndex);
			if (i % (numOfPopulation / 3) == 0) {
				signalIndex++;
			}
			
			switch (signalIndex) {
				case 0:
					s = new Signal(SignalTyp.KURZ, getRandom(Main.MINKURZTIME, Main.MAXKURZTIME));
				break;
				case 1:
					s = new Signal(SignalTyp.MITTEL, getRandom(Main.MINMITTELTIME, Main.MAXMITTELTIME));
				break;
				case 2:
					s = new Signal(SignalTyp.LANG, getRandom(Main.MINLANGTIME, Main.MAXLANGTIME));
				break;
				default: 
					System.out.println("ERROR in Population Konstruktor");
				break;
			}
			
			
			population[i] = new DNA(s);
		}
		
		calculate();
	}
	
	/**
	 * berechnet die Grenzen der Signaltypen
	 */
	public boolean calculateZone() {
		boolean res = false;
		int minK = 0;
		int maxK = 0;
		int minM = 0;
		int maxM = 0;
		int minL = 0;
		int maxL = 0;
		
		
		for (int i = 0; i < numOfPopulation; i++) {
			Signal s = population[i].getSignal();
			int time = s.getTime();
			switch (s.getType()) {
			case KURZ: // kurz
				if (minK == 0) {
					minK = time;
				}
				if (maxK < time) {
					maxK = time;
					if ((minM < time) && (maxM < time)) {
						minM = time;
						maxM = time;
						if ((minL < time) && (maxL < time)) {
							minL = time;
							maxL = time;
						}
					}
				}
			break;
			case MITTEL: // mittel
				if (maxM < time) {
					maxM = time;
					if ((minL < time) && (maxL < time)) {
						minL = time;
						maxL = time;
					}
				}
				if (minM <= maxK) {
					minM = maxM;
				}
			break;
			case LANG:	// lang
				if (maxL < time) {
					maxL = time;
				}
				if (minL <= maxM) {
					minL = time;
				}
 			break;
			default:
				System.out.println("ERROR in der calculateZone Funktion");
			break;
			}
		}
		if ((minK < maxK) && (maxK < minM) && (minM < maxM) && (maxM < minL) && (minL < maxL)) {
			res = true;
			System.out.println(minK);
			System.out.println(maxK);
			System.out.println(minM);
			System.out.println(maxM);
			System.out.println(minL);
			System.out.println(maxL);
			zones[0] = minK;
			zones[1] = maxK;
			zones[2] = minM;
			zones[3] = maxM;
			zones[4] = minL;
			zones[5] = maxL;
		}
		
		return res;
	}
	
	public void calculateArithmeticMedian() {
		int midK = 0;
		int indexK = 0;
		int midM = 0;
		int indexM = 0;
		int midL = 0;
		int indexL = 0;
		
		for (int i = 0; i < numOfPopulation; i++) {
			Signal s = population[i].getSignal();
			switch (s.getType()) {
			case KURZ:
				midK = midK + s.getTime();
				indexK++;
			break;
			case MITTEL:
				midM = midM + s.getTime();
				indexM++;
			break;
			case LANG:
				midL = midL + s.getTime();
				indexL++;
			break;
			default:
				System.out.println("ERROR in der calculateArithmeticMedian Funktion");
				break;
			}
		}
		
		arithmetikMedian[0] = midK / indexK;
		arithmetikMedian[1] = midM / indexM;
		arithmetikMedian[2] = midL / indexL;
	}
	
	public void calculateNewZones() {
		// die minimale und maximale grenze von einem Signal kann nur 50 / 1000 sein, daher dürfen diese nicht weiter angepasst werden
		// daher wird zuerst das kurzen und danach das Lange Signal so angepasst, dass der median ungefähr passt
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
		
		if (medK > arithmetikMedian[0]) {
			// berechne eine neue obere grenze für K
			newMaxK = (arithmetikMedian[0] * 2) - minK;
		}
		
		if (medL > arithmetikMedian[2]) {
			// bereche eine neue untere grenze für L 
			newMinL = (arithmetikMedian[2] * 2) - maxL;
		}
		
		if (maxK > newMaxK) {
			maxK = newMaxK;
			minM = newMaxK + 50; // plus mindestabstand
		}
		
		if (minL > newMinL) {
			minL = newMinL;
			maxM = newMinL - 50; // minus mindestandtand
		}		
		
		/*int indexLeft = minM;
		int indexRight = maxM;
		int diff = 0;
		
		int minDiff = 1000;
		ArrayList bestLeft = new ArrayList();
		ArrayList bestRight = new ArrayList();
		ArrayList bestMid = new ArrayList();
		int arrayIndex = 0;
		
		medM = (indexLeft + indexRight) / 2;
		do {
			do {
				if (medM > arithmetikMedian[1]) {
					// TODO berechne besten Median
					// berechne kleinsten abstand zum median
					diff = medM - arithmetikMedian[1];
					if (diff < minDiff) {
						minDiff = diff;
						bestLeft.add(indexLeft);
						bestRight.add(indexRight);
						bestMid.add(minDiff);
						arrayIndex++;
					}
				}
				indexLeft++;
				medM = (indexLeft + indexRight) / 2;
			} while (indexLeft <= arithmetikMedian[1]);
			indexRight--;
			if (indexLeft >= arithmetikMedian[1]) {
				indexLeft = minM;
			}
		} while (indexRight >= arithmetikMedian[1]);
		
		for (int i = 0; i < bestLeft.size(); i++) {
			System.out.print(i + ". Best Left  => " + bestLeft.get(i));
			System.out.print("\t Best Right  => " + bestRight.get(i));
			System.out.println("\t Best Mid  => " + bestMid.get(i));
		}*/
	}
	
	public void calculate() {
		calculateFitness();
		selection();
		generate(poolK, 0);
		generate(poolM, 1);
		generate(poolL, 2);
		print();
	}
	
	private void calculateFitness() {
		for (int i = 0; i < numOfPopulation; i++) {
			population[i].fitness();
		}
	}
	
	/**
	 * erzeugt eine Population also einen pool von Signalen
	 */
	public void selection() {
		poolK.clear();
		poolM.clear();
		poolL.clear();
		
		// Bestimme die maximale Population von der population für ein Signal
		double maxFitness[] = new double[3];
		int index = -1;
		for (int i = 0; i < numOfPopulation; i++) {
			if (i % (numOfPopulation / 3) == 0) {
				index++;
			}
			if (population[i].getFitness() > maxFitness[index]) {
				maxFitness[index] = population[i].getFitness();
			}
			
	    	}
		
		// Basierend auf der Fitness, jedes Element der Population wird eine bestimmte Anzahl an malen in den pool hinzugefügt
		// eine hohe fitness = mehr einträge im pool = größere Wahrscheinlichkeit als Eltern ausgewählt zu werden
		// eine kleine Fitness = weniger Einträge in pool = kleinere Wahrscheinlichkeit, als Eltern ausgewählt zu werden
		
		index = -1;
		
	    	for (int i = 0; i < numOfPopulation; i++) {
	    		
	    		if (i % (numOfPopulation / 3) == 0) {
				index++;
			}
		    
	    		// berechne das intervall zwischen dem 0 und der maximalen Fitness auf das Interwall zwischen 0 und 1
	    		double fitness = map(population[i].getFitness(),0,maxFitness[index],0,1);
	    		int n = (int) (fitness * 100);  			// wandle das ergebnis zwischen 0 und 100 statt 0 und 1
	    		switch(index) {
	    		case 0:
	    			for (int j = 0; j < n; j++) {            // füge die Anzahl der Fitness oft der population in den pool herein
	    				poolK.add(population[i]);
	    			}
	    		break;
	    		case 1:
	    			for (int j = 0; j < n; j++) {            // füge die Anzahl der Fitness oft der population in den pool herein
	    				poolM.add(population[i]);
	    			}
	    		break;
	    		case 2:
	    			for (int j = 0; j < n; j++) {            // füge die Anzahl der Fitness oft der population in den pool herein
	    				poolL.add(population[i]);
	    			}
	    		break;
	    		default:
	    			System.out.println("ERROR !!!!! 1234");
	    		break;
	    		}
	    	} 
	}
	
	public double map(double n, double start1, double stop1, double start2, double stop2) {
		double newval = (n - start1) / (stop1 - start1) * (stop2 - start2) + start2;
		if (start2 < stop2) {
			return constrain(newval, start2, stop2);
		} else {
			return constrain(newval, stop2, start2);
		}
	}
	
	private double constrain(double n, double low, double high) {
		return Math.max(Math.min(n, high), low);
	}
	
	/**
	 * erzeuge die neue Gernation
	 */
	public void generate(ArrayList<DNA> pool, int index) {
		int min = index * (numOfPopulation / 3);
		int max = (index + 1) * (numOfPopulation / 3);
		for (int i = min; i < max; i++) {
			int randA;
			int randB;
			do {
				randA = getRandom(0, (pool.size()-1));
				randB = getRandom(0, (pool.size()-1));
			} while (randA == randB);
			
			DNA parentA = pool.get(randA);
			DNA parentB = pool.get(randB);
			
			DNA kind = parentA.crosover(parentB);
			kind.mutate(Main.MUTATIONRATE);
			population[i] = kind;
		}
	}
	
	public void print() {
		int signalIndex = -1;
		for (int i = 0; i < numOfPopulation; i++) {
			System.out.println("i = " + i + " signalIndex = " + signalIndex);
			if (i % (numOfPopulation / 3) == 0) {
				signalIndex++;
			}
			
			System.out.println("--------------------------");
			switch (signalIndex) {
				case 0:
					System.out.println("KURZ");
					population[i].getSignal().printString();
					System.out.println("Population INDEX = " + i + " Fitness " + population[i].getFitness());
				break;
				case 1:
					System.out.println("Mittel");
					population[i].getSignal().printString();
					System.out.println("Population INDEX = " + i + " Fitness " + population[i].getFitness());
				break;
				case 2:
					System.out.println("LANG");
					population[i].getSignal().printString();
					System.out.println("Population INDEX = " + i + " Fitness " + population[i].getFitness());
				break;
				default: 
					System.out.println("ERROR in Population Konstruktor");
				break;
			}
		}
	}
	
	private int getRandom(int min, int max) {
		Random r = new Random();
		int res = r.nextInt((max - min) + 1) + min;
		return res;
	}
}
