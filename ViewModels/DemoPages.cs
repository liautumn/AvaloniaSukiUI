using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Content;
using SukiUI.Dialogs;

namespace AvaloniaSukiUI.ViewModels;

public abstract class DemoPageViewModel : ViewModelBase
{
    protected DemoPageViewModel(MainViewModel shell, string title, string description, object icon)
    {
        Shell = shell;
        Title = title;
        Description = description;
        Icon = icon;
    }

    public MainViewModel Shell { get; }
    public string Title { get; }
    public string Description { get; }
    public object Icon { get; }
}

public sealed class OverviewPageViewModel(MainViewModel shell)
    : DemoPageViewModel(shell, "概览与主题", "颜色、明暗模式和 SukiUI 基础容器", Icons.Star)
{
    public string VersionText => "SukiUI 7.0.1 · Avalonia 12.1.1";
}

public sealed partial class ButtonsPageViewModel(MainViewModel shell)
    : DemoPageViewModel(shell, "按钮", "Button、DropDownButton、SplitButton 与 HyperlinkButton", Icons.Plus)
{
    [ObservableProperty]
    private bool isLoading;

    [RelayCommand]
    private async Task SimulateLoading()
    {
        IsLoading = true;
        await Task.Delay(1400);
        IsLoading = false;
        Shell.ShowSuccessToastCommand.Execute(null);
    }
}

public sealed partial class InputsPageViewModel(MainViewModel shell)
    : DemoPageViewModel(shell, "输入框与表单", "常用输入控件、选择控件和表单提交", Icons.Pencil)
{
    public IReadOnlyList<string> Countries { get; } = ["中国", "日本", "德国", "法国", "美国", "英国"];
    public IReadOnlyList<string> Roles { get; } = ["设计师", "工程师", "产品经理", "研究员"];

    [ObservableProperty]
    private string selectedCountry = "中国";

    [ObservableProperty]
    private double volume = 62;

    [ObservableProperty]
    private bool notificationsEnabled = true;

    [ObservableProperty]
    private bool advancedEnabled;

    [ObservableProperty]
    private string userName = "张三";

    [ObservableProperty]
    private string email = "zhangsan@example.com";

    [ObservableProperty]
    private string formStatus = "等待提交";

    [RelayCommand]
    private void SubmitForm()
    {
        FormStatus = string.IsNullOrWhiteSpace(UserName) || !Email.Contains('@')
            ? "请填写有效的姓名和邮箱"
            : $"已提交：{UserName} / {SelectedCountry}";
    }
}

