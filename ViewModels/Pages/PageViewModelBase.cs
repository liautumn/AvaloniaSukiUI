using AvaloniaSukiUI.ViewModels.Common;
using Material.Icons;

namespace AvaloniaSukiUI.ViewModels.Pages;

public abstract class PageViewModelBase(
    string title,
    string description,
    MaterialIconKind icon) : ViewModelBase
{
    public string Title { get; } = title;
    public string Description { get; } = description;
    public MaterialIconKind Icon { get; } = icon;
}
