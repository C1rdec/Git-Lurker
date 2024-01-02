namespace GitLurker.Core.Models;

using Lurker.AppData;

public class PatreonFile : AppDataFileBase<PatreonToken>
{
    protected override string FileName => "Patreon.json";

    protected override string FolderName => "GitLurker";
}
