using System;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Ports.Security
{
    /// <summary>
    /// Contract the game's logic uses to sign the access tokens of the players and to check the ones the
    /// clients present, without knowing the algorithm or the key behind them.
    /// </summary>
    public interface IAccessTokenSigner
    {
        /// <summary>
        /// Signs a new access token for a player.
        /// </summary>
        /// <param name="playerId">Player the token identifies.</param>
        /// <param name="issuedAt">Moment the token is issued, in UTC.</param>
        /// <param name="expiresAt">Moment the token stops being valid, in UTC.</param>
        /// <returns>The signed token.</returns>
        AccessToken Sign(int playerId, DateTime issuedAt, DateTime expiresAt);

        /// <summary>
        /// Checks the signature and the expiry of a token and reads the player it identifies.
        /// </summary>
        /// <param name="token">Encoded token, as the client presented it.</param>
        /// <param name="moment">Moment of the check, in UTC.</param>
        /// <returns>The identifier of the player.</returns>
        /// <exception cref="InvalidAccessTokenException">Thrown when the token is malformed, altered or expired.</exception>
        int ReadPlayerId(string token, DateTime moment);
    }
}
