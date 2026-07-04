namespace ISO11820.App.Core;

/// <summary>
/// 试验状态机状态枚举
/// </summary>
public enum TestState
{
    /// <summary>空闲（炉子未加热）</summary>
    Idle = 0,

    /// <summary>升温中（炉温 < 747°C）</summary>
    Preparing = 1,

    /// <summary>就绪（温度稳定在 745~755°C，可以开始记录）</summary>
    Ready = 2,

    /// <summary>记录中（正在记录每秒温度数据）</summary>
    Recording = 3,

    /// <summary>完成（记录结束，等待保存试验记录）</summary>
    Complete = 4
}

/// <summary>
/// 状态中文描述
/// </summary>
public static class TestStateExtensions
{
    public static string ToChinese(this TestState state) => state switch
    {
        TestState.Idle => "空闲",
        TestState.Preparing => "升温中",
        TestState.Ready => "就绪",
        TestState.Recording => "记录中",
        TestState.Complete => "完成",
        _ => "未知"
    };
}
