#ifndef COMMUNICATION_H
#define COMMUNICATION_H

void USB_Send_Handshake(void);
void USB_Send_SensorData(float *data, int count);

#endif /* COMMUNICATION_H */