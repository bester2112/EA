#include <Adafruit_TLC59711.h>
#define DEBUG // for Serial output

////////////////////////////////////////////
// SPI

#define DATA_PIN A4
#define CLOCK_PIN A3

#define NUM_TLC59711 19
#define OUTPUTS_PER_BOARD 12
#define VIB_DELAY_MS 70

Adafruit_TLC59711 tlc = Adafruit_TLC59711(NUM_TLC59711, CLOCK_PIN, DATA_PIN);

void setup() {
  pinMode(DATA_PIN, OUTPUT);
  pinMode(CLOCK_PIN, OUTPUT);

  pinMode(D0, OUTPUT);
  digitalWrite(D0, HIGH);
  
  tlc.begin();
  tlc.write();

}

//rm led mm lm

void allOnForBoard(int board) {
  
  for (int i = 0; i < 1; i++){
    uint8_t channel = i * 12;

    uint16_t duty_cycle = 0;
    
    if (board == i) {
      duty_cycle = 65535; 
    }
    
    for (int output = 0; output < OUTPUTS_PER_BOARD; output++) {
      tlc.setPWM(channel + output, duty_cycle);
    }
  }
  
  tlc.write();
}

void loop() {
  for(int i = 0; i < NUM_TLC59711; i++){
    allOnForBoard(i);
    delay(VIB_DELAY_MS);
  }
}





