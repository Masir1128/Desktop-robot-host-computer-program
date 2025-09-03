using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using OxyPlot.Axes;    // 如果需要坐标轴等类

namespace rbo
{
    public partial class Dataview : Form
    {

        private Panel panel1, panel2, panel3, panel4;

        // 第一象限：电压曲线
        private PlotView _plotViewVoltage;
        private LineSeries _voltageSeries;

        // 第四象限：机器人坐标 (动态)
        private PlotView _plotViewRobotPose;
        private ScatterSeries _robotPoseSeries;

        // 定时器模拟动态数据
        private Timer _timer;
        private double _currentTime = 0.0;
        private Random _rand = new Random();

        public Dataview()
        {
            InitializeComponent();

            this.Text = "机器人数据采集分析";
            this.ClientSize = new Size(2068, 1284);

            // 创建四个象限的 Panel
            InitializePanels();

            // 初始化每个象限的内容
            InitializeQuadrant1(); // 第一象限：电压曲线
            InitializeQuadrant2(); // 第二象限：饼图
            InitializeQuadrant3(); // 第三象限：散点图
            InitializeQuadrant4(); // 第四象限：机器人坐标

            // 启动定时器，模拟动态数据
            _timer = new Timer
            {
                Interval = 500 // 每隔500ms刷新一次
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }



        // 初始化四个 Panel
        private void InitializePanels()
        {
            panel1 = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(0, 0),
                Size = new Size(this.ClientSize.Width / 2, this.ClientSize.Height / 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            panel1 = CreateLabeledPanel("第一象限：机器人电压实时采集", 0, 0);
            this.Controls.Add(panel1);

            panel2 = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(this.ClientSize.Width / 2, 0),
                Size = new Size(this.ClientSize.Width / 2, this.ClientSize.Height / 2),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            panel2 = CreateLabeledPanel("第二象限：机器人模拟数据（样本）", this.ClientSize.Width / 2, 0);
            this.Controls.Add(panel2);

            panel3 = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(0, this.ClientSize.Height / 2),
                Size = new Size(this.ClientSize.Width / 2, this.ClientSize.Height / 2),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            panel3 = CreateLabeledPanel("第三象限：散点图", 0, this.ClientSize.Height / 2);
            this.Controls.Add(panel3);

            panel4 = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2),
                Size = new Size(this.ClientSize.Width / 2, this.ClientSize.Height / 2),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            panel4 = CreateLabeledPanel("第四象限：机器人坐标实时位置", this.ClientSize.Width / 2, this.ClientSize.Height / 2);
            this.Controls.Add(panel4);
        }


        // 创建带标题的面板
        private Panel CreateLabeledPanel(string title, int x, int y)
        {
            var panel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(x, y),
                Size = new Size(this.ClientSize.Width / 2, this.ClientSize.Height / 2)
            };

            // 添加标题 Label
            var label = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Height = 30,
                BackColor = Color.LightGray
            };
            panel.Controls.Add(label);

            return panel;
        }


        // ======================================================================
        // (1) 第一象限: 电压曲线 (动态数据)
        // ======================================================================
        private void InitializeQuadrant1()
        {
            _plotViewVoltage = new PlotView
            {
                Dock = DockStyle.Fill
            };
            panel1.Controls.Add(_plotViewVoltage);

            var voltageModel = new PlotModel { Title = "机器人电压随时间变化 (动态)" };
            _voltageSeries = new LineSeries
            {
                MarkerType = MarkerType.None,
                Title = "Voltage(V)"
            };
            voltageModel.Series.Add(_voltageSeries);

            // 时间轴
            voltageModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Time (s)"
            });

            // 电压值轴
            voltageModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Voltage",
                Minimum = 0,
                Maximum = 30
            });

            _plotViewVoltage.Model = voltageModel;
        }

        // ======================================================================
        // (2) 第二象限: 饼图
        // ======================================================================
        private void InitializeQuadrant2()
        {
            var plotViewPie = new PlotView
            {
                Dock = DockStyle.Fill
            };
            panel2.Controls.Add(plotViewPie);

            var pieModel = new PlotModel { Title = "第二象限 - 饼图" };
            var pieSeries = new PieSeries
            {
                StrokeThickness = 1.0,
                InsideLabelPosition = 0.8,
                AngleSpan = 360,
                StartAngle = 0
            };

            pieSeries.Slices.Add(new PieSlice("苹果", 40) { IsExploded = false, Fill = OxyColors.Red });
            pieSeries.Slices.Add(new PieSlice("香蕉", 25) { IsExploded = true, Fill = OxyColors.Yellow });
            pieSeries.Slices.Add(new PieSlice("葡萄", 10) { IsExploded = false, Fill = OxyColors.Purple });
            pieSeries.Slices.Add(new PieSlice("橘子", 25) { IsExploded = false, Fill = OxyColors.Orange });

            pieModel.Series.Add(pieSeries);
            plotViewPie.Model = pieModel;
        }

        // ======================================================================
        // (3) 第三象限: 散点图
        // ======================================================================
        private void InitializeQuadrant3()
        {
            var plotViewScatter = new PlotView
            {
                Dock = DockStyle.Fill
            };
            panel3.Controls.Add(plotViewScatter);

            var scatterModel = new PlotModel { Title = "第三象限 - 散点图" };
            var scatterSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Diamond,
                MarkerSize = 5,
                Title = "Scatter Example"
            };

            for (int x = 0; x < 10; x++)
            {
                double y = x * x + _rand.NextDouble() * 3.0;
                scatterSeries.Points.Add(new ScatterPoint(x, y));
            }

            scatterModel.Series.Add(scatterSeries);

            scatterModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "X" });
            scatterModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Y" });

            plotViewScatter.Model = scatterModel;
        }

        // ======================================================================
        // (4) 第四象限: 机器人坐标 (动态数据)
        // ======================================================================
        private void InitializeQuadrant4()
        {
            _plotViewRobotPose = new PlotView
            {
                Dock = DockStyle.Fill
            };
            panel4.Controls.Add(_plotViewRobotPose);

            var poseModel = new PlotModel { Title = "机器人坐标 (2D 动态)" };
            _robotPoseSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                Title = "Robot XY"
            };
            poseModel.Series.Add(_robotPoseSeries);

            poseModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "X" });
            poseModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Y" });

            _plotViewRobotPose.Model = poseModel;
        }

        // ======================================================================
        // 定时器：动态刷新数据
        // ======================================================================
        private void Timer_Tick(object sender, EventArgs e)
        {
            _currentTime += 0.5;

            // 机器人坐标 (第四象限)
            double x = Math.Sin(_currentTime) * 5 + _rand.NextDouble();
            double y = Math.Cos(_currentTime) * 5 + _rand.NextDouble();
            _robotPoseSeries.Points.Add(new ScatterPoint(x, y));
            if (_robotPoseSeries.Points.Count > 200)
                _robotPoseSeries.Points.RemoveAt(0);

            _plotViewRobotPose.Model?.InvalidatePlot(true);

            // 电压曲线 (第一象限)
            double voltage = 24.0 - (_rand.NextDouble() * 3.0);
            _voltageSeries.Points.Add(new DataPoint(_currentTime, voltage));
            if (_voltageSeries.Points.Count > 200)
                _voltageSeries.Points.RemoveAt(0);

            _plotViewVoltage.Model?.InvalidatePlot(true);
        }
    }
}
