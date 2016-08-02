/*
  C. Thomas Brittain
  Lumi Unit Test Peripheral
  Written for the ATtiny85.

*/
#include <SoftwareSerial.h>

// PB4 = 4, TX
// PB3 = 3, RX

SoftwareSerial mySerial(3, 4); // RX, TX

void setup() {
  // set the data rate for the SoftwareSerial port
  mySerial.begin(9600);  
}

void loop() { // run over and over
  if (mySerial.available()) {
    mySerial.write(mySerial.read());
  }
}
