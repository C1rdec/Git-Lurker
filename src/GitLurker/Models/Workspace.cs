namespace GitLurker.Models
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Class Workspace.
    /// </summary>
    public class Workspace
    {
        #region Fields

        private List<Repository> _repositories;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Workspace"/> class.
        /// </summary>
        /// <param name="folder">The folder.</param>
        public Workspace(string folderPath)
        {
            this._repositories = new List<Repository>();
            foreach (var folder in Directory.GetDirectories(folderPath))
            {
                // Check is the folder is a git repository.
                if (Repository.IsValid(folder))
                {
                    this._repositories.Add(new Repository(folder));
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the repositories.
        /// </summary>
        /// <value>The repositories.</value>
        public IEnumerable<Repository> Repositories => this._repositories;

        #endregion
    }
}
