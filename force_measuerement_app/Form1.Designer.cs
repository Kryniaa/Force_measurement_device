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
            cmbPorts = new ComboBox();
            btnConnect = new Button();
            btnTare = new Button();
            btnMeasure = new Button();
            txtConsole = new RichTextBox();
            btnToggleConsole = new Button();
            progressBar1 = new ProgressBar();
            lblValue1 = new Label();
            lblValue2 = new Label();
            lblValue3 = new Label();
            lblValue4 = new Label();
            lblValue5 = new Label();
            lblValue6 = new Label();
            lblValue7 = new Label();
            lblValue8 = new Label();
            lblValue9 = new Label();
            lblValue10 = new Label();
            groupBox1 = new GroupBox();
            flowLayoutPanel1 = new FlowLayoutPanel();
            groupBox2 = new GroupBox();
            flowLayoutPanel2 = new FlowLayoutPanel();
            groupBox3 = new GroupBox();
            tableLayoutPanel1 = new TableLayoutPanel();
            label1 = new Label();
            progressBar6 = new ProgressBar();
            label10 = new Label();
            label9 = new Label();
            progressBar7 = new ProgressBar();
            label8 = new Label();
            label11 = new Label();
            progressBar8 = new ProgressBar();
            label7 = new Label();
            progressBar2 = new ProgressBar();
            label6 = new Label();
            progressBar9 = new ProgressBar();
            label5 = new Label();
            label3 = new Label();
            progressBar10 = new ProgressBar();
            label4 = new Label();
            progressBar3 = new ProgressBar();
            progressBar5 = new ProgressBar();
            progressBar4 = new ProgressBar();
            groupBox1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            groupBox2.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            groupBox3.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
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
            // txtConsole
            // 
            txtConsole.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtConsole.BackColor = SystemColors.MenuText;
            txtConsole.ForeColor = Color.Cyan;
            txtConsole.Location = new Point(12, 569);
            txtConsole.Name = "txtConsole";
            txtConsole.Size = new Size(1076, 96);
            txtConsole.TabIndex = 4;
            txtConsole.Text = "";
            txtConsole.Visible = false;
            // 
            // btnToggleConsole
            // 
            btnToggleConsole.BackColor = SystemColors.ControlLight;
            btnToggleConsole.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 238);
            btnToggleConsole.Location = new Point(3, 93);
            btnToggleConsole.Name = "btnToggleConsole";
            btnToggleConsole.Size = new Size(173, 39);
            btnToggleConsole.TabIndex = 10;
            btnToggleConsole.Text = "Show/Hide Console";
            btnToggleConsole.UseVisualStyleBackColor = false;
            // 
            // progressBar1
            // 
            progressBar1.Dock = DockStyle.Fill;
            progressBar1.Location = new Point(3, 33);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(78, 420);
            progressBar1.TabIndex = 12;
            // 
            // lblValue1
            // 
            lblValue1.AutoSize = true;
            lblValue1.Dock = DockStyle.Fill;
            lblValue1.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            lblValue1.Location = new Point(3, 459);
            lblValue1.Margin = new Padding(3);
            lblValue1.Name = "lblValue1";
            lblValue1.Size = new Size(78, 24);
            lblValue1.TabIndex = 22;
            lblValue1.Text = "0";
            lblValue1.TextAlign = ContentAlignment.MiddleCenter;
            lblValue1.Click += lblValue1_Click;
            // 
            // lblValue2
            // 
            lblValue2.AutoSize = true;
            lblValue2.Dock = DockStyle.Fill;
            lblValue2.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            lblValue2.Location = new Point(87, 459);
            lblValue2.Margin = new Padding(3);
            lblValue2.Name = "lblValue2";
            lblValue2.Size = new Size(78, 24);
            lblValue2.TabIndex = 23;
            lblValue2.Text = "0";
            lblValue2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblValue3
            // 
            lblValue3.AutoSize = true;
            lblValue3.Dock = DockStyle.Fill;
            lblValue3.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            lblValue3.Location = new Point(171, 459);
            lblValue3.Margin = new Padding(3);
            lblValue3.Name = "lblValue3";
            lblValue3.Size = new Size(78, 24);
            lblValue3.TabIndex = 24;
            lblValue3.Text = "0";
            lblValue3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblValue4
            // 
            lblValue4.AutoSize = true;
            lblValue4.Dock = DockStyle.Fill;
            lblValue4.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            lblValue4.Location = new Point(255, 459);
            lblValue4.Margin = new Padding(3);
            lblValue4.Name = "lblValue4";
            lblValue4.Size = new Size(78, 24);
            lblValue4.TabIndex = 25;
            lblValue4.Text = "0";
            lblValue4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblValue5
            // 
            lblValue5.AutoSize = true;
            lblValue5.Dock = DockStyle.Fill;
            lblValue5.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            lblValue5.Location = new Point(339, 459);
            lblValue5.Margin = new Padding(3);
            lblValue5.Name = "lblValue5";
            lblValue5.Size = new Size(78, 24);
            lblValue5.TabIndex = 26;
            lblValue5.Text = "0";
            lblValue5.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblValue6
            // 
            lblValue6.AutoSize = true;
            lblValue6.Dock = DockStyle.Fill;
            lblValue6.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            lblValue6.Location = new Point(423, 459);
            lblValue6.Margin = new Padding(3);
            lblValue6.Name = "lblValue6";
            lblValue6.Size = new Size(78, 24);
            lblValue6.TabIndex = 27;
            lblValue6.Text = "0";
            lblValue6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblValue7
            // 
            lblValue7.AutoSize = true;
            lblValue7.Dock = DockStyle.Fill;
            lblValue7.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            lblValue7.Location = new Point(507, 459);
            lblValue7.Margin = new Padding(3);
            lblValue7.Name = "lblValue7";
            lblValue7.Size = new Size(78, 24);
            lblValue7.TabIndex = 28;
            lblValue7.Text = "0";
            lblValue7.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblValue8
            // 
            lblValue8.AutoSize = true;
            lblValue8.Dock = DockStyle.Fill;
            lblValue8.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            lblValue8.Location = new Point(591, 459);
            lblValue8.Margin = new Padding(3);
            lblValue8.Name = "lblValue8";
            lblValue8.Size = new Size(78, 24);
            lblValue8.TabIndex = 29;
            lblValue8.Text = "0";
            lblValue8.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblValue9
            // 
            lblValue9.AutoSize = true;
            lblValue9.Dock = DockStyle.Fill;
            lblValue9.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            lblValue9.Location = new Point(675, 459);
            lblValue9.Margin = new Padding(3);
            lblValue9.Name = "lblValue9";
            lblValue9.Size = new Size(78, 24);
            lblValue9.TabIndex = 30;
            lblValue9.Text = "0";
            lblValue9.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblValue10
            // 
            lblValue10.AutoSize = true;
            lblValue10.Dock = DockStyle.Fill;
            lblValue10.Font = new Font("Segoe UI", 10.8F, FontStyle.Bold);
            lblValue10.Location = new Point(759, 459);
            lblValue10.Margin = new Padding(3);
            lblValue10.Name = "lblValue10";
            lblValue10.Size = new Size(86, 24);
            lblValue10.TabIndex = 31;
            lblValue10.Text = "0";
            lblValue10.TextAlign = ContentAlignment.MiddleCenter;
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
            groupBox2.Size = new Size(188, 179);
            groupBox2.TabIndex = 33;
            groupBox2.TabStop = false;
            groupBox2.Text = "Control Panel";
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.Controls.Add(btnMeasure);
            flowLayoutPanel2.Controls.Add(btnTare);
            flowLayoutPanel2.Controls.Add(btnToggleConsole);
            flowLayoutPanel2.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel2.Location = new Point(6, 26);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new Size(176, 142);
            flowLayoutPanel2.TabIndex = 11;
            // 
            // groupBox3
            // 
            groupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox3.Controls.Add(tableLayoutPanel1);
            groupBox3.Location = new Point(234, 12);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(854, 512);
            groupBox3.TabIndex = 34;
            groupBox3.TabStop = false;
            groupBox3.Text = "Force Visualization";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 10;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(lblValue10, 9, 2);
            tableLayoutPanel1.Controls.Add(progressBar6, 9, 1);
            tableLayoutPanel1.Controls.Add(label10, 9, 0);
            tableLayoutPanel1.Controls.Add(progressBar1, 0, 1);
            tableLayoutPanel1.Controls.Add(label9, 8, 0);
            tableLayoutPanel1.Controls.Add(progressBar7, 8, 1);
            tableLayoutPanel1.Controls.Add(lblValue9, 8, 2);
            tableLayoutPanel1.Controls.Add(lblValue1, 0, 2);
            tableLayoutPanel1.Controls.Add(label8, 7, 0);
            tableLayoutPanel1.Controls.Add(label11, 1, 0);
            tableLayoutPanel1.Controls.Add(progressBar8, 7, 1);
            tableLayoutPanel1.Controls.Add(label7, 6, 0);
            tableLayoutPanel1.Controls.Add(lblValue8, 7, 2);
            tableLayoutPanel1.Controls.Add(progressBar2, 1, 1);
            tableLayoutPanel1.Controls.Add(label6, 5, 0);
            tableLayoutPanel1.Controls.Add(progressBar9, 6, 1);
            tableLayoutPanel1.Controls.Add(lblValue2, 1, 2);
            tableLayoutPanel1.Controls.Add(label5, 4, 0);
            tableLayoutPanel1.Controls.Add(lblValue7, 6, 2);
            tableLayoutPanel1.Controls.Add(label3, 2, 0);
            tableLayoutPanel1.Controls.Add(progressBar10, 5, 1);
            tableLayoutPanel1.Controls.Add(label4, 3, 0);
            tableLayoutPanel1.Controls.Add(progressBar3, 2, 1);
            tableLayoutPanel1.Controls.Add(lblValue3, 2, 2);
            tableLayoutPanel1.Controls.Add(lblValue6, 5, 2);
            tableLayoutPanel1.Controls.Add(progressBar5, 4, 1);
            tableLayoutPanel1.Controls.Add(progressBar4, 3, 1);
            tableLayoutPanel1.Controls.Add(lblValue4, 3, 2);
            tableLayoutPanel1.Controls.Add(lblValue5, 4, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(3, 23);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.Size = new Size(848, 486);
            tableLayoutPanel1.TabIndex = 51;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(3, 3);
            label1.Margin = new Padding(3);
            label1.Name = "label1";
            label1.Size = new Size(78, 24);
            label1.TabIndex = 41;
            label1.Text = "CH1";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressBar6
            // 
            progressBar6.Dock = DockStyle.Fill;
            progressBar6.Location = new Point(759, 33);
            progressBar6.Name = "progressBar6";
            progressBar6.Size = new Size(86, 420);
            progressBar6.TabIndex = 40;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Dock = DockStyle.Fill;
            label10.Location = new Point(759, 3);
            label10.Margin = new Padding(3);
            label10.Name = "label10";
            label10.Size = new Size(86, 24);
            label10.TabIndex = 50;
            label10.Text = "CH10";
            label10.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Dock = DockStyle.Fill;
            label9.Location = new Point(675, 3);
            label9.Margin = new Padding(3);
            label9.Name = "label9";
            label9.Size = new Size(78, 24);
            label9.TabIndex = 49;
            label9.Text = "CH9";
            label9.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressBar7
            // 
            progressBar7.Dock = DockStyle.Fill;
            progressBar7.Location = new Point(675, 33);
            progressBar7.Name = "progressBar7";
            progressBar7.Size = new Size(78, 420);
            progressBar7.TabIndex = 39;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Dock = DockStyle.Fill;
            label8.Location = new Point(591, 3);
            label8.Margin = new Padding(3);
            label8.Name = "label8";
            label8.Size = new Size(78, 24);
            label8.TabIndex = 48;
            label8.Text = "CH8";
            label8.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Dock = DockStyle.Fill;
            label11.Location = new Point(87, 3);
            label11.Margin = new Padding(3);
            label11.Name = "label11";
            label11.Size = new Size(78, 24);
            label11.TabIndex = 42;
            label11.Text = "CH2";
            label11.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressBar8
            // 
            progressBar8.Dock = DockStyle.Fill;
            progressBar8.Location = new Point(591, 33);
            progressBar8.Name = "progressBar8";
            progressBar8.Size = new Size(78, 420);
            progressBar8.TabIndex = 38;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Dock = DockStyle.Fill;
            label7.Location = new Point(507, 3);
            label7.Margin = new Padding(3);
            label7.Name = "label7";
            label7.Size = new Size(78, 24);
            label7.TabIndex = 47;
            label7.Text = "CH7";
            label7.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressBar2
            // 
            progressBar2.Dock = DockStyle.Fill;
            progressBar2.Location = new Point(87, 33);
            progressBar2.Name = "progressBar2";
            progressBar2.Size = new Size(78, 420);
            progressBar2.TabIndex = 32;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Dock = DockStyle.Fill;
            label6.Location = new Point(423, 3);
            label6.Margin = new Padding(3);
            label6.Name = "label6";
            label6.Size = new Size(78, 24);
            label6.TabIndex = 46;
            label6.Text = "CH6";
            label6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressBar9
            // 
            progressBar9.Dock = DockStyle.Fill;
            progressBar9.Location = new Point(507, 33);
            progressBar9.Name = "progressBar9";
            progressBar9.Size = new Size(78, 420);
            progressBar9.TabIndex = 37;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Dock = DockStyle.Fill;
            label5.Location = new Point(339, 3);
            label5.Margin = new Padding(3);
            label5.Name = "label5";
            label5.Size = new Size(78, 24);
            label5.TabIndex = 45;
            label5.Text = "CH5";
            label5.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = DockStyle.Fill;
            label3.Location = new Point(171, 3);
            label3.Margin = new Padding(3);
            label3.Name = "label3";
            label3.Size = new Size(78, 24);
            label3.TabIndex = 43;
            label3.Text = "CH3";
            label3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressBar10
            // 
            progressBar10.Dock = DockStyle.Fill;
            progressBar10.Location = new Point(423, 33);
            progressBar10.Name = "progressBar10";
            progressBar10.Size = new Size(78, 420);
            progressBar10.TabIndex = 36;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Dock = DockStyle.Fill;
            label4.Location = new Point(255, 3);
            label4.Margin = new Padding(3);
            label4.Name = "label4";
            label4.Size = new Size(78, 24);
            label4.TabIndex = 44;
            label4.Text = "CH4";
            label4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // progressBar3
            // 
            progressBar3.Dock = DockStyle.Fill;
            progressBar3.Location = new Point(171, 33);
            progressBar3.Name = "progressBar3";
            progressBar3.Size = new Size(78, 420);
            progressBar3.TabIndex = 33;
            // 
            // progressBar5
            // 
            progressBar5.Dock = DockStyle.Fill;
            progressBar5.Location = new Point(339, 33);
            progressBar5.Name = "progressBar5";
            progressBar5.Size = new Size(78, 420);
            progressBar5.TabIndex = 35;
            // 
            // progressBar4
            // 
            progressBar4.Dock = DockStyle.Fill;
            progressBar4.Location = new Point(255, 33);
            progressBar4.Name = "progressBar4";
            progressBar4.Size = new Size(78, 420);
            progressBar4.TabIndex = 34;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1102, 677);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(txtConsole);
            Name = "Form1";
            Text = "Form1";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            flowLayoutPanel2.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private ComboBox cmbPorts;
        private Button btnConnect;
        private Button btnTare;
        private Button btnMeasure;
        private RichTextBox txtConsole;
        private Button btnToggleConsole;
        private ProgressBar progressBar1;
        private Label lblValue1;
        private Label lblValue2;
        private Label lblValue3;
        private Label lblValue4;
        private Label lblValue5;
        private Label lblValue6;
        private Label lblValue7;
        private Label lblValue8;
        private Label lblValue9;
        private Label lblValue10;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private Label label10;
        private Label label9;
        private Label label8;
        private Label label7;
        private Label label6;
        private Label label5;
        private Label label4;
        private Label label3;
        private Label label11;
        private Label label1;
        private ProgressBar progressBar6;
        private ProgressBar progressBar7;
        private ProgressBar progressBar8;
        private ProgressBar progressBar9;
        private ProgressBar progressBar10;
        private ProgressBar progressBar5;
        private ProgressBar progressBar4;
        private ProgressBar progressBar3;
        private ProgressBar progressBar2;
        private TableLayoutPanel tableLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel2;
    }
}
