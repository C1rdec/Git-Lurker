namespace GitLurker.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using GitLurker.Extensions;
    using GitLurker.Services;
    using WindowsInput;
    using WindowsInput.Native;

    public class Repository : NugetService
    {
        #region Fields

        private static readonly string OpenVsCodeCommand = "code .";
        private static readonly InputSimulator InputSimulator = new InputSimulator();
        private string _name;
        private FileInfo[] _slnFiles;
        private string _folder;
        private bool _duplicate;
        private Configuration _configuration;
        private GitConfigurationService _gitConfigurationService;

        #endregion

        #region Constructors

        public Repository(string folder, IEnumerable<Repository> existingRepos)
            : base(folder)
        {
            _folder = folder;
            _slnFiles = GetFiles(".sln");
            _configuration = GetConfiguration(folder);
            _gitConfigurationService = new GitConfigurationService(folder);

            SetName();

            var repos = existingRepos.Where(r => r.Name == _name);
            _duplicate = repos.Any();

            foreach(var repo in repos)
            {
                repo.SetDuplicate();
            }
        }

        #endregion

        #region Properties

        public string Name => _name;

        public bool HasIcon => _configuration != null && !string.IsNullOrEmpty(_configuration.IconPath);

        public string IconPath => _configuration == null ? string.Empty : Path.Join(_folder, _configuration.IconPath);

        public string Folder => _folder;

        public bool Duplicate => _duplicate;

        public bool HasFrontEnd => _configuration != null && !string.IsNullOrEmpty(_configuration.FrontEndPath);

        #endregion

        #region Methods

        public static bool IsValid(string folder)
        {
            if (Path.GetFileName(folder).StartsWith("."))
            {
                return false;
            }

            return true;
        }

        public void SetDuplicate()
        {
            _duplicate = true;
        }

        public Task PullAsync() => ExecuteCommandAsync("git pull", true);

        public void Open() => Open(false);

        public void Open(bool skipModifier)
        {
            AddToRecent();

            if (!skipModifier && HandleModifiers())
            {
                return;
            }

            if (HandleSln())
            {
                return;
            }

            if (HandleVsCode())
            {
                return;
            }
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

        private string AzurePullRequestUrl(string repoUrl)
        {
            return $"{repoUrl}/pullrequestcreate?sourceRef={GetCurrentBranchName()}";
        }

        private string GithubPullRequestUrl(string repoUrl)
        {
            repoUrl = repoUrl.Replace(".git", "");
            return $"{repoUrl}/compare/{GetCurrentBranchName()}?expand=1";
        }

        public Task OpenFrontEnd()
        {
            if (_configuration == null || string.IsNullOrEmpty(_configuration.FrontEndPath))
            {
                return Task.CompletedTask;
            }

            AddToRecent();
            var path = Path.Combine(_folder, _configuration.FrontEndPath);
            return ExecuteCommandAsync(OpenVsCodeCommand, path);
        }

        public string GetCurrentBranchName() => _gitConfigurationService.GetCurrentBranchName();

        public async Task<NugetInformation> GetNewNugetAsync(string nugetSource)
        {
            NugetInformation newNuget = null;
            var existingNugets = await ExecuteCommandAsync($@"{NugetListCommand} -source {nugetSource} -prerelease");

            // To get out of the UI Thread
            await Task.Run(() => 
            {
                var filePaths = GetFiles($"*.nupkg").Where(n => n.FullName.Contains("\\bin\\"));
                if (filePaths.Any())
                {
                    var nugetsInRepo = filePaths.Select(p => NugetInformation.Parse(p.FullName));

                    nugetsInRepo.GroupBy(n => n.PackageName).OrderBy(g => g.Count()).First();

                    var list = nugetsInRepo.ToList();
                    list.Sort((n1, n2) => n2.CompareTo(n1));
                    newNuget = list.FirstOrDefault();
                }
            });

            if (newNuget != null)
            {
                // If the package is installed
                if (existingNugets.Contains(newNuget.Name))
                {
                    return null;
                }
            }

            return newNuget;
        }

        private FileInfo[] GetFiles(string extention) => new DirectoryInfo(_folder).GetFiles($"*{extention}", SearchOption.AllDirectories);

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

            if (_slnFiles.Count() == 1)
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

        private void AddToRecent()
        {
            var settings = new SettingsFile();
            settings.Initialize();
            settings.AddToRecent(_folder);
        }

        private bool HandleSln()
        {
            // TODO: Handle multiple sln files
            var slnFile = _slnFiles.FirstOrDefault();
            if (slnFile == null)
            {
                return false;
            }

            var process = GetActiveSlnProcess(slnFile);
            if (process != null)
            {
                Native.SetForegroundWindow(process.MainWindowHandle);
                InputSimulator.Keyboard.KeyPress(VirtualKeyCode.LMENU);
            }
            else
            {
                new Process()
                {
                    StartInfo = new ProcessStartInfo(slnFile.FullName)
                    {
                        UseShellExecute = true,
                    }
                }.Start();
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
                    Process.Start("explorer.exe", _folder);
                    return true;
                }

                // Alt
                if (Native.IsKeyPressed(Native.VirtualKeyStates.VK_MENU))
                {
                    var repoUrl = GetRepoUrl();
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

        private bool HandleVsCode()
        {
            var directoryInformation = new DirectoryInfo(_folder);

            // TODO: Parse file to check project type
            var packageFile = directoryInformation.GetFiles("package.json").FirstOrDefault();

            // Flutter
            var pubspecFile = directoryInformation.GetFiles("pubspec.yaml").FirstOrDefault();
            if (packageFile != null || pubspecFile != null)
            {
                _ = ExecuteCommandAsync(OpenVsCodeCommand);
                return true;
            }

            return false;
        }

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
                $"{solutionName} - Microsoft Visual Studio",
                $"{solutionName} (Running) - Microsoft Visual Studio",
                $"{solutionName} (Debugging) - Microsoft Visual Studio",
            };

            foreach(var title in expectedTitles)
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
        {
            var configFilePath = Path.Join(_folder, ".git", "config");
            if (!File.Exists(configFilePath))
            {
                return string.Empty;
            }

            var text = File.ReadAllText(configFilePath);
            var url = text.GetLineAfter("url = ");
            if (!string.IsNullOrEmpty(url) && url.StartsWith("git@ssh"))
            {
                try
                {
                    url = FormatSshUrl(url);
                }
                catch
                {
                }
            }

            if (url.Contains("dev.azure.com"))
            {
                var uri = new Uri(url);
                url = $"{uri.Scheme}://{uri.Authority}{uri.AbsolutePath}";
            }

            return url;
        }

        private string FormatSshUrl(string url)
        {
            url = url.Replace("git@ssh.", string.Empty);
            var segments = url.Split('/');
            var firstSegment = segments.First();
            var index = firstSegment.IndexOf(":v");
            var domainName = firstSegment.Substring(0, index);
            var newSegments = new List<string>();
            newSegments.Add($"https://{domainName}");
            newSegments.AddRange(segments.Skip(1).Take(segments.Length - 2));
            newSegments.Add("_git");
            newSegments.Add(segments.Last());
            return string.Join("/", newSegments);
        }

        #endregion
    }
}
