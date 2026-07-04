namespace ISO11820.App.Core;

/// <summary>
/// 数据广播事件参数
/// DaqWorker 每秒生成一次，通过事件发送到 UI 层
/// </summary>
public class DataBroadcastEventArgs : EventArgs
{
    /// <summary>炉温1 (°C)</summary>
    public double TF1 { get; set; }

    /// <summary>炉温2 (°C)</summary>
    public double TF2 { get; set; }

    /// <summary>表面温度 (°C)</summary>
    public double TS { get; set; }

    /// <summary>中心温度 (°C)</summary>
    public double TC { get; set; }

    /// <summary>校准温度 (°C)</summary>
    public double TCal { get; set; }

    /// <summary>记录阶段已过秒数（非 Recording 状态为 0）</summary>
    public int ElapsedSeconds { get; set; }

    /// <summary>当前状态的中文描述</summary>
    public string StatusText { get; set; } = string.Empty;

    /// <summary>当前状态枚举值</summary>
    public TestState CurrentState { get; set; }

    /// <summary>温度漂移 (°C/10min)</summary>
    public double TemperatureDrift { get; set; }

    /// <summary>温度是否稳定</summary>
    public bool IsStable { get; set; }

    /// <summary>本轮产生的系统消息（0~N 条）</summary>
    public List<MasterMessage> Messages { get; set; } = new();

    /// <summary>当前试验的样品编号（可能为空）</summary>
    public string? ProductId { get; set; }

    /// <summary>当前试验的试验标识（可能为空）</summary>
    public string? TestId { get; set; }
}
