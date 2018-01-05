
// Sketch for the "Teco_Env" sensor module with temp/hum/press, triple gas and dust sensor
//
// by Iris Mehrbrodt

#include <BLE_API.h>
#include <Wire.h>
#include "MutichannelGasSensor.h" // include des Gassensors
//if this doesnt compile due to missing SPI.h, use the custom
//bme library from the repo
#include "SparkFunBME280.h"

#define DEVICE_NAME       "Teco_Env" // Name des Gerates
#define TIMEOUT           120 //after this number of ticks the connection is closed
//pins for dust sensor
#define MEASURE_PIN       A4 // Dust PIN
#define LED_PIN           A3 // LED PIN

//pulsing times for dust sensor
//dont touch except you know what you are doing
int samplingTime = 280;
int deltaTime = 40;
int sleepTime = 9680;

// BLE, TIcker und timecounter
BLE                       ble;
Ticker                    ticker;
unsigned int timeoutCounter = 0;

BME280 bme;

// Initialisierung der UUID's 
const char* gas_service_uuid = "4b822f90-3941-4a4b-a3cc-b2602ffe0d00";
const char* co_uuid = "4b822fa1-3941-4a4b-a3cc-b2602ffe0d00";
const char* no2_uuid = "4b822f91-3941-4a4b-a3cc-b2602ffe0d00";
const char* nh3_uuid = "4b822fb1-3941-4a4b-a3cc-b2602ffe0d00";

const char* co_r0_uuid = "4b822fa2-3941-4a4b-a3cc-b2602ffe0d00";
const char* no2_r0_uuid = "4b822f92-3941-4a4b-a3cc-b2602ffe0d00";
const char* nh3_r0_uuid = "4b822fb2-3941-4a4b-a3cc-b2602ffe0d00";

const char* dust_service_uuid = "4b822fe0-3941-4a4b-a3cc-b2602ffe0d00";
const char* dust_uuid = "4b822fe1-3941-4a4b-a3cc-b2602ffe0d00";

// UUID LISTE 
static const uint16_t uuid16_list[] = {GattService::UUID_ENVIRONMENTAL_SERVICE};

// Variablen Deklarieren und Initialisieren
uint16_t co_value = 0;
uint16_t no2_value = 0;
uint16_t nh3_value = 0;

uint16_t co_r0_value = 0;
uint16_t no2_r0_value = 0;
uint16_t nh3_r0_value = 0;

uint16_t dust_value = 0;

int16_t temp_value = 0;
uint16_t hum_value = 0;
uint32_t press_value = 0;

// erstellen der Characteristiken und serives 
// Create characteristic and service

// co, no2 unc nh3 Characteristik erstellen
GattCharacteristic co_char(
                     UUID(co_uuid), 
                     (uint8_t*) &co_value, 
                     sizeof(co_value), 
                     sizeof(co_value), 
                     GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_NOTIFY);
                     
GattCharacteristic no2_char(UUID(no2_uuid), (uint8_t*) &no2_value, sizeof(no2_value), sizeof(no2_value), GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_NOTIFY);
GattCharacteristic nh3_char(UUID(nh3_uuid), (uint8_t*) &nh3_value, sizeof(nh3_value), sizeof(nh3_value), GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_NOTIFY);

// co, no2 unc nh3 r0 Characteristik erstellen
GattCharacteristic co_r0_char(UUID(co_r0_uuid), (uint8_t*) &co_r0_value, sizeof(co_r0_value), sizeof(co_r0_value), GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_NOTIFY);
GattCharacteristic no2_r0_char(UUID(no2_r0_uuid), (uint8_t*) &no2_r0_value, sizeof(no2_r0_value), sizeof(no2_r0_value), GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_NOTIFY);
GattCharacteristic nh3_r0_char(UUID(nh3_r0_uuid), (uint8_t*) &nh3_r0_value, sizeof(nh3_r0_value), sizeof(nh3_r0_value), GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_NOTIFY);

// eine Gas Charasteric erstellen, sowie noch einen Service 
GattCharacteristic *gasList[] = {&co_char, &no2_char, &nh3_char, &co_r0_char, &no2_r0_char, &nh3_r0_char };
GattService        gasService(
                      UUID(gas_service_uuid), 
                      gasList, 
                      sizeof(gasList) / sizeof(GattCharacteristic *));

