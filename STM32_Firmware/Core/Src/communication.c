#include "communication.h"
#include "ux_api.h"
#include "ux_device_class_cdc_acm.h"
#include "app_usbx_device.h"
#include "string.h"
#include "stdio.h"

extern UX_SLAVE_CLASS_CDC_ACM  *cdc_acm_handle;

/**
 * @brief Wysyła ramkę identyfikacyjną urządzenia przez port szeregowy USB (CDC ACM).
 * Funkcja pozwala aplikacji na komputerze PC potwierdzić,
 * że połączyła się z właściwym sterownikiem czujników siły.
 */
void USB_Send_Handshake(void) {
    // Sprawdzenie, czy instancja USB została poprawnie zainicjalizowana.
    if (cdc_acm_handle != NULL) {

        // Unikalne ID
        uint8_t msg[] = "ID:FORCE_MEASUREMENT_DEVICE_V1\r\n";

        // Liczba bajtów, którą rzeczywiście udało się wysłać do bufora.
        ULONG actual_length;

        // Wywołanie funkcji zapisu USBX.
        _ux_device_class_cdc_acm_write(cdc_acm_handle, msg, strlen((char*)msg), &actual_length);
    }
}

/**
 * @brief Wysyła paczkę danych pomiarowych z czujników przez port szeregowy USB.
 * Dane są formatowane jako tekst (CSV) z przedrostkiem "D:", co ułatwia ich
 * odczyt i parsowanie w aplikacji docelowej na komputerze PC.
 * * @param data  Wskaźnik na tablicę z surowymi wynikami pomiarów.
 * @param count Liczba czujników (elementów w tablicy) do wysłania.
 */
void USB_Send_SensorData(uint16_t *data, uint8_t count) {

    // Zabezpieczenie przed próbą wysłania, gdy kabel jest odpięty lub brak danych
    if ((cdc_acm_handle == NULL) || (count == 0)) {
        return;
    }

    // Bufor na ramkę tekstową (128 bajtów gwarantuje brak przepełnienia przy 10 czujnikach)
    char tx_buffer[128];
    int offset = 0;
    ULONG actual_length;

    //Zapis nagłówka ramki ("D:" oznacza Data)
    offset += snprintf(tx_buffer + offset, 128 - offset, "D:");

    //Formatowanie kolejnych pomiarów jako tekst i oddzielanie ich przecinkami
    for (int i = 0; i < count; i++) {
        offset += snprintf(tx_buffer + offset, 128 - offset, "%u,", data[i]);
    }

    //Dodanie znaków końca linii (sygnalizuje aplikacji PC koniec pojedynczej paczki)
    offset += snprintf(tx_buffer + offset, 128 - offset, "\r\n");

    //Fizyczne wysłanie gotowego bufora przez interfejs USBX
    _ux_device_class_cdc_acm_write(cdc_acm_handle, (UCHAR*)tx_buffer, offset, &actual_length);
}
