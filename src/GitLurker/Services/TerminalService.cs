namespace GitLurker.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json.Linq;

    public class TerminalService
    {
        #region Properties

        private string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private string PackagesFolder => Path.Join(LocalAppData, "Packages");

        private string TerminalFolderName => "Microsoft.WindowsTerminal";

        #endregion

        #region Methods

        public Guid AddProfile(string filePath)
        {
            var id = Guid.NewGuid();
            var termitJson = File.ReadAllText(filePath);
            var config = JObject.Parse(termitJson);
            config["startingDirectory"] = Path.GetDirectoryName(filePath);
            config["guid"] = $"{{{id}}}";

            var folder = Directory.GetDirectories(PackagesFolder).FirstOrDefault(d => d.Contains(TerminalFolderName));
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
