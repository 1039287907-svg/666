using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace ISO11820.App.Forms;

/// <summary>
/// 图表控件 — 单 Y 轴（0~800°C），4 条曲线。
/// TS/TC 在 Recording 前以 NaN 占位不显示，Recording 后按设计文档算法接近炉温。
/// </summary>
public sealed class ChartPanel : Panel
{
    private readonly PlotView _plotView;
    private readonly PlotModel _plotModel;

    private readonly LineSeries _seriesTF1;
    private readonly LineSeries _seriesTF2;
    private readonly LineSeries _seriesTS;
    private readonly LineSeries _seriesTC;

    private int _dataCount;

    private double _minTemp = double.MaxValue, _maxTemp = double.MinValue;

    private readonly LinearAxis _axisX;
    private readonly LinearAxis _axisY;

    public ChartPanel()
    {
        this.Dock = DockStyle.Fill;
        this.BackColor = Color.White;

        _plotModel = new PlotModel
        {
            Title = "温度曲线",
            IsLegendVisible = true
        };

        // === X 轴 ===
        _axisX = new LinearAxis
        {
            Position = AxisPosition.Bottom,
            Title = "时间 (秒)",
            IsZoomEnabled = true,
            IsPanEnabled = true
        };
        _plotModel.Axes.Add(_axisX);

        // === Y 轴 ===
        _axisY = new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "温度 (°C)",
            Minimum = 0,
            Maximum = 800,
            IsZoomEnabled = true,
            IsPanEnabled = true
        };
        _plotModel.Axes.Add(_axisY);

        // === 4 条曲线 ===
        _seriesTF1 = new LineSeries { Title = "炉温1", Color = OxyColor.FromRgb(245, 34, 45),   StrokeThickness = 1.5 };
        _seriesTF2 = new LineSeries { Title = "炉温2", Color = OxyColor.FromRgb(24, 144, 255),  StrokeThickness = 1.5 };
        _seriesTS  = new LineSeries { Title = "表面温", Color = OxyColor.FromRgb(82, 196, 26),   StrokeThickness = 2.0 };
        _seriesTC  = new LineSeries { Title = "中心温", Color = OxyColor.FromRgb(250, 140, 22),  StrokeThickness = 2.0 };

        _plotModel.Series.Add(_seriesTF1);
        _plotModel.Series.Add(_seriesTF2);
        _plotModel.Series.Add(_seriesTS);
        _plotModel.Series.Add(_seriesTC);

        // === PlotView ===
        _plotView = new PlotView { Dock = DockStyle.Fill, BackColor = Color.White };
        _plotView.Model = _plotModel;
        _plotView.Resize += (_, _) => _plotView.InvalidatePlot(true);

        // === 手绘图例条 ===
        var legendBar = new Panel { Height = 30, Dock = DockStyle.Bottom, BackColor = Color.FromArgb(250, 250, 250) };
        AddLegendItem(legendBar, "炉温1",  Color.FromArgb(245, 34, 45),   10);
        AddLegendItem(legendBar, "炉温2",  Color.FromArgb(24, 144, 255),  80);
        AddLegendItem(legendBar, "表面温", Color.FromArgb(82, 196, 26),   150);
        AddLegendItem(legendBar, "中心温", Color.FromArgb(250, 140, 22),  240);

        this.Controls.Add(_plotView);
        this.Controls.Add(legendBar);
    }

    // ================================================================
    // 公开接口
    // ================================================================

    public void AddData(double tf1, double tf2, double ts, double tc,
        int elapsed, bool isRecording)
    {
        _dataCount++;
        _seriesTF1.Points.Add(new DataPoint(_dataCount, tf1));
        _seriesTF2.Points.Add(new DataPoint(_dataCount, tf2));
        _seriesTS.Points.Add(new DataPoint(_dataCount, isRecording ? ts : double.NaN));
        _seriesTC.Points.Add(new DataPoint(_dataCount, isRecording ? tc : double.NaN));

        // 追踪炉温范围
        _minTemp = Math.Min(_minTemp, Math.Min(tf1, tf2));
        _maxTemp = Math.Max(_maxTemp, Math.Max(tf1, tf2));
        if (isRecording)
        {
            _minTemp = Math.Min(_minTemp, Math.Min(ts, tc));
            _maxTemp = Math.Max(_maxTemp, Math.Max(ts, tc));
        }

        // X 轴滚动
        if (_dataCount > 600)
        {
            _axisX.Minimum = _dataCount - 600;
            _axisX.Maximum = _dataCount;
        }

        // 动态 Y 轴
        if (_maxTemp > _minTemp)
        {
            double r = _maxTemp - _minTemp;
            _axisY.Minimum = Math.Max(0, Math.Floor((_minTemp - r * 0.08) / 50) * 50);
            _axisY.Maximum = Math.Ceiling((_maxTemp + r * 0.08) / 50) * 50;
        }

        _plotView.InvalidatePlot(true);
    }

    public void Reset()
    {
        _dataCount = 0;
        _minTemp = double.MaxValue;
        _maxTemp = double.MinValue;
        _seriesTF1.Points.Clear();
        _seriesTF2.Points.Clear();
        _seriesTS.Points.Clear();
        _seriesTC.Points.Clear();
        _axisX.Minimum = double.NaN;
        _axisX.Maximum = double.NaN;
        _axisY.Minimum = 0;
        _axisY.Maximum = 800;
        _plotView.InvalidatePlot(true);
    }

    // ================================================================
    // 内部
    // ================================================================

    private static void AddLegendItem(Panel bar, string text, Color color, int x)
    {
        bar.Controls.Add(new Panel
        {
            Location = new Point(x, 6), Size = new Size(20, 14),
            BackColor = color, BorderStyle = BorderStyle.FixedSingle
        });
        bar.Controls.Add(new Label
        {
            Text = text, Location = new Point(x + 24, 6),
            AutoSize = true, Font = new Font("Microsoft YaHei", 9F),
            BackColor = Color.Transparent
        });
    }
}
