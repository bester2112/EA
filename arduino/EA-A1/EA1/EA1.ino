
// include BlueTooth API 
#include <BLE_API.h>
#include "Adafruit_TLC59711.h"

//Definitionen 
#define DEVICE_NAME           "EA 3"    // Name des Gerätes
#define TXRX_BUF_LEN          20        // die maximale Länge sind 20 bytes die man über BLE senden kann

#define VIBRATION_LENGTH      200       // Vibration lengs (ms) - mode0 only
#define VIBRATION_STRENGTH    65535     // Vibrationsstärke für die Benutzung des TLC's
#define PAUSE_LENGTH          50        // Pausen Länge
#define MAX_BPS               10        // maximal mögliche BPS (Baud rate (bps) – Configures the UART baud rate.)
#define TIME_LENGTH           2000      // intervall länge
#define MAXSIGNALS_SEND       10

// Definiere Zustände
#define MODE_STANDBY          0x00      // standby
#define MODE_DEF              0x02      // Mode 0
#define MODE_ALT              0x02      // Mode 1
#define MODE_END_SIGNAL       0xFF      // Ende des Signals

#define DEBUG                 1         // DEBUG Mode
#define NUM_TLC59711          2         // Anzahl der TLC's

// PINS 
#define DATA_PIN              A4        // PIN für die Daten
#define LED_PIN               A3        // PIN für die LED
#define VCC_ON                D3        // muss sofort im setup() auf HIGH gesetzt werden

#define STUPID_EDGE_CASE     1
int internalIndex;

// Anlegen des Vibrationsmotors
Adafruit_TLC59711 tlc = Adafruit_TLC59711(NUM_TLC59711, LED_PIN, DATA_PIN);
const uint8_t actor[2] = {0, 12};       // actor channels 

// BLE Definition
BLE ble;

//////////////// TODO EIGENE CHARAKTERISICS ERSTELLEN

//static const uint8_t writeService_uuid[]      = {0x71, 0x3D, 0, 0, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};
//static const uint8_t   gatt_write_uuid[]      = {0x71, 0x3D, 0, 3, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};

//static const uint8_t writeModeService_uuid[]  = {0x81, 0x3D, 0, 0, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};
//static const uint8_t  gatt_write_mode_uuid[]  = {0x81, 0x3D, 0, 3, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};

// SELBST ERSTELLTE Charakteristics
static const uint8_t lengthService_uuid[]     = {0x71, 0x3D, 0, 0, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};
static const uint8_t service_tx_uuid[] = {0x71, 0x3D, 0, 3, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};

static const uint8_t modeService_uuid[] = {0x81, 0x3D, 0, 0, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};
static const uint8_t service_rx_uuid[]    = {0x81, 0x3D, 0, 3, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};

//uint8_t      write_value[TXRX_BUF_LEN]  = {0};
//uint8_t write_mode_value[TXRX_BUF_LEN]  = {0};
// Bedeutung von RX / TX 
// https://devzone.nordicsemi.com/question/56744/rx-and-tx-characteristics-in-ble_app_uart-example/
uint8_t tx_value[TXRX_BUF_LEN] = {0};
uint8_t rx_value[TXRX_BUF_LEN] = {0};


// Variablen
// vibration mode:
//    0 - STAND BY
//    1 - Beats per second
//    2 - ~ Pauses per second (README in development..)
byte mode;

boolean motorUp;      // true: top-actor; false: bot-actor
byte    currentBPS;
byte    nextBPS;
byte    currentSignal[TXRX_BUF_LEN];
byte    nextSignal[TXRX_BUF_LEN];
//                         Signal----  Pause-----  
byte    tempOfTesting[] = {0x14, 0x00, 0x24, 0x00, 
//                         Signal----  Pause-----
                           0x13, 0x00, 0x23, 0x00, 
//                         Signal----  Pause-----
                           0x12, 0x00, 0x22, 0x00, 
//                         Signal----  Pause-----
                           0x11, 0x00, 0x21, 0x00, 
//                         Signal----  Pause-----
                           0x14, 0x00, 0x24, 0x00};


int intervalLength;   // length of current interval
int   curVibLength;   // length of vib-signal (based on mode)
boolean newSignal;
boolean replay; 

int lengthOfSignal[MAXSIGNALS_SEND];
int signalType[MAXSIGNALS_SEND]; // 1 = Signal 2 = Pause

