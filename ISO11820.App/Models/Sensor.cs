namespace ISO11820.App.Models;

/// <summary>
/// 传感器通道配置（对应 sensors 表）
/// </summary>
public class Sensor
{
    public int SensorId { get; set; }
    public string SensorName { get; set; } = string.Empty;   // 传感器代号，如 TF1
    public string DispName { get; set; } = string.Empty;     // 显示名，如 炉内温度1
    public string SensorGroup { get; set; } = string.Empty;  // 分组标识
    public string Unit { get; set; } = string.Empty;         // 单位，如 ℃
    public string Discription { get; set; } = string.Empty;  // 描述
    public string Flag { get; set; } = string.Empty;         // 标记
    public double SignalZero { get; set; }                    // 信号零点
    public double SignalSpan { get; set; }                    // 信号量程
    public double OutputZero { get; set; }                    // 输出温度下限
    public double OutputSpan { get; set; }                    // 输出温度上限
    public double OutputValue { get; set; }                   // 当前温度值
    public double InputValue { get; set; }                    // 当前输入值
    public int SignalType { get; set; }                       // 信号类型：4=数字量（仿真用）
}
