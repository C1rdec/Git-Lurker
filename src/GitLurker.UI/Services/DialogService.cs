namespace GitLurker.UI.Services;

using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;

public class DialogService
{
    #region Fields

    private static readonly IDialogCoordinator Coordinator = DialogCoordinator.Instance;

    private object _context;

    #endregion

    #region Methods

    public async Task ShowProgressAsync(string title, string message, Task task)
    {
        var controller = await Coordinator.ShowProgressAsync(_context, title, message);
        controller.SetIndeterminate();

        await task;

        await controller.CloseAsync();
    }

    public void Register(object context)
    {
        _context = context;
    }

    #endregion
}
