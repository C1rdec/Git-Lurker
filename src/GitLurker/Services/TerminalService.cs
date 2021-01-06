namespace GitLurker.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json.Linq;

    public class TerminalService
    {
        #region Properties

        /// <summary>
        /// Gets the local application data.
        /// </summary>
        private string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        /// <summary>
        /// Gets the packages folder.
        /// </summary>
        private string PackagesFolder => Path.Join(LocalAppData, "Packages");

        /// <summary>
        /// Gets the name of the terminal folder.
        /// </summary>
        private string TerminalFolderName => "Microsoft.WindowsTerminal";

        #endregion

        #region Methods

        /// <summary>
        /// Adds the profile.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">
        /// Cannot find terminal folder
        /// or
        /// Cannot find terminal settings file
        /// </exception>
        public Guid AddProfile(string filePath)
        {
            var id = Guid.NewGuid();
            var directory = Path.GetDirectoryName(filePath);
            var termitJson = File.ReadAllText(filePath);
            var config = JObject.Parse(termitJson);
            config["startingDirectory"] = directory;
            config["guid"] = $"{{{id}}}";

            var folder = Directory.GetDirectories(this.PackagesFolder).FirstOrDefault(d => d.Contains(TerminalFolderName));
            if (folder == null)
            {
                throw new Exception("Cannot find terminal folder");
            }

            var settingsPath = Path.Join(folder, "LocalState", "settings.json");
            if (!File.Exists(settingsPath))
            {
                throw new Exception("Cannot find terminal settings file");
            }


            var settingsObject = JObject.Parse(File.ReadAllText(settingsPath));
            var profiles = (JObject)settingsObject["profiles"];
            var list = (JArray)profiles["list"];
            list.Add(config);

            File.WriteAllText(settingsPath, settingsObject.ToString());
            return id;
        }

        #endregion
    }
}
