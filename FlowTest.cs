using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace IGPR
{
    class FlowTest
    {
        // 流量计算参数  1bar=100kPa=100000Pa
        // 参数说明：d： 相对密度（空气相对密度为1,天然气相对于空气的密度为0.5548），无量纲.
        //           Te：进口气体温度（单位摄氏度）
        //           Pb:当地大气压（绝对值）bar。 
        // 分量1： 密度和温度值 dt= 13.57/sqrt(d*(Te+273))      
        // 阀口开度表
        private const UInt16 FLOW_TAB_MAXLEN  = 20;         // 最大20段
        private const UInt16 VALVE_MAX_UM = 12000;      // 12mm 

        private static UInt16[] flow_um_table = 
        {                  // 阀口开度表, 假定阀口开度最大12000um
        0,               // 0mm      0%
        600,             // 0.6um    5%
        1200,            // 1.2mm    10%
        1800,            // 1.8mm    15%
        2400,            // 2.4mm    20%
        3000,            // 3.0mm    25%
        3600,            // 3.6mm    30%
        4800,            // 4.8mm    40%
        8400,            // 8.4mm    70%
        9600,            // 9.6mm    80%
        10800,           // 10.8mm   90%
        VALVE_MAX_UM,    // 12mm     100%
        VALVE_MAX_UM,    // 12mm     100%
        VALVE_MAX_UM,    // 12mm     100%
        VALVE_MAX_UM,    // 12mm     100%
        VALVE_MAX_UM,    // 12mm     100%
        VALVE_MAX_UM,    // 12mm     100%
        VALVE_MAX_UM,    // 12mm     100%
        VALVE_MAX_UM,    // 12mm     100%
        VALVE_MAX_UM     // 12mm     100%
        };
        // 不可压缩阶段流量
        private static UInt16[] flow_cv_table =
        {           // 流量系数 X10
          0,        // 0mm      0%
          4585,    // 0.6um    5%
          7500,      // 1.2mm    10%
          10170,     // 1.8mm    15%
          12810,     // 2.4mm    20%
          15480,     // 3.0mm    25%
          18280,     // 3.6mm    30%
          23780,     // 4.8mm    40%
          39530,     // 8.4mm    70%
          44720,     // 9.6mm    80%
          48960,     // 10.8mm   90%
          55640,     // 12mm     100%
          55640,     // 12mm     100%
          55640,     // 12mm     100%
          55640,     // 12mm     100%
          55640,     // 12mm     100%
          55640,     // 12mm     100%
          55640,     // 12mm     100%
          55640,     // 12mm     100%
          55640      // 12mm     100%
        };

        // 可压缩临界阶段
        private static UInt16[] flow_cg_table =
        {           // 流量系数  X10
          0,        // 0mm      0%
          2420,      // 0.6um    5%
          4440,      // 1.2mm    10%
          5820,      // 1.8mm    15%
          8070,      // 2.4mm    20%
          8860,      // 3.0mm    25%
          10550,     // 3.6mm    30%
          13680,     // 4.8mm    40%
          24500,     // 8.4mm    70%
          27780,     // 9.6mm    80%
          29500,     // 10.8mm   90%
          33970,     // 12mm     100%
          33970,     // 12mm     100%
          33970,     // 12mm     100%
          33970,     // 12mm     100%
          33970,     // 12mm     100%
          33970,     // 12mm     100%
          33970,     // 12mm     100%
          33970,     // 12mm     100%
          33970      // 12mm     100%
        };


        // 可压缩亚临界阶段
        private static UInt16[] flow_k1_table =
        {           // 流量系数 X1000
          0,        // 0mm      0%
          1760,     // 0.6um    5%
          1580,     // 1.2mm    10%
          1680,     // 1.8mm    15%
          1580,     // 2.4mm    20%
          1700,     // 3.0mm    25%
          1680,     // 3.6mm    30%
          1710,     // 4.8mm    40%
          1600,     // 8.4mm    70%
          1600,     // 9.6mm    80%
          1660,     // 10.8mm   90%
          1640,     // 12mm     100%
          1640,     // 12mm     100%
          1640,     // 12mm     100%
          1640,     // 12mm     100%
          1640,     // 12mm     100%
          1640,     // 12mm     100%
          1640,     // 12mm     100%
          1640,     // 12mm     100%
          1640      // 12mm     100%
        };

        // 根据阀口开度，计算流量系数
        private static float flow_gas_cv = 0;
        private static float flow_gas_cg = 0;
        private static float flow_gas_k1 = 0;

        // 计算流量系数
        // 入口参数： 阀口开度，单位um
        public static void flow_param_count(UInt16 valve)
        {
            float tmp_f;
            UInt16 loc;
            flow_gas_cv = 0;
            flow_gas_cg = 0;
            flow_gas_k1 = 0;
            if (valve > VALVE_MAX_UM)
                valve = VALVE_MAX_UM;
            for (loc = 0; loc < FLOW_TAB_MAXLEN; loc++)
            {
                if (valve <= flow_um_table[loc])  // 寻找最大位置
                    break;
            }
            if (loc == 0)
                return;
            tmp_f =(valve - flow_um_table[loc - 1]);
            tmp_f = tmp_f/(flow_um_table[loc] - flow_um_table[loc - 1]);
            // 不可压缩阶段流量系数
            flow_gas_cv = (float)(flow_cv_table[loc - 1] + tmp_f * (flow_cv_table[loc] - flow_cv_table[loc - 1])/10.0);
            // 可压缩临界阶段流量系数
            flow_gas_cg = (float)((flow_cg_table[loc - 1] + tmp_f * (flow_cg_table[loc] - flow_cg_table[loc - 1]))/10.0);
            // 可压缩亚临界阶段流量系数
            flow_gas_k1 = (float)(flow_k1_table[loc - 1] + tmp_f * (flow_k1_table[loc] - flow_k1_table[loc - 1])/1000.0);
        }

        // 流量通用公式 
        // 单位换算： 1bar = 100kPa=100000Pa      温度摄氏度转换整数 X100
        // 三阶段流量计算公共公式 (Pe+Pb)/2 * 13.57/sqrt(D(Te+273.15)) (压力单位bar，温度单位摄氏度)
        // 简化1： 6.785*(Pe+Pb)/sqrt(D(Te+273.15))        除以2
        // 简化2： 67.85*(Pe+Pb)/sqrt(D(Te+27315))      温度放大100倍转为整数
        // 简化3： 0.0006785*(Pe+Pb)/sqrt(D(Te+27315))  压力单位bar换算Pa
        // 简化4： 0.06785*(Pe+Pb)/sqrt(D(Te+27315))    相对密度放大10000
        public static Int32 flow_gas_D = 9999;     // 出口密度,与出口压力和温度有关。可以配置，0.5548 相对空气密度系数10000，无单位,这个密度需要及计算 
        public static Int32 flow_gas_Te = 1500;    // 入口温度， 温度放大100倍，0.01度。如1500表示15摄氏度，温度有负数    
        public static Int32 flow_gas_Pe = 200000;  // 进口相对压力，单位Pa      数值现场测量
        public static Int32 flow_gas_Pa = 2200;    // 出口相对压力，单位Pa，    数值现场测量
        public static Int32 flow_gas_Pb = 101325;  // 可以配置，当地大气绝对压力，单位Pa, 数值需要现场配置。需要计算大气压影响值

        public const UInt16 FLOW_CNT_HOUR = 36000;       // 采样计算间隔100ms， 1小时=3600×10                             
        public static float flow_gas_Qm3h = 0;           // 瞬时流量（工况流量）  m3/h
        public static float flow_gas_QNm3h = 0;          // 瞬时流量（标况流量）  Nm3/h

        public static float flow_gas_sumQm3 = 0;         // 累计流量（工况流量）  m3
        public static float flow_gas_sumQNm3 = 0;        // 累计流量（标况流量）  Nm3

        // 校准系数
        public static float flow_gasQ_Coef = 1.0F;         // 密度计算校准系数


        // 累计流量计算
        // 采样时间假定100ms T, 瞬时流量值Q, 1小时=3600秒=36000（100ms） 
        // Q/36000 累计
        // 根据进出口压力，温度，密度，计算流量通用部分
        public static void flow_count_Q(UInt16 valve)
        {
            float tmp_p, tmp_f;
            flow_param_count(valve);    // 三个阶段计算流量系数
            if (flow_gas_cv == 0)          // 阀口开度为零，流量为零
            {
                flow_gas_Qm3h = 0;
                flow_gas_QNm3h = 0;
                return;
            }
            tmp_p = flow_gas_cg * flow_gas_cg / (flow_gas_cv * flow_gas_cv);   // 临界点压差比
            flow_gas_Qm3h = (float)(0.06785 * (flow_gas_Pe + flow_gas_Pb) / Math.Sqrt((flow_gas_Te + 27315) * flow_gas_D));
            flow_gas_Qm3h = flow_gas_Qm3h * flow_gasQ_Coef;

            tmp_f = (flow_gas_Pe - flow_gas_Pa);
            tmp_f = tmp_f/(flow_gas_Pe + flow_gas_Pb);
            if (tmp_f < 0.02)                                                             // 低压差，不可压缩阶段
                flow_gas_Qm3h = (float)(flow_gas_Qm3h * flow_gas_cv * Math.Sqrt(tmp_f));  // 瞬时流量
            else
            {
                flow_gas_Qm3h = flow_gas_Qm3h * flow_gas_cg;                              // 可压缩临界阶段   瞬时流量
                if (tmp_f <= tmp_p)                                                       // 可压缩亚临界阶段  瞬时流量
                {
                    tmp_f = (float)(flow_gas_k1 * Math.Sqrt(tmp_f));                      // flow_gas_k1可以设定恒定值为1.67
                    flow_gas_Qm3h = (float)(flow_gas_Qm3h * Math.Sin(tmp_f));
                }
            }
            flow_gas_QNm3h = (float)(flow_gas_Qm3h*0.289317*(flow_gas_Pa+flow_gas_Pb)/(27315 + flow_gas_Te));  // 标况流量 
            flow_gas_sumQm3 = flow_gas_sumQm3 + flow_gas_Qm3h;      // 累计工况流量
            flow_gas_sumQNm3 = flow_gas_sumQNm3 + flow_gas_QNm3h;   // 累计标况流量
        }


        // 标况流量(Qn)：单位Nm3/h，中国标准，101.325Kpa，20摄氏度的状态流量
        // 工况流量(Qg)：单位m3/h，实际运行时的流量
        // Qn=Qg * Zn/Zg * (Pg+Pa)/Pn * Tn/Tg
        // 带n是标况参数，带g是工况参数. 
        // Z表示压缩系数，缺省为1
        // Pg表压（Pa），Pa当地大气压（Pa）
        // Pn标准大气压（101325Pa）
        // Tn国标20摄氏度下绝对温度  293.15K
        // Tg其他绝对温度， (273.15+t) K

        // Qn=Qg * Zn/Zg * (Pg+Pa)/101325 * 29315/Tg
        //   =Qg * Zn/Zg * (Pg+Pa)/Tg * 0.2893165556377992
        //   =Qg * (Pg+Pa)/Tg * 0.289317
    }
}
