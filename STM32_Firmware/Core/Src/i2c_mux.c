#include "i2c_mux.h"
#include "main.h"

// Zewnętrzny uchwyt do magistrali I2C (zdefiniowany przez CubeMX w main.c)
extern I2C_HandleTypeDef hi2c1;

/**
 * @brief Przełącza aktywny kanał w multiplekserze I2C (np. z rodziny TCA9548A).
 * * Multiplekser ten działa w oparciu o 8-bitowy rejestr kontrolny. Każdy bit
 * odpowiada jednemu kanałowi sprzętowemu (bit 0 = kanał 0, bit 7 = kanał 7).
 * Ustawienie danego bitu na '1' otwiera połączenie z tym kanałem.
 * * @param mux_address 8-bitowy adres I2C układu multipleksera (przesunięty w lewo o 1 bit).
 * @param channel_nr  Numer kanału do aktywacji (zakres od 0 do 7).
 */
HAL_StatusTypeDef Channel_Choice(uint8_t mux_address, uint8_t channel_nr) {
    uint8_t data = 1 << channel_nr;
    return HAL_I2C_Master_Transmit(&hi2c1, mux_address, &data, 1, 100);
}
