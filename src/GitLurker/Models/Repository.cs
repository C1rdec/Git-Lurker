namespace GitLurker.Models
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using GitLurker.Extensions;

    public class Repository
    {
        #region Fields

        private string _name;
        private FileInfo[] _slnFiles;
        private string _folder;
        private Configuration _configuration;

        #endregion

        #region Constructors

        public Repository(string folder)
        {
            _folder = folder;
            _name = Path.GetFileName(folder);
            _slnFiles = new DirectoryInfo(folder).GetFiles("*.sln", SearchOption.AllDirectories);
            _configuration = GetConfiguration(folder);
        }

        #endregion

        #region Properties

        public string Name => _name;

        public bool HasIcon => _configuration != null && !string.IsNullOrEmpty(_configuration.IconPath);

        public string IconPath => _configuration == null ? string.Empty : Path.Join(_folder, _configuration.IconPath);

        public string Folder => _folder;

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

        /// <summary>
        /// Opens this instance.
        /// </summary>
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

        private static Configuration GetConfiguration(string folder)
        {
            var configPath = Path.Join(folder, "gitlurker");
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

        private void AddToRecent()
        {
            var settings = new SettingsFile();
            settings.Initialize();
            settings.AddToRecent(_folder);
        }

        private void ExecuteCommand(string command)
        {
            new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    WorkingDirectory = this._folder,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true,
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                },
            }.Start();
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
                ExecuteCommand($"wt nt -d \"{_folder}\"");
                return true;
            }

            return false;
        }

        private bool HandleVsCode()
        {
            var directoryInformation = new DirectoryInfo(this._folder);

            // TODO: Parse file to check project type
            var packageFile = directoryInformation.GetFiles("package.json").FirstOrDefault();

            // Flutter
            var pubspecFile = directoryInformation.GetFiles("pubspec.yaml").FirstOrDefault();
            if (packageFile != null || pubspecFile != null)
            {
                ExecuteCommand("code .");
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
                // TODO: Handle Rider
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
