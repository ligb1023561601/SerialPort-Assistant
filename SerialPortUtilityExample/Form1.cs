using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;//绘图所用
using System.Threading;
namespace SerialPortUtilityExample
{
    public partial class Form1 : Form
    {
        private SerialPort sp = null;//声明一个串口类
        private bool isOpen = false;//打开串口的标志
        private bool isSetProperty = false;//
        private bool isHex = false;//
        private bool isTimingSend = false;
        private List<int> Xbufferlist = new List<int>();
        private List<int> Ybufferlist = new List<int>();
        private int index = 0;
        

        public void WaveDraw()
        {
            this.Invoke((EventHandler)delegate
            {
                chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true;//如果chart中是0个数据，那么网格自动计算大小的时候就会除以零，然后就出错了
                chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
                chart1.Series[0].Points.DataBindXY(Xbufferlist, Ybufferlist);
                chart1.DataBind();
                
            });
            
        }
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 窗体加载时的各项初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            //最大化最小化选项的设置
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.MaximizeBox = true;
            //添加背景图片
            this.BackgroundImage= Image.FromFile(@"E:\Picture\htc\photo\b9f140e9f69d75c528534b72c773d49c.jpg");
            //添加串口号选项，COM0~COM1
            for (int i = 0; i < 10; i++)
            {
                cbxCOMPort.Items.Add("COM"+(i+1).ToString());
            }
            //cbxCOMPort.SelectedIndex = 0;//设置默认显示的项,不写就不显示
            //列出常用的波特率
            cbxBaudRate.Items.Add("1200");
            cbxBaudRate.Items.Add("2400");
            cbxBaudRate.Items.Add("4800");
            cbxBaudRate.Items.Add("9600");
            cbxBaudRate.Items.Add("19200");
            cbxBaudRate.Items.Add("38400");
            cbxBaudRate.Items.Add("43000");
            cbxBaudRate.Items.Add("56000");
            cbxBaudRate.Items.Add("57600");
            cbxBaudRate.Items.Add("115200");
            cbxBaudRate.SelectedIndex = 5;//设置默认显示的项,不写就不显示
            //列出停止位
            cbxStopBits.Items.Add("0");
            cbxStopBits.Items.Add("1");
            cbxStopBits.Items.Add("1.5");
            cbxStopBits.Items.Add("2");
            cbxStopBits.SelectedIndex = 1;//设置默认显示的项,不写就不显示
            //列出数据位
            cbxDataBits.Items.Add("8");
            cbxDataBits.Items.Add("7");
            cbxDataBits.Items.Add("6");
            cbxDataBits.Items.Add("5");
            cbxDataBits.SelectedIndex = 0;//设置默认显示的项,不写就不显示
            //列出奇偶校验位
            cbxParity.Items.Add("无");
            cbxParity.Items.Add("奇校验");
            cbxParity.Items.Add("偶校验");
            cbxParity.SelectedIndex = 0;//设置默认显示的项,不写就不显示
            //默认为字符显式
            rbnChar.Checked = true;
            chart1.Titles.Add("显示波形");
           // chart1.Series[0].LegendText = "Waveform";
            // 画样条曲线（Spline）
            chart1.Series[0].ChartType = SeriesChartType.Line;
            
            // 线宽2个像素
            chart1.Series[0].BorderWidth = 2;
            // 线的颜色：红色
            chart1.Series[0].Color = System.Drawing.Color.Red;
            // 图示上的文字
            chart1.Series[0].LegendText = "收数显示";
            
            chart1.ChartAreas[0].AxisX.Enabled = AxisEnabled.True;
            chart1.ChartAreas[0].AxisY.Enabled = AxisEnabled.True;
            chart1.ChartAreas[0].AxisX.Name = "数据编号";
            chart1.ChartAreas[0].AxisY.Name = "数值";
            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false ;
            chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false ;
            chart1.ChartAreas[0].AxisX.Title = "数据编号";
            chart1.ChartAreas[0].AxisY.Title = "数值";
            chart1.ChartAreas[0].AxisX.MajorGrid.Interval = 10;
            chart1.ChartAreas[0].AxisY.MajorGrid.Interval = 0.5;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            chart1.ChartAreas[0].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;

