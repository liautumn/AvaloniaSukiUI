using System.Collections.Generic;
using AvaloniaSukiUI.ViewModels.Common;
using AvaloniaSukiUI.ViewModels.Dialogs;
using AvaloniaSukiUI.ViewModels.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;

namespace AvaloniaSukiUI.ViewModels.Shell;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        // 页面共用窗口级 DialogManager，业务页无需直接访问主窗口。
        Pages =
        [
            new HomePageViewModel(),
            new StudentPageViewModel(DialogManager),
        ];
        ActivePage = Pages[0];
    }

    public IReadOnlyList<PageViewModelBase> Pages { get; }
    public ISukiDialogManager DialogManager { get; } = new SukiDialogManager();

    [ObservableProperty]
    private PageViewModelBase? activePage;

    [RelayCommand]
    private void OpenThemeSettings()
    {
        DialogManager.CreateDialog()
            .ShowCardBackground(true)
            .WithViewModel(_ => new ThemeSettingsViewModel())
            .Dismiss().ByClickingBackground()
            .TryShow();
    }
}
