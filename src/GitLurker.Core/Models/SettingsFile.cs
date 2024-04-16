namespace GitLurker.Core.Models;

using System;
using System.Linq;
using Lurker.AppData;

public class SettingsFile : AppDataFileBase<Settings>
{
    #region Fields

    private static readonly int MaxRecentCount = 5;

    #endregion

    #region Properties

    protected override string FileName => "Workspaces.json";

    protected override string FolderName => "GitLurker";

    #endregion

    #region Methods

    public void AddSnippet(Snippet snippet)
        => Entity.Snippets.Add(snippet);

    public void RemoveSnippet(Snippet snippet)
    {
        var existingSnippet = Entity.Snippets.FirstOrDefault(s => s.Id == snippet.Id);
        if (existingSnippet != null)
        {
            Entity.Snippets.Remove(existingSnippet);
        }
    }

    public void AddRecent(string folder)
    {
        var recentRepos = Entity.RecentRepos;
        var index = recentRepos.IndexOf(folder);
        if (index == -1)
        {
            if (recentRepos.Count >= MaxRecentCount)
            {
                recentRepos.RemoveAt(MaxRecentCount - 1);
            }
        }
        else
        {
            recentRepos.RemoveAt(index);
        }

        Entity.RecentRepos.Insert(0, folder);
        Save();
    }

    public void RemoveRecent(string folder)
    {
        var recentRepos = Entity.RecentRepos;
        var index = recentRepos.IndexOf(folder);
        if (index == -1)
        {
            return;
        }

        recentRepos.RemoveAt(index);
        Save();
    }

    public void RemoveWorkspace(string folderPath)
    {
        var workspace = Entity.Workspaces.FirstOrDefault(w => w == folderPath);
        if (workspace == null)
        {
            return;
        }

        Entity.Workspaces.Remove(workspace);
        Save();
    }

    public bool HasNugetSource() => !string.IsNullOrEmpty(Entity.LocalNugetSource);

    public Mode GetNextMode()
    {
        var modes = Enum.GetValues(typeof(Mode)).Cast<Mode>().ToList();
        var currentIndex = modes.IndexOf(Entity.Mode);

        var turnOn = false;

        var nextMode = Mode.Git;
        while (!turnOn)
        {
            var nextIndex = currentIndex + 1;
            if (nextIndex >= modes.Count)
            {
                nextIndex = 0;
            }

            nextMode = modes[nextIndex];
            currentIndex = modes.IndexOf(nextMode);

            switch (nextMode)
            {
                case Mode.Audio:
                    turnOn = Entity.AudioEnabled;
                    break;
                case Mode.Game:
                    turnOn = Entity.SteamEnabled;
                    break;
                case Mode.Git:
                    turnOn = true;
                    break;
            }
        }

        return nextMode;
    }

    #endregion
}
