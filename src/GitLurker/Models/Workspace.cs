namespace GitLurker.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using GitLurker.Services;

    public class Workspace : ProcessService
    {
        #region Fields

        private string _folder;
        private List<Repository> _repositories;

        #endregion

        #region Constructors

        public Workspace(string folderPath)
            : base (folderPath)
        {
            _folder = folderPath;
            _repositories = new List<Repository>();
            var option = new EnumerationOptions()
            {
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.ReparsePoint,
                RecurseSubdirectories = true,
            };

            foreach (var folder in Directory.GetDirectories(_folder, ".git", option))
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
                    _repositories.Add(new Repository(path, _repositories));
                }
            }
        }

        #endregion

        #region Properties

        public string Folder => _folder;

        public IEnumerable<Repository> Repositories => _repositories;

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

        public async Task<Repository> CloneAsync(Uri url)
        {
            await ExecuteCommandAsync($"git clone {url}", true);
            var lastSegment = url.LocalPath.Split("/").LastOrDefault();

            if (string.IsNullOrEmpty(lastSegment))
            {
                return null;
            }

            var folderName = lastSegment.Replace(".git", string.Empty);
            var folderPath = Path.Combine(_folder, folderName);

            if (!Directory.Exists(folderPath))
            {
                return null;
            }

            return new Repository(folderPath, _repositories);
        }

        public void AddRepo(Repository repo)
        {
            _repositories.Add(repo);
            repo.AddToRecent();
        }

        #endregion
    }
}