// service and characteristics
// Eine Characteristic namen "lengthCharacteristic"  erstellt 
GattCharacteristic  lengthCharacteristic(
                        service_tx_uuid, 
                        tx_value,
                        1,
                        TXRX_BUF_LEN,
                        GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_WRITE |
                        GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_WRITE_WITHOUT_RESPONSE );
// erstellen einer Liste von Characteristic mit dem "lengthCharacteristic"
GattCharacteristic *uartChars[] = {&lengthCharacteristic};
// erstellen eines Services 
GattService         lengthService(
                        lengthService_uuid,
                        uartChars,
                        sizeof(uartChars) / sizeof(GattCharacteristic *));

// erstellen einer Characteristic namens "modeCharacteristic"
GattCharacteristic  modeCharacteristic(
                        service_rx_uuid,
                        rx_value,
                        1,
                        TXRX_BUF_LEN,
                        GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_WRITE |
                        GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_WRITE_WITHOUT_RESPONSE );
// Liste einer Characteristic aus "modeCharacteristic" erstellt
GattCharacteristic *uartCharsMode[] = { &modeCharacteristic };
// erstellen eines Services
GattService         modeService(
                        modeService_uuid,
                        uartCharsMode,
                        sizeof(uartCharsMode) / sizeof(GattCharacteristic *));
///////////////


// NICHT UMBEDINGT NOTWENDIG; KANN AUCH WIEDER ENTFERNT WERDEN
/** @brief  Connection callback handle
 *
 *  @param[in] *params   params->handle : The ID for this connection
 *                       params->role : PERIPHERAL  = 0x1, // Peripheral Role
 *                                      CENTRAL     = 0x2, // Central Role.
 *                       params->peerAddrType : The peer's BLE address type
 *                       params->peerAddr : The peer's BLE address
 *                       params->ownAddrType : This device's BLE address type
 *                       params->ownAddr : This devices's BLE address
 *                       params->connectionParams->minConnectionInterval
 *                       params->connectionParams->maxConnectionInterval
 *                       params->connectionParams->slaveLatency
 *                       params->connectionParams->connectionSupervisionTimeout
 */
void connectionCallBack( const Gap::ConnectionCallbackParams_t *params ) {
  if (DEBUG) {
    Serial.println("BLE DEVICE is CONNECTED");
  }
}

/** @brief  Disconnect callback handle
 *
 *  @param[in] *params   params->handle : connect handle
 *                       params->reason : CONNECTION_TIMEOUT                          = 0x08,
 *                                        REMOTE_USER_TERMINATED_CONNECTION           = 0x13,
 *                                        REMOTE_DEV_TERMINATION_DUE_TO_LOW_RESOURCES = 0x14,  // Remote device terminated connection due to low resources.
 *                                        REMOTE_DEV_TERMINATION_DUE_TO_POWER_OFF     = 0x15,  // Remote device terminated connection due to power off.
 *                                        LOCAL_HOST_TERMINATED_CONNECTION            = 0x16,
 *                                        CONN_INTERVAL_UNACCEPTABLE                  = 0x3B,
 */
void disconnectionCallBack(const Gap::DisconnectionCallbackParams_t *params) {
  if (DEBUG) {
    Serial.println("BLE is Disconnected");
    Serial.println("BLE Restart advertising");
  }
  ble.startAdvertising();
}

// überschreiben von nachrichten im Buffer
/** @brief  write callback handle of Gatt server
 *
 *  @param[in] *Handler   Handler->connHandle : The handle of the connection that triggered the event
 *                        Handler->handle : Attribute Handle to which the write operation applies
 *                        Handler->writeOp : OP_INVALID               = 0x00,  // Invalid operation.
 *                                           OP_WRITE_REQ             = 0x01,  // Write request.
 *                                           OP_WRITE_CMD             = 0x02,  // Write command.
 *                                           OP_SIGN_WRITE_CMD        = 0x03,  // Signed write command.
 *                                           OP_PREP_WRITE_REQ        = 0x04,  // Prepare write request.
 *                                           OP_EXEC_WRITE_REQ_CANCEL = 0x05,  // Execute write request: cancel all prepared writes.
 *                                           OP_EXEC_WRITE_REQ_NOW    = 0x06,  // Execute write request: immediately execute all prepared writes.
 *                        Handler->offset : Offset for the write operation
 *                        Handler->len : Length (in bytes) of the data to write
 *                        Handler->data : Pointer to the data to write
 */
