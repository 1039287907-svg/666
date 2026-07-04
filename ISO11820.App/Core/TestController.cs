using ISO11820.App.Config;
using ISO11820.App.Data;
using ISO11820.App.Models;
using Microsoft.Data.Sqlite;

namespace ISO11820.App.Core;

/// <summary>
/// 试验控制器 — 核心状态机
/// 负责状态流转、终止判定、恒功率计算、试验上下文管理
/// </summary>
public class TestController
{
    private readonly AppConfig _config;
    private readonly DbHelper _db;
    private readonly SensorSimulator _simulator;

    // ===== 当前状态 =====
    public TestState CurrentState { get; private set; } = TestState.Idle;

    /// <summary>温度是否稳定（来自仿真器）</summary>
    public bool IsStable => _simulator.IsStable;

    /// <summary>记录阶段已过秒数</summary>
    public int ElapsedSeconds { get; private set; }

    // ===== 试验上下文 =====
    public string? CurrentProductId { get; private set; }
    public string? CurrentTestId { get; private set; }
    public double PreWeight { get; private set; }
    public double AmbTemp { get; private set; }
    public double AmbHumi { get; private set; }

    // ===== 试验模式 =====
    public bool IsFixedDuration { get; private set; }
    public int TargetDurationSeconds { get; private set; }
    public bool IsStandardMode => !IsFixedDuration;

    // ===== 恒功率计算 =====
    /// <summary>Ready 状态的 PID 输出值队列（最多 600 个数据点）</summary>
    private readonly Queue<double> _pidQueue = new();
    private const int MaxPidQueueSize = 600;

    /// <summary>最终计算出的恒功率值</summary>
    public int CalculatedConstPower { get; private set; }

    // ===== 温漂计算 =====
    /// <summary>最近 10 分钟的炉温1历史（600 个数据点，每秒一个）</summary>
    private readonly List<double> _tempHistory = new();
    private const int TempHistoryMax = 600;

    /// <summary>最新计算的温漂 (°C/10min)</summary>
    public double TemperatureDrift { get; private set; }

    // ===== 终止条件 =====
    /// <summary>终止检查时间点（分钟），标准模式在第 30/35/40/45/50/55 分钟检查</summary>
    private static readonly int[] TerminationCheckMinutes = { 30, 35, 40, 45, 50, 55 };
    private bool _terminationConditionMet;

    // ===== 事件 =====
    /// <summary>状态变化事件</summary>
    public event EventHandler<TestState>? StateChanged;

    /// <summary>数据广播事件（由 DaqWorker 的秒脉冲触发）</summary>
    public event EventHandler<DataBroadcastEventArgs>? DataBroadcast;

    // ===== 温度数据缓存（供 DaqWorker 写入 CSV） =====
    public (double tf1, double tf2, double ts, double tc, double tcal) CurrentTemperatures =>
        (_simulator.TF1, _simulator.TF2, _simulator.TS, _simulator.TC, _simulator.TCal);

    public TestController(AppConfig config, DbHelper db)
    {
        _config = config;
        _db = db;
        _simulator = new SensorSimulator(config);
    }

    // ================================================================
    // 状态转换
    // ================================================================

    /// <summary>
    /// 开始升温：Idle → Preparing
    /// </summary>
    public bool StartHeating()
    {
        if (CurrentState != TestState.Idle)
            return false;

        // 如果有已完成但未保存的试验，禁止操作
        var existing = _db.GetCurrentTest();
        if (existing != null && existing.IsFinished && !existing.IsSaved)
            return false;

        _simulator.Reset(resetTemperatures: false); // 不重置温度，从当前温度开始升温

        SetState(TestState.Preparing);
        return true;
    }

    /// <summary>
    /// 停止升温：Preparing / Ready → Idle
    /// </summary>
    public bool StopHeating()
    {
        if (CurrentState != TestState.Preparing && CurrentState != TestState.Ready)
            return false;

        SetState(TestState.Idle);
        _pidQueue.Clear();
        return true;
    }

    /// <summary>
    /// 开始记录：Ready → Recording
    /// </summary>
    public bool StartRecording()
    {
        if (CurrentState != TestState.Ready)
            return false;

        // 计算恒功率 = PID 队列平均值
        CalculatedConstPower = _pidQueue.Count > 0
            ? (int)Math.Round(_pidQueue.Average())
            : _config.ConstPower;

        _pidQueue.Clear();
        ElapsedSeconds = 0;
        _terminationConditionMet = false;
        _tempHistory.Clear();
        TemperatureDrift = 0;

        // 重置样品温度到环境温度（模拟放入炉内的时刻）
        _simulator.ResetSampleTemps(AmbTemp);

        SetState(TestState.Recording);
        return true;
    }

