using System.Diagnostics;
using Caliburn.Micro;
using GitLurker.Core.Models;
using Lurker.Patreon;

namespace GitLurker.UI.ViewModels;

public class PatreonViewModel : Screen
{
    #region Fields

    private string _patreonId;
    private bool _isPledged;
    private PatreonFile _patreonFile;

    #endregion

    public PatreonViewModel()
    {
        _patreonFile = new PatreonFile();
        _patreonFile.Initialize();
    }

    #region Properties

    public string PatreonId
    {
        get => _patreonId;
        set
        {
            _patreonId = value;
            NotifyOfPropertyChange();
        }
    }

    public bool IsPledged
    {
        get => _isPledged;
        set
        {
            _isPledged = value;
            NotifyOfPropertyChange();
        }
    }

    #endregion

    #region Methods

    public async void Login()
    {
        using var service = CreateService();
        var tokenResult = await service.GetAccessTokenAsync();

        _patreonFile.Save(PatreonToken.FromTokenResult(tokenResult));

        PatreonId = await service.GetPatronId(tokenResult.AccessToken);
        IsPledged = await service.IsPledging("3779584", tokenResult.AccessToken);
    }

    public void Pledge()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "https://www.patreon.com/poelurker",
            UseShellExecute = true
        };
        Process.Start(psi);
    }

    private PatreonService CreateService()
        => new PatreonService(new int[] { 8080, 8181, 8282 }, "uI0ZqaEsUckHlpQdOgnJfGtA9tjdKy4A9IpfJj9M2ZIMRkZrRZSemBJ2DtNxbPJm");

    #endregion
}
