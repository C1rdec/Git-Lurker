using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitLurker.Models;

namespace GitLurker.Services
{
    public class RepositoryService
    {
        #region Fields

        private IEnumerable<Workspace> _workspaces;

        #endregion

        #region Constructors

        public RepositoryService()
        {
            _workspaces = new List<Workspace>();
        }

        #endregion

        #region Methods

        public IEnumerable<Repository> Search(string term)
        {
            var allRepos = GetAllRepo();

            var startWith = allRepos.Where(r => r.Name.ToUpper().StartsWith(term.ToUpper()));
            var contain = allRepos.Where(r => r.Name.ToUpper().Contains(term.ToUpper())).ToList();
            contain.InsertRange(0, startWith);

            return contain.Distinct().Take(5);
        }

        public IEnumerable<Repository> GetAllRepo()
        {
            var allRepos = new List<Repository>();
            foreach (var workspace in _workspaces)
            {
                allRepos.AddRange(workspace.Repositories);
            }

            return allRepos;
        }

        public IEnumerable<Workspace> GetWorkspaces()
        {
            var settingsFile = new SettingsFile();
            settingsFile.Initialize();

            _workspaces = settingsFile.Entity.Workspaces.Select(w => new Workspace(w)).ToArray();
            return _workspaces; 
        }

        public Repository GetReposiotry(string folderPath)
        {
            foreach (var workspace in _workspaces)
            {
                var repo = workspace.GetRepo(folderPath);
                if (repo != null)
                {
                    return repo;
                }
            }

            return null;
        }

        #endregion
    }
}
