namespace GitLurker.UI.ViewModels
{
    public class RepoViewModel
    {
        #region Fields

        private string _folder;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RepoViewModel"/> class.
        /// </summary>
        /// <param name="folder">The folder.</param>
        public RepoViewModel(string folder)
        {
            this._folder = folder;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the icon path.
        /// </summary>
        /// <value>The icon path.</value>
        public string IconSource { get; set; }

        #endregion
    }
}
