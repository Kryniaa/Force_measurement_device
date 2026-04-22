/* USER CODE BEGIN Header */
/**
  ******************************************************************************
  * @file    app_threadx.c
  * @author  MCD Application Team
  * @brief   ThreadX applicative file
  ******************************************************************************
    * @attention
  *
  * Copyright (c) 2026 STMicroelectronics.
  * All rights reserved.
  *
  * This software is licensed under terms that can be found in the LICENSE file
  * in the root directory of this software component.
  * If no LICENSE file comes with this software, it is provided AS-IS.
  *
  ******************************************************************************
  */
/* USER CODE END Header */

/* Includes ------------------------------------------------------------------*/
#include "app_threadx.h"
#include "force_sensor.h"
#include "tx_api.h"

/* Private includes ----------------------------------------------------------*/
/* USER CODE BEGIN Includes */
#include <stdio.h>
#include <stdint.h>
#include "communication.h"
#include "tx_port.h"
#include "ux_api.h"
#include "ux_device_class_cdc_acm.h"
/* USER CODE END Includes */

/* Private typedef -----------------------------------------------------------*/
/* USER CODE BEGIN PTD */

/* USER CODE END PTD */

/* Private define ------------------------------------------------------------*/
/* USER CODE BEGIN PD */

/* USER CODE END PD */

/* Private macro -------------------------------------------------------------*/
/* USER CODE BEGIN PM */

/* USER CODE END PM */

/* Private variables ---------------------------------------------------------*/
/* USER CODE BEGIN PV */
extern uint16_t sensor_data[10];

extern void Read_All_Force_Sensors(void);

extern UX_SLAVE_CLASS_CDC_ACM *cdc_acm_handle;

TX_THREAD main_app_thread;

uint8_t main_thread_stack[2048];
/* USER CODE END PV */

/* Private function prototypes -----------------------------------------------*/
/* USER CODE BEGIN PFP */
extern void Read_All_Force_Sensors(void);
void main_thread_entry(ULONG thread_input);
/* USER CODE END PFP */

/**
  * @brief  Application ThreadX Initialization.
  * @param memory_ptr: memory pointer
  * @retval int
  */
UINT App_ThreadX_Init(VOID *memory_ptr)
{
  UINT ret = TX_SUCCESS;
  /* USER CODE BEGIN App_ThreadX_MEM_POOL */

  /* USER CODE END App_ThreadX_MEM_POOL */
  /* USER CODE BEGIN App_ThreadX_Init */
  tx_thread_create(&main_app_thread, "Sensor Loop", main_thread_entry, 0, main_thread_stack, sizeof(main_thread_stack), 5, 5, TX_NO_TIME_SLICE, TX_AUTO_START);
  /* USER CODE END App_ThreadX_Init */

  return ret;
}

  /**
  * @brief  Function that implements the kernel's initialization.
  * @param  None
  * @retval None
  */
void MX_ThreadX_Init(void)
{
  /* USER CODE BEGIN Before_Kernel_Start */

  /* USER CODE END Before_Kernel_Start */

  tx_kernel_enter();

  /* USER CODE BEGIN Kernel_Start_Error */

  /* USER CODE END Kernel_Start_Error */
}

/* USER CODE BEGIN 1 */
/**
 * @brief Główna pętla sterująca komunikacją USB i odczytem czujników.
 * Funkcja działa jako dedykowany wątek systemu ThreadX.
 * * @param thread_input Parametr wejściowy wątku (wymagany przez ThreadX, nieużywany).
 */
void main_thread_entry(ULONG thread_input) {

    /* Inicjalizacja lokalnych zmiennych pomocniczych:
     * - receive_buffer: tablica na dane przychodzące z komputera.
     * - actual_length: liczba bajtów faktycznie odebranych przez USB.
     * - status: przechowuje wynik operacji USBX.
     * - is_authorized: flaga logiczna określająca, czy urządzenie
     * przeszło etap Handshake.
     */
    UCHAR receive_buffer[16];
    ULONG actual_length;
    UINT status;
    uint8_t is_authorized = 0;

    while(1) {

        /* Sprawdzenie fizycznego i logicznego połączenia.
         * cdc_acm_handle != NULL: Czy port USB został zainicjalizowany?
         * ux_slave_class_cdc_acm_data_dtr_state == 1: Czy aplikacja na PC
         * otworzyła port COM? Jeśli tak, możemy rozmawiać.
         */
        if (cdc_acm_handle != NULL && cdc_acm_handle->ux_slave_class_cdc_acm_data_dtr_state == 1) {

            /* Autoryzacja.
             * Jeśli urządzenie jest połączone, ale jeszcze nie zostało "przywitane".
             */
            if (!is_authorized) {
                // Próba nieblokującego odczytu danych z portu USB
                status = ux_device_class_cdc_acm_read(cdc_acm_handle, receive_buffer, 16, &actual_length);

                /* Jeśli odczyt się udał, odebrano dane, a pierwszym znakiem jest litera 'I':
                 * 1. Wyślij ramkę identyfikacyjną do PC (Handshake).
                 * 2. Ustaw flagę autoryzacji na 1, co odblokuje przesyłanie pomiarów.
                 */
                if (status == UX_SUCCESS && actual_length > 0 && receive_buffer[0] == 'I') {
                    USB_Send_Handshake();
                    is_authorized = 1;
                }
            }
            /* Transmisja danych pomiarowych.
             * Jeśli urządzenie jest zautoryzowane, wykonuj pomiary i wysyłka do PC.
             */
            else {
                Read_All_Force_Sensors();        // Pobieranie surowe dane
                USB_Send_SensorData(sensor_data, 10); // Wysyłka jako tekst
            }

        }
        /* Resetowanie stanu.
         * Jeśli kabel zostanie odpięty lub aplikacja na PC zamknięta,
         * wymuś ponowną autoryzację przy następnym połączeniu.
         */
        else {
            is_authorized = 0;
        }

        /* Uśpienie wątku.
         * Wstrzymuje wątek na 10 ticków systemowych.
         * Zapobiega to blokowaniu procesora i pozwala na pracę innym wątkom.
         */
        tx_thread_sleep(10);
    }
}
/* USER CODE END 1 */