void gattServerWriteCallBack(const GattWriteCallbackParams *Handler) {
  uint8_t buf[TXRX_BUF_LEN];
  uint16_t bytesRead, index;

  // retrieving graph:
  //
  //    > byte array with values from [0x00,0x15]
  //
  //    > [0x00,0x09] ~ [-10,-1] || [0x0A,0x15] ~ [0-10]
  //
  //    > either 20 (TXRX_BUF_LEN) elements OR n elements + 0xFF <- EndOfGraph-code
  //
  if (DEBUG) {
    Serial.println("BLE onDataWritten : ");
  }

  // DONE EIGENE CHARACTERISTIK benutzen |
  //                                     v
  if (Handler->handle == lengthCharacteristic.getValueAttribute().getHandle()) {
    ble.readCharacteristicValue(lengthCharacteristic.getValueAttribute().getHandle(), buf, &bytesRead);

    if (DEBUG) {
      Serial.print("Gelesene Butes im ARRAY {");
      for(int index = 0; index < bytesRead; index++)
      {
        Serial.print(buf[index]);
        if (index != TXRX_BUF_LEN -1) {
          Serial.print(", ");
        }
      }
      Serial.println("}");

      memcpy(nextSignal, buf, TXRX_BUF_LEN * sizeof(byte));
      newSignal = true;
      Serial.println("the newSignal is now true");
    }
  }

  // get mode // TODO
  // DONE EIGENE CHARACTERISTIK benutzen |
  //                                     v
  if (Handler->handle == modeCharacteristic.getValueAttribute().getHandle()) {
    ble.readCharacteristicValue(modeCharacteristic.getValueAttribute().getHandle(), buf, &bytesRead);
    
    mode = buf[0];
    if (DEBUG) {
      Serial.print("mode = buf[0] (CONTENT) = ");
      Serial.println(mode);
    }

    replay = true;
    newSignal = true;
  } else {
    if (DEBUG) {
      Serial.print("I WAS NOT IN mode = buf[0] (CONTENT) = ");
    }
  }
}

// NICHT UMBEDINGT NOTWENDIG; KANN AUCH WIEDER ENTFERNT WERDEN
// WICHTIG WIRD BISHER NOCH NICHT BENTUTZT !!!!!!!!
void passkeyDisplayCallback(Gap::Handle_t handle, const SecurityManager::Passkey_t passkey)
{
    printf("Input passKey: ");
    for (unsigned i = 0; i < Gap::ADDR_LEN; i++) {
        printf("%c ", passkey[i]);
    }
    printf("\r\n");
}
// NICHT UMBEDINGT NOTWENDIG; KANN AUCH WIEDER ENTFERNT WERDEN
  // securitySetupCompletedCallback
  // Set up a callback for when the security setup procedure (key generation and exchange) for a link has completed.
  // This will be skipped for bonded devices. The callback is passed in the success/failure status of the security setup procedure.
static void securitySetupCompletedCallback(Gap::Handle_t handle, SecurityManager::SecurityCompletionStatus_t status)
{
  if (status == SecurityManager::SEC_STATUS_SUCCESS) {
    printf("Security success %d\r\n", status);
  } else {
    printf("Security failed %d\r\n", status);
  }
}

// Initialisierungsmethode für BLE
void initBLE() {
  ble.init();

  // BEDEUTUNG VON SMP http://www.fte.com/webhelp/bpa600/Content/Documentation/WhitePapers/BTLE/Pairing.htm
  // initialisierung des BLE SMP (Security Managers Protokoll) 
  ble.securityManager().init();

  // GAP 
  ble.onConnection(connectionCallBack);
  ble.onDisconnection(disconnectionCallBack);
  // GATT Server
  ble.onDataWritten(gattServerWriteCallBack);
  // GATT Client 
  ble.onSecuritySetupCompleted(securitySetupCompletedCallback);

  // Setup adv_data and srp_data
  ble.accumulateAdvertisingPayload(GapAdvertisingData::BREDR_NOT_SUPPORTED | GapAdvertisingData::LE_GENERAL_DISCOVERABLE);
  ble.accumulateAdvertisingPayload(GapAdvertisingData::COMPLETE_LOCAL_NAME,  (uint8_t *)DEVICE_NAME, sizeof(DEVICE_NAME));
  ble.accumulateAdvertisingPayload(GapAdvertisingData::SHORTENED_LOCAL_NAME, (uint8_t *)DEVICE_NAME, sizeof(DEVICE_NAME));

  // set adv_type
  ble.setAdvertisingType(GapAdvertisingParams::ADV_CONNECTABLE_UNDIRECTED);

  // add service 
  ble.addService(lengthService);
  ble.addService(modeService);

  // set device name 
  ble.setDeviceName((const uint8_t *)DEVICE_NAME);
  // set tx power,valid values are -40, -20, -16, -12, -8, -4, 0, 4
  ble.setTxPower(4);
  // set adv_interval, 100ms in multiples of 0.625ms.
  ble.setAdvertisingInterval(160);
  // set adv_timeout, in seconds
  ble.setAdvertisingTimeout(0);

  // start advertising
  ble.startAdvertising();

  if (DEBUG) {
    Serial.println("Start advertising");
  }
}

