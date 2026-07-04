using ISO11820.App.Config;

namespace ISO11820.App.Core;

/// <summary>
/// 5通道温度仿真引擎
/// 每 800ms 调用一次 Update()，根据当前试验状态和仿真阶段计算下一帧温度
/// </summary>
public class SensorSimulator
{
    private readonly AppConfig _config;
    private readonly Random _rng = new();

    // ===== 当前温度值 =====
    public double TF1 { get; private set; }
    public double TF2 { get; private set; }
    public double TS { get; private set; }
    public double TC { get; private set; }
    public double TCal { get; private set; }

    // ===== 稳定判定 =====
    /// <summary>连续稳定帧计数</summary>
    public int StableCount { get; private set; }

    /// <summary>温度是否已稳定</summary>
    public bool IsStable { get; private set; }

    // ===== 升温阶段标记 =====
    /// <summary>是否曾在记录阶段记录过（Reset 时清除）</summary>
    public bool HasEnteredRecording { get; private set; }

    // 缓存的配置值
    private readonly double _targetTemp;
    private readonly double _stableThreshold;
    private readonly double _heatingRatePerSecond;
    private readonly double _tempFluctuation;
    private readonly double _heatingPerTick; // 每帧升温量

    public SensorSimulator(AppConfig config)
    {
        _config = config;
        _targetTemp = config.TargetFurnaceTemp;
        _stableThreshold = config.StableThreshold;
        _heatingRatePerSecond = config.HeatingRatePerSecond;
        _tempFluctuation = config.TempFluctuation;

        // 每 800ms 一帧，每帧升温 = 每秒升温 × 0.8
        _heatingPerTick = _heatingRatePerSecond * 0.8;

        // 初始温度
        TF1 = config.InitialFurnaceTemp;
        TF2 = config.InitialFurnaceTemp;
        TS = TF1 * 0.3;
        TC = TF1 * 0.25;
        TCal = TF1;
    }

    /// <summary>
    /// 每帧调用一次（800ms），更新所有通道的温度值
    /// </summary>
    /// <param name="state">当前试验状态</param>
    /// <returns>本轮是否触发了稳定→就绪的切换</returns>
    public bool Update(TestState state)
    {
        switch (state)
        {
            case TestState.Idle:
                return UpdateCooling();

            case TestState.Preparing:
                return UpdateHeating();

            case TestState.Ready:
                return UpdateStable();

            case TestState.Recording:
                HasEnteredRecording = true;
                return UpdateRecording();

            case TestState.Complete:
                return UpdateStable(); // Complete 后保持炉温

            default:
                return false;
        }
    }

    /// <summary>
    /// 升温阶段：TF1 < 747°C 时持续升温，>= 747°C 后钳位到目标温度并等待稳定
    /// </summary>
    private bool UpdateHeating()
    {
        if (TF1 < _targetTemp - _stableThreshold) // < 747°C：持续升温
        {
            TF1 += _heatingPerTick + Noise();
            TF2 += _heatingPerTick + Noise();
        }
        else // >= 747°C：钳位到目标温度，等待稳定
        {
            TF1 = _targetTemp + Noise();
            TF2 = _targetTemp + Noise();
        }

        // 表面温和中心温在非记录阶段低值跟随
        TS = TF1 * 0.3 + Noise();
        TC = TF1 * 0.25 + Noise();

        // 校准温
        TCal = TF1 + Noise() * 2;

        // 检查是否进入稳定阶段
        if (TF1 >= _targetTemp - _stableThreshold) // >= 747°C
        {
            return CheckStableCondition();
        }
        else
        {
            StableCount = 0;
            IsStable = false;
        }
        return false;
    }

    /// <summary>
    /// 稳定阶段：钳位到 750°C 附近，累计稳定计数
    /// </summary>
    private bool UpdateStable()
    {
        TF1 = _targetTemp + Noise();
        TF2 = _targetTemp + Noise();

        // 非记录阶段，表面温和中心温保持低值跟随
        if (!HasEnteredRecording)
        {
            TS = TF1 * 0.3 + Noise();
            TC = TF1 * 0.25 + Noise();
        }

        TCal = TF1 + Noise() * 2;

        return CheckStableCondition();
    }

    /// <summary>
    /// 记录阶段：炉温保持稳定，表面温和中心温向目标值指数接近。
    /// 按设计文档：surfaceTarget = min(TF1 × 0.95, 800)、centerTarget = min(TF1 × 0.85, 750)
    /// </summary>
    private bool UpdateRecording()
    {
        TF1 = _targetTemp + Noise();
        TF2 = _targetTemp + Noise();

        double surfaceTarget = Math.Min(TF1 * 0.95, 800);
        TS += (surfaceTarget - TS) * 0.02 + Noise();

        double centerTarget = Math.Min(TF1 * 0.85, 750);
        TC += (centerTarget - TC) * 0.01 + Noise();

        TCal = TF1 + Noise() * 2;

        CheckStableCondition();
        return false;
    }

    /// <summary>
    /// 降温阶段：Idle 状态下炉温缓慢下降
    /// </summary>
    private bool UpdateCooling()
    {
        TF1 -= 0.5 + Noise() * 0.1;
        TF2 -= 0.5 + Noise() * 0.1;

        // 不低于环境温度（约 25°C）
        if (TF1 < 25) TF1 = 25;
        if (TF2 < 25) TF2 = 25;

        TS = TF1 * 0.3 + Noise();
        TC = TF1 * 0.25 + Noise();
        TCal = TF1 + Noise() * 2;

        StableCount = 0;
        IsStable = false;
        return false;
    }

    /// <summary>
    /// 检查稳定条件：745~755°C 且连续稳定计数 > 3（约 3.2 秒）
    /// </summary>
    private bool CheckStableCondition()
    {
        bool inRange = TF1 >= 745 && TF1 <= 755;

        if (inRange)
        {
            StableCount++;

            if (StableCount > 3 && !IsStable)
            {
                IsStable = true;
                return true; // 刚变为稳定，通知调用方（触发 Preparing→Ready）
            }
        }
        else
        {
            StableCount = 0;

            if (IsStable)
            {
                IsStable = false;
                // 温度跌出范围 → 自动回退到 Preparing
                // 返回 false 因为不是"刚稳定"
            }
        }
        return false;
    }

    /// <summary>
    /// 随机噪声 = Random(-1, 1) × TempFluctuation
    /// </summary>
    private double Noise()
    {
        return (_rng.NextDouble() * 2 - 1) * _tempFluctuation;
    }

    /// <summary>
    /// 重置仿真器状态（新建试验或停止加热后调用）
    /// </summary>
    /// <param name="resetTemperatures">是否将温度也重置为初始值</param>
    public void Reset(bool resetTemperatures = true)
    {
        if (resetTemperatures)
        {
            TF1 = _config.InitialFurnaceTemp;
            TF2 = _config.InitialFurnaceTemp;
            TS = TF1 * 0.3;
            TC = TF1 * 0.25;
            TCal = TF1;
        }
        StableCount = 0;
        IsStable = false;
        HasEnteredRecording = false;
    }

    /// <summary>
    /// 仅重置稳定状态（新建试验后，炉温不变但重新判定稳定）
    /// </summary>
    public void ResetStability()
    {
        StableCount = 0;
        IsStable = false;
        // 不重置 HasEnteredRecording — 样品已加热，温度应保持
    }

    /// <summary>
    /// 重置样品温度到环境温度（模拟开始记录时将样品放入炉内）
    /// </summary>
    public void ResetSampleTemps(double ambientTemp)
    {
        TS = ambientTemp + Noise();
        TC = ambientTemp + Noise();
    }
}