public sealed partial class CollectionsPageViewModel
    : DemoPageViewModel
{
    private const string AllTeams = "全部团队";
    private const string AllStatuses = "全部状态";

    private readonly List<PersonSample> _allPeople = [];
    private int _nextId = 1;

    public CollectionsPageViewModel(MainViewModel shell)
        : base(shell, "数据表格与分页", "搜索、增删改、DataGrid 数据展示和分页", Icons.Menu)
    {
        SeedPeople();
        RefreshView();
    }

    public ObservableCollection<PersonSample> People { get; } = [];
    public ObservableCollection<PageOption> Pages { get; } = [];
    public IReadOnlyList<string> TeamOptions { get; } = ["工程", "产品", "设计", "研究"];
    public IReadOnlyList<string> StatusOptions { get; } = ["在线", "忙碌", "离开"];
    public IReadOnlyList<string> TeamFilters { get; } = [AllTeams, "工程", "产品", "设计", "研究"];
    public IReadOnlyList<string> StatusFilters { get; } = [AllStatuses, "在线", "忙碌", "离开"];
    public IReadOnlyList<int> PageSizeOptions { get; } = [6, 10, 20];

    [ObservableProperty]
    private string searchName = string.Empty;

    [ObservableProperty]
    private string selectedTeamFilter = AllTeams;

    [ObservableProperty]
    private string selectedStatusFilter = AllStatuses;

    [ObservableProperty]
    private int selectedPageSize = 6;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int totalCount;

    [ObservableProperty]
    private int totalPages = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    private PersonSample? selectedPerson;

    public bool HasSelection => SelectedPerson is not null;
    public bool CanGoPrevious => CurrentPage > 1;
    public bool CanGoNext => CurrentPage < TotalPages;

    [RelayCommand]
    private void Search() => MoveToFirstPageAndRefresh();

    [RelayCommand]
    private void ResetSearch()
    {
        SearchName = string.Empty;
        SelectedTeamFilter = AllTeams;
        SelectedStatusFilter = AllStatuses;
        MoveToFirstPageAndRefresh();
    }

    [RelayCommand]
    private void AddPerson()
    {
        Shell.DialogManager.CreateDialog()
            .ShowCardBackground(true)
            .WithViewModel(dialog => new PersonEditorDialogViewModel(
                dialog,
                "新增用户",
                "新增",
                TeamOptions,
                StatusOptions,
                null,
                CreatePerson))
            .TryShow();
    }

    [RelayCommand]
    private void EditPerson(PersonSample? person)
    {
        person ??= SelectedPerson;
        if (person is null)
            return;

        Shell.DialogManager.CreateDialog()
            .ShowCardBackground(true)
            .WithViewModel(dialog => new PersonEditorDialogViewModel(
                dialog,
                "修改用户",
                "保存",
                TeamOptions,
                StatusOptions,
                person,
                editor => UpdatePerson(person.Id, editor)))
            .TryShow();
    }

    [RelayCommand]
    private void DeletePerson(PersonSample? person)
    {
        person ??= SelectedPerson;
        if (person is null)
            return;

        Shell.DialogManager.CreateDialog()
            .WithTitle("删除用户")
            .WithContent($"确定删除“{person.Name}（{person.Code}）”吗？删除后无法恢复。")
            .OfType(NotificationType.Warning)
            .WithActionButton("取消", _ => { }, true, "Basic")
            .WithActionButton("删除", _ => ConfirmDelete(person), true, "Danger")
            .TryShow();
    }

    [RelayCommand]
    private void GoToPage(int page) => CurrentPage = Math.Clamp(page, 1, TotalPages);

    [RelayCommand]
    private void PreviousPage() => GoToPage(CurrentPage - 1);

    [RelayCommand]
    private void NextPage() => GoToPage(CurrentPage + 1);

    private PageOption CreatePage(int number) =>
        new(number, new RelayCommand(() => GoToPage(number)), number == CurrentPage);

    partial void OnCurrentPageChanged(int value)
    {
        RefreshView();
    }

    partial void OnSelectedPageSizeChanged(int value) => MoveToFirstPageAndRefresh();

    private string? CreatePerson(PersonEditorDialogViewModel editor)
    {
        if (CodeExists(editor.Code))
            return "用户编号已存在，请更换后重试。";

        _allPeople.Insert(0, editor.ToPerson(_nextId++));
        MoveToFirstPageAndRefresh();
        Shell.ShowSuccessToastCommand.Execute(null);
        return null;
    }

    private string? UpdatePerson(int id, PersonEditorDialogViewModel editor)
    {
        if (CodeExists(editor.Code, id))
            return "用户编号已存在，请更换后重试。";

        var index = _allPeople.FindIndex(person => person.Id == id);
        if (index < 0)
            return "没有找到要修改的用户。";

        _allPeople[index] = editor.ToPerson(id);
        RefreshView();
        Shell.ShowSuccessToastCommand.Execute(null);
        return null;
    }

    private bool CodeExists(string code, int? exceptId = null) =>
        _allPeople.Any(person =>
            person.Id != exceptId &&
            string.Equals(person.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));

    private void ConfirmDelete(PersonSample person)
    {
        _allPeople.RemoveAll(item => item.Id == person.Id);
        RefreshView();
        Shell.ShowSuccessToastCommand.Execute(null);
    }

    private void MoveToFirstPageAndRefresh()
    {
        if (CurrentPage == 1)
            RefreshView();
        else
            CurrentPage = 1;
    }

    private void RefreshView()
    {
        var filtered = _allPeople.Where(person =>
            (string.IsNullOrWhiteSpace(SearchName) ||
             person.Name.Contains(SearchName.Trim(), StringComparison.OrdinalIgnoreCase)) &&
            (SelectedTeamFilter == AllTeams || person.Team == SelectedTeamFilter) &&
            (SelectedStatusFilter == AllStatuses || person.Status == SelectedStatusFilter))
            .ToList();

        TotalCount = filtered.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)SelectedPageSize));

        var validPage = Math.Clamp(CurrentPage, 1, TotalPages);
        if (validPage != CurrentPage)
        {
            CurrentPage = validPage;
            return;
        }

        Pages.Clear();
        for (var page = 1; page <= TotalPages; page++)
            Pages.Add(CreatePage(page));

        People.Clear();
        foreach (var person in filtered.Skip((CurrentPage - 1) * SelectedPageSize).Take(SelectedPageSize))
            People.Add(person);

        SelectedPerson = null;
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(CanGoNext));
    }

    private void SeedPeople()
    {
        string[] names =
        [
            "张伟", "王芳", "李娜", "刘洋", "陈晨", "杨帆",
            "赵敏", "周杰", "吴桐", "郑凯", "孙悦", "何雨",
            "冯雪", "朱宁", "马超", "胡月", "林涛", "高晴",
        ];
        string[] notes = ["核心成员", "远程办公", "", "本周出差"];

        for (var index = 0; index < names.Length; index++)
        {
            var id = _nextId++;
            _allPeople.Add(new PersonSample(
                id,
                $"USR-{id:000}",
                names[index],
                TeamOptions[index % TeamOptions.Count],
                StatusOptions[index % StatusOptions.Count],
                notes[index % notes.Length]));
        }
    }
}

public sealed partial class NavigationPageViewModel(MainViewModel shell)
    : DemoPageViewModel(shell, "布局与导航", "TabControl、SplitView、Expander、GridSplitter 与菜单", Icons.ArrowRight)
{
    [ObservableProperty]
    private bool isPaneOpen;

    [ObservableProperty]
    private int selectedTab;

    [RelayCommand]
    private void TogglePane() => IsPaneOpen = !IsPaneOpen;
}

public sealed partial class ProgressPageViewModel(MainViewModel shell)
    : DemoPageViewModel(shell, "进度与加载", "ProgressBar、CircleProgressBar、WaveProgress、Stepper 与 Loading", Icons.Refresh)
{
    [ObservableProperty]
    private double progress = 68;

    [ObservableProperty]
    private bool isIndeterminate;

    [ObservableProperty]
    private int stepIndex = 1;

    public IReadOnlyList<string> Steps { get; } = ["准备", "处理中", "已完成"];

    [RelayCommand]
    private void ChangeStep(bool increase)
    {
        StepIndex = Math.Clamp(StepIndex + (increase ? 1 : -1), 0, Steps.Count - 1);
    }
}

public sealed partial class NotificationsPageViewModel(MainViewModel shell)
    : DemoPageViewModel(shell, "消息与弹窗", "InfoBar、Toast、Dialog 与 Flyout", Icons.CircleInformation)
{
    [ObservableProperty]
    private bool infoBarOpen = true;

    [ObservableProperty]
    private string infoMessage = "这条 InfoBar 可以关闭，也支持文本选择。";
}

public sealed record PersonSample(int Id, string Code, string Name, string Team, string Status, string Notes);

public sealed record PageOption(int Number, IRelayCommand SelectCommand, bool IsCurrent);
