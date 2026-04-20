#ifndef COMMUNICATION_H
#define COMMUNICATION_H

#include <stdint.h>
#include <stdio.h>

void USB_Send_Handshake(void);
void USB_Send_SensorData(uint16_t *data, uint8_t count);

#endif /* COMMUNICATION_H */