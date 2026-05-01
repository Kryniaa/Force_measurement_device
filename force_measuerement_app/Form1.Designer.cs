namespace force_measuerement_app
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            cmbPorts = new ComboBox();
            btnConnect = new Button();
            btnTare = new Button();
            btnMeasure = new Button();
            btnToggleConsole = new Button();
            groupBox1 = new GroupBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            groupBox2 = new GroupBox();
            flowLayoutPanel2 = new FlowLayoutPanel();
            btnClearTare = new Button();
            groupBox3 = new GroupBox();
            chartWykres = new System.Windows.Forms.DataVisualization.Charting.Chart();
            timer1 = new System.Windows.Forms.Timer(components);
            btnDemo = new Button();
            rtbConsole = new RichTextBox();
            groupBox1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            groupBox2.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartWykres).BeginInit();
            SuspendLayout();
            // 
            // cmbPorts
            // 
            cmbPorts.FormattingEnabled = true;
            cmbPorts.Location = new Point(3, 3);
            cmbPorts.Name = "cmbPorts";
            cmbPorts.Size = new Size(170, 28);
            cmbPorts.TabIndex = 0;
            cmbPorts.Text = "Choose COM";
            cmbPorts.DropDown += cmbPorts_DropDown;
            cmbPorts.SelectedIndexChanged += cmbPorts_SelectedIndexChanged;
            // 
            // btnConnect
            // 
            btnConnect.BackColor = Color.FromArgb(192, 255, 192);
            btnConnect.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnConnect.Location = new Point(3, 37);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(170, 43);
            btnConnect.TabIndex = 1;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = false;
            btnConnect.Click += btnConnect_Click;
            // 
            // btnTare
            // 
            btnTare.BackColor = SystemColors.Info;
            btnTare.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnTare.Location = new Point(3, 48);
            btnTare.Name = "btnTare";
            btnTare.Size = new Size(173, 39);
            btnTare.TabIndex = 2;
            btnTare.Text = "Tare";
            btnTare.UseVisualStyleBackColor = false;
            btnTare.Click += btnTare_Click;
            // 
            // btnMeasure
            // 
            btnMeasure.BackColor = SystemColors.ActiveCaption;
            btnMeasure.BackgroundImageLayout = ImageLayout.None;
            btnMeasure.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnMeasure.Location = new Point(3, 3);
            btnMeasure.Name = "btnMeasure";
            btnMeasure.Size = new Size(173, 39);
            btnMeasure.TabIndex = 3;
            btnMeasure.Text = "Start Measure";
            btnMeasure.UseVisualStyleBackColor = false;
            // 
            // btnToggleConsole
            // 
            btnToggleConsole.BackColor = SystemColors.ControlLight;
            btnToggleConsole.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 238);
            btnToggleConsole.Location = new Point(3, 138);
            btnToggleConsole.Name = "btnToggleConsole";
            btnToggleConsole.Size = new Size(173, 39);
            btnToggleConsole.TabIndex = 10;
            btnToggleConsole.Text = "Show Console";
            btnToggleConsole.UseVisualStyleBackColor = false;
            btnToggleConsole.Click += btnToggleConsole_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(flowLayoutPanel1);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(188, 129);
            groupBox1.TabIndex = 32;
            groupBox1.TabStop = false;
            groupBox1.Text = "USB Connection";
            groupBox1.Enter += groupBox1_Enter;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(cmbPorts);
            flowLayoutPanel1.Controls.Add(btnConnect);
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(9, 26);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(173, 94);
            flowLayoutPanel1.TabIndex = 2;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(flowLayoutPanel2);
            groupBox2.Location = new Point(12, 224);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(188, 214);
            groupBox2.TabIndex = 33;
            groupBox2.TabStop = false;
            groupBox2.Text = "Control Panel";
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Controls.Add(btnMeasure);
            flowLayoutPanel2.Controls.Add(btnTare);
            flowLayoutPanel2.Controls.Add(btnClearTare);
            flowLayoutPanel2.Controls.Add(btnToggleConsole);
            flowLayoutPanel2.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel2.Location = new Point(6, 26);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(176, 182);
            flowLayoutPanel2.TabIndex = 11;
            // 
            // btnClearTare
            // 
            btnClearTare.BackColor = Color.FromArgb(255, 192, 128);
            btnClearTare.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 238);
            btnClearTare.Location = new Point(3, 93);
            btnClearTare.Name = "btnClearTare";
            btnClearTare.Size = new Size(173, 39);
            btnClearTare.TabIndex = 11;
            btnClearTare.Text = "Reset Tare";
            btnClearTare.UseVisualStyleBackColor = false;
            btnClearTare.Click += btnClearTare_Click;
            // 
            // groupBox3
            // 
            groupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox3.Controls.Add(chartWykres);
            groupBox3.Location = new Point(234, 12);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(854, 512);
            groupBox3.TabIndex = 34;
            groupBox3.TabStop = false;
            groupBox3.Text = "Force Visualization";
            // 
            // chartWykres
            // 
            chartArea1.Name = "ChartArea1";
            chartWykres.ChartAreas.Add(chartArea1);
            chartWykres.Dock = DockStyle.Fill;
            chartWykres.Location = new Point(3, 23);
            chartWykres.Name = "chartWykres";
            series1.ChartArea = "ChartArea1";
            series1.Name = "Series1";
            chartWykres.Series.Add(series1);
            chartWykres.Size = new Size(848, 486);
            chartWykres.TabIndex = 0;
            chartWykres.Text = "chart1";
            chartWykres.Click += chartWykres_Click;
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // btnDemo
            // 
            btnDemo.Location = new Point(71, 169);
            btnDemo.Name = "btnDemo";
            btnDemo.Size = new Size(94, 29);
            btnDemo.TabIndex = 35;
            btnDemo.Text = "Start Demo";
            btnDemo.UseVisualStyleBackColor = true;
            btnDemo.Click += button1_Click;
            // 
            // rtbConsole
            // 
            rtbConsole.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            rtbConsole.BackColor = Color.Black;
            rtbConsole.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 238);
            rtbConsole.ForeColor = Color.Lime;
            rtbConsole.Location = new Point(12, 542);
            rtbConsole.Name = "rtbConsole";
            rtbConsole.ReadOnly = true;
            rtbConsole.Size = new Size(1078, 123);
            rtbConsole.TabIndex = 36;
            rtbConsole.Text = "";
            rtbConsole.Visible = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1102, 677);
            Controls.Add(rtbConsole);
            Controls.Add(btnDemo);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Name = "Form1";
            Text = "Form1";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            flowLayoutPanel2.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)chartWykres).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private ComboBox cmbPorts;
        private Button btnConnect;
        private Button btnTare;
        private Button btnMeasure;
        private Button btnToggleConsole;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private FlowLayoutPanel flowLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartWykres;
        private System.Windows.Forms.Timer timer1;
        private Button btnDemo;
        private Button btnClearTare;
        private RichTextBox rtbConsole;
    }
}
