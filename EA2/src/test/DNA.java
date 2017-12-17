package test;

import java.util.Random;
import java.util.Scanner;

public class DNA {
	
	private Signal signal;
	private int input;
	private int inputType;
	private double fitness;
	
	public DNA(Signal signal) {
		this.signal = signal;
	}
	
	public void calculateSignalType() {
		// benutzer erfragen
		chooseType();
		validateType();
	}
	
	public void chooseType() {
		System.out.println("-------------------------------------------");
		System.out.println("Das Signal ist : " );
		signal.printString();
		System.out.println("Bitte bewerten Sie das Signal \n\t 1 für kurz \n\t 2 für mittel \n\t 3 für lang");
		
		Scanner reader = new Scanner(System.in); 
		System.out.print("Ihre Eingabe : ");
		int n = reader.nextInt();
		
		inputType = n;
	}
	
	public void validateType() {
		switch (inputType) {
		case 1: // kurz
			signal.setType(SignalTyp.KURZ);
		break;
		case 2: // mittel
			signal.setType(SignalTyp.MITTEL);
		break;
		case 3:	// lang
			signal.setType(SignalTyp.LANG);
		break;
		default:
			System.out.println("ERROR in der validateType Funktion");
		break;
		}
	}
	
	// die Fitness Funktion wird von dem Probanden evaluieren lassen.
	public void fitness() {
		// Aufrufen der Benutzer Eingabe 
		userInput();
		calculateFitness();
	}
	
	public void userInput() {
		String stype = " UNINIZIALISIERT ";
		switch (signal.getType()){
			case KURZ:
				stype = "Kurz";
			break;
			case MITTEL:
				stype = "Mittel";
			break;
			case LANG:
				stype = "Lang";
			break;

			default:
				System.out.println("ERROR in der userInput Funktion");
			break;
		}

		System.out.println("-------------------- " );
		System.out.println("Das Signal ist : " );
		signal.printString();		
		System.out.println("Das folgende Signal ist " + stype + ".");
		System.out.println("Bewerten Sie bitte mit den Zahlen \n\t1 gar nicht \n\t2 schlecht \n\t3 ok / geht so \n\t4 gut \n\t5 sehr gut");
		Scanner reader = new Scanner(System.in); 
		System.out.print("Ihre Eingabe : ");
		int n = reader.nextInt();
		
		input = n;
	}
	
	public void calculateFitness() {
		switch (input) {
		case 1: 	// Eingabe wurde 'gar nicht' erkannt
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
			System.out.println("ERROR in der calculateFitness Funktion");
		break;
		}
	}
	
	
	// TODO durchgehen, was passiert bei ungeraden zahlen, also 151 oder 154 ms
	// crossover erzeugt ein neues Kind aus den beiden Eltern
	public DNA crosover(DNA partner) {
		Signal pSignal = partner.getSignal();
		int time = -1;
		
		int temp = Math.abs(signal.getEins() - pSignal.getEins());
		int res = temp / 2;
		if (signal.getEins() > pSignal.getEins()) {
			time = pSignal.getTime() + (res * 5);
		} else {
			time = signal.getTime() + (res * 5);
		}
		
		Signal childSignal = new Signal(signal.getType(), time);
		DNA child = new DNA(childSignal);
		
		return child;
	}
	
	// 
	public void mutate(int mutationRate) {
		int min = 0; // das ist fest, es beginn immer bei 0
		int max = 99;
		
		Random r = new Random();
		int resEins = r.nextInt((max - min) + 1) + min;
		int resNull = r.nextInt((max - min) + 1) + min;
		System.out.print("RANDOM 0 : " + resNull);
		System.out.print(" RANDOM 1 : " + resEins);
		System.out.println();
		
		if ((resNull < mutationRate) && (resEins < mutationRate)) {
			// die Stelle 0 und die Stelle 1 war zufall, das beide geändert werden sollten
			//  tt
			// 1100 => 1010 X geht nicht, erweitere dann einfach eine 0 zur 1 
			signal.setEins(signal.getEins() + 1);
			signal.setNull(signal.getNull() - 1);
			signal.printString();
		} else {
			// nur eine stelle wollte gewechselt werden
			if (resNull < mutationRate) { //  1100  => 1110
				signal.setEins(signal.getEins() + 1);
				signal.setNull(signal.getNull() - 1);
			}
			if (resEins < mutationRate) { //  1100  => 1000
				signal.setEins(signal.getEins() - 1);
				signal.setNull(signal.getNull() + 1);
			}
			signal.printString();
		}
	}
	
	public Signal getSignal() {
		return signal;
	}
	
	public double getFitness() {
		return fitness;
	}
	
	/*public int getInputType() {
		return inputType;
	}*/
}
