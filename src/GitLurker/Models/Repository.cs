namespace GitLurker.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Desktop.Robot;
    using GitLurker.Extensions;
    using GitLurker.Services;

    public class Repository
    {
        #region Fields

        private static readonly Lazy<Robot> MyRobot = new(() => new Robot());
        private string _name;
        private FileInfo[] _slnFiles;
        private string _folder;
        private bool _duplicate;
        private Configuration _configuration;
        private GitConfigurationService _gitConfigurationService;

        #endregion

        #region Constructors

        public Repository(string folder, IEnumerable<Repository> existingRepos)
        {
            _folder = folder;
            _slnFiles = new DirectoryInfo(folder).GetFiles("*.sln", SearchOption.AllDirectories);
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

        #region Events

        public event EventHandler<string> NewProcessMessage;

        #endregion

        #region Properties

        public string Name => _name;

        public bool HasIcon => _configuration != null && !string.IsNullOrEmpty(_configuration.IconPath);

        public string IconPath => _configuration == null ? string.Empty : Path.Join(_folder, _configuration.IconPath);

        public string Folder => _folder;

        public bool Duplicate => _duplicate;

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

        public void Open()
        {
            AddToRecent();

            if (HandleModifiers())
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

        public string GetCurrentBranchName() => _gitConfigurationService.GetCurrentBranchName();

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

        private Task ExecuteCommandAsync(string command) => ExecuteCommandAsync(command, false);

        private Task ExecuteCommandAsync(string command, bool listen)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = _folder,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = listen,
                    UseShellExecute = !listen,
                    RedirectStandardOutput = listen,
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                },
            };

            if (listen)
            {
                DataReceivedEventHandler handler = default;
                handler = (s, a) =>
                {
                    if (string.IsNullOrEmpty(a.Data))
                    {
                        process.OutputDataReceived -= handler;
                        return;
                    }

                    NewProcessMessage?.Invoke(this, a.Data);
                };

                process.OutputDataReceived += handler;
            }

            process.Start();

            if (listen)
            {
                process.BeginOutputReadLine();
                return process.WaitForExitAsync();
            }

            return Task.CompletedTask;
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
                MyRobot.Value.KeyPress(Key.Alt);
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
                _ = ExecuteCommandAsync("code .");
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
                    return FormatSshUrl(url);
                }
                catch
                {
                }
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
