using ISO11820.App.Config;
using ISO11820.App.Data;

namespace ISO11820.App.Core;

/// <summary>
/// 全局应用上下文（单例模式）
/// 持有所有核心对象引用，贯穿整个应用生命周期
/// </summary>
public class AppContext
{
    private static readonly Lazy<AppContext> _instance = new(() => new AppContext());
    public static AppContext Instance => _instance.Value;

    private AppContext() { }

    /// <summary>配置管理器</summary>
    public AppConfig Config { get; set; } = null!;

    /// <summary>数据库操作助手</summary>
    public DbHelper Db { get; set; } = null!;

    /// <summary>当前登录的用户名</summary>
    public string CurrentOperator { get; set; } = string.Empty;

    /// <summary>当前登录的用户角色（admin / operator）</summary>
    public string CurrentUserType { get; set; } = string.Empty;

    /// <summary>试验控制器（状态机）</summary>
    public TestController Controller { get; set; } = null!;

    /// <summary>数据采集工作器</summary>
    public DaqWorker DaqWorker { get; set; } = null!;
}
