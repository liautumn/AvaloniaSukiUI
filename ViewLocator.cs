using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaloniaSukiUI.ViewModels.Common;

namespace AvaloniaSukiUI;

/// <summary>
/// 按照 ViewModel 与 View 的同名约定自动定位视图。
/// </summary>
[RequiresUnreferencedCode("默认视图定位器使用反射，发布裁剪时需要保留对应视图类型。")]
public sealed class ViewLocator : IDataTemplate
{
    public Control? Build(object? parameter)
    {
        if (parameter is null)
            return null;

        var viewName = parameter.GetType().FullName!
            .Replace(".ViewModels.", ".Views.", StringComparison.Ordinal)
            .Replace("ViewModel", "View", StringComparison.Ordinal);
        var viewType = Type.GetType(viewName);

        return viewType is null
            ? new TextBlock { Text = $"未找到视图：{viewName}" }
            : (Control)Activator.CreateInstance(viewType)!;
    }

    public bool Match(object? data) => data is ViewModelBase;
}
