using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Xml.Schema;
using System.Configuration;

namespace NaradaBatReader
{
    public partial class Form1 : Form
    {
        public List<Label> labels_cell_voltage;
        public List<Label> labels_cell_temps;
        public Label current;
        public Label capacity;
        public Label cycles;

        public int data_counter = 0;
        public byte[] data_buffer;
        public byte current_panel = 255;
        public byte current_battery_id = 0;
        public byte[] bids;
        public byte bid_counter = 0;

        public Boolean show_graphs=false;
        public Boolean show_capacity = false;
        public Boolean show_soh = false;
        public Boolean show_bms_version = false;

        #region MyDefinedEnums
        public enum BAT_STATE { ACTIVE, IDLE }
        public enum ViewModes { BATTERIES, BAT_REL_PERCENTAGES, BAT_REL_CURRENTS, CELL_VOLTAGES, CELL_TEMPS }
        ViewModes current_view_mode;
        public enum RS485_READ_MODES { BMS_VERSIONS, GENERAL_DATA }
        RS485_READ_MODES read_modes = RS485_READ_MODES.GENERAL_DATA;
        int batteries_read = 0;
        public enum BATTERY_TYPES { NOT_SET, NPFC50, NPFC100_TP1, NPFC100_TP2, NPFC100_TP3 };
        #endregion

        #region MyClasses
        public class battery_data
        {
            public byte id;
            public BATTERY_TYPES battery_bms_type= BATTERY_TYPES.NOT_SET;
            public double[] cell_voltages;
            public double soc=0;
            public double current=0;
            public byte[] alarm_bits;
            public double[] temperatures;
            public int cycles=0;
            public int remaining_capacity = 0;
            public int total_capacity = 0;
            public double soh = 0;
            public string bms_version = "Not Set";

            public battery_data()
            {
                cell_voltages = new double[15];
                alarm_bits = new byte[6];
                temperatures = new double[6];

                for (int i = 0; i < cell_voltages.Length; i++) cell_voltages[i] = 0;
                for (int i = 0; i < alarm_bits.Length; i++) alarm_bits[i] = 0;
                for (int i = 0; i < temperatures.Length; i++) temperatures[i] = 0;
            }

            public double GetMaxCellVoltage()
            {
                double v = 0;
                for (int i=0;i<cell_voltages.Length;i++)
                    if (cell_voltages[i] > v)
                        v = cell_voltages[i];

                return v;
            }

            public double GetMinCellVoltage()
            {
                double v = cell_voltages[0];
                for (int i = 0; i < cell_voltages.Length; i++)
                    if ( cell_voltages[i] < v )
                        v = cell_voltages[i];

                return v;
            }
        }

        public class MyBasicPanel : Panel
        {
            public MyBasicPanel()
                {
                //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

                this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint, true);
            }
        }

        public class MyPanel : Panel
        {
            public byte col;
            public byte row;
            public byte id;
            public byte battery_id;
            public battery_data bat_data;
            public Timer t;
            public Boolean updated;

            public BAT_STATE battery_state;
            public MyGraph sg;
            public CountDownTimer cdt;

            public void Updated()
            {
                battery_state = BAT_STATE.ACTIVE;
                cdt.RestartCountdownTimer();

                updated = true;
                t.Enabled = true;
            }

            public void UpdateFailed()
            { 
                battery_state = BAT_STATE.IDLE;
                t.Enabled = false;
            }
            public void Settings(byte panelid, byte bat_id, BATTERY_TYPES bat_type, byte column, byte rw)
            {
                col = column;
                row = rw;
                id = panelid;
                battery_id = bat_id;
                bat_data.battery_bms_type = bat_type;
            }

            public MyPanel()
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

                t = new Timer();
                t.Interval = 400;
                t.Enabled = false;
                t.Tick += T_Tick;
                sg = new MyGraph();
                sg.Init(false, 1000, 1000, 3);
                sg.SetupTrace("Current+", 0, 40, 1, Color.Blue);
                sg.SetupTrace("Current-", 0, 40, 1, Color.Red);
                sg.SetupTrace("SOC", 30, 98, 1, Color.Green);
                this.Controls.Add(sg);
                this.Resize += MyPanel_Resize;

                cdt = new CountDownTimer();
                this.cdt.CountdownExpired += Cdt_CountdownExpired;
                cdt.CountdownInterval = 15000;
                cdt.Interval = 1000;
                cdt.StartCountdownTimer();
            }

            private void Cdt_CountdownExpired(object sender, EventArgs e)
            {
                UpdateFailed();
                this.Refresh();
            }

            private void MyPanel_Resize(object sender, EventArgs e)
            {
                sg.Left = 1;
                sg.Width = this.Width-2;
                sg.Top = this.Height /10*2;
                sg.Height = this.Height / 10*8;
            }

