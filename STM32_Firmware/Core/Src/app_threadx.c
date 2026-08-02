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

/* Private includes ----------------------------------------------------------*/
/* USER CODE BEGIN Includes */
#include <stdio.h>
#include <stdint.h>
#include "communication.h"
#include "main.h"
#include "tx_port.h"
#include "ux_api.h"
#include "ux_device_class_cdc_acm.h"
#include "ux_dcd_stm32.h"
#include "ux_device_descriptors.h"
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
  tx_thread_create(&main_app_thread, "Sensor Loop", main_thread_entry, 0, main_thread_stack, sizeof(main_thread_stack), 30, 30, TX_NO_TIME_SLICE, TX_AUTO_START);
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
    UCHAR receive_buffer[16];
    ULONG actual_length;
    UINT status;
    uint8_t is_authorized = 0;

    extern PCD_HandleTypeDef hpcd_USB_DRD_FS;

    HAL_PCDEx_PMAConfig(&hpcd_USB_DRD_FS, 0x00, PCD_SNG_BUF, 0x40);
    HAL_PCDEx_PMAConfig(&hpcd_USB_DRD_FS, 0x80, PCD_SNG_BUF, 0x80);
    HAL_PCDEx_PMAConfig(&hpcd_USB_DRD_FS, USBD_CDCACM_EPOUT_ADDR,    PCD_SNG_BUF, 0xC0);
    HAL_PCDEx_PMAConfig(&hpcd_USB_DRD_FS, USBD_CDCACM_EPIN_ADDR,     PCD_SNG_BUF, 0x100);
    HAL_PCDEx_PMAConfig(&hpcd_USB_DRD_FS, USBD_CDCACM_EPINCMD_ADDR,  PCD_SNG_BUF, 0x140);

    ux_dcd_stm32_initialize((ULONG)hpcd_USB_DRD_FS.Instance, (ULONG)&hpcd_USB_DRD_FS);

    HAL_PCD_Start(&hpcd_USB_DRD_FS);

    /* Pętla główna */
    while(1) {
        if (cdc_acm_handle != NULL && cdc_acm_handle->ux_slave_class_cdc_acm_data_dtr_state == 1) {

            if (!is_authorized) {
                status = ux_device_class_cdc_acm_read(cdc_acm_handle, receive_buffer, 16, &actual_length);

                if (status == UX_SUCCESS && actual_length > 0 && receive_buffer[0] == 'I') {
                    USB_Send_Handshake();
                    is_authorized = 1;
                }
            } else {
            	Read_All_Force_Sensors();
                USB_Send_SensorData(sensor_data, 10);
            }
        } else {
            is_authorized = 0;
        }

        tx_thread_sleep(5);
    }
}
/* USER CODE END 1 */
