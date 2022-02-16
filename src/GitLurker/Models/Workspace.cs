namespace GitLurker.Models
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

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
            foreach (var folder in Directory.GetDirectories(folderPath, ".git", SearchOption.AllDirectories))
            {
                var parentFolderInformation = Directory.GetParent(folder);
                var path = parentFolderInformation.ToString();

                // Check if the folder is a git repository.
                if (Repository.IsValid(path))
                {
                    this._repositories.Add(new Repository(path));
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

        #region Methods

        public IEnumerable<Repository> Search(string term)
        {
            var startWith = Repositories.Where(r => r.Name.ToUpper().StartsWith(term.ToUpper()));
            var contain = Repositories.Where(r => r.Name.ToUpper().Contains(term.ToUpper())).ToList();
            contain.InsertRange(0, startWith);
            return contain.Distinct();
        }

        #endregion
    }
}
