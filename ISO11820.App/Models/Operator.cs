namespace ISO11820.App.Models;

/// <summary>
/// 操作员/用户账号（对应 operators 表）
/// </summary>
public class Operator
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Pwd { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty; // "admin" 或 "operator"
}
