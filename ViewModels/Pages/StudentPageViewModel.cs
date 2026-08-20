using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using AvaloniaSukiUI.Models;
using AvaloniaSukiUI.ViewModels.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI.Dialogs;

namespace AvaloniaSukiUI.ViewModels.Pages;

/// <summary>
/// 学生 CRUD 页面状态。搜索、分页、选择和弹窗交互都由该 ViewModel 驱动。
/// </summary>
public partial class StudentPageViewModel : PageViewModelBase
{
    private readonly ISukiDialogManager _dialogManager;
    private readonly ObservableCollection<Student> _allStudents;
    private string _appliedStudentNumber = string.Empty;
    private string _appliedName = string.Empty;

    public StudentPageViewModel(ISukiDialogManager dialogManager)
        : base("学生管理", "学生信息 CRUD 示例", MaterialIconKind.AccountSchoolOutline)
    {
        _dialogManager = dialogManager;
        _allStudents = new ObservableCollection<Student>(CreateDemoStudents());

        // DataGridCollectionView 是 Avalonia DataGrid 提供的分页数据视图。
        // DataGrid 绑定该视图后，只会枚举当前页，不需要手动创建“当前页集合”。
        StudentsView = new DataGridCollectionView(_allStudents)
        {
            PageSize = int.Parse(PageSizeOptions[SelectedPageSizeIndex]),
            Filter = MatchesAppliedFilters,
        };
        StudentsView.PageChanged += (_, _) => SyncPaginationState();
        SyncPaginationState();
    }

    public DataGridCollectionView StudentsView { get; }
    public IReadOnlyList<string> PageSizeOptions { get; } = ["10", "20", "50"];
    public string PageSummary => $"共 {TotalCount} 条，第 {CurrentPage} / {TotalPages} 页";

    [ObservableProperty]
    private string studentNumberFilter = string.Empty;

    [ObservableProperty]
    private string nameFilter = string.Empty;

    [ObservableProperty]
    private Student? selectedStudent;

    [ObservableProperty]
    private int selectedPageSizeIndex;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    private int totalCount;

    partial void OnSelectedStudentChanged(Student? value)
    {
        EditSelectedCommand.NotifyCanExecuteChanged();
        DeleteSelectedCommand.NotifyCanExecuteChanged();
    }

    partial void OnSelectedPageSizeIndexChanged(int value)
    {
        if (value < 0 || value >= PageSizeOptions.Count ||
            !int.TryParse(PageSizeOptions[value], out var pageSize))
            return;

        // 页大小直接交给 DataGridCollectionView，它负责重新计算页面。
        StudentsView.PageSize = pageSize;
        StudentsView.MoveToFirstPage();
        SyncPaginationState();
    }

