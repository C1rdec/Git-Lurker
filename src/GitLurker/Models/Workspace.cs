namespace GitLurker.Models
{
    using System;
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
            _repositories = new List<Repository>();
            var option = new EnumerationOptions()
            {
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.ReparsePoint,
                RecurseSubdirectories = true,
            };

            foreach (var folder in Directory.GetDirectories(folderPath, ".git", option))
            {
                var parentFolderInformation = Directory.GetParent(folder);
                var path = parentFolderInformation.ToString();

                var parentRepo = _repositories.FirstOrDefault(r => folder.StartsWith(r.Folder));
                if (parentRepo != null)
                {
                    var directoryName = Path.GetFileName(Path.GetDirectoryName(folder));
                    var parentDirectoryName = Path.GetFileName(parentRepo.Folder);
                    if (!directoryName.StartsWith(parentDirectoryName))
                    {
                        continue;
                    }
                }

                // Check if the folder is a git repository.
                if (Repository.IsValid(path))
                {
                    _repositories.Add(new Repository(path));
                }
            }
        }

        #endregion

        #region Properties

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

        public Repository GetRepo(string folderPath) => _repositories.FirstOrDefault(r => r.Folder == folderPath);

        private static IEnumerable<string> ShowAllFoldersUnder(string path, string searchTerm, int indent = 0)
        {
            var folders = new List<string>();
            try
            {
                if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                {
                    foreach (string folder in Directory.GetDirectories(path, searchTerm))
                    {
                        folders.Add(folder);
                        folders.AddRange(ShowAllFoldersUnder(folder, searchTerm, indent + 2));
                    }
                }
            }
            catch (UnauthorizedAccessException) 
            { }

            return folders;
        }

        #endregion
    }
}
