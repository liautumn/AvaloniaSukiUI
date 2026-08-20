using System;
using Avalonia;

namespace AvaloniaSukiUI;

sealed class Program
{
    // Avalonia 启动前不要调用依赖 UI 或同步上下文的 API。
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // 此配置同时供应用启动和 XAML 设计器使用。
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
