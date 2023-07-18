using System.IO;
using Caliburn.Micro;
using GitLurker.Core.Models;
using GitLurker.UI.Views;

namespace GitLurker.UI.ViewModels
{
    public class FileViewModel : ItemViewModelBase
    {
        #region Fields

        private string _filePath;
        private Repository _repo;

        #endregion

        #region Constructors

        public FileViewModel(string filePath, Repository repo)
        {
            _repo = repo;
            _filePath = filePath;
        }

        #endregion

        #region Properties

        public string FileName => Path.GetFileName(_filePath);

        public override string Id => _filePath;

        #endregion

        #region Methods

        public async void Open()
        {
            await _repo.ExecuteCommandAsync($"git difftool -x \"code --wait --diff\" -y -- \"{_filePath}\"");
        }

        public string Select()
        {
            IsSelected = true;
            Execute.OnUIThread(() => (View as FileView).MainBorder.BringIntoView());

            return Id;
        }

        #endregion
    }
}
