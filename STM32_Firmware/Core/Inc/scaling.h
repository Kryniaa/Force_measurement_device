#ifndef SCALING_H
#define SCALING_H

#define NUM_SENSORS 10
#define SAMPLES_PER_SENSOR 10

#include <stdint.h>
#include "stm32h5xx_hal.h"


void Readings_scaling(uint16_t sensor_data10[10][10], uint16_t output[10]);
void Tare_sensors(void);

#endif /* SCALING_H */