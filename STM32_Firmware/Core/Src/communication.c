#include "communication.h"
#include "ux_api.h"
#include "ux_device_class_cdc_acm.h"
#include "app_usbx_device.h"
#include "string.h"
#include "stdio.h"

extern UX_SLAVE_CLASS_CDC_ACM  *cdc_acm_instance;

/**
 * @brief Wysyła ramkę identyfikacyjną urządzenia przez port szeregowy USB (CDC ACM).
 * Funkcja pozwala aplikacji na komputerze PC potwierdzić, 
 * że połączyła się z właściwym sterownikiem czujników siły.
 */
void USB_Send_Handshake(void) {
    // Sprawdzenie, czy instancja USB została poprawnie zainicjalizowana.
    if (cdc_acm_instance != NULL) {

        // Unikalne ID
        uint8_t msg[] = "ID:FORCE_MEASUREMENT_DEVICE_V1\r\n";

        // Liczba bajtów, którą rzeczywiście udało się wysłać do bufora.
        ULONG actual_length;

        // Wywołanie funkcji zapisu USBX.
        _ux_device_class_cdc_acm_write(cdc_acm_instance, msg, strlen((char*)msg), &actual_length);
    }
}