#include "i2c_mux.h"
extern I2C_HandleTypeDef hi2c1;

void Channel_Choice(uint8_t mux_address, uint8_t channel_nr){
    uint8_t data = 1 << channel_nr;

    HAL_I2C_Master_Transmit(&hi2c1, mux_address, &data, 1, 100);
}