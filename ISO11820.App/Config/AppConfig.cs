using Microsoft.Extensions.Configuration;

namespace ISO11820.App.Config;

/// <summary>
/// 应用程序配置，从 appsettings.json 读取
/// </summary>
public class AppConfig
{
    private readonly IConfiguration _config;

    public AppConfig()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        _config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    // ===== 数据库 =====
    public string DatabaseProvider => _config["Database:Provider"] ?? "Sqlite";
    public string SqlitePath => _config["Database:SqlitePath"] ?? "Data\\ISO11820.db";

    public string FullSqlitePath => Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, SqlitePath);

    // ===== 硬件 =====
    public int ConstPower => int.TryParse(_config["Hardware:ConstPower"], out var v) ? v : 2048;
    public int PidTemperature => int.TryParse(_config["Hardware:PidTemperature"], out var v) ? v : 750;
    public string SensorProtocol => _config["Hardware:SensorProtocol"] ?? "ModbusRtu";

    // ===== 仿真参数 =====
    public bool EnableSimulation => bool.TryParse(_config["Simulation:EnableSimulation"], out var v1) && v1;
    public bool SimulateSensors => bool.TryParse(_config["Simulation:SimulateSensors"], out var v2) && v2;
    public bool SimulatePidController => bool.TryParse(_config["Simulation:SimulatePidController"], out var v3) && v3;
    public double InitialFurnaceTemp => double.TryParse(_config["Simulation:InitialFurnaceTemp"], out var v4) ? v4 : 720.0;
    public double TargetFurnaceTemp => double.TryParse(_config["Simulation:TargetFurnaceTemp"], out var v5) ? v5 : 750.0;
    public double HeatingRatePerSecond => double.TryParse(_config["Simulation:HeatingRatePerSecond"], out var v6) ? v6 : 40.0;
    public double TempFluctuation => double.TryParse(_config["Simulation:TempFluctuation"], out var v7) ? v7 : 0.5;
    public double StableThreshold => double.TryParse(_config["Simulation:StableThreshold"], out var v8) ? v8 : 3.0;
    public bool SimulateFlame => bool.TryParse(_config["Simulation:SimulateFlame"], out var v9) && v9;
    public double MaxTemperatureDriftPerTenMinutes =>
        double.TryParse(_config["Simulation:MaxTemperatureDriftPerTenMinutes"], out var v10) ? v10 : 2.0;
    public int UpdateIntervalMs => int.TryParse(_config["Simulation:UpdateIntervalMs"], out var v11) ? v11 : 800;

    // ===== 文件存储 =====
    public string BaseDirectory => _config["FileStorage:BaseDirectory"] ?? "D:\\ISO11820";
    public string TestDataDirectory => _config["FileStorage:TestDataDirectory"] ?? "D:\\ISO11820\\TestData";

    // ===== 报告 =====
    public string OutputDirectory => _config["Report:OutputDirectory"] ?? "D:\\ISO11820\\Reports";
    public bool EnablePdfExport => bool.TryParse(_config["Report:EnablePdfExport"], out var v12) && v12;
}
