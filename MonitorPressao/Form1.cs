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
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;

namespace MonitorPressao
{
    public partial class Monitor : Form
    {
        bool isConnected = false;
        SerialPort myPort;
        String[] listarCOMs;
        string dados, Xg, Yg;
        double tempo = 0;
        int x = 0, y = 0, pressaoMax = 0;
        Rectangle r1 = new Rectangle();
        Point? prevPosition = null;
        ToolTip tooltip = new ToolTip();

        public Monitor()
        {
            InitializeComponent();
            defaultBotao();           
                           
        }

        private void defaultBotao()
        {
            btnConectar.Enabled = false;
            btnConectar.Visible = true;
            btnDesconectar.Enabled = false;
            btnDesconectar.Visible = false;
            btnSave.Enabled = false;
            btnStart.Enabled = false;
            btnReset.Enabled = false;
        }

        private void carregarPortas()
        {
            listarCOMs = SerialPort.GetPortNames();
        }

        private void statusBotao()
        {
            if (cboCOMs.Items == null && isConnected == false)
            {
                btnConectar.Enabled = false;
                btnConectar.Visible = true;
                btnDesconectar.Enabled = false;
                btnDesconectar.Visible = false;
                btnSave.Enabled = false;
                btnStart.Enabled = false;
                btnReset.Enabled = false;                
            }
            else
            {
                btnConectar.Enabled = true;
                btnConectar.Visible = true;
                btnDesconectar.Enabled = false;
                btnDesconectar.Visible = false;
                btnSave.Enabled = false;
                btnStart.Enabled = false;
                btnReset.Enabled = false;
            }
        }

        private void atualizarPorts()
        {
            try
            {
                myPort = new SerialPort("" + cboCOMs.SelectedItem, 115200, Parity.None, 8, StopBits.One);
                statusBotao();
            }
            catch(Exception)
            {
                MessageBox.Show("Erro na conexão ", myPort.PortName);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            tempo++;
            graficoPressao.Series[0].Points.AddXY((tempo/2 ), dados);
        }

        private void Minimizar_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void Sair_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnConectar_Click(object sender, EventArgs e)
        {
            try
            {                
                if(cboCOMs.Items == null)
                {
                    return;
                }
                else
                {
                    atualizarPorts();
                    cboCOMs.Enabled = false;
                    btnConectar.Visible = false;
                    btnDesconectar.Visible = true;
                    btnDesconectar.Enabled = true;
                    btnStart.Enabled = true;
                    Console.WriteLine("Conectado");
                }                
            }
            catch(Exception)
            {
                MessageBox.Show("Erro ao conectar a porta ", myPort.PortName);
            }

        }

        private void btnDesconectar_Click(object sender, EventArgs e)
        {
            try
            {
                isConnected = false;
                Thread.Sleep(10);
                myPort.Close();
                timer1.Stop();
                statusBotao();
                cboCOMs.Enabled = true;
                pressaoMax = 0;
                txtDados.Text = " ";
                graficoPressao.Series[0].Points.Clear();
                tempo = 0.0;
                Console.WriteLine("Desconectado");
                               
            }
            catch(Exception)
            {
                MessageBox.Show("Ocorreu um erro para desconectar a porta " + myPort.PortName);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            myPort.DataReceived += myPort_DataReceived;
            try
            {
                Thread.Sleep(10);
                if (myPort.IsOpen == true)
                {
                    btnSave.Enabled = true;
                    btnReset.Enabled = true;
                    Console.WriteLine("Start 1");
                }   
                else
                {
                    myPort.Open();
                    btnSave.Enabled = true;
                    btnReset.Enabled = true;
                    Console.WriteLine("Start 2");
                }
                if (myPort.IsOpen == false)
                {
                    return;
                }

            }
            catch (Exception)
            {
                MessageBox.Show("Erro para iniciar o recebimento dos dados");
            }
            
        }

        private void myPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(10);
                if(myPort.IsOpen == false)
                {                    
                    return;
                }
                else
                {
                    this.Invoke(new EventHandler(displayDados));
                    dados = myPort.ReadLine();
                    Console.WriteLine(dados);
                }
            }
            catch(Exception)
            {
                MessageBox.Show("Não foi possível realizar o recimento dos dados");
            }
        }

