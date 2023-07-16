using System.IO;

namespace GitLurker.UI.ViewModels
{
    public class FileViewModel : ItemViewModelBase
    {
        #region Fields

        private string _filePath;

        #endregion

        #region Constructors

        public FileViewModel(string filePath)
        {
            _filePath = filePath;
        }

        #endregion

        #region Properties

        public string FileName => Path.GetFileName(_filePath);

        #endregion

        #region Methods

        public void Open()
        {
        }

        #endregion
    }
}
