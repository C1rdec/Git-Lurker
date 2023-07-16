using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GitLurker.Core.Models;

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

        public FileViewModel SelectedGameViewModel
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

        public bool HasSelectedFile => SelectedGameViewModel != null || _mouseOver;

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
            await _repository.SyncAsync(_message);

            return false;
        }

        public void Search(string term)
        {
            _message = term;
        }

        public void Clear()
        {
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
        }

        public void MoveUp()
        {
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
                _fileViewModels.Add(new FileViewModel(file));
            }
        }
    }
}
