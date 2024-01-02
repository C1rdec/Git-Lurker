namespace GitLurker.UI.ViewModels;

using System.Threading.Tasks;

public interface IItemListViewModel
{
    Task RefreshItems();

    void Search(string term);

    bool Close();

    void Clear();

    void ShowRecent();

    void MoveUp();

    void MoveDown();

    Task<bool> Open(bool skipModifier);

    void EnterLongPressed();

    void NextTabPressed();
}
