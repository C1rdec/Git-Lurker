namespace GitLurker.Core.Models
{
    public class FileChange
    {
        public string FilePath { get; set; }

        public ChangeStatus Status { get; set; }
    }
}
