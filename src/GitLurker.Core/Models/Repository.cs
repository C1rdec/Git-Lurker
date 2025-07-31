namespace GitLurker.Core.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using GitLurker.Core.Extensions;
using GitLurker.Core.Services;
using LibGit2Sharp;
using SetStartupProjects;
using WindowsInput;
using WindowsInput.Native;

public class Repository : NugetService
{
    #region Fields

    private static readonly string OpenVsCodeCommand = "code .";
    private static readonly InputSimulator InputSimulator = new();
    private string _name;
    private FileInfo[] _slnFiles;
    private string _folder;
    private bool _duplicate;
    private Configuration _configuration;
    private GitService _gitService;
    private CancellationTokenSource _tokenSource;

    #endregion

    #region Constructors

    public Repository(string folder, IEnumerable<Repository> existingRepos)
        : base(folder)
    {
        _folder = folder;
        _slnFiles = GetFiles(".sln");
        _configuration = GetConfiguration(folder);
        _gitService = new GitService(folder);

        SetName();

        var repos = existingRepos.Where(r => r.Name == _name);
        _duplicate = repos.Any();

        foreach (var repo in repos)
        {
            repo.SetDuplicate();
        }
    }

    #endregion

    #region Properties

    public bool IsRunning => _tokenSource != null;

    public string Name => _name;

    public bool HasSln => _slnFiles.Any();

    public bool HasIcon => _configuration != null && !string.IsNullOrEmpty(_configuration.IconPath);

    public string IconPath => _configuration == null ? string.Empty : Path.Join(_folder, _configuration.IconPath);

    public string Folder => _folder;

    public bool Duplicate => _duplicate;

    public bool HasFrontEnd => _configuration != null && !string.IsNullOrEmpty(_configuration.FrontEndPath);

    public bool Exist => Directory.Exists(_folder);

    #endregion

    #region Methods

    public static bool IsValid(string folder)
        => !Path.GetFileName(folder).StartsWith('.');

    public async Task StartDefaultProject()
    {
        if (_tokenSource != null)
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;

            return;
        }

        var defaultProject = GetDefaultProject();
        if (defaultProject == null)
        {
            return;
        }

        _tokenSource = new CancellationTokenSource();
        AddToRecent();

        _ = OpenProject(defaultProject);
        await ExecuteCommandAsync($"dotnet run -c Debug --project {defaultProject.RelativePath}", false, _folder, _tokenSource.Token);

