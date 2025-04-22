namespace NaradaBatReader
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.batteriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showGraphsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showCapacityToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showSOHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showBMSVersionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.readBMSVersionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.percentagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.currentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cellVoltagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.temperaturesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // serialPort1
            // 
            this.serialPort1.PortName = "COM7";
            this.serialPort1.ReadTimeout = 20;
            this.serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 700;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer2
            // 
            this.timer2.Interval = 300;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.batteriesToolStripMenuItem,
            this.percentagesToolStripMenuItem,
            this.currentsToolStripMenuItem,
            this.cellVoltagesToolStripMenuItem,
            this.temperaturesToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1264, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "Cell Voltages";
            // 
            // batteriesToolStripMenuItem
            // 
            this.batteriesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showGraphsToolStripMenuItem,
            this.showCapacityToolStripMenuItem,
            this.showSOHToolStripMenuItem,
            this.showBMSVersionToolStripMenuItem,
            this.readBMSVersionsToolStripMenuItem});
            this.batteriesToolStripMenuItem.Name = "batteriesToolStripMenuItem";
            this.batteriesToolStripMenuItem.Size = new System.Drawing.Size(64, 20);
            this.batteriesToolStripMenuItem.Text = "Batteries";
            this.batteriesToolStripMenuItem.Click += new System.EventHandler(this.batteriesToolStripMenuItem_Click);
            // 
            // showGraphsToolStripMenuItem
            // 
            this.showGraphsToolStripMenuItem.Name = "showGraphsToolStripMenuItem";
            this.showGraphsToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.showGraphsToolStripMenuItem.Text = "Show Graphs";
            this.showGraphsToolStripMenuItem.Click += new System.EventHandler(this.showGraphsToolStripMenuItem_Click);
            // 
            // showCapacityToolStripMenuItem
            // 
            this.showCapacityToolStripMenuItem.Name = "showCapacityToolStripMenuItem";
            this.showCapacityToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.showCapacityToolStripMenuItem.Text = "Show Capacity";
            this.showCapacityToolStripMenuItem.Click += new System.EventHandler(this.showCapacityToolStripMenuItem_Click);
            // 
            // showSOHToolStripMenuItem
            // 
            this.showSOHToolStripMenuItem.Name = "showSOHToolStripMenuItem";
            this.showSOHToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.showSOHToolStripMenuItem.Text = "Show SOH";
            this.showSOHToolStripMenuItem.Click += new System.EventHandler(this.showSOHToolStripMenuItem_Click);
            // 
            // showBMSVersionToolStripMenuItem
            // 
            this.showBMSVersionToolStripMenuItem.Name = "showBMSVersionToolStripMenuItem";
            this.showBMSVersionToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.showBMSVersionToolStripMenuItem.Text = "Show BMS Version";
            this.showBMSVersionToolStripMenuItem.Click += new System.EventHandler(this.showBMSVersionToolStripMenuItem_Click);
            // 
            // readBMSVersionsToolStripMenuItem
            // 
            this.readBMSVersionsToolStripMenuItem.Name = "readBMSVersionsToolStripMenuItem";
            this.readBMSVersionsToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.readBMSVersionsToolStripMenuItem.Text = "Read BMS Versions";
            this.readBMSVersionsToolStripMenuItem.Click += new System.EventHandler(this.readBMSVersionsToolStripMenuItem_Click);
            // 
            // percentagesToolStripMenuItem
            // 
            this.percentagesToolStripMenuItem.Name = "percentagesToolStripMenuItem";
            this.percentagesToolStripMenuItem.Size = new System.Drawing.Size(83, 20);
            this.percentagesToolStripMenuItem.Text = "Percentages";
            this.percentagesToolStripMenuItem.Click += new System.EventHandler(this.percentagesToolStripMenuItem_Click);
            // 
            // currentsToolStripMenuItem
            // 
            this.currentsToolStripMenuItem.Name = "currentsToolStripMenuItem";
            this.currentsToolStripMenuItem.Size = new System.Drawing.Size(64, 20);
            this.currentsToolStripMenuItem.Text = "Currents";
            this.currentsToolStripMenuItem.Click += new System.EventHandler(this.currentsToolStripMenuItem_Click);
            // 
            // cellVoltagesToolStripMenuItem
            // 
            this.cellVoltagesToolStripMenuItem.Name = "cellVoltagesToolStripMenuItem";
            this.cellVoltagesToolStripMenuItem.Size = new System.Drawing.Size(86, 20);
            this.cellVoltagesToolStripMenuItem.Text = "Cell Voltages";
            this.cellVoltagesToolStripMenuItem.Click += new System.EventHandler(this.cellVoltagesToolStripMenuItem_Click);
            // 
            // temperaturesToolStripMenuItem
            // 
            this.temperaturesToolStripMenuItem.Name = "temperaturesToolStripMenuItem";
            this.temperaturesToolStripMenuItem.Size = new System.Drawing.Size(91, 20);
            this.temperaturesToolStripMenuItem.Text = "Temperatures";
            this.temperaturesToolStripMenuItem.Click += new System.EventHandler(this.temperaturesToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 985);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "s";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem batteriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem percentagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem currentsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cellVoltagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showGraphsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showCapacityToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showSOHToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showBMSVersionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem readBMSVersionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem temperaturesToolStripMenuItem;
    }
}

