using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace rbo
{
    public partial class RbtHome : Form
    {
        public RbtHome()
        {
           
            InitializeComponent();
            bar();
        }
        string receiveMode = "HEX模式";
        string receiveCoding = "GBK";
        string sendMode = "HEX模式";
        string sendCoding = "GBK";

        Double link1 = 61;  //一关节的长度
        Double link2 = 61;  //二关节的长度
        Double joint1;      //一关节的角度
        Double joint2;      //二关节的角度
        Double RbtX = 104.127;      //初始化机械臂X轴坐标
        Double RbtY = 43.127;       //初始化机械臂Y轴坐标

        int num1 = 90;
        int num2 = 90;
        int num3 = 90;
        int num4 = 90;
        int num5 = 90;
        int num6 = 180;
        int num7 = 50; // 定义一个机器人速度

        Boolean flagrse = false;


        double lj1, lj2, lj3, lj4;

        List<byte> byteBuffer = new List<byte>();       //接收字节缓存区

        Socket tcpServer;
        Thread connectThread; //连接线程
        Socket clientSocket;

        public void init()
        {
         
            //新建一个接收现场
            Thread ClientRecive = new Thread(Recive);
            //ClientRecive.Start();
          
            ClientRecive.Start();
        }


        private void bar()
        {
            ProgressBar h = new ProgressBar();
            h.Location = new Point(920, 352);
            h.Size = new Size(200, 30);

            h.Maximum = 100;
            h.Value = 40;

            h.Style = ProgressBarStyle.Continuous;
            h.ForeColor = Color.Black;

            this.Controls.Add(h);

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            Thread Running = new Thread(run);
            if (btnOpen.Text == "连接机器人")
            {
                OpenSerialPort();
                tbReceive.AppendText("机器人连接成功...");       //字节流转文本
                                                          // 新建心跳包现场
                
                Running.Start();
            }
            else if (btnOpen.Text == "关闭机器人")
            {
                Running.Abort();
                CloseSerialPort();

               
            }
        }

        private void OpenSerialPort()       //打开串口
        {
            try
            {
                serialPort.PortName = cbPortName.Text;
                serialPort.BaudRate = Convert.ToInt32(cbBaudRate.Text);
                serialPort.DataBits = Convert.ToInt32(cbDataBits.Text);
                StopBits[] sb = { StopBits.One, StopBits.OnePointFive, StopBits.Two };
                serialPort.StopBits = sb[cbStopBits.SelectedIndex];
                Parity[] pt = { Parity.None, Parity.Odd, Parity.Even };
                serialPort.Parity = pt[cbParity.SelectedIndex];
                serialPort.Open();

                btnOpen.BackColor = Color.Pink;
                btnOpen.Text = "关闭机器人";
                
                cbPortName.Enabled = false;
                cbBaudRate.Enabled = false;
                cbDataBits.Enabled = false;
                cbStopBits.Enabled = false;
                cbParity.Enabled = false;
                // 初始化空间条
                trackBar1.Value = 90;
                trackBar2.Value = 90;
                trackBar3.Value = 90;
                trackBar4.Value = 90;
              

                label1.Text = "90°";
                label15.Text = "90°";
                label16.Text = "90°";
                label17.Text = "90°";

               

                textBox3.Text = "90";
                textBox4.Text = "90";
                textBox6.Text = "90";
                textBox5.Text = "90";
            

                int res = (int)((90) / (double)180 * 500);
                
               
                
                
            }
            catch
            {
                MessageBox.Show("串口打开失败", "提示");
            }
        }

        private void CloseSerialPort()      //关闭串口
        {
            serialPort.Close();

            btnOpen.BackColor = SystemColors.ControlLight;
            btnOpen.Text = "连接机器人";
           
            cbPortName.Enabled = true;
            cbBaudRate.Enabled = true;
            cbDataBits.Enabled = true;
            cbStopBits.Enabled = true;
            cbParity.Enabled = true;
        }

        // 串口心跳包
        public void run()
        {
            while (true)
            {
                pictureBox1.BackColor = Color.Green;        //改变PictureBox的颜色
                Thread.Sleep(500);
                pictureBox1.BackColor = Color.White;        //改变PictureBox的颜色
                Thread.Sleep(500);
            }

        }

   
        private void cbPortName_DropDown(object sender, EventArgs e)
        {
            string currentName = cbPortName.Text;
            string[] names = SerialPort.GetPortNames();       //搜索可用串口号并添加到下拉列表
            cbPortName.Items.Clear();
            cbPortName.Items.AddRange(names);
            cbPortName.Text = currentName;

            cbBaudRate.Text = "115200";
        }

        // 关灯
        private void on_Click(object sender, EventArgs e)
        {
            byte[] dataSend = TextToBytes("@LED_OFF\r\n", sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送
            
        }

        // 开灯
        private void off_Click(object sender, EventArgs e)
        {
            byte[] dataSend = TextToBytes("@LED_ON\r\n", sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送
           
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
            cbBaudRate.SelectedIndex = 1;       //控件状态初始化
            cbDataBits.SelectedIndex = 3;
            cbStopBits.SelectedIndex = 0;
            cbParity.SelectedIndex = 0;

        }

        #region 机器人单轴运动控制指令事件

        // 第1关节单点运动 -
        private void button1_Click(object sender, EventArgs e)
        {
            num1--;
            if (num1 < 0 || num1 > 180)
            {
                MessageBox.Show("1关节超限");
            }
            else
            {
                trackBar1.Value = num1;
                label1.Text = num1 + "°";
                textBox3.Text = num1.ToString();
                byte[] dataSend = TextToBytes("@J1-\r\n", sendCoding);      //文本转字节流
                int count = dataSend.Length;
                serialPort.Write(dataSend, 0, count);       //串口发送
            }

            //string msg4 = "1JA";                                     //获取用户输入的信息
            //byte[] bufferJ1 = new byte[1024];
            //bufferJ1 = Encoding.UTF8.GetBytes(msg4);
            //clientSocket.Send(bufferJ1);
            //Thread.Sleep(20);
            //string msg1 = "Stop";
            //bufferJ1 = Encoding.UTF8.GetBytes(msg1);
            //clientSocket.Send(bufferJ1);
        }

        // 第1关节单点运动 +
        private void jointdel_Click(object sender, EventArgs e)
        {
            // 滑动条试试显示数据
            num1++;
            if (num1 < 0 || num1 > 180)
            {
                MessageBox.Show("1关节超限");
            }
            else
            {
                trackBar1.Value = num1;
                label1.Text = num1 + "°";
                textBox3.Text = num1.ToString();
                trackBar1.Value = num1;
                label1.Text = num1 + "°";


                byte[] dataSend = TextToBytes("@J1+\r\n", sendCoding);      //文本转字节流
                int count = dataSend.Length;
                serialPort.Write(dataSend, 0, count);       //串口发送
            }

            // 网络事件
            //string msg3 = "1JD";                                     //获取用户输入的信息
            //byte[] bufferJ1 = new byte[1024];
            //bufferJ1 = Encoding.UTF8.GetBytes(msg3);
            //clientSocket.Send(bufferJ1);
            //Thread.Sleep(20);
            //string msg1 = "Stop";
            //bufferJ1 = Encoding.UTF8.GetBytes(msg1);
            //clientSocket.Send(bufferJ1);

        }

        // 第2关节单点运动 -
        private void button3_Click(object sender, EventArgs e)
        {
            // 滑动条试试显示数据
            if (num2 == 0)
            {
                num2 = 0;
                MessageBox.Show("2关节已到极限");
            }else if (num2 == 80){
                num2 = 80;
                MessageBox.Show("2关节已到极限");
            }
            else
            {

                num2--;
                trackBar2.Value = num2;
                label15.Text = num2 + "°";
                textBox4.Text = num2.ToString();
                trackBar2.Value = num2;
                label15.Text = num2 + "°";

                byte[] dataSend = TextToBytes("@J2-\r\n", sendCoding);      //文本转字节流
                int count = dataSend.Length;
                serialPort.Write(dataSend, 0, count);       //串口发送
            }
            // 网络通讯
            //string msg2 = "2JD";                                     //获取用户输入的信息
            //byte[] bufferJ2 = new byte[1024];
            //bufferJ2 = Encoding.UTF8.GetBytes(msg2);
            //clientSocket.Send(bufferJ2);
            //Thread.Sleep(20);
            //msg2 = "Stop";
            //bufferJ2 = Encoding.UTF8.GetBytes(msg2);
            //clientSocket.Send(bufferJ2);
        }

        // 第2关节单点运动 +
        private void button4_Click(object sender, EventArgs e)
        {
            // 滑动条试试显示数据
         
            if (num2 < 0 || num2 > 180)
            {
                MessageBox.Show("2关节超限");
            }
            else
            {
                num2++;
                trackBar2.Value = num2;
                label15.Text = num2 + "°";
                textBox4.Text = num2.ToString();
                trackBar2.Value = num2;
                label15.Text = num2 + "°";

                byte[] dataSend = TextToBytes("@J2+\r\n", sendCoding);      //文本转字节流
                int count = dataSend.Length;
                serialPort.Write(dataSend, 0, count);       //串口发送
            }
            // 网络通讯
            //string msg = "2JA";                                     //获取用户输入的信息
            //byte[] bufferJ2 = new byte[1024];
            //bufferJ2 = Encoding.UTF8.GetBytes(msg);
            //clientSocket.Send(bufferJ2);
            //Thread.Sleep(20);
            //string msg1 = "Stop";
            //bufferJ2 = Encoding.UTF8.GetBytes(msg1);
            //clientSocket.Send(bufferJ2);
        }

        // 第3关节单点运动 -
        private void button12_Click(object sender, EventArgs e)
        {
            // 滑动条试试显示数据
            num3--;
            if (num3 <= 0 || num3 >= 180)
            {
                MessageBox.Show("3关节超限");
            }
            else
            {
                trackBar3.Value = num3;
                label16.Text = num3 + "°";
                textBox6.Text = num3.ToString();
                trackBar3.Value = num3;
                label16.Text = num3 + "°";
                byte[] dataSend = TextToBytes("@J3-\r\n", sendCoding);      //文本转字节流
                int count = dataSend.Length;
                serialPort.Write(dataSend, 0, count);       //串口发送
            }

            //string msg3 = "3JA";       //获取用户输入的信息
            //byte[] bufferJ1 = new byte[1024];
            //bufferJ1 = Encoding.UTF8.GetBytes(msg3);
            //clientSocket.Send(bufferJ1);
            //Thread.Sleep(20);
            //string msg1 = "Stop";
            //bufferJ1 = Encoding.UTF8.GetBytes(msg1);
            //clientSocket.Send(bufferJ1);
        }

        // 第3关节单点运动 +
        private void button13_Click(object sender, EventArgs e)
        {
            // 滑动条试试显示数据
            num3++;
            if (num3 <= 0 || num3 >= 180)
            {
                MessageBox.Show("3关节超限");
            }
            else
            {
                trackBar3.Value = num3;
                label16.Text = num3 + "°";
                textBox6.Text = num3.ToString();
                trackBar3.Value = num3;
                label16.Text = num3 + "°";

                byte[] dataSend = TextToBytes("@J3+\r\n", sendCoding);      //文本转字节流
                int count = dataSend.Length;
                serialPort.Write(dataSend, 0, count);       //串口发送
            }
            //string msg3 = "3JD";                                     //获取用户输入的信息
            //byte[] bufferJ1 = new byte[1024];
            //bufferJ1 = Encoding.UTF8.GetBytes(msg3);
            //clientSocket.Send(bufferJ1);
            //Thread.Sleep(20);
            //string msg1 = "Stop";
            //bufferJ1 = Encoding.UTF8.GetBytes(msg1);
            //clientSocket.Send(bufferJ1);


        }

        // 第4关节单点运动 -
        private void button14_Click(object sender, EventArgs e)
        {
            // 滑动条试试显示数据
            num4--;
            if (num4 <= 0 || num4 >= 180)
            {
                MessageBox.Show("4关节超限");
            }
            else
            {
                trackBar4.Value = num4;
                label17.Text = num4 + "°";
                textBox5.Text = num4.ToString();
                trackBar4.Value = num4;
                label17.Text = num4 + "°";

                byte[] dataSend = TextToBytes("@J4-\r\n", sendCoding);      //文本转字节流
                int count = dataSend.Length;
                serialPort.Write(dataSend, 0, count);       //串口发送
            }
            //string msg3 = "4JD";                                     //获取用户输入的信息
            //byte[] bufferJ1 = new byte[1024];
            //bufferJ1 = Encoding.UTF8.GetBytes(msg3);
            //clientSocket.Send(bufferJ1);
            //Thread.Sleep(20);
            //string msg1 = "Stop";
            //bufferJ1 = Encoding.UTF8.GetBytes(msg1);
            //clientSocket.Send(bufferJ1);

        }

        // 第4关节单点运动 +
        private void button15_Click(object sender, EventArgs e)
        {

            num4++;
            if (num4 <= 0 || num4 >= 180)
            {
                MessageBox.Show("4关节超限");
            }
            else
            {
                trackBar4.Value = num4;
                label17.Text = num4 + "°";
                textBox5.Text = num4.ToString();
                trackBar4.Value = num4;
                label17.Text = num4 + "°";

                byte[] dataSend = TextToBytes("@J4+\r\n1", sendCoding);      //文本转字节流
                int count = dataSend.Length;
                serialPort.Write(dataSend, 0, count);       //串口发送
            }
            //string msg3 = "4JA";                                     //获取用户输入的信息
            //byte[] bufferJ1 = new byte[1024];
            //bufferJ1 = Encoding.UTF8.GetBytes(msg3);
            //clientSocket.Send(bufferJ1);
            //Thread.Sleep(20);
            //string msg1 = "Stop";
            //bufferJ1 = Encoding.UTF8.GetBytes(msg1);
            //clientSocket.Send(bufferJ1);


        }

        // 第5关节单点运动 +
        private void button21_Click_1(object sender, EventArgs e)
        {
            num5++;
            if (num5 <= 0 || num5 >= 180)
            {
                MessageBox.Show("5关节超限");
            }
            else
            {
               
                
            
                byte[] dataSend = TextToBytes("@J5+\r\n", sendCoding);      //文本转字节流
                int count = dataSend.Length;
                serialPort.Write(dataSend, 0, count);       //串口发送
            }
            //string msg3 = "5JA";                                     //获取用户输入的信息
            //byte[] bufferJ1 = new byte[1024];
            //bufferJ1 = Encoding.UTF8.GetBytes(msg3);
            //clientSocket.Send(bufferJ1);
            //Thread.Sleep(20);
            //string msg1 = "Stop";
            //bufferJ1 = Encoding.UTF8.GetBytes(msg1);
            //clientSocket.Send(bufferJ1);

        }

        // 第5关节单点运动 -
        private void button22_Click_1(object sender, EventArgs e)
        {
            // 滑动条试试显示数据
            num5--;
            if (num5 <= 0 || num5 >= 180)
            {
                MessageBox.Show("5关节超限");
            }
            else
            {
                
                
                byte[] dataSend = TextToBytes("@J5-\r\n", sendCoding);      //文本转字节流
                int count = dataSend.Length;
                serialPort.Write(dataSend, 0, count);       //串口发送
            }
            //string msg3 = "5JD";                                     //获取用户输入的信息
            //byte[] bufferJ1 = new byte[1024];
            //bufferJ1 = Encoding.UTF8.GetBytes(msg3);
            //clientSocket.Send(bufferJ1);
            //Thread.Sleep(20);
            //string msg1 = "Stop";
            //bufferJ1 = Encoding.UTF8.GetBytes(msg1);
            //clientSocket.Send(bufferJ1);
        }

       

        #endregion
  
        private string BytesToText(byte[] bytes, string encoding)       //字节流转文本
        {
            List<byte> byteDecode = new List<byte>();   //需要转码的缓存区
            byteBuffer.AddRange(bytes);     //接收字节流到接收字节缓存区
            if (encoding == "GBK")
            {
                int count = byteBuffer.Count;
                for (int i = 0; i < count; i++)
                {
                    if (byteBuffer.Count == 0)
                    {
                        break;
                    }
                    if (byteBuffer[0] < 0x80)       //1字节字符
                    {
                        byteDecode.Add(byteBuffer[0]);
                        byteBuffer.RemoveAt(0);
                    }
                    else       //2字节字符
                    {
                        if (byteBuffer.Count >= 2)
                        {
                            byteDecode.Add(byteBuffer[0]);
                            byteBuffer.RemoveAt(0);
                            byteDecode.Add(byteBuffer[0]);
                            byteBuffer.RemoveAt(0);
                        }
                    }
                }
            }
            else if (encoding == "UTF-8")
            {
                int count = byteBuffer.Count;
                for (int i = 0; i < count; i++)
                {
                    if (byteBuffer.Count == 0)
                    {
                        break;
                    }
                    if ((byteBuffer[0] & 0x80) == 0x00)     //1字节字符
                    {
                        byteDecode.Add(byteBuffer[0]);
                        byteBuffer.RemoveAt(0);
                    }
                    else if ((byteBuffer[0] & 0xE0) == 0xC0)     //2字节字符
                    {
                        if (byteBuffer.Count >= 2)
                        {
                            byteDecode.Add(byteBuffer[0]);
                            byteBuffer.RemoveAt(0);
                            byteDecode.Add(byteBuffer[0]);
                            byteBuffer.RemoveAt(0);
                        }
                    }
                    else if ((byteBuffer[0] & 0xF0) == 0xE0)     //3字节字符
                    {
                        if (byteBuffer.Count >= 3)
                        {
                            byteDecode.Add(byteBuffer[0]);
                            byteBuffer.RemoveAt(0);
                            byteDecode.Add(byteBuffer[0]);
                            byteBuffer.RemoveAt(0);
                            byteDecode.Add(byteBuffer[0]);
                            byteBuffer.RemoveAt(0);
                        }
                    }
                    else if ((byteBuffer[0] & 0xF8) == 0xF0)     //4字节字符
                    {
                        if (byteBuffer.Count >= 4)
                        {
                            byteDecode.Add(byteBuffer[0]);
                            byteBuffer.RemoveAt(0);
                            byteDecode.Add(byteBuffer[0]);
                            byteBuffer.RemoveAt(0);
                            byteDecode.Add(byteBuffer[0]);
                            byteBuffer.RemoveAt(0);
                            byteDecode.Add(byteBuffer[0]);
                            byteBuffer.RemoveAt(0);
                        }
                    }
                    else        //其他
                    {
                        byteDecode.Add(byteBuffer[0]);
                        byteBuffer.RemoveAt(0);
                    }
                }
            }
            return Encoding.GetEncoding(encoding).GetString(byteDecode.ToArray());
        }

        private string BytesToHex(byte[] bytes)     //字节流转HEX
        {
            string hex = "";
            foreach (byte b in bytes)
            {
                hex += b.ToString("X2") + " ";
            }
            return hex;
        }

        // 串口接受函数
        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort.IsOpen)
            {
                int count = serialPort.BytesToRead;
                byte[] dataReceive = new byte[count];
                serialPort.Read(dataReceive, 0, count);     //串口接收

                this.BeginInvoke((EventHandler)(delegate
                {

                    string s = BytesToText(dataReceive, receiveCoding) + System.Environment.NewLine;

                    String timetamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    tbReceive.AppendText($"{timetamp}:{s}");       //字节流转文本
                    Console.WriteLine((BytesToText(dataReceive, receiveCoding)));
                    string aa = timetamp + s;
                    string[] arr = aa.Split('*');

                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (arr[i].StartsWith("J"))
                        {
                            string[] jvalue = arr[i].Split('=');
                            string jname = jvalue[0];
                            int value = int.Parse(jvalue[1]);

                            if (jname == "J1")
                            {
                                textBox3.Text = value.ToString();
                                trackBar1.Value = value;
                             
                            }
                            if (jname == "J2")
                            {
                                textBox4.Text = value.ToString();
                                trackBar2.Value = value;
                            }
                            if (jname == "J3")
                            {
                                textBox6.Text = value.ToString();
                                trackBar3.Value = value;
                            }
                            if (jname == "J4")
                            {
                                textBox5.Text = value.ToString();
                                trackBar4.Value = value;
                            }
                           
                        }

                    }


                }));
            }
        }

        // 网络通讯TCP接收信息函数
        public void Recive()
        {
            try
            {
                byte[] buffer1 = new byte[1024];
                while (true)
                {
                    int len = clientSocket.Receive(buffer1);
                    string str = Encoding.UTF8.GetString(buffer1, 0, len);

                    // 委托
                    // 为了解决线程内容 - 线程内部无法访问控件的问题
                    this.Invoke(new Action(() =>
                    {
                        string a = "Socket=";
                        string s = str.ToString();

                        //String timetamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        tbReceive.AppendText($"{a}:{s}");       //字节流转文本
                        tbReceive.AppendText("\r\n");       //字节流转文本

                        Console.WriteLine("真实接受的数据：" + s);

                       

                            string[] arr = s.Split('=');
                            int number = 0;

                            Console.WriteLine(arr[0]);
                            String ServoSpeed = comboBox2.Text;

                            if (arr[0] == "J1Y+" || arr[0] == "J1Y-")
                            {
                                number = 180 - ((int)double.Parse(arr[1]) + 80);
                                Console.WriteLine("1关节+方向旋转的角度" + number.ToString());

                                string st = "P2P_J" + ";" + "J1" + ";" + number.ToString() + ";" + ServoSpeed;
                                byte[] dataSend = TextToBytes(st, sendCoding);      //文本转字节流
                                int count = dataSend.Length;
                                serialPort.Write(dataSend, 0, count);       //串口发送

                                textBox3.Text = number.ToString();
                                trackBar1.Value = number;
                                label1.Text = number.ToString();
                                

                        }

                            else if (arr[0] == "J2X+" || arr[0] == "J2X-")
                            {
                                number = (int)double.Parse(arr[1]) + 90;
                                Console.WriteLine("2关节+方向旋转的角度" + number.ToString());

                                string st = "P2P_J" + ";" + "J2" + ";" + number.ToString() + ";" + ServoSpeed;
                                byte[] dataSend = TextToBytes(st, sendCoding);      //文本转字节流
                                int count = dataSend.Length;
                                serialPort.Write(dataSend, 0, count);       //串口发送

                                textBox4.Text = number.ToString();
                                trackBar2.Value = number;

                                label15.Text = number.ToString();
                         
                        }

                            else if (arr[0] == "J3X+" || arr[0] == "J3X-")
                            {
                                number = 180 - ((int)double.Parse(arr[1]) + 90);
                                Console.WriteLine("3关节+方向旋转的角度" + number.ToString());

                                string st = "P2P_J" + ";" + "J3" + ";" + number.ToString() + ";" + ServoSpeed;
                                byte[] dataSend = TextToBytes(st, sendCoding);      //文本转字节流
                                int count = dataSend.Length;
                                serialPort.Write(dataSend, 0, count);       //串口发送

                                textBox6.Text = number.ToString();
                                trackBar3.Value = number;
                            
                                label16.Text = number.ToString();
                            
                        }

                            else if (arr[0] == "J4Z+" || arr[0] == "J4X-")
                            {
                                number = (int)double.Parse(arr[1]) + 90;
                                Console.WriteLine("4关节+方向旋转的角度" + number.ToString());

                                string st = "P2P_J" + ";" + "J4" + ";" + number.ToString() + ";" + ServoSpeed;
                                byte[] dataSend = TextToBytes(st, sendCoding);      //文本转字节流
                                int count = dataSend.Length;
                                serialPort.Write(dataSend, 0, count);       //串口发送

                                textBox5.Text = number.ToString();
                                trackBar4.Value = number;
                                label17.Text = number.ToString();
                        
                        }

                          

                    }));

                };


            }
            catch (Exception e)
            {
                throw e;
            }

        }

        // 回零动作
        private void button23_Click(object sender, EventArgs e)
        {
            // 角度回零
            num1 = 90;
            num2 = 90;
            num3 = 90;
            num4 = 90;
            num5 = 90;
            num6 = 180;

            // 初始化空间条
            trackBar1.Value = 90;
            trackBar2.Value = 90;
            trackBar3.Value = 90;
            trackBar4.Value = 90;
        

            label1.Text = "90°";
            label15.Text = "90°";
            label16.Text = "90°";
            label17.Text = "90°";
           

            textBox3.Text = "90";
            textBox4.Text = "90";
            textBox6.Text = "90";
            textBox5.Text = "90";
         

            byte[] dataSend = TextToBytes("@rbthome\r\n", sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送

            //// 网络事件
            //string msg3 = "HOME";                                     //获取用户输入的信息
            //byte[] bufferJ1 = new byte[1024];
            //bufferJ1 = Encoding.UTF8.GetBytes(msg3);
            //clientSocket.Send(bufferJ1);
            //Thread.Sleep(20);
            //string msg1 = "Stop";
            //bufferJ1 = Encoding.UTF8.GetBytes(msg1);
            //clientSocket.Send(bufferJ1);



        }

        // 清楚log
        private void clear_Click(object sender, EventArgs e)
        {
            tbReceive.Text = "";
        }

        #region 拖动条
        // 第1个拖动条
        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            label1.Text = trackBar1.Value.ToString() + "°";
            textBox3.Text = trackBar1.Value.ToString();
            num1 = trackBar1.Value;
            string aa = "@MOVE:J1:" + num1 + "\r\n";
            Console.WriteLine(aa);
            byte[] dataSend = TextToBytes(aa, sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送

        }

        // 第2个拖动条
        private void trackBar2_MouseUp(object sender, MouseEventArgs e)
        {
            label15.Text = trackBar2.Value.ToString() + "°";
            textBox4.Text = trackBar2.Value.ToString();
            num2 = trackBar2.Value;

            string aa = "@MOVE:J2:" + num2 + "\r\n";
            Console.WriteLine(aa);
            byte[] dataSend = TextToBytes(aa, sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送


        }

        // 第3个拖动条
        private void trackBar3_MouseUp(object sender, MouseEventArgs e)
        {
            label16.Text = trackBar3.Value.ToString() + "°";
            textBox6.Text = trackBar3.Value.ToString();
            String ServoSpeed = comboBox2.Text;
            num3 = trackBar3.Value;
            string aa = "@MOVE:J3:" + num3 + "\r\n";
            Console.WriteLine(aa);
            byte[] dataSend = TextToBytes(aa, sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送



        }

        // 第4个拖动条
        private void trackBar4_MouseUp(object sender, MouseEventArgs e)
        {
            label17.Text = trackBar4.Value.ToString() + "°";
            textBox5.Text = trackBar4.Value.ToString();
            String ServoSpeed = comboBox2.Text;
            num4 = trackBar4.Value;

            string aa = "@MOVE:J4:" + num4 + "\r\n";
            Console.WriteLine(aa);
            byte[] dataSend = TextToBytes(aa, sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送



        }
       
        // 第7个拖动条 速度
        private void trackBar7_MouseUp(object sender, MouseEventArgs e)
        {


            label35.Text = trackBar7.Value.ToString() + "°";
            num7 = trackBar7.Value;
            string aa = "@" + "SPEED:ALL:" + num7 + "\r\n";
            Console.WriteLine(aa);
            byte[] dataSend = TextToBytes(aa, sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送
        }
        #endregion

        #region 滚动条

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label1.Text = trackBar1.Value.ToString() + "°";
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            label15.Text = trackBar2.Value.ToString() + "°";
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            label16.Text = trackBar3.Value.ToString() + "°";
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            label17.Text = trackBar4.Value.ToString() + "°";
        }

       

        private void trackBar7_Scroll(object sender, EventArgs e)
        {
            label35.Text = trackBar7.Value.ToString() + "°";
        }

       

        #endregion

        // 保存文件函数
        private void button5_Click(object sender, EventArgs e)
        {
        
            SaveListBoxToFile();
        }

        private void SaveListBoxToFile()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";  // 文件类型过滤器
                saveFileDialog.DefaultExt = "txt";  // 默认文件扩展名

                // 如果用户选择了一个文件路径
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 获取用户选择的文件路径
                        string filePath = saveFileDialog.FileName;

                        // 使用 StreamWriter 来写入文件
                        using (StreamWriter writer = new StreamWriter(filePath, true))  // true 表示追加到文件
                        {
                            writer.Write(tbReceive.Text);  // 将 TextBox 中的内容写入文件
                        }

                        MessageBox.Show("内容已成功保存！");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"保存失败: {ex.Message}");
                    }
                }
            }
        }


       

    

        // 指定角度舵机运动执行函数
        private void ServoAngleBtn_Click(object sender, EventArgs e)
        {
            String ServoJoint = comboBox1.Text;
            String ServoAngle =  textBox1.Text;
            String ServoSpeed = comboBox2.Text;
            String No = "@MOVE:" + ServoJoint + ":" + ServoAngle +":"+ ServoSpeed + "\r\n";
            byte[] dataSend = TextToBytes(No, sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送
            pictureBox1.BackColor = Color.Green;
        }

        // 在线运动测试程序
        private void button20_Click_1(object sender, EventArgs e)
        {
       

            byte[] dataSend = TextToBytes("@AUTO\r\n", sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送

        }

 
        // 读取当前舵机角度
        private void button10_Click_1(object sender, EventArgs e)
        {
            byte[] dataSend = TextToBytes("READ", sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送
           
        }

        private void pauseBtn_Click(object sender, EventArgs e)
        {
            byte[] dataSend = TextToBytes("PAUSE", sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送
        }

        private void resumeBtn_Click(object sender, EventArgs e)
        {
            byte[] dataSend = TextToBytes("RESUME", sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送
        }

        
       

 
        // 插入动作函数，目前测试虚拟仿真
        private void button9_Click_1(object sender, EventArgs e)
        {
            // 网络事件
            string msg3 = "HH";                                     //获取用户输入的信息
            byte[] bufferJ1 = new byte[1024];
            bufferJ1 = Encoding.UTF8.GetBytes(msg3);
            clientSocket.Send(bufferJ1);
            Thread.Sleep(20);
            string msg1 = "Stop";
            bufferJ1 = Encoding.UTF8.GetBytes(msg1);
            clientSocket.Send(bufferJ1);

        }

        private void RbtAddX_Click(object sender, EventArgs e)
        {
            byte[] dataSend = TextToBytes("P2P_XD", sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送

            // 网络事件
            string msg3 = "XD";                                     //获取用户输入的信息
            byte[] bufferJ1 = new byte[1024];
            bufferJ1 = Encoding.UTF8.GetBytes(msg3);
            clientSocket.Send(bufferJ1);
            Thread.Sleep(20);
            string msg1 = "Stop";
            bufferJ1 = Encoding.UTF8.GetBytes(msg1);
            clientSocket.Send(bufferJ1);
        }

        private void RbtDelX_Click(object sender, EventArgs e)
        {
            byte[] dataSend = TextToBytes("P2P_XA", sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送

            // 网络事件
            string msg3 = "XA";                                     //获取用户输入的信息
            byte[] bufferJ1 = new byte[1024];
            bufferJ1 = Encoding.UTF8.GetBytes(msg3);
            clientSocket.Send(bufferJ1);
            Thread.Sleep(20);
            string msg1 = "Stop";
            bufferJ1 = Encoding.UTF8.GetBytes(msg1);
            clientSocket.Send(bufferJ1);

        }

        private void RbtAddY_Click(object sender, EventArgs e)
        {
            byte[] dataSend = TextToBytes("P2P_YA", sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送

            // 网络事件
            string msg3 = "YA";                                     //获取用户输入的信息
            byte[] bufferJ1 = new byte[1024];
            bufferJ1 = Encoding.UTF8.GetBytes(msg3);
            clientSocket.Send(bufferJ1);
            Thread.Sleep(20);
            string msg1 = "Stop";
            bufferJ1 = Encoding.UTF8.GetBytes(msg1);
            clientSocket.Send(bufferJ1);
        }

        private void RbtDelY_Click(object sender, EventArgs e)
        {
            byte[] dataSend = TextToBytes("P2P_YD", sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送

            // 网络事件
            string msg3 = "YD";                                     //获取用户输入的信息
            byte[] bufferJ1 = new byte[1024];
            bufferJ1 = Encoding.UTF8.GetBytes(msg3);
            clientSocket.Send(bufferJ1);
            Thread.Sleep(20);
            string msg1 = "Stop";
            bufferJ1 = Encoding.UTF8.GetBytes(msg1);
            clientSocket.Send(bufferJ1);
        }

        private void RbtAddZ_Click(object sender, EventArgs e)
        {
            byte[] dataSend = TextToBytes("P2P_ZA", sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送
        }

        private void RbtDelZ_Click(object sender, EventArgs e)
        {
            byte[] dataSend = TextToBytes("P2P_ZD", sendCoding);      //文本转字节流
            int count = dataSend.Length;
            serialPort.Write(dataSend, 0, count);       //串口发送
        }

        private void RbtHome_Load(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox11_Enter(object sender, EventArgs e)
        {

        }

        private void RbtHome_KeyPress(object sender, KeyPressEventArgs e)
        {
            Console.WriteLine(e.KeyChar.ToString());

            if (e.KeyChar.ToString() == "1")
            {
                trackBar5.Value = 0;
            }
            else if (e.KeyChar.ToString() == "2")
            {
                trackBar5.Value = 1;
            }
            else if (e.KeyChar.ToString() == "3")
            {
                trackBar5.Value = 2;
            }


            int MoveStep = trackBar5.Value;
            int MoveDegree = 0;
            Console.WriteLine(MoveStep);
            if (MoveStep == 0)
            {
                MoveDegree = 1;
            }
            else if (MoveStep == 1)
            {
                MoveDegree = 5;
            }
            else if (MoveStep == 2)
            {
                MoveDegree = 10;
            }

            // 一轴正向移动
            if (e.KeyChar.ToString() == "q")
            {

                // num1++;
                num1 = num1 + MoveDegree;
                if (num1 < 0 || num1 > 180)
                {
                    num1 = num1 - MoveDegree;
                    MessageBox.Show("1关节超限");
                }
                else
                {
                    trackBar1.Value = num1;
                    label1.Text = num1 + "°";
                    textBox3.Text = num1.ToString();
                    
                    byte[] dataSend = TextToBytes("@MOVE:J1:" + num1 + "\r\n", sendCoding);      //文本转字节流
                    int count = dataSend.Length;
                    serialPort.Write(dataSend, 0, count);       //串口发送
                }

            }
            // 一轴负向移动
            else if (e.KeyChar.ToString() == "a")
            {
                //num1--;
                num1 = num1 - MoveDegree;
                if (num1 < 0 || num1 > 180)
                {
                    num1 = num1 + MoveDegree;
                    MessageBox.Show("1关节超限");
                }
                else
                {
                    trackBar1.Value = num1;
                    label1.Text = num1 + "°";
                    textBox3.Text = num1.ToString();

                    byte[] dataSend = TextToBytes("@MOVE:J1:" + num1 + "\r\n", sendCoding);      //文本转字节流
                    int count = dataSend.Length;
                    serialPort.Write(dataSend, 0, count);       //串口发送
                }

            }
            // 二轴正向移动
            else if (e.KeyChar.ToString() == "w")
            {
                //num2++;
                num2 = num2 + MoveDegree;
                if (num2 < 0 || num2 > 180)
                {
                    num2 = num2 - MoveDegree;
                    MessageBox.Show("2关节超限");
                }
                else
                {
                    trackBar2.Value = num2;
                    label15.Text = num2 + "°";
                    textBox4.Text = num2.ToString();

                    byte[] dataSend = TextToBytes("@MOVE:J2:" + num2 + "\r\n", sendCoding);      //文本转字节流
                    int count = dataSend.Length;
                    serialPort.Write(dataSend, 0, count);       //串口发送
                }

            }
            // 二轴负向移动
            else if (e.KeyChar.ToString() == "s")
            {
                //num2--;
                num2 = num2 - MoveDegree;
                if (num2 < 0 || num2 > 180)
                {
                    num2 = num2 + MoveDegree;
                    MessageBox.Show("2关节超限");
                }
                else
                {
                    trackBar2.Value = num2;
                    label15.Text = num2 + "°";
                    textBox4.Text = num2.ToString();

                    byte[] dataSend = TextToBytes("@MOVE:J2:" + num2 + "\r\n", sendCoding);      //文本转字节流
                    int count = dataSend.Length;
                    serialPort.Write(dataSend, 0, count);       //串口发送
                }

            }
            // 三轴正向移动
            else if (e.KeyChar.ToString() == "e")
            {
                //num3++;
                num3 = num3 + MoveDegree;
                if (num3 <= 0 || num3 >= 180)
                {
                    num3 = num3 - MoveDegree;
                    MessageBox.Show("3关节超限");
                }
                else
                {
                    trackBar3.Value = num3;
                    label16.Text = num3 + "°";
                    textBox6.Text = num3.ToString();

                    byte[] dataSend = TextToBytes("@MOVE:J3:" + num3 + "\r\n", sendCoding);      //文本转字节流
                    int count = dataSend.Length;
                    serialPort.Write(dataSend, 0, count);       //串口发送
                }

            }
            // 三轴负向移动
            else if (e.KeyChar.ToString() == "d")
            {
                //num3--;
                num3 = num3 - MoveDegree;
                if (num3 <= 0 || num3 >= 180)
                {
                    num3 = num3 + MoveDegree;
                    MessageBox.Show("3关节超限");
                }
                else
                {
                    trackBar3.Value = num3;
                    label16.Text = num3 + "°";
                    textBox6.Text = num3.ToString();
                    
                    byte[] dataSend = TextToBytes("@MOVE:J3:" + num3 + "\r\n", sendCoding);      //文本转字节流
                    int count = dataSend.Length;
                    serialPort.Write(dataSend, 0, count);       //串口发送
                }

            }
            // 四轴正向移动
            else if (e.KeyChar.ToString() == "r")
            {
                //num4++;
                num4 = num4 + MoveDegree;
                if (num4 <= 0 || num4 >= 180)
                {
                    num4 = num4 - MoveDegree;
                    MessageBox.Show("4关节超限");
                }
                else
                {
                    trackBar4.Value = num4;
                    label17.Text = num4 + "°";
                    textBox5.Text = num4.ToString();

                    byte[] dataSend = TextToBytes("@MOVE:J4:" + num4 + "\r\n", sendCoding);      //文本转字节流
                    int count = dataSend.Length;
                    serialPort.Write(dataSend, 0, count);       //串口发送
                }

            }
            // 四轴负向移动
            else if (e.KeyChar.ToString() == "f")
            {
                //num4--;
                num4 = num4 - MoveDegree;
                if (num4 <= 0 || num4 >= 180)
                {
                    num4 = num4 + MoveDegree;
                    MessageBox.Show("4关节超限");
                }
                else
                {
                    trackBar4.Value = num4;
                    label17.Text = num4 + "°";
                    textBox5.Text = num4.ToString();

                    byte[] dataSend = TextToBytes("@MOVE:J4:" + num4 + "\r\n", sendCoding);      //文本转字节流
                    int count = dataSend.Length;
                    serialPort.Write(dataSend, 0, count);       //串口发送
                }

            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
           
        }

        #region 工具函数
        private byte[] TextToBytes(string str, string encoding)     //文本转字节流
        {
            return Encoding.GetEncoding(encoding).GetBytes(str);
        }

        private byte[] HexToBytes(string str)       //HEX转字节流
        {
            string str1 = Regex.Replace(str, "[^A-F^a-f^0-9]", "");     //清除非法字符

            double i = str1.Length;     //将字符两两拆分
            int len = 2;
            string[] strList = new string[int.Parse(Math.Ceiling(i / len).ToString())];
            for (int j = 0; j < strList.Length; j++)
            {
                len = len <= str1.Length ? len : str1.Length;
                strList[j] = str1.Substring(0, len);
                str1 = str1.Substring(len, str1.Length - len);
            }

            int count = strList.Length;     //将拆分后的字符依次转换为字节
            byte[] bytes = new byte[count];
            for (int j = 0; j < count; j++)
            {
                bytes[j] = byte.Parse(strList[j], NumberStyles.HexNumber);
            }

            return bytes;
        }

     
        
        #endregion
    }


}
