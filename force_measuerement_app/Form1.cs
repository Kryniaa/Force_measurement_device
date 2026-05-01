using System.IO.Ports;
using System.Linq.Expressions;
using System.Windows.Forms.DataVisualization.Charting;

namespace force_measuerement_app
{
    public partial class Form1 : Form
    {
        private SerialPort myPort = new SerialPort();

        private double[] _tareOffsets = new double[10];
        private double[] _currentAbsoluteForces = new double[10];
        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (myPort.IsOpen)
            {
                myPort.DataReceived -= MyPort_DataReceived;
                myPort.Close();

                btnConnect.Text = "Connect";
                btnConnect.BackColor = Color.LightGreen;
                LogMessage("Disconnected from STM32.");
                return;
            }

            try
            {
                if (cmbPorts.SelectedItem == null)
                {
                    MessageBox.Show("Proszę najpierw wybrać port COM z listy!", "Błąd połączenia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogMessage("Connection aborted: No COM port selected.");
                    return;
                }
                myPort.PortName = cmbPorts.SelectedItem.ToString();
                myPort.BaudRate = 115200;

                myPort.ReadTimeout = 2000;
                myPort.WriteTimeout = 1000;

                myPort.Open();
                LogMessage($"Port {myPort.PortName} opened. Sending handshake...");

                myPort.WriteLine("I");

                string response = myPort.ReadLine().Trim();

                if (response == "ID:FORCE_MEASUREMENT_DEVICE_V1")
                {
                    LogMessage($"Handshake successful! STM32 is connected.");

                    btnConnect.Text = "Disconnect";
                    btnConnect.BackColor = Color.Salmon;

                    timer1.Enabled = false;

                    myPort.DataReceived += MyPort_DataReceived;
                }
                else
                {
                    LogMessage($"Handshake failed. Unexpected response: {response}");
                    myPort.Close();
                }

            }
            catch (TimeoutException)
            {
                LogMessage("Handshake timeout. STM32 did not respond.");
                myPort.Close();
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}");
                if (myPort.IsOpen) myPort.Close();
            }
        }

        private void MyPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string incomingData = myPort.ReadLine().Trim();