        private void displayDados(object sender, EventArgs e)
        {
            Thread.Sleep(10);
            
            if(Convert.ToDouble(dados) >= 5)
            {
                timer1.Start();
                dadosPressao_Max(Convert.ToDouble(dados));
                Console.WriteLine(" Dados: "+dados);
                Console.WriteLine(" Tempo: "+tempo);
            }
            if (Convert.ToDouble(dados) < 10 && tempo > 10)
            {
                timer1.Stop();                
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                Thread.Sleep(10);
                txtDados.Text = " ";             
                graficoPressao.Series[0].Points.Clear();
                tempo = 0.0;
                pressaoMax = 0;
                btnSave.Enabled = false;
                btnReset.Enabled = false;
            }
            catch(Exception)
            {
                MessageBox.Show("Ocorreu um erro no reset dos dados");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                Thread.Sleep(10);
                SaveFileDialog sf = new SaveFileDialog();
                sf.Filter = "JPG(*.JPG)|*.jpg";

                if (sf.ShowDialog() == DialogResult.OK)
                {
                    graficoPressao.SaveImage(sf.FileName, ChartImageFormat.Png);
                }
                
                MessageBox.Show("Arquivo Salvo");
            }
            catch(Exception)
            {
                MessageBox.Show("Não foi possivel salvar os dados");
            }
        }

        private void btnPortas_Click(object sender, EventArgs e)
        {
            try
            {
                Thread.Sleep(10);
                carregarPortas();
                    foreach (string myPort in listarCOMs)
                    {
                        cboCOMs.Items.Add(myPort);
                        Console.WriteLine(myPort);
                        if (myPort[0] != null)
                        {
                            cboCOMs.SelectedItem = myPort[0];
                            cboCOMs.SelectedIndex = 0;
                        }
                        isConnected = true;
                        statusBotao();
                    }                                
            }
            catch(Exception)
            {
                MessageBox.Show("Erro detectar as portas");
            }
        }

        private void grafico_PostPaint(object sender, ChartPaintEventArgs e)
        {
            Font drawFont = new Font("Verdana", 8);
            SolidBrush drawBrush = new SolidBrush(Color.Red);
            StringFormat drawFormat = new StringFormat();
            if(!(string.IsNullOrEmpty(Xg) && string.IsNullOrEmpty(Yg)))
            {
                e.ChartGraphics.Graphics.DrawString("ELong = " + Xg + "\n" + "Pressão = " + Yg, drawFont, drawBrush, x + 5, y - 2);
                e.ChartGraphics.Graphics.DrawRectangle(new Pen(Color.Red, 3), r1);
            }
        }

        private void grafico_MouseDown(object sender, MouseEventArgs e)
        {
            graficoPressao.Invalidate();
            r1.X = x;
            r1.Y = y;
            r1.Width = 3;
            r1.Height = 3;
        }

        private void grafico_MouseMove(object sender, MouseEventArgs e)
        {
            var pos1 = e.Location;
            if (prevPosition.HasValue && pos1 == prevPosition.Value)
                return;
            tooltip.RemoveAll();
            prevPosition = pos1;
            var results1 = graficoPressao.HitTest(pos1.X, pos1.Y, false, ChartElementType.DataPoint);
            foreach (var result1 in results1)
            {
                if (result1.ChartElementType == ChartElementType.DataPoint)
                {
                    var prop1 = result1.Object as DataPoint;
                    if (prop1 != null)
                    {
                        var pointXPixel = result1.ChartArea.AxisX.ValueToPixelPosition(prop1.XValue);
                        var pointYPixel = result1.ChartArea.AxisY.ValueToPixelPosition(prop1.YValues[0]);

                        // check if the cursor is really close to the point (2 pixels around)

                        tooltip.Show("Tempo = " + prop1.XValue + ", Pressão = " + prop1.YValues[0], this.graficoPressao, pos1.X, pos1.Y - 15);

                    }
                }
            }
        }

        private void dadosPressao_Max(double pressao)
        {
            if(pressao > pressaoMax)
            {
                pressaoMax = Convert.ToInt32(pressao);
                txtDados.Text = " " + Convert.ToString(pressaoMax) + " mmHg";
            }
        }
    }
}
