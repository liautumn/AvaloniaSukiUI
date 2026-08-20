using Avalonia.Styling;
using AvaloniaSukiUI.ViewModels.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI;
using SukiUI.Enums;

namespace AvaloniaSukiUI.ViewModels.Dialogs;

public partial class ThemeSettingsViewModel : ViewModelBase
{
    private readonly SukiTheme _theme = SukiTheme.GetInstance();

    public ThemeSettingsViewModel()
    {
        IsLightTheme = _theme.ActiveBaseTheme == ThemeVariant.Light;
        SelectedColor = _theme.ThemeColor;
    }

    public string Title => "外观设置";
    public bool IsDarkTheme => !IsLightTheme;
    public bool IsBlueTheme => SelectedColor == SukiColor.Blue;
    public bool IsGreenTheme => SelectedColor == SukiColor.Green;
    public bool IsRedTheme => SelectedColor == SukiColor.Red;
    public bool IsOrangeTheme => SelectedColor == SukiColor.Orange;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDarkTheme))]
    private bool isLightTheme;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBlueTheme))]
    [NotifyPropertyChangedFor(nameof(IsGreenTheme))]
    [NotifyPropertyChangedFor(nameof(IsRedTheme))]
    [NotifyPropertyChangedFor(nameof(IsOrangeTheme))]
    private SukiColor selectedColor;

    [RelayCommand]
    private void SelectLightTheme()
    {
        _theme.ChangeBaseTheme(ThemeVariant.Light);
        IsLightTheme = true;
        OnPropertyChanged(nameof(IsLightTheme));
        OnPropertyChanged(nameof(IsDarkTheme));
    }

    [RelayCommand]
    private void SelectDarkTheme()
    {
        _theme.ChangeBaseTheme(ThemeVariant.Dark);
        IsLightTheme = false;
        OnPropertyChanged(nameof(IsLightTheme));
        OnPropertyChanged(nameof(IsDarkTheme));
    }

    [RelayCommand]
    private void SelectColorTheme(SukiColor color)
    {
        _theme.ChangeColorTheme(color);
        SelectedColor = color;
        OnPropertyChanged(nameof(IsBlueTheme));
        OnPropertyChanged(nameof(IsGreenTheme));
        OnPropertyChanged(nameof(IsRedTheme));
        OnPropertyChanged(nameof(IsOrangeTheme));
    }

}