                this.Invoke(new Action(() =>
                {
                    ProcessData(incomingData);
                }));
            }
            catch (Exception ex) { }

        }

        private void progressBar2_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void cmbPorts_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chartWykres.Series.Clear();
            chartWykres.Titles.Clear();

            chartWykres.Titles.Add("Force measurement");
            chartWykres.Titles[0].Font = new Font("Arial", 14, FontStyle.Bold);

            chartWykres.Titles.Add("Average: 0.00 N");
            chartWykres.Titles[1].Font = new Font("Arial", 12, FontStyle.Bold);
            chartWykres.Titles[1].ForeColor = Color.Red;
            chartWykres.Titles[1].Alignment = ContentAlignment.TopRight;
            chartWykres.Titles[1].Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;

            var obszar = chartWykres.ChartAreas[0];
            obszar.AxisY.Minimum = 0;
            obszar.AxisY.Maximum = 250;
            obszar.AxisY.Title = "Force [N]";
            obszar.AxisY.TitleFont = new Font("Arial", 10, FontStyle.Bold);

            obszar.AxisX.MajorGrid.Enabled = false;

            var seria = chartWykres.Series.Add("Sila");
            seria.Points.Clear();
            seria.ChartType = SeriesChartType.Column;
            seria.Color = Color.CornflowerBlue;
            seria["PointWidth"] = "0.7";
            //labele na slupkach przyklejone do slupkow
            seria.Font = new Font("Arial", 10, FontStyle.Bold);

            for (int i = 0; i < 10; i++)
            {
                seria.Points.AddXY(i + 1, 0);
                seria.Points[i].AxisLabel = "CH " + (i + 1);
            }

            //labele nad wykresem nie przyklejone do slupkow
            //var seriaWartosci = chartWykres.Series.Add("WartosciSily");
            //seriaWartosci.ChartType = SeriesChartType.Point;
            //seriaWartosci.Color = Color.Transparent;
            //seriaWartosci.Font = new Font("Arial", 10, FontStyle.Bold);

            //for (int i = 0; i < 10; i++)
            //{
            //    seriaWartosci.Points.AddXY(i + 1, 250);
            //    seriaWartosci.Points[i].Label = "0.0";
            //}
        }

        private void lblValue1_Click(object sender, EventArgs e)
        {

        }

        private void chartWykres_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Random rnd = new Random();
            var values = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                values.Add(rnd.Next(0, 4096).ToString());
            }

            string mockString = "D:" + string.Join(",", values);

            ProcessData(mockString);
            chartWykres.Invalidate();
        }

        private void UpdateAverageLine(double averageValue)
        {
            chartWykres.Annotations.Clear();

            HorizontalLineAnnotation linia = new HorizontalLineAnnotation();
            linia.AxisX = chartWykres.ChartAreas[0].AxisX;
            linia.AxisY = chartWykres.ChartAreas[0].AxisY;
            linia.Y = averageValue;
            linia.LineColor = Color.Red;
            linia.LineWidth = 2;
            linia.ClipToChartArea = chartWykres.ChartAreas[0].Name;

            linia.IsInfinitive = true;

            chartWykres.Annotations.Add(linia);

            chartWykres.Titles[1].Text = $"Average: {averageValue:F2} N";
        }

        private void ProcessData(string rawData)
        {
            try
            {
                if (rawData.StartsWith("D:"))
                {
                    LogMessage($"Received: {rawData}");

                    string dataPart = rawData.Substring(2);
                    string[] values = dataPart.Split(',');

                    if (values.Length == 10)
                    {
                        double sum = 0;
                        double maxForceN = 250.0;

                        for (int i = 0; i < 10; i++)
                        {
                            if (int.TryParse(values[i], out int adcValue))
                            {
                                double absoluteForce = (adcValue / 4095.0) * maxForceN;
                                _currentAbsoluteForces[i] = absoluteForce;
                                double netForce = absoluteForce - _tareOffsets[i];
                                sum += netForce;

                                chartWykres.Series["Sila"].Points[i].YValues[0] = netForce;
                                //labele na slupkach przyklejone do slupkow
                                chartWykres.Series["Sila"].Points[i].Label = $"{netForce:F2}";
                                //labele nad wykresem nie przyklejone do slupkow
                                //chartWykres.Series["WartosciSily"].Points[i].Label = $"{netForce:F2} N";
                            }
                        }
                        double average = sum / 10.0;
                        UpdateAverageLine(average);
                    }
                }
            }
            catch (Exception ex) { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;

            if (timer1.Enabled)
            {
                btnDemo.Text = "Stop Demo";
                btnDemo.BackColor = Color.Orange;
            }
            else
            {
                btnDemo.Text = "Start Demo";
                btnDemo.BackColor = SystemColors.Control;
            }
        }

        private void btnTare_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                _tareOffsets[i] = _currentAbsoluteForces[i];
            }

            LogMessage("System Tared. Offsets updated.");

            chartWykres.Invalidate();
        }

        private void btnClearTare_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                _tareOffsets[i] = 0;
            }
        }

        private void btnToggleConsole_Click(object sender, EventArgs e)
        {
            rtbConsole.Visible = !rtbConsole.Visible;

            if (rtbConsole.Visible)
            {
                btnToggleConsole.Text = "Hide Console";
            }
            else
            {
                btnToggleConsole.Text = "Show Console";
            }
        }

        private void LogMessage(string message)
        {
            rtbConsole.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            rtbConsole.ScrollToCaret();
        }

        private void cmbPorts_DropDown(object sender, EventArgs e)
        {
            string? selectedPorts = cmbPorts.SelectedItem?.ToString();

            cmbPorts.Items.Clear();

            string[] availablePorts = SerialPort.GetPortNames();
            cmbPorts.Items.AddRange(availablePorts);

            if (selectedPorts != null && cmbPorts.Items.Contains(selectedPorts))
            {
                cmbPorts.SelectedItem = selectedPorts;
            }
            else if (cmbPorts.Items.Count > 0)
            {
                cmbPorts.SelectedIndex = 0;
            }
        }
    }
}
