#ifndef I2C_MUX_H
#define I2C_MUX_H

#include <stdint.h>
#include "stm32h5xx_hal.h"

#define MUX1_ADDRESS  (0x70 << 1)
#define MUX2_ADDRESS  (0x71 << 1)

void Channel_Choice(uint8_t mux_address, uint8_t channel_nr);

#endif /* I2C_MUX_H */