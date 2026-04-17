#include "force_sensor.h"
#include "i2c_mux.h"

#define SENSOR_ADDRESS (0x28 << 1)

uint16_t sensor_data[10];

extern I2C_HandleTypeDef hi2c1;

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
        HAL_I2C_IsDeviceReady(&hi2c1, SENSOR_ADDRESS, 1, 10);
        
        HAL_Delay(5);

        if(HAL_))
    }
    





    uint8_t status = (raw_data[0] >> 6) & 0x03;
    if (status == 0x00){
        uint16_t force_counts = ((raw_data[0] & 0x3F) << 8) | raw_data[1];
    }
}