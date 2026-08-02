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
    uint8_t raw_data[2];

    for (int i = 0; i <= 9; i++) {
        if (i < 8){
            current_mux_address = MUX1_ADDRESS;
            current_channel = i;
        } else {
            current_mux_address = MUX2_ADDRESS;
            current_channel = i - 8;
        }

        Channel_Choice(current_mux_address, current_channel);

        uint8_t dummy;
        HAL_I2C_Master_Receive(&hi2c1, SENSOR_ADDRESS, &dummy, 1, 10);

        HAL_Delay(10);

        if(HAL_I2C_Master_Receive(&hi2c1, SENSOR_ADDRESS, raw_data, 2, 10) == HAL_OK){
            uint8_t status = (raw_data[0] >> 6) & 0x03;
            if (status == 0x00){
                uint16_t force_counts = ((raw_data[0] & 0x3F) << 8) | raw_data[1];
                sensor_data[i] = force_counts;
            } else {
                sensor_data[i] = 0xFFFF;
            }
        } else {
            sensor_data[i] = 0xFFFF;
        }
    }
}