void startVibration() {
  for (int index = 0; index < NUM_TLC59711; index++) {
    uint8_t channel = actor[index];
    
    tlc.setPWM(channel, VIBRATION_STRENGTH);
    tlc.setPWM(channel + 1, VIBRATION_STRENGTH);
    tlc.setPWM(channel + 2, VIBRATION_STRENGTH);
  }
  tlc.write(); // WICHTIG: Zum Bus schreiben!
}

void stopVibration() {
  for (int index = 0; index < NUM_TLC59711; index++) {
    uint8_t channel = actor[index];
    uint16_t noVibrationStrengh = 0;
    
    tlc.setPWM(channel, noVibrationStrengh);
    tlc.setPWM(channel + 1, noVibrationStrengh);
    tlc.setPWM(channel + 2, noVibrationStrengh);
  }
  tlc.write(); // WICHTIG: Zum Bus schreiben!
}

boolean calculateSignalLength() {
  // nacheinander folgend, werden Signale gesendet die maximale Länge ist 0x0400 also 1024 
  // um zu spezifizieren, ob es ein Signal oder eine Pause ist, 
  // sendet man am Anfang eine 1 also 0x1400 für Signal (4096)
  // und Am Anfang eine 2 also 0x2400 für eine Pause (8192)
  // Es muss zuerst ein Signal gefolgt von einer Pause gesendet werden. 
  // Es können mittels 20 Bytes somit 5 Signale hintereinander gesendet werden.
  //int[] lengthOfSignal;
  //int[] signalType; // 1 = Signal 2 = Pause
  int index = 0;
  boolean res = true;
  
  for (int i = 0; i < TXRX_BUF_LEN; i += 2) {
    if (DEBUG) {
      Serial.print("index = ");
      Serial.println(i);
    
      Serial.print("currentSignal[");
      Serial.print(i);
      Serial.print("] = ");
      Serial.println(currentSignal[i]);

      Serial.print("currentSignal[");
    }
    int iN = i + 1;
    if (DEBUG) {
      Serial.print(iN);
      Serial.print("] = ");
    }
    int cS = currentSignal[iN];
    if (DEBUG) {
      Serial.println(currentSignal[iN]);
    }

    int temp = (currentSignal[i] * 256); 

    if (DEBUG) {
      Serial.print("currentSignal[");
      Serial.print(i);
      Serial.print("] * 256 = ");
      Serial.println(temp);
    }
    int temp2 = (temp + cS);

    if (DEBUG) {
      Serial.print("currentSignal[");
      Serial.print(i);
      Serial.print("] * 256 + currentSignal[");
      Serial.print(iN);
      Serial.print("] = ");
      Serial.print(currentSignal[i]);
      Serial.print(" + ");
      Serial.print(cS);
      Serial.print(" = ");
      Serial.println(temp2);
    }
    
    int res1 = temp2 % 4096;

    if (DEBUG) {
      Serial.print("temp2 % 4096 = ");
      Serial.print(temp);
      Serial.print(" % 4096 = ");
      Serial.println(res1);
    }
    
    lengthOfSignal[index] = res1;
    signalType[index] = lengthOfSignal[index] / 4096; // ergebniss ist 1 fuer Signal oder 2 fuer Pause
    index++;

    if (DEBUG) {
      Serial.print("lengthOfSignal[");
      Serial.print(i);
      Serial.print("] = ");
      Serial.println(lengthOfSignal[i]);

      Serial.print("signalType[");
      Serial.print(i);
      Serial.print("] = ");
      Serial.println(signalType[i]);
    }
  }

  for (int n = 0; n < TXRX_BUF_LEN; n++) {
    if (n % 2 == 0) { // signale
      if (signalType[n] == 2) { // falls hier eine Pause drinnen ist 
         res = false;
      }
    } else if (n % 2 == 1) { // pausen 
      if (signalType[n] == 1) { // falls hier ein Signal drinnen ist
        res = false;
      }
    }
  }

  return res;
}

