namespace ISO11820.App.Core;

/// <summary>
/// 系统消息（每个 tick 产生 0~N 条，广播到 UI 的消息日志）
/// </summary>
public class MasterMessage
{
    /// <summary>消息时间，格式 HH:mm:ss</summary>
    public string Time { get; set; } = string.Empty;

    /// <summary>消息内容</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 消息类型，用于 UI 配色
    /// </summary>
    public MessageType Type { get; set; } = MessageType.Info;

    public static MasterMessage Info(string msg) => new()
    {
        Time = DateTime.Now.ToString("HH:mm:ss"),
        Message = msg,
        Type = MessageType.Info
    };

    public static MasterMessage Warning(string msg) => new()
    {
        Time = DateTime.Now.ToString("HH:mm:ss"),
        Message = msg,
        Type = MessageType.Warning
    };
}

/// <summary>
/// 消息类型（决定 UI 显示颜色）
/// </summary>
public enum MessageType
{
    Info,    // 白色 — 普通状态变更
    Warning, // 黄色 — 提醒/警告
    Success  // 绿色 — 操作成功
}