    partial void OnCurrentPageChanged(int value)
    {
        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(PageSummary));
    }

    partial void OnTotalPagesChanged(int value)
    {
        PreviousPageCommand.NotifyCanExecuteChanged();
        NextPageCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(PageSummary));
    }

    partial void OnTotalCountChanged(int value) => OnPropertyChanged(nameof(PageSummary));

    [RelayCommand]
    private void Search()
    {
        // 输入框和已应用条件分离，因此只有点击“搜索”才真正刷新结果。
        _appliedStudentNumber = StudentNumberFilter.Trim();
        _appliedName = NameFilter.Trim();
        RefreshStudentsView(moveToFirstPage: true);
    }

    [RelayCommand]
    private void Reset()
    {
        StudentNumberFilter = string.Empty;
        NameFilter = string.Empty;
        _appliedStudentNumber = string.Empty;
        _appliedName = string.Empty;
        RefreshStudentsView(moveToFirstPage: true);
    }

    [RelayCommand]
    private void AddStudent() => OpenEditor(null);

    [RelayCommand(CanExecute = nameof(HasSelectedStudent))]
    private void EditSelected() => OpenEditor(SelectedStudent);

    [RelayCommand]
    private void EditStudent(Student? student) => OpenEditor(student);

    [RelayCommand(CanExecute = nameof(HasSelectedStudent))]
    private Task DeleteSelectedAsync() => DeleteStudentAsync(SelectedStudent);

    [RelayCommand]
    private async Task DeleteStudentAsync(Student? student)
    {
        if (student is null)
            return;

        var confirmed = await _dialogManager.CreateDialog()
            .OfType(NotificationType.Warning)
            .WithTitle("删除学生")
            .WithContent($"确定删除学生“{student.Name}”（{student.StudentNumber}）吗？此操作无法撤销。")
            .WithYesNoResult("删除", "取消")
            .TryShowAsync();

        if (!confirmed)
            return;

        _allStudents.Remove(student);
        RefreshStudentsView(moveToFirstPage: false);
    }

    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private void PreviousPage()
    {
        StudentsView.MoveToPreviousPage();
        SyncPaginationState();
    }

    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private void NextPage()
    {
        StudentsView.MoveToNextPage();
        SyncPaginationState();
    }

    private bool HasSelectedStudent() => SelectedStudent is not null;
    private bool CanGoToPreviousPage() => CurrentPage > 1;
    private bool CanGoToNextPage() => CurrentPage < TotalPages;

    private void OpenEditor(Student? source)
    {
        _dialogManager.CreateDialog()
            .ShowCardBackground(true)
            .WithViewModel(dialog => new StudentEditorDialogViewModel(
                _dialogManager,
                dialog,
                source,
                number => IsStudentNumberDuplicated(number, source),
                savedStudent => SaveStudent(source, savedStudent)))
            .TryShow();
    }

    private bool IsStudentNumberDuplicated(string number, Student? source) =>
        _allStudents.Any(student =>
            !ReferenceEquals(student, source) &&
            string.Equals(student.StudentNumber, number, StringComparison.OrdinalIgnoreCase));

    private void SaveStudent(Student? source, Student savedStudent)
    {
        if (source is null)
        {
            _allStudents.Add(savedStudent);
        }
        else
        {
            var index = _allStudents.IndexOf(source);
            if (index >= 0)
                _allStudents[index] = savedStudent;
        }

        RefreshStudentsView(moveToFirstPage: false);
    }

    private bool MatchesAppliedFilters(object item)
    {
        if (item is not Student student)
            return false;

        return (string.IsNullOrEmpty(_appliedStudentNumber) ||
                student.StudentNumber.Contains(_appliedStudentNumber, StringComparison.OrdinalIgnoreCase)) &&
               (string.IsNullOrEmpty(_appliedName) ||
                student.Name.Contains(_appliedName, StringComparison.OrdinalIgnoreCase));
    }

    private void RefreshStudentsView(bool moveToFirstPage)
    {
        // Refresh 会重新应用 Filter，并由内置分页视图计算当前页内容。
        StudentsView.Refresh();
        if (moveToFirstPage)
            StudentsView.MoveToFirstPage();
        else if (StudentsView.PageIndex >= GetPageCount())
            StudentsView.MoveToLastPage();

        SelectedStudent = null;
        SyncPaginationState();
    }

    private void SyncPaginationState()
    {
        // DataGridCollectionView 使用从 0 开始的 PageIndex，界面显示时转换为从 1 开始。
        TotalCount = StudentsView.TotalItemCount;
        TotalPages = GetPageCount();
        CurrentPage = Math.Max(1, StudentsView.PageIndex + 1);
        OnPropertyChanged(nameof(PageSummary));
    }

    // PageCount 在当前 DataGrid 包中不是公开属性，因此用视图公开的总数和页大小计算展示值。
    private int GetPageCount() => Math.Max(1, (int)Math.Ceiling(StudentsView.TotalItemCount / (double)StudentsView.PageSize));

    private static IEnumerable<Student> CreateDemoStudents()
    {
        string[] names =
        [
            "张明", "李华", "王芳", "赵宇", "陈晨", "刘洋",
            "杨帆", "黄静", "周宁", "吴昊", "徐欣", "孙悦",
        ];

        for (var index = 0; index < 36; index++)
        {
            yield return new Student(
                $"2026{index + 1:0000}",
                names[index % names.Length],
                index % 2 == 0 ? "男" : "女",
                17 + index % 4,
                $"高{index % 3 + 1}",
                $"{index % 6 + 1}班");
        }
    }
}
