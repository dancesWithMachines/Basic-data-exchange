/***************************************************************************
  This is a library for the BMP280 humidity, temperature & pressure sensor

  Designed specifically to work with the Adafruit BMEP280 Breakout
  ----> http://www.adafruit.com/products/2651

  These sensors use I2C or SPI to communicate, 2 or 4 pins are required
  to interface.

  Adafruit invests time and resources providing this open source code,
  please support Adafruit andopen-source hardware by purchasing products
  from Adafruit!

  Written by Limor Fried & Kevin Townsend for Adafruit Industries.
  BSD license, all text above must be included in any redistribution
 ***************************************************************************/
/*
Author: Timax (Mateusz Kusiak)
Date of making: 30.11.2019
Licence: Do whateva u want boi/gurl, have fun
You might need to dovnload BMP280 Adafruit library
*/
 
#include <Wire.h>
#include <SPI.h>
#include <Adafruit_BMP280.h>
#include <ctype.h>

#define BMP_SCK  (13)
#define BMP_MISO (12)
#define BMP_MOSI (11)
#define BMP_CS   (10)

Adafruit_BMP280 bmp;
const int pResistor = A0;

float intensivity;
String serialInput;
boolean isOn=false;
boolean isSetting=false;
float pressure = 1017;

void(* resetFunc) (void) = 0; //declare reset function @ address 0

void setup() {
  Serial.begin(9600);
  bmp.begin(0x76);
  
  pinMode(LED_BUILTIN, OUTPUT);
  digitalWrite(LED_BUILTIN, LOW);
  
  pinMode(pResistor, INPUT);
  
  /* Default settings from datasheet. */
  bmp.setSampling(Adafruit_BMP280::MODE_NORMAL,     /* Operating Mode. */
                  Adafruit_BMP280::SAMPLING_X2,     /* Temp. oversampling */
                  Adafruit_BMP280::SAMPLING_X16,    /* Pressure oversampling */
                  Adafruit_BMP280::FILTER_X16,      /* Filtering. */
                  Adafruit_BMP280::STANDBY_MS_500); /* Standby time. */
                  
  Serial.println("Mini weather station. Made by Timax (M.Kusiak). Type in 'enable' to enable device.");
}

void loop() {
  
  if (Serial.available() > 0){
    serialInput = Serial.readString();
    serialInput.trim();
  }
  
  if (serialInput=="ENABLE"||serialInput=="enable"){
    isOn=true;
    digitalWrite(LED_BUILTIN, HIGH);
    Serial.println("!Device is now enabled! Type in commands, remember to keep the same letter size.");
    Serial.println("<'GET'-Get weatcher info><'SETPRESSURE'-Set se lvl pressure><'DISABLE'-Disable device> ");
  }
  
  if (serialInput=="DISABLE"||serialInput=="disable"){
    isOn=false;
    //Serial.println("Device is now disabled");  <--For sum funkin reason this shajt can't be there
    //digitalWrite(LED_BUILTIN, LOW);  <--unnecessary
    resetFunc();
  }

  if (isSetting==true && isOn==true && serialInput!="")
  {
    //Serial.println(serialInput.toFloat());
    if (serialInput.toFloat() > 0)
      pressure=serialInput.toFloat();
    else
      Serial.print("Unable to set. "); 
    Serial.print("Pressure is set to ");
    Serial.print(pressure);
    Serial.println("HPa");
    isSetting=false;
  }

  if (serialInput=="setPressure"||serialInput=="setpressure"||serialInput=="SETPRESSURE" && isOn==true){
    isSetting=true;
    Serial.println("Type in your sea level pressure (in HPa)");
  }
    
  if (serialInput=="GET"||serialInput=="get" && isOn==true){
    Serial.print(F("Temperature:"));
    Serial.print(bmp.readTemperature());
    Serial.println("*C; ");

    Serial.print(F("Pressure:"));
    Serial.print(bmp.readPressure());
    Serial.println("Pa; ");

    Serial.print(F("ApproximateAltitude:"));
    Serial.print(bmp.readAltitude(pressure)); /* Adjusted to local forecast! */
    Serial.println("m.n.p.m; ");

    Serial.print("Light:");
    intensivity=(float)analogRead(pResistor)/804;
    Serial.print(intensivity*100);
    Serial.println("%;");
  }
  
  serialInput = "";

}