            private void T_Tick(object sender, EventArgs e)
            {
                updated = false;
                this.Refresh();
            }

        }
        #endregion

        #region MyVariables
        public List<MyPanel> panels;
        public MyBasicPanel percentage_panel;
        public MyBasicPanel currents_panel;
        public MyBasicPanel cell_voltages;
        public MyBasicPanel cell_temps;

        int left_margin = 10;
        int right_mnargin = 10;
        int spacingx = 10;

        int top_margin = 10;
        int bottom_margin = 10;
        int spacingy = 10;

        int border_x=20;
        int border_y = 50;

        int rows = 4;
        int columns = 3;
        #endregion

        public Form1()
        {
            InitializeComponent();

            data_buffer = new byte[256];
            bids= new byte[256];

            percentage_panel = new MyBasicPanel();
            this.Controls.Add(percentage_panel);
            percentage_panel.Paint += Percentage_panel_Paint;

            currents_panel = new MyBasicPanel();
            this.Controls.Add(currents_panel);
            currents_panel.Paint += Currents_panel_Paint;

            cell_voltages = new MyBasicPanel();
            this.Controls.Add(cell_voltages);
            cell_voltages.Paint += Cell_voltages_Paint; 
            
            cell_temps = new MyBasicPanel();
            this.Controls.Add(cell_temps);
            cell_temps.Paint += Cell_temps_Paint;

            panels = new List<MyPanel>();
            for (int i = 0; i < 11; i++)
            {
                MyPanel p = new MyPanel();
                p.bat_data = new battery_data();

                panels.Add(p);
            }

            // panel ID, BAT ID, BAT_TYPE, COL, ROW
            panels[0].Settings(0, 1, BATTERY_TYPES.NPFC50, 1, 1);
            panels[1].Settings(1, 2, BATTERY_TYPES.NPFC50, 1, 2);
            panels[2].Settings(2, 7, BATTERY_TYPES.NPFC100_TP1, 1, 3);
            panels[3].Settings(3, 3, BATTERY_TYPES.NPFC50, 1, 4);

            panels[4].Settings(4, 4, BATTERY_TYPES.NPFC50, 2, 1);
            panels[5].Settings(5, 5, BATTERY_TYPES.NPFC50, 2, 2);
            panels[6].Settings(6, 8, BATTERY_TYPES.NPFC100_TP1, 2, 3);
            panels[7].Settings(7, 6, BATTERY_TYPES.NPFC50, 2, 4);

            panels[8].Settings(8, 11, BATTERY_TYPES.NPFC100_TP2, 3, 1);
            panels[9].Settings(9, 10, BATTERY_TYPES.NPFC100_TP2, 3, 2);
            panels[10].Settings(10, 9, BATTERY_TYPES.NPFC100_TP3, 3, 3);

            foreach (MyPanel panel in panels)
            {
                panel.Paint += Panel_Paint;
                this.Controls.Add(panel);
            }

            SetViewMode(ViewModes.BATTERIES);
            SetGraphVisibility(false);
        }



        #region PaintedPanels
        private void Panel_Paint(object sender, PaintEventArgs e)
        {

            SolidBrush black_brush = new SolidBrush(Color.Black);
            SolidBrush green_brush = new SolidBrush(Color.Green);
            SolidBrush yellow_brush = new SolidBrush(Color.Yellow);
            SolidBrush orange_brush = new SolidBrush(Color.Orange);
            SolidBrush lg_brush = new SolidBrush(Color.LightGreen);
            SolidBrush gray_brush = new SolidBrush(Color.Gray);
            SolidBrush red_brush = new SolidBrush(Color.Red);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            MyPanel panel = (MyPanel)sender;

            // Battery ID String
            e.Graphics.DrawString(panel.battery_id.ToString(),
                                    panel.Font,
                                    black_brush,
                                    new Point(10, 10));

            int middle_left = panel.Width / 2;
            int middle_top = panel.Height / 3;



            // SOC String
            Font f1 = new Font("Arial", GetScaledFontSize(8));
            String s1 = "SOC:";
            SizeF fs1 = e.Graphics.MeasureString(s1, f1);

            Font f2 = new Font("Arial", GetScaledFontSize(14), FontStyle.Bold);
            String s2 = panel.bat_data.soc.ToString("0.0");
            SizeF fs2 = e.Graphics.MeasureString(s2, f2);

            e.Graphics.DrawString(s1,
                                  f1,
                                  black_brush,
                                  new Point(middle_left - (int)fs1.Width - (int)fs2.Width / 2, middle_top - (int)fs2.Height / 2 - (int)fs1.Height / 2));

            e.Graphics.DrawString(s2,
                                  f2,
                                  black_brush,
                                  new Point(middle_left - (int)fs2.Width / 2, middle_top - (int)fs2.Height));
            // Current String
            s1 = "Amps:";
            fs1 = e.Graphics.MeasureString(s1, f1);

            f2 = new Font("Arial", GetScaledFontSize(12), FontStyle.Bold);
            s2 = panel.bat_data.current.ToString("0.0");
            fs2 = e.Graphics.MeasureString(s2, f2);
            e.Graphics.DrawString(s1,
                                  f1,
                                  black_brush,
                                  new Point(middle_left - (int)fs1.Width - (int)fs2.Width / 2, middle_top));

            if (panel.bat_data.current >= 0)
                e.Graphics.DrawString(s2,
                                  f2,
                                  green_brush,
                                  new Point(middle_left - (int)fs2.Width / 2, middle_top));
            else
                e.Graphics.DrawString(s2,
                                  f2,
                                  red_brush,
                                  new Point(middle_left - (int)fs2.Width / 2, middle_top));

            // Capicity Strings
            if (show_capacity)
            {
                s2 = "RAh=" + panel.bat_data.remaining_capacity.ToString() +
                     " TAh=" + panel.bat_data.total_capacity;
                fs2 = e.Graphics.MeasureString(s2, f1);
                e.Graphics.DrawString(s2,
                                      f1,
                                      green_brush,
                                      new Point(middle_left - (int)fs2.Width / 2, middle_top + 20));
            }

            // SOH
            if (show_soh)
            {
                s2 = "SOH=" + panel.bat_data.soh.ToString();
                fs2 = e.Graphics.MeasureString(s2, f1);
                e.Graphics.DrawString(s2,
                                      f1,
                                      green_brush,
                                      new Point(middle_left - (int)fs2.Width / 2, middle_top + 30));
            }


            // BMS Strings
            if (show_bms_version)
            {
                s2 = panel.bat_data.bms_version;
                fs2 = e.Graphics.MeasureString(s2, f1);
                e.Graphics.DrawString(s2,
                                      f1,
                                      green_brush,
                                      new Point(middle_left - (int)fs2.Width / 2, middle_top + 40));
            }

            // Activity Green Light
            if (panel.battery_id==11)
            {
                int tt = 0;
            }
            if (panel.updated)
                e.Graphics.FillEllipse(lg_brush, new Rectangle(new Point(panel.Width - 15, 7), new Size(8, 8)));
            else
                e.Graphics.FillEllipse(gray_brush, new Rectangle(new Point(panel.Width - 15, 7), new Size(8, 8)));

            // Avtive Light Red/Green
            if (panel.battery_state==BAT_STATE.ACTIVE)
                e.Graphics.FillEllipse(green_brush, new Rectangle(new Point(panel.Width - 25, 7), new Size(8, 8)));
            else
                e.Graphics.FillEllipse(red_brush, new Rectangle(new Point(panel.Width - 25, 7), new Size(8, 8)));

            // Panel Border
            e.Graphics.DrawRectangle(SystemPens.WindowText, new Rectangle(0, 0, panel.Width - 1, panel.Height - 1));

            // Panel Border
            e.Graphics.DrawRectangle(SystemPens.WindowText, new Rectangle(0, 0, panel.Width - 1, panel.Height - 1));

            f1.Dispose();
            f2.Dispose();
            black_brush.Dispose();
            green_brush.Dispose();
            yellow_brush.Dispose();
            orange_brush.Dispose();
            lg_brush.Dispose();
            gray_brush.Dispose();
            red_brush.Dispose();

        }
        private void Cell_voltages_Paint(object sender, PaintEventArgs e)
        {
            int left = 30;
            int top = 30;
            int column = 0;
            int column_spacing=(this.Width - left - left) / panels.Count;
            int row_spacing = (this.Height - top - top) / 100 * 80 / (15+8);
            SolidBrush sb = null;
            string s = "";

            SolidBrush black_brush = new SolidBrush(Color.Black);
            SolidBrush green_brush = new SolidBrush(Color.Green);
            SolidBrush yellow_brush = new SolidBrush(Color.Gold);
            SolidBrush orange_brush = new SolidBrush(Color.Orange);
            SolidBrush lg_brush = new SolidBrush(Color.LightGreen);
            SolidBrush gray_brush = new SolidBrush(Color.Gray);
            SolidBrush red_brush = new SolidBrush(Color.Red);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Font f = new Font("Arial", GetScaledFontSize(12), FontStyle.Bold);

            foreach (MyPanel panel in panels)
            {
                // Draw Battery ID
                e.Graphics.DrawString("ID: " + panel.battery_id.ToString(),
                                            f,
                                            SystemBrushes.GrayText,
                                            new Point(left + column * column_spacing, top ));

                double avg_cell_voltage = 0;
                double battery_voltage = 0;

                for (int i = 0; i < panel.bat_data.cell_voltages.Length; i++) battery_voltage += panel.bat_data.cell_voltages[i];
                avg_cell_voltage = battery_voltage / panel.bat_data.cell_voltages.Length;

                // Draw Cell voltages
                for (int i = 0; i < panel.bat_data.cell_voltages.Length; i++)
                {
                    double vdiff = Math.Abs(panel.bat_data.cell_voltages[i] - avg_cell_voltage);

                    if (vdiff < 3)
                        sb = green_brush;
                    else if (vdiff < 6)
                        sb = yellow_brush;
                    else if (vdiff < 10)
                        sb = orange_brush;
                    else
                        sb = red_brush;

                    s = panel.bat_data.cell_voltages[i].ToString("0000");
                    SolidBrush sb2 = new SolidBrush(Color.Green);
                    e.Graphics.DrawString(s,
                                            f,
                                            sb,
                                            new Point(left + column * column_spacing, top + (i + 1) * row_spacing));

                }

                // Draw Max Battery Voltage based on sum of cells
                e.Graphics.DrawString("VMax:",
                                            f,
                                            SystemBrushes.GrayText,
                                            new Point(left + column * column_spacing, top + 17 * row_spacing));

                e.Graphics.DrawString((panel.bat_data.GetMaxCellVoltage()).ToString("0000"),
                                            f,
                                            SystemBrushes.GrayText,
                                            new Point(left + column * column_spacing, top + 18 * row_spacing));

                // Draw Battery Voltage based on sum of cells
                e.Graphics.DrawString("VMin: ",
                                            f,
                                            SystemBrushes.GrayText,
                                            new Point(left + column * column_spacing, top + 20 * row_spacing));

                e.Graphics.DrawString((panel.bat_data.GetMinCellVoltage()).ToString("0000"),
                                            f,
                                            SystemBrushes.GrayText,
                                            new Point(left + column * column_spacing, top + 21 * row_spacing));


                // Draw Battery Voltage based on sum of cells
                e.Graphics.DrawString("V: " + (battery_voltage/1000).ToString("00.0"),
                                            f,
                                            SystemBrushes.GrayText,
                                            new Point(left + column * column_spacing, top + 23* row_spacing));
                battery_voltage = 0;
                column++;
            }
            sb.Dispose();
            black_brush.Dispose();
            green_brush.Dispose();
            yellow_brush.Dispose();
            orange_brush.Dispose();
            lg_brush.Dispose();
            gray_brush.Dispose();
            red_brush.Dispose();
            f.Dispose();
        }
        private void Currents_panel_Paint(object sender, PaintEventArgs e)
        {
            double avg_current = 0;
            double left_margin = this.Width / 20;
            double available_width = this.Width / 10 * 8;
            double ystart = this.Height / 20 * 10;
            double max_col_heigth = this.Height / 20 * 7;
            double col_width = available_width / panels.Count;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Font f = new Font("Arial", GetScaledFontSize(12), FontStyle.Bold);
            SizeF szf = e.Graphics.MeasureString("H", f);

            foreach (MyPanel panel in panels) { avg_current = avg_current + panel.bat_data.current; }
            //avg_current /= panels.Count;

            // Draw Avg Current String
            e.Graphics.DrawString("Total Current : " + avg_current.ToString("0.0") + " Amps",
                                    f,
                                    SystemBrushes.GrayText,
                                    new Point((int)(left_margin), (int)(ystart - szf.Height*2 - max_col_heigth)));

            // Draw Percentage bars and add percentage values
            double[] relative_percentages = new double[panels.Count];
            for (int i = 0; i < panels.Count; i++)
            {
                relative_percentages[i] = (panels[i].bat_data.current) / 10; // /10 = max 10%
                if (relative_percentages[i] >= 0)
                {
                    int y = (int)(Math.Abs(max_col_heigth * relative_percentages[i]));
                    if (y > max_col_heigth) y = (int)(max_col_heigth);

                    e.Graphics.FillRectangle(SystemBrushes.MenuText,
                                                new Rectangle((int)(left_margin + col_width * i),
                                                                (int)(ystart - y),
                                                                (int)(col_width),
                                                                y)
                                                );

                    e.Graphics.DrawString((relative_percentages[i] * 10).ToString("0.0"),
                        f,
                        SystemBrushes.GrayText,
                        new Point((int)(left_margin + col_width * i), (int)(ystart - y - szf.Height)));

                }
                else
                {
                    int y = (int)(Math.Abs(max_col_heigth * relative_percentages[i]));
                    if (y > max_col_heigth) y = (int)(max_col_heigth);
                    e.Graphics.FillRectangle(SystemBrushes.MenuText,
                                                new Rectangle((int)(left_margin + col_width * i),
                                                                (int)(ystart),
                                                                (int)(col_width),
                                                                y)
                                                );

                    e.Graphics.DrawString((relative_percentages[i] * 10).ToString("0.0"),
                        f,
                        SystemBrushes.GrayText,
                        new Point((int)(left_margin + col_width * i), (int)(ystart + y + szf.Height)));
                }
            }

            // Draw vertial axis
            e.Graphics.DrawLine(SystemPens.ControlDark,
                                new Point((int)(left_margin), (int)(ystart - max_col_heigth - 20)),
                                new Point((int)(left_margin), (int)(ystart + max_col_heigth + 20))
                                );

            // Draw horizontal axis's
            int left = (int)left_margin;
            int right = (int)(left_margin + available_width);
            int[] yy = new int[5];
            yy[0] = (int)(ystart - max_col_heigth); // 10%
            yy[1] = (int)(ystart - max_col_heigth / 2); // 5%
            yy[2] = (int)(ystart); // 0%
            yy[3] = (int)(ystart + max_col_heigth / 2);
            yy[4] = (int)(ystart + max_col_heigth);

            int yyy = 10;
            for (int i = 0; i < 5; i++)
            {
                e.Graphics.DrawString(yyy.ToString(), f, SystemBrushes.MenuText, (int)(left - 20 - szf.Width), (int)yy[i] - szf.Height/2);
                yyy = yyy - 5;
                e.Graphics.DrawLine(SystemPens.ControlDark, new Point(left - 10, yy[i]), new Point(right + 10, yy[i]));
            }
        }
        private void Percentage_panel_Paint(object sender, PaintEventArgs e)
        {
            double avg_soc = 0;
            double left_margin = this.Width / 20;
            double available_width = this.Width / 10 * 8;
            double ystart = this.Height / 20 * 9;
            double max_col_heigth = this.Height / 20 * 7;
            double col_width = available_width / panels.Count;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Font f = new Font("Arial", GetScaledFontSize(12), FontStyle.Bold);
            SizeF szf = e.Graphics.MeasureString("H", f);

            // Calculate avg SOC
            int active_batteries = 0;
            foreach (MyPanel panel in panels) 
                {
                if (panel.battery_state==BAT_STATE.ACTIVE)
                    {
                    avg_soc = avg_soc + panel.bat_data.soc;
                    active_batteries++;
                    }
                }

            // Draw SOC for all battteries
            if (active_batteries > 0)
            {
                avg_soc /= active_batteries;

                // Draw Avg SOC String
                e.Graphics.DrawString("Average SOC : " + avg_soc.ToString("0.0") + " %",
                                        f,
                                        SystemBrushes.GrayText,
                                        new Point((int)(left_margin), (int)(ystart - szf.Height*2 - max_col_heigth)));

                // Draw Percentage bars and add percentage values
                double[] relative_percentages = new double[panels.Count];
                for (int i = 0; i < panels.Count; i++)
                {
                    if (panels[i].battery_state==BAT_STATE.ACTIVE)
                    {
                        relative_percentages[i] = (panels[i].bat_data.soc - avg_soc) / 10; // /10 = max 10%
                        if (relative_percentages[i] >= 0)
                        {
                            int y = (int)(Math.Abs(max_col_heigth * relative_percentages[i]));
                            if (y > max_col_heigth) y = (int)(max_col_heigth);

                            e.Graphics.FillRectangle(SystemBrushes.MenuText,
                                                        new Rectangle((int)(left_margin + col_width * i),
                                                                        (int)(ystart - y),
                                                                        (int)(col_width),
                                                                        y)
                                                        );

                            e.Graphics.DrawString((relative_percentages[i] * 10).ToString("0.0"),
                                f,
                                SystemBrushes.GrayText,
                                new Point((int)(left_margin + col_width * i), (int)(ystart - y - szf.Height)));

                        }
                        else
                        {
                            int y = (int)(Math.Abs(max_col_heigth * relative_percentages[i]));
                            if (y > max_col_heigth) y = (int)(max_col_heigth);
                            e.Graphics.FillRectangle(SystemBrushes.MenuText,
                                                        new Rectangle((int)(left_margin + col_width * i),
                                                                        (int)(ystart),
                                                                        (int)(col_width),
                                                                        y)
                                                        );

                            e.Graphics.DrawString((relative_percentages[i] * 10).ToString("0.0"),
                                f,
                                SystemBrushes.GrayText,
                                new Point((int)(left_margin + col_width * i), (int)(ystart + y + szf.Height)));
                        }
                    }
                }
            }

            // Draw vertial axis
            e.Graphics.DrawLine(SystemPens.ControlDark,
                                new Point((int)(left_margin), (int)(ystart - max_col_heigth - 20)),
                                new Point((int)(left_margin), (int)(ystart + max_col_heigth + 20))
                                );

            // Draw horizontal axis's
            int left = (int)left_margin;
            int right = (int)(left_margin + available_width);
            int[] yy = new int[5];
            yy[0] = (int)(ystart - max_col_heigth); // 10%
            yy[1] = (int)(ystart - max_col_heigth/2); // 5%
            yy[2] = (int)(ystart); // 0%
            yy[3] = (int)(ystart + max_col_heigth/2); 
            yy[4] = (int)(ystart + max_col_heigth);

            int yyy = 10;
            for (int i=0;i<5;i++)
                {
                e.Graphics.DrawString(yyy.ToString(), f, SystemBrushes.MenuText, (int)(left - 20 - szf.Width), (int)yy[i] - szf.Height / 2);
                yyy = yyy - 5;
                e.Graphics.DrawLine(SystemPens.ControlDark, new Point(left-10, yy[i]), new Point(right+10, yy[i]));
                }
        }
        private void Cell_temps_Paint(object sender, PaintEventArgs e)
        {
            int left = 30;
            int top = 30;
            int column = 0;
            int column_spacing = (this.Width - left - left) / panels.Count;
            int row_spacing = (this.Height - top - top) / 100 * 80 / 18;
            String s = "";

            SolidBrush black_brush = new SolidBrush(Color.Black);

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Font f = new Font("Arial", GetScaledFontSize(12), FontStyle.Bold);

            foreach (MyPanel panel in panels)
            {
                // Draw Battery ID
                e.Graphics.DrawString("ID: " + panel.battery_id.ToString(),
                                            f,
                                            SystemBrushes.GrayText,
                                            new Point(left + column * column_spacing, top));

                // Draw Cell Temps
                for (int i = 0; i < panel.bat_data.temperatures.Length; i++)
                {
                    s = panel.bat_data.temperatures[i].ToString();
                    SolidBrush sb2 = new SolidBrush(Color.Green);
                    e.Graphics.DrawString(s,
                                            f,
                                            black_brush,
                                            new Point(left + column * column_spacing, top + (i + 1) * row_spacing));

                }

                column++;
            }
            black_brush.Dispose();
        }
        #endregion

        #region HelperFunctions
        public float GetScaledFontSize(int current_size)
        {
            float x = current_size;
            // based on a 800 x 600
            if (this.Width>=800)
                 x= (float)this.Width / (float)800 * current_size;
            return x;
        }
        public void SetViewMode( ViewModes vm)
        {
            current_view_mode = vm;
            switch (vm)
            {
                case ViewModes.BATTERIES:
                    percentage_panel.Visible = false;
                    currents_panel.Visible = false;
                    cell_voltages.Visible = false;
                    cell_temps.Visible = false;
                    SetVisibilityofPanels(true);
                    break;
                case ViewModes.BAT_REL_CURRENTS:
                    SetVisibilityofPanels(false);
                    percentage_panel.Visible = false;
                    cell_voltages.Visible = false;
                    cell_temps.Visible = false;
                    currents_panel.Visible = true;
                    break;
                case ViewModes.BAT_REL_PERCENTAGES:
                    SetVisibilityofPanels(false);
                    currents_panel.Visible = false;
                    cell_voltages.Visible = false;
                    cell_temps.Visible = false;
                    percentage_panel.Visible = true;
                    break;
                case ViewModes.CELL_VOLTAGES:
                    SetVisibilityofPanels(false);
                    currents_panel.Visible = false;
                    percentage_panel.Visible = false;
                    cell_temps.Visible = false;
                    cell_voltages.Visible = true;
                    break;
                case ViewModes.CELL_TEMPS:
                    SetVisibilityofPanels(false);
                    currents_panel.Visible = false;
                    percentage_panel.Visible = false;
                    cell_voltages.Visible = false;
                    cell_temps.Visible = true;
                    break;

            }
        }
        public void SetVisibilityofPanels(Boolean b) { foreach (MyPanel p in panels) { p.Visible = b; } }
        public void SetGraphVisibility(Boolean b) {  foreach (MyPanel p in panels) { p.sg.Visible = b; } }
        public void RefreshAllPanels() { foreach (MyPanel p in panels) { p.Refresh(); } }
        public static int HexToDec(String s)
        {
            int result = 0;
            int a;
            if (s.Substring(0, 2) == "0x")
            {
                String hex_String = s.Substring(2);
                for (int i = 0; i < hex_String.Length; i++)
                {
                    a = (int)Math.Pow((float)2, (float)((hex_String.Length - i - 1) * 4));

                    result = result + (LookupChar(hex_String.ToCharArray()[i]) * a);
                }
            }
            return result;
        }
        public byte GetNextBatteryID()
        {
            if (current_panel == 255)
            {
                current_panel = 0;
                return panels[current_panel].battery_id;
            }
            else
            {
                Boolean next = false;
                foreach (MyPanel panel in panels)
                {
                    if (!next)
                    {
                        if (panel.id == current_panel)
                            next = true;
                    }
                    else
                    {
                        current_panel = panel.id;
                        return panels[current_panel].battery_id;
                    }
                }
            }
            current_panel = 0;
            return panels[current_panel].battery_id; ;
        }
        byte nerada_485_checksum(byte[] buf, byte len)
        {
            byte b = 0;
            byte b2 = 0;
            int num = 0;
            for (b = 0; b < len; b++)
            {
                b2 ^= buf[b];
                num += buf[b];
            }
            return (byte)((uint)(b2 ^ num) & 0xFFu);
        }
        private static Byte LookupChar(char ch)
        {
            switch (ch)
            {
                case '0':
                    return 0;
                case '1':
                    return 1;
                case '2':
                    return 2;
                case '3':
                    return 3;
                case '4':
                    return 4;
                case '5':
                    return 5;
                case '6':
                    return 6;
                case '7':
                    return 7;
                case '8':
                    return 8;
                case '9':
                    return 9;
                case 'a':
                case 'A':
                    return 10;
                case 'b':
                case 'B':
                    return 11;
                case 'c':
                case 'C':
                    return 12;
                case 'd':
                case 'D':
                    return 13;
                case 'e':
                case 'E':
                    return 14;
                case 'f':
                case 'F':
                    return 15;
                default:
                    return 0;
            }
        }
        public int FindLeftPosofPanelinColumn(byte col)
        {
            foreach (MyPanel panel in panels)
            {
                if (panel.col == col) return panel.Left;
            }
            return -1;
        }

        #endregion

        #region SerialFunctions
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int no_of_bytes = serialPort1.BytesToRead;
            serialPort1.Read(data_buffer, data_counter, no_of_bytes);
            data_counter += no_of_bytes;
        }
        public void SendReadString(byte bid)
        {

            byte crc = 254;
            byte[] data_bytes = { 126, bid, 1, 0, crc, 13 };
            byte b = nerada_485_checksum(data_bytes, 4);
            data_bytes[4] = b;
            serialPort1.Write(data_bytes, 0, 6);
        }
        public void SendBMSReadString(byte bid)
        {

            byte crc = 254;
            byte[] data_bytes = { 126, bid, 51, 0, crc, 13 };
            byte b = nerada_485_checksum(data_bytes, 4);
            data_bytes[4] = b;
            serialPort1.Write(data_bytes, 0, 6);
        }
        public void DecodeBMSData()
        {
            MyPanel panel = panels[current_panel];

            if (panel != null)
            {
                panel.bat_data.bms_version = "";
                for (int i=4;i< data_buffer[3]-1;i++)
                    panel.bat_data.bms_version = panel.bat_data.bms_version + char.ToString((char)data_buffer[i]);
            }

            int xx = 0;
        }
        public void DecodeGeneralData()
        {
            MyPanel panel = panels[current_panel];

            if (panel != null)
            {

                // cell voltages
                // -----------------------------------------------------------------------------
                int v;
                double vv;
                int offset = 6;
                for (int i = 0; i < 15; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        v = data_buffer[offset + i * 2] * 256 + data_buffer[offset + i * 2 + 1];
                        panel.bat_data.cell_voltages[i] = v;
                    }
                }

                // SOC
                // -----------------------------------------------------------------------------
                offset = 42;
                vv = data_buffer[offset] * 256 + data_buffer[offset + 1];
                vv = vv / 100;
                panel.bat_data.soc = vv;

                // Temps
                // -----------------------------------------------------------------------------
                offset = 49;
                int batteries = data_buffer[offset];
                offset++;
                offset++;
                for (int i = 0; i < batteries; i++)
                {
                    v = data_buffer[offset + i * 2];
                    panel.bat_data.temperatures[i] = (v & 0xFF) - 50;
                }


                // Total Capacity
                // -----------------------------------------------------------------------------
                offset = 46;
                v = data_buffer[offset] * 256 + data_buffer[offset + 1];
                panel.bat_data.total_capacity = v/100;

                // Remaining Capacity
                // -----------------------------------------------------------------------------
                offset = 42;
                v = data_buffer[offset] * 256 + data_buffer[offset + 1];
                panel.bat_data.remaining_capacity = v/100* panel.bat_data.total_capacity/100;

                // SOH
                // -----------------------------------------------------------------------------
                if (panel.bat_data.battery_bms_type == BATTERY_TYPES.NPFC50 || 
                    panel.bat_data.battery_bms_type == BATTERY_TYPES.NPFC100_TP1 )
                {
                    offset = 84;
                    vv = data_buffer[offset] * 256 + data_buffer[offset + 1];
                    panel.bat_data.soh = vv / 100;
                }
                if (panel.bat_data.battery_bms_type == BATTERY_TYPES.NPFC100_TP2 ||
                    panel.bat_data.battery_bms_type == BATTERY_TYPES.NPFC100_TP3)
                {
                    offset = 82;
                    vv = data_buffer[offset] * 256 + data_buffer[offset + 1];
                    panel.bat_data.soh = vv / 100;
                }

                // Current
                // -----------------------------------------------------------------------------
                offset = 38;
                vv = data_buffer[offset] * 256 + data_buffer[offset + 1];
                panel.bat_data.current = (30000 - vv) / 100;

                // Cycles
                // -----------------------------------------------------------------------------
                // offset = 76;
                v = data_buffer[offset] * 256 + data_buffer[offset + 1];
                panel.bat_data.cycles = v;


                double cp = 0;
                double cm = 0;

                if (panel.bat_data.current>=0)
                {
                    cp = panel.bat_data.current;
                    cm = 0;
                }
                else
                {
                    cp = 0;
                    cm = Math.Abs(panel.bat_data.current);
                }

                double[] data = { cp,cm, panel.bat_data.soc };
                panel.sg.AddData(data);
                data = null;


                this.Refresh();
                /*
                switch (current_view_mode)
                {
                    case ViewModes.BATTERIES:
                        panel.Refresh();
                        break;
                    case ViewModes.BAT_REL_CURRENTS:
                        currents_panel.Refresh();
                        break;
                    case ViewModes.BAT_REL_PERCENTAGES:
                        percentage_panel.Refresh();
                        break;
                    case ViewModes.CELL_VOLTAGES:
                        cell_voltages.Refresh();
                        break;
                    case ViewModes.CELL_TEMPS:
                        cell_temps.Refresh();
                        break;
                }
                */
            }
        }
        #endregion

        #region TimerFunctions
        private void timer2_Tick(object sender, EventArgs e)
        {
            data_counter = 0;

            MyPanel panel = panels[current_panel];

            if (data_buffer[0] == 126 && data_buffer[1] == current_battery_id)
            {
                panel.Updated();
                if (read_modes==RS485_READ_MODES.BMS_VERSIONS)
                    DecodeBMSData();

                if (read_modes==RS485_READ_MODES.GENERAL_DATA)
                    DecodeGeneralData();

                for (int i=0; i< data_buffer.Length-1;i++)  data_buffer[i] = 0;
            }
            else
            {
                serialPort1.ReadExisting();
            }
            timer2.Enabled = false;
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (serialPort1 != null)
            {
                if (serialPort1.IsOpen)
                {
                    if (read_modes == RS485_READ_MODES.BMS_VERSIONS)
                    {
                        current_battery_id = GetNextBatteryID();
                        SendBMSReadString(current_battery_id);
                        timer2.Enabled = true;
                        batteries_read++;
                        if (batteries_read == panels.Count + 1)
                        {
                            read_modes = RS485_READ_MODES.GENERAL_DATA;
                            current_battery_id = 255;
                        }
                    }

                    if (read_modes == RS485_READ_MODES.GENERAL_DATA)
                    {
                        current_battery_id = GetNextBatteryID();
                        SendReadString(current_battery_id);
                        timer2.Enabled = true;
                    }
                }
                else
                {
                    try
                    {
                        serialPort1.BaudRate = 9600;
                        serialPort1.PortName = "COM7";
                        serialPort1.ReadBufferSize = 32;
                        serialPort1.ReadTimeout = -1;
                        serialPort1.Open();
                        data_counter = 0;
                    }
                    catch (Exception ex) { }
                }
            }
        }
        #endregion

        #region FormFunctions
        private void Form1_Resize(object sender, EventArgs e)
        {
            FormResizeBody();
        }

        public void FormResizeBody()
        {
            int available_height;
            int start_height;

            // Do this for panels
            available_height = this.Height - menuStrip1.Height * 3;
            start_height = menuStrip1.Height * 3;
            
            int col_width = (this.Width - left_margin - right_mnargin - ((columns - 1) * spacingx) - border_x) / columns;
            int row_height = (available_height - top_margin - bottom_margin - ((rows - 1) * spacingy) - border_y) / rows;

            foreach (MyPanel panel in panels)
            {
                panel.Left = left_margin + (panel.col - 1) * col_width + (panel.col - 1) * spacingx;
                panel.Top = start_height + top_margin + (panel.row - 1) * row_height + (panel.row - 1) * spacingy;
                panel.Width = col_width;
                panel.Height = row_height;
                panel.BackColor = SystemColors.ControlLight;
                panel.Refresh();
            }

            // Do this for the rest of the displays
            available_height = this.Height - menuStrip1.Height;
            start_height = menuStrip1.Height;

            percentage_panel.Left = left_margin;
            percentage_panel.Top = start_height;
            percentage_panel.Width = this.Width - left_margin - right_mnargin;
            percentage_panel.Height = available_height;
            percentage_panel.Refresh();

            currents_panel.Left = left_margin;
            currents_panel.Top = start_height;
            currents_panel.Width = this.Width - left_margin - right_mnargin;
            currents_panel.Height = available_height;
            currents_panel.Refresh();

            cell_voltages.Left = left_margin;
            cell_voltages.Top = start_height;
            cell_voltages.Width = this.Width - left_margin - right_mnargin;
            cell_voltages.Height = available_height;
            cell_voltages.Refresh();

            cell_temps.Left = left_margin;
            cell_temps.Top = start_height;
            cell_temps.Width = this.Width - left_margin - right_mnargin;
            cell_temps.Height = available_height;
            cell_temps.Refresh();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (current_view_mode == ViewModes.BATTERIES)
            {
                double[] columns = new double[3];
                int col = 0;
                SolidBrush green_brush = new SolidBrush(Color.Green);
                SolidBrush red_brush = new SolidBrush(Color.Red);
                Font f = new Font("Arial", 12, FontStyle.Bold);

                foreach (MyPanel p in panels)
                {
                    if (p.col == 1)
                        columns[0] += p.bat_data.current;
                    if (p.col == 2)
                        columns[1] += p.bat_data.current;
                    if (p.col == 3)
                        columns[2] += p.bat_data.current;
                }

                try
                {
                    int top = 50;
                    int left2 = 10;

                    for (byte i = 0; i < 3; i++)
                    {
                        int l = left2 + FindLeftPosofPanelinColumn((byte)(i + 1));
                        String s = "Total Amps: " + columns[i].ToString();
                        if (columns[i] >= 0)
                            e.Graphics.DrawString(s, f, green_brush, new Point(l, top));
                        else
                            e.Graphics.DrawString(s, f, red_brush, new Point(l, top));
                    }
                }
                catch (Exception ex) { }
            }

            foreach (MyPanel panel in panels) panel.Refresh();
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            FormResizeBody();
        }
        #endregion

        #region MenuClickFunctions
        private void batteriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetViewMode(ViewModes.BATTERIES);
        }

        private void percentagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetViewMode(ViewModes.BAT_REL_PERCENTAGES);
        }

        private void currentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetViewMode(ViewModes.BAT_REL_CURRENTS);
        }

        private void cellVoltagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetViewMode(ViewModes.CELL_VOLTAGES);
        }

        private void showGraphsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            show_graphs = !show_graphs;
            SetGraphVisibility(show_graphs);
            if (show_graphs)
                showGraphsToolStripMenuItem.Text = "Hide Graphs";
            else
                showGraphsToolStripMenuItem.Text = "Show Graphs";
            RefreshAllPanels();
        }

        private void showCapacityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            show_capacity = !show_capacity;
            if (show_capacity)
                showCapacityToolStripMenuItem.Text = "Hide Capaity";
            else
                showCapacityToolStripMenuItem.Text = "Show Graphs";
            RefreshAllPanels();

        }

        private void showSOHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            show_soh = !show_soh;
            if (show_soh)
                showSOHToolStripMenuItem.Text = "Hide SOH";
            else
                showSOHToolStripMenuItem.Text = "Show SOH";
            RefreshAllPanels();

        }

        private void showBMSVersionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            show_bms_version = !show_bms_version;
            if (show_bms_version)
                showBMSVersionToolStripMenuItem.Text = "Hide BMS Version";
            else
                showBMSVersionToolStripMenuItem.Text = "Show BMS Version";
            RefreshAllPanels();

        }

        private void readBMSVersionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            read_modes = RS485_READ_MODES.BMS_VERSIONS;
            current_battery_id = 255;
        }

        private void temperaturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetViewMode(ViewModes.CELL_TEMPS);
        }
        #endregion
    }
}