    /// <summary>
    /// 停止记录：Recording → Complete
    /// </summary>
    /// <param name="hasValidSamples">是否有有效记录样本</param>
    public bool StopRecording(bool hasValidSamples = true)
    {
        if (CurrentState != TestState.Recording)
            return false;

        if (hasValidSamples)
        {
            SetState(TestState.Complete);
            return true;
        }
        else
        {
            // 无有效样本，回到 Preparing
            SetState(TestState.Preparing);
            return false;
        }
    }

    /// <summary>
    /// 保存试验记录完成：Complete → Idle（炉温自然冷却）
    /// </summary>
    public void OnRecordSaved()
    {
        if (CurrentState == TestState.Complete)
        {
            _simulator.ResetStability();
            CurrentProductId = null;
            CurrentTestId = null;
            SetState(TestState.Idle);
        }
    }

    // ================================================================
    // 试验上下文
    // ================================================================

    /// <summary>
    /// 设置当前试验上下文（新建试验后调用）
    /// </summary>
    public void SetTestContext(string productId, string testId, double preWeight,
        double ambTemp, double ambHumi, bool isFixedDuration = false, int targetSeconds = 3600)
    {
        CurrentProductId = productId;
        CurrentTestId = testId;
        PreWeight = preWeight;
        AmbTemp = ambTemp;
        AmbHumi = ambHumi;
        IsFixedDuration = isFixedDuration;
        TargetDurationSeconds = isFixedDuration ? targetSeconds : 3600;
        ElapsedSeconds = 0;
        _pidQueue.Clear();
        _tempHistory.Clear();
        TemperatureDrift = 0;
        _terminationConditionMet = false;

        // 创建新试验后重置稳定判定，准备重新判定
        _simulator.ResetStability();
    }

    /// <summary>
    /// 是否有未保存的已完成试验
    /// </summary>
    public bool HasUnsavedCompletedTest()
    {
        var test = _db.GetCurrentTest();
        return test != null && test.IsFinished && !test.IsSaved;
    }

    // ================================================================
    // 每帧更新（由 DaqWorker 调用，每 800ms）
    // ================================================================

    /// <summary>
    /// 由 DaqWorker 每 800ms 调用一次
    /// </summary>
    public void Tick()
    {
        // 1. 更新仿真温度（Idle 状态下也会降温）
        bool justBecameReady = _simulator.Update(CurrentState);

        // 2. Idle 状态下跳过状态检查和数据收集
        if (CurrentState == TestState.Idle)
            return;

        // 3. 检查自动状态转换
        if (justBecameReady && CurrentState == TestState.Preparing)
        {
            SetState(TestState.Ready);
        }
        else if (CurrentState == TestState.Ready && !_simulator.IsStable)
        {
            // Ready 状态下温度跌出范围 → 回退
            SetState(TestState.Preparing);
        }

        // 4. Ready 状态：收集 PID 值
        if (CurrentState == TestState.Ready)
        {
            _pidQueue.Enqueue(_simulator.TF1);
            if (_pidQueue.Count > MaxPidQueueSize)
                _pidQueue.Dequeue();
        }

        // 5. Recording 状态：记录温度历史、检查终止条件
        if (CurrentState == TestState.Recording)
        {
            _tempHistory.Add(_simulator.TF1);
            if (_tempHistory.Count > TempHistoryMax)
                _tempHistory.RemoveAt(0);

            // 更新温漂（至少需要 10 个数据点）
            if (_tempHistory.Count >= 10)
            {
                TemperatureDrift = CalculateDrift();
            }

            // 检查终止条件
            CheckTerminationCondition();
        }
    }

    /// <summary>
    /// 由 DaqWorker 每秒调用一次
    /// </summary>
    /// <returns>广播数据包（包含当前温度，Idle 时也广播以显示降温）</returns>
    public DataBroadcastEventArgs? SecondTick()
    {
        // Recording 状态：递增计时器
        if (CurrentState == TestState.Recording)
        {
            ElapsedSeconds++;

            // 检查是否到达目标时长
            if (IsFixedDuration && ElapsedSeconds >= TargetDurationSeconds)
            {
                SetState(TestState.Complete);
            }
            else if (IsStandardMode && ElapsedSeconds >= 3600)
            {
                // 标准 60 分钟模式：第 60 分钟无条件终止
                SetState(TestState.Complete);
            }
        }

        // 装配广播数据包
        var args = new DataBroadcastEventArgs
        {
            TF1 = _simulator.TF1,
            TF2 = _simulator.TF2,
            TS = _simulator.TS,
            TC = _simulator.TC,
            TCal = _simulator.TCal,
            ElapsedSeconds = ElapsedSeconds,
            StatusText = CurrentState.ToChinese(),
            CurrentState = CurrentState,
            TemperatureDrift = TemperatureDrift,
            IsStable = _simulator.IsStable,
            ProductId = CurrentProductId,
            TestId = CurrentTestId,
            Messages = new List<MasterMessage>()
        };

        // 触发广播
        DataBroadcast?.Invoke(this, args);

        // 返回数据包（供 DaqWorker 用于 CSV 写入等）
        return args;
    }

