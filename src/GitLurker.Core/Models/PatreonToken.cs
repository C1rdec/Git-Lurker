using System;
using Lurker.Patreon;

namespace GitLurker.Core.Models;

public class PatreonToken
{
    #region Constructors

    public PatreonToken()
    {
    }

    #endregion

    #region Properties

    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }

    public DateTime ExpiredDate { get; set; }

    #endregion

    #region Methods

    public static PatreonToken FromTokenResult(TokenResult result)
    {
        var buffer = TimeSpan.FromSeconds(result.ExpiresIn).Add(TimeSpan.FromDays(-5));

        return new PatreonToken
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            ExpiredDate = DateTime.UtcNow.Add(buffer),
        };
    }

    #endregion
}
