#include "checksum.h"

#include <stdint.h>

uint16_t CalculateChecksum(uint8_t* data, uint8_t data_size){

	uint16_t a = 0;
	uint16_t b = 0;

	for(uint8_t i = 0; i < data_size ; i++){
		a = (uint8_t)((a + data[i]) % 255);
		b = (uint8_t)((a + b) % 255);
	}

	return (uint16_t)((b << 8) | a);

}
