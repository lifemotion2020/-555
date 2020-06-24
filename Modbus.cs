

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace IGPR
{
    /*
 * MODBUS协议
 * MODBUS 3.5T是如何计算的？
 * T = 1000毫秒*(1起始位+数据位+奇偶校验+停止位)/波特率

如果你的通讯方式是：波特率115200（表示每秒传输多少字符）,数据位8，无奇偶校验。
那么你发送一个字符的时间是：T=1000* (1起始位+8数据位+0奇偶校验+1停止位)/ 115200=0.087ms。

T= 1000*(1+8+0+1)/19200=0.52ms     3.5T=1.83ms=2ms    7T=4ms     

发送端：发送一帧后延时7*T（其中3.5T是停止时间，3.5T是起始时间）再发送第二帧，保证一帧数据里头各字节间间隔延时不能超过1.5T。
接收端：接收一个字节，查询2T时间，是否有接收到下一个字节，有则这帧数据未完，继续循环接收；没有则默认这帧已经接收完毕。

 * 
 * 介绍：
 * 此modbus上位机 协议类 具有较强的通用性
 * 本协议类最主要的思想是 把所有向下位机发送的指令 先存放在缓冲区中（命名为管道）
 * 再将管道中的指令逐个发送出去。
 * 管道遵守FIFO的模式。管道中所存放指令的个数 在全局变量中定义。
 * 管道内主要分为两部分：1，定时循环发送指令。2，一次性发送指令。
 * 定时循环发送指令:周期性间隔时间发送指令，一般针对“输入寄存器”或“输入线圈”等实时更新的变量。
 * 这两部分的长度由用户所添加指令个数决定（所以自由性强）。
 * 指令的最大发送次数，及管道中最大存放指令的个数在常量定义中 可进行设定。
 * 
 * 使用说明：
 * 1，首先对所定义的寄存器或线圈进行分组定义，并定义首地址。
 * 2，在MBDataTable数组中添加寄存器或线圈所对应的地址。 注意 寄存器：ob = new UInt16()。线圈：ob = new byte()。
 * 3，对所定义的地址 用属性进行定义 以方便在类外进行访问及了解所对应地址的含义。
 * 4，GetAddressValueLength函数中 对使用说明的"第一步"分组 的元素个数进行指定。
 * 5，在主程序中调用MBConfig进行协议初始化（初始化内容参考函数）。
 * 6，在串口中断函数中调用MBDataReceive()。
 * 7，定时器调用MBRefresh()。（10ms以下）
 *    指令发送间隔时间等于实时器乘以10。 例：定时器5ms调用一次  指令发送间隔为50ms。
 * 8，在主程序初始化中添加固定实时发送的指令操作 用MBAddRepeatCmd函数。
 * 9，在主程序运行过程中 根据需要添加 单个的指令操作(非固定重复发送的指令)用MBAddCmd函数。
 * 
 * 
 * 
 * 0x03 读寄存器个数
 * 设备地址  功能码  第一个地址高   第一个地址低    读寄存器个数高   读寄存器个数低   CRC高  CRC低
 *  01       03       00                00              00               01            84     0A
 * 应答
 * 设备地址  功能码  数据字节长度    数据高  数据低   CRC高  CRC低
 * 01          03         02           00      01       79     84
 * 
 * 
 * 0x10 修改多个寄存器
 * 设备地址  功能码  第一个地址高   第一个地址低    寄存器个数高   寄存器个数低  数据长度字节                CRC
 *  01         10       00             0C              00                02           04       27 10 00 01 
 * 应答
 * 01 10 00 0C 00 02 CRC
*/



    public class Modbus
    {
        #region 全局变量
        public const UInt16 REG_Sys_HardStart = 0x4200;
        public const UInt16 REG_sysValveGoDone = REG_Sys_HardStart + 0x00;    // 到达 0：阀口下限 1：零点 2：最大阀口  3： 阀口上限
        public const UInt16 REG_sysHardAutoDone = REG_Sys_HardStart + 0x01;    // 磨合状态， 0： 停止磨合  1： 启动磨合
        public const UInt16 REG_sysClutchDone = REG_Sys_HardStart + 0x02;    // 离合操作， 0： 分离  1： 结合
        public const UInt16 REG_sysMotorStepGo = REG_Sys_HardStart + 0x03;    // 阀口行进位移值。 0：停止； >0开阀  <0关阀
        public const UInt16 REG_sysValvePos = REG_Sys_HardStart + 0x04;    // 阀口光栅位置值
        public const UInt16 REG_Sys_HardEnd = 0x4210;


        public const UInt16 REG_Sys_PressStart = 0x4100;

        public const UInt16 REG_sysPID_FenMu = REG_Sys_PressStart + 0;        // PID参数的分母，缺省10000

        public const UInt16 REG_sysPID_NormKp = REG_Sys_PressStart + 1;         // 正常工作PID调压参数分子 比例  
        public const UInt16 REG_sysPID_NormKi = REG_Sys_PressStart + 2;         // 正常PID调压参数分子 微分
        public const UInt16 REG_sysPID_NormKd = REG_Sys_PressStart + 3;         // 正常PID调压参数分子 积分
        public const UInt16 REG_sysPID_NormInterval = REG_Sys_PressStart + 4;        // 正常PID调压时间间隔ms

        private static SerialPort comm = null;
        public static UInt16 devAddr;
        public static UInt16 devBaud;


        #endregion


        #region MODBUS 地址对应表
        //public static UInt16 gBaud = 19200;
        //--------------------------------------------------------------------------------------
        public static UInt16 lastSendDevAddr = 0;                        // 最后一次发送设备地址
        public static UInt16 lastSendCmd = 0;                              // 最后一次发送的命令
        public static UInt16 lastSendRegAddr = 0;                        // 最后一次发送寄存器
        public static UInt16 lastSendRegLen = 0;                         // 最后一次发送长度

        public static Byte curRecvDevAddr = 0;                        // 当前接收设备地址
        public static Byte curRecvCmd = 0;                            // 当前接收的命令
        public static UInt16 curRecvRegAddr = 0;                        // 当前接收寄存器
        public static UInt16 curRecvRegLen = 0;                         // 当前接收长度
        public static UInt16[] curRecvData = new UInt16[200];           // 当前接收长度

        // 最低200个命令发送缓冲。每个命令最大255个字节。
        public static Byte[] ModbusSendBuf = new Byte[256];
        public static Byte[,] ModbusSendArrayBuf = new Byte[200, 255];  // 命令格式：第一个字节是命令长度，不发送。
        public static UInt16 ModbusReadPtr = 0;                   // 命令读取
        public static UInt16 ModbusWritePtr = 0;                  // 命令写入

        public static UInt16 ModbusSend_ClearCommand()
        {
            ModbusReadPtr = 0;
            ModbusWritePtr = 0;
            lastSendDevAddr = 0;
            lastSendCmd = 0;
            lastSendRegAddr = 0;
            lastSendRegLen = 0;
            return 1;
        }

        public static UInt16 Modbus_Read03Command(Byte devAddr, UInt16 regAddr, UInt16 regLen)
        {
            Byte[] tmpbuf = new Byte[16];
            tmpbuf[0] = devAddr;                        // 设备地址
            tmpbuf[1] = 0x03;                          // 0x03
                                                       // 寄存器起始地址 0000-FFFF
            tmpbuf[2] = (byte)(regAddr >> 8);          // 高地址
            tmpbuf[3] = (byte)(regAddr & 0xFF);        // 低地址
                                                       // 读取N(1-125)个寄存器
            tmpbuf[4] = (byte)(regLen >> 8);          // 长度高 
            tmpbuf[5] = (byte)(regLen & 0xFF);        // 长度低 
            if (regLen > 125)
                return 0;
            Modbus.ModbusSend_WriteCommand(6, tmpbuf);
            return 1;
            // 应答：      01（1字节地址） 03（1字节功能）， 02（1字节数据长度字节）， 00 00（2字节数据）
            // 错误应答：  01（1字节地址） 83（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        public static UInt16 Modbus_Write10Command(Byte devAddr, UInt16 regAddr, UInt16 regLen, UInt16[] dat)
        {
            Byte[] tmpbuf = new Byte[255];
            UInt16 tmpdat;
            tmpbuf[0] = devAddr;                       // 设备地址 地址码， 1字节 
            tmpbuf[1] = 0x10;                          // 功能0x10， 1字节
                                                       // 寄存器地址， 2字节 
            tmpbuf[2] = (byte)(regAddr >> 8);          // 高地址
            tmpbuf[3] = (byte)(regAddr & 0xFF);        // 低地址

            UInt16 writelen = regLen;                  // 写入N个(数值1-123)寄存器，2字节
            tmpbuf[4] = (byte)(writelen >> 8);         // 长度高 
            tmpbuf[5] = (byte)(writelen & 0xFF);       // 长度低 

            writelen = (UInt16)(writelen * 2);         // 写入2*N（数值2-246）个字节， 1字节
            tmpbuf[6] = (byte)(writelen);              // 字节

            UInt16 ptr = 7;
            // 写一个数据到寄存器，2字节
            for (int i = 0; i < regLen; i++)
            {
                tmpdat = dat[i];
                tmpbuf[ptr] = (byte)(tmpdat >> 8);              // 数据高 
                ptr = (UInt16)(ptr + 1);
                tmpbuf[ptr] = (byte)(tmpdat & 0xFF);            // 数据低 
                ptr = (UInt16)(ptr + 1);
            }

            Modbus.ModbusSend_WriteCommand(ptr, tmpbuf);
            return 1;
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        public static UInt16 ModbusSend_WriteCommand(UInt16 len, Byte[] dat)
        {
            UInt16 ptr = 0;
            ptr = (UInt16)(ModbusWritePtr + 1);
            ptr = (UInt16)(ptr % 200);
            if (len > 255)
                return 2;   // over, error
            if (ModbusReadPtr == ptr)  // 数据满，不保存
                return 0;
            else
            {
                Byte[] tmpbuf = new Byte[256];
                ModbusSendArrayBuf[ptr, 0] = (Byte)(len + 2);
                for (UInt16 i = 0; i < len; i++)
                {
                    ModbusSendArrayBuf[ptr, i + 1] = dat[i];
                    tmpbuf[i] = dat[i];
                }
                int crcVal = 0;
                crcVal = Crc16(tmpbuf, len);
                
                ModbusSendArrayBuf[ptr, len + 1] = (byte)(crcVal & 0xFF);
                ModbusSendArrayBuf[ptr, len + 2] = (byte)(crcVal >> 8);
                ModbusWritePtr = ptr;
                return 1;
            }
        }

        public static UInt16 ModbusSend_ReadCommand()
        {
            if (ModbusReadPtr == ModbusWritePtr)  // 没有数据
                return 0;
            else
            {
                UInt16 ptr = 0;
                UInt16 len = 0;
                ptr = (UInt16)(ModbusReadPtr + 1);
                ptr = (UInt16)(ptr % 200);
                len = ModbusSendArrayBuf[ptr, 0];   // length
                ModbusReadPtr = ptr;  ///
                if (len > 255)
                {
                    return 3;    // 长度无效
                }
                for (UInt16 i = 0; i < len; i++)
                {
                    ModbusSendBuf[i] = ModbusSendArrayBuf[ptr, i + 1];
                }
                if (comm.IsOpen)    // 如果串口打开，发送数据，否则丢掉
                {
                    comm.Write(ModbusSendBuf, 0, len);
                    lastSendDevAddr = ModbusSendBuf[0];
                    lastSendCmd = ModbusSendBuf[1];
                    lastSendRegAddr = (UInt16)((ModbusSendBuf[2] << 8) + ModbusSendBuf[3]);
                    lastSendRegLen = (UInt16)((ModbusSendBuf[4] << 8) + ModbusSendBuf[5]);
                    return 1;    // 发送成功
                }
                else
                    return 2;   // 串口没有打开，发送数据无效
            }
        }

        #endregion

        #region 校验
        private static readonly byte[] aucCRCHi = {
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40
        };
        private static readonly byte[] aucCRCLo = {
            0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7,
            0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E,
            0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9,
            0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC,
            0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
            0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32,
            0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D,
            0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38,
            0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF,
            0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
            0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1,
            0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4,
            0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB,
            0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA,
            0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
            0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0,
            0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97,
            0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C, 0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E,
            0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89,
            0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
            0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83,
            0x41, 0x81, 0x80, 0x40
        };
        /// <summary>
        /// CRC效验
        /// </summary>
        /// <param name="pucFrame">效验数据</param>
        /// <param name="usLen">数据长度</param>
        /// <returns>效验结果</returns>
        public static int Crc16(byte[] pucFrame, int usLen)
        {
            int i = 0;
            byte ucCRCHi = 0xFF;
            byte ucCRCLo = 0xFF;
            UInt16 iIndex = 0x0000;

            while (usLen-- > 0)
            {
                iIndex = (UInt16)(ucCRCLo ^ pucFrame[i++]);
                ucCRCLo = (byte)(ucCRCHi ^ aucCRCHi[iIndex]);
                ucCRCHi = aucCRCLo[iIndex];
            }
            return (ucCRCHi << 8 | ucCRCLo);
        }

        #endregion


        /// <summary>
        /// 串口参数配置
        /// </summary>
        /// <param name="commx">所用到的串口</param>
        /// <param name="node"></param>
        /// <param name="baud"></param>
        public static void MBConfig(SerialPort commx, UInt16 node, UInt16 baud)
        {
            //gBaud = baud;
            comm = commx;
            devAddr = node;
            devBaud = baud;
        }
        
    }
}
           
       
       
