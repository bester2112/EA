package readFile;

import java.io.BufferedReader;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;

public class ReadPart1 {

	public ReadPart1() {
		
	}
	
	public void readFilePart() {
		FileReader fr = null;
		try {
			fr = new FileReader("C:\\Users\\Enchis\\Desktop\\studie\\ewifhieehisefh-Kopie.txt");
		} catch (FileNotFoundException e) {
			e.printStackTrace();
		}
		if (fr != null) {
		    BufferedReader br = new BufferedReader(fr);
		    try {
		    	while(br.ready()) {
		    		String zeile = br.readLine();
		    		String nextA[] = zeile.split("!");
		    		String nextH[] = zeile.split("#");
		    		if (nextA.length > 1 && (zeile.charAt(0) == '!'))
		    			System.out.printf("!: '" + nextA[1] + "'\n");
		    		if (nextH.length > 1 && (zeile.charAt(0) == '#'))
		    			System.out.printf("#: '" + nextH[1] + "'\n");
		    		/*if (zeile.charAt(0) == '+') {
			    		String nextP[] = zeile.split("+");
		    			System.out.printf("+: '" + nextP[1] + "'\n");	
		    		}	*/	    		
		    	}
			} catch (IOException e) {
				e.printStackTrace();
			}
		}

	}
}
