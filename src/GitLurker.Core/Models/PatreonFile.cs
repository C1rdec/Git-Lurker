using Lurker.AppData;

namespace GitLurker.Core.Models;

public class PatreonFile : AppDataFileBase<PatreonToken>
{
    protected override string FileName => "Patreon.json";

    protected override string FolderName => "GitLurker";
}
