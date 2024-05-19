namespace GitLurker.Core.Services;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitLurker.Core.Models;

public class RepositoryService
{
    #region Fields

    private IEnumerable<Workspace> _workspaces;

    #endregion

    #region Constructors

    public RepositoryService()
    {
        _workspaces = new List<Workspace>();
        GetWorkspaces();
    }

    #endregion

    public IEnumerable<Workspace> Workspaces => _workspaces;

    #region Methods

    public IEnumerable<Repository> Search(string term)
    {
        var allRepos = GetAllRepo();

        var startWith = allRepos.Where(r => r.Name.StartsWith(term, System.StringComparison.CurrentCultureIgnoreCase));
        var contain = allRepos.Where(r => r.Name.Contains(term, System.StringComparison.CurrentCultureIgnoreCase)).ToList();
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

    public Repository GetRepo(string name)
    {
        return GetAllRepo().FirstOrDefault(r => r.Name == name);
    }

    public IEnumerable<Workspace> GetWorkspaces()
    {
        var settingsFile = new SettingsFile();
        settingsFile.Initialize();

        _workspaces = settingsFile.Entity.Workspaces.Where(Directory.Exists).Select(w => new Workspace(w)).ToArray();
        return _workspaces;
    }

    public Repository GetRepository(string folderPath)
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
