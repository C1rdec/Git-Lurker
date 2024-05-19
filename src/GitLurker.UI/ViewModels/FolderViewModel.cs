namespace GitLurker.UI.ViewModels;

using System;
using System.IO;
using System.Windows.Media;

public class FolderViewModel
{
    #region Fields

    private string _folder;
    private Action<string> _deleteCallback;

    #endregion

    #region Constructors

    public FolderViewModel(string folder, Action<string> deleteCallback)
    {
        _folder = folder;
        _deleteCallback = deleteCallback;
    }

    #endregion

    #region Properties

    public string Folder => _folder;

    public Brush Foreground => Directory.Exists(Folder) ? Brushes.Black : Brushes.Red;

    #endregion

    #region Methods

    public void Delete() => _deleteCallback(_folder);

    #endregion
}
