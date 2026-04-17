#ifndef FORCE_SENSOR_H
#define FORCE_SENSOR_H

#include <stdint.h>
#include "stm32h5xx_hal.h"

extern uint16_t sensor_data[10];

void Read_All_Force_Sensors(void);

#endif /* FORCE_SENSOR_H */