using Caliburn.Micro;

namespace GitLurker.UI.ViewModels
{
    public abstract class ItemViewModelBase : PropertyChangedBase
    {
        #region Fields

        private bool _isSelected;

        #endregion

        #region Properties

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion
    }
}