        _tokenSource?.Dispose();
        _tokenSource = null;
    }

    private Project GetDefaultProject()
    {
        var solutionFile = _slnFiles.FirstOrDefault();
        if (solutionFile == null || !File.Exists(solutionFile.FullName))
        {
            return null;
        }

        var startupProjectGuid = new StartProjectFinder().GetStartProjects(solutionFile.FullName).FirstOrDefault();
        if (startupProjectGuid == null)
        {
            return null;
        }

        return SolutionProjectExtractor.GetAllProjectFiles(solutionFile.FullName).FirstOrDefault(p => p.Guid == startupProjectGuid);
    }

    public bool IsBehind()
        => _gitService.IsBehind();

    public void SetDuplicate()
    {
        _duplicate = true;
    }

    public Task<ExecutionResult> PullAsync() => ExecuteCommandAsync("git pull", true);

    public async Task<ExecutionResult> SyncAsync(string message)
    {
        AddToRecent();

        var result = await ExecuteCommandAsync("git add -A -- .");
        if (result.ExitCode < 0)
        {
            return result;
        }

        result = await ExecuteCommandAsync($"git commit -m \"{message}\"");
        if (result.ExitCode < 0)
        {
            return result;
        }

        var currentBranch = GetCurrentBranchName();
        var branchExist = _gitService.GetRemoteBranches().Any(b => b.FriendlyName.Replace("origin/", string.Empty) == currentBranch);
        if (branchExist)
        {
            return await ExecuteCommandAsync($"git push", true);
        }
        else
        {
            return await ExecuteCommandAsync($"git push --set-upstream origin {currentBranch}", true);
        }
    }

    public Task RebaseAsync(string branchName) => ExecuteCommandAsync($"git rebase {branchName}", true);

    public Task MergeAsync(string branchName) => ExecuteCommandAsync($"git merge {branchName}", true);

    public Task CleanBranches() => ExecuteCommandAsync("git fetch origin --prune");

    public void Open(bool skipModifier)
    {
        if (!Directory.Exists(_folder))
        {
            return;
        }

        AddToRecent();

        if (!skipModifier && HandleModifiers())
        {
            return;
        }

        if (HandleSln())
        {
            return;
        }

        OpenVsCode();
    }

    public void OpenPullRequest()
    {
        var repoUrl = GetRepoUrl();
        if (string.IsNullOrEmpty(repoUrl))
        {
            return;
        }

        string pullRequestUrl = default;

        if (repoUrl.Contains("dev.azure.com") || repoUrl.Contains("visualstudio.com"))
        {
            pullRequestUrl = AzurePullRequestUrl(repoUrl);
        }
        else if (repoUrl.Contains("github.com"))
        {
            pullRequestUrl = GithubPullRequestUrl(repoUrl);
        }

        if (string.IsNullOrEmpty(pullRequestUrl))
        {
            return;
        }

        var psi = new ProcessStartInfo
        {
            FileName = pullRequestUrl,
            UseShellExecute = true
        };
        Process.Start(psi);
    }

    public Task CancelOperationAsync() => HandleOperation("abort");

    public Task ContinueOperationAsync() => HandleOperation("continue");

    public void OpenUserSecret(string secretId)
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var folderPath = Path.Combine(appdata, "Microsoft", "UserSecrets", secretId);

        if (!Directory.Exists(folderPath))
        {
            SetExitCode(-1);

            return;
        }

        var filePath = Path.Combine(folderPath, "secrets.json");
        if (!File.Exists(filePath))
        {
            SetExitCode(-1);

            return;
        }

        OpenFile(filePath);
        SetExitCode(-1);
    }

    public Task CheckoutLastBranchAsync() => ExecuteCommandAsync($"git checkout -", true);

    private async Task OpenProject(Project project)
    {
        var folder = Path.GetDirectoryName(project.FullPath);

        var launchSettingsPath = Path.Combine(folder, "Properties", "launchSettings.json");
        if (!File.Exists(launchSettingsPath))
        {
            return;
        }

        var text = File.ReadAllText(launchSettingsPath);
        var document = JsonDocument.Parse(text);

        var profiles = document.RootElement.GetProperty("profiles");

        foreach (var profile in profiles.EnumerateObject())
        {
            if (profile.Value.TryGetProperty("applicationUrl", out var applicationUrl))
            {
                var urls = applicationUrl.GetString().Split(";");
                var url = urls.FirstOrDefault(u => u.StartsWith("https"));

                if (profile.Value.TryGetProperty("launchUrl", out var launchUrl))
                {
                    url = $"{url}/{launchUrl.GetString()}";
                }

                await ExecuteCommandAsync($"start {url}");

                return;
            }
        }

        return;
    }

    private async Task HandleOperation(string operation)
    {
        var currentOperation = _gitService.GetCurrentOperation();
        if (currentOperation == CurrentOperation.None)
        {
            return;
        }

        if (currentOperation == CurrentOperation.Rebase)
        {
            await ExecuteCommandAsync($"git -c core.editor=true rebase --{operation}", true);
        }

        if (currentOperation == CurrentOperation.Merge)
        {
            await ExecuteCommandAsync($"git -c core.editor=true merge --{operation}", true);
        }
    }

    private string AzurePullRequestUrl(string repoUrl)
    {
        return $"{repoUrl}/pullrequestcreate?sourceRef={GetCurrentBranchName()}";
    }

    private string GithubPullRequestUrl(string repoUrl)
    {
        repoUrl = repoUrl.Replace(".git", "");
        return $"{repoUrl}/compare/{GetCurrentBranchName()}?expand=1";
    }

    public Task<ExecutionResult> OpenFrontEnd()
    {
        if (_configuration == null || string.IsNullOrEmpty(_configuration.FrontEndPath))
        {
            return Task.FromResult<ExecutionResult>(null);
        }

        var path = Path.Combine(_folder, _configuration.FrontEndPath);

        return ExecuteCommandAsync(OpenVsCodeCommand, true, path, CancellationToken.None);
    }

    public string GetCurrentBranchName() => _gitService.GetCurrentBranchName();

    public IEnumerable<string> GetBranchNames() => _gitService.GetBranchNames();

    public IEnumerable<FileChange> GetFilesChanged() => _gitService.GetFilesChanged();

    public IEnumerable<Stash> GetStashes() => _gitService.GetStashes();

    public void Pop() => _gitService.Pop();

    public void Stash() => _gitService.Stash();

    public string GetUserSecretId()
    {
        var defaultProject = GetDefaultProject();
        if (defaultProject == null)
        {
            return null;
        }

        var fileContent = File.ReadAllText(defaultProject.FullPath);
        try
        {
            var xml = XDocument.Parse(fileContent);
            var userSecretId = xml.Root.Descendants("UserSecretsId").FirstOrDefault();
            if (userSecretId != null)
            {
                return userSecretId.Value;
            }
        }
        catch
        {
        }

        return null;
    }

    public void AddToRecent()
    {
        var settings = new SettingsFile();
        settings.Initialize();
        settings.AddRecent(_folder);
    }

    public void Fetch()
        => _gitService.Fetch();

    public bool HasOperationInProgress() => _gitService.HasOperationInProgress();


    private static Configuration GetConfiguration(string folder)
    {
        var configPath = Path.Join(folder, ".gitlurker");
        if (!File.Exists(configPath))
        {
            return null;
        }

        var text = File.ReadAllText(configPath);
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Configuration>(text);
        }
        catch
        {
            return null;
        }
    }

    private void SetName()
    {
        if (_configuration != null && !string.IsNullOrEmpty(_configuration.Name))
        {
            _name = _configuration.Name;
            return;
        }

        if (_slnFiles.Length == 1)
        {
            var sln = _slnFiles.FirstOrDefault();
            var name = Path.GetFileNameWithoutExtension(sln.Name);
            var segments = name.Split('.');

            // To escape big solution name 
            if (segments.Length < 3)
            {
                _name = name;
                return;
            }
        }

        _name = Path.GetFileName(_folder);
    }

    private bool HandleSln()
    {
        // TODO: Handle multiple sln files
        var slnFile = _slnFiles.FirstOrDefault();
        if (slnFile == null || !File.Exists(slnFile.FullName))
        {
            return false;
        }

        var process = GetActiveSlnProcess(slnFile);
        if (process != null)
        {
            _ = Native.SetForegroundWindow(process.MainWindowHandle);
            InputSimulator.Keyboard.KeyPress(VirtualKeyCode.LMENU);
        }
        else
        {
            OpenFile(slnFile.FullName);
        }

        return true;
    }

    private bool HandleModifiers()
    {
        // Control
        if (Native.IsKeyPressed(Native.VirtualKeyStates.VK_CONTROL))
        {
            // Shift
            if (Native.IsKeyPressed(Native.VirtualKeyStates.VK_SHIFT))
            {
                _ = ExecuteCommandAsync($"start {_folder}");
                return true;
            }

            // Alt
            if (Native.IsKeyPressed(Native.VirtualKeyStates.VK_MENU))
            {
                var repoUrl = GetRepoUrl(handleWorkItem: true);
                if (string.IsNullOrEmpty(repoUrl))
                {
                    return true;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = repoUrl,
                    UseShellExecute = true
                };
                Process.Start(psi);
                return true;
            }

            // {wt}Windows Terminal, {nt}New Tab {d}Destination
            _ = ExecuteCommandAsync($"wt -w 0 nt -d \"{_folder}\"");
            return true;
        }

        return false;
    }

    private void OpenVsCode()
        => ExecuteCommandAsync(OpenVsCodeCommand);

    private Process GetActiveSlnProcess(FileInfo slnFile)
    {
        if (slnFile == null)
        {
            return null;
        }

        var runningVS = Process.GetProcessesByName("devenv");
        var solutionName = Path.GetFileName(slnFile.FullName).Replace(".sln", string.Empty);
        var expectedTitles = new[]
        {
            $"{solutionName} -",
            $"{solutionName} (Running) - Microsoft Visual Studio",
            $"{solutionName} (Debugging) - Microsoft Visual Studio",
        };

        foreach (var title in expectedTitles)
        {
            var process = runningVS.FirstOrDefault(p => p.MainWindowTitle.StartsWith(title));
            if (process != null)
            {
                return process;
            }
        }

        return null;
    }

    private string GetRepoUrl()
        => GetRepoUrl(false);

    private string GetRepoUrl(bool handleWorkItem)
    {
        var configFilePath = Path.Join(_folder, ".git", "config");
        if (!File.Exists(configFilePath))
        {
            return string.Empty;
        }

        var text = File.ReadAllText(configFilePath);
        var url = text.GetLineAfter("url = ");
        if (!string.IsNullOrEmpty(url) && url.StartsWith("git@ssh") || url.Contains("@vs-ssh.visualstudio.com") || url.StartsWith("git@"))
        {
            try
            {
                url = FormatSshUrl(url);
            }
            catch
            {
            }
        }

        if (url.Contains("dev.azure.com") || url.Contains("visualstudio.com"))
        {
            var uri = new Uri(url);
            var absolutePath = uri.AbsolutePath;

            if (handleWorkItem)
            {
                var branchName = GetCurrentBranchName();
                if (branchName != null)
                {
                    var segments = branchName.Split("/");
                    var lastSegment = segments.Last();

                    if (lastSegment.All(char.IsDigit))
                    {
                        var index = absolutePath.IndexOf("_git");
                        absolutePath = $"{absolutePath.Substring(0, index)}_workitems/edit/{lastSegment}/";
                    }
                }
            }

            url = $"{uri.Scheme}://{uri.Authority}{absolutePath}";
        }

        return url;
    }

    private string FormatSshUrl(string url)
    {
        var vsSsh = "vs-ssh.";
        var vsSshIndex = url.IndexOf(vsSsh);
        var isAzureDevops = vsSshIndex >= 0;
        if (isAzureDevops)
        {
            url = url.Substring(vsSshIndex + vsSsh.Length);
        }
        else
        {
            url = url.Replace("git@ssh.", string.Empty);
            url = url.Replace("git@", string.Empty);
        }

        var segments = url.Split('/');
        var firstSegment = segments.First();
        var index = firstSegment.IndexOf(":v");
        var domainName = firstSegment;
        if (index >= 0)
        {
            domainName = firstSegment.Substring(0, index);
        }
        else
        {
            domainName = domainName.Replace(":", "/");
        }

        domainName = domainName.Replace("visualstudio.com", "dev.azure.com");

        var newSegments = new List<string>
        {
            $"https://{domainName}"
        };
        newSegments.AddRange(segments.Skip(1).Take(segments.Length - 2));
        if (isAzureDevops)
        {
            newSegments.Add("_git");
        }

        newSegments.Add(segments.Last());

        return string.Join("/", newSegments);
    }

    #endregion
}
