package test;

import java.util.ArrayList;
import java.util.Random;

public class Population {
	
	private DNA[] population;	
	private int numOfPopulation;
	
	private ArrayList<DNA> poolK;									// Starte mit einem leeren pool
	private ArrayList<DNA> poolM;
	private ArrayList<DNA> poolL;
	
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
