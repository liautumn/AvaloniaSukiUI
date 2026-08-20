using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaSukiUI.ViewModels;
using AvaloniaSukiUI.Views;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace AvaloniaSukiUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Toast 与 Dialog 共用全局管理器，由主窗体中的 Host 负责显示。
            var toastManager = new SukiToastManager();
            var dialogManager = new SukiDialogManager();

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(toastManager, dialogManager),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
