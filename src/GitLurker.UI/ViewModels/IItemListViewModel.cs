using System.Threading.Tasks;

namespace GitLurker.UI.ViewModels
{
    public interface IItemListViewModel
    {
        Task RefreshItems();

        void Search(string term);

        bool Close();

        void Clear();

        void ShowRecent();

        void MoveUp();

        void MoveDown();

        Task Open(bool skipModifier);

        void EnterLongPressed();

        void NextTabPressed();
    }
}
