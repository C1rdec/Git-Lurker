namespace GitLurker.UI.Messages;

using GitLurker.UI.ViewModels;

public class ActionMessage
{
    public string WaterMark { get; set; }

    public IItemListViewModel ListViewModel { get; set; }

    public bool Focus { get; set; }
}