void playSignal() { // TODO 
  if (DEBUG) {
      Serial.print("Vibration Time = ");
      Serial.println(lengthOfSignal[internalIndex]);
  }
  startVibration();
  delay(lengthOfSignal[internalIndex]);
  internalIndex++; // Signal wurde abgespielt, index muss jetzt hochgezaehlt werden
  if (DEBUG) {
      Serial.print("Pause Time = ");
      Serial.println(lengthOfSignal[internalIndex]);
  }
  stopVibration();
  delay(lengthOfSignal[internalIndex]);
  internalIndex++; // Pause wurde abgespielt index muss jetzt hochgezaehlt werden
}

void run() {
  // mache erst was, wenn sich das Gerät nicht mehr im Standby befindet

  if (newSignal == true) {
  //if (mode != MODE_STANDBY) {
    memcpy(currentSignal, nextSignal, TXRX_BUF_LEN * sizeof(byte));

    internalIndex = 0;
    
    if (DEBUG) {
      Serial.print("RUN: ");
    }

    if(STUPID_EDGE_CASE){
      for (int i = 0; i < TXRX_BUF_LEN; i++) {
        currentSignal[i] = tempOfTesting[i];
        if (DEBUG) {
          Serial.print("temp[");
          Serial.print(i);
          Serial.print("] = ");
          Serial.println(tempOfTesting[i]);
        
          Serial.print("curr[");
          Serial.print(i);
          Serial.print("] = ");
          Serial.println(currentSignal[i]);
        }
      }
    }
    if (!replay) {
      boolean calculateOK = calculateSignalLength();
      if (calculateOK) {
      
        Serial.println(" berechnung war erfolgreich ");
      } else {
        Serial.println(" Die Reihenfolge von den Signalen war nicht einwandfrei => Fehler in Calculate Funktion ");
      }
    } else {
      replay = false; 
    }

    for(int index = 0; index < MAXSIGNALS_SEND / 2; index++)
    {
      if (DEBUG) {
        Serial.print("currentSignal [");
        Serial.print(index);
        Serial.print("] = ");
        Serial.println(currentSignal[index]);
      }
      if (currentSignal[index] == MODE_END_SIGNAL) {
        break;
      }
      
      /*if (mode == MODE_STANDBY) {
        break;
      }*/
      Serial.println("after break");
      playSignal();
      Serial.println("I was in the playSignal()");
    }
    /*if (DEBUG) {
      Serial.println("RUN: TEST OUTPUT ");
      Serial.print("MODE_STANDBY = ");
      Serial.print(MODE_STANDBY);
      Serial.print("DEBUG = ");
      Serial.print(DEBUG);
      Serial.print("MODE_DEF = ");
      Serial.print(MODE_DEF);
      Serial.print("MODE_ALT = ");
      Serial.print(MODE_ALT);
      Serial.print("MODE_END_SIGNAL = ");
      Serial.print(MODE_END_SIGNAL);
      Serial.println("");
      
      //MODE_STANDBY          0x00
      //DEBUG                 0x01
      //MODE_DEF              0x02
      //MODE_ALT              0x02
      //MODE_END_SIGNAL       0xFF
    }*/

    newSignal = false;
    //mode = MODE_STANDBY;
  }
}

void setup() {
  // put your setup code here, to run once:
  if (DEBUG) {
    Serial.begin(9600);
  }
  pinMode(VCC_ON, OUTPUT);    // activating
  digitalWrite(VCC_ON, HIGH); // board

  // initialisierung der Variablen
  // ...
  newSignal = false;
  replay = false;

  // TLC 
  tlc.begin();
  tlc.write();

  // BLE
  initBLE();
  if (DEBUG) {
    Serial.print("BLE is ON");
  }
}

void loop() {
  // put your main code here, to run repeatedly:
  run();
  //ble.waitForEvent();
}


