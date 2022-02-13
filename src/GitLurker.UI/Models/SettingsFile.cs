namespace GitLurker.UI.Models
{
    public class SettingsFile : AppDataFileManager.AppDataFileBase<Settings>
    {
        protected override string FileName => "Workspaces.json";

        protected override string FolderName => "GitLurker";
    }
}
