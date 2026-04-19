#include "scaling.h"

/**
 * @brief  Uśrednia serię pomiarów z czujników FX29 i przelicza wynik na siłę (Niutony).
 * Stosuje oversampling w celu redukcji szumów i zakłóceń mechanicznych.
 * @param  sensor_data Dwuwymiarowa tablica: [numer_czujnika][numer_probki]
 * @param  output      Jednowymiarowa tablica wyjściowa z uśrednioną siłą (N) dla każdego czujnika.
 * @retval None
 */
void Readings_scaling(uint16_t sensor_data[SAMPLES_PER_SENSOR][NUM_SENSORS], float output[NUM_SENSORS]) {
    
    // Zabezpieczenie przed przekazaniem pustych wskaźników
    if (sensor_data == NULL || output == NULL) return;

    // Pętla iterująca po fizycznych czujnikach
    for (int i = 0; i < NUM_SENSORS; i++) {

        uint32_t raw_sum = 0;
        uint8_t valid_samples = 0; // Liczymy, ile próbek nie miało błędu
        
        // Pętla iterująca po 10 próbkach z jednego, konkretnego czujnika
        for (int j = 0; j < SAMPLES_PER_SENSOR; j++) {
            
            uint16_t raw_val = sensor_data[j][i];
           

            // Odrzucenie błędnego odczytu
            if (raw_val == 0xFFFF) continue;
            
            // Zgodnie z notą katalogową "zero" to wartość 1000. 
            if (raw_val < 1000) raw_val = 1000; 
            
            // Wartość 15000 to twardy limit czujnika (50 lbf). 
            if (raw_val > 15000) raw_val = 15000;

            raw_sum += raw_val; // Dodajemy odszumione wartości całkowite do siebie
            valid_samples++;
        }

        if (valid_samples == 0) {
            output[i] = -999.0f; // Błąd sprzętowy
            continue;
        }
        // Wyliczamy średnią z surowych danych
        float avg_raw = (float)raw_sum / (float)valid_samples;
        
        // Zmiana na Newtony (zgodnie z notą katalogową)
        float avg_force_newtons = ((avg_raw - 1000.0f) * 50.0f / 14000.0f) * 4.44822f;
        
        // Zapisanie ostatecznego, uśrednionego wyniku czujnika do tablicy wyjściowej
        output[i] = avg_force_newtons;
    }
}

void Tare_sensors(void){
    
}