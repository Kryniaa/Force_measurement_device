using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;

namespace force_measuerement_app
{
    public partial class Form1 : Form
    {
        private SerialPort myPort = new SerialPort();
        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {

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

            var obszar = chartWykres.ChartAreas[0];
            obszar.AxisY.Minimum = 0;
            obszar.AxisY.Maximum = 250;
            obszar.AxisY.Title = "Force [N]";

            obszar.AxisX.MajorGrid.Enabled = false;

            var seria = chartWykres.Series.Add("Sila");
            seria.Points.Clear();
            seria.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
            seria.Color = Color.CornflowerBlue;
            seria["PointWidth"] = "0.7";

            for (int i = 0; i < 10; i++)
            {
                seria.Points.AddXY(i + 1, 0);
                seria.Points[i].AxisLabel = "CH " + (i + 1);
            }

            string[] dostepnePorty = SerialPort.GetPortNames();
            cmbPorts.Items.AddRange(dostepnePorty);

            if (dostepnePorty.Length > 0)
            {
                cmbPorts.SelectedIndex = 0;
            }
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
            var mainArea = chartWykres.ChartAreas[0];

            mainArea.AxisY.StripLines.Clear();

            StripLine averageLine = new StripLine();

            averageLine.IntervalOffset = averageValue;
            averageLine.BorderColor = Color.Red;
            averageLine.BorderWidth = 3;

            averageLine.Text = $"Average: {averageValue:F2} N";
            averageLine.TextAlignment = StringAlignment.Far;
            averageLine.ForeColor = Color.Red;
            averageLine.Font = new Font("Arial", 9, FontStyle.Bold);

            mainArea.AxisY.StripLines.Add(averageLine);
        }

        private void ProcessData(string rawData)
        {
            try
            {
                if (rawData.StartsWith("D:"))
                {
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
                                double forceN = (adcValue / 4095.0) * maxForceN;
                                sum += forceN;

                                chartWykres.Series["Sila"].Points[i].YValues[0] = forceN;
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
    }
}
