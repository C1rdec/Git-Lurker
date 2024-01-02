namespace GitLurker.Core.Models;

using System.Collections.Generic;

public class CustomAction
{
    #region Constructors

    public CustomAction()
    {
        Repositories = [];
    }

    #endregion

    #region Properties

    public string Name { get; set; }

    public string Icon { get; set; }

    public string Command { get; set; }

    public List<string> Repositories { get; set; }

    public bool OpenConsole { get; set; }

    public bool ExcludeRepositories { get; set; }

    #endregion

    #region Methods

    public bool AddRepository(Repository repo)
    {
        if (Repositories.Contains(repo.Folder))
        {
            return false;
        }

        Repositories.Add(repo.Folder);
        return true;
    }

    public bool RemoveRepository(Repository repo)
    {
        return Repositories.Remove(repo.Folder);
    }

    public bool IsIncluded(string repoPath)
    {
        var contains = Repositories.Contains(repoPath);
        return ExcludeRepositories ? !contains : contains;
    }

    #endregion
}
