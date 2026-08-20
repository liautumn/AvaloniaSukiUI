using System;
using System.Collections.Generic;
using AvaloniaSukiUI.Models;
using AvaloniaSukiUI.ViewModels.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;

namespace AvaloniaSukiUI.ViewModels.Dialogs;

/// <summary>
/// 新增和修改共用的表单 ViewModel。
/// </summary>
public partial class StudentEditorDialogViewModel : ViewModelBase
{
    private readonly ISukiDialogManager _dialogManager;
    private readonly ISukiDialog _dialog;
    private readonly Func<string, bool> _isStudentNumberDuplicated;
    private readonly Action<Student> _onSaved;

    public StudentEditorDialogViewModel(
        ISukiDialogManager dialogManager,
        ISukiDialog dialog,
        Student? student,
        Func<string, bool> isStudentNumberDuplicated,
        Action<Student> onSaved)
    {
        _dialogManager = dialogManager;
        _dialog = dialog;
        _isStudentNumberDuplicated = isStudentNumberDuplicated;
        _onSaved = onSaved;

        DialogTitle = student is null ? "新增学生" : "修改学生";
        StudentNumber = student?.StudentNumber ?? string.Empty;
        Name = student?.Name ?? string.Empty;
        Gender = student?.Gender ?? GenderOptions[0];
        Age = student?.Age ?? 18;
        Grade = student?.Grade ?? string.Empty;
        ClassName = student?.ClassName ?? string.Empty;
    }

    public string DialogTitle { get; }
    public IReadOnlyList<string> GenderOptions { get; } = ["男", "女"];
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    private string studentNumber = string.Empty;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string gender = string.Empty;

    [ObservableProperty]
    private decimal age = 18;

    [ObservableProperty]
    private string grade = string.Empty;

    [ObservableProperty]
    private string className = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string errorMessage = string.Empty;

    [RelayCommand]
    private void Save()
    {
        // ViewModel 统一完成清洗和校验，View 只显示绑定结果。
        var number = StudentNumber.Trim();
        var name = Name.Trim();
        var gender = Gender.Trim();
        var grade = Grade.Trim();
        var className = ClassName.Trim();

        ErrorMessage = Validate(number, name, gender, grade, className);
        if (HasError)
            return;

        var student = new Student(
            number,
            name,
            gender,
            decimal.ToInt32(Age),
            grade,
            className);

        // 通过回调把表单结果交还列表 ViewModel，再关闭当前对话框。
        _onSaved(student);
        _dialogManager.TryDismissDialog(_dialog);
    }

    [RelayCommand]
    private void Cancel() => _dialogManager.TryDismissDialog(_dialog);

    private string Validate(
        string number,
        string name,
        string gender,
        string grade,
        string className)
    {
        if (string.IsNullOrWhiteSpace(number))
            return "请输入学号。";
        if (_isStudentNumberDuplicated(number))
            return "该学号已存在，请更换后再保存。";
        if (string.IsNullOrWhiteSpace(name))
            return "请输入姓名。";
        if (string.IsNullOrWhiteSpace(gender))
            return "请选择性别。";
        if (Age is < 6 or > 100 || Age != decimal.Truncate(Age))
            return "年龄必须是 6 到 100 之间的整数。";
        if (string.IsNullOrWhiteSpace(grade))
            return "请输入年级。";
        if (string.IsNullOrWhiteSpace(className))
            return "请输入班级。";

        return string.Empty;
    }
}
