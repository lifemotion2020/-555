using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IGPR
{
    class Flowmodbus
    {
        

        // 流量计量参数恒定值  
        public const UInt16 REG_Sys_FlowStart = 0x4000;
        public const UInt16 REG_sysFlowValveMax = REG_Sys_FlowStart + 0x00;           // 流量计算最大阀口位移10000um=10mm  
        public const UInt16 REG_sysFlowValveZero = REG_Sys_FlowStart + 0x01;           // 流量计算阀口零点位置100um
        public const UInt16 REG_sysFlowValveCoef = REG_Sys_FlowStart + 0x02;           // 阀口系数1um
        public const UInt16 REG_sysFlowRangeMax = REG_Sys_FlowStart + 0x03;           // 流量标况瞬间量程,整数，0-65536Nm3/h 
        public const UInt16 REG_sysFlowRangeMin = REG_Sys_FlowStart + 0x04;           // 流量标况瞬间最低值，整数
        public const UInt16 REG_sysFlow_gas_D_Coef = REG_Sys_FlowStart + 0x05;           // 空气，值为1. 燃气和空气密度比例系数, 空气密度大,设定的是空气密度.
        public const UInt16 REG_sysFlow_cv = REG_Sys_FlowStart + 0x06;           // 不可压缩阶段流量计算常量,缺省2000
        public const UInt16 REG_sysFlow_cg = REG_Sys_FlowStart + 0x07;           // 可压缩临界阶段流量计算常量，缺省1200
        public const UInt16 REG_sysFlow_k1 = REG_Sys_FlowStart + 0x08;           // 可压缩亚临界阶段流量计算常量，缺省1.7
        public const UInt16 REG_sysFlow_temp0 = REG_Sys_FlowStart + 0x09;
        public const UInt16 REG_sysFlowLimitMax = REG_Sys_FlowStart + 0x0A;           // 限定标况瞬间流量最大设定值，0表示不限制。
        public const UInt16 REG_sysFlowLimitMin = REG_Sys_FlowStart + 0x0B;           // 限定标况瞬间流量最少设定值，0表示不限制。
        public const UInt16 REG_sysFlowSumRemainL = REG_Sys_FlowStart + 0x0C;           // 限定累计流量低，0表示不限制。  
        public const UInt16 REG_sysFlowSumRemainH = REG_Sys_FlowStart + 0x0D;           // 限定累计流量高，0表示不限制。  
        public const UInt16 REG_sysFlowCurRemainL = REG_Sys_FlowStart + 0x0E;           // 累计剩余流量低,每10分钟存一次
        public const UInt16 REG_sysFlowCurRemainH = REG_Sys_FlowStart + 0x0F;           // 累计剩余流量高,每10分钟存一次
        public const UInt16 REG_sysFlowSumAllL = REG_Sys_FlowStart + 0x10;           // 累计流量低
        public const UInt16 REG_sysFlowSumAllH = REG_Sys_FlowStart + 0x11;           // 累计流量高
        public const UInt16 REG_sysFlowValveStep = REG_Sys_FlowStart + 0x12;           // 阀口10000um对应步数


        // 流量校准计算系数,低压  
        public const UInt16 REG_sysFlowInPressLow = REG_Sys_FlowStart + 0x20;           // 流量系数对应的进口压力 
        public const UInt16 REG_sysFlowOutPressLow = REG_Sys_FlowStart + 0x21;           // 流量系数对应的出口压力 
        public const UInt16 REG_sysCoef0_Low = REG_Sys_FlowStart + 0x22;           // 流量系数0
        public const UInt16 REG_sysCoef1_Low = REG_Sys_FlowStart + 0x23;           // 流量系数1
        public const UInt16 REG_sysCoef2_Low = REG_Sys_FlowStart + 0x24;           // 流量系数2
        public const UInt16 REG_sysCoef3_Low = REG_Sys_FlowStart + 0x25;           // 流量系数3
        public const UInt16 REG_sysCoef4_Low = REG_Sys_FlowStart + 0x26;           // 流量系数4
        public const UInt16 REG_sysCoef5_Low = REG_Sys_FlowStart + 0x27;           // 流量系数5
        public const UInt16 REG_sysCoef6_Low = REG_Sys_FlowStart + 0x28;           // 流量系数6
        public const UInt16 REG_sysCoef7_Low = REG_Sys_FlowStart + 0x29;           // 流量系数7
        public const UInt16 REG_sysCoef8_Low = REG_Sys_FlowStart + 0x2A;           // 流量系数8
        public const UInt16 REG_sysCoef9_Low = REG_Sys_FlowStart + 0x2B;           // 流量系数9
        public const UInt16 REG_sysCoef10_Low = REG_Sys_FlowStart + 0x2C;           // 流量系数10
        public const UInt16 REG_sysCoef11_Low = REG_Sys_FlowStart + 0x2D;           // 流量系数11
        public const UInt16 REG_sysCoef12_Low = REG_Sys_FlowStart + 0x2E;           // 流量系数12
        public const UInt16 REG_sysCoefClear_Low = REG_Sys_FlowStart + 0x2F;        // 流量系数13

        // 流量阀口分段，低压 
        public const UInt16 REG_sysPercent0_Low = REG_Sys_FlowStart + 0x30;           // 流量阀口分段0
        public const UInt16 REG_sysPercent1_Low = REG_Sys_FlowStart + 0x31;           // 流量阀口分段1
        public const UInt16 REG_sysPercent2_Low = REG_Sys_FlowStart + 0x32;           // 流量阀口分段2
        public const UInt16 REG_sysPercent3_Low = REG_Sys_FlowStart + 0x33;           // 流量阀口分段3
        public const UInt16 REG_sysPercent4_Low = REG_Sys_FlowStart + 0x34;           // 流量阀口分段4
        public const UInt16 REG_sysPercent5_Low = REG_Sys_FlowStart + 0x35;           // 流量阀口分段5
        public const UInt16 REG_sysPercent6_Low = REG_Sys_FlowStart + 0x36;           // 流量阀口分段6
        public const UInt16 REG_sysPercent7_Low = REG_Sys_FlowStart + 0x37;           // 流量阀口分段7

        public const UInt16 REG_sysPercent8_Low = REG_Sys_FlowStart + 0x38;           // 流量阀口分段8
        public const UInt16 REG_sysPercent9_Low = REG_Sys_FlowStart + 0x39;           // 流量阀口分段9
        public const UInt16 REG_sysPercent10_Low = REG_Sys_FlowStart + 0x3A;           // 流量阀口分段10
        public const UInt16 REG_sysPercent11_Low = REG_Sys_FlowStart + 0x3B;           // 流量阀口分段11
        public const UInt16 REG_sysPercent12_Low = REG_Sys_FlowStart + 0x3C;           // 流量阀口分段12
        public const UInt16 REG_sysPercentClear_Low = REG_Sys_FlowStart + 0x3D;           // 流量阀口分段13

        // 流量校准计算系数,中压  
        public const UInt16 REG_sysFlowInPressMid = REG_Sys_FlowStart + 0x40;           // 流量系数对应的进口压力 
        public const UInt16 REG_sysFlowOutPressMid = REG_Sys_FlowStart + 0x41;           // 流量系数对应的出口压力 
        public const UInt16 REG_sysCoef0_Mid = REG_Sys_FlowStart + 0x42;           // 流量系数0
        public const UInt16 REG_sysCoef1_Mid = REG_Sys_FlowStart + 0x43;           // 流量系数1
        public const UInt16 REG_sysCoef2_Mid = REG_Sys_FlowStart + 0x44;           // 流量系数2
        public const UInt16 REG_sysCoef3_Mid = REG_Sys_FlowStart + 0x45;           // 流量系数3
        public const UInt16 REG_sysCoef4_Mid = REG_Sys_FlowStart + 0x46;           // 流量系数4
        public const UInt16 REG_sysCoef5_Mid = REG_Sys_FlowStart + 0x47;           // 流量系数5
        public const UInt16 REG_sysCoef6_Mid = REG_Sys_FlowStart + 0x48;           // 流量系数6
        public const UInt16 REG_sysCoef7_Mid = REG_Sys_FlowStart + 0x49;           // 流量系数7
        public const UInt16 REG_sysCoef8_Mid = REG_Sys_FlowStart + 0x4A;           // 流量系数8
        public const UInt16 REG_sysCoef9_Mid = REG_Sys_FlowStart + 0x4B;           // 流量系数9
        public const UInt16 REG_sysCoef10_Mid = REG_Sys_FlowStart + 0x4C;           // 流量系数10
        public const UInt16 REG_sysCoef11_Mid = REG_Sys_FlowStart + 0x4D;           // 流量系数11
        public const UInt16 REG_sysCoef12_Mid = REG_Sys_FlowStart + 0x4E;           // 流量系数12
        public const UInt16 REG_sysCoefClear_Mid = REG_Sys_FlowStart + 0x4F;           // 流量系数13

        // 流量阀口分段，中压 
        public const UInt16 REG_sysPercent0_Mid = REG_Sys_FlowStart + 0x50;           // 流量阀口分段0
        public const UInt16 REG_sysPercent1_Mid = REG_Sys_FlowStart + 0x51;           // 流量阀口分段1
        public const UInt16 REG_sysPercent2_Mid = REG_Sys_FlowStart + 0x52;           // 流量阀口分段2
        public const UInt16 REG_sysPercent3_Mid = REG_Sys_FlowStart + 0x53;           // 流量阀口分段3
        public const UInt16 REG_sysPercent4_Mid = REG_Sys_FlowStart + 0x54;           // 流量阀口分段4
        public const UInt16 REG_sysPercent5_Mid = REG_Sys_FlowStart + 0x55;           // 流量阀口分段5
        public const UInt16 REG_sysPercent6_Mid = REG_Sys_FlowStart + 0x56;           // 流量阀口分段6
        public const UInt16 REG_sysPercent7_Mid = REG_Sys_FlowStart + 0x57;           // 流量阀口分段7

        public const UInt16 REG_sysPercent8_Mid = REG_Sys_FlowStart + 0x58;           // 流量阀口分段8
        public const UInt16 REG_sysPercent9_Mid = REG_Sys_FlowStart + 0x59;           // 流量阀口分段9
        public const UInt16 REG_sysPercent10_Mid = REG_Sys_FlowStart + 0x5A;           // 流量阀口分段10
        public const UInt16 REG_sysPercent11_Mid = REG_Sys_FlowStart + 0x5B;           // 流量阀口分段11
        public const UInt16 REG_sysPercent12_Mid = REG_Sys_FlowStart + 0x5C;           // 流量阀口分段12
        public const UInt16 REG_sysPercentClear_Mid = REG_Sys_FlowStart + 0x5D;           // 流量阀口分段13


        // 流量校准计算系数,高压  
        public const UInt16 REG_sysFlowInPressHigh = REG_Sys_FlowStart + 0x60;           // 流量系数对应的进口压力 
        public const UInt16 REG_sysFlowOutPressHigh = REG_Sys_FlowStart + 0x61;           // 流量系数对应的出口压力 
        public const UInt16 REG_sysCoef0_High = REG_Sys_FlowStart + 0x62;           // 流量系数0
        public const UInt16 REG_sysCoef1_High = REG_Sys_FlowStart + 0x63;          // 流量系数1
        public const UInt16 REG_sysCoef2_High = REG_Sys_FlowStart + 0x64;           // 流量系数2
        public const UInt16 REG_sysCoef3_High = REG_Sys_FlowStart + 0x65;           // 流量系数3
        public const UInt16 REG_sysCoef4_High = REG_Sys_FlowStart + 0x66;           // 流量系数4
        public const UInt16 REG_sysCoef5_High = REG_Sys_FlowStart + 0x67;           // 流量系数5
        public const UInt16 REG_sysCoef6_High = REG_Sys_FlowStart + 0x68;           // 流量系数6
        public const UInt16 REG_sysCoef7_High = REG_Sys_FlowStart + 0x69;           // 流量系数7
        public const UInt16 REG_sysCoef8_High = REG_Sys_FlowStart + 0x6A;           // 流量系数8
        public const UInt16 REG_sysCoef9_High = REG_Sys_FlowStart + 0x6B;           // 流量系数9
        public const UInt16 REG_sysCoef10_High = REG_Sys_FlowStart + 0x6C;           // 流量系数10
        public const UInt16 REG_sysCoef11_High = REG_Sys_FlowStart + 0x6D;           // 流量系数11
        public const UInt16 REG_sysCoef12_High = REG_Sys_FlowStart + 0x6E;           // 流量系数12
        public const UInt16 REG_sysCoefClear_High = REG_Sys_FlowStart + 0x6F;           // 流量系数13

        // 流量阀口分段，高压 
        public const UInt16 REG_sysPercent0_High = REG_Sys_FlowStart + 0x70;           // 流量阀口分段0  0.50Qmin
        public const UInt16 REG_sysPercent1_High = REG_Sys_FlowStart + 0x71;           // 流量阀口分段1  1.00Qmin
        public const UInt16 REG_sysPercent2_High = REG_Sys_FlowStart + 0x72;           // 流量阀口分段2
        public const UInt16 REG_sysPercent3_High = REG_Sys_FlowStart + 0x73;           // 流量阀口分段3
        public const UInt16 REG_sysPercent4_High = REG_Sys_FlowStart + 0x74;           // 流量阀口分段4
        public const UInt16 REG_sysPercent5_High = REG_Sys_FlowStart + 0x75;           // 流量阀口分段5
        public const UInt16 REG_sysPercent6_High = REG_Sys_FlowStart + 0x76;          // 流量阀口分段6
        public const UInt16 REG_sysPercent7_High = REG_Sys_FlowStart + 0x77;           // 流量阀口分段7

        public const UInt16 REG_sysPercent8_High = REG_Sys_FlowStart + 0x78;           // 流量阀口分段8
        public const UInt16 REG_sysPercent9_High = REG_Sys_FlowStart + 0x79;           // 流量阀口分段9
        public const UInt16 REG_sysPercent10_High = REG_Sys_FlowStart + 0x7A;           // 流量阀口分段10
        public const UInt16 REG_sysPercent11_High = REG_Sys_FlowStart + 0x7B;           // 流量阀口分段11
        public const UInt16 REG_sysPercent12_High = REG_Sys_FlowStart + 0x7C;           // 流量阀口分段12
        public const UInt16 REG_sysPercentClear_High = REG_Sys_FlowStart + 0x7D;           // 流量阀口分段13


        // 流量限制24小时时间段  
        public const UInt16 REG_sysFlowValid = REG_Sys_FlowStart + 0x80;           // 流量限流有限标志
        public const UInt16 REG_sysFlowMinutes = REG_Sys_FlowStart + 0x81;           // 流量限流时间间隔1小时 = 60

        public const UInt16 REG_sysFlowTable0 = REG_Sys_FlowStart + 0x82;          // 24个区间段,0小时
        public const UInt16 REG_sysFlowTable1 = REG_Sys_FlowStart + 0x83;          // 24个区间段,1小时
        public const UInt16 REG_sysFlowTable2 = REG_Sys_FlowStart + 0x84;           // 24个区间段,2小时
        public const UInt16 REG_sysFlowTable3 = REG_Sys_FlowStart + 0x85;           // 24个区间段,3小时
        public const UInt16 REG_sysFlowTable4 = REG_Sys_FlowStart + 0x86;           // 24个区间段,4小时
        public const UInt16 REG_sysFlowTable5 = REG_Sys_FlowStart + 0x87;           // 24个区间段,5小时
        public const UInt16 REG_sysFlowTable6 = REG_Sys_FlowStart + 0x88;           // 24个区间段,6小时
        public const UInt16 REG_sysFlowTable7 = REG_Sys_FlowStart + 0x89;           // 24个区间段,7小时

        public const UInt16 REG_sysFlowTable8 = REG_Sys_FlowStart + 0x8A;          // 24个区间段,8小时
        public const UInt16 REG_sysFlowTable9 = REG_Sys_FlowStart + 0x8B;           // 24个区间段,9小时
        public const UInt16 REG_sysFlowTable10 = REG_Sys_FlowStart + 0x8C;          // 24个区间段,10小时
        public const UInt16 REG_sysFlowTable11 = REG_Sys_FlowStart + 0x8D;          // 24个区间段,11小时
        public const UInt16 REG_sysFlowTable12 = REG_Sys_FlowStart + 0x8E;          // 24个区间段,12小时
        public const UInt16 REG_sysFlowTable13 = REG_Sys_FlowStart + 0x8F;          // 24个区间段,13小时
        public const UInt16 REG_sysFlowTable14 = REG_Sys_FlowStart + 0x90;          // 24个区间段,14小时
        public const UInt16 REG_sysFlowTable15 = REG_Sys_FlowStart + 0x91;           // 24个区间段,15小时

        public const UInt16 REG_sysFlowTable16 = REG_Sys_FlowStart + 0x92;           // 24个区间段,16小时
        public const UInt16 REG_sysFlowTable17 = REG_Sys_FlowStart + 0x93;          // 24个区间段,17小时
        public const UInt16 REG_sysFlowTable18 = REG_Sys_FlowStart + 0x94;          // 24个区间段,18小时
        public const UInt16 REG_sysFlowTable19 = REG_Sys_FlowStart + 0x95;         // 24个区间段,19小时
        public const UInt16 REG_sysFlowTable20 = REG_Sys_FlowStart + 0x96;           // 24个区间段,20小时
        public const UInt16 REG_sysFlowTable21 = REG_Sys_FlowStart + 0x97;           // 24个区间段,21小时
        public const UInt16 REG_sysFlowTable22 = REG_Sys_FlowStart + 0x98;          // 24个区间段,22小时
        public const UInt16 REG_sysFlowTable23 = REG_Sys_FlowStart + 0x99;           // 24个区间段,23小时

        public const UInt16 REG_Sys_FlowEnd = REG_sysFlowTable23+16;

      

        public static void FlowSet24HourTable(UInt16 hour, UInt16 flowVal)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowTable0;                   // 寄存器地址， 2字节 
            regAddr = (UInt16)(regAddr + hour);
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)flowVal;       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }

        public static void FlowSet24HourTableEnable(UInt16 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowValid;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)Val;       // 写一个数据到寄存器，2字节
            if (dat[0] != 0)            // 0或1 
                dat[0] = 1;
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }

        public static void Flow24HourTable_Read(UInt16 len)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowValid;                   // 寄存器地址， 2字节 
            UInt16 readlen = len;                       // 读N个(数值1-125)寄存器，2字节
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }


        public static void FlowCoefInPress(UInt16 type, UInt32 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowInPressLow;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)Val;       // 写一个数据到寄存器，2字节
            switch (type)
            {
                case 0:
                    regAddr = REG_sysFlowInPressLow;                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
                    break;
                case 1:
                    regAddr = REG_sysFlowInPressMid;                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
                    break;
                case 2:
                    regAddr = REG_sysFlowInPressHigh;                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
                    break;
                default:
                    break;
            }
        }

        public static void FlowCoefOutPress(UInt16 type, UInt32 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowOutPressLow;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)Val;       // 写一个数据到寄存器，2字节
            switch (type)
            {
                case 0:
                    regAddr = REG_sysFlowOutPressLow;                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
                    break;
                case 1:
                    regAddr = REG_sysFlowOutPressMid;                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
                    break;
                case 2:
                    regAddr = REG_sysFlowOutPressHigh;                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
                    break;
                default:
                    break;
            }
        }

        public static void FlowValveSet(UInt16 type, UInt16 point,UInt16 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysPercent0_Low;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)Val;       // 写一个数据到寄存器，2字节
            if (point >= 13)
                return;    //点无效
            switch (type)
            {
                case 0:
                    regAddr = (UInt16)(REG_sysPercent0_Low+point);                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
                    break;
                case 1:
                    regAddr = (UInt16)(REG_sysPercent0_Mid + point);                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
                    break;
                case 2:
                    regAddr = (UInt16)(REG_sysPercent0_High + point);                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
                    break;
                default:
                    break;
            }
        }


        public static void FlowCoefSet(UInt16 type, UInt16 point, UInt32 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysCoef0_Low;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)Val;       // 写一个数据到寄存器，2字节
            if (point >= 13)
                return;    //点无效
            switch (type)
            {
                case 0:
                    regAddr = (UInt16)(REG_sysCoef0_Low + point);                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
                    break;
                case 1:
                    regAddr = (UInt16)(REG_sysCoef0_Mid + point);                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
                    break;
                case 2:
                    regAddr = (UInt16)(REG_sysCoef0_High + point);                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
                    break;
                default:
                    break;
            }
        }


        public static void FlowValveList_Read(UInt16 type, UInt16 len)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysPercent0_Low;                   // 寄存器地址， 2字节 
            UInt16 readlen = len;                       // 读N个(数值1-125)寄存器，2字节
            switch (type)
            {
                case 0:
                    regAddr = REG_sysPercent0_Low;                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Read03Command(devaddr, regAddr, readlen); // 
                    break;
                case 1:
                    regAddr =REG_sysPercent0_Mid;                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Read03Command(devaddr, regAddr, readlen); // 
                    break;
                case 2:
                    regAddr =REG_sysPercent0_High;                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Read03Command(devaddr, regAddr, readlen); // 
                    break;
                default:
                    break;
            }
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }


        public static void FlowCoefList_Read(UInt16 type, UInt16 len)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowInPressLow;                   // 寄存器地址， 2字节 
            UInt16 readlen = len;                       // 读N个(数值1-125)寄存器，2字节
            switch (type)
            {
                case 0:
                    regAddr = REG_sysFlowInPressLow;                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Read03Command(devaddr, regAddr, readlen); // 
                    break;
                case 1:
                    regAddr = REG_sysFlowInPressMid;                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Read03Command(devaddr, regAddr, readlen); // 
                    break;
                case 2:
                    regAddr = REG_sysFlowInPressHigh;                   // 寄存器地址， 2字节 
                    Modbus.Modbus_Read03Command(devaddr, regAddr, readlen); // 
                    break;
                default:
                    break;
            }
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }


        public static void FlowInfoConfig_Read(UInt16 len)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowValveMax;                   // 寄存器地址， 2字节 
            UInt16 readlen = len;                       // 读N个(数值1-125)寄存器，2字节
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }


        public static void FlowSet_sysFlowValveMax(UInt16 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowValveMax;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)Val;       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }

        public static void FlowSet_sysFlowValveZero(UInt16 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowValveZero;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)Val;       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }


        public static void FlowSet_sysFlowValveCoef(UInt16 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowValveCoef;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)Val;       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }

        public static void FlowSet_sysFlowValveStep(UInt16 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowValveStep;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)Val;       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }

        public static void FlowSet_sysFlowRangeMax(UInt32 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowRangeMax;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            
            dat[0] = (UInt16)DataTrans.translate_uint32_to_uint16(Val);       // 写一个数据到寄存器，2字节
            
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }


        public static void FlowSet_sysFlowRangeMin(UInt32 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowRangeMin;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];

            dat[0] = (UInt16)DataTrans.translate_uint32_to_uint16(Val);       // 写一个数据到寄存器，2字节
            
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }


        public static void FlowSet_sysFlow_cv(UInt16 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlow_cv;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];

            dat[0] = Val;       // 写一个数据到寄存器，2字节
            
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }

        public static void FlowSet_sysFlow_cg(UInt16 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlow_cg;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];

            dat[0] = Val;       // 写一个数据到寄存器，2字节

            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }

        public static void FlowSet_sysFlow_kl(UInt16 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlow_k1;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];

            dat[0] = Val;       // 写一个数据到寄存器，2字节

            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }

        public static void FlowSet_sysFlow_gas_D_Coef(UInt16 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlow_gas_D_Coef;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];

            dat[0] = Val;       // 写一个数据到寄存器，2字节

            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }


        public static void FlowSet_sysFlowLimitMax(UInt16 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowLimitMax;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];

            dat[0] = Val;       // 写一个数据到寄存器，2字节

            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }


        public static void FlowSet_sysFlowLimitMin(UInt16 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowLimitMin;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];

            dat[0] = Val;       // 写一个数据到寄存器，2字节

            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }


        public static void FlowSet_sysFlowLimitSum(UInt32 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysFlowSumRemainL;                   // 寄存器地址， 2字节 
            UInt16 writelen = 2;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];

            dat[0] = (UInt16)Val;       // 写一个数据到寄存器，2字节
            dat[1] = (UInt16)(Val>>16);       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }

        public static void FlowSet_FlowCoefDefault(UInt32 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysCoefClear_Low;                   // 寄存器地址， 2字节 
            switch(Val)
            {
                case 0:
                    regAddr = REG_sysCoefClear_Low;
                    break;
                case 1:
                    regAddr = REG_sysCoefClear_Mid;
                    break;
                case 2:
                    regAddr = REG_sysCoefClear_High;
                    break;
                default:
                    return;
                    break;
            }
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 0;       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }


        public static void FlowSet_FlowValveDefault(UInt32 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysPercentClear_Low;                   // 寄存器地址， 2字节 
            switch (Val)
            {
                case 0:
                    regAddr = REG_sysPercentClear_Low;
                    break;
                case 1:
                    regAddr = REG_sysPercentClear_Mid;
                    break;
                case 2:
                    regAddr = REG_sysPercentClear_High;
                    break;
                default:
                    return;
                    break;
            }
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 0;       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
        }


    }
}
