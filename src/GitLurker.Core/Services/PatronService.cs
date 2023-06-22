using System.Threading.Tasks;
using GitLurker.Core.Models;
using Lurker.Patreon;

namespace GitLurker.Core.Services
{
    public class PatronService
    {
        #region Fields

        private static readonly string CampaignId = "3779584";
        private PatreonFile _patreonFile;
        private bool _isPledged;
        private string _patreonId;

        #endregion

        #region Constructors

        public PatronService()
        {
            _patreonFile = new PatreonFile();
            _patreonFile.Initialize();
        }

        #endregion

        #region Properties

        public string PatreonId => _patreonId;

        public bool IsPledged => _isPledged;

        #endregion

        #region Methods

        public void LogOut()
        {
            _patreonFile.Delete();
        }

        public async Task LoginAsync()
        {
            using var service = CreateService();
            var tokenResult = await service.GetAccessTokenAsync();

            _patreonFile.Save(PatreonToken.FromTokenResult(tokenResult));

            _isPledged = await service.IsPledging(CampaignId, tokenResult.AccessToken);
            _patreonId = await service.GetPatronId(tokenResult.AccessToken);
        }

        public async Task<bool> CheckPledgeStatusAsync()
        {
            if (string.IsNullOrEmpty(_patreonFile.Entity.AccessToken))
            {
                return false;
            }

            using var service = CreateService();
            _isPledged = await service.IsPledging(CampaignId, _patreonFile.Entity.AccessToken);
            _patreonId = await service.GetPatronId(_patreonFile.Entity.AccessToken);

            return _isPledged;
        }

        private PatreonService CreateService()
            => new(new int[] { 8080, 8181, 8282 }, "uI0ZqaEsUckHlpQdOgnJfGtA9tjdKy4A9IpfJj9M2ZIMRkZrRZSemBJ2DtNxbPJm");

        #endregion
    }
}
