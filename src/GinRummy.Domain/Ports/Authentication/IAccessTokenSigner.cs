using System;

using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Domain.Ports.Authentication
{
    public interface IAccessTokenSigner
    {
        AccessToken Sign(AccessTokenOwner owner, DateTime issuedAt, DateTime expiresAt);

        AccessTokenOwner ReadAccessTokenOwner(string token, DateTime moment);
    }
}
