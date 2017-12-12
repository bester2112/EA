package test;

public class Main {
	
	final static int MUTATIONRATE = 5; // angabe in Prozent 5 ist das zwischen 0 und 4 also 5 % die wahrscheinlichkeit liegt, dass eine Zufällige Zahl bestimmt wird
	
	//final static int ; 
	
	final static int MINKURZTIME = 100;
	final static int MAXKURZTIME = 300;
	
	final static int MINMITTELTIME = 400;
	final static int MAXMITTELTIME = 500;
	
	final static int MINLANGTIME = 600;
	final static int MAXLANGTIME = 700;

	public static void main(String[] args) {
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
		
		
		System.out.println("child von k1 und k2");
		dk1.crosover(dk2);
		System.out.println("child von k1 und k3");
		dk1.crosover(dk3);
		
		System.out.println("child von m1 und m2");
		dm1.crosover(dm2);
		System.out.println("child von m1 und m3");
		dm1.crosover(dm3);
		
		System.out.println("child von l1 und l2");
		dl1.crosover(dl2);
		System.out.println("child von l1 und l3");
		dl1.crosover(dl3);
		
		
		for (int i = 0; i < 100; i++) {
			dk2.mutate(MUTATIONRATE);
		}*/
		
		Population p = new Population(10);
		
		for (int i = 0; i < 10; i++) {
			p.calculate();
		}
		
		/*int min = 0;
		int max = 99;
		for (int i = 0; i < 100; i++) {
			Random r = new Random();
			int res = r.nextInt((max - min) + 1) + min;
			System.out.print("RANDOM : " + res);
			if (res < 5) {
				System.out.print(" ______TRUEEEEEEEE______");
			}
			System.out.println();
		}*/
	}
	
	
}