// Dust Characteristic und Liste von dust Characteristic und auch noch einen Service
GattCharacteristic dust_char(UUID(dust_uuid), (uint8_t*) &dust_value, sizeof(dust_value), sizeof(dust_value), GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_NOTIFY);
GattCharacteristic *dustList[] = { &dust_char };
GattService        dustService(UUID(dust_service_uuid), dustList, sizeof(dustList) / sizeof(GattCharacteristic *));

// erstellen einer der Temp, hum, press und Liste von den Elementen als Chraracteristic 
// aus der liste wurde ein Serice erstellt
GattCharacteristic temp_char(GattCharacteristic::UUID_TEMPERATURE_CHAR, (uint8_t*)&temp_value, sizeof(temp_value), sizeof(temp_value), GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_NOTIFY);
GattCharacteristic hum_char(GattCharacteristic::UUID_HUMIDITY_CHAR, (uint8_t*)&hum_value, sizeof(hum_value), sizeof(hum_value), GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_NOTIFY);
GattCharacteristic press_char(GattCharacteristic::UUID_PRESSURE_CHAR, (uint8_t*)&press_value, sizeof(press_value), sizeof(press_value), GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_NOTIFY);
GattCharacteristic *envCharList[] = { &temp_char, &hum_char, &press_char };
GattService envService(GattService::UUID_ENVIRONMENTAL_SERVICE, envCharList, sizeof(envCharList) / sizeof(GattCharacteristic *));


void disconnectionCallBack(const Gap::DisconnectionCallbackParams_t *params) {
  Serial.println("Disconnected!");
  Serial.println("Restarting the advertising process");
  ble.startAdvertising();
}

void periodicCallback() {
  if (ble.getGapState().connected) {

    //automated timeout: kick every connection that lasts longer than TIMEOUT ticks
    if(timeoutCounter >= TIMEOUT){
      Serial.println("Timeout. Disconnecting...");
      ble.disconnect(Gap::CONNECTION_TIMEOUT);
      timeoutCounter = 0; 
      return;
    }
    timeoutCounter++;
    
    
    //calculated gas values for debugging
    //like i would directly send these values lol
    float c = gas.measure_CO(); 
    Serial.print("CO: "); 
    Serial.print(c);
    c = gas.measure_NO2();
    Serial.print(", NO2: "); 
    Serial.print(c); 
    c = gas.measure_NH3(); 
    Serial.print(", NH3: "); 
    Serial.print(c);

    //raw gas values
    gas.readR();
    gas.readR0();

    //software communication error handling: if potential co_value is 0 (this occured),
    //assume the reading was not successful and abort
    if(gas.res[1] == 0){
      Serial.println("Communication error");
      return;
    }

    //set gas values
    co_value = gas.res[1];
    no2_value = gas.res[2];
    nh3_value = gas.res[0];   

    co_r0_value = gas.res0[1];
    no2_r0_value = gas.res0[2];
    nh3_r0_value = gas.res0[0];


    Serial.print(", co raw: ");
    Serial.print(co_value);
    Serial.print(", co r0: ");
    Serial.print(co_r0_value);

    Serial.print(", no2 raw: ");
    Serial.print(no2_value);
    Serial.print(", no2 r0: ");
    Serial.print(no2_r0_value);

    Serial.print(", nh3 raw: ");
    Serial.print(nh3_value);
    Serial.print(", nh3 r0: ");
    Serial.print(nh3_r0_value);

    //take one dust sensor reading
    digitalWrite(LED_PIN,LOW); // power on the LED
    delayMicroseconds(samplingTime);
   
    dust_value = analogRead(MEASURE_PIN); // read the dust value
   
    delayMicroseconds(deltaTime);
    digitalWrite(LED_PIN,HIGH); // turn the LED off
    delayMicroseconds(sleepTime);

    //read data from bme
    float temp = bme.readTempC();
    Serial.print(", Temp: ");
    Serial.print(temp);
    temp_value = int16_t(temp * 100);

    float hum = bme.readFloatHumidity();
    Serial.print(", Hum: ");
    Serial.print(hum);
    hum_value = uint16_t(hum * 100);

    float pressure = bme.readFloatPressure();
    Serial.print(", Press: ");
    Serial.println(pressure);
    press_value = uint32_t(pressure * 10);

    //write new values to characteristics
    ble.updateCharacteristicValue(co_char.getValueAttribute().getHandle(), (uint8_t*) &co_value, sizeof(co_value));
    ble.updateCharacteristicValue(no2_char.getValueAttribute().getHandle(), (uint8_t*) &no2_value, sizeof(no2_value));
    ble.updateCharacteristicValue(nh3_char.getValueAttribute().getHandle(), (uint8_t*) &nh3_value, sizeof(nh3_value));

    ble.updateCharacteristicValue(co_r0_char.getValueAttribute().getHandle(), (uint8_t*) &co_r0_value, sizeof(co_r0_value));
    ble.updateCharacteristicValue(no2_r0_char.getValueAttribute().getHandle(), (uint8_t*) &no2_r0_value, sizeof(no2_r0_value));
    ble.updateCharacteristicValue(nh3_r0_char.getValueAttribute().getHandle(), (uint8_t*) &nh3_r0_value, sizeof(nh3_r0_value));

    ble.updateCharacteristicValue(dust_char.getValueAttribute().getHandle(), (uint8_t*) &dust_value, sizeof(dust_value));

    ble.updateCharacteristicValue(temp_char.getValueAttribute().getHandle(), (uint8_t*)&temp_value, sizeof(temp_value));
    ble.updateCharacteristicValue(hum_char.getValueAttribute().getHandle(), (uint8_t*)&hum_value, sizeof(hum_value));
    ble.updateCharacteristicValue(press_char.getValueAttribute().getHandle(), (uint8_t*)&press_value, sizeof(press_value));
  }
}

