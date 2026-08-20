using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Controls;
using SukiUI.Dialogs;

namespace AvaloniaSukiUI.ViewModels;

public sealed partial class PersonEditorDialogViewModel : ViewModelBase
{
    private readonly ISukiDialog _dialog;
    private readonly Func<PersonEditorDialogViewModel, string?> _save;

    public PersonEditorDialogViewModel(
        ISukiDialog dialog,
        string title,
        string submitText,
        IReadOnlyList<string> teams,
        IReadOnlyList<string> statuses,
        PersonSample? person,
        Func<PersonEditorDialogViewModel, string?> save)
    {
        _dialog = dialog;
        _save = save;
        Title = title;
        SubmitText = submitText;
        Teams = teams;
        Statuses = statuses;
        Code = person?.Code ?? string.Empty;
        Name = person?.Name ?? string.Empty;
        SelectedTeam = person?.Team ?? teams[0];
        SelectedStatus = person?.Status ?? statuses[0];
        Notes = person?.Notes ?? string.Empty;
    }

    public string Title { get; }
    public string SubmitText { get; }
    public IReadOnlyList<string> Teams { get; }
    public IReadOnlyList<string> Statuses { get; }

    [ObservableProperty]
    private string code;

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string selectedTeam;

    [ObservableProperty]
    private string selectedStatus;

    [ObservableProperty]
    private string notes;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string validationMessage = string.Empty;

    public bool HasError => !string.IsNullOrWhiteSpace(ValidationMessage);

    [RelayCommand]
    private void Cancel() => _dialog.Dismiss();

    [RelayCommand]
    private void Save()
    {
        ValidationMessage = Validate();
        if (HasError)
            return;

        ValidationMessage = _save(this) ?? string.Empty;
        if (!HasError)
            _dialog.Dismiss();
    }

    public PersonSample ToPerson(int id) => new(
        id,
        Code.Trim(),
        Name.Trim(),
        SelectedTeam,
        SelectedStatus,
        Notes.Trim());

    private string Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return "请输入用户姓名。";
        if (string.IsNullOrWhiteSpace(Code))
            return "请输入用户编号。";
        if (!Teams.Contains(SelectedTeam))
            return "请选择所属团队。";
        if (!Statuses.Contains(SelectedStatus))
            return "请选择用户状态。";
        return string.Empty;
    }
}
