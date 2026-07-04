using System.Globalization;
using System.Text;
using ISO11820.App.Config;
using ISO11820.App.Data;

namespace ISO11820.App.Core;

/// <summary>
/// 数据采集工作器
/// - 每 800ms 执行一次仿真更新
/// - 每秒记录 CSV + 触发数据广播
/// - 追踪各通道温度最大值
/// </summary>
public class DaqWorker : IDisposable
{
    private readonly AppConfig _config;
    private readonly DbHelper _db;
    private readonly TestController _controller;

    private System.Timers.Timer? _timer;
    private bool _isRunning;

    // ===== 秒边界追踪 =====
    /// <summary>距上次秒脉冲累积的毫秒数</summary>
    private double _accumulatedMs;

    // ===== 各通道最大值追踪 =====
    public double MaxTf1 { get; private set; }
    public double MaxTf2 { get; private set; }
    public double MaxTs { get; private set; }
    public double MaxTc { get; private set; }
    public int MaxTf1Time { get; private set; }
    public int MaxTf2Time { get; private set; }
    public int MaxTsTime { get; private set; }
    public int MaxTcTime { get; private set; }

    // ===== CSV 写入 =====
    private string? _csvFilePath;
    private bool _csvHeaderWritten;

    // ===== 事件（转发给 UI） =====
    /// <summary>数据广播事件（每秒一次，在后台线程触发）</summary>
    public event EventHandler<DataBroadcastEventArgs>? DataBroadcast;

    public DaqWorker(AppConfig config, DbHelper db, TestController controller)
    {
        _config = config;
        _db = db;
        _controller = controller;
    }

    /// <summary>
    /// 启动数据采集
    /// </summary>
    public void Start()
    {
        if (_isRunning) return;
        _isRunning = true;

        _timer = new System.Timers.Timer(_config.UpdateIntervalMs); // 800ms
        _timer.Elapsed += OnTick;
        _timer.AutoReset = false; // 防止回调重叠，在 OnTick 末尾手动重启
        _timer.Start();
    }

    /// <summary>
    /// 停止数据采集
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }

    /// <summary>
    /// 准备 CSV 文件路径（新建试验后调用）
    /// </summary>
    public void PrepareCsv(string productId, string testId)
    {
        var dir = Path.Combine(_config.TestDataDirectory, productId, testId);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        _csvFilePath = Path.Combine(dir, "sensor_data.csv");
        _csvHeaderWritten = false;
    }

    /// <summary>
    /// 每 800ms 触发一次（在后台线程）
    /// </summary>
    private void OnTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (!_isRunning) return;

        try
        {
            var state = _controller.CurrentState;

            // 1. 更新仿真温度（已在 TestController.Tick 中完成）
            //    TestController.Tick 由本方法主动调用
            _controller.Tick();

            // 2. 更新传感器数据库值
            UpdateSensorValues();

            // 3. 秒边界检测（800ms 累积 → 1000ms 触发秒脉冲）
            _accumulatedMs += _config.UpdateIntervalMs;
            if (_accumulatedMs >= 1000)
            {
                _accumulatedMs -= 1000;
                OnSecondTick();
            }
        }
        catch (Exception ex)
        {
            // 后台线程异常不应崩溃，记录到日志
            System.Diagnostics.Debug.WriteLine($"[DaqWorker] Error: {ex.Message}");
        }
        finally
        {
            // AutoReset=false 时手动重启定时器
            if (_isRunning)
                _timer?.Start();
        }
    }

    /// <summary>
    /// 每秒触发一次：写入 CSV、广播数据
    /// </summary>
    private void OnSecondTick()
    {
        var state = _controller.CurrentState;

        // 1. 触发 TestController 的秒脉冲（计时器递增、终止条件检查）
        var broadcastData = _controller.SecondTick();
        if (broadcastData == null) return; // Idle 状态不广播

        // 2. Recording 状态：更新最大值 + 写入 CSV
        if (state == TestState.Recording)
        {
            UpdateMaxTracking();
            WriteCsvRow();
        }

        // 3. 触发数据广播事件
        DataBroadcast?.Invoke(this, broadcastData);
    }

    // ================================================================
    // 传感器值更新
    // ================================================================

    private void UpdateSensorValues()
    {
        try
        {
            // 更新 5 个主要传感器通道
            var temps = _controller.CurrentTemperatures;
            _db.UpdateSensorValue(0, temps.tf1, temps.tf1);   // 炉温1
            _db.UpdateSensorValue(1, temps.tf2, temps.tf2);   // 炉温2
            _db.UpdateSensorValue(2, temps.ts, temps.ts);     // 表面温度
            _db.UpdateSensorValue(3, temps.tc, temps.tc);     // 中心温度
            _db.UpdateSensorValue(16, temps.tcal, temps.tcal); // 校准温度
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DaqWorker] DB update error: {ex.Message}");
        }
    }

    // ================================================================
    // CSV 写入
    // ================================================================

    private void WriteCsvRow()
    {
        if (string.IsNullOrEmpty(_csvFilePath)) return;

        try
        {
            using var writer = new StreamWriter(_csvFilePath, append: true, Encoding.UTF8);

            if (!_csvHeaderWritten)
            {
                writer.WriteLine("Time,Temp1,Temp2,TempSurface,TempCenter,TempCalibration");
                _csvHeaderWritten = true;
            }

            var temps = _controller.CurrentTemperatures;
            writer.WriteLine(string.Create(CultureInfo.InvariantCulture,
                $"{_controller.ElapsedSeconds}," +
                $"{temps.tf1:F1},{temps.tf2:F1},{temps.ts:F1},{temps.tc:F1},{temps.tcal:F1}"));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DaqWorker] CSV write error: {ex.Message}");
        }
    }

    // ================================================================
    // 最大值追踪
    // ================================================================

    private void UpdateMaxTracking()
    {
        int sec = _controller.ElapsedSeconds;

        var temps = _controller.CurrentTemperatures;
        if (temps.tf1 > MaxTf1) { MaxTf1 = temps.tf1; MaxTf1Time = sec; }
        if (temps.tf2 > MaxTf2) { MaxTf2 = temps.tf2; MaxTf2Time = sec; }
        if (temps.ts > MaxTs) { MaxTs = temps.ts; MaxTsTime = sec; }
        if (temps.tc > MaxTc) { MaxTc = temps.tc; MaxTcTime = sec; }
    }

    public void ResetMaxTracking()
    {
        MaxTf1 = 0; MaxTf2 = 0; MaxTs = 0; MaxTc = 0;
        MaxTf1Time = 0; MaxTf2Time = 0; MaxTsTime = 0; MaxTcTime = 0;
    }

    /// <summary>
    /// 获取各通道最大值快照
    /// </summary>
    public (double tf1, double tf2, double ts, double tc, int tf1t, int tf2t, int tst, int tct) GetMaxValues()
    {
        return (MaxTf1, MaxTf2, MaxTs, MaxTc, MaxTf1Time, MaxTf2Time, MaxTsTime, MaxTcTime);
    }

    public void Dispose()
    {
        Stop();
    }
}
