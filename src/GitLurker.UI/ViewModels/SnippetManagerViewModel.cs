namespace GitLurker.UI.ViewModels;

using System.Collections.ObjectModel;
using Caliburn.Micro;
using GitLurker.Core.Models;
using GitLurker.UI.Services;

public class SnippetManagerViewModel : PropertyChangedBase
{
    #region Fields

    private SettingsFile _file;

    #endregion

    #region Constructors

    public SnippetManagerViewModel()
    {
        _file = IoC.Get<SettingsFile>();
        _file.OnFileSaved += File_OnFileSaved;
        Snippets = new ObservableCollection<SnippetTileViewModel>();
        AddSnippets();
    }

    #endregion

    #region Properties

    public ObservableCollection<SnippetTileViewModel> Snippets { get; set; }

    #endregion

    public void Add()
    {
        var viewModel = new SnippetViewModel(new Snippet());
        IoC.Get<FlyoutService>().Show("New Snippet", viewModel, MahApps.Metro.Controls.Position.Right);
    }

    private void File_OnFileSaved(object sender, Settings e)
    {
        foreach (var snippet in Snippets)
        {
            snippet.PropertyChanged -= CustomActionManagerViewModel_PropertyChanged;
        }

        Snippets.Clear();
        AddSnippets();
    }

    private void AddSnippets()
    {
        foreach (var snippet in _file.Entity.Snippets)
        {
            var viewModel = new SnippetTileViewModel(snippet);
            viewModel.PropertyChanged += CustomActionManagerViewModel_PropertyChanged;
            Snippets.Add(viewModel);
        }
    }

    private void CustomActionManagerViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is not SnippetTileViewModel viewModel)
        {
            return;
        }

        if (e.PropertyName == "Deleted")
        {
            viewModel.PropertyChanged -= CustomActionManagerViewModel_PropertyChanged;
            Snippets.Remove(viewModel);
        }
    }
}
