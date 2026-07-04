namespace ISO11820.App.Models;

/// <summary>
/// 样品信息（对应 productmaster 表）
/// </summary>
public class ProductMaster
{
    public string ProductId { get; set; } = string.Empty;   // 样品编号（主键）
    public string ProductName { get; set; } = string.Empty; // 样品名称
    public string Specific { get; set; } = string.Empty;    // 规格型号
    public double Diameter { get; set; }                     // 直径（mm）
    public double Height { get; set; }                       // 高度（mm）
    public string? Flag { get; set; }                        // 备用字段
}