            chart1.ChartAreas[0].CursorX.AutoScroll = true;
            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].CursorX.LineColor = Color.Blue;
            chart1.ChartAreas[0].CursorY.AutoScroll = true;
            chart1.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].CursorY.LineColor = Color.Blue;
            //chart1.ChartAreas[0].AxisX.Maximum=10;
            //chart1.ChartAreas[0].AxisX.Minimum=0;
            //chart1.ChartAreas[0].AxisY.Maximum=10;
            //chart1.ChartAreas[0].AxisY.Minimum=0;
            //chart1.Series[0].Points.AddXY(0, 0);
            
            
        }
        /// <summary>
        /// 串口检测功能,检测到的串口才予以显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCheckCOM_Click(object sender, EventArgs e)
        {
            bool isexistence = false;
            cbxCOMPort.Items.Clear();
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    //以各个COM序号去检测是否有相应串口存在，用serialname方法比这个好使
#if false
       string[] serialName= SerialPort.GetPortNames();
                    if (serialName!=null)
	{   
        foreach(string item in serialName)
        {
             cbxCOMPort.Items.Add(item);
        }
                        isexistence=true;
		
	}
       
#endif
                    SerialPort sp=new SerialPort("COM"+(i+1).ToString());
                    sp.Open();
                    sp.Close();
                    cbxCOMPort.Items.Add("COM" + (i + 1).ToString());
                    isexistence =true;
                    
                }
                catch (Exception)
                {
                    continue; 
                }
                cbxCOMPort.SelectedIndex = 0;//用来检测是否选到东西了,并显示一个可用的串口
                
            }

            if (!isexistence)
            {
                MessageBox.Show("没有找到可用的串口！", "错误提示");
            } 
        }
        /// <summary>
        /// 检测串口是否设置
        /// </summary>
        /// <returns></returns>
        private bool CheckPortSetting()
        {
            if (cbxCOMPort.Text.Trim()=="")//加上trim，不然可能有问题
            {
                return false;
            }
            if (cbxBaudRate.Text.Trim() == "")
            {
                return false;
            }
            if (cbxDataBits.Text.Trim() == "")
            {
                return false;
            }
            if (cbxParity.Text.Trim() == "")
            {
                return false;
            }
            if (cbxStopBits.Text.Trim() == "")
            {
                return false;
            }
            return true;
        }
       
        /// <summary>
        /// 检测是否有发送的数据
        /// </summary>
        /// <returns></returns>
        private bool CheckSendData()
        {
            if (tbxSendData.Text.Trim() == "")
                return false;
            else
                return true;
        }

        /// <summary>
        /// 设置要打开的串口的属性
        /// </summary>
        private void SetPortProperty()
        {
            sp = new SerialPort();
            sp.PortName = cbxCOMPort.Text.Trim();//设置名称
            sp.BaudRate = Convert.ToInt32(cbxBaudRate.Text.Trim());//波特率
            float f = Convert.ToSingle(cbxStopBits.Text.Trim());//停止位
            if (f==0)
            {
                sp.StopBits = StopBits.None;//不使用停止位
                
            }
            else if (f==1.5)
            {
                sp.StopBits = StopBits.OnePointFive;
            }
            else if (f==1)
            {
                sp.StopBits = StopBits.One;
            }
            else if (f==2)
            {
                sp.StopBits = StopBits.Two;
            }
            else
            {
                sp.StopBits = StopBits.One;
            }

            sp.DataBits = Convert.ToInt16(cbxDataBits.Text.Trim());//设置数据位

            string s = cbxParity.Text.Trim();//设置奇偶校验位

            if (s.CompareTo("无")==0)
            {
                sp.Parity = Parity.None;
            }
            else if (s.CompareTo("奇校验") == 0)
            {
                sp.Parity = Parity.Odd;
            }
            else if (s.CompareTo("偶校验") == 0)
            {
                sp.Parity = Parity.Even;
            }
            else
            {
                sp.Parity = Parity.None;
            }
            
            
            sp.ReadTimeout = -1;//-1代表的是读取超时时间设置为无穷
            sp.RtsEnable = true;//启用请求发送信号

            
            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);

        }

       
        /// <summary>
        /// 定义收到数据的Receive事件，跟其他的控件的事件注册是一样的，只不过那些是在designer那边而已
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            System.Threading.Thread.Sleep(100);//延时100ms等待串口数据接受完再开始处理
            //this.Invoke是跨线程访问ui，其实感觉chart放这里面应该能行，若是数据的处理放在另一个线程中进行，那么
            //可考虑采用全局队列queue来储存收到的数据
            this.Invoke((EventHandler)(delegate
            {
                if (isHex==false)
                {
                    //tbxRecvData.Text += sp.ReadLine();
                    //tbxRecvData.Text += " ";
                    Byte[] RecvByteBuffer=new Byte[sp.BytesToRead];
                    sp.Read(RecvByteBuffer, 0, sp.BytesToRead);
                    string str=null;
                    foreach (var item in RecvByteBuffer)
                    {
                        if (item!=32)
                        {
                            
                            str += item.ToString();
                            str += " ";
                            Ybufferlist.Add(item);
                            Xbufferlist.Add(index++);
                        }
                        
                    }
                    tbxRecvData.Text += str;
                    Thread DrawThread = new Thread(new ThreadStart(WaveDraw));
                    DrawThread.Start();
                   
                    //string[] str = tbxRecvData.Text.Split(' ');//将收到的字符串进行分解
                    //int[] values = new int[1];//这里处理一下，否则空字符也会占用一个位置
                    //for (int i = 0; i < str.Length; i++)
                    //{
                    //    if (str[i]!="")
                    //    {
                    //        if (i==values.Length)
                    //        {
                    //            int[] newvalues = new int[i + 10];
                    //            values.CopyTo(newvalues, 0);
                    //            values = newvalues;
                    //            try
                    //            {
                    //                values[i] = int.Parse(str[i]);
                                    
                    //            }
                    //            catch (Exception)
                    //            {
                    //                MessageBox.Show("输入的不是整型数据!", "错误提示");
                    //            }
                    //        }
                    //        else
                    //        {
                    //            try
                    //            {
                    //                values[i] = int.Parse(str[i]);
                    //            }
                    //            catch (Exception)
                    //            {
                    //                MessageBox.Show("输入的不是整型数据!", "错误提示");
                    //            }

                    //        }
                            
                    //    }
                    //    else
                    //    {
                    //        continue;
                    //    }
                        
                    //}
                
                    //foreach (var item in values)
                    //{
                    //    Ybufferlist.Add(item);
                    //    Xbufferlist.Add(index++);

                    //}
                    //while (Xbufferlist.Count > 10)//这可以只显示一部分数据
                    //{
                    //    Xbufferlist.RemoveAt(0);
                    //    Ybufferlist.RemoveAt(0);
                    //}
                   
                    
                    // 在chart中显示数据
                    //int x = 0;
                    //for (int i = 0; i < str.Length-1; i++)
                    {
                        //series.Points.AddY(values[i]);
                    }
                    //foreach (float v in values)
                    //{
                    //    series.Points.AddY(v);
                    //    //x++;
                    //}

                    // 设置显示范围
                   // ChartArea chartArea = chart1.ChartAreas[0];
                    //chartArea.AxisX.Minimum = 0;
                    //chartArea.AxisX.Maximum = 10;
                    //chartArea.AxisY.Minimum = 0d;
                    //chartArea.AxisY.Maximum = 100d;

                }
                else
                {
                    Byte[] ReceivedData=new Byte[sp.BytesToRead];//创建接收字节数组
                    sp.Read(ReceivedData,0,ReceivedData.Length);//读取所接收到的数据
                    string RecvDataText=null;
                    for (int i = 0; i < ReceivedData.Length; i++)
                    {
                        if ((ReceivedData[i]!=0x0a))//WriteLine带来的转行符号
                        {
                            ReceivedData[i] -= 0x30;//转换ascll码
                            RecvDataText += ("0x" + ReceivedData[i].ToString("X2") + " ");//X2表示输出2位的16进制数
                        }
                        else
                        {
                            continue;
                        }
                    }
                    tbxRecvData.Text+=RecvDataText;
                }
                sp.DiscardInBuffer();//丢弃接收缓冲区数据
            }));

        }
        /// <summary>
        /// 打开串口，一个按钮有两种用的例子！！
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenCOM_Click(object sender, EventArgs e)
        {
            if (isOpen==false)
            {
                if (!CheckPortSetting())//检查各项是否选好
                {
                    MessageBox.Show("串口未设置！","错误提示");
                    return;
                }
                if (!isSetProperty)//讲选好的属性进行设置
                {
                    SetPortProperty();
                    isSetProperty = true;
                }
                try//打开串口
                {
                    sp.Open();
                    isOpen = true;
                    btnOpenCOM.Text = "关闭串口";
                    //串口打开后则相关的串口设置按钮不再可用
                    cbxCOMPort.Enabled = false;
                    cbxBaudRate.Enabled = false;
                    cbxDataBits.Enabled = false;
                    cbxParity.Enabled = false;
                    cbxStopBits.Enabled = false;
                    //rbnChar.Enabled = false;
                    //rbnHex.Enabled = false;
                }
                catch (Exception)
                {
                    //打开串口失败后，相应的标志位取消
                    isSetProperty = false;
                    isOpen = false;
                    MessageBox.Show("串口无效或正被占用！", "错误提示");
                }
            }
            else
            {
                try//关闭串口
                {
                    sp.Close();
                    isOpen = false;
                    btnOpenCOM.Text = "打开串口";
                    //串口关闭后则相关的串口设置按钮可用
                    cbxCOMPort.Enabled = true;
                    cbxBaudRate.Enabled = true;
                    cbxDataBits.Enabled = true;
                    cbxParity.Enabled = true;
                    cbxStopBits.Enabled = true;
                    //rbnChar.Enabled = true;
                    //rbnHex.Enabled = true;
                }
                catch (Exception)
                {
                    MessageBox.Show("串口关闭失败！", "错误提示");
                }
            }
        }
        /// <summary>
        /// 发送串口数据，若是发送数据也给开一个新的线程，那么在发送时还能对窗体进行操作，但现在暂时没必要
        /// 用writeline会带上换行符也就是0x0A
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (isOpen)
            {
                try
                {
                    sp.Write(tbxSendData.Text.Trim());//传输数据
                }
                catch (Exception)
                {
                    MessageBox.Show("发送数据时发生错误", "错误提示");
                    return;
                }
            }
            else
            {
                MessageBox.Show("串口未打开！", "错误提示");
                return;
            }
            if (!CheckSendData())
            {
                MessageBox.Show("请输入要发送的数据！", "错误提示");
                return;
            }
        }
        /// <summary>
        /// 清空所有数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCleanData_Click(object sender, EventArgs e)
        {
            tbxRecvData.Text = "";
            tbxSendData.Text = "";
            chart1.Series[0].Points.Clear();
        }
        /// <summary>
        /// 以十六进制进行显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbnHex_CheckedChanged(object sender, EventArgs e)
        {
            isHex = true;
        }
        /// <summary>
        /// 以字符进行显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbnChar_CheckedChanged(object sender, EventArgs e)
        {
            isHex = false;
        }


        /// <summary>
        /// 关闭时的确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result= MessageBox.Show("确定要退出此助手？", "提示信息", MessageBoxButtons.YesNoCancel);
            //不加此句那就点哪个按钮都是关闭
            if (result!=DialogResult.Yes)
            {
                e.Cancel = true;
            }
        }
        /// <summary>
        /// 定时发送按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_SendBtn_Click(object sender, EventArgs e)
        {
            if (!isTimingSend)
            {
                Sendtimer.Start();
                isTimingSend = true;
                Timer_SendBtn.Text = "停止发送";
            }
            else
            {
                Sendtimer.Stop();
                isTimingSend = false;
                Timer_SendBtn.Text = "定时发送";
            }
            
        }
        /// <summary>
        /// 定时发送定时器Tick事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sendtimer_Tick(object sender, EventArgs e)
        {
            btnSend.PerformClick();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset();
            chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset();
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                HitTestResult myTestResult = chart1.HitTest(e.X, e.Y);
                if (myTestResult.ChartElementType == ChartElementType.DataPoint)
                {
                    this.Cursor = Cursors.Cross;
                    int i = myTestResult.PointIndex;
                    DataPoint dp = myTestResult.Series.Points[i];

                    double doubleXValue = dp.XValue;
                    double doubleYValue = dp.YValues[0];


                    toolTip1.SetToolTip(chart1, "数值：" + doubleYValue.ToString() + Environment.NewLine + "编号:" + (doubleXValue).ToString());
                }
                else
                {
                    this.Cursor = Cursors.Default;
                }
            }
            catch (System.Exception ex)
            {

            }          
        }

     
      

       
       

        
        
    }
}
