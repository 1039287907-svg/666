using Serilog;
using ISO11820.App.Config;
using ISO11820.App.Core;
using ISO11820.App.Data;
using AppContext = ISO11820.App.Core.AppContext;
using ISO11820.App.Forms;

namespace ISO11820.App;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // ===== 1. 初始化 Serilog 文件日志 =====
        var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(logDir, "iso11820-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Log.Information("=== ISO11820 应用启动 ===");

            // ===== 2. 加载配置 =====
            var config = new AppConfig();
            Log.Information("配置加载完成，数据库路径: {Path}", config.SqlitePath);

            // ===== 3. 初始化数据库 + 建表 + 初始数据 =====
            var dbHelper = new DbHelper(config.FullSqlitePath);
            dbHelper.InitializeDatabase();
            Log.Information("数据库初始化完成");

            // ===== 4. 组装全局上下文 =====
            AppContext.Instance.Config = config;
            AppContext.Instance.Db = dbHelper;

            // ===== 5. 创建核心业务对象 =====
            AppContext.Instance.Controller = new TestController(config, dbHelper);
            AppContext.Instance.DaqWorker = new DaqWorker(config, dbHelper, AppContext.Instance.Controller);
            Log.Information("核心业务对象初始化完成");

            // ===== 6. 启动 WinForms 应用 =====
            ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "应用启动失败");
            MessageBox.Show($"应用启动失败:\n{ex.Message}", "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Log.Information("=== ISO11820 应用关闭 ===");
            Log.CloseAndFlush();
        }
    }
}
