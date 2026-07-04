namespace ISO11820.App.Models;

/// <summary>
/// 试验设备信息（对应 apparatus 表）
/// </summary>
public class Apparatus
{
    public int ApparatusId { get; set; }
    public string InnerNumber { get; set; } = string.Empty;   // 设备内部编号，如 FURNACE-01
    public string ApparatusName { get; set; } = string.Empty; // 设备名称，如 一号试验炉
    public DateTime CheckDateF { get; set; }                   // 检定有效期开始
    public DateTime CheckDateT { get; set; }                   // 检定有效期结束
    public string PidPort { get; set; } = string.Empty;       // PID串口
    public string PowerPort { get; set; } = string.Empty;     // 功率串口
    public int? ConstPower { get; set; }                       // 恒功率值
}
