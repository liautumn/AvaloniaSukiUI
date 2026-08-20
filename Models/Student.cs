namespace AvaloniaSukiUI.Models;

/// <summary>
/// 学生领域模型。使用不可变记录，编辑时生成新对象，避免取消编辑后污染表格中的原数据。
/// </summary>
public sealed record Student(
    string StudentNumber,
    string Name,
    string Gender,
    int Age,
    string Grade,
    string ClassName);
