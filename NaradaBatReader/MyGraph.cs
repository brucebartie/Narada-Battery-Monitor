using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NaradaBatReader
{
    public partial class MyGraph : UserControl
    {
        public class trace
        {
            public List<double> values;
            public trace() { }
            public trace(int no_of_data_points)
            {
                values = new List<double>(no_of_data_points);
                for (int i = 0; i < no_of_data_points; i++) values.Add(0);
            }
        }
        public class tracegroup
        {
            public double min;
            public double max;
            public double scale_factor;
            public String trace_name;
            public trace trc;
            public Pen trace_pen;
            public Brush trace_brush;
            public tracegroup(int no_of_data_points)
            {
                scale_factor = 1;
                trc = new trace(no_of_data_points);
            }
        }

        int x_axis_data_points;
        int no_of_traces;
        int no_of_display_points;
        int border_left;
        int border_right;
        int border_top;
        int border_bottom;
        double x_width;
        double y_height;
        double y_trace_spacing;
        double y_trace_height;
        double x_width_step;
        int data_pos;
        //int tick_marks;
        Boolean zoom;
        Boolean split_trace;
        public Boolean PaintOnParent;
        public Panel ParentPanel;
        int zoom_trace;

        Pen linePen;

        public List<tracegroup> traces;

        public MyGraph()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            linePen = new Pen(new SolidBrush(Color.Blue));
        }

        public void Init(Boolean split_trc, int data_points, int display_points, int notraces)
        {
            split_trace = split_trc;
            data_pos = 0;
            x_axis_data_points = data_points;
            no_of_display_points = display_points;
            no_of_traces = notraces;
            y_trace_spacing = 0;
            border_left = 5;
            border_right = 5;
            border_top = 5;
            border_bottom = 10;
            traces = new List<tracegroup>(0);

            x_width = this.Width - border_left - border_right;
            y_height = this.Height - border_top - border_bottom;

            if (split_trace)
                y_trace_height = (int)((y_height - (y_trace_spacing * (double)(no_of_traces))) / (double)no_of_traces);
            else
                y_trace_height = (int)(y_height);

            x_width_step = x_width / (double)no_of_display_points;
        }

        public void SetupTrace(String name, double min, double max, double scale, Color trace_color)
        {
            traces.Add(new tracegroup(x_axis_data_points));
            int i = traces.Count;
            traces[i - 1].trace_name = name;
            traces[i - 1].min = min;
            traces[i - 1].max = max;
            traces[i - 1].scale_factor = scale;
            traces[i - 1].trace_brush = new SolidBrush(trace_color);
            traces[i - 1].trace_pen = new Pen(traces[i - 1].trace_brush);
        }

        public void AddData(params double[] values)
        {
            double a = 0;
            if (data_pos == (no_of_display_points - 1))
                StepData();
            else
                data_pos++;

            for (int i = 0; i < no_of_traces; i++)
            {
                a = traces[i].max - traces[i].min;
                try
                {
                    traces[i].trc.values[data_pos] = (values[i] - traces[i].min) / a;
                }
                catch
                {
                    traces[i].trc.values[data_pos] = traces[i].min;
                }
            }
            this.Refresh();
        }

        public void StepData()
        {
            for (int i = 0; i < no_of_traces; i++)
            {
                for (int j = 0; j < no_of_display_points - 1; j++)
                {
                    traces[i].trc.values[j] = traces[i].trc.values[j + 1];
                }
            }
        }

        private void MyGraph_Paint(object sender, PaintEventArgs e)
        {
            double x1, x2, y1, y2;
            Graphics g = e.Graphics;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            /*
            // Draw Trace Background
            if (split_trace)
            {
                for (int i = 0; i < no_of_traces; i++)
                {
                    for (int j = 1; j < no_of_display_points; j++)
                    {
                        e.Graphics.FillRectangle(SystemBrushes.ControlDark,
                            new Rectangle(border_left,
                                            (int)((double)border_top + ((double)(i + 1) * y_trace_spacing) + ((double)i * y_trace_height)),
                                            (int)x_width,
                                            (int)y_trace_height + 1));
                    }
                }
            }
            else
            {
                for (int j = 1; j < no_of_display_points; j++)
                {
                    e.Graphics.FillRectangle(SystemBrushes.ControlDark,
                        new Rectangle(border_left,
                                        (int)((double)border_top + y_trace_spacing),
                                        (int)x_width,
                                        (int)y_trace_height + 1));
                }
            }
            */

            // Draw Vertical Axis
            Font f = new Font("Arial", 7);

            e.Graphics.DrawLine(SystemPens.ControlDark, new Point(2, 0), new Point(2, this.Height));
            e.Graphics.DrawString(traces[0].max.ToString(),f,
                                    traces[0].trace_brush,
                                    new Point(border_left, border_top)
                                    );
            e.Graphics.DrawString(traces[0].min.ToString(), f,
                                    traces[0].trace_brush,
                                    new Point(border_left, (int)(border_top+y_trace_height))
                                    );

            e.Graphics.DrawLine(SystemPens.ControlDark, new Point(2, 0), new Point(2, this.Height));
            e.Graphics.DrawString(traces[2].max.ToString(), f,
                                    traces[2].trace_brush,
                                    new Point((int)(border_left+x_width-10), border_top)
                                    );
            e.Graphics.DrawString(traces[2].min.ToString(), f,
                                    traces[2].trace_brush,
                                    new Point((int)(border_left+x_width-10), (int)(border_top + y_trace_height))
                                    );

            // Draw Data Points
            for (int i = 0; i < no_of_traces; i++)
            {
                for (int j = 1; j < data_pos; j++)
                {
                    x1 = (j - 1) * x_width_step + border_left;
                    x2 = j * x_width_step + border_left;
                    if (split_trace)
                    {
                        y1 = border_top + ((i + 1) * y_trace_spacing) + ((i + 1) * y_trace_height) - (traces[i].trc.values[j - 1] * y_trace_height);
                        y2 = border_top + ((i + 1) * y_trace_spacing) + ((i + 1) * y_trace_height) - (traces[i].trc.values[j] * y_trace_height);
                        e.Graphics.DrawLine(traces[i].trace_pen,
                                            new Point((int)x1, (int)y1),
                                            new Point((int)x2, (int)y2));
                    }
                    else
                    {
                        y1 = border_top + (y_trace_spacing + y_trace_height) - (traces[i].trc.values[j - 1] * y_trace_height);
                        y2 = border_top + (y_trace_spacing + y_trace_height) - (traces[i].trc.values[j] * y_trace_height);
                        e.Graphics.DrawLine(traces[i].trace_pen,
                                            new Point((int)x1, (int)y1),
                                            new Point((int)x2, (int)y2));
                    }
                }
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

        }

        private void MyGraph_SizeChanged(object sender, EventArgs e)
        {
            x_width = this.Width - border_left - border_right -2;
            y_height = this.Height - border_top - border_bottom -2;

            if (split_trace)
                y_trace_height = (int)((y_height - (y_trace_spacing * (double)(no_of_traces))) / (double)no_of_traces);
            else
                y_trace_height = (int)(y_height);

            x_width_step = x_width / (double)no_of_display_points;
        }

        private void MyGraph_Click(object sender, EventArgs e)
        {

        }

        private void MyGraph_MouseClick(object sender, MouseEventArgs e)
        {
            int y, ymin, ymax;

            y = e.Location.Y;

            if (zoom) { zoom = false; return; }

            for (int i = 0; i < no_of_traces; i++)
            {
                ymax = (int)(border_top + ((i + 1) * y_trace_spacing) + ((i + 1) * y_trace_height));
                ymin = (int)(border_top + ((i + 1) * y_trace_spacing) + ((i) * y_trace_height) - y_trace_height);
                if (y <= ymax && y >= ymin)
                {
                    zoom = true;
                    zoom_trace = i;
                    return;
                }
            }
            zoom = false;
        }
    }
}
