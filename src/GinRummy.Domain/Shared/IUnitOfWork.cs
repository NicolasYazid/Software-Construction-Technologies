using System;

using GinRummy.Domain.Ports.AccountProtection;
using GinRummy.Domain.Ports.Authentication;
using GinRummy.Domain.Ports.DirectDuel;
using GinRummy.Domain.Ports.Friendship;
using GinRummy.Domain.Ports.Gameplay;
using GinRummy.Domain.Ports.PlayerCareer;
using GinRummy.Domain.Ports.PlayerModeration;
using GinRummy.Domain.Ports.PlayerPreferences;
using GinRummy.Domain.Ports.SocialCommunication;

namespace GinRummy.Domain.Shared
{
    public interface IUnitOfWork : IDisposable
    {
        IPlayerPreferencesRepository PlayerPreferences { get; }

        IPlayerAccountRepository PlayerAccounts { get; }

        IVerificationCodeRepository VerificationCodes { get; }

        ILocaleRepository Locales { get; }

        IGuestSessionRepository GuestSessions { get; }

        IFriendRepository Friends { get; }

        IModerationRepository Moderation { get; }

        IMessageRepository Messages { get; }

        IChallengeRepository Challenges { get; }

        IGameRepository Games { get; }

        IRankingRepository Rankings { get; }

        void Commit();

        void Rollback();
    }
}
