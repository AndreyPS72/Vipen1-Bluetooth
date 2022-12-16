#ifndef _Analog_h
#define _Analog_h

#include <stdint.h>





#define GAP_ADTYPE_FLAGS                    1
#define GAP_ADTYPE_LOCAL_NAME_COMPLETE      9
#define GAP_ADTYPE_MANUFACTURER_SPECIFIC    0xFF

#define GAP_ADTYPE_FLAGS_GENERAL    0x02
#define GAP_ADTYPE_FLAGS_BREDR_NOT_SUPPORTED    0x04


#define TI_COMPANY_ID (0x000D) // Company Identifier: Texas Instruments Inc. (13)
#define ViPen_Bt_ID   (0x4F5C) // Magic number


// Команда, Write:
#define ViPen_Command_Start	0x0001 // Запустить измерение (зажать кнопку)
#define ViPen_Command_Stop	0x0002 // Остановить измерение (отпустить кнопку)
#define ViPen_Command_Off	0x0003 // Выключить прибор
#define ViPen_Command_Idle	0x0004 // Чтобы прибор не выключался через 1 минуту, иногда будить его


// Эти команды только для производителя. Пользователю их не показывать
#define ViPen_Command_Calibration  	0x0010	// Вход в режим тест/калибровка без переключателя



// Биты состояния, Read, Notify:
#define ViPen_State_Stoped	(0<<0) // Прибор стоит
#define ViPen_State_Started	(1<<0) // Прибор в режиме измерения (может быть с кнопки)
#define ViPen_State_NoData	(0<<1) // Данных нет (после инициализации)
#define ViPen_State_Data	(1<<1) // Есть данные


#define ViPen_Get_Data_Vel  (0x0010) // запросить сигнал Канала Velocity
#define ViPen_Get_Data_Acc	(0x0011) // запросить сигнал Канала Acceleration








#pragma pack(1) // Все структуры упакованные 




// Data Length Extension = Max 247 байт (не 27)

#define DATA_BLOCK_LEN (150) // длина блока данных

#define STAMPS_IN_BLOCK ((DATA_BLOCK_LEN-2)/2) // 74 отсчёта в блоке

typedef __packed struct stWaveform_Header
{
    uint8_t ViPen_Get_Data_Command;	// Команда из п.5
    uint8_t ViPen_Get_Data_Block; 	// Номер блока из п.5
    uint8_t ViPen_Get_Wave_ID; 	// Счётчик, позволяет проверить, что качаем тот-же замер
                                // Увеличивается на 1, при запросе заголовка
    uint8_t Reserv1;
    uint32_t Timestamp;	//  Счётчик 1024 Гц, совпадает с User_Data. Timestamp
    float Coeff;	// Коэф перевода данных sint16_t  в float, 4 байта
    uint16_t Reserv2[(DATA_BLOCK_LEN-12)/2];
} TWaveform_Header;
#define szTWaveform_Header sizeof(TWaveform_Header)
static_assert(szTWaveform_Header==DATA_BLOCK_LEN, "");


typedef __packed struct stWaveform_Data
{
    uint8_t ViPen_Get_Data_Block; 	// Номер блока из п.5
    uint8_t ViPen_Get_Wave_ID; 	// Счётчик, позволяет проверить, что качаем тот-же замер
    int16_t Wave[STAMPS_IN_BLOCK];	// отсчёты = 2 байта знаковое *74 отсчёта в блоке
} TWaveform_Data;
#define szTWaveform_Data sizeof(TWaveform_Data)
static_assert(szTWaveform_Data==DATA_BLOCK_LEN, "");



#define BLOCKS_IN_WAVEFORM (ANALOG_READ_SIZE/STAMPS_IN_BLOCK+1+1)





typedef __packed struct stAdvertisingData 
{
    uint8_t Len1;
    uint8_t Type1;
    uint8_t Flag1; 
    
    uint8_t Len9;
    uint8_t Type9;
    char Name9[5];
        
    uint8_t LenFF;
    uint8_t TypeFF;
    uint16_t ManID;
    uint8_t Addr;	// ==0
    uint16_t ID;	// const uint16_t ViPen_Bt_ID     = 0x4F5C; // Magic number
    uint32_t TimeStamp;	// Счётчик 1024 Гц для проверки, что появились новые данные
    int16_t Values[4];	// Velocity, Acceleration, Excess, Temperature
    
} TAdvertisingData;
#define szTAdvertisingData sizeof(TAdvertisingData)

static_assert(szTAdvertisingData==29, "");




// бекон
static TAdvertisingData AdvertisingData =
{
    0x02, GAP_ADTYPE_FLAGS, GAP_ADTYPE_FLAGS_GENERAL | GAP_ADTYPE_FLAGS_BREDR_NOT_SUPPORTED,
    0x06, GAP_ADTYPE_LOCAL_NAME_COMPLETE, 'V', 'i', 'P', 'e', 'n',
    0x12, GAP_ADTYPE_MANUFACTURER_SPECIFIC, TI_COMPANY_ID, 
        0x00, ViPen_Bt_ID, 0x00000000, 0, 0, -200, 0
};




#pragma pack()


#endif
