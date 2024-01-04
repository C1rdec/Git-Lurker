namespace GitLurker.Core.Models;

using System.Collections.Generic;
using LibGit2Sharp;

public class Settings
{
    #region Constructors

    public Settings()
    {
        Snippets = [];
        Workspaces = [];
        RecentRepos = [];
        HotKey = new Hotkey()
        {
            Modifier = Winook.Modifiers.Control,
            KeyCode = Winook.KeyCode.G,
        };
        DevToysHotKey = new Hotkey()
        {
            Modifier = Winook.Modifiers.None,
            KeyCode = Winook.KeyCode.None,
        };

        StartWithWindows = true;
        AddToStartMenu = true;
        RebaseOperation = CurrentOperation.Rebase;
        Mode = Mode.Git;
    }

    #endregion

    #region Properties

    public List<Snippet> Snippets { get; set; }

    public List<string> Workspaces { get; set; }

    public List<string> RecentRepos { get; set; }

    public Hotkey HotKey { get; set; }

    public Hotkey DevToysHotKey { get; set; }

    public bool ConsoleOuput { get; set; }

    public bool StartWithWindows { get; set; }

    public bool AddToStartMenu { get; set; }

    public string NugetSource { get; set; }

    public bool IsAdmin { get; set; }

    public bool SteamEnabled { get; set; }

    public Scheme Scheme { get; set; }

    public CurrentOperation RebaseOperation { get; set; }

    public Mode Mode { get; set; }

    #endregion
}
