#include "btle_driver.h"
#include "btle_callbacks.h"
#include "checksum.h"

#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include <stdarg.h>
#include <stdlib.h>

extern HANDLE mainCharTxHandle;
extern HANDLE mainCharRxHandle;

#define LEFT 0
#define RIGHT 1
#define STOPD 2
#define FRONT 0
#define BACK 1

extern uint8_t SPEED;
extern int8_t DIR;
extern uint8_t FORWARD;

extern void SetDirection(uint8_t axis, uint8_t dir);
extern void SetPWM(uint8_t channelIndex, uint32_t value);

void BTLE_DisconnectHandler(void){
	SPEED = 0;
	DIR = 0;
	FORWARD = 1;
	SetPWM(1, 0);
	SetPWM(2, 0);
	SetDirection(LEFT, STOPD);
	SetDirection(RIGHT, STOPD);
}

void BTLE_CommandsHandler(uint8_t size, uint8_t *buffer){
	if(buffer[0] == 0)
		return;

	//checksum
	uint16_t c1 = (buffer[size-2]<<8)|buffer[size-1];
	uint16_t c2 = CalculateChecksum(buffer, size-2);

	if(c1 != c2){
		printf("Bad CMD checksum 0x%X 0x%X!\r\n", c1, c2);
		return;
	}

	if(buffer[0] == 0xAA){
		printf("CMD: SPEED %u DIR %d %s\r\n", buffer[1], (int8_t)buffer[2], buffer[3] == 1 ? "FORWARD" : "BACKWARD");

		SPEED = buffer[1];
		DIR = (int8_t)buffer[2];
		FORWARD = buffer[3];
	}

	memset(buffer, 0, size);
}

void BTLE_AttributeModifiedCallback(uint16_t handle, uint8_t data_length, uint8_t *att_data){
	if(handle == mainCharRxHandle+1){
		BTLE_CommandsHandler(data_length, att_data);
	}
}
