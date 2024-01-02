namespace GitLurker.UI.ViewModels;

using System.IO;
using Caliburn.Micro;
using GitLurker.Core.Models;
using GitLurker.UI.Views;

public class FileViewModel : ItemViewModelBase
{
    #region Fields

    private FileChange _fileChange;
    private Repository _repo;

    #endregion

    #region Constructors

    public FileViewModel(FileChange fileChange, Repository repo)
    {
        _repo = repo;
        _fileChange = fileChange;
    }

    #endregion

    #region Properties

    public string FileName => Path.GetFileName(_fileChange.FilePath);

    public override string Id => _fileChange.FilePath;

    public string Letter => _fileChange.Status switch
    {
        ChangeStatus.Deleted => "D",
        ChangeStatus.Modified => "M",
        ChangeStatus.Added => "A",
        _ => string.Empty,
    };

    #endregion

    #region Methods

    public async void Open()
    {
        await _repo.ExecuteCommandAsync($"git difftool -x \"code --wait --diff\" -y -- \"{_fileChange.FilePath}\"");
    }

    public string Select()
    {
        IsSelected = true;
        Execute.OnUIThread(() => (View as FileView).MainBorder.BringIntoView());

        return Id;
    }

    #endregion
}
