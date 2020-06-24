using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IGPR
{
    class PressPIDModus
    {
        /*
压力参数Modbus配置
*/
        public const UInt16 REG_Sys_PressStart = 0x4100;

        public const UInt16 REG_sysPID_FenMu = REG_Sys_PressStart + 0;        // PID参数的分母，缺省10000

        public const UInt16 REG_sysPID_NormKp = REG_Sys_PressStart + 1;         // 正常工作PID调压参数分子 比例  
        public const UInt16 REG_sysPID_NormKi = REG_Sys_PressStart + 2;         // 正常PID调压参数分子 微分
        public const UInt16 REG_sysPID_NormKd = REG_Sys_PressStart + 3;         // 正常PID调压参数分子 积分
        public const UInt16 REG_sysPID_NormInterval = REG_Sys_PressStart + 4;        // 正常PID调压时间间隔ms

        public const UInt16 REG_sysPID_IncKp = REG_Sys_PressStart + 5;        // 匀速PID调压参数分子 比例  
        public const UInt16 REG_sysPID_IncKi = REG_Sys_PressStart + 6;        // 匀速PID调压参数分子 微分
        public const UInt16 REG_sysPID_IncKd = REG_Sys_PressStart + 7;        // 匀速PID调压参数分子 积分
        public const UInt16 REG_sysPID_IncInterval = REG_Sys_PressStart + 8;         // 匀速PID调压时间间隔ms
        public const UInt16 REG_sysPID_IncMaxTime = REG_Sys_PressStart + 9;        // 匀速加压最大时间ms

        public const UInt16 REG_sysPID_MaxSpeed = REG_Sys_PressStart + 10;       // 电机最大步数
        public const UInt16 REG_sysPID_DiffMin = REG_Sys_PressStart + 11;       // 压力差值小于该值不调压
        public const UInt16 REG_sysPID_DiffMax = REG_Sys_PressStart + 12;        // 压力差值大于该值最大调压

        public const UInt16 REG_sysPID_Clear = REG_Sys_PressStart + 15;       // 恢复缺省配置

        public const UInt16 REG_sysPressOutObj = REG_Sys_PressStart + 16;        // 输出目标压力 
        public const UInt16 REG_sysPressOutObjMax = REG_Sys_PressStart + 17;     // 输出最高目标压力 
        public const UInt16 REG_sysPressOutObjMin = REG_Sys_PressStart + 18;     // 输出最低目标压力 
        public const UInt16 REG_sysPressFlowLimitMax = REG_Sys_PressStart + 19;    // 最大瞬时流量限制 
        public const UInt16 REG_sysPressFlowLimitMin = REG_Sys_PressStart + 20;     // 低流瞬时流量限制 
        public const UInt16 REG_sysPressFlowSumLimitL = REG_Sys_PressStart + 21;    // 累计流量限制low 16
        public const UInt16 REG_sysPressFlowSumLimitH = REG_Sys_PressStart + 22;    // 累计流量限制high 16

        // 读取软件和硬件版本
        public const UInt16 REG_sysSoftVer = REG_Sys_PressStart + 24;     // 软件版本
        public const UInt16 REG_sysHradVer = REG_Sys_PressStart + 25;     // 硬件版本

        // Modbus模式
        public const UInt16 REG_sysModbusSel = REG_Sys_PressStart + 26;     // Modbus模式选择，输入正确密码进入，错误密码退出
        public const UInt16 REG_sysModbusUser = REG_Sys_PressStart + 27;     // Modbus用户模式
        public const UInt16 REG_sysModbusAdmin = REG_Sys_PressStart + 28;     // Modbus管理模式
        public const UInt16 REG_sysModbusSupper = REG_Sys_PressStart + 29;     // Modbus超级管理员 

        // 用户ID
        public const UInt16 REG_sysDeviceID_L = REG_Sys_PressStart + 30;       // YY
        public const UInt16 REG_sysDeviceID_H = REG_Sys_PressStart + 31;      // MM

        public const UInt16 REG_sysRunMode = REG_Sys_PressStart + 33;     
        public const UInt16 REG_sysStartMode = REG_Sys_PressStart + 34;    // 0: 停止调压， 1: 自动调压 
        public const UInt16 REG_sysIdleValveStaus = REG_Sys_PressStart + 35;   // 0: 断电关阀口， 1：断电打开阀口
        public const UInt16 REG_sysValveType = REG_Sys_PressStart + 36;    // 阀类型：DN40,DN80,DN100，DN150, DN200,DN250
        public const UInt16 REG_sysModbusAddr = REG_Sys_PressStart + 37;    // Modbus设备地址
        public const UInt16 REG_sysModbudBaud = REG_Sys_PressStart + 38;    // Modbus波特率 

        public const UInt16 REG_sysRunKeyMode = REG_Sys_PressStart+39;     // 本机启停按键 （0解锁，1锁键）
        public const UInt16 REG_sysRunPowerMode = REG_Sys_PressStart + 40;    // 外部供电（0电源供电，1电池供电）
        public const UInt16 REG_sysRunClutchStatus = REG_Sys_PressStart + 41;     // 阀口离合状态，结合（0结合，1分离）

        public const UInt16 REG_sysSyncYear = REG_Sys_PressStart + 42;     // 时钟年BCD 
        public const UInt16 REG_sysSyncMD = REG_Sys_PressStart + 43;    // 时钟月日
        public const UInt16 REG_sysSyncWH = REG_Sys_PressStart + 44;   // 时钟周时
        public const UInt16 REG_sysSyncMS = REG_Sys_PressStart + 45;   // 时钟分秒 

        // 累计流量存储                      // 页标志
        public const UInt16 REG_FlowResvSumL = REG_Sys_PressStart + 46;        // 剩余累计流量
        public const UInt16 REG_FlowResvSumH = REG_Sys_PressStart + 47;       // 剩余累计流量
        public const UInt16 REG_FlowSumL = REG_Sys_PressStart + 48;        // 累计流量
        public const UInt16 REG_FlowSumH = REG_Sys_PressStart + 49;        // 累计流量

        // 错误存储,最多7个
        public const UInt16 REG_ErrorPtrSt = REG_Sys_PressStart + 50;        // 错误存储指针，最多存7个错误
        public const UInt16 REG_ErrorPtrEd = REG_Sys_PressStart + 51;       // 错误存储指针，最多存7个错误
        public const UInt16 REG_ErrorYyMm0 = REG_Sys_PressStart + 52;       // 错误年月BCD
        public const UInt16 REG_ErrorDdHh0 = REG_Sys_PressStart + 53;     // 错误日时BCD
        public const UInt16 REG_ErrorMmSs0 = REG_Sys_PressStart + 54;      // 错误分秒BCD
        public const UInt16 REG_ErrorType0 = REG_Sys_PressStart + 55;       // 错误类型 

        public const UInt16 REG_ErrorYyMm1 = REG_Sys_PressStart + 56;     // 错误年月BCD
        public const UInt16 REG_ErrorDdHh1 = REG_Sys_PressStart + 57;       // 错误日时BCD
        public const UInt16 REG_ErrorMmSs1 = REG_Sys_PressStart + 58;       // 错误分秒BCD
        public const UInt16 REG_ErrorType1 = REG_Sys_PressStart + 59;       // 错误类型 

        public const UInt16 REG_ErrorYyMm2 = REG_Sys_PressStart + 60;      // 错误年月BCD
        public const UInt16 REG_ErrorDdHh2 = REG_Sys_PressStart + 61;        // 错误日时BCD
        public const UInt16 REG_ErrorMmSs2 = REG_Sys_PressStart + 62;       // 错误分秒BCD
        public const UInt16 REG_ErrorType2 = REG_Sys_PressStart + 63;       // 错误类型 

        public const UInt16 REG_ErrorYyMm3 = REG_Sys_PressStart + 64;      // 错误年月BCD
        public const UInt16 REG_ErrorDdHh3 = REG_Sys_PressStart + 65;       // 错误日时BCD
        public const UInt16 REG_ErrorMmSs3 = REG_Sys_PressStart + 66;       // 错误分秒BCD
        public const UInt16 REG_ErrorType3 = REG_Sys_PressStart + 67;       // 错误类型 

        public const UInt16 REG_ErrorYyMm4 = REG_Sys_PressStart + 68;      // 错误年月BCD
        public const UInt16 REG_ErrorDdHh4 = REG_Sys_PressStart + 69;      // 错误日时BCD
        public const UInt16 REG_ErrorMmSs4 = REG_Sys_PressStart + 70;      // 错误分秒BCD
        public const UInt16 REG_ErrorType4 = REG_Sys_PressStart + 71;       // 错误类型 

        public const UInt16 REG_ErrorYyMm5 = REG_Sys_PressStart + 72;    // 错误年月BCD
        public const UInt16 REG_ErrorDdHh5 = REG_Sys_PressStart + 73;       // 错误日时BCD
        public const UInt16 REG_ErrorMmSs5 = REG_Sys_PressStart + 74;        // 错误分秒BCD
        public const UInt16 REG_ErrorType5 = REG_Sys_PressStart + 75;       // 错误类型 

        public const UInt16 REG_ErrorYyMm6 = REG_Sys_PressStart + 76;        // 错误年月BCD
        public const UInt16 REG_ErrorDdHh6 = REG_Sys_PressStart + 77;       // 错误日时BCD
        public const UInt16 REG_ErrorMmSs6 = REG_Sys_PressStart + 78;       // 错误分秒BCD
        public const UInt16 REG_ErrorType6 = REG_Sys_PressStart + 79;        // 错误类型 

        public const UInt16 REG_ErrorClear = REG_Sys_PressStart + 80;       // 清除错误  

        public const UInt16 REG_Sys_PressEnd = REG_ErrorClear + 1;


        public static void ReadPressErrorList(UInt16 len)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_ErrorPtrSt;                   // 寄存器地址， 2字节 
            UInt16 readlen = 30;                       // 读N个(数值1-125)寄存器，2字节
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }

        public static void SetPressErrorListClear(UInt16 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_ErrorClear;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 0; ;// (UInt16)Val;       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); 
        }

        public static void ReadPressDateTime()
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysSyncYear;                   // 寄存器地址， 2字节 
            UInt16 readlen = 4;                       // 读N个(数值1-125)寄存器，2字节
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }

        public static void SyncPressDeviceDateTime()
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysSyncYear;                   // 寄存器地址， 2字节 
            UInt16 writelen = 4;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            UInt16 year=0, md=0,wh=0,ms=0;
            DateTime dt = DateTime.Now;
            year = (UInt16)dt.Year;
            md = (UInt16)dt.Month;
            md = (UInt16)(md << 8);
            md = (UInt16)(md+dt.Day);

            wh = (UInt16)dt.DayOfWeek;
            wh = (UInt16)(wh << 8);
            wh = (UInt16)(wh + dt.Hour);

            ms = (UInt16)dt.Minute;
            ms = (UInt16)(ms << 8);
            ms = (UInt16)(ms + dt.Second);

            dat[0] = year;   // 写一个数据到寄存器，2字节
            dat[1] = md;     // 写一个数据到寄存器，2字节
            dat[2] = wh;     // 写一个数据到寄存器，2字节
            dat[3] = ms;     // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }


        public static void ReadPressFlowRefreshSumValue()
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_FlowResvSumL;                   // 寄存器地址， 2字节 
            UInt16 readlen = 4;                       // 读N个(数值1-125)寄存器，2字节
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }

        public static void ReadPressDeviceID()
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysDeviceID_L;                   // 寄存器地址， 2字节 
            UInt16 readlen = 12;                       // 读N个(数值1-125)寄存器，2字节
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }

        public static void ReadPressPIDParam()
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysPID_FenMu;                   // 寄存器地址， 2字节 
            UInt16 readlen = 13;                       // 读N个(数值1-125)寄存器，2字节
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }

        public static void ReadPressUserConfig()
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = REG_sysPressOutObj;                   // 寄存器地址， 2字节 
            UInt16 readlen = 7;                       // 读N个(数值1-125)寄存器，2字节
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }


        public static void SetPresRegisterValue(UInt16 Reg, UInt16 Val)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Reg;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)Val;       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

        public static void SetPresRegisterBuffer(UInt16 Reg, UInt16 []Val, UInt16 len)
        {
            if (len > 255)
                return;
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Reg;                   // 寄存器地址， 2字节 
            UInt16 writelen = len;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[256];
            for(int i=0; i< writelen; i++)
               dat[i] = Val[i];       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

    }
}
