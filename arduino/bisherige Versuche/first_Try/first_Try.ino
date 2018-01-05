#include <Adafruit_TLC59711.h> // wird benötigt für die Vibrationen

#define DEBUG // für Serielle Ausgabe


// Each TLC59711 can control 8 LEDs, so the numbers should be between 0 and 7.
// Once you have set the color of each of the LEDs that are to be set, you need to write the data to the chip.
// The example does that. Your code does not.

//----------------------------------
// notwendige PINs für die Programmierung der Vibratoren

#define NUM_TLC59711 1  // number of chained TLCs

// Vibrator 1
#define dataPin1  A4
#define clockPin  A3
Adafruit_TLC59711 tlc1 = Adafruit_TLC59711(NUM_TLC59711, clockPin, dataPin1);

// Vibrator 2 
#define dataPin2  D2
#define clockPin2 D0
Adafruit_TLC59711 tlc2 = Adafruit_TLC59711(NUM_TLC59711, clockPin, dataPin2);
//----------------------------------

uint16_t wait_time = 2500;

void Terminal(String text) {
  Serial.println(text);
  //Serial.println(" ");
}

void debugSetup() {
  #ifdef DEBUG
  Serial.begin(9600);
  Terminal("Start");
  #endif
}

void setup() {
  debugSetup();

  // Start TLC 
  tlc1.begin();
  //tlc2.begin();
  
}

// Slightly different, this makes the rainbow equally distributed throughout
void rainbowCycle(uint8_t wait) {
  uint32_t i, j;

  for(j=0; j<65535; j+=10) { // 1 cycle of all colors on wheel
    for(i=0; i < 4*NUM_TLC59711; i++) { // zwischen 0 und 3 * NUM (hier ist NUM = 1)
      Wheel(i, ((i * 65535 / (4*NUM_TLC59711)) + j) & 65535);
    }
    tlc.write();
    delay(wait);
  }
}

// Input a value 0 to 4095 to get a color value.
// The colours are a transition r - g - b - back to r.
void Wheel(uint8_t ledn, uint16_t WheelPos) {               // LED ist nur von 0 bis 3 
  if(WheelPos < 21845) {                                    // von 0 bis 1/3 
    tlc.setLED(ledn, 3*WheelPos, 65535 - 3*WheelPos, 0);    // 3*WheelPos kann max 3/3 sein
  } else if(WheelPos < 43690) {                             // von 1/3 bis 2/3 
    WheelPos -= 21845;                                      // Auf den Wert zwischen 0 und 1/3 ändern
    tlc.setLED(ledn, 65535 - 3*WheelPos, 0, 3*WheelPos);    
  } else {                                                  // sonst, also höher 
    WheelPos -= 43690;                                      // Auf den Wert zwischen 0 und 1/3 ändern
    tlc.setLED(ledn, 0, 3*WheelPos, 65535 - 3*WheelPos);
  }
}

// Fill the dots one after the other with a color
void color(uint16_t r, uint16_t g, uint16_t b) {
  for(uint16_t i=0; i<4; i++) { 
      Terminal("" + i);
      tlc1.setLED(i, r, g, b);
      
      delay(wait_time);
  }
}

int i = 0;
void loop() {
   if (i == 0) {
     Terminal("alles 0");
     color(0, 0, 0);  
     delay(wait_time);

     Terminal("R an");
     color(65535, 0, 0); 
     delay(wait_time);

     Terminal("G an");
     color(0, 65535, 0); 
     delay(wait_time);

     Terminal("B an");
     color(0, 0, 65535); 
     delay(wait_time);

     i++;
   }
}
