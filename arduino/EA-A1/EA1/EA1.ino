
// include BlueTooth API 
#include <BLE_API.h>
#include "Adafruit_TLC59711.h"

//Definitionen 
#define DEVICE_NAME           "EA 3"    // Name des Gerätes
#define TXRX_BUF_LEN          20        // die maximale Länge sind 20 bytes die man über BLE senden kann


#define VIBRATION_LENGTH      100       // Vibration lengs (ms) - mode0 only
#define VIBRATION_STRENGTH    65535     // Vibrationsstärke für die Benutzung des TLC's
#define PAUSE_LENGTH          50        // Pausen Länge
#define MAX_BPS               10        // maximal mögliche BPS (Baud rate (bps) – Configures the UART baud rate.)
#define TIME_LENGTH           2000      // intervall länge

// Definiere Zustände
#define MODE_STANDBY          0x00      // standby
#define MODE_DEBUG            0x01      // DEBUG Mode
#define MODE_DEF              0x02      // Mode 0
#define MODE_ALT              0x02      // Mode 1
#define MODE_END_SIGNAL       0xFF      // Ende des Signals

#define NUM_TLC59711          2         // Anzahl der TLC's

// PINS 
#define DATA_PIN              A4        // PIN für die Daten
#define LED_PIN               A3        // PIN für die LED
#define VCC_ON                D3        // muss sofort im setup() auf HIGH gesetzt werden

// Anlegen des Vibrationsmotors
Adafruit_TLC59711 tlc = Adafruit_TLC59711(NUM_TLC59711, LED_PIN, DATA_PIN);
const uint8_t actor[2] = {0, 12};

// BLE Definition
BLE ble;

//////////////// TODO EIGENE CHARAKTERISICS ERSTELLEN

//static const uint8_t writeService_uuid[]      = {0x71, 0x3D, 0, 0, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};
//static const uint8_t   gatt_write_uuid[]      = {0x71, 0x3D, 0, 3, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};

//static const uint8_t writeModeService_uuid[]  = {0x81, 0x3D, 0, 0, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};
//static const uint8_t  gatt_write_mode_uuid[]  = {0x81, 0x3D, 0, 3, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};

// SELBST ERSTELLTE Charakteristics
static const uint8_t myService_uuid[]     = {0x71, 0x3D, 0, 0, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};
static const uint8_t service_tx_uuid[] = {0x71, 0x3D, 0, 3, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};

static const uint8_t modeService_uuid[] = {0x81, 0x3D, 0, 0, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};
static const uint8_t service_rx_uuid[]    = {0x81, 0x3D, 0, 3, 0x50, 0x3E, 0x4C, 0x75, 0xBA, 0x94, 0x31, 0x48, 0xF1, 0x8D, 0x94, 0x1E};

//uint8_t      write_value[TXRX_BUF_LEN]  = {0};
//uint8_t write_mode_value[TXRX_BUF_LEN]  = {0};
// BEdeutung von RX / TX 
// https://devzone.nordicsemi.com/question/56744/rx-and-tx-characteristics-in-ble_app_uart-example/
uint8_t tx_value[TXRX_BUF_LEN] = {0};
uint8_t rx_value[TXRX_BUF_LEN] = {0};


// vibration mode:
//    0 - STAND BY
//    1 - Beats per second
//    2 - ~ Pauses per second (README in development..)
byte mode;

// service and characteristics
// Eine Characteristic namen "myCharacteristic"  erstellt 
GattCharacteristic  myCharacteristic(
                        service_tx_uuid, 
                        tx_value,
                        1,
                        TXRX_BUF_LEN,
                        GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_WRITE |
                        GattCharacteristic::BLE_GATT_CHAR_PROPERTIES_WRITE_WITHOUT_RESPONSE );
// erstellen einer Liste von Characteristic mit dem "myCharacteristic"
GattCharacteristic *uartChars[] = {&myCharacteristic};
// erstellen eines Services 
GattService         myService(
                        myService_uuid,
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
  if (MODE_DEBUG) {
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
  if (MODE_DEBUG) {
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
  if (MODE_DEBUG) {
    Serial.println("BLE onDataWritten : ");
  }

  // DONE EIGENE CHARACTERISTIK benutzen |
  //                                     v
  if (Handler->handle == myCharacteristic.getValueAttribute().getHandle()) {
    ble.readCharacteristicValue(myCharacteristic.getValueAttribute().getHandle(), buf, &bytesRead);

    if (MODE_DEBUG) {
      Serial.print("Gelesene Butes im ARRAY {");
      for(int index=0; index<bytesRead; index++)
      {
        Serial.print(buf[index]);
        if (index != TXRX_BUF_LEN -1) {
          Serial.print(", ");
        }
      }
      Serial.println("}");

    // TODO
    //     memcpy(nextGraph, buf, TXRX_BUF_LEN * sizeof(byte));
    }
    
    // get mode // TODO
    // DONE EIGENE CHARACTERISTIK benutzen |
    //                                     v
    if (Handler->handle == modeCharacteristic.getValueAttribute().getHandle()) {
      ble.readCharacteristicValue(modeCharacteristic.getValueAttribute().getHandle(), buf, &bytesRead);
    
      mode = buf[0];
      if (MODE_DEBUG) {
        Serial.print("mode = buf[0] (CONTENT) = ");
        Serial.println(mode);
      }
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
  ble.addService(myService);
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

  if (MODE_DEBUG) {
    Serial.println("Start advertising");
  }
}


void setup() {
  // put your setup code here, to run once:
  if (MODE_DEBUG) {
    Serial.begin(9600);
  }
  pinMode(VCC_ON, OUTPUT);    // activating
  digitalWrite(VCC_ON, HIGH); // board

  // initialisierung der Variablen
  // ...

  // TLC 
  tlc.begin();
  tlc.write();

  // BLE
  initBLE();
  if (MODE_DEBUG) {
    Serial.print("BLE is ON");
  }
}

void loop() {
  // put your main code here, to run repeatedly:
  ble.waitForEvent();
}


