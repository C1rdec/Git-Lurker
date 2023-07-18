using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GitLurker.Core.Models;
using GitLurker.UI.Services;

namespace GitLurker.UI.ViewModels
{
    internal class CommitActionViewModel : PropertyChangedBase, IItemListViewModel
    {
        private Repository _repository;
        private ObservableCollection<FileViewModel> _fileViewModels;
        private FileViewModel _selectedFileViewModel;
        private bool _mouseOver;
        private string _message;

        public CommitActionViewModel(Repository repo)
        {
            _repository = repo;
            _fileViewModels = new ObservableCollection<FileViewModel>();
        }

        public FileViewModel SelectedFileViewModel
        {
            get
            {
                return _selectedFileViewModel;
            }

            private set
            {
                _selectedFileViewModel = value;
                NotifyOfPropertyChange(() => HasSelectedFile);
            }
        }

        public bool HasSelectedFile => SelectedFileViewModel != null || _mouseOver;

        public ObservableCollection<FileViewModel> FileViewModels => _fileViewModels;

        public void OnMouseEnter()
        {
            _mouseOver = true;
            NotifyOfPropertyChange(() => HasSelectedFile);
        }

        public void OnMouseLeave()
        {
            _mouseOver = false;
            NotifyOfPropertyChange(() => HasSelectedFile);
        }

        public async Task<bool> Open(bool skipModifier)
        {
            if (SelectedFileViewModel != null)
            {
                SelectedFileViewModel.Open();
                SelectedFileViewModel = null;

                return true;
            }

            if (!string.IsNullOrEmpty(_message))
            {
                IoC.Get<ConsoleService>().Listen(_repository);
                await _repository.SyncAsync(_message);
            }

            return false;
        }

        public void Search(string term)
        {
            _message = term;
        }

        public void Clear()
        {
            Execute.OnUIThread(() => _fileViewModels.Clear());
        }

        public bool Close()
        {
            return true;
        }

        public void EnterLongPressed()
        {
        }

        public void MoveDown()
        {
            if (SelectedFileViewModel == null)
            {
                SelectedFileViewModel = _fileViewModels.FirstOrDefault();
                if (SelectedFileViewModel != null)
                {
                    SelectedFileViewModel.IsSelected = true;
                }

                return;
            }

            SelectedFileViewModel = _fileViewModels.FirstOrDefault(g => g.Id == SelectedFileViewModel.Id);

            var index = _fileViewModels.IndexOf(SelectedFileViewModel);
            if (index == -1 || (index + 1) >= _fileViewModels.Count)
            {
                return;
            }

            index++;
            SelectedFileViewModel.IsSelected = false;
            SelectedFileViewModel = _fileViewModels.ElementAt(index);
            SelectedFileViewModel.IsSelected = true;
        }

        public void MoveUp()
        {
            if (SelectedFileViewModel == null)
            {
                return;
            }

            SelectedFileViewModel = _fileViewModels.FirstOrDefault(g => g.Id == SelectedFileViewModel.Id);
            var index = _fileViewModels.IndexOf(SelectedFileViewModel);
            if (index <= 0)
            {
                return;
            }

            index--;
            SelectedFileViewModel.IsSelected = false;
            SelectedFileViewModel = _fileViewModels.ElementAt(index);
            SelectedFileViewModel.IsSelected = true;
        }

        public void NextTabPressed()
        {
        }

        public Task RefreshItems()
        {
            return Task.CompletedTask;
        }

        public void ShowRecent()
        {
            foreach (var file in _repository.GetFilesChanged().OrderBy(f => Path.GetFileName(f)))
            {
                _fileViewModels.Add(new FileViewModel(file, _repository));

            }
        }
    }
}
