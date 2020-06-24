using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace IGPR
{ 
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    struct Float_Uint16
    {
        [FieldOffset(0)]
        public byte b0;
        [FieldOffset(1)]
        public byte b1;
        [FieldOffset(2)]
        public byte b2;
        [FieldOffset(3)]
        public byte b3;

        [FieldOffset(0)]
        public float f;

        [FieldOffset(0)]
        public UInt32 i;
    };


    class DataTrans
    {

        public static Float_Uint16 press_float= new Float_Uint16();

        // 转换压力数据为16位表示。最高2位是倍数
        public static UInt16 translate_uint32_to_uint16(UInt32 dat)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = dat;
            if (val <= 16383)   // 16.383kpa
            {
                val = val + 0x0000;
            }
            else
            {
                if (val <= 163830)   // 163.83kpa
                {
                    val = val / 10;
                    val = val + 0x4000;
                }
                else
                {
                    if (val <= 1638300)   // 1638.3kpa
                    {
                        val = val / 100;
                        val = val + 0x8000;
                    }
                    else                 // 16383kpa
                    {
                        val = val / 1000;
                        val = val + 0xC000;
                    }
                }
            }
            return (UInt16)val;
        }

        // 16位数据dat_in转换实际数据dat_out表示
        public static UInt32 trans_uint16_t_to_uint32(UInt16 dat_in)
        {
            UInt32 dat_out = 0;
            if (dat_in <= 0x3FFF)
            {
                dat_out = (UInt16)(dat_in - 0x0000);
            }
            else
            {
                if (dat_in <= 0x7FFF)
                {
                    dat_out = (UInt16)(dat_in - 0x4000);
                    dat_out = dat_out * 10;
                }
                else
                {
                    if (dat_in <= 0xBFFF)
                    {
                        dat_out = (UInt16)(dat_in - 0x8000);
                        dat_out = dat_out * 100;
                    }
                    else
                    {
                        dat_out = (UInt16)(dat_in - 0xC000);  // 数据最大16383000，否则超过就是最大值
                        dat_out = dat_out * 1000;
                    }
                }
            }
            return dat_out;
        }

        // 实际正浮点数据dat_in转换16位数据dat_out表示
        public static UInt16 trans_float_to_uint16(float dat_in)
        {
            UInt16 dat_out = 0xFFFF;
            if (dat_in <= 163.83)
            {
                dat_out = (UInt16)(dat_in * 100.0 + 0.5);
                dat_out = (UInt16)(dat_out + 0x0000);
            }
            else
            {
                if (dat_in <= 1638.3)
                {
                    dat_out = (UInt16)(dat_in * 10.0 + 0.5);
                    dat_out = (UInt16)(dat_out + 0x4000);
                }
                else
                {
                    if (dat_in <= 16383)
                    {
                        dat_out = (UInt16)(dat_in + 0.5);
                        dat_out = (UInt16)(dat_out + 0x8000);
                    }
                    else
                    {
                        if (dat_in <= 163830)
                        {  // 数据最大163830，否则超过就是最大值
                            dat_out = (UInt16)(dat_in / 10.0 + 0.5);
                            dat_out = (UInt16)(dat_out + 0xC000);
                        }
                    }
                }
            }
            return dat_out;
        }

        // 16位数据dat_in转换实际正浮点数据dat_out表示
        public static float trans_uint16_to_float(UInt16 dat_in)
        {
            float dat_out = 0.0f;
            UInt16 tmp;
            if (dat_in <= 0x3FFF)
            {
                tmp = (UInt16)(dat_in - 0x0000);
                dat_out = (float)(tmp / 100.0);
            }
            else
            {
                if (dat_in <= 0x7FFF)
                {
                    tmp = (UInt16)(dat_in - 0x4000);
                    dat_out = (float)(tmp / 10.0);
                }
                else
                {
                    if (dat_in <= 0xBFFF)
                    {
                        tmp = (UInt16)(dat_in - 0x8000);
                        dat_out = (float)(tmp * 1.0);
                    }
                    else
                    {
                        tmp = (UInt16)(dat_in - 0xC000);  // 数据最大163830，否则超过就是最大值
                        dat_out = (float)(tmp * 10.0);
                    }
                }
            }
            return dat_out;
        }

    }
}