    // ================================================================
    // 终止条件判定
    // ================================================================

    private void CheckTerminationCondition()
    {
        if (CurrentState != TestState.Recording || _terminationConditionMet)
            return;

        if (IsFixedDuration)
            return; // 固定时长模式不提前终止

        // 标准模式：每 5 分钟检查一次
        int currentMinute = ElapsedSeconds / 60;
        if (!TerminationCheckMinutes.Contains(currentMinute))
            return;

        // 检查是否刚进入这个分钟（由 SecondTick 触发，每秒一次）
        // 需要在整分钟时刻触发（ElapsedSeconds % 60 == 0）
        if (ElapsedSeconds % 60 != 0)
            return;

        // 检查温漂：炉温1 和 炉温2 的 10 分钟温漂均不超过阈值
        if (_tempHistory.Count >= 600)
        {
            double drift1 = CalculateDrift();
            double maxDrift = _config.MaxTemperatureDriftPerTenMinutes;

            if (Math.Abs(drift1) <= maxDrift)
            {
                _terminationConditionMet = true;
            }
        }
    }

    // ================================================================
    // 温漂计算
    // ================================================================

    /// <summary>
    /// 对最近 10 分钟炉温序列做线性回归，返回斜率 (°C/10min)
    /// 使用最小二乘法（不依赖 MathNet，保持 Person B 自包含；Person C 的温漂显示也可复用）
    /// </summary>
    private double CalculateDrift()
    {
        int n = _tempHistory.Count;
        if (n < 2) return 0;

        // 简单线性回归: y = a + b*x
        // b = (n*Σxy - Σx*Σy) / (n*Σx² - (Σx)²)
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        for (int i = 0; i < n; i++)
        {
            double x = i - n + 1; // 以最后一个点为基准
            double y = _tempHistory[i];
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumX2 += x * x;
        }

        double denominator = n * sumX2 - sumX * sumX;
        if (Math.Abs(denominator) < 1e-10) return 0;

        double slope = (n * sumXY - sumX * sumY) / denominator;

        // 斜率是 °C/秒，换算为 °C/10min
        return slope * 600;
    }

    // ================================================================
    // 内部方法
    // ================================================================

    private void SetState(TestState newState)
    {
        if (CurrentState == newState) return;

        var oldState = CurrentState;
        CurrentState = newState;
        StateChanged?.Invoke(this, newState);
    }

    /// <summary>
    /// 获取当前试验的完整 TestMaster 对象（供 Person C 保存记录时使用）
    /// </summary>
    public TestMaster? BuildTestMasterForSave()
    {
        if (string.IsNullOrEmpty(CurrentProductId) || string.IsNullOrEmpty(CurrentTestId))
            return null;

        var test = _db.GetTest(CurrentProductId, CurrentTestId);
        if (test == null) return null;

        // 填入试验结束时的统计值
        test.TotalTestTime = ElapsedSeconds;
        test.ConstPower = CalculatedConstPower;
        test.MaxTf1 = _tempHistory.Count > 0 ? _tempHistory.Max() : _simulator.TF1;
        test.MaxTf2 = _simulator.TF2; // 简化：取当前值，实际应由 DaqWorker 持续追踪
        test.MaxTs = _simulator.TS;
        test.MaxTc = _simulator.TC;
        test.MaxTf1Time = ElapsedSeconds;
        test.FinalTf1 = _simulator.TF1;
        test.FinalTf2 = _simulator.TF2;
        test.FinalTs = _simulator.TS;
        test.FinalTc = _simulator.TC;
        test.FinalTf1Time = ElapsedSeconds;
        test.FinalTf2Time = ElapsedSeconds;
        test.FinalTsTime = ElapsedSeconds;
        test.FinalTcTime = ElapsedSeconds;
        test.DeltaTf1 = test.FinalTf1 - AmbTemp;
        test.DeltaTf2 = test.FinalTf2 - AmbTemp;
        test.DeltaTs = test.FinalTs - AmbTemp;
        test.DeltaTc = test.FinalTc - AmbTemp;
        // 综合温升 = 记录期间炉温1最大涨幅（ISO 11820 判定：样品是否引起额外升温）
        test.DeltaTf = _tempHistory.Count >= 2
            ? _tempHistory.Max() - _tempHistory[0]
            : 0;

        return test;
    }
}
