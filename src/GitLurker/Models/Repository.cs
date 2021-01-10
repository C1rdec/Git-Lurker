namespace GitLurker.Models
{
    using System.IO;

    public class Repository
    {
        #region Fields

        private string _name;
        private Configuration _configuration;
        private FileInfo[] _slnFiles;

        #endregion

        #region Constructors

        public Repository(string folder)
        {
            this._name = Path.GetFileName(folder);
            this._slnFiles = new DirectoryInfo(folder).GetFiles("*.sln", SearchOption.AllDirectories);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns><c>true</c> if the specified folder is valid; otherwise, <c>false</c>.</returns>
        public static bool IsValid(string folder)
        {
            if (Path.GetFileName(folder).StartsWith("."))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => this._name;

        #endregion
    }
}
