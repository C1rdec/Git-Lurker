namespace GitLurker.Models
{
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;

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

            if (Native.IsKeyPressed(Native.VirtualKeyStates.VK_CONTROL))
            {
                if (Native.IsKeyPressed(Native.VirtualKeyStates.VK_SHIFT))
                {
                    Process.Start("explorer.exe", _folder);
                    return;
                }

                // {wt}Windows Terminal, {nt}New Tab {d}Destination
                ExecuteCommand($"wt nt -d \"{_folder}\"");
                return;
            }

            if (HandleSln())
            {
                return;
            }

            var directoryInformation = new DirectoryInfo(this._folder);

            // TODO: Parse file to check project type
            var packageFile = directoryInformation.GetFiles("package.json").FirstOrDefault();

            // Flutter
            var pubspecFile = directoryInformation.GetFiles("pubspec.yaml").FirstOrDefault();
            if (packageFile != null || pubspecFile != null)
            {
                ExecuteCommand("code .");
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

        #endregion
    }
}
