using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace IGPR
{
   
    public partial class MainForm : Form
    {
        //private System.IO.FileStream fInput;

        public MainForm()
        {
            InitializeComponent();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////// 
        private const int microsensor_recv_bufLen = 1200;
        private Byte[] microsensor_recvBuf = new Byte[sensor_recv_bufLen];
        private Byte[] microsensor_pack_recv = new Byte[128];


        private int sensor_recv_head_ptr=0;
        private int sensor_recv_end_ptr = 0;
        private int sensor_recv_temp_ptr;

        private const int sensor_recv_bufLen = 1200;
        private Byte[] sensor_recvBuf = new Byte[sensor_recv_bufLen];
        private Byte[] sensor_pack_recv = new Byte[128];

       
        private Byte sensor_update = 0;
        private Byte sensor_updateInfo = 0;

        private UInt32 sensor_InRange=0;
        private UInt32 sensor_OutRange = 0;

        private UInt32 sensor_InPress = 0;
        private UInt32 sensor_OutPress = 0;
        private UInt32 sensor_BackPress = 0;

        private UInt32 sensor_CutPress = 0;
        private UInt32 sensor_CutBackPress = 0;

        private UInt32 sensor_StandPress = 0;

        private float sensor_InTemp = 0;
        private float sensor_OutTemp = 0;

        private Byte sensor_Gas = 0;
        private Byte sensor_Status = 0;

        private UInt32 sensor_InZero = 0;
        private UInt32 sensor_OutZero = 0;
        private UInt32 sensor_BackZero = 0;
        private UInt32 sensor_DefaultPa = 0;
        private UInt32 sensor_NTC1_Adc = 0;
        private UInt32 sensor_NTC2_Adc = 0;
        /// <summary>
        /// 波特率 9600，8位数据，odd校验
        /// 流量计发送命令  01 03 00 00 00 10 44 06
        /// 应答37字节： 01 03 20 8个字节工况累计流量， 8个字节标况累计流量，
        ///        4个字节工况流量，4个字节标况流量，4个字节温度，4个字节压力，2个字节校验码
        /// </summary>
        // public byte[] flowrecvBuf = new byte[100];
        ///public int flowrecvLen = 0;
        public float flowNm3 = 0.0f;             // 标况瞬间流量
        public float flowLastNm3 = 0.0f;         // 标况瞬间流量上一个数据
        public float flowSumNm3 = 0.0f;          // 标况累计流量
        public float flowTemperature = 0.0f;     // 温度
        public float flowPressure = 0.0f;        // 压力

        private byte[] flowsendbuffer = { 0x17, 0x03, 0x00, 0x0D, 0x00, 0x10, 0xD7, 0x33 };
        // 0x01, 0x03, 0x00, 0x00, 0x00, 0x10, 0x44, 0x06
        // 0x17, 0x03, 0x00, 0x0D, 0x00, 0x10, 0xD7, 0x33    EVC300 协议
        /// <summary>
        /// 波特率 9600，8位数据，odd校验
        /// 流量计发送命令  01 03 00 00 00 10 44 06
        /// 应答37字节： 01 03 20 8个字节工况累计流量， 8个字节标况累计流量，
        ///        4个字节工况流量，4个字节标况流量，4个字节温度，4个字节压力，2个字节校验码
        /// </summary>
        public float smartNm3 = 0.0f;                // 智能标况瞬间流量
        public float smartLastNm3 = 0.0f;            // 智能标况瞬间流量上一个数据
        public float smartSumNm3 = 0.0f;             // 智能标况累计流量
        public float smartTemperature = 0.0f;        // 智能温度
        public float smartInPressure = 0.0f;         // 智能压力
        public float smartOutPressure = 0.0f;        // 智能压力
        public float smartValvePer = 0.0f;           // 阀开口度

                                          // 0x01, 0x03, 0x00, 0x00, 0x00, 0x15, 0x44, 0x06
        private byte[] smartsendbuffer = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x12, 0xC5, 0xC7 };


        ///public int linerefreshCnt=0;
        public float smartSumNmZero = 0.0f;          // 智能标况累计流量
        public float flowSumNmZero = 0.0f;          // 标况累计流量

        public int smartSumUpdate = 0;                 // 智能数据更新
        public int flowSumUpdate = 0;                  // 流量数据更新

        public float smartLineSumNmZero = 0.0f;          // 智能标况累计流量
        public float flowLineSumNmZero = 0.0f;          // 标况累计流量

        ///private const int LineRefreshZero = 800;    // 


        private void MainForm_Shown(object sender, EventArgs e)
        { 
            String[] array= System.IO.Ports.SerialPort.GetPortNames();
            combFlowUartList.Items.Clear();
            combPressUartList.Items.Clear();
            combSensorUartList.Items.Clear();
            combMotorUartList.Items.Clear();

            combFlowBaudList.SelectedIndex = 2;
            combPressBaudList.SelectedIndex = 2;
            combSensorBaudList.SelectedIndex = 4;
            combMotorBaudList.SelectedIndex = 2;

            combFlowCheckList.SelectedIndex = 1;
            combPressCheckList.SelectedIndex = 2;
            combSensorCheckList.SelectedIndex = 0;
            combMotorCheckList.SelectedIndex = 2;

            for (int i = 0; i < array.Length; i++)
            {
                combFlowUartList.Items.Add(array[i]);
                combPressUartList.Items.Add(array[i]);
                combSensorUartList.Items.Add(array[i]);
                combMotorUartList.Items.Add(array[i]);
            }
            if (array.Length > 0)
            {
                combFlowUartList.SelectedIndex = 0;
                combPressUartList.SelectedIndex = 0;
                combSensorUartList.SelectedIndex = 0;
                combMotorUartList.SelectedIndex = 0;

                btnCloseFlowUart.Enabled = false;
                btnOpenFlowUart.Enabled = true;
                btnRefreshFlowUart.Enabled = true;

                btnClosePressUart.Enabled = false;
                btnOpenPressUart.Enabled = true;
                btnRefreshPressUart.Enabled = true;

                btnCloseSensorUart.Enabled = false;
                btnOpenSensorUart.Enabled = true;
                btnRefreshSensorUart.Enabled = true;

                btnCloseMotorUart.Enabled = false;
                btnOpenMotorUart.Enabled = true;
                btnRefreshMotorUart.Enabled = true;
            }
            else
            {
                btnCloseFlowUart.Enabled = false;
                btnOpenFlowUart.Enabled = false;
                btnRefreshFlowUart.Enabled = true;

                btnClosePressUart.Enabled = false;
                btnOpenPressUart.Enabled = false;
                btnRefreshPressUart.Enabled = true;

                btnCloseSensorUart.Enabled = false;
                btnOpenSensorUart.Enabled = false;
                btnRefreshSensorUart.Enabled = true;

                btnCloseMotorUart.Enabled = false;
                btnOpenMotorUart.Enabled = false;
                btnRefreshMotorUart.Enabled = true;
            }
            chartOnline.Series[0].Points.Clear();
            chartOnline.Series[1].Points.Clear();
            chartOnline.Series[2].Points.Clear();
            chartOnline.Series[3].Points.Clear();
            chartOnline.Series[4].Points.Clear();

            chartSensorOnline.Series[0].Points.Clear();
            chartSensorOnline.Series[1].Points.Clear();
            chartSensorOnline.Series[2].Points.Clear();
            
            for (int i = 0; i < 1; i++)
            {
                chartOnline.Series[0].Points.AddY(0);
                chartOnline.Series[1].Points.AddY(0);
                chartOnline.Series[2].Points.AddY(0);
                chartOnline.Series[3].Points.AddY(0);
                chartOnline.Series[4].Points.AddY(0);

                chartSensorOnline.Series[0].Points.AddY(0);
                chartSensorOnline.Series[1].Points.AddY(0);
                chartSensorOnline.Series[2].Points.AddY(0);
            }

            chartOnline.ChartAreas[0].AxisY.Maximum = 4000;// 4000;
            chartOnline.ChartAreas[0].AxisY2.Maximum = 150;

            chartSensorOnline.ChartAreas[0].AxisY.Maximum = 200000;// 4000;
            chartSensorOnline.ChartAreas[0].AxisY2.Maximum = 2000;

            btnEnableOnline.Tag = 0;
            btnEnableOnline.Text = "打开监测";

            combModbusList.SelectedIndex = 0;
            btnModbusUserChange.Enabled = false;
            btnModbusAdminChange.Enabled = false;

            tabPage12.Parent = null;
        }

       

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPortPress.IsOpen)
            {
                try
                {
                    serialPortPress.Close();
                }
                catch (Exception bpe)
                {
                    
                }
            }
            if (FlowPort.IsOpen)
            {
                try
                {
                    FlowPort.Close();
                }
                catch (Exception bpe)
                {
                    
                }
            }
        }

        private void updateNumObject(object sender, EventArgs e)
        {
            UInt32 tmp32;
            UInt16 tmp16;
            float tmpf;
            UInt16 regtmp = Modbus.curRecvRegAddr;
            UInt16 val = 0;
            if (Modbus.curRecvRegAddr == 0x9000 && Modbus.curRecvRegLen == 17)
            {
                tmp16 = Modbus.curRecvData[0];
                tbSenGas.Text = (tmp16 / 256).ToString();
                tbSenControl.Text = (tmp16 % 256).ToString();
                tmp32 = trans_uint16_t_to_uint32(Modbus.curRecvData[1]);
                tbSenInRange.Text = (tmp32 / 1000).ToString();
                tmp32 = trans_uint16_t_to_uint32(Modbus.curRecvData[2]);
                tbSenOutRange.Text = (tmp32 / 1000).ToString();

                tmp32 = trans_uint16_t_to_uint32(Modbus.curRecvData[3]);
                tmpf = tmp32 / 1000.0f;
                tbSenDefaultPress.Text = string.Format("{0:F3}", tmpf);

                tmp16 = Modbus.curRecvData[4];
                tbSenInPressZero.Text = tmp16.ToString();
                tmp16 = Modbus.curRecvData[5];
                tbSenOutPressZero.Text = tmp16.ToString();
                tmp16 = Modbus.curRecvData[6];
                tbSenBackPressZero.Text = tmp16.ToString();

                tmp16 = Modbus.curRecvData[7];
                tbSenADC_NTC1.Text = tmp16.ToString();
                tmp16 = Modbus.curRecvData[8];
                tbSenADC_NTC2.Text = tmp16.ToString();

                tmp32 = trans_uint16_t_to_uint32(Modbus.curRecvData[9]);
                tmpf = tmp32 / 1000.0f;
                tbSenInPress.Text = string.Format("{0:F3}", tmpf);

                tmp32 = trans_uint16_t_to_uint32(Modbus.curRecvData[10]);
                tmpf = tmp32 / 1000.0f;
                tbSenOutPress.Text = string.Format("{0:F3}", tmpf);

                tmp32 = trans_uint16_t_to_uint32(Modbus.curRecvData[11]);
                tmpf = tmp32 / 1000.0f;
                tbSenBackPress.Text = string.Format("{0:F3}", tmpf);

                tmp32 = trans_uint16_t_to_uint32(Modbus.curRecvData[12]);
                tmpf = tmp32 / 1000.0f;
                tbSenCutOutPress.Text = string.Format("{0:F3}", tmpf);

                tmp32 = trans_uint16_t_to_uint32(Modbus.curRecvData[13]);
                tmpf = tmp32 / 1000.0f;
                tbSenCutBackPress.Text = string.Format("{0:F3}", tmpf);

                tmp32 = trans_uint16_t_to_uint32(Modbus.curRecvData[14]);
                tmpf = tmp32 / 1000.0f; 
                tbSenStandardPress.Text = string.Format("{0:F3}", tmpf);

                tmpf = trans_uint16_to_float(Modbus.curRecvData[15]);
                tmpf = tmpf-100.0f;    
                tbSenInTemp.Text = string.Format("{0:F2}", tmpf);

                tmpf = trans_uint16_to_float(Modbus.curRecvData[16]);
                tmpf = tmpf-100.0f;
                tbSenOutTemp.Text = string.Format("{0:F2}", tmpf);
                return;
            }

            for (int i=0; i<Modbus.curRecvRegLen; i++)
            {
                val = Modbus.curRecvData[i];
                switch (regtmp)
                {
                    default:
                        break;
                }
                regtmp = (UInt16)(regtmp + 1);
            }
        }













        private List<byte> xgj_motorfrecv_buffer = new List<byte>(1024);
        private byte[] xgj_motortemp_buf = new byte[256];

        private List<byte> xgj_frecv_buffer = new List<byte>(1024);
        private byte[] xgj_temp_buf = new byte[256];

        private List<byte> frecv_buffer = new List<byte>(1024);
        private byte[] temp_buf = new byte[256];
        private byte flow_id, flow_cmd;
        private UInt16 flow_regaddr,flow_reglen;

        private void FlowPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // 流量计数据获取   9600,8/odd   37个字节  40ms
            byte cmd = 0;
            if (FlowPort == null)                       // 如果串口没有被初始化直接退出
                return;
            int byteNum = FlowPort.BytesToRead;
            byte[] buftmp = new byte[byteNum];
            FlowPort.Read(buftmp, 0, byteNum);          // 读到在数据存储到buf
            frecv_buffer.AddRange(buftmp);
            int len = 0;
            int crcVal = 0;
            UInt16 tmpcrc;
            UInt64 tmp64;
            UInt16 tmp16;
            UInt16 flag = 0;
            while (frecv_buffer.Count >= 5) //至少包含addr、cmd、len/error crc crc
            {
                cmd = frecv_buffer[1];   // 读取命令
                switch (cmd)
                {
                    case 0x03:  //read
                        len = frecv_buffer[2];
                        if(len>=0x40)   // 数据长度不能够太长
                        {
                            frecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                        }
                        else
                        {
                            if (frecv_buffer.Count >= (len + 5))
                            {
                                frecv_buffer.CopyTo(0, temp_buf, 0, len + 5);
                                crcVal = Modbus.Crc16(temp_buf, len + 3);
                                tmpcrc = (UInt16)(temp_buf[len + 4] << 8);
                                tmpcrc = (UInt16)(tmpcrc + temp_buf[len + 3]);
                                if (crcVal == tmpcrc)   // 校验和正确
                                {
                                    if (flow_cmd == 0x03 && flow_regaddr == 0x0000 && flow_reglen == 0x10) // 命令，地址，长度
                                    {
                                        tmp64 = (UInt64)((temp_buf[11] << 16) + (temp_buf[12] << 8) + (temp_buf[13])); // 11 6字节/2字节 标况累计
                                        tmp64 = tmp64 << 24;
                                        tmp64 = tmp64 + (UInt64)((temp_buf[14] << 16) + (temp_buf[15] << 8) + temp_buf[16]);
                                        tmp16 = (UInt16)((temp_buf[17] << 8) + temp_buf[18]);
                                        flowSumNm3 = (float)(tmp64 + tmp16 / 65536.0);

                                        // 19 3字节/1字节 工况
                                        tmp64 = (UInt32)((temp_buf[23] << 16) + (temp_buf[24] << 8) + temp_buf[25]);   // 23 3字节/1字节 标况
                                        tmp16 = (UInt16)temp_buf[26];
                                        flowLastNm3 = flowNm3;
                                        flowNm3 = (float)(tmp64 * 1.0 + tmp16 / 256.0);

                                        tmp64 = (UInt32)((temp_buf[27] << 16) + (temp_buf[28] << 8) + temp_buf[29]);   // 27 3字节/1字节 温度
                                        tmp16 = (UInt16)temp_buf[30];
                                        flowTemperature = (float)(tmp64 * 1.0 + tmp16 / 256.0);

                                        tmp64 = (UInt32)((temp_buf[31] << 16) + (temp_buf[32] << 8) + temp_buf[33]);   // 31 3字节/1字节 压力
                                        tmp16 = (UInt16)temp_buf[34];
                                        flowPressure = (float)(tmp64 * 1.0 + tmp16 / 256.0);
                                    }
                                    // EVC300 协议
                                    byte[] tmpbyte=new byte[8];
                                    if (flow_cmd == 0x03 && flow_regaddr == 0x000D && flow_reglen == 0x10) // 命令，地址，长度
                                    {
                                        // 标况累计  0-8字节
                                        //tmp64 = (UInt64)((temp_buf[11] << 16) + (temp_buf[12] << 8) + (temp_buf[13])); // 11 6字节/2字节 标况累计
                                        //tmp64 = tmp64 << 24;
                                        //tmp64 = tmp64 + (UInt64)((temp_buf[14] << 16) + (temp_buf[15] << 8) + temp_buf[16]);
                                        //tmp16 = (UInt16)((temp_buf[17] << 8) + temp_buf[18]);
                                        tmpbyte[7] = temp_buf[3];
                                        tmpbyte[6] = temp_buf[4];
                                        tmpbyte[5] = temp_buf[5];
                                        tmpbyte[4] = temp_buf[6];
                                        tmpbyte[3] = temp_buf[7];
                                        tmpbyte[2] = temp_buf[8];
                                        tmpbyte[1] = temp_buf[9];
                                        tmpbyte[0] = temp_buf[10];
                                        flowSumNm3 = (float)BitConverter.ToDouble(tmpbyte, 0);

                                        // 标况瞬间 0-4
                                        // 19 3字节/1字节 工况
                                        //tmp64 = (UInt32)((temp_buf[23] << 16) + (temp_buf[24] << 8) + temp_buf[25]);   // 23 3字节/1字节 标况
                                        //tmp16 = (UInt16)temp_buf[26];
                                        //flowLastNm3 = flowNm3;
                                        tmpbyte[3] = temp_buf[19];
                                        tmpbyte[2] = temp_buf[20];
                                        tmpbyte[1] = temp_buf[21];
                                        tmpbyte[0] = temp_buf[22];
                                        flowNm3 = BitConverter.ToSingle(tmpbyte, 0);

                                        // 四字节浮点
                                        //tmp64 = (UInt32)((temp_buf[27] << 16) + (temp_buf[28] << 8) + temp_buf[29]);   // 27 3字节/1字节 温度
                                        //tmp16 = (UInt16)temp_buf[30];
                                        tmpbyte[3] = temp_buf[27];
                                        tmpbyte[2] = temp_buf[28];
                                        tmpbyte[1] = temp_buf[29];
                                        tmpbyte[0] = temp_buf[30];
                                        flowTemperature = BitConverter.ToSingle(tmpbyte, 0);

                                        // 四字节浮点
                                        //tmp64 = (UInt32)((temp_buf[31] << 16) + (temp_buf[32] << 8) + temp_buf[33]);   // 31 3字节/1字节 压力
                                        //tmp16 = (UInt16)temp_buf[34];
                                        tmpbyte[3] = temp_buf[31];
                                        tmpbyte[2] = temp_buf[32];
                                        tmpbyte[1] = temp_buf[33];
                                        tmpbyte[0] = temp_buf[34];
                                        flowPressure = BitConverter.ToSingle(tmpbyte, 0);
                                    }

                                    frecv_buffer.RemoveRange(0, len + 5); // 删除整个数据包
                                }
                                else
                                    frecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                            }
                            else
                                flag = 1;
                        }
                        
                        break;
                    case 0x10: //write
                        len = 3;
                        if (frecv_buffer.Count >= 8)
                        {
                            frecv_buffer.CopyTo(0, temp_buf, 0, len + 5);
                            crcVal = Modbus.Crc16(temp_buf, len + 3);
                            tmpcrc = (UInt16)(temp_buf[len + 4] << 8);
                            tmpcrc = (UInt16)(tmpcrc + temp_buf[len + 3]);
                            if (crcVal == tmpcrc)   // 校验和正确
                            {
                                frecv_buffer.RemoveRange(0, len + 5); // 删除整个数据包
                            }
                            else
                                frecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                        }
                        else
                            flag = 1;
                        break;
                    case 0x83:  // addr, 81 error, crc crc
                        len = 0;
                        frecv_buffer.CopyTo(0, temp_buf, 0, len + 5);
                        crcVal = Modbus.Crc16(temp_buf, len + 3);
                        tmpcrc = (UInt16)(temp_buf[len + 4] << 8);
                        tmpcrc = (UInt16)(tmpcrc + temp_buf[len + 3]);
                        if (crcVal == tmpcrc)   // 校验和正确
                        {
                            frecv_buffer.RemoveRange(0, len + 5); // 删除整个数据包
                        }
                        else
                            frecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                        break;
                    case 0x90:  // addr, 81 error, crc crc
                        len = 0;
                        frecv_buffer.CopyTo(0, temp_buf, 0, len + 5);
                        crcVal = Modbus.Crc16(temp_buf, len + 3);
                        tmpcrc = (UInt16)(temp_buf[len + 4] << 8);
                        tmpcrc = (UInt16)(tmpcrc + temp_buf[len + 3]);
                        if (crcVal == tmpcrc)   // 校验和正确
                        {
                            frecv_buffer.RemoveRange(0, len + 5); // 删除整个数据包
                        }
                        else
                            frecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                        break;
                    default:
                        frecv_buffer.RemoveAt(0);
                        break;
                }
                if (flag == 1)
                    break;
            }
        }

        private void flow_timer_Tick(object sender, EventArgs e)
        {
            float tmp,tmpf1,tmpf2;
            int tmpi;
            if(FlowPort.IsOpen)  // 
            {
                // 01 03 00 00 00 10 44 06
                FlowPort.Write(flowsendbuffer, 0, 8);
                flow_id = flowsendbuffer[0];
                flow_cmd = flowsendbuffer[1];
                flow_regaddr = (UInt16)((flowsendbuffer[2]<<8)+ flowsendbuffer[3]);
                flow_reglen = (UInt16)((flowsendbuffer[4] << 8) + flowsendbuffer[5]);
                tBFlowNQ.Text = flowNm3.ToString();
                tBFlowSumNQ.Text = flowSumNm3.ToString();
                tBFlowPress.Text = flowPressure.ToString();
                tBFlowTemp.Text = flowTemperature.ToString();
                tmp = flowSumNm3 - flowSumNmZero;
                tBFlowSumZero.Text = string.Format("{0:f4}", tmp);  ///tmp.ToString();
                flowSumUpdate = 1;
               
            }
            if(serialPortPress.IsOpen)
            {
                tmpi = (int)btnEnableOnline.Tag;
                if (tmpi==1)
                {
                    serialPortPress.Write(smartsendbuffer, 0, 8);

                    Modbus.lastSendRegAddr = (UInt16)((smartsendbuffer[2] << 8) + smartsendbuffer[3]);
                    Modbus.lastSendRegLen = (UInt16)((smartsendbuffer[4] << 8) + smartsendbuffer[5]);
                }
                tBSmartNm.Text = smartNm3.ToString();
                tBSmartSumNm.Text = smartSumNm3.ToString();
                tBSmartInPress.Text = smartInPressure.ToString();
                tBSmartOutPress.Text = smartOutPressure.ToString();
                tBSmartTemp.Text = smartTemperature.ToString();
                tBSmartFaKou.Text = smartValvePer.ToString() + "%";
                tmp = smartSumNm3 - smartSumNmZero;
                tBsmartFlowSumZero.Text = string.Format("{0:f4}", tmp); // tmp.ToString();  
                smartSumUpdate=1;
            
            }
        
            if (flowSumUpdate == 1 && smartSumUpdate==1)
            {
                tmpf1 = smartSumNm3 - smartSumNmZero;
                tmpf2 = flowSumNm3 - flowSumNmZero;
                if (tmpf1 == 0)
                {
                    tmpf1 = 0;
                    tBSumPerc.Text = "NULL";
                }
                else
                {
                    tmpf1 = tmpf2 / tmpf1;
                    tmpf1 = (float)(tmpf1 - 1.0);
                    tBSumPerc.Text = string.Format("{0:F2}", tmpf1) + "%";
                }
                flowSumUpdate = 0;
                smartSumUpdate = 0;
            }
            if (FlowPort.IsOpen || serialPortPress.IsOpen)
            {
                if (chartOnline.Series[0].Points.Count > 400)
                {
                    chartOnline.Series[0].Points.RemoveAt(0);   // MotADC
                    chartOnline.Series[1].Points.RemoveAt(0);   // LuxADC
                    chartOnline.Series[2].Points.RemoveAt(0);
                    chartOnline.Series[3].Points.RemoveAt(0);
                    chartOnline.Series[4].Points.RemoveAt(0);
                }

                chartOnline.Series[0].Points.AddY(flowNm3);
                chartOnline.Series[1].Points.AddY(smartNm3);

                tmp = flowSumNm3+1- flowLineSumNmZero;
                chartOnline.Series[2].Points.AddY(tmp);
                tmp = smartSumNm3+1- smartLineSumNmZero;
                chartOnline.Series[3].Points.AddY(tmp);
                chartOnline.Series[4].Points.AddY(smartOutPressure*200);
            }
        }

        private UInt16 DecToBCD(int Dec, int loc)
        {
            UInt16 tmp16=0;
            int tmp;
            byte[] Bcd = new byte[8];
            for (int i = 7; i >= 0; i--)
            {
                tmp = Dec % 100;
                Bcd[i] = (byte)(((tmp / 10) << 4) + ((tmp % 10) & 0x0F));
                Dec /= 100;
            }
            if(loc==0)
            {
                tmp16 = (UInt16)((Bcd[loc*2] << 8) + Bcd[loc*2+1]);
            }
            if (loc == 1)
            {
                tmp16 = (UInt16)((Bcd[loc*2] << 8) + Bcd[loc*2+1]);
            }
            if (loc == 2)
            {
                tmp16 = (UInt16)((Bcd[loc*2] << 8) + Bcd[loc*2+1]);
            }
            if (loc == 3)
            {
                tmp16 = (UInt16)((Bcd[loc * 2] << 8) + Bcd[loc * 2 + 1]);
            }
            return tmp16;
        }

      
       

        private void btnClearSumNqZero_Click(object sender, EventArgs e)
        {
            smartLineSumNmZero = smartSumNm3;
            flowLineSumNmZero = flowSumNm3;
        }

        public static byte ConvertBCDToInt(byte b)
        {
            //高四位  
            byte b1 = (byte)((b >> 4) & 0xF);
            //低四位  
            byte b2 = (byte)(b & 0xF);

            return (byte)(b1 * 10 + b2);

        }


        
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte cmd = 0;
            int valPtr;
            if (serialPortPress == null)                       // 如果串口没有被初始化直接退出
                return;
            int byteNum = serialPortPress.BytesToRead;
            byte[] buftmp = new byte[byteNum];
            serialPortPress.Read(buftmp, 0, byteNum);          // 读到在数据存储到buf
            xgj_frecv_buffer.AddRange(buftmp);
            int len;
            int crcVal;
            UInt16 tmpcrc;
            UInt16 val;
            Int32 tmp32;
            Int64 tmp64;
            Int32 tmpdot;
            //Int16 tmperature;
            UInt16 recv_flag = 0;
            while (xgj_frecv_buffer.Count >= 5) //至少包含addr、cmd、len/error crc crc
            {
                cmd = xgj_frecv_buffer[1];   // 读取命令
                switch (cmd)
                {
                    case 0x03:  //read
                        len = xgj_frecv_buffer[2];
                        if(len>0x40 || len==0)
                        {
                            xgj_frecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                        }
                        else
                        {
                            if (xgj_frecv_buffer.Count >= (len + 5))
                            {
                                xgj_frecv_buffer.CopyTo(0, xgj_temp_buf, 0, len + 5);
                                crcVal = Modbus.Crc16(xgj_temp_buf, len + 3);
                                tmpcrc = (UInt16)(xgj_temp_buf[len + 4] << 8);
                                tmpcrc = (UInt16)(tmpcrc + xgj_temp_buf[len + 3]);
                                if (crcVal == tmpcrc)   // 校验和正确
                                {
                                    ///System.Diagnostics.Debug.WriteLine("crc ok");

                                    Modbus.curRecvDevAddr = xgj_temp_buf[0];
                                    Modbus.curRecvCmd = xgj_temp_buf[1];
                                    Modbus.curRecvRegAddr = Modbus.lastSendRegAddr;
                                    Modbus.curRecvRegLen = (UInt16)(xgj_temp_buf[2] / 2);   //bytes length
                                    valPtr = 0;
                                    for (int i = 0; i < Modbus.curRecvRegLen; i++)
                                    {
                                        val = (UInt16)(xgj_temp_buf[valPtr + 3] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + xgj_temp_buf[valPtr + 4]);
                                        Modbus.curRecvData[i] = val;
                                        valPtr = (UInt16)(valPtr + 2); 
                                    }
                                    
                                    if (Modbus.curRecvRegAddr >= 0x5000)
                                        this.Invoke(new EventHandler(updateNumObject));

                                    if (Modbus.curRecvRegAddr >= Flowmodbus.REG_Sys_FlowStart && Modbus.curRecvRegAddr<=Flowmodbus.REG_Sys_FlowEnd)
                                        this.Invoke(new EventHandler(DeviceModbus_UpdateFlow_Info_To_Text));

                                    if (Modbus.curRecvRegAddr >= Modbus.REG_Sys_HardStart && Modbus.curRecvRegAddr <= Modbus.REG_Sys_HardEnd)
                                        this.Invoke(new EventHandler(DeviceModbus_UpdateTestMode_Info_To_Text));

                                    if (Modbus.curRecvRegAddr >= PressPIDModus.REG_Sys_PressStart && Modbus.curRecvRegAddr <= PressPIDModus.REG_Sys_PressEnd)
                                        this.Invoke(new EventHandler(DeviceModbus_UpdatePressPID_Info_To_Text));

                                    if (Modbus.curRecvRegAddr == 0x0000 && Modbus.curRecvRegLen == 0x12)
                                    {
                                        smartInPressure = (float)(Modbus.curRecvData[0x00] / 10.0);      // 进口压力   kPa 1位小数
                                        smartOutPressure = (float)(Modbus.curRecvData[0x01] / 100.0);    // 出口压力   kPa  2位小数
                                        smartValvePer = (float)(Modbus.curRecvData[0x02] / 100.0);       // 阀开口度   %  2位小数

                                        tmpdot = Modbus.curRecvData[3] >> 8;
                                        tmp64 = (Modbus.curRecvData[3] & 0x0FF) * 65536;
                                        tmp64 = tmp64 + Modbus.curRecvData[4];
                                        smartLastNm3 = smartNm3;
                                        smartNm3 = (float)(tmp64 / 1000.0);            // 智能标况瞬间流量 3位小数

                                        tmpdot = Modbus.curRecvData[0x0F] >> 8;         // 小数位
                                        tmp32 = ConvertBCDToInt((byte)(Modbus.curRecvData[0x0F] & 0x0FF));
                                        tmp64 = tmp32 * 100000000;

                                        tmp32 = ConvertBCDToInt((byte)((Modbus.curRecvData[0x10] >> 8) & 0x0FF));
                                        tmp64 = tmp64 + tmp32 * 1000000;

                                        tmp32 = ConvertBCDToInt((byte)(Modbus.curRecvData[0x10] & 0x0FF));
                                        tmp64 = tmp64 + tmp32 * 10000;

                                        tmp32 = ConvertBCDToInt((byte)((Modbus.curRecvData[0x11] >> 8) & 0x0FF));
                                        tmp64 = tmp64 + tmp32 * 100;

                                        tmp32 = ConvertBCDToInt((byte)(Modbus.curRecvData[0x11] & 0x0FF));
                                        tmp64 = tmp64 + tmp32 * 1;

                                        smartSumNm3 = (float)(tmp64 / 10.0);          // 智能标况累计流量

                                        tmp32 = (Int16)Modbus.curRecvData[5];
                                        if (tmp32 >=0x8000)
                                        {
                                            tmp32 = (Int16)(32768 - tmp32);
                                            smartTemperature = (float)(tmp32 / -10.0);    // 智能温度
                                        }
                                        else
                                            smartTemperature = (float)(tmp32 / 10.0);    // 智能温度
                                    }
                                    xgj_frecv_buffer.RemoveRange(0, len + 5); // 删除整个数据包
                                }
                                else
                                   xgj_frecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                            }
                            else
                                recv_flag = 1;
                        }
                        break;
                    case 0x10: //write
                        len = 3;
                        if (xgj_frecv_buffer.Count >= 8)
                        {
                            xgj_frecv_buffer.CopyTo(0, xgj_temp_buf, 0, len + 5);
                            crcVal = Modbus.Crc16(xgj_temp_buf, len + 3);
                            tmpcrc = (UInt16)(xgj_temp_buf[len + 4] << 8);
                            tmpcrc = (UInt16)(tmpcrc + xgj_temp_buf[len + 3]);
                            if (crcVal == tmpcrc)   // 校验和正确
                            {
                                Modbus.curRecvDevAddr = xgj_temp_buf[0];
                                Modbus.curRecvCmd = xgj_temp_buf[1];
                                Modbus.curRecvRegAddr = (UInt16)(xgj_temp_buf[2] << 8 + xgj_temp_buf[3]);
                                xgj_frecv_buffer.RemoveRange(0, len + 5); // 删除整个数据包
                            }
                            else
                                xgj_frecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                        }
                        else
                            recv_flag = 1;
                        
                        break;
                    case 0x83:  // addr, 81 error, crc crc
                        len = 0;
                        xgj_frecv_buffer.CopyTo(0, xgj_temp_buf, 0, len + 5);
                        crcVal = Modbus.Crc16(xgj_temp_buf, len + 3);
                        tmpcrc = (UInt16)(xgj_temp_buf[len + 4] << 8);
                        tmpcrc = (UInt16)(tmpcrc + xgj_temp_buf[len + 3]);
                        if (crcVal == tmpcrc)   // 校验和正确
                        {
                            Modbus.curRecvDevAddr = xgj_temp_buf[0];
                            Modbus.curRecvCmd = (byte)(xgj_temp_buf[1] - 0x80);
                            xgj_frecv_buffer.RemoveRange(0, len + 5); // 删除整个数据包
                        }
                        else
                            xgj_frecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                        break;
                    case 0x90:  // addr, 81 error, crc crc
                        len = 0;
                        xgj_frecv_buffer.CopyTo(0, xgj_temp_buf, 0, len + 5);
                        crcVal = Modbus.Crc16(xgj_temp_buf, len + 3);
                        tmpcrc = (UInt16)(xgj_temp_buf[len + 4] << 8);
                        tmpcrc = (UInt16)(tmpcrc + xgj_temp_buf[len + 3]);
                        if (crcVal == tmpcrc)   // 校验和正确
                        {
                            Modbus.curRecvDevAddr = xgj_temp_buf[0];
                            Modbus.curRecvCmd = (byte)(xgj_temp_buf[1] - 0x80);
                            xgj_frecv_buffer.RemoveRange(0, len + 5); // 删除整个数据包
                        }
                        else
                            xgj_frecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                        break;
                    default:
                        xgj_frecv_buffer.RemoveAt(0);
                        break;
                }
                if (recv_flag == 1)
                    break;
            }
        }

        private void btnZeroLine_Click(object sender, EventArgs e)
        {
            /*Byte devaddr = 0x01;                            // 设备地址
            UInt16 regAddr = Modbus.REG_LINE_POSITION;      // 寄存器地址， 2字节 
            UInt16 writelen = 1;                            // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 0x0000;       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); //  
            */
        }

        

        private void btnEnableOnline_Click(object sender, EventArgs e)
        {
            int tmp = (int)(btnEnableOnline.Tag);
            if(tmp==0)
            {
                btnEnableOnline.Tag = 1;
                btnEnableOnline.Text = "关闭监测";
            }
            else
            {
                btnEnableOnline.Tag = 0;
                btnEnableOnline.Text = "打开监测";
            }
        }

       

        private void btnStartSumNm_Click(object sender, EventArgs e)
        {
            smartSumNmZero = smartSumNm3;
            flowSumNmZero = flowSumNm3;
        }

        

        private void chkUpdateInpress_CheckedChanged(object sender, EventArgs e)
        {
            numInputInpress.ReadOnly = chkUpdateInpress.Checked;
        }

        private void chkUpdateOutpress_CheckedChanged(object sender, EventArgs e)
        {
            numInputOutPress.ReadOnly = chkUpdateOutpress.Checked;
        }

        // 转换压力数据为16位表示。最高2位是倍数
        public UInt16 translate_uint32_to_uint16(UInt32 dat)
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
        public UInt32 trans_uint16_t_to_uint32(UInt16 dat_in)
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
        public UInt16 trans_float_to_uint16(float dat_in)
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
        public float trans_uint16_to_float(UInt16 dat_in)
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
                        dat_out = (float)(tmp*1.0);
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



        private void btnCalcFlow_Click(object sender, EventArgs e)
        {
            
        }

        private void btnCalcInPress_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numInputInpress.Value;
            // 写多个数据到寄存器 
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = 0x9009;  // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = translate_uint32_to_uint16(val);      // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        private void btnCalcOutPress_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numInputOutPress.Value;
            // 写多个数据到寄存器 
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr =0x900A;         // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); 
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }


        private void btnInRangeWrite_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numInRange.Value;
            val = val * 1000;
            // 写多个数据到寄存器 
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = 0x9001;     // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

       

        private void btnOutRangeWrite_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numOutRange.Value;
            val = val * 1000;
            // 写多个数据到寄存器 
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = 0x9002;     // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }


        private void timerSensor_Tick(object sender, EventArgs e)
        {
            float tmp;
            int crc = 0;
            UInt16 val;
            int byteNum = sensor_recv_end_ptr + sensor_recv_bufLen - sensor_recv_head_ptr;   // 有效数据长度
            byteNum = byteNum % sensor_recv_bufLen;
            if (byteNum < 6)
                return;
            int recv_read_ptr = sensor_recv_head_ptr;
            int pack_ptr = 0;
            int pack_len = 0;
            sensor_pack_recv[pack_ptr] = 0;  // 初始化包头
            while (recv_read_ptr != sensor_recv_end_ptr)
            {
                if(sensor_pack_recv[0]!=0xFF)   // 没有发现包头，无效数据
                {
                    pack_ptr = 0;
                    sensor_pack_recv[pack_ptr] = sensor_recvBuf[recv_read_ptr];
                    pack_ptr = pack_ptr + 1;
                    recv_read_ptr = (recv_read_ptr + 1)% sensor_recv_bufLen;   // 读取一个数据
                    sensor_recv_head_ptr = recv_read_ptr;        // 下一个数据
                }
                else
                {
                    sensor_pack_recv[pack_ptr] = sensor_recvBuf[recv_read_ptr];
                    pack_ptr = pack_ptr + 1;
                    recv_read_ptr = (recv_read_ptr + 1) % sensor_recv_bufLen;   // 读取一个数据
                    if (sensor_pack_recv[1] != 0xFE)   // 没有第二个数据标志,无效数据
                    {
                        sensor_pack_recv[0] = 0x00;
                        pack_ptr = 0;
                        sensor_recv_head_ptr = recv_read_ptr;  // 下一个数据
                    }
                    else
                    {
                        if(pack_ptr>5)  // FF FE n cmd CRC CRC
                        {
                            pack_len = sensor_pack_recv[2];
                            if (pack_len == pack_ptr)   // 数据包长度合适
                            {
                                crc = Modbus.Crc16(sensor_pack_recv, pack_len);
                               // crcTemp = sensor_pack_recv[pack_len - 2] * 256 + sensor_pack_recv[pack_len - 1];
                                if (crc == 0)   // CRC OK
                                {
                                    sensor_recv_head_ptr = recv_read_ptr;  // 下一个数据
                                    if(pack_len==24 && sensor_pack_recv[3]==0x66)    // 
                                    {   // FF FE 28 66 
                                        
                                        val = (UInt16)(sensor_pack_recv[4] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[5]);
                                        sensor_InPress = trans_uint16_t_to_uint32(val);

                                        val = (UInt16)(sensor_pack_recv[6] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[7]);
                                        sensor_OutPress = trans_uint16_t_to_uint32(val);

                                        val = (UInt16)(sensor_pack_recv[8] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[9]);
                                        sensor_BackPress = trans_uint16_t_to_uint32(val);

                                        val = (UInt16)(sensor_pack_recv[10] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[11]);
                                        sensor_CutPress = trans_uint16_t_to_uint32(val);

                                        val = (UInt16)(sensor_pack_recv[12] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[13]);
                                        sensor_CutBackPress = trans_uint16_t_to_uint32(val);

                                        val = (UInt16)(sensor_pack_recv[14] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[15]);
                                        sensor_StandPress = trans_uint16_t_to_uint32(val);

                                        val = (UInt16)(sensor_pack_recv[16] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[17]);
                                        sensor_InTemp = trans_uint16_to_float(val) - 100;

                                        val = (UInt16)(sensor_pack_recv[18] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[19]);
                                        sensor_OutTemp = trans_uint16_to_float(val) - 100;

                                        sensor_Gas = sensor_pack_recv[20];
                                        sensor_Status = sensor_pack_recv[21];
                                        sensor_update = 1;
                                    }
                                    if (pack_len == 24 && sensor_pack_recv[3] == 0x22)    // 
                                    {   // FF FE 24 22
                                        val = (UInt16)(sensor_pack_recv[4] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[5]);
                                        sensor_InRange = trans_uint16_t_to_uint32(val);

                                        val = (UInt16)(sensor_pack_recv[6] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[7]);
                                        sensor_OutRange = trans_uint16_t_to_uint32(val);

                                        val = (UInt16)(sensor_pack_recv[8] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[9]);
                                        sensor_InZero = trans_uint16_t_to_uint32(val);

                                        val = (UInt16)(sensor_pack_recv[10] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[11]);
                                        sensor_OutZero = trans_uint16_t_to_uint32(val);

                                        val = (UInt16)(sensor_pack_recv[12] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[13]);
                                        sensor_BackZero = trans_uint16_t_to_uint32(val);

                                        val = (UInt16)(sensor_pack_recv[14] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[15]);
                                        sensor_DefaultPa = trans_uint16_t_to_uint32(val);

                                        val = (UInt16)(sensor_pack_recv[16] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[17]);
                                        sensor_NTC1_Adc = trans_uint16_t_to_uint32(val);

                                        val = (UInt16)(sensor_pack_recv[18] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + sensor_pack_recv[19]);
                                        sensor_NTC2_Adc = trans_uint16_t_to_uint32(val);

                                        sensor_Gas = sensor_pack_recv[20];
                                        sensor_Status = sensor_pack_recv[21];
                                        sensor_updateInfo = 1;
                                    }
                                    pack_ptr = 0;
                                }
                                else
                                {   // 无效数据包
                                    sensor_pack_recv[0] = 0x00;
                                    pack_ptr = 0;
                                    recv_read_ptr = sensor_recv_head_ptr;  // 重开始数据
                                }
                            }
                        }
                    }
                }
            }
            
            if (sensor_update == 1)
            {
                
                tmp = sensor_InPress / 1000.0f;
                tbSensorInPress.Text = string.Format("{0:F3}", tmp);               //result: 56789.00
                tmp = sensor_OutPress / 1000.0f;
                tbSensorOutPress.Text = string.Format("{0:F3}", tmp);
                tmp = sensor_BackPress / 1000.0f;
                tbSensorBackPress.Text = string.Format("{0:F3}", tmp);
                tmp = sensor_CutPress / 1000.0f;
                tbSensorCutPress.Text = string.Format("{0:F3}", tmp);
                tmp = sensor_CutBackPress / 1000.0f;
                tbSensorCutBackPress.Text = string.Format("{0:F3}", tmp);
                tmp = sensor_StandPress / 1000.0f;
                tbLocalPress.Text = string.Format("{0:F3}", tmp);

                tmp = sensor_InTemp / 1.0f;
                tbSensorTempIn.Text = string.Format("{0:F2}", tmp);
                tmp = sensor_OutTemp / 1.0f;
                tbSensorTempOut.Text = string.Format("{0:F2}", tmp);

                tbSensorGasLevel.Text = sensor_Gas.ToString();
                tbSensorStatus.Text = sensor_Status.ToString();

                if (chartSensorOnline.Series[0].Points.Count > 400)
                {
                    chartSensorOnline.Series[0].Points.RemoveAt(0);   // MotADC
                    chartSensorOnline.Series[1].Points.RemoveAt(0);   // LuxADC
                    chartSensorOnline.Series[2].Points.RemoveAt(0);
                }

                chartSensorOnline.Series[0].Points.AddY(sensor_InPress);
                chartSensorOnline.Series[1].Points.AddY(sensor_OutPress);
                chartSensorOnline.Series[2].Points.AddY(sensor_BackPress);
                
                sensor_update = 0;
            }

            if(sensor_updateInfo==1)
            {
                sensor_updateInfo = 0;
                tbSensorGasLevel.Text = sensor_Gas.ToString();
                tbSensorStatus.Text = sensor_Status.ToString();

                tmp = sensor_InRange / 1000.0f;
                tbSensorInRange.Text = string.Format("{0:F0}", tmp);
                tmp = sensor_OutRange / 1000.0f;
                tbSensorOutRange.Text = string.Format("{0:F0}", tmp);
                tbSensorInZero.Text = sensor_InZero.ToString();
                tbSensorOutZero.Text = sensor_OutZero.ToString();
                tbSensorBackZero.Text = sensor_BackZero.ToString();

                tmp = sensor_DefaultPa / 1000.0f;
                tbSensorDefaultPa.Text = string.Format("{0:F3}", tmp);
                tbSensorNTC1.Text = sensor_NTC1_Adc.ToString();
                tbSensorNTC2.Text = sensor_NTC2_Adc.ToString();

                tbSensorGasLevel.Text = sensor_Gas.ToString();
                tbSensorStatus.Text = sensor_Status.ToString();
            }
        }

        

        private void btnSenSetInRange_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numSenInRange.Value;
            val = val * 1000;   // pa
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x30;    // command 0x30
            send_buf[4] = (Byte)(dat/256);
            send_buf[5] = (Byte)(dat%256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal%256);
            send_buf[7] = (byte)(crcVal/256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSenSetOutRange_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numSenOutRange.Value;
            val = val * 1000;   // pa
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x31;    // command 0x31
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSenSetStandPress_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numSenStandPress.Value;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x32;    // command 0x32
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSenSetInPress_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numSenInPress.Value;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x80;    // command 0x80
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        

        private void btnSenSetOutPress_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numSenOutPress.Value;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x81;    // command 0x81
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSenSetBackPress_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numSenOutPress.Value;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x82;    // command 0x82
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSenSetOutAllPress_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numSenOutPress.Value;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x83;    // command 0x83
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSenSetInZero_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = 0;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x85;    // command 0x85
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSenSetOutZero_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = 0;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x86;    // command 0x86
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSenSetBackZero_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = 0;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x87;    // command 0x87
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSenSetZero_Click_1(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = 0;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x84;    // command 0x84
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSensorPressReset_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = 90900;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x88;    // command 0x88
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSenSetNTC1_10K_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numSenResit10K.Value;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x90;    // command 0x90
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSensorNTC1_Reset_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = 0;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x91;    // command 0x91
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSenSetNTC2_10K_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numSenResit10K.Value;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x92;    // command 0x92
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSensorNTC2_Reset_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = 0;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x93;    // command 0x93
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void btnSenReadInfo_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = 0;
            // 写多个数据到寄存器 
            UInt16 dat = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Byte[] send_buf = new Byte[16];
            send_buf[0] = 0x0FF;
            send_buf[1] = 0x0FE;
            send_buf[2] = 0x08;
            send_buf[3] = 0x20;    // command 0x20
            send_buf[4] = (Byte)(dat / 256);
            send_buf[5] = (Byte)(dat % 256);
            int crcVal = Modbus.Crc16(send_buf, 6);
            send_buf[6] = (byte)(crcVal % 256);
            send_buf[7] = (byte)(crcVal / 256);
            if (serialPortSensor.IsOpen)
                serialPortSensor.Write(send_buf, 0, 8);
        }

        private void serialPortSensor_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (serialPortSensor.IsOpen == false )                       // 如果串口没有被初始化直接退出
                return;
            int byteNum = serialPortSensor.BytesToRead;
            byte[] buftmp = new byte[byteNum];
            serialPortSensor.Read(buftmp, 0, byteNum);          // 读到在数据存储到buf
            for (int i = 0; i < byteNum; i++)
            {
                sensor_recvBuf[sensor_recv_end_ptr] = buftmp[i];
                sensor_recv_temp_ptr = (sensor_recv_end_ptr + 1)% sensor_recv_bufLen;
                sensor_recv_end_ptr = sensor_recv_temp_ptr;
                if (sensor_recv_temp_ptr == sensor_recv_head_ptr)   // 数据满
                {
                    sensor_recv_head_ptr = 0;
                    sensor_recv_end_ptr = 0;
                    for (int j = 0; j < sensor_recv_bufLen; j++)
                        sensor_recvBuf[j] = 0;    // 数据满，清空重接收数据
                    break;
                }
            }
        }

       
        private void btnCaliPressZero_Click(object sender, EventArgs e)
        {
            // 写多个数据到寄存器 
            Byte devaddr = 0x01;                                // 设备地址
            UInt16 regAddr = 0x9004;         // 寄存器地址， 2字节 
            UInt16 writelen = 1;                                // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 0;      
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        private void btnCaliPressReset_Click(object sender, EventArgs e)
        {
            // 写多个数据到寄存器 
            Byte devaddr = 0x01;                                // 设备地址
            UInt16 regAddr = 0x900E;         // 寄存器地址， 2字节 
            UInt16 writelen = 1;                                // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 3;       // 0:进口压力， 1：出口压力， 2：出口备份压力，3：所有压力    写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        private void btnCaliNTC_In_Click(object sender, EventArgs e)
        {
            // 写多个数据到寄存器 
            UInt32 val = (UInt32)numNTC.Value;
            Byte devaddr = 0x01;                                // 设备地址
            UInt16 regAddr = 0x9007;         // 寄存器地址， 2字节 
            UInt16 writelen = 1;                                // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节       
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        private void btnCaliRstNTC_In_Click(object sender, EventArgs e)
        {
            // 写多个数据到寄存器 
            Byte devaddr = 0x01;                                // 设备地址
            UInt16 regAddr = 0x900F;         // 寄存器地址， 2字节 
            UInt16 writelen = 1;                                // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 0;      
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        private void btnCaliNTC_Out_Click(object sender, EventArgs e)
        {
            // 写多个数据到寄存器 
            UInt32 val = (UInt32)numNTC.Value;
            Byte devaddr = 0x01;                                // 设备地址
            UInt16 regAddr = 0x9008;         // 寄存器地址， 2字节 
            UInt16 writelen = 1;                                // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节       
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        private void btnCaliRstNTC_Out_Click(object sender, EventArgs e)
        {
            // 写多个数据到寄存器 
            Byte devaddr = 0x01;                                // 设备地址
            UInt16 regAddr = 0x9010;         // 寄存器地址， 2字节 
            UInt16 writelen = 1;                                // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 0;      
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        private void btnSetStandPress_Click(object sender, EventArgs e)
        {
            // 写多个数据到寄存器 
            UInt32 val = (UInt32)numStandPress.Value;
            // 写多个数据到寄存器 
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = 0x9003;  // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = translate_uint32_to_uint16(val);      // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        private void timerCounter_Tick(object sender, EventArgs e)
        {
            float tmp1 = float.Parse(tBsmartFlowSumZero.Text);
            float tmp2 = float.Parse(tBFlowSumZero.Text);
            //if (tmp1 == 0)
            //    tmp1 = 0;
            //else
            //tmp1 = tmp2 / tmp1;
            //tmp1 = (float)(tmp1 - 1.0);
           // tBSumPerc.Text = tmp1.ToString() + "%";

            tmp1 = float.Parse(tBSmartNm.Text);
            tmp2 = float.Parse(tBFlowNQ.Text);
            if (tmp1 == 0)
            {
                tmp1 = 0;
                tBm3Perc.Text = "NULL";
            }
            else
            {
                tmp1 = (float)(100.0*tmp2 / tmp1 - 100.0);
                tBm3Perc.Text = string.Format("{0:F2}", tmp1) + "%";
            }
        }

        private void btnSenSetLine_Click(object sender, EventArgs e)
        {
            chartSensorOnline.ChartAreas[0].AxisY.Maximum = (UInt32)numSenInMax.Value;
            chartSensorOnline.ChartAreas[0].AxisY2.Maximum = (UInt32)numSenOutMax.Value;
        }

       

        private void btnCaliOutPressZero_Click(object sender, EventArgs e)
        {
            // 写多个数据到寄存器 
            Byte devaddr = 0x01;                                // 设备地址
            UInt16 regAddr = 0x9005;         // 寄存器地址， 2字节 
            UInt16 writelen = 1;                                // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 0;
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        private void btnCaliBackPressZero_Click(object sender, EventArgs e)
        {
            // 写多个数据到寄存器 
            Byte devaddr = 0x01;                                // 设备地址
            UInt16 regAddr = 0x9006;         // 寄存器地址， 2字节 
            UInt16 writelen = 1;                                // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 0;
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        private void btnCalcBackPress_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numInputOutPress.Value;
            // 写多个数据到寄存器 
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = 0x900B;         // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        private void BtnCaliZeroPress_Click(object sender, EventArgs e)
        {
            // 写多个数据到寄存器 
            Byte devaddr = 0x01;                                // 设备地址
            UInt16 regAddr = 0x900C;         // 寄存器地址， 2字节 
            UInt16 writelen = 1;                                // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 0;
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat); // 
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        private void btnCaliOutBackPress_Click(object sender, EventArgs e)
        {
            // 高2位表示数据放大倍数   3=1000   2=100  1=10   0=1
            // 后面14位表示有效数据。  0-16383
            // 校准进口压力,最大10000KPa=10MPa
            UInt32 val = (UInt32)numInputOutPress.Value;
            // 写多个数据到寄存器 
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = 0x900D;         // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = translate_uint32_to_uint16(val);       // 写一个数据到寄存器，2字节
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
            // 应答：      01（1字节地址） 10（1字节功能）， XXXX（2字节,寄存器始地址）， 00 00（2字节，寄存器长度）
            // 错误应答：  01（1字节地址） 90（1字节功能）， 01（01/02/03/04 1个字节，错误类型)
            // 必须记录发送命令的地址，否则没有办法区分
        }

        private void btnSensorGetAllInfo_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = 0x9000;
            UInt16 readlen = 17;                       // 读N个(数值1-125)寄存器，2字节
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }

        private void btnFlowLimitEnable_Click(object sender, EventArgs e)
        {
            if(chkFlowLimitEnable.Checked)
                Flowmodbus.FlowSet24HourTableEnable(1);
            else
                Flowmodbus.FlowSet24HourTableEnable(0);
        }

        private void btnFlowLimitRead_Click(object sender, EventArgs e)
        {
            //UInt16 val = (UInt16)numFlowLimitVal.Value;
            Flowmodbus.Flow24HourTable_Read(26);   // 24小时+2个参数
        }

        private void btnFlowInPressLowSet_Click(object sender, EventArgs e)
        {
            UInt32 val = (UInt32)numFlowInPressLow.Value;
            val = translate_uint32_to_uint16(val);
            UInt16 type = 0;
            Flowmodbus.FlowCoefInPress(type, val);
        }

        private void btnFlowOutPressLowSet_Click(object sender, EventArgs e)
        {
            UInt32 val = (UInt32)numFlowOutPressLow.Value;
            val = translate_uint32_to_uint16(val);
            UInt16 type = 0;
            Flowmodbus.FlowCoefOutPress(type, val);
        }

        private void btnFlowValveSet_Click(object sender, EventArgs e)
        {
            UInt16 val = UInt16.Parse(textBValvePercentLow.Text);   // 参考阀口
            if (val >= 15000)
                return;
            if (val == 0)
                return;
            UInt16 type = 0;//(UInt16)combFlowCalcType.SelectedIndex;
            UInt16 point = (UInt16)combFlowPointLow.SelectedIndex;
            if (point > 100)
            {
                combFlowPointLow.SelectedIndex = 0;
                point = 0;
            }
                
            Flowmodbus.FlowValveSet(type, point, val);
        }

        private void btnFlowCalcSet_Click(object sender, EventArgs e)
        {
            float valf = float.Parse(textBFlowRefLow.Text);   // 参考流量
            if (valf > 32768)   // 流量最大32768
                return;
            UInt32 val = (UInt32)valf;
            if (valf <= 163.84)                            
                val = (UInt32)(valf * 100.0);               // 如果流量小于327.68， 放大100倍，提高精度
            else
            {
                if(valf<1638.4)
                    val = (UInt32)(valf * 10.0)+16384;               // 如果流量小于327.68， 放大10倍，提高精度
                else
                    val = (UInt32)(valf * 1.0)+32768;               // 如果流量大于327.68， 放大1倍，提高精度
            }
            UInt16 type = 0;//(UInt16)combFlowCalcType.SelectedIndex;
            
            UInt16 point = (UInt16)combFlowPointLow.SelectedIndex;
            Flowmodbus.FlowCoefSet(type, point, val);
        }

        private void btnFlowValveGet_Click(object sender, EventArgs e)
        {
            int tmp16 = (int)(smartValvePer * 10000/100);
            textBValvePercentLow.Text = tmp16.ToString(); 
        }

        private void btnFlowCalcGet_Click(object sender, EventArgs e)
        {
            textBFlowRefLow.Text = tBFlowNQ.Text;
            textBFlowNewValLow.Text = tBSmartNm.Text;
        }

        private void btnFlowRead_Click(object sender, EventArgs e)
        {
            UInt16 type = 0;//(UInt16)combFlowCalcType.SelectedIndex;
            UInt16 len = 15;
            Flowmodbus.FlowCoefList_Read(type, len);
        }

        private void btnFlowValveListRead_Click(object sender, EventArgs e)
        {
            UInt16 type = 0;//(UInt16)combFlowCalcType.SelectedIndex;
            UInt16 len = 13;
            Flowmodbus.FlowValveList_Read(type, len);
        }

        private void btnFlowValveMaxSet_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowValveMax.Value;
            Flowmodbus.FlowSet_sysFlowValveMax(val);
        }

        private void btnFlowValveCoefSet_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowValveCoef.Value;
            Flowmodbus.FlowSet_sysFlowValveCoef(val);
        }

        private void btnFlowValveZeroSet_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowValveZero.Value;
            Flowmodbus.FlowSet_sysFlowValveZero(val);
        }

        private void btnFlowMaxSet_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowMax.Value;
            Flowmodbus.FlowSet_sysFlowRangeMax(val);
        }

        private void btnFlowMinSet_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowMin.Value;
            Flowmodbus.FlowSet_sysFlowRangeMin(val);
        }

        private void btnFlowCVSet_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowCV.Value;
            Flowmodbus.FlowSet_sysFlow_cv(val);
        }

        private void btnFlowCGSet_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowCG.Value;
            Flowmodbus.FlowSet_sysFlow_cg(val);
        }

        private void btnFlowKLSet_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowKL.Value;
            Flowmodbus.FlowSet_sysFlow_kl(val);
        }

        private void FlowDCoefSet_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowDCoef.Value;
            Flowmodbus.FlowSet_sysFlow_gas_D_Coef(val);
        }

        private void btnFlowLimitMaxSet_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitMax.Value;
            Flowmodbus.FlowSet_sysFlowLimitMax(val);
        }

        private void btnFlowLimitMinSet_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitMin.Value;
            Flowmodbus.FlowSet_sysFlowLimitMin(val);
        }

        private void btnFlowLimitSumSet_Click(object sender, EventArgs e)
        {
            UInt32 val = (UInt32)numFlowLimitSum.Value;
            Flowmodbus.FlowSet_sysFlowLimitSum(val);
        }

        private void btnFlowConfigRead_Click(object sender, EventArgs e)
        {
            Flowmodbus.FlowInfoConfig_Read(19);
        }

        private void btnFlowLimitTab0_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab0.Value;
            Flowmodbus.FlowSet24HourTable(0, val);
        }

        private void btnFlowLimitTab1_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab1.Value;
            Flowmodbus.FlowSet24HourTable(1, val);
        }

        private void btnFlowLimitTab2_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab2.Value;
            Flowmodbus.FlowSet24HourTable(2, val);
        }

        private void btnFlowLimitTab3_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab3.Value;
            Flowmodbus.FlowSet24HourTable(3, val);
        }

        private void btnFlowLimitTab4_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab4.Value;
            Flowmodbus.FlowSet24HourTable(4, val);
        }

        private void btnFlowLimitTab5_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab5.Value;
            Flowmodbus.FlowSet24HourTable(5, val);
        }

        private void btnFlowLimitTab6_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab6.Value;
            Flowmodbus.FlowSet24HourTable(6, val);
        }

        private void btnFlowLimitTab7_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab7.Value;
            Flowmodbus.FlowSet24HourTable(7, val);
        }

        private void btnFlowLimitTab8_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab8.Value;
            Flowmodbus.FlowSet24HourTable(8, val);
        }

        private void btnFlowLimitTab9_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab9.Value;
            Flowmodbus.FlowSet24HourTable(9, val);
        }

        private void btnFlowLimitTab10_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab10.Value;
            Flowmodbus.FlowSet24HourTable(10, val);
        }

        private void btnFlowLimitTab11_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab11.Value;
            Flowmodbus.FlowSet24HourTable(11, val);
        }

        private void btnFlowLimitTab12_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab12.Value;
            Flowmodbus.FlowSet24HourTable(12, val);
        }

        private void btnFlowLimitTab13_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab13.Value;
            Flowmodbus.FlowSet24HourTable(13, val);
        }

        private void btnFlowLimitTab14_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab14.Value;
            Flowmodbus.FlowSet24HourTable(14, val);
        }

        private void btnFlowLimitTab15_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab15.Value;
            Flowmodbus.FlowSet24HourTable(15, val);
        }

        private void btnFlowLimitTab16_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab16.Value;
            Flowmodbus.FlowSet24HourTable(16, val);
        }

        private void btnFlowLimitTab17_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab17.Value;
            Flowmodbus.FlowSet24HourTable(17, val);
        }

        private void btnFlowLimitTab18_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab18.Value;
            Flowmodbus.FlowSet24HourTable(18, val);
        }

        private void btnFlowLimitTab19_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab19.Value;
            Flowmodbus.FlowSet24HourTable(19, val);
        }

        private void btnFlowLimitTab20_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab20.Value;
            Flowmodbus.FlowSet24HourTable(20, val);
        }

        private void FlowLimitTab21_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab21.Value;
            Flowmodbus.FlowSet24HourTable(21, val);
        }

        private void btnFlowLimitTab22_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab22.Value;
            Flowmodbus.FlowSet24HourTable(22, val);
        }

        private void btnFlowLimitTab23_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowLimitTab23.Value;
            Flowmodbus.FlowSet24HourTable(23, val);
        }

        private void timerModbus_Tick(object sender, EventArgs e)
        {
            Modbus.ModbusSend_ReadCommand();
        }

        private void chart1_GetToolTipText(object sender, System.Windows.Forms.DataVisualization.Charting.ToolTipEventArgs e)
        {
            if (e.HitTestResult.ChartElementType == System.Windows.Forms.DataVisualization.Charting.ChartElementType.DataPoint)
            {
                /*
                int i = e.HitTestResult.PointIndex;
                if (i >= 0)
                {
                    String val = "Val:";
                    if (e.HitTestResult.Series.Name == "Pressure")
                        val = "Pressure: ";
                    if (e.HitTestResult.Series.Name == "Pulse")
                        val = "Pulse: ";
                    e.Text = string.Format("X={0:F0}, Y={1:F0}", i, e.HitTestResult.Series.Points[i].YValues[0]);
                    e.Text = val + e.Text;
                }*/
            }
        }

        private void btnFlowReadMid_Click(object sender, EventArgs e)
        {
            UInt16 type = 1;//(UInt16)combFlowCalcType.SelectedIndex;
            UInt16 len = 15;
            Flowmodbus.FlowCoefList_Read(type, len);
        }

        private void btnFlowReadHigh_Click(object sender, EventArgs e)
        {
            UInt16 type = 2;//(UInt16)combFlowCalcType.SelectedIndex;
            UInt16 len = 15;
            Flowmodbus.FlowCoefList_Read(type, len);
        }

        private void btnFlowValveListReadMid_Click(object sender, EventArgs e)
        {
            UInt16 type = 1;//(UInt16)combFlowCalcType.SelectedIndex;
            UInt16 len = 13;
            Flowmodbus.FlowValveList_Read(type, len);
        }

        private void btnFlowValveListReadHigh_Click(object sender, EventArgs e)
        {
            UInt16 type = 2;//(UInt16)combFlowCalcType.SelectedIndex;
            UInt16 len = 13;
            Flowmodbus.FlowValveList_Read(type, len);
        }

        private void btnFlowInPressHighSet_Click(object sender, EventArgs e)
        {
            UInt32 val = (UInt32)numFlowInPressHigh.Value;
            val = translate_uint32_to_uint16(val);
            UInt16 type = 2;
            Flowmodbus.FlowCoefInPress(type, val);
        }

        private void btnFlowInPressMidSet_Click(object sender, EventArgs e)
        {
            UInt32 val = (UInt32)numFlowInPressMid.Value;
            val = translate_uint32_to_uint16(val);
            UInt16 type = 1;
            Flowmodbus.FlowCoefInPress(type, val);
        }

        private void btnFlowOutPressMidSet_Click(object sender, EventArgs e)
        {
            UInt32 val = (UInt32)numFlowOutPressMid.Value;
            val = translate_uint32_to_uint16(val);
            UInt16 type = 1;
            Flowmodbus.FlowCoefOutPress(type, val);
        }

        private void btnFlowOutPressHighSet_Click(object sender, EventArgs e)
        {
            UInt32 val = (UInt32)numFlowOutPressHigh.Value;
            val = translate_uint32_to_uint16(val);
            UInt16 type = 2;
            Flowmodbus.FlowCoefOutPress(type, val);
        }

        private void btnFlowCalcMidGet_Click(object sender, EventArgs e)
        {
            textBFlowRefMid.Text = "120.7";
        }

        private void btnFlowValveMidGet_Click(object sender, EventArgs e)
        {
            textBValvePercentMid.Text = "200";
            textBFlowNewValMid.Text = "67.9";
        }

        private void btnFlowCalcHighGet_Click(object sender, EventArgs e)
        {
            textBFlowRefHigh.Text = "120.7";
        }

        private void btnFlowValveHighGet_Click(object sender, EventArgs e)
        {
            textBValvePercentHigh.Text = "200";
            textBFlowNewValHigh.Text = "67.9";
        }

        private void btnFlowCalcMidSet_Click(object sender, EventArgs e)
        {
            float valf = float.Parse(textBFlowRefMid.Text);   // 参考流量
            if (valf > 32768)   // 流量最大32768
                return;
            UInt32 val = (UInt32)valf;
            if (valf <= 163.84)
                val = (UInt32)(valf * 100.0);               // 如果流量小于327.68， 放大100倍，提高精度
            else
            {
                if (valf < 1638.4)
                    val = (UInt32)(valf * 10.0) + 16384;               // 如果流量小于327.68， 放大10倍，提高精度
                else
                    val = (UInt32)(valf * 1.0) + 32768;               // 如果流量大于327.68， 放大1倍，提高精度
            }
            UInt16 type = 1;

            UInt16 point = (UInt16)combFlowPointLow.SelectedIndex;
            Flowmodbus.FlowCoefSet(type, point, val);
        }

        private void btnFlowCalcHighSet_Click(object sender, EventArgs e)
        {
            float valf = float.Parse(textBFlowRefHigh.Text);   // 参考流量
            if (valf > 32768)   // 流量最大32768
                return;
            UInt32 val = (UInt32)valf;
            if (valf <= 163.84)
                val = (UInt32)(valf * 100.0);               // 如果流量小于327.68， 放大100倍，提高精度
            else
            {
                if (valf < 1638.4)
                    val = (UInt32)(valf * 10.0) + 16384;               // 如果流量小于327.68， 放大10倍，提高精度
                else
                    val = (UInt32)(valf * 1.0) + 32768;               // 如果流量大于327.68， 放大1倍，提高精度
            }
            UInt16 type = 2;

            UInt16 point = (UInt16)combFlowPointLow.SelectedIndex;
            Flowmodbus.FlowCoefSet(type, point, val);
        }

        private void btnFlowValveMidSet_Click(object sender, EventArgs e)
        {
            UInt16 val = UInt16.Parse(textBValvePercentMid.Text);   // 参考阀口 %
            if (val >= 15000)
                return;
            UInt16 type = 1;
            UInt16 point = (UInt16)combFlowPointLow.SelectedIndex;
            if (point > 100)
            {
                combFlowPointLow.SelectedIndex = 0;
                point = 0;
            }

            Flowmodbus.FlowValveSet(type, point, val);
        }

        private void btnFlowValveHighSet_Click(object sender, EventArgs e)
        {
            UInt16 val = UInt16.Parse(textBValvePercentHigh.Text);   // 参考阀口 %
            if (val >= 15000)
                return;
            UInt16 type = 2;
            UInt16 point = (UInt16)combFlowPointLow.SelectedIndex;
            if (point > 100)
            {
                combFlowPointLow.SelectedIndex = 0;
                point = 0;
            }

            Flowmodbus.FlowValveSet(type, point, val);
        }

        private void btnFlowCoefResetLow_Click(object sender, EventArgs e)
        {
            Flowmodbus.FlowSet_FlowCoefDefault(0);
        }

        private void btnFlowCoefResetMid_Click(object sender, EventArgs e)
        {
            Flowmodbus.FlowSet_FlowCoefDefault(1);
        }

        private void btnFlowCoefResetHigh_Click(object sender, EventArgs e)
        {
            Flowmodbus.FlowSet_FlowCoefDefault(2);
        }

        private void btnFlowValveResetLow_Click(object sender, EventArgs e)
        {
            Flowmodbus.FlowSet_FlowValveDefault(0);
        }

        private void btnFlowValveResetMid_Click(object sender, EventArgs e)
        {
            Flowmodbus.FlowSet_FlowValveDefault(1);
        }

        private void btnFlowValveResetHigh_Click(object sender, EventArgs e)
        {
            Flowmodbus.FlowSet_FlowValveDefault(2);
        }

        private void btnPressErrorRead_Click(object sender, EventArgs e)
        {
            PressPIDModus.ReadPressErrorList(30);
        }

        private void btnPressErrorClear_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPressErrorListClear(0);
        }

        private void DeviceModbus_UpdateFlow_Info_To_Text(object sender, EventArgs e)
        {
            UInt16 tmp16;
            UInt16 regtmp = Modbus.curRecvRegAddr;
            for(int i=0;i<Modbus.curRecvRegLen;i++)
            {
                tmp16 = Modbus.curRecvData[i];
                FlowUpdate_DataToText((UInt16)(regtmp+i), tmp16);
            }
            
        }

        private void DeviceModbus_UpdateTestMode_Info_To_Text(object sender, EventArgs e)
        {
            UInt16 tmp16;
            UInt16 regtmp = Modbus.curRecvRegAddr;
            for (int i = 0; i < Modbus.curRecvRegLen; i++)
            {
                tmp16 = Modbus.curRecvData[i];
                TestUpdate_DataToText((UInt16)(regtmp + i), tmp16);
            }
        }

        public UInt32 TestUpdate_DataToText(UInt16 regaddr, UInt16 dat)
        {
            UInt32 rtn = 0;  
            switch (regaddr)
            {
                case Modbus.REG_sysValveGoDone:             //  到达 0：阀口下限 1：零点 2：最大阀口  3： 阀口上限
                    break;
                case Modbus.REG_sysHardAutoDone:    // 磨合状态， 0： 停止磨合  1： 启动磨合
                    break;
                case Modbus.REG_sysClutchDone:    // 离合操作， 0： 分离  1： 结合
                    break;
                case Modbus.REG_sysMotorStepGo:    // 阀口行进位移值。 0：停止； >0开阀  <0关阀
                    break;
                
                case Modbus.REG_sysValvePos:
                    numValvePos.Value = dat;
                    break;
                default:
                    break;
            }
            return rtn;
        }


         public  UInt32 FlowUpdate_DataToText(UInt16 regaddr, UInt16 dat)
        {
            UInt32 rtn = 0;
            UInt32 rtn32;
            // 流量计量参数恒定值  
            switch (regaddr)
            {
                case Flowmodbus.REG_sysFlowValveMax:             // 流量计算最大阀口位移10000um=10mm,整数  
                    rtn = dat;
                    numFlowValveMax.Value = dat;
                    numValveMax.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowValveZero:           // 流量计算阀口零点位置100um
                    rtn = dat;
                    numFlowValveZero.Value = dat;
                    numValveZero.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowValveCoef:            // 阀口系数1um  
                    rtn = dat;
                    numFlowValveCoef.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowRangeMax:             // 流量标况瞬间量程,整数，0-65536Nm3/h，整数
                    rtn = DataTrans.trans_uint16_t_to_uint32(dat);
                    numFlowMax.Value = rtn;
                    break;
                case Flowmodbus.REG_sysFlowRangeMin:             // 流量标况瞬间最低值，整数
                    rtn = DataTrans.trans_uint16_t_to_uint32(dat);
                    numFlowMin.Value = rtn;
                    break;
                case Flowmodbus.REG_sysFlow_gas_D_Coef:          // 空气，值为1. 燃气和空气密度比例系数, 空气密度大,设定的是空气密度.
                    rtn = dat;
                    numFlowDCoef.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlow_cv:                  // 不可压缩阶段流量计算常量,缺省2000
                    rtn = dat;
                    numFlowCV.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlow_cg:                  // 可压缩临界阶段流量计算常量，缺省1200
                    rtn = dat;
                    numFlowCG.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlow_k1:                  // 可压缩亚临界阶段流量计算常量，缺省1.7
                    rtn = dat;
                    numFlowKL.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlow_temp0:
                    rtn = 0;
                    break;
                case Flowmodbus.REG_sysFlowLimitMax:             // 限定标况瞬间流量最大设定值，0表示不限制。
                    rtn = dat;
                    numFlowLimitMax.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowLimitMin:             // 限定标况瞬间流量最少设定值，0表示不限制。
                    rtn = dat;
                    numFlowLimitMin.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowSumRemainL:            // 限定累计流量低，0表示不限制。 
                    rtn32 = (UInt32)numFlowLimitSum.Value;
                    rtn32 = (rtn32 & 0xFFFF0000) + dat;           // 低16位
                    rtn = dat;
                    numFlowLimitSum.Value = rtn32;
                    break;
                case Flowmodbus.REG_sysFlowSumRemainH:            // 限定累计流量高，0表示不限制。 
                    rtn32 = (UInt32)numFlowLimitSum.Value;
                    rtn32 = (UInt32)((rtn32&0x0000FFFF) + (dat<<16));
                    rtn = dat;
                    numFlowLimitSum.Value = rtn32;
                    break;
                case Flowmodbus.REG_sysFlowCurRemainL:            // 累计剩余流量低,每10分钟存一次
                    rtn32 = (UInt32)numFlowResvSum.Value;
                    rtn32 = (rtn32 & 0xFFFF0000) + dat;           // 低16位
                    rtn = dat;
                    numFlowResvSum.Value = rtn32;
                    break;
                case Flowmodbus.REG_sysFlowCurRemainH:            // 累计剩余流量高,每10分钟存一次
                    rtn32 = (UInt32)numFlowResvSum.Value;
                    rtn32 = (UInt32)((rtn32 & 0x0000FFFF) + (dat << 16));
                    rtn = dat;
                    numFlowResvSum.Value = rtn32;
                    break;
                case Flowmodbus.REG_sysFlowSumAllL:               // 累计流量低
                    rtn32 = (UInt32)numFlowSum.Value;
                    rtn32 = (rtn32 & 0xFFFF0000) + dat;           // 低16位
                    rtn = dat;
                    numFlowSum.Value = rtn32;
                    break;
                case Flowmodbus.REG_sysFlowSumAllH:               // 累计流量高
                    rtn32 = (UInt32)numFlowSum.Value;
                    rtn32 = (UInt32)((rtn32 & 0x0000FFFF) + (dat << 16));
                    rtn = dat;
                    numFlowSum.Value = rtn32;
                    break;
                case Flowmodbus.REG_sysFlowValveStep:               // 10mm对应步数
                    rtn = dat;
                    numFlowValveStep.Value = dat;
                    break;

                // 流量校准计算系数,低压  
                case Flowmodbus.REG_sysFlowInPressLow:              // 流量系数对应的进口压力 
                    rtn32 = DataTrans.trans_uint16_t_to_uint32(dat);  // 16进制压缩数据
                    numFlowInPressLow.Value = rtn32;
                    break;
                case Flowmodbus.REG_sysFlowOutPressLow:              // 流量系数对应的出口压力 
                    rtn32 = DataTrans.trans_uint16_t_to_uint32(dat); ;  // 16进制压缩数据
                    numFlowOutPressLow.Value = rtn32;
                    break;
                case Flowmodbus.REG_sysCoef0_Low:                   // 流量系数0
                    rtn = dat;
                    listFlowCalcLow.Items.Clear();
                    listFlowCalcLow.Items.Add("测量点    流量系数");
                    listFlowCalcLow.Items.Add("0.50Qmin    " + String.Format("{0:d}",rtn));
                    break;
                case Flowmodbus.REG_sysCoef1_Low:                    // 流量系数1
                    rtn = dat;
                    listFlowCalcLow.Items.Add("1.00Qmin    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef2_Low:                    // 流量系数2
                    rtn = dat;
                    listFlowCalcLow.Items.Add("0.10Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef3_Low:                    // 流量系数3
                    rtn = dat;
                    listFlowCalcLow.Items.Add("0.15Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef4_Low:                    // 流量系数4
                    rtn = dat;
                    listFlowCalcLow.Items.Add("0.20Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef5_Low:                   // 流量系数5
                    rtn = dat;
                    listFlowCalcLow.Items.Add("0.25Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef6_Low:                  // 流量系数6
                    rtn = dat;
                    listFlowCalcLow.Items.Add("0.40Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef7_Low:                   // 流量系数7
                    rtn = dat;
                    listFlowCalcLow.Items.Add("0.55Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef8_Low:                   // 流量系数8
                    rtn = dat;
                    listFlowCalcLow.Items.Add("0.70Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef9_Low:                   // 流量系数9
                    rtn = dat;
                    listFlowCalcLow.Items.Add("0.85Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef10_Low:                   // 流量系数10
                    rtn = dat;
                    listFlowCalcLow.Items.Add("1.00Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef11_Low:                   // 流量系数11
                    rtn = dat;
                    listFlowCalcLow.Items.Add("1.25Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef12_Low:                   // 流量系数12
                    rtn = dat;
                    listFlowCalcLow.Items.Add("1.50Qmax    " + String.Format("{0:d}", rtn));
                    break;
                //case Flowmodbus.REG_sysCoef13_Low:                   // 流量系数13  
                //    rtn = 0;
                    break;
                // 流量阀口分段，低压  
                case Flowmodbus.REG_sysPercent0_Low:                 // 流量阀口分段0
                    rtn = dat;
                    listBFlowValveLow.Items.Clear();
                    listBFlowValveLow.Items.Add("测量点    阀口%");
                    listBFlowValveLow.Items.Add("0.50Qmin    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent1_Low:                 // 流量阀口分段1
                    rtn = dat;
                    listBFlowValveLow.Items.Add("1.00Qmin    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent2_Low:                 // 流量阀口分段2
                    rtn = dat;
                    listBFlowValveLow.Items.Add("0.10Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent3_Low:                 // 流量阀口分段3
                    rtn = dat;
                    listBFlowValveLow.Items.Add("0.15Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent4_Low:                 // 流量阀口分段4
                    rtn = dat;
                    listBFlowValveLow.Items.Add("0.20Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent5_Low:                 // 流量阀口分段5
                    rtn = dat;
                    listBFlowValveLow.Items.Add("0.25Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent6_Low:                 // 流量阀口分段6
                    rtn = dat;
                    listBFlowValveLow.Items.Add("0.40Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent7_Low:                 // 流量阀口分段7
                    rtn = dat;
                    listBFlowValveLow.Items.Add("0.55Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent8_Low:                 // 流量阀口分段8
                    rtn = dat;
                    listBFlowValveLow.Items.Add("0.70Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent9_Low:                // 流量阀口分段9
                    rtn = dat;
                    listBFlowValveLow.Items.Add("0.85Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent10_Low:                // 流量阀口分段10
                    rtn = dat;
                    listBFlowValveLow.Items.Add("1.00Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent11_Low:                // 流量阀口分段11
                    rtn = dat;
                    listBFlowValveLow.Items.Add("1.25Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent12_Low:                // 流量阀口分段12
                    rtn = dat;
                    listBFlowValveLow.Items.Add("1.50Qmax    " + String.Format("{0:d}", rtn));
                    break;
                //case Flowmodbus.REG_sysPercent13_Low:                // 流量阀口分段13 
                //    rtn = 0;
                //    break;

                // 流量校准计算系数,中压  
                // 流量校准计算系数,中压  
                case Flowmodbus.REG_sysFlowInPressMid:      // 流量系数对应的进口压力 
                    rtn32 = DataTrans.trans_uint16_t_to_uint32(dat);  // 16进制压缩数据
                    numFlowInPressMid.Value = rtn32;
                    break;
                case Flowmodbus.REG_sysFlowOutPressMid:     // 流量系数对应的出口压力 
                    rtn32 = DataTrans.trans_uint16_t_to_uint32(dat);  // 16进制压缩数据
                    numFlowOutPressLow.Value = rtn32;
                    break;
                case Flowmodbus.REG_sysCoef0_Mid:           // 流量系数0
                    rtn = dat;
                    listFlowCalcMid.Items.Clear();
                    listFlowCalcMid.Items.Add("测量点    流量系数");
                    listFlowCalcMid.Items.Add("0.50Qmin    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef1_Mid:           // 流量系数1
                    rtn = dat;
                    listFlowCalcMid.Items.Add("1.00Qmin    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef2_Mid:           // 流量系数2
                    rtn = dat;
                    listFlowCalcMid.Items.Add("0.10Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef3_Mid:           // 流量系数3
                    rtn = dat;
                    listFlowCalcMid.Items.Add("0.15Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef4_Mid:           // 流量系数4
                    rtn = dat;
                    listFlowCalcMid.Items.Add("0.20Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef5_Mid:           // 流量系数5
                    rtn = dat;
                    listFlowCalcMid.Items.Add("0.25Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef6_Mid:           // 流量系数6
                    rtn = dat;
                    listFlowCalcMid.Items.Add("0.40Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef7_Mid:           // 流量系数7
                    rtn = dat;
                    listFlowCalcMid.Items.Add("0.55Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef8_Mid:           // 流量系数8
                    rtn = dat;
                    listFlowCalcMid.Items.Add("0.70Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef9_Mid:           // 流量系数9
                    rtn = dat;
                    listFlowCalcMid.Items.Add("0.85Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef10_Mid:          // 流量系数10
                    rtn = dat;
                    listFlowCalcMid.Items.Add("1.00Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef11_Mid:          // 流量系数11
                    rtn = dat;
                    listFlowCalcMid.Items.Add("1.25Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef12_Mid:          // 流量系数12
                    rtn = dat;
                    listFlowCalcMid.Items.Add("1.50Qmax    " + String.Format("{0:d}", rtn));
                    break;
                //case Flowmodbus.REG_sysCoef13_Mid:          // 流量系数13
                //    rtn = 0;
                //    break;

                // 流量阀口分段，中压 
                case Flowmodbus.REG_sysPercent0_Mid:           // 流量阀口分段0
                    rtn = dat;
                    listBFlowValveMid.Items.Clear();
                    listBFlowValveMid.Items.Add("测量点    阀口%");
                    listBFlowValveMid.Items.Add("0.50Qmin    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent1_Mid:           // 流量阀口分段1
                    rtn = dat;
                    listBFlowValveMid.Items.Add("1.00Qmin    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent2_Mid:           // 流量阀口分段2
                    rtn = dat;
                    listBFlowValveMid.Items.Add("0.10Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent3_Mid:           // 流量阀口分段3
                    rtn = dat;
                    listBFlowValveMid.Items.Add("0.15Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent4_Mid:           // 流量阀口分段4
                    rtn = dat;
                    listBFlowValveMid.Items.Add("0.20Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent5_Mid:           // 流量阀口分段5
                    rtn = dat;
                    listBFlowValveMid.Items.Add("0.25Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent6_Mid:           // 流量阀口分段6
                    rtn = dat;
                    listBFlowValveMid.Items.Add("0.40Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent7_Mid:           // 流量阀口分段7
                    rtn = dat;
                    listBFlowValveMid.Items.Add("0.55Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent8_Mid:           // 流量阀口分段8
                    rtn = dat;
                    listBFlowValveMid.Items.Add("0.70Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent9_Mid:           // 流量阀口分段9
                    rtn = dat;
                    listBFlowValveMid.Items.Add("0.85Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent10_Mid:          // 流量阀口分段10
                    rtn = dat;
                    listBFlowValveMid.Items.Add("1.00Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent11_Mid:          // 流量阀口分段11
                    rtn = dat;
                    listBFlowValveMid.Items.Add("1.25Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent12_Mid:          // 流量阀口分段12
                    rtn = dat;
                    listBFlowValveMid.Items.Add("1.50Qmax    " + String.Format("{0:d}", rtn));
                    break;
                //case Flowmodbus.REG_sysPercent13_Mid:          // 流量阀口分段13
                //    rtn = 0;
               //     break;

                // 流量校准计算系数,高压  
                case Flowmodbus.REG_sysFlowInPressHigh:        // 流量系数对应的进口压力 
                    rtn32 = DataTrans.trans_uint16_t_to_uint32(dat);   // 16进制压缩数据
                    numFlowInPressHigh.Value = rtn32;
                    break;
                case Flowmodbus.REG_sysFlowOutPressHigh:       // 流量系数对应的出口压力 
                    rtn32 = DataTrans.trans_uint16_t_to_uint32(dat);   // 16进制压缩数据
                    numFlowOutPressHigh.Value = rtn32;
                    break;
                case Flowmodbus.REG_sysCoef0_High:             // 流量系数0
                    rtn = dat;
                    listFlowCalcHigh.Items.Clear();
                    listFlowCalcHigh.Items.Add("测量点    流量系数");
                    listFlowCalcHigh.Items.Add("0.50Qmin    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef1_High:             // 流量系数1
                    rtn = dat;
                    listFlowCalcHigh.Items.Add("1.00Qmin    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef2_High:             // 流量系数2
                    rtn = dat;
                    listFlowCalcHigh.Items.Add("0.10Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef3_High:             // 流量系数3
                    rtn = dat;
                    listFlowCalcHigh.Items.Add("0.15Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef4_High:             // 流量系数4
                    rtn = dat;
                    listFlowCalcHigh.Items.Add("0.20Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef5_High:             // 流量系数5
                    rtn = dat;
                    listFlowCalcHigh.Items.Add("0.25Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef6_High:             // 流量系数6
                    rtn = dat;
                    listFlowCalcHigh.Items.Add("0.40Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef7_High:             // 流量系数7
                    rtn = dat;
                    listFlowCalcHigh.Items.Add("0.55Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef8_High:             // 流量系数8
                    rtn = dat;
                    listFlowCalcHigh.Items.Add("0.70Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef9_High:             // 流量系数9
                    rtn = dat;
                    listFlowCalcHigh.Items.Add("0.85Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef10_High:            // 流量系数10
                    rtn = dat;
                    listFlowCalcHigh.Items.Add("1.00Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef11_High:            // 流量系数11
                    rtn = dat;
                    listFlowCalcHigh.Items.Add("1.25Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysCoef12_High:            // 流量系数12
                    rtn = dat;
                    listFlowCalcHigh.Items.Add("1.50Qmax    " + String.Format("{0:d}", rtn));
                    break;
                //case Flowmodbus.REG_sysCoef13_High:            // 流量系数13
                //    rtn = 0;
                //    break;

                // 流量阀口分段，高压 
                case Flowmodbus.REG_sysPercent0_High:           // 流量阀口分段0  0.50Qmin
                    rtn = dat;
                    listBFlowValveHigh.Items.Clear();
                    listBFlowValveHigh.Items.Add("测量点    阀口%");
                    listBFlowValveHigh.Items.Add("0.50Qmin    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent1_High:           // 流量阀口分段1  1.00Qmin
                    rtn = dat;
                    listBFlowValveHigh.Items.Add("1.00Qmin    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent2_High:           // 流量阀口分段2
                    rtn = dat;
                    listBFlowValveHigh.Items.Add("0.10Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent3_High:           // 流量阀口分段3
                    rtn = dat;
                    listBFlowValveHigh.Items.Add("0.15Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent4_High:           // 流量阀口分段4
                    rtn = dat;
                    listBFlowValveHigh.Items.Add("0.20Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent5_High:           // 流量阀口分段5
                    rtn = dat;
                    listBFlowValveHigh.Items.Add("0.25Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent6_High:           // 流量阀口分段6
                    rtn = dat;
                    listBFlowValveHigh.Items.Add("0.40Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent7_High:           // 流量阀口分段7
                    rtn = dat;
                    listBFlowValveHigh.Items.Add("0.55Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent8_High:           // 流量阀口分段8
                    rtn = dat;
                    listBFlowValveHigh.Items.Add("0.70Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent9_High:           // 流量阀口分段9
                    rtn = dat;
                    listBFlowValveHigh.Items.Add("0.85Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent10_High:          // 流量阀口分段10
                    rtn = dat;
                    listBFlowValveHigh.Items.Add("1.00Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent11_High:          // 流量阀口分段11
                    rtn = dat;
                    listBFlowValveHigh.Items.Add("1.25Qmax    " + String.Format("{0:d}", rtn));
                    break;
                case Flowmodbus.REG_sysPercent12_High:          // 流量阀口分段12
                    rtn = dat;
                    listBFlowValveHigh.Items.Add("1.50Qmax    " + String.Format("{0:d}", rtn));
                    break;
                //case Flowmodbus.REG_sysPercent13_High:          // 流量阀口分段13
                //    rtn = 0;
                //    break;

                // 流量限制24小时时间段  
                case Flowmodbus.REG_sysFlowValid:               // 流量限流有限标志
                    rtn = dat;
                    if (dat == 0)
                        chkFlowLimitEnable.Checked = false;
                    else
                        chkFlowLimitEnable.Checked = true;
                    break;
                case Flowmodbus.REG_sysFlowMinutes:             // 流量限流时间间隔1小时 = 60
                    rtn = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable0:              // 24个区间段,0小时
                    rtn = dat;
                    numFlowLimitTab0.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable1:              // 24个区间段,1小时
                    rtn = dat;
                    numFlowLimitTab1.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable2:              // 24个区间段,2小时
                    rtn = dat;
                    numFlowLimitTab2.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable3:              // 24个区间段,3小时
                    rtn = dat;
                    numFlowLimitTab3.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable4:              // 24个区间段,4小时
                    rtn = dat;
                    numFlowLimitTab4.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable5:              // 24个区间段,5小时
                    rtn = dat;
                    numFlowLimitTab5.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable6:              // 24个区间段,6小时
                    rtn = dat;
                    numFlowLimitTab6.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable7:              // 24个区间段,7小时
                    rtn = dat;
                    numFlowLimitTab7.Value = dat;
                    break;

                case Flowmodbus.REG_sysFlowTable8:              // 24个区间段,8小时
                    rtn = dat;
                    numFlowLimitTab8.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable9:              // 24个区间段,9小时
                    rtn = dat;
                    numFlowLimitTab9.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable10:             // 24个区间段,10小时
                    rtn = dat;
                    numFlowLimitTab10.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable11:             // 24个区间段,11小时
                    rtn = dat;
                    numFlowLimitTab11.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable12:             // 24个区间段,12小时
                    rtn = dat;
                    numFlowLimitTab12.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable13:             // 24个区间段,13小时
                    rtn = dat;
                    numFlowLimitTab13.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable14:             // 24个区间段,14小时
                    rtn = dat;
                    numFlowLimitTab14.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable15:             // 24个区间段,15小时
                    rtn = dat;
                    numFlowLimitTab15.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable16:             // 24个区间段,16小时
                    rtn = dat;
                    numFlowLimitTab16.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable17:             // 24个区间段,17小时
                    rtn = dat;
                    numFlowLimitTab17.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable18:             // 24个区间段,18小时
                    rtn = dat;
                    numFlowLimitTab18.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable19:             // 24个区间段,19小时
                    rtn = dat;
                    numFlowLimitTab19.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable20:             // 24个区间段,20小时
                    numFlowLimitTab20.Value = dat;
                    rtn = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable21:             // 24个区间段,21小时
                    numFlowLimitTab21.Value = dat;
                    rtn = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable22:             // 24个区间段,22小时
                    rtn = dat;
                    numFlowLimitTab22.Value = dat;
                    break;
                case Flowmodbus.REG_sysFlowTable23:             // 24个区间段,23小时  
                    rtn = dat;
                    numFlowLimitTab23.Value = dat;
                    break;
                default:
                    break;
            }
            return rtn;
        }


        //===============================================================================================
        private void DeviceModbus_UpdatePressPID_Info_To_Text(object sender, EventArgs e)
        {
            UInt16 tmp16;
            UInt16 regtmp = Modbus.curRecvRegAddr;
            for (int i = 0; i < Modbus.curRecvRegLen; i++)
            {
                tmp16 = Modbus.curRecvData[i];
                PressPIDUpdate_DataToText((UInt16)(regtmp + i), tmp16);
            }

        }

        private void btnDeviceDateRead_Click(object sender, EventArgs e)
        {
            PressPIDModus.ReadPressDateTime();
        }

        private void btnDeviceDataSync_Click(object sender, EventArgs e)
        {
            PressPIDModus.SyncPressDeviceDateTime();
        }

        private void btnFlowSumRead_Click(object sender, EventArgs e)
        {
            PressPIDModus.ReadPressFlowRefreshSumValue();
        }

        private void btnPressPIDRead_Click(object sender, EventArgs e)
        {
            PressPIDModus.ReadPressPIDParam();
        }

        private void btnPressDeviceInfoRead_Click(object sender, EventArgs e)
        {
            PressPIDModus.ReadPressDeviceID();
        }

        private void btnPressUserRead_Click(object sender, EventArgs e)
        {
            PressPIDModus.ReadPressUserConfig();
        }

        private void btnPressPIDFenMu_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPID_FenMu, (UInt16)numPressPIDFenMu.Value);
        }

        private void btnPressPIDKp_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPID_NormKp, (UInt16)numPressPIDKp.Value);
        }

        private void btnPressPIDKi_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPID_NormKi, (UInt16)numPressPIDKi.Value);
        }

        private void btnPressPIDKd_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPID_NormKd, (UInt16)numPressPIDKd.Value);
        }

        private void btnPressPIDTms_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPID_NormInterval, (UInt16)numPressPIDTms.Value);
        }

        private void btnPressPIDStepMax_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPID_MaxSpeed, (UInt16)numPressPIDStepMax.Value);
        }

        private void btnPressPIDDiffMin_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPID_DiffMin, (UInt16)numPressPIDDiffMin.Value);
        }

        private void btnPressPIDDiffMax_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPID_DiffMax, (UInt16)numPressPIDDiffMax.Value);
        }

        private void btnPressPIDIncKp_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPID_IncKp, (UInt16)numPressPIDIncKp.Value);
        }

        private void btnPressPIDIncKi_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPID_IncKi, (UInt16)numPressPIDIncKi.Value);
        }

        private void btnPressPIDIncKd_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPID_IncKd, (UInt16)numPressPIDIncKd.Value);
        }

        private void btnPressPIDIncTms_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPID_IncInterval, (UInt16)numPressPIDIncTms.Value);
        }

        private void btnPressPIDIncAllTms_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPID_IncMaxTime, (UInt16)numPressPIDIncAllTms.Value);
        }

        private void btnDeviceAddrSet_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysModbusAddr, (UInt16)numDeviceAddr.Value);
        }

        private void btnDeviceBaudSet_Click(object sender, EventArgs e)
        {
            UInt16 dat = (UInt16)combDeviceBaud.SelectedIndex;
            if (dat > 3)
                dat = 2;
            combDeviceBaud.SelectedIndex = dat;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysModbudBaud, dat);
        }

        private void btnWorkModeSet_Click(object sender, EventArgs e)
        {
            checkBRunStatus.Checked = true;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysStartMode, 1);
        }
        private void btnWorkModeStop_Click(object sender, EventArgs e)
        {
            checkBRunStatus.Checked = false;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysStartMode, 0);
        }
        private void btnControlModeSet_Click(object sender, EventArgs e)
        {
            UInt16 dat = (UInt16)combRunKeyControl.SelectedIndex;
            if (dat > 1)
                dat = 0;
            combRunKeyControl.SelectedIndex = dat;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysRunKeyMode, dat);
        }

        private void btnValveOffSet_Click(object sender, EventArgs e)
        {
            UInt16 dat = (UInt16)combValveOff.SelectedIndex;
            if (dat > 1)
                dat = 0;
            combValveOff.SelectedIndex = dat;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysIdleValveStaus, dat);
        }

        private void btnValveTypeSet_Click(object sender, EventArgs e)
        {
            UInt16 dat = (UInt16)combValveType.SelectedIndex;
            if (dat > 5)
                dat = 3;
            combValveType.SelectedIndex = dat;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysValveType, dat);
        }

        private void btnDeviceID_Click(object sender, EventArgs e)
        {
            UInt32 rtn32 = (UInt32)numDeviceID.Value;
            UInt16[] dat = new UInt16[16];
            UInt16 tmp16L = (UInt16)(rtn32&0x0000FFFF);
            dat[0] = tmp16L;
            UInt16 tmp16H = (UInt16)(rtn32/65536);
            dat[1] = tmp16H;
            PressPIDModus.SetPresRegisterBuffer(PressPIDModus.REG_sysDeviceID_L, dat,2);
        }

        private void btnRefreshFlowUart_Click(object sender, EventArgs e)
        {
            String[] array = System.IO.Ports.SerialPort.GetPortNames();
            combFlowUartList.Items.Clear();
            for (int i = 0; i < array.Length; i++)
            {
                combFlowUartList.Items.Add(array[i]);
            }
            if (array.Length > 0)
            {
                combFlowUartList.SelectedIndex = 0;
                
                btnCloseFlowUart.Enabled = false;
                btnOpenFlowUart.Enabled = true;
                btnRefreshFlowUart.Enabled = true;
            }
            else
            {
                btnCloseFlowUart.Enabled = false;
                btnOpenFlowUart.Enabled = false;
                btnRefreshFlowUart.Enabled = true;
            }
        }

        private void btnRefreshPressUart_Click(object sender, EventArgs e)
        {
            String[] array = System.IO.Ports.SerialPort.GetPortNames();
            combPressUartList.Items.Clear();
            for (int i = 0; i < array.Length; i++)
            {
                combPressUartList.Items.Add(array[i]);
            }
            if (array.Length > 0)
            {
                combPressUartList.SelectedIndex = 0;

                btnClosePressUart.Enabled = false;
                btnOpenPressUart.Enabled = true;
                btnRefreshPressUart.Enabled = true;
            }
            else
            {
                btnClosePressUart.Enabled = false;
                btnOpenPressUart.Enabled = false;
                btnRefreshPressUart.Enabled = true;
            }
        }

        private void btnRefreshSensorUart_Click(object sender, EventArgs e)
        {
            String[] array = System.IO.Ports.SerialPort.GetPortNames();
            combSensorUartList.Items.Clear();
            for (int i = 0; i < array.Length; i++)
            {
                combSensorUartList.Items.Add(array[i]);
            }
            if (array.Length > 0)
            {
                combSensorUartList.SelectedIndex = 0;

                btnCloseSensorUart.Enabled = false;
                btnOpenSensorUart.Enabled = true;
                btnRefreshSensorUart.Enabled = true;
            }
            else
            {
                btnCloseSensorUart.Enabled = false;
                btnOpenSensorUart.Enabled = false;
                btnRefreshSensorUart.Enabled = true;
            }
        }

        private void btnRefreshMotorUart_Click(object sender, EventArgs e)
        {
            String[] array = System.IO.Ports.SerialPort.GetPortNames();
            combMotorUartList.Items.Clear();
            for (int i = 0; i < array.Length; i++)
            {
                combMotorUartList.Items.Add(array[i]);
            }
            if (array.Length > 0)
            {
                combMotorUartList.SelectedIndex = 0;

                btnCloseMotorUart.Enabled = false;
                btnOpenMotorUart.Enabled = true;
                btnRefreshMotorUart.Enabled = true;
            }
            else
            {
                btnCloseMotorUart.Enabled = false;
                btnOpenMotorUart.Enabled = false;
                btnRefreshMotorUart.Enabled = true;
            }
        }

        private void btnOpenFlowUart_Click(object sender, EventArgs e)
        {
            UInt16 rtn;
            if (combFlowUartList.Items.Count <= 0)
                return;
            if (FlowPort.IsOpen)
            {
                return;
            }
            else
            {
                FlowPort.PortName = combFlowUartList.Items[combFlowUartList.SelectedIndex].ToString();
                rtn = (UInt16)combFlowBaudList.SelectedIndex;
                switch(rtn)
                {
                    case 0:
                        FlowPort.BaudRate = 2400;
                        break;
                    case 1:
                        FlowPort.BaudRate = 4800;
                        break;
                    default:
                        FlowPort.BaudRate = 9600;
                        break;
                    case 3:
                        FlowPort.BaudRate = 115200;
                        break;
                    case 4:
                        FlowPort.BaudRate = 460800;
                        break;
                }
                FlowPort.DataBits = 8;
                FlowPort.StopBits = System.IO.Ports.StopBits.One;
                rtn = (UInt16)combFlowCheckList.SelectedIndex;
                switch(rtn)
                {
                    case 0:
                        FlowPort.Parity = System.IO.Ports.Parity.None;
                        break;
                    default:
                        FlowPort.Parity = System.IO.Ports.Parity.Odd;
                        break;
                    case 2:
                        FlowPort.Parity = System.IO.Ports.Parity.Even;
                        break;
                }
                
                FlowPort.Handshake = System.IO.Ports.Handshake.None;
                FlowPort.Encoding = Encoding.UTF8;
                try
                {
                    FlowPort.Open();
                }
                catch (Exception bpe)
                {
                    flow_timer.Enabled = false;
                    btnCloseFlowUart.Enabled = false;
                    btnRefreshFlowUart.Enabled = true;
                    btnOpenFlowUart.Enabled = true;
                }
                finally
                {
                    if (FlowPort.IsOpen)
                    {
                        chartOnline.ChartAreas[0].AxisY.Maximum = 1000;// 4000;  流量
                        chartOnline.ChartAreas[0].AxisY2.Maximum = 300;// 4000;
                        flow_timer.Enabled = true;

                        btnCloseFlowUart.Enabled = true;
                        btnRefreshFlowUart.Enabled = false;
                        btnOpenFlowUart.Enabled = false;
                    }  
                }
            }
        }

        private void btnCloseFlowUart_Click(object sender, EventArgs e)
        {
            if (combFlowUartList.Items.Count <= 0)
                return;
            if (!FlowPort.IsOpen)
            {
                return;
            }
            else
            {
                FlowPort.Close();
                flow_timer.Enabled = false;
                btnCloseFlowUart.Enabled = false;
                btnRefreshFlowUart.Enabled = true;
                btnOpenFlowUart.Enabled = true;
            }
        }

        private void btnClosePressUart_Click(object sender, EventArgs e)
        {
            if (combPressUartList.Items.Count <= 0)
                return;
            if (!serialPortPress.IsOpen)
            {
                return;
            }
            else
            {
                //tBSmartFaKou.Text = "o";
                serialPortPress.Close();
                timerModbus.Enabled = false;

                btnClosePressUart.Enabled = false;
                btnRefreshPressUart.Enabled = true;
                btnOpenPressUart.Enabled = true;

                gbControlSensor.Enabled = false;
                gbControlPress.Enabled = false;
                gbControlFlow.Enabled = false;
                gbControlOnline.Enabled = false;
                

                gbMachineTest.Enabled = false;
            }
        }

        private void btnOpenPressUart_Click(object sender, EventArgs e)
        {
            UInt16 rtn = 0;
            if (combPressUartList.Items.Count <= 0)
                return;
            if (serialPortPress.IsOpen)
            {
                return;
            }
            else
            {
                serialPortPress.PortName = combPressUartList.Items[combPressUartList.SelectedIndex].ToString();
                int baud = System.Convert.ToInt32(combPressBaudList.Items[combPressBaudList.SelectedIndex].ToString());

                UInt32 node = (UInt32)numPressDeviceAddr.Value;
                serialPortPress.BaudRate = baud;
                serialPortPress.DataBits = 8;
                serialPortPress.StopBits = System.IO.Ports.StopBits.One;
                rtn = (UInt16)combPressCheckList.SelectedIndex;
                switch (rtn)
                {
                    case 0:
                        serialPortPress.Parity = System.IO.Ports.Parity.None;
                        break;
                    default:
                        serialPortPress.Parity = System.IO.Ports.Parity.Odd;
                        break;
                    case 2:
                        serialPortPress.Parity = System.IO.Ports.Parity.Even;
                        break;
                }
                serialPortPress.Handshake = System.IO.Ports.Handshake.None;
                serialPortPress.Encoding = Encoding.UTF8;

                try
                {
                    serialPortPress.Open();
                }
                catch (Exception bpe)
                {
                    timerModbus.Enabled = false;

                    btnClosePressUart.Enabled = false;
                    btnRefreshPressUart.Enabled = true;
                    btnOpenPressUart.Enabled = true;

                    gbControlSensor.Enabled = false;
                    gbControlPress.Enabled = false;
                    gbControlFlow.Enabled = false;
                    gbControlOnline.Enabled = false;
                    gbMachineTest.Enabled = false;
                }
                finally
                {
                    if (serialPortPress.IsOpen)
                    {
                        btnClosePressUart.Enabled = true;
                        btnRefreshPressUart.Enabled = false;
                        btnOpenPressUart.Enabled = false;

                        Modbus.MBConfig(serialPortPress, (ushort)node, (ushort)baud);
                        timerModbus.Enabled = false;
                        Modbus.ModbusSend_ClearCommand();   // 清除所有命令
                        timerModbus.Interval = 100; // 100ms
                        timerModbus.Enabled = true;

                        gbControlSensor.Enabled = true;
                        gbControlPress.Enabled = true;
                        gbControlFlow.Enabled = true;
                        gbControlOnline.Enabled = true;
                        gbMachineTest.Enabled = true;
                    } 
                }
            }
        }

        private void btnOpenSensorUart_Click(object sender, EventArgs e)
        {
            UInt16 rtn = 0;
            if (combSensorUartList.Items.Count <= 0)
            {
                gbSensorDevice.Enabled = false;
                gbSenInfodevice.Enabled = gbSensorDevice.Enabled;
                return;
            }
            if (serialPortSensor.IsOpen)
            {
                return;
            }
            else
            {
                timerSensor.Stop();
                timerSensor.Enabled = false;
                sensor_recv_head_ptr = 0;
                sensor_recv_end_ptr = 0;
                timerSensor.Interval = 100;
                timerSensor.Enabled = true;
                timerSensor.Start();
                serialPortSensor.PortName = combSensorUartList.Items[combSensorUartList.SelectedIndex].ToString();
                int baud = System.Convert.ToInt32(combSensorBaudList.Items[combSensorBaudList.SelectedIndex].ToString());
                serialPortSensor.BaudRate = baud;
                serialPortSensor.DataBits = 8;
                serialPortSensor.StopBits = System.IO.Ports.StopBits.One;
                rtn = (UInt16)combSensorCheckList.SelectedIndex;
                switch (rtn)
                {
                    case 0:
                        serialPortSensor.Parity = System.IO.Ports.Parity.None;
                        break;
                    default:
                        serialPortSensor.Parity = System.IO.Ports.Parity.Odd;
                        break;
                    case 2:
                        serialPortSensor.Parity = System.IO.Ports.Parity.Even;
                        break;
                }
               // serialPortSensor.Parity = System.IO.Ports.Parity.None;
                serialPortSensor.Handshake = System.IO.Ports.Handshake.None;
                serialPortSensor.Encoding = Encoding.UTF8;
                try
                {
                    serialPortSensor.Open();
                }
                catch (Exception bpe)
                {
                    timerSensor.Enabled = false;
                    gbSensorDevice.Enabled = false;
                    btnRefreshSensorUart.Enabled = true;
                    btnCloseSensorUart.Enabled = false;
                    btnOpenSensorUart.Enabled = true;
                }
                finally
                {
                    if (serialPortSensor.IsOpen)
                    {
                        chartSensorOnline.ChartAreas[0].AxisY.Maximum = 400000;// 4000;
                        chartSensorOnline.ChartAreas[0].AxisY2.Maximum = 5000;// 4000;
                        timerSensor.Enabled = false;
                        timerSensor.Interval = 100;
                        timerSensor.Enabled = true;
                        gbSensorDevice.Enabled = true;
                        btnSenReadInfo_Click(null, null);
                        btnRefreshSensorUart.Enabled = false;
                        btnCloseSensorUart.Enabled = true;
                        btnOpenSensorUart.Enabled = false;
                    } 
                }
            }
            gbSenInfodevice.Enabled = gbSensorDevice.Enabled;
        }

        private void btnCloseSensorUart_Click(object sender, EventArgs e)
        {
            if (combSensorUartList.Items.Count <= 0)
            {
                gbSensorDevice.Enabled = false;
                gbSenInfodevice.Enabled = gbSensorDevice.Enabled;
                return;
            }
            if (serialPortSensor.IsOpen)
            {
                serialPortSensor.Close();
                gbSensorDevice.Enabled = false;
                timerSensor.Stop();
                timerSensor.Enabled = false;
                btnRefreshSensorUart.Enabled = true;
                btnCloseSensorUart.Enabled = false;
                btnOpenSensorUart.Enabled = true;

                gbSensorDevice.Enabled = false;
            }
        }

        private void btnCloseMotorUart_Click(object sender, EventArgs e)
        {
            if (combMotorUartList.Items.Count <= 0)
            {
                return;
            }
            if (serialPortMotor.IsOpen)
            {
                serialPortMotor.Close();

                btnRefreshMotorUart.Enabled = true;
                btnCloseMotorUart.Enabled = false;
                btnOpenMotorUart.Enabled = true;

                gbControlValveTest.Enabled = false;
            }
        }

        private void btnOpenMotorUart_Click(object sender, EventArgs e)
        {
            UInt16 rtn = 0;
            if (combMotorUartList.Items.Count <= 0)
            {
                return;
            }
            if (serialPortMotor.IsOpen)
            {
                return;
            }
            else
            {
                serialPortMotor.PortName = combMotorUartList.Items[combMotorUartList.SelectedIndex].ToString();
                int baud = System.Convert.ToInt32(combMotorBaudList.Items[combMotorBaudList.SelectedIndex].ToString());

                UInt32 node = (UInt32)numMotorDeviceAddr.Value;
                serialPortMotor.BaudRate = baud;
                serialPortMotor.DataBits = 8;
                serialPortMotor.StopBits = System.IO.Ports.StopBits.One;
                rtn = (UInt16)combMotorCheckList.SelectedIndex;
                switch (rtn)
                {
                    case 0:
                        serialPortMotor.Parity = System.IO.Ports.Parity.None;
                        break;
                    default:
                        serialPortMotor.Parity = System.IO.Ports.Parity.Odd;
                        break;
                    case 2:
                        serialPortMotor.Parity = System.IO.Ports.Parity.Even;
                        break;
                }
                // serialPortSensor.Parity = System.IO.Ports.Parity.None;
                serialPortMotor.Handshake = System.IO.Ports.Handshake.None;
                serialPortMotor.Encoding = Encoding.UTF8;
                try
                {
                    serialPortMotor.Open();
                }
                catch (Exception bpe)
                {
                    btnRefreshMotorUart.Enabled = true;
                    btnCloseMotorUart.Enabled = false;
                    btnOpenMotorUart.Enabled = true;
                    gbControlValveTest.Enabled = false;
                }
                finally
                {
                    if (serialPortMotor.IsOpen)
                    {
                        MotorModbus.MBConfig(serialPortMotor, (ushort)node, (ushort)baud);
                        timerMotor.Enabled = false;
                        MotorModbus.mModbusSend_ClearCommand();   // 清除所有命令
                        timerMotor.Interval = 100; // 100ms
                        timerMotor.Enabled = true;

                        btnRefreshMotorUart.Enabled = false;
                        btnCloseMotorUart.Enabled = true;
                        btnOpenMotorUart.Enabled = false;
                        gbControlValveTest.Enabled = true;
                    }    
                }
            }

        }

        private void btnPressUserObj_Click(object sender, EventArgs e)
        {
            UInt32 dat = (UInt32)numPressUserObj.Value;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPressOutObj, DataTrans.translate_uint32_to_uint16(dat));
        }

        private void btnPressUserObjMax_Click(object sender, EventArgs e)
        {
            UInt32 dat = (UInt32)numPressUserObjMax.Value;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPressOutObjMax, DataTrans.translate_uint32_to_uint16(dat));
        }

        private void btnPressUserObjMin_Click(object sender, EventArgs e)
        {
            UInt32 dat = (UInt32)numPressUserObjMin.Value;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPressOutObjMin, DataTrans.translate_uint32_to_uint16(dat));
        }

        private void btnnumFlowUserCurMax_Click(object sender, EventArgs e)
        {
            UInt16 dat = (UInt16)numFlowUserCurMax.Value;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPressFlowLimitMax, dat);
        }

        private void btnFlowUserCurMin_Click(object sender, EventArgs e)
        {
            UInt16 dat = (UInt16)numFlowUserCurMin.Value;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysPressFlowLimitMin, dat);
        }

        private void btnFlowUserCurMax_Click(object sender, EventArgs e)
        {
            UInt32 dat = (UInt32)numFlowUserSumMax.Value;
            UInt16[] buf = new UInt16[16];
            buf[0] = (UInt16)(dat & 0x0000FFFF);
            buf[1] = (UInt16)(dat>>16);
            PressPIDModus.SetPresRegisterBuffer(PressPIDModus.REG_sysPressFlowSumLimitL, buf,2);
        }

        private void btnFlowResvSumSet_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_FlowResvSumL, 0);
        }

        private void btnFlowSumSet_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_FlowSumL, 0);
        }

        private void btnModbusmodeExit_Click(object sender, EventArgs e)
        {
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysModbusSel, 0);
        }

        private void btnModbusUserEnter_Click(object sender, EventArgs e)
        {
            UInt16 dat = (UInt16)numModbusUser.Value;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysModbusSel, dat);
        }

        private void btnModbusAdminEnter_Click(object sender, EventArgs e)
        {
            UInt16 dat = (UInt16)numModbusAdmin.Value;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysModbusSel, dat);
        }

        private void btnModbusSupperEnter_Click(object sender, EventArgs e)
        {
            UInt16 dat = (UInt16)numModbusSupper.Value;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysModbusSel, dat);
        }

        private void btnModbusUserChange_Click(object sender, EventArgs e)
        {
            UInt16 dat = (UInt16)numModbusUser.Value;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysModbusUser, dat);
        }

        private void btnModbusAdminChange_Click(object sender, EventArgs e)
        {
            UInt16 dat = (UInt16)numModbusAdmin.Value;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysModbusAdmin, dat);
        }

        private void btnModbusSupperChange_Click(object sender, EventArgs e)
        {
            UInt16 dat = (UInt16)numModbusSupper.Value;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysModbusSupper, dat);
        }

        private void btnModbusModeRead_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                                            // 设备地址
            UInt16 regAddr = PressPIDModus.REG_sysSoftVer;                // 寄存器地址， 2字节 
            UInt16 readlen = 6;                                             // 读N个(数值1-125)寄存器，2字节
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }

        private void btnRunModeSet_Click(object sender, EventArgs e)
        {
            UInt16 dat = (UInt16)combRunModeSel.SelectedIndex;
            if (dat > 6)
                dat = 0;
            combRunModeSel.SelectedIndex = dat;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysRunMode, dat);
        }

        private void btnTestLimitDown_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Modbus.REG_sysValveGoDone;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 0;       // 写一个数据到寄存器，2字节  到达 0：阀口下限
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

        private void btnTestZero_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Modbus.REG_sysValveGoDone;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 1;       // 写一个数据到寄存器，2字节  1：零点
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

        private void btnTestValveMax_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Modbus.REG_sysValveGoDone;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 2;       // 写一个数据到寄存器，2字节  2：最大阀口
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

        private void btnTestLimitUp_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Modbus.REG_sysValveGoDone;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 3;       // 写一个数据到寄存器，2字节  3： 阀口上限
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

        private void btnTestAutoStart_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Modbus.REG_sysHardAutoDone;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)(numAutoTestNumber.Value);       // 写一个数据到寄存器，2字节   1： 启动磨合
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

       

        private void btnMagnetConn_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Modbus.REG_sysClutchDone;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 1;       // 写一个数据到寄存器，2字节   1： 结合
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

        private void btnMagnetDisc_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Modbus.REG_sysClutchDone;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 0;       // 写一个数据到寄存器，2字节   0： 分离
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

        private void btnTestValveMaxGet_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Flowmodbus.REG_sysFlowValveMax;                   // 寄存器地址， 2字节 
            UInt16 readlen = 1;                       // 读N个(数值1-125)寄存器，2字节
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }

        

        private void btnTestValveInc_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Modbus.REG_sysMotorStepGo;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)numValveStep.Value;       // 写一个数据到寄存器，2字节  
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

        private void btnTestValveDec_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Modbus.REG_sysMotorStepGo;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)numValveStep.Value;       // 写一个数据到寄存器，2字节  
            dat[0] = (UInt16)(0-dat[0]);
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

        private void btnTestValveStop_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Modbus.REG_sysMotorStepGo;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 0;       // 写一个数据到寄存器，2字节  
            Modbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

        private void btnTestValveRead_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Modbus.REG_sysValvePos;                   // 寄存器地址， 2字节 
            UInt16 readlen = 1;                       // 读N个(数值1-125)寄存器，2字节
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }

        private void btnTestValveZeroGet_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = Flowmodbus.REG_sysFlowValveZero;                   // 寄存器地址， 2字节 
            UInt16 readlen = 1;                       // 读N个(数值1-125)寄存器，2字节
            Modbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }

        private void btnTestValveZeroSet_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numValveZero.Value;
            if (MessageBox.Show("确认阀口零点设定新值="+val.ToString()+"um?", "阀口零点", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                Flowmodbus.FlowSet_sysFlowValveZero(val);
        }

        private void btnFlowValveStepSet_Click(object sender, EventArgs e)
        {
            UInt16 val = (UInt16)numFlowValveStep.Value;
            Flowmodbus.FlowSet_sysFlowValveStep(val);
        }

        private void btnTestModeEnter_Click(object sender, EventArgs e)
        {
            UInt16 dat = 6;
            PressPIDModus.SetPresRegisterValue(PressPIDModus.REG_sysRunMode, dat);
        }

        private void btnMotorTestEnter_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = MotorModbus.REG_sysRunMode;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 6;       // 写一个数据到寄存器，2字节  测试模式 
            MotorModbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

        private void btnMotorTestUp_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = MotorModbus.REG_sysMotorStepGo;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)numMotorStep.Value;       // 写一个数据到寄存器，2字节  
            MotorModbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

        private void btnMotorTestDown_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = MotorModbus.REG_sysMotorStepGo;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = (UInt16)numMotorStep.Value;       // 写一个数据到寄存器，2字节  
            dat[0]=(UInt16)(0 - dat[0]);
            MotorModbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

        private void btnMotorTestStop_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = MotorModbus.REG_sysMotorStepGo;                   // 寄存器地址， 2字节 
            UInt16 writelen = 1;                       // 写入N个(数值1-123)寄存器，2字节
            UInt16[] dat = new UInt16[16];
            dat[0] = 0;       // 写一个数据到寄存器，2字节  
            MotorModbus.Modbus_Write10Command(devaddr, regAddr, writelen, dat);
        }

        private void btnMotorTestValveRead_Click(object sender, EventArgs e)
        {
            Byte devaddr = 0x01;                       // 设备地址
            UInt16 regAddr = MotorModbus.REG_sysValvePos;                   // 寄存器地址， 2字节 
            UInt16 readlen = 1;                       // 读N个(数值1-125)寄存器，2字节
            MotorModbus.Modbus_Read03Command(devaddr, regAddr, readlen);
        }

        private void timerMotor_Tick(object sender, EventArgs e)
        {
            MotorModbus.ModbusSend_ReadCommand();
            ///tBSmartTemp
        }

        private void MotorModbus_Update_Info_To_Text(object sender, EventArgs e)
        {
            UInt16 tmp16;
            UInt16 regtmp = MotorModbus.mcurRecvRegAddr;
            for (int i = 0; i < MotorModbus.mcurRecvRegLen; i++)
            {
                tmp16 = MotorModbus.mcurRecvData[i];
                switch (regtmp+i)
                {
                    // PID参数
                    case MotorModbus.REG_sysValvePos:         // PID参数的分母，缺省10000
                        numMotorLocation.Value = tmp16;
                        break;
                }
            }
        }

        private void serialPortMotor_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte cmd = 0;
            int valPtr;
            if (serialPortMotor == null)                       // 如果串口没有被初始化直接退出
                return;
            int byteNum = serialPortMotor.BytesToRead;
            byte[] buftmp = new byte[byteNum];
            serialPortMotor.Read(buftmp, 0, byteNum);          // 读到在数据存储到buf
            xgj_motorfrecv_buffer.AddRange(buftmp);
            int len;
            int crcVal;
            UInt16 tmpcrc;
            UInt16 val;
            Int32 tmp32;
            Int64 tmp64;
            Int32 tmpdot;
            UInt16 recv_flag = 0;
            while (xgj_motorfrecv_buffer.Count >= 5) //至少包含addr、cmd、len/error crc crc
            {
                cmd = xgj_motorfrecv_buffer[1];   // 读取命令
                switch (cmd)
                {
                    case 0x03:  //read
                        len = xgj_motorfrecv_buffer[2];
                        if (len > 0x40 || len == 0)
                        {
                            xgj_motorfrecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                        }
                        else
                        {
                            if (xgj_motorfrecv_buffer.Count >= (len + 5))
                            {
                                xgj_motorfrecv_buffer.CopyTo(0, xgj_motortemp_buf, 0, len + 5);
                                crcVal = MotorModbus.Crc16(xgj_motortemp_buf, len + 3);
                                tmpcrc = (UInt16)(xgj_motortemp_buf[len + 4] << 8);
                                tmpcrc = (UInt16)(tmpcrc + xgj_motortemp_buf[len + 3]);
                                if (crcVal == tmpcrc)   // 校验和正确
                                {
                                    ///System.Diagnostics.Debug.WriteLine("crc ok");

                                    MotorModbus.mcurRecvDevAddr = xgj_motortemp_buf[0];
                                    MotorModbus.mcurRecvCmd = xgj_motortemp_buf[1];
                                    MotorModbus.mcurRecvRegAddr = MotorModbus.mlastSendRegAddr;
                                    MotorModbus.mcurRecvRegLen = (UInt16)(xgj_motortemp_buf[2] / 2);   //bytes length
                                    valPtr = 0;
                                    for (int i = 0; i < MotorModbus.mcurRecvRegLen; i++)
                                    {
                                        val = (UInt16)(xgj_motortemp_buf[valPtr + 3] << 8);  // 小端：较高的有效字节存储在较高的存储器地址，较低的有效字节存储在较低的存储器地址
                                        val = (UInt16)(val + xgj_motortemp_buf[valPtr + 4]);
                                        MotorModbus.mcurRecvData[i] = val;
                                        valPtr = (UInt16)(valPtr + 2);
                                    }

                                    this.Invoke(new EventHandler(MotorModbus_Update_Info_To_Text));
                                    xgj_motorfrecv_buffer.RemoveRange(0, len + 5); // 删除整个数据包
                                }
                                else
                                    xgj_motorfrecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                            }
                            else
                                recv_flag = 1;
                        }
                        break;
                    case 0x10: //write
                        len = 3;
                        if (xgj_motorfrecv_buffer.Count >= 8)
                        {
                            xgj_motorfrecv_buffer.CopyTo(0, xgj_motortemp_buf, 0, len + 5);
                            crcVal = Modbus.Crc16(xgj_motortemp_buf, len + 3);
                            tmpcrc = (UInt16)(xgj_motortemp_buf[len + 4] << 8);
                            tmpcrc = (UInt16)(tmpcrc + xgj_motortemp_buf[len + 3]);
                            if (crcVal == tmpcrc)   // 校验和正确
                            {
                                MotorModbus.mcurRecvDevAddr = xgj_motortemp_buf[0];
                                MotorModbus.mcurRecvCmd = xgj_motortemp_buf[1];
                                MotorModbus.mcurRecvRegAddr = (UInt16)(xgj_motortemp_buf[2] << 8 + xgj_motortemp_buf[3]);
                                xgj_motorfrecv_buffer.RemoveRange(0, len + 5); // 删除整个数据包
                            }
                            else
                                xgj_motorfrecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                        }
                        else
                            recv_flag = 1;

                        break;
                    case 0x83:  // addr, 81 error, crc crc
                        len = 0;
                        xgj_motorfrecv_buffer.CopyTo(0, xgj_motortemp_buf, 0, len + 5);
                        crcVal = Modbus.Crc16(xgj_motortemp_buf, len + 3);
                        tmpcrc = (UInt16)(xgj_motortemp_buf[len + 4] << 8);
                        tmpcrc = (UInt16)(tmpcrc + xgj_motortemp_buf[len + 3]);
                        if (crcVal == tmpcrc)   // 校验和正确
                        {
                            MotorModbus.mcurRecvDevAddr = xgj_motortemp_buf[0];
                            MotorModbus.mcurRecvCmd = (byte)(xgj_motortemp_buf[1] - 0x80);
                            xgj_motorfrecv_buffer.RemoveRange(0, len + 5); // 删除整个数据包
                        }
                        else
                            xgj_motorfrecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                        break;
                    case 0x90:  // addr, 81 error, crc crc
                        len = 0;
                        xgj_motorfrecv_buffer.CopyTo(0, xgj_motortemp_buf, 0, len + 5);
                        crcVal = Modbus.Crc16(xgj_motortemp_buf, len + 3);
                        tmpcrc = (UInt16)(xgj_motortemp_buf[len + 4] << 8);
                        tmpcrc = (UInt16)(tmpcrc + xgj_motortemp_buf[len + 3]);
                        if (crcVal == tmpcrc)   // 校验和正确
                        {
                            MotorModbus.mcurRecvDevAddr = xgj_motortemp_buf[0];
                            MotorModbus.mcurRecvCmd = (byte)(xgj_motortemp_buf[1] - 0x80);
                            xgj_motorfrecv_buffer.RemoveRange(0, len + 5); // 删除整个数据包
                        }
                        else
                            xgj_motorfrecv_buffer.RemoveAt(0);            // 无效数据包，删除第一个，移动
                        break;
                    default:
                        xgj_motorfrecv_buffer.RemoveAt(0);
                        break;
                }
                if (recv_flag == 1)
                    break;
            }
        }

        private void btnTestValveZeroAuto_Click(object sender, EventArgs e)
        {
            UInt16 val = 65535;
            if (MessageBox.Show("确认以当前位置作为阀口零点？", "设定阀口零点", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                Flowmodbus.FlowSet_sysFlowValveZero(val);
        }

        private void btnMicroUartRefresh_Click(object sender, EventArgs e)
        {
            String[] array = System.IO.Ports.SerialPort.GetPortNames();
            combMicroUartList.Items.Clear();
            for (int i = 0; i < array.Length; i++)
            {
                combMicroUartList.Items.Add(array[i]);
            }
            if (array.Length > 0)
            {
                combMicroUartList.SelectedIndex = 0;

                btnMicroClose.Enabled = false;
                btnMicroOpen.Enabled = true;
                btnMicroUartRefresh.Enabled = true;
            }
            else
            {
                btnMicroClose.Enabled = false;
                btnMicroOpen.Enabled = false;
                btnMicroUartRefresh.Enabled = true;
            }
        }

        private void btnMicroClose_Click(object sender, EventArgs e)
        {
            if (combMicroUartList.Items.Count <= 0)
                return;
            if (!microserialPort.IsOpen)
            {
                return;
            }
            else
            {
                microserialPort.Close();

                btnMicroClose.Enabled = false;
                btnMicroUartRefresh.Enabled = true;
                btnMicroOpen.Enabled = true;
            }
        }

        private void btnMicroOpen_Click(object sender, EventArgs e)
        {
            UInt16 rtn;
            if (combMicroUartList.Items.Count <= 0)
                return;
            if (microserialPort.IsOpen)
            {
                return;
            }
            else
            {
                microserialPort.PortName = combMicroUartList.Items[combMicroUartList.SelectedIndex].ToString();

                microserialPort.BaudRate = 9600;

                microserialPort.DataBits = 8;
                microserialPort.StopBits = System.IO.Ports.StopBits.One;

                microserialPort.Parity = System.IO.Ports.Parity.None;

                microserialPort.Handshake = System.IO.Ports.Handshake.None;
                microserialPort.Encoding = Encoding.UTF8;
                try
                {
                    microserialPort.Open();
                }
                catch (Exception bpe)
                {
                    btnMicroClose.Enabled = false;
                    btnMicroUartRefresh.Enabled = true;
                    btnMicroOpen.Enabled = true;
                }
                finally
                {
                    if (microserialPort.IsOpen)
                    {
                        btnMicroClose.Enabled = true;
                        btnMicroUartRefresh.Enabled = false;
                        btnMicroOpen.Enabled = false;
                    }
                }
            }
        }


        static byte HexCharToBinBinChar(byte c)
        {
            if (c >= '0' && c <= '9')
                return (byte)(c - '0');
            else if (c >= 'a' && c <= 'f')
                return (byte)(c - 'a' + 10);
            else if (c >= 'A' && c <= 'F')
                return (byte)(c - 'A' + 10);
            return 0xFF;
        }

        static byte Hex2Bin(byte[] p)
        {
            byte tmp = 0;
            tmp = HexCharToBinBinChar(p[0]);
            tmp <<= 4;
            tmp |= HexCharToBinBinChar(p[1]);
            return tmp;
        }


        void Bin2Hex(byte[] buf, byte dat)
        {
            byte tmp = (byte)(dat >> 4);
            if (tmp <= 9)
                buf[0] = (byte)(0x30 + tmp);
            else
                buf[0] = (byte)(tmp - 10 + 'A');
            tmp = (byte)(dat & 0x0F);
            if (tmp <= 9)
                buf[1] = (byte)(0x30 + tmp);
            else
                buf[1] = (byte)(tmp - 10 + 'A');
        }
        private void btnMicroReadMeas_Click(object sender, EventArgs e)
        {
            if(microserialPort.IsOpen)
            {
                Byte addr = (Byte)numMicroDeviceAddr.Value;
                byte[] tmp2 = new byte[4];
                Bin2Hex(tmp2, addr);
                Byte []tmpbuf = new Byte[32];
                //02 32 32 38 38 30 30 38 03
                tmpbuf[0] = 0x02;
                tmpbuf[1] = 0x32;
                tmpbuf[2] = tmp2[0];// 0x32;
                tmpbuf[3] = tmp2[1];// 0x38;
                tmpbuf[4] = 0x38;
                tmpbuf[5] = 0x30;
                tmpbuf[6] = 0x30;
                tmpbuf[7] = 0x38;
                tmpbuf[8] = 0x03;
                microserialPort.Write(tmpbuf, 0, 9);
                chBMicroMeasUpdate.Checked= false;
            }
        }

        private void btnMicroPressZeroCalc_Click(object sender, EventArgs e)
        {
            if (microserialPort.IsOpen)
            {
                Byte addr = (Byte)numMicroDeviceAddr.Value;
                byte[] tmp2 = new byte[4];
                Bin2Hex(tmp2, addr);
                Byte[] tmpbuf = new Byte[32];
                //02 33 32 38 31 30 30 30 30 30 30 30 30 30 03
                tmpbuf[0] = 0x02;
                tmpbuf[1] = 0x33;
                tmpbuf[2] = tmp2[0];// 0x32;
                tmpbuf[3] = tmp2[1];// 0x38;
                tmpbuf[4] = 0x31;
                tmpbuf[5] = 0x30;

                tmpbuf[6] = 0x30;
                tmpbuf[7] = 0x30;
                tmpbuf[8] = 0x30;
                tmpbuf[9] = 0x30;
                tmpbuf[10] = 0x30;
                tmpbuf[11] = 0x30;
                tmpbuf[12] = 0x30;
                tmpbuf[13] = 0x30;
                tmpbuf[14] = 0x03;
                microserialPort.Write(tmpbuf, 0, 15);
            }
        }


        

        private void btnMicroRangeSet_Click(object sender, EventArgs e)
        {
            if (microserialPort.IsOpen)
            {
                Byte addr = (Byte)numMicroDeviceAddr.Value;
                byte[] tmp2 = new byte[4];
                Bin2Hex(tmp2, addr);
                UInt32 tmp32 = (UInt32)numMicroRangeKpa.Value;
                Byte tmp;
                Byte[] tmpbuf = new Byte[32];
                //02 33 32 38 31 30 30 30 30 30 30 30 30 30 03
                tmpbuf[0] = 0x02;
                tmpbuf[1] = 0x33;
                tmpbuf[2] = tmp2[0];// 0x32;
                tmpbuf[3] = tmp2[1];// 0x38;
                tmpbuf[4] = 0x31;
                tmpbuf[5] = 0x36;
                tmp = (Byte)(tmp32 >> 24);
                Bin2Hex(tmp2, tmp);
                tmpbuf[6] = tmp2[0];
                tmpbuf[7] = tmp2[1];
                tmp = (Byte)(tmp32 >> 16);
                Bin2Hex(tmp2, tmp);
                tmpbuf[8] = tmp2[0];
                tmpbuf[9] = tmp2[1];
                tmp = (Byte)(tmp32 >> 8);
                Bin2Hex(tmp2, tmp);
                tmpbuf[10] = tmp2[0];
                tmpbuf[11] = tmp2[1];
                tmp = (Byte)(tmp32);
                Bin2Hex(tmp2, tmp);
                tmpbuf[12] = tmp2[0];
                tmpbuf[13] = tmp2[1];
                tmpbuf[14] = 0x03;
                microserialPort.Write(tmpbuf, 0, 15);
            }
        }

        private void btnMicroPressCalc_Click(object sender, EventArgs e)
        {
            if (microserialPort.IsOpen)
            {
                Byte addr = (Byte)numMicroDeviceAddr.Value;
                byte[] tmp2 = new byte[4];
                Bin2Hex(tmp2, addr);
                UInt32 tmp32 = (UInt32)numMicroPress.Value;
                Byte tmp;
                Byte[] tmpbuf = new Byte[32];
                //02 33 32 38 31 31 30 30 30 30 30 30 30 30 03
                tmpbuf[0] = 0x02;
                tmpbuf[1] = 0x33;
                tmpbuf[2] = tmp2[0];// 0x32;
                tmpbuf[3] = tmp2[1];// 0x38;
                tmpbuf[4] = 0x31;
                tmpbuf[5] = 0x31;

                tmp = (Byte)(tmp32 >> 24);
                Bin2Hex(tmp2, tmp);
                tmpbuf[6] = tmp2[0];
                tmpbuf[7] = tmp2[1];

                tmp = (Byte)(tmp32 >> 16);
                Bin2Hex(tmp2, tmp);
                tmpbuf[8] = tmp2[0];
                tmpbuf[9] = tmp2[1];

                tmp = (Byte)(tmp32 >> 8);
                Bin2Hex(tmp2, tmp);
                tmpbuf[10] = tmp2[0];
                tmpbuf[11] = tmp2[1];

                tmp = (Byte)(tmp32);
                Bin2Hex(tmp2, tmp);
                tmpbuf[12] = tmp2[0];
                tmpbuf[13] = tmp2[1];

                tmpbuf[14] = 0x03;
                microserialPort.Write(tmpbuf, 0, 15);
            }
        }

        private void btnMicroPressCount_Click(object sender, EventArgs e)
        {
            if (numMicroPress.Value <= 100)
                return;
            if (numMicroRefPress.Value <= 100)
                return;
            double tmpf = (double)numMicroRefPress.Value;// * 100000.0;
            tmpf = tmpf * 100000.0;
            tmpf = tmpf / (float)numMicroPress.Value;
            UInt32 tmp32 = (UInt32)(tmpf);
            tbMicroPressCoef.Text = tmp32.ToString();
        }

        private void btnMicroTempCount_Click(object sender, EventArgs e)
        {
            if (numMicroTemp10.Value <= 1000)
                return;
            if (numMicroRefTemp.Value <= 1000)
                return;
            double tmpf = (double)numMicroRefTemp.Value;// * 100000.0;
            tmpf = tmpf * 100000.0;
            tmpf = tmpf / (float)numMicroTemp10.Value;
            UInt32 tmp32 = (UInt32)(tmpf);
            tbMicroTempCoef.Text = tmp32.ToString();
        }

        private void btnMicroPressCoefSet_Click(object sender, EventArgs e)
        {
            UInt32 tmp32 = UInt32.Parse(tbMicroPressCoef.Text);
            if (tmp32 == 0)
                return;
            if (microserialPort.IsOpen)
            {
                Byte addr = (Byte)numMicroDeviceAddr.Value;
                byte[] tmp2 = new byte[4];
                Bin2Hex(tmp2, addr);
                Byte tmp;
                Byte[] tmpbuf = new Byte[32];
                //02 33 32 38 31 33 30 30 30 30 32 37 31 31 03
                tmpbuf[0] = 0x02;
                tmpbuf[1] = 0x33;
                tmpbuf[2] = tmp2[0];// 0x32;
                tmpbuf[3] = tmp2[1];// 0x38;
                tmpbuf[4] = 0x31;
                tmpbuf[5] = 0x33;

                tmp = (Byte)(tmp32 >> 24);
                Bin2Hex(tmp2, tmp);
                tmpbuf[6] = tmp2[0];
                tmpbuf[7] = tmp2[1];

                tmp = (Byte)(tmp32 >> 16);
                Bin2Hex(tmp2, tmp);
                tmpbuf[8] = tmp2[0];
                tmpbuf[9] = tmp2[1];

                tmp = (Byte)(tmp32 >> 8);
                Bin2Hex(tmp2, tmp);
                tmpbuf[10] = tmp2[0];
                tmpbuf[11] = tmp2[1];

                tmp = (Byte)(tmp32);
                Bin2Hex(tmp2, tmp);
                tmpbuf[12] = tmp2[0];
                tmpbuf[13] = tmp2[1];

                tmpbuf[14] = 0x03;
                microserialPort.Write(tmpbuf, 0, 15);
            }
        }

        private void btnMicroTempCoefSet_Click(object sender, EventArgs e)
        {
            UInt32 tmp32 = UInt32.Parse(tbMicroTempCoef.Text);
            if (tmp32 == 0)
                return;
            if (microserialPort.IsOpen)
            {
                Byte addr = (Byte)numMicroDeviceAddr.Value;
                byte[] tmp2 = new byte[4];
                Bin2Hex(tmp2, addr);
                Byte tmp;
                Byte[] tmpbuf = new Byte[32];
                //02 33 32 38 31 33 30 30 30 30 32 37 31 31 03
                tmpbuf[0] = 0x02;
                tmpbuf[1] = 0x33;
                tmpbuf[2] = tmp2[0];// 0x32;
                tmpbuf[3] = tmp2[1];// 0x38;
                tmpbuf[4] = 0x31;
                tmpbuf[5] = 0x34;

                tmp = (Byte)(tmp32 >> 24);
                Bin2Hex(tmp2, tmp);
                tmpbuf[6] = tmp2[0];
                tmpbuf[7] = tmp2[1];

                tmp = (Byte)(tmp32 >> 16);
                Bin2Hex(tmp2, tmp);
                tmpbuf[8] = tmp2[0];
                tmpbuf[9] = tmp2[1];

                tmp = (Byte)(tmp32 >> 8);
                Bin2Hex(tmp2, tmp);
                tmpbuf[10] = tmp2[0];
                tmpbuf[11] = tmp2[1];

                tmp = (Byte)(tmp32);
                Bin2Hex(tmp2, tmp);
                tmpbuf[12] = tmp2[0];
                tmpbuf[13] = tmp2[1];

                tmpbuf[14] = 0x03;
                microserialPort.Write(tmpbuf, 0, 15);
            }
        }

        byte []rxdbuf=new byte[32];
        int rxdptr = 0;
        int rxdValidPack = 0;
        private void recv_uart(byte dat)
        {
            if (rxdValidPack == 1)           // 有效数据包没有处理，不接收命令
                return;
            if (dat == 0x02)                 // 包头
            {
                rxdptr = 0;
                rxdbuf[rxdptr] = dat;
            }
            else
            {
                rxdptr = rxdptr + 1;
                rxdbuf[rxdptr] = dat;         // 保存数据    
                if (rxdptr >= 63)              // 数据包溢出，丢掉
                {
                    rxdptr = 0;                 // 数据包初始化
                    rxdbuf[rxdptr] = 0;
                }
                if (rxdbuf[0] == 0x02)         // 有效数据包头 
                {
                    if (dat == 0x03)             // end
                        rxdValidPack = 1;       //  有效数据包
                }
                else
                    rxdptr = 0;                 // 丢掉数据
            }
        }

        UInt32 cur_press_pa;
        UInt32 cur_tempx10;
        UInt32 cur_reg;
        UInt32 cur_state;
        UInt32 cur_range;
        byte cur_temp8;

        

        private void micro_meas_update(object sender, EventArgs e)
        {
            if (cur_reg == 0x80 && cur_state == 0x02)
            {
                numMicroPress.Value = cur_press_pa;
                numMicroRangeKpa.Value = cur_range;
                numMicroTemp10.Value = cur_tempx10;
                chBMicroMeasUpdate.Checked = true;
            }

        }

        private void microserialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (microserialPort.IsOpen == false)                       // 如果串口没有被初始化直接退出
                return;
            int byteNum = microserialPort.BytesToRead;
            byte[] buftmp = new byte[byteNum];
            microserialPort.Read(buftmp, 0, byteNum);          // 读到在数据存储到buf
            byte[] buftmp2 = new byte[4];
            for (int i = 0; i < byteNum; i++)
            {
                recv_uart(buftmp[i]);
                if(rxdValidPack==1)
                {
                    rxdValidPack = 0;
                    rxdbuf[0] = 0;
                    cur_reg = rxdbuf[1];
                    buftmp2[0] = rxdbuf[2];
                    buftmp2[1] = rxdbuf[3];
                    cur_state = Hex2Bin(buftmp2);

                    buftmp2[0] = rxdbuf[4];
                    buftmp2[1] = rxdbuf[5];
                    cur_temp8 = Hex2Bin(buftmp2);
                    cur_press_pa = cur_temp8;

                    buftmp2[0] = rxdbuf[6];
                    buftmp2[1] = rxdbuf[7];
                    cur_temp8 = Hex2Bin(buftmp2);
                    cur_press_pa = (cur_press_pa << 8) + cur_temp8;

                    buftmp2[0] = rxdbuf[8];
                    buftmp2[1] = rxdbuf[9];
                    cur_temp8 = Hex2Bin(buftmp2);
                    cur_press_pa = (cur_press_pa << 8) + cur_temp8;

                    buftmp2[0] = rxdbuf[10];
                    buftmp2[1] = rxdbuf[11];
                    cur_temp8 = Hex2Bin(buftmp2);
                    cur_press_pa = (cur_press_pa << 8) + cur_temp8;

                    buftmp2[0] = rxdbuf[12];
                    buftmp2[1] = rxdbuf[13];
                    cur_temp8 = Hex2Bin(buftmp2);
                    cur_tempx10 = cur_temp8;

                    buftmp2[0] = rxdbuf[14];
                    buftmp2[1] = rxdbuf[15];
                    cur_temp8 = Hex2Bin(buftmp2);
                    cur_tempx10 = (cur_tempx10 << 8) + cur_temp8;

                    buftmp2[0] = rxdbuf[16];
                    buftmp2[1] = rxdbuf[17];
                    cur_temp8 = Hex2Bin(buftmp2);
                    cur_range = cur_temp8;

                    buftmp2[0] = rxdbuf[18];
                    buftmp2[1] = rxdbuf[19];
                    cur_temp8 = Hex2Bin(buftmp2);
                    cur_range = (cur_range << 8) + cur_temp8;
                    if(cur_reg==0x80 && cur_state==0x02)
                    {
                        this.Invoke(new EventHandler(micro_meas_update));
                    }
                }
            }
        }

        private void btnMicroWakeup_Click(object sender, EventArgs e)
        {
            if (microserialPort.IsOpen)
            {
                Byte[] tmpbuf = new Byte[32];
                //02 31 00 00 03
                tmpbuf[0] = 0x02;
                tmpbuf[1] = 0x31;
                tmpbuf[2] = 0;
                tmpbuf[3] = 0;
                tmpbuf[4] = 0x03;
                microserialPort.Write(tmpbuf, 0, 5);
            }
        }

        private void btnMicroSleep_Click(object sender, EventArgs e)
        {
            if (microserialPort.IsOpen)
            {
                Byte[] tmpbuf = new Byte[32];
                //02 30 00 00 03
                tmpbuf[0] = 0x02;
                tmpbuf[1] = 0x30;
                tmpbuf[2] = 0;
                tmpbuf[3] = 0;
                tmpbuf[4] = 0x03;
                microserialPort.Write(tmpbuf, 0, 5);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (microserialPort.IsOpen)
            {
                Byte addr = (Byte)numMicroDeviceAddr.Value;
                byte[] tmp2 = new byte[4];
                Bin2Hex(tmp2, addr);
                UInt32 tmp32 = (UInt32)numMicroDeviceAddr.Value;
                Byte tmp;
                Byte[] tmpbuf = new Byte[32];
                //02 33 32 38 31 31 30 30 30 30 30 30 30 30 03
                tmpbuf[0] = 0x02;
                tmpbuf[1] = 0x33;
                tmpbuf[2] = tmp2[0];// 0x32;
                tmpbuf[3] = tmp2[1];// 0x38;
                tmpbuf[4] = 0x32;
                tmpbuf[5] = 0x30;

                tmp = (Byte)(tmp32 >> 24);
                Bin2Hex(tmp2, tmp);
                tmpbuf[6] = tmp2[0];
                tmpbuf[7] = tmp2[1];

                tmp = (Byte)(tmp32 >> 16);
                Bin2Hex(tmp2, tmp);
                tmpbuf[8] = tmp2[0];
                tmpbuf[9] = tmp2[1];

                tmp = (Byte)(tmp32 >> 8);
                Bin2Hex(tmp2, tmp);
                tmpbuf[10] = tmp2[0];
                tmpbuf[11] = tmp2[1];

                tmp = (Byte)(tmp32);
                Bin2Hex(tmp2, tmp);
                tmpbuf[12] = tmp2[0];
                tmpbuf[13] = tmp2[1];

                tmpbuf[14] = 0x03;
                microserialPort.Write(tmpbuf, 0, 15);
            }
        }

        private void btnMicroResetSeting_Click(object sender, EventArgs e)
        {
            if (microserialPort.IsOpen)
            {
                Byte addr = (Byte)numMicroDeviceAddr.Value;
                byte[] tmp2 = new byte[4];
                Bin2Hex(tmp2, addr);
                UInt32 tmp32 = (UInt32)numMicroDeviceAddr.Value;
                Byte tmp;
                Byte[] tmpbuf = new Byte[32];
                //02 33 32 38 31 31 30 30 30 30 30 30 30 30 03
                tmpbuf[0] = 0x02;
                tmpbuf[1] = 0x33;
                tmpbuf[2] = tmp2[0];// 0x32;
                tmpbuf[3] = tmp2[1];// 0x38;
                tmpbuf[4] = 0x32;
                tmpbuf[5] = 0x31;

                tmp = (Byte)(tmp32 >> 24);
                Bin2Hex(tmp2, tmp);
                tmpbuf[6] = tmp2[0];
                tmpbuf[7] = tmp2[1];

                tmp = (Byte)(tmp32 >> 16);
                Bin2Hex(tmp2, tmp);
                tmpbuf[8] = tmp2[0];
                tmpbuf[9] = tmp2[1];

                tmp = (Byte)(tmp32 >> 8);
                Bin2Hex(tmp2, tmp);
                tmpbuf[10] = tmp2[0];
                tmpbuf[11] = tmp2[1];

                tmp = (Byte)(tmp32);
                Bin2Hex(tmp2, tmp);
                tmpbuf[12] = tmp2[0];
                tmpbuf[13] = tmp2[1];

                tmpbuf[14] = 0x03;
                microserialPort.Write(tmpbuf, 0, 15);
            }
        }

        private void ntnMicroPressamp_Click(object sender, EventArgs e)
        {
            if (microserialPort.IsOpen)
            {
                Byte addr = (Byte)numMicroDeviceAddr.Value;
                byte[] tmp2 = new byte[4];
                Bin2Hex(tmp2, addr);
                UInt32 tmp32 = UInt32.Parse(combPressAmp.Text);
                switch(tmp32)
                {
                    case 1:
                        tmp32 = 0;
                        break;
                    case 2:
                        tmp32 = 1;
                        break;
                    case 64:
                        tmp32 = 2;
                        break;
                    case 128:
                        tmp32 = 3;
                        break;
                    default:
                        return;
                        break;
                }
                Byte tmp;
                Byte[] tmpbuf = new Byte[32];
                //02 33 32 38 31 31 30 30 30 30 30 30 30 30 03
                tmpbuf[0] = 0x02;
                tmpbuf[1] = 0x33;
                tmpbuf[2] = tmp2[0];// 0x32;
                tmpbuf[3] = tmp2[1];// 0x38;
                tmpbuf[4] = 0x31;
                tmpbuf[5] = 0x37;

                tmp = (Byte)(tmp32 >> 24);
                Bin2Hex(tmp2, tmp);
                tmpbuf[6] = tmp2[0];
                tmpbuf[7] = tmp2[1];

                tmp = (Byte)(tmp32 >> 16);
                Bin2Hex(tmp2, tmp);
                tmpbuf[8] = tmp2[0];
                tmpbuf[9] = tmp2[1];

                tmp = (Byte)(tmp32 >> 8);
                Bin2Hex(tmp2, tmp);
                tmpbuf[10] = tmp2[0];
                tmpbuf[11] = tmp2[1];

                tmp = (Byte)(tmp32);
                Bin2Hex(tmp2, tmp);
                tmpbuf[12] = tmp2[0];
                tmpbuf[13] = tmp2[1];

                tmpbuf[14] = 0x03;
                microserialPort.Write(tmpbuf, 0, 15);
            }
        }

        private void combFlowPointLow_SelectedIndexChanged(object sender, EventArgs e)
        {
            int tmp = 0;
            switch (combFlowPointLow.SelectedIndex)
            {
                case 0:      // 校准点1     0.50*Qmin
                    tmp = (int)numFlowRefRangeMin.Value;
                    tmp = (int)(tmp * 0.5);
                    break;
                case 1:      // 校准点2     1.00*Qmin
                    tmp = (int)numFlowRefRangeMin.Value;
                    break;
                case 2:      // 校准点3     0.10*Qmax
                    tmp = (int)numFlowRefRangeMax.Value;
                    tmp = (int)(tmp * 0.10);
                    break;
                case 3:      // 校准点4     0.15*Qmax
                    tmp = (int)numFlowRefRangeMax.Value;
                    tmp = (int)(tmp * 0.15);
                    break;
                case 4:      // 校准点5     0.20*Qmax
                    tmp = (int)numFlowRefRangeMax.Value;
                    tmp = (int)(tmp * 0.20);
                    break;
                case 5:      // 校准点6     0.25*Qmax
                    tmp = (int)numFlowRefRangeMax.Value;
                    tmp = (int)(tmp * 0.25);
                    break;
                case 6:      // 校准点7     0.40*Qmax
                    tmp = (int)numFlowRefRangeMax.Value;
                    tmp = (int)(tmp * 0.40);
                    break;
                case 7:      // 校准点8     0.55*Qmax
                    tmp = (int)numFlowRefRangeMax.Value;
                    tmp = (int)(tmp * 0.55);
                    break;
                case 8:      // 校准点9     0.70*Qmax
                    tmp = (int)numFlowRefRangeMax.Value;
                    tmp = (int)(tmp * 0.70);
                    break;
                case 9:      // 校准点10     0.85*Qmax
                    tmp = (int)numFlowRefRangeMax.Value;
                    tmp = (int)(tmp * 0.85);
                    break;
                case 10:      // 校准点11     1.00*Qmax
                    tmp = (int)numFlowRefRangeMax.Value;
                    break;
                case 11:      // 校准点12     1.25*Qmax
                    tmp = (int)numFlowRefRangeMax.Value;
                    tmp = (int)(tmp * 1.25);
                    break;
                case 12:      // 校准点13     1.50*Qmax
                    tmp = (int)numFlowRefRangeMax.Value;
                    tmp = (int)(tmp * 1.50);
                    break;
                default:
                    break;
            }
            textBObjFlowLow.Text = tmp.ToString();
        }

        private void btnY1_Click(object sender, EventArgs e)
        {
            UInt32 tmp = UInt32.Parse(textBoxY1.Text);
            if (tmp == 1000)
                return;
            chartOnline.ChartAreas[0].AxisY.Maximum = tmp;// 4000;
            //chartOnline.ChartAreas[0].AxisY2.Maximum = tmp;
        }

        private void btnY2_Click(object sender, EventArgs e)
        {
            UInt32 tmp = UInt32.Parse(textBoxY2.Text);
            if (tmp == 1000)
                return;
            //chartOnline.ChartAreas[0].AxisY.Maximum = 2000;// 4000;
            chartOnline.ChartAreas[0].AxisY2.Maximum = tmp;
        }

        public String ListStringTmp="";

        public UInt32 PressPIDUpdate_DataToText(UInt16 regaddr, UInt16 dat)
        {
            UInt32 rtn = 0;
            String rtnStr;
            // 流量计量参数恒定值  
            switch (regaddr)
            {
                // PID参数
                case PressPIDModus.REG_sysPID_FenMu:         // PID参数的分母，缺省10000
                    numPressPIDFenMu.Value = dat;
                    break;
                case PressPIDModus.REG_sysPID_NormKp:          // 正常工作PID调压参数分子 比例  
                    numPressPIDKp.Value = dat;
                    break;
                case PressPIDModus.REG_sysPID_NormKi:          // 正常PID调压参数分子 微分
                    numPressPIDKi.Value = dat;
                    break;
                case PressPIDModus.REG_sysPID_NormKd:          // 正常PID调压参数分子 积分
                    numPressPIDKd.Value = dat;
                    break;
                case PressPIDModus.REG_sysPID_NormInterval:          // 正常PID调压时间间隔ms
                    numPressPIDTms.Value = dat;
                    break;
                case PressPIDModus.REG_sysPID_IncKp:          // 匀速PID调压参数分子 比例  
                    numPressPIDIncKp.Value = dat;
                    break;
                case PressPIDModus.REG_sysPID_IncKi:          // 匀速PID调压参数分子 微分
                    numPressPIDIncKi.Value = dat;
                    break;
                case PressPIDModus.REG_sysPID_IncKd:          // 匀速PID调压参数分子 积分
                    numPressPIDIncKd.Value = dat;
                    break;
                case PressPIDModus.REG_sysPID_IncInterval:          // 匀速PID调压时间间隔ms
                    numPressPIDIncTms.Value = dat;
                    break;
                case PressPIDModus.REG_sysPID_IncMaxTime:          // 匀速加压最大时间ms
                    numPressPIDIncAllTms.Value = dat;
                    break;
                case PressPIDModus.REG_sysPID_MaxSpeed:         // 电机最大步数
                    numPressPIDStepMax.Value = dat;
                    break;
                case PressPIDModus.REG_sysPID_DiffMin:         // 压力差值小于该值不调压
                    numPressPIDDiffMin.Value = dat;
                    break;
                case PressPIDModus.REG_sysPID_DiffMax:         // 压力差值大于该值最大调压
                    numPressPIDDiffMax.Value = dat;
                    break;


                // 用户配置压力和流量
                case PressPIDModus.REG_sysPressOutObj:        // 输出目标压力 
                    numPressUserObj.Value = DataTrans.trans_uint16_t_to_uint32(dat);
                    break;
                case PressPIDModus.REG_sysPressOutObjMax:     // 输出最高目标压力 
                    numPressUserObjMax.Value= DataTrans.trans_uint16_t_to_uint32(dat);
                    break;
                case PressPIDModus.REG_sysPressOutObjMin:     // 输出最低目标压力 
                    numPressUserObjMin.Value = DataTrans.trans_uint16_t_to_uint32(dat);
                    break;
                case PressPIDModus.REG_sysPressFlowLimitMax:     // 最大瞬时流量限制 
                    numFlowUserCurMax.Value = dat;
                    break;
                case PressPIDModus.REG_sysPressFlowLimitMin:     // 低流瞬时流量限制 
                    numFlowUserCurMin.Value = dat;
                    break;
                case PressPIDModus.REG_sysPressFlowSumLimitL:     // 累计流量限制low 16
                    rtn = (UInt32)numFlowUserSumMax.Value;
                    rtn = (rtn & 0xFFFF0000)+dat;
                    numFlowUserSumMax.Value = rtn;
                    break;
                case PressPIDModus.REG_sysPressFlowSumLimitH:     // 累计流量限制high 16
                    rtn = (UInt32)numFlowUserSumMax.Value;
                    rtn = (UInt32)((rtn & 0x0000FFFF) + (dat<<16));
                    numFlowUserSumMax.Value = rtn;
                    break;

                // 读取软件和硬件版本
                case PressPIDModus.REG_sysSoftVer:     // 软件版本
                    textBSoftVer.Text = dat.ToString();
                    break;
                case PressPIDModus.REG_sysHradVer:     // 硬件版本
                    textBHardVer.Text = dat.ToString();
                    break;

                // Modbus模式
                case PressPIDModus.REG_sysModbusSel:     // Modbus模式选择，输入正确密码进入，错误密码退出
                    if(dat<4)
                        combModbusList.SelectedIndex = dat;
                    switch(dat)
                    {
                        case 0:
                            btnModbusUserChange.Enabled = false;
                            btnModbusAdminChange.Enabled = false;
                            break;
                        case 1:
                            btnModbusUserChange.Enabled = true;
                            btnModbusAdminChange.Enabled = false;
                            break;
                        case 2:
                            btnModbusUserChange.Enabled = true;
                            btnModbusAdminChange.Enabled = true;
                            break;
                        case 3:
                            btnModbusUserChange.Enabled = true;
                            btnModbusAdminChange.Enabled = true;
                            break;
                    }
                    break;
                case PressPIDModus.REG_sysModbusUser:     // Modbus用户模式
                    if (dat > 0)
                        numModbusUser.Value = dat;
                    break;
                case PressPIDModus.REG_sysModbusAdmin:    // Modbus管理模式
                    if (dat > 0)
                        numModbusAdmin.Value = dat;
                    break;
                case PressPIDModus.REG_sysModbusSupper:     // Modbus超级管理员
                    if (dat > 0)
                        numModbusSupper.Value = dat;
                    break;

                // 用户ID
                case PressPIDModus.REG_sysDeviceID_L://            REG_Sys_PressStart+30        // YY
                    rtn = (UInt32)numDeviceID.Value;
                    rtn = (UInt32)((rtn&0xFFFF0000)+dat);
                    numDeviceID.Value = rtn;
                    break;
                case PressPIDModus.REG_sysDeviceID_H://           REG_Sys_PressStart+31        // MM
                    rtn = (UInt32)numDeviceID.Value;
                    rtn = (UInt32)(rtn & 0x0000FFFF);
                    rtn = (UInt32)(rtn + (dat << 16));
                    numDeviceID.Value = rtn;
                    break;
                case PressPIDModus.REG_sysRunMode:
                    if(dat<=6)
                        combRunModeSel.SelectedIndex = dat;
                    break;
                case PressPIDModus.REG_sysRunKeyMode://           REG_Sys_PressStart+33     // 0: 现场控制， 1: 托管 
                    if (dat >= 1)
                        dat = 1;
                    combRunKeyControl.SelectedIndex = dat;
                    break;
                case PressPIDModus.REG_sysStartMode://             REG_Sys_PressStart+34     // 0: 停止调压， 1: 自动调压
                    if (dat >= 1)
                        dat = 1;
                    if (dat == 0)
                        checkBRunStatus.Checked = false;
                    else
                        checkBRunStatus.Checked = true;
                    break;
                case PressPIDModus.REG_sysIdleValveStaus://        REG_Sys_PressStart+35     // 0: 断电关阀口， 1：断电打开阀口
                    if (dat >= 1)
                        dat = 1;
                    combValveOff.SelectedIndex = dat;
                    break;
                case PressPIDModus.REG_sysValveType://             REG_Sys_PressStart+36     // 阀类型：DN40,DN80,DN100，DN150, DN200,DN250
                    if (dat >= 5)
                        dat = 5;
                    combValveType.SelectedIndex = dat;
                    break;

                case PressPIDModus.REG_sysModbusAddr://            REG_Sys_PressStart+37     // Modbus设备地址
                    numDeviceAddr.Value = dat;
                    break;
                case PressPIDModus.REG_sysModbudBaud://            REG_Sys_PressStart+38     // Modbus波特率 
                    if (dat >= 3)
                        dat = 3;
                    combDeviceBaud.SelectedIndex = dat;
                    break;

                // 累计流量数据
                case PressPIDModus.REG_FlowResvSumL:             // 剩余累计流量
                    listFlowSum.Items.Clear();
                    DataTrans.press_float.b1 = (byte)(dat>>8);
                    DataTrans.press_float.b0 = (byte)(dat);
                    break;
                case PressPIDModus.REG_FlowResvSumH:             // 剩余累计流量
                    DataTrans.press_float.b3 = (byte)(dat >> 8);
                    DataTrans.press_float.b2 = (byte)(dat);
                    listFlowSum.Items.Add(String.Format("剩余累计流量: {0:f3}", DataTrans.press_float.f));
                    break;
                case PressPIDModus.REG_FlowSumL:             // 累计流量
                    DataTrans.press_float.b1 = (byte)(dat >> 8);
                    DataTrans.press_float.b0 = (byte)(dat);
                    break;
                case PressPIDModus.REG_FlowSumH:             // 累计流量
                    DataTrans.press_float.b3 = (byte)(dat >> 8);
                    DataTrans.press_float.b2 = (byte)(dat);
                    listFlowSum.Items.Add(String.Format("  总累计流量: {0:f3}", DataTrans.press_float.f));
                    break;

                // 日期
                case PressPIDModus.REG_sysSyncYear:             // year
                    rtnStr = String.Format("{0:D4}",dat);
                    textBDeviceDate.Text = rtnStr;
                    break;
                case PressPIDModus.REG_sysSyncMD:             // month,day
                    rtnStr=textBDeviceDate.Text+"/"+String.Format("{0:D2}", (dat>>8));
                    rtnStr = rtnStr+"/"+ String.Format("{0:D2}", (dat&0x0FF));
                    textBDeviceDate.Text = rtnStr;
                    break;
                case PressPIDModus.REG_sysSyncWH:             // week,hour
                    rtnStr = textBDeviceDate.Text + " W" + String.Format("{0:D2}", (dat >> 8));
                    rtnStr = rtnStr + " " + String.Format("{0:D2}", (dat & 0x0FF));
                    textBDeviceDate.Text = rtnStr;
                    break;
                case PressPIDModus.REG_sysSyncMS:             // minute,second
                    rtnStr = textBDeviceDate.Text + ":" + String.Format("{0:D2}", (dat >> 8));
                    rtnStr = rtnStr + ":" + String.Format("{0:D2}", (dat & 0x0FF));
                    textBDeviceDate.Text = rtnStr;
                    break;

                // 错误记录
                case PressPIDModus.REG_ErrorYyMm0:             // 错误日志
                    listPressError.Items.Clear();
                    listPressError.Items.Add("错误日期   错误类型");
                    ListStringTmp = dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorDdHh0:             // 错误日志
                    ListStringTmp = ListStringTmp + " "+dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorMmSs0:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorType0:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    listPressError.Items.Add(ListStringTmp);
                    break;
                case PressPIDModus.REG_ErrorYyMm1:             // 错误日志
                    ListStringTmp = dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorDdHh1:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorMmSs1:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorType1:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    listPressError.Items.Add(ListStringTmp);
                    break;
                case PressPIDModus.REG_ErrorYyMm2:             // 错误日志
                    ListStringTmp = dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorDdHh2:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorMmSs2:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorType2:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    listPressError.Items.Add(ListStringTmp);
                    break;
                case PressPIDModus.REG_ErrorYyMm3:             // 错误日志
                    ListStringTmp = dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorDdHh3:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorMmSs3:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorType3:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    listPressError.Items.Add(ListStringTmp);
                    break;
                case PressPIDModus.REG_ErrorYyMm4:             // 错误日志
                    ListStringTmp = dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorDdHh4:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorMmSs4:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorType4:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    listPressError.Items.Add(ListStringTmp);
                    break;
                case PressPIDModus.REG_ErrorYyMm5:             // 错误日志
                    ListStringTmp = dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorDdHh5:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorMmSs5:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorType5:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    listPressError.Items.Add(ListStringTmp);
                    break;
                case PressPIDModus.REG_ErrorYyMm6:             // 错误日志
                    ListStringTmp = dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorDdHh6:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorMmSs6:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    break;
                case PressPIDModus.REG_ErrorType6:             // 错误日志
                    ListStringTmp = ListStringTmp + " " + dat.ToString();
                    listPressError.Items.Add(ListStringTmp);
                    break;
                default:
                    break;
            }
            return rtn;
        }



    }
}
