using System.Collections.ObjectModel;
using Caliburn.Micro;
using GitLurker.Models;

namespace GitLurker.UI.ViewModels
{
    public class ConsoleViewModel : PropertyChangedBase
    {
        #region Fields

        private Repository _repository;

        #endregion

        #region Constructors

        public ConsoleViewModel()
        {
            Lines = new ObservableCollection<string>();
        }

        #endregion

        #region Properties

        public ObservableCollection<string> Lines { get; set; }

        #endregion

        #region Methods

        public void Initialize(Repository repository)
        {
            if (_repository != null)
            {
                Lines.Clear();
                _repository.NewProcessMessage -= Repository_NewProcessMessage;
            }

            _repository = repository;
            _repository.NewProcessMessage += Repository_NewProcessMessage;
        }

        private void Repository_NewProcessMessage(object sender, string e) => Execute.OnUIThread(() => Lines.Add(e));

        #endregion
    }
}