void setup() {
  Serial.begin(9600);
  Serial.println("Serial ok");

  // Gas senssor initialisieren 
  //init gas sensor
  gas.begin(0x04);
  gas.powerOn();      // aufruf der powerOn Methode aus MultichannelGasSensor.h/cpp

  Serial.println("Gas Sensor ok");

  // Pin als Output erzeugt.
  //init pins for dust sensor
  pinMode(LED_PIN, OUTPUT);

  Serial.println("Dust Sensor ok... I guess");

  // initialisierung vom BME
  //init bme
  bme.settings.commInterface = I2C_MODE;
  bme.settings.I2CAddress = 0x77;
  bme.settings.runMode = 3; //  3, Normal mode
  bme.settings.tStandby = 0; //  0, 0.5ms
  bme.settings.filter = 0; //  0, filter off

  bme.settings.tempOverSample = 1;
  bme.settings.pressOverSample = 1;
  bme.settings.humidOverSample = 1;
  delay(10);
  bme.begin();

  Serial.println("BME ok");

  // Initialisierung für den Timer
  // Init timer task
  ticker.attach(periodicCallback, 1);

  // Initialisierung des BLES
  // Init ble
  ble.init();
  ble.onDisconnection(disconnectionCallBack);

  // setup adv_data and srp_data
  ble.accumulateAdvertisingPayload(GapAdvertisingData::BREDR_NOT_SUPPORTED | GapAdvertisingData::LE_GENERAL_DISCOVERABLE);
  ble.accumulateAdvertisingPayload(GapAdvertisingData::COMPLETE_LOCAL_NAME, (uint8_t *)DEVICE_NAME, sizeof(DEVICE_NAME));
  ble.accumulateAdvertisingPayload(GapAdvertisingData::SHORTENED_LOCAL_NAME, (uint8_t *)DEVICE_NAME, sizeof(DEVICE_NAME));
  ble.accumulateAdvertisingPayload(GapAdvertisingData::COMPLETE_LIST_16BIT_SERVICE_IDS, (uint8_t*)uuid16_list, sizeof(uuid16_list));

  // set adv_type
  ble.setAdvertisingType(GapAdvertisingParams::ADV_CONNECTABLE_UNDIRECTED);
  
  // add service 
  ble.addService(gasService);
  ble.addService(dustService);
  ble.addService(envService);

  // set device name
  ble.setDeviceName((const uint8_t *)DEVICE_NAME);
  // set tx power,valid values are -40, -20, -16, -12, -8, -4, 0, 4
  ble.setTxPower(4);
  // set adv_interval, 100ms in multiples of 0.625ms.
  ble.setAdvertisingInterval(1600);
  // set adv_timeout, in seconds
  ble.setAdvertisingTimeout(0);

  // start advertising
  ble.startAdvertising();
}

void loop() {
  // put your main code here, to run repeatedly:
  ble.waitForEvent();
}
