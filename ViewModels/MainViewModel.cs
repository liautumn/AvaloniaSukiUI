using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Avalonia.Controls.Notifications;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI;
using SukiUI.Dialogs;
using SukiUI.Enums;
using SukiUI.Toasts;

namespace AvaloniaSukiUI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly SukiTheme _theme;
    private readonly ISukiToastManager _toastManager;
    private readonly ISukiDialogManager _dialogManager;

    public ObservableCollection<DemoPageViewModel> DemoPages { get; } = [];

    [ObservableProperty]
    private DemoPageViewModel? activePage;

    [ObservableProperty]
    private bool isDarkTheme;

    [ObservableProperty]
    private ColorChoice selectedColor = null!;

    public IReadOnlyList<ColorChoice> ColorChoices { get; }

    public ISukiToastManager ToastManager => _toastManager;
    public ISukiDialogManager DialogManager => _dialogManager;
    public SukiTheme Theme => _theme;

    public MainViewModel(ISukiToastManager toastManager, ISukiDialogManager dialogManager)
    {
        _toastManager = toastManager;
        _dialogManager = dialogManager;
        _theme = SukiTheme.GetInstance();
        IsDarkTheme = _theme.ActiveBaseTheme == ThemeVariant.Dark;
        ColorChoices =
        [
            CreateColorChoice("海蓝", SukiColor.Blue),
            CreateColorChoice("森林绿", SukiColor.Green),
            CreateColorChoice("珊瑚红", SukiColor.Red),
            CreateColorChoice("琥珀橙", SukiColor.Orange),
        ];

        DemoPages.Add(new OverviewPageViewModel(this));
        DemoPages.Add(new ButtonsPageViewModel(this));
        DemoPages.Add(new InputsPageViewModel(this));
        DemoPages.Add(new CollectionsPageViewModel(this));
        DemoPages.Add(new NavigationPageViewModel(this));
        DemoPages.Add(new ProgressPageViewModel(this));
        DemoPages.Add(new NotificationsPageViewModel(this));
        ActivePage = DemoPages[0];
        SelectedColor = ColorChoices[0];
    }

    // 明暗主题通过 SukiTheme 统一切换，所有 DynamicResource 会自动更新。
    [RelayCommand]
    private void ToggleBaseTheme()
    {
        _theme.SwitchBaseTheme();
        IsDarkTheme = _theme.ActiveBaseTheme == ThemeVariant.Dark;
    }

    [RelayCommand]
    private void ChangeColorTheme(ColorChoice? choice)
    {
        if (choice is not null)
            _theme.ChangeColorTheme(choice.Value);
    }

    partial void OnSelectedColorChanged(ColorChoice value) => ChangeColorTheme(value);

    private ColorChoice CreateColorChoice(string name, SukiColor value) =>
        new(name, value, new RelayCommand(() => _theme.ChangeColorTheme(value)));

    [RelayCommand]
    private void ShowInfoToast() => ShowToast(NotificationType.Information, "信息通知", "这是一个 SukiToast 信息示例。");

    [RelayCommand]
    private void ShowSuccessToast() => ShowToast(NotificationType.Success, "操作成功", "数据已保存，Toast 会自动消失。");

    [RelayCommand]
    private void ShowWarningToast() => ShowToast(NotificationType.Warning, "注意", "这是一个可点击关闭的警告通知。");

    [RelayCommand]
    private void ShowErrorToast() => ShowToast(NotificationType.Error, "发生错误", "请检查输入内容后重试。");

    private void ShowToast(NotificationType type, string title, string content)
    {
        _toastManager.CreateToast()
            .WithTitle(title)
            .WithContent(content)
            .OfType(type)
            .Dismiss().After(TimeSpan.FromSeconds(4))
            .Dismiss().ByClicking()
            .Queue();
    }

    [RelayCommand]
    private void OpenDialog()
    {
        // Dialog 内容可直接是字符串，也可以替换成自定义 ViewModel。
        _dialogManager.CreateDialog()
            .WithTitle("SukiDialog")
            .WithContent("这是一个由 SukiUI 管理的模态对话框。\n可以继续添加按钮、表单或自定义视图。")
            .OfType(NotificationType.Information)
            .WithActionButton("取消", _ => { }, true, "Basic")
            .WithActionButton("确认", _ => ShowSuccessToast(), true, "Accent")
            .Dismiss().ByClickingBackground()
            .TryShow();
    }
}

public sealed record ColorChoice(string Name, SukiColor Value, IRelayCommand ApplyCommand);
