namespace GitLurker.UI.ViewModels
{
    public class FolderViewModel
    {
        #region Fields

        private string _folder;

        #endregion

        #region Constructors

        public FolderViewModel(string folder)
        {
            _folder = folder;
        }

        #endregion

        #region Properties

        public string Folder => _folder;

        #endregion

        #region Methods

        public void Delete()
        {

        }

        #endregion
    }
}
