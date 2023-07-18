using GitLurker.UI.ViewModels;

namespace GitLurker.UI.Messages
{
    public class ActionMessage
    {
        public string WaterMark { get; set; }

        public IItemListViewModel ListViewModel { get; set; }
    }
}
