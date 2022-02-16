namespace GitLurker.Models
{
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    public class Repository
    {
        #region Fields

        private string _name;
        private FileInfo[] _slnFiles;
        private string _folder;

        #endregion

        #region Constructors

        public Repository(string folder)
        {
            this._folder = folder;
            this._name = Path.GetFileName(folder);
            this._slnFiles = new DirectoryInfo(folder).GetFiles("*.sln", SearchOption.AllDirectories);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name => this._name;

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns><c>true</c> if the specified folder is valid; otherwise, <c>false</c>.</returns>
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
            if (Native.IsKeyPressed(Native.VirtualKeyStates.VK_CONTROL))
            {
                // {wt}Windows Terminal, {nt}New Tab {d}Destination
                ExecuteCommand($"wt nt -d \"{_folder}\"");
                return;
            }

            // TODO: Handle multiple sln files
            var slnFile = _slnFiles.FirstOrDefault();
            if (slnFile != null)
            {
                new Process()
                {
                    StartInfo = new ProcessStartInfo(slnFile.FullName)
                    {
                        UseShellExecute = true,
                    }
                }.Start();
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

        #endregion
    }
}
