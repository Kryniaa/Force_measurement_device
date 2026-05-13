#include "force_sensor.h"
#include "i2c_mux.h"

//Adres I2C czujnika siły.
#define SENSOR_ADDRESS (0x28 << 1)

// Globalna tablica przechowująca 10 gotowych, przeliczonych odczytów siły
uint16_t sensor_data[10];

// Zewnętrzny uchwyt do magistrali I2C (zdefiniowany domyślnie przez CubeMX w main.c)
extern I2C_HandleTypeDef hi2c1;

/**
 * @brief Odczytuje dane ze wszystkich 10 czujników siły podłączonych przez dwa multipleksery.
 * Wyniki są dekodowane i zapisywane do globalnej tablicy sensor_data.
 * W przypadku awarii czujnika lub błędu komunikacji I2C, w tablicy zapisywana jest wartość 0xFFFF.
 */
void Read_All_Force_Sensors(void) {
    uint8_t current_mux_address;
    uint8_t current_channel;
    uint8_t raw_data[2]; // Bufor na 2 bajty surowych danych odzyskiwanych z czujnika

    // Główna pętla skanująca wszystkie 10 czujników (indeksy 0-9)
    for (int i = 0; i <= 9; i++) {

        // Czujniki 0-7 są fizycznie podłączone do pierwszego MUX, a 8-9 do drugiego.
        if (i < 8){
            current_mux_address = MUX1_ADDRESS;
            current_channel = i;
        } else {
            current_mux_address = MUX2_ADDRESS;
            current_channel = i - 8; // Reset indeksu kanału dla drugiego MUXa (kanały 0 i 1)
        }

        // Przełączenie sprzętowe aktywnego kanału na właściwym układzie
        Channel_Choice(current_mux_address, current_channel);

        // Wysłanie "pustego" żądania bez czytania danych, co wybudza czujnik z trybu uśpienia.
        uint8_t dummy;
        HAL_I2C_Master_Receive(&hi2c1, SENSOR_ADDRESS, &dummy, 1, 10);

        // Oczekujemy 10 ms na wykonanie konwersji analogowo-cyfrowej przez układ czujnika
        HAL_Delay(10);

        // Pytamy czujnik o 2 bajty. Przetwarzamy dane tylko, jeśli operacja zwróci status HAL_OK (sukces).
        if(HAL_I2C_Master_Receive(&hi2c1, SENSOR_ADDRESS, raw_data, 2, 10) == HAL_OK){

            // Wycięcie dwóch najstarszych bitów (S1, S0) z pierwszego bajtu za pomocą masek.
            uint8_t status = (raw_data[0] >> 6) & 0x03;

            // Wartość 0x00 oznacza "Normal Operation" (pomiar jest ważny i bezbłędny)
            if (status == 0x00){

                // 1. Zabezpieczamy niższe 6 bitów pierwszego bajtu (maska 0x3F).
                // 2. Przesuwamy je o 8 pozycji w lewo (robiąc miejsce dla drugiego bajtu).
                // 3. Łączymy (Suma logiczna OR) z drugim bajtem, uzyskując pełną 16-bitową zmienną.
                uint16_t force_counts = ((raw_data[0] & 0x3F) << 8) | raw_data[1];

                // Zapis czystego wyniku pomiaru
                sensor_data[i] = force_counts;
            } else {
                // Obsługa błędu sprzętowego czujnika (np. status "Stale Data" lub "Fault")
                sensor_data[i] = 0xFFFF;
            }
        } else {
            // Obsługa błędu komunikacji I2C (np. przerwany przewód, brak zasilania MUX)
            sensor_data[i] = 0xFFFF;
        }
    }
}
