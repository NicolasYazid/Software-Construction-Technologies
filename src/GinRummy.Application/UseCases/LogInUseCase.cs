using System;
using System.Globalization;

using GinRummy.Application.Ports;
using GinRummy.Domain;
using GinRummy.Domain.Entities.AccountProtection;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases
{
    public class LogInUseCase : ILogInPort
    {
        private const string GuestNamePrefix = "Guest_";
        private const int FirstGuestNumber = 1000;
        private const int GuestNumberLimit = 10000;

        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountAccessPolicy _accessPolicy;
        private readonly VerificationCodeSender _codeSender;

        public LogInUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountAccessPolicy accessPolicy,
            VerificationCodeSender codeSender)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accessPolicy = accessPolicy ?? throw new ArgumentNullException(nameof(accessPolicy));
            _codeSender = codeSender ?? throw new ArgumentNullException(nameof(codeSender));
        }

        public LogInResult LogIn(string email, string password, bool keepSignedIn)
        {
            TypedFieldValidator.EnsureAreTyped(email, password);
            TypedFieldValidator.EnsureHaveNoForbiddenCharacters(email, password);

            LogInResult result;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = unitOfWork.PlayerAccounts.FindByEmail(PlayerAccount.NormalizeEmail(email));
                _accessPolicy.EnsureCredentialsMatch(account, password);
                _accessPolicy.EnsureAccessAllowed(account, unitOfWork.Moderation.GetBansOf(account.PlayerAccountId));
                if (account.IsTwoFactorEnabled)
                {
                    result = StartTwoFactor(unitOfWork, account);
                }
                else
                {
                    AccessToken accessToken = _accessPolicy.GrantAccess(account, keepSignedIn);
                    unitOfWork.Commit();
                    result = LogInResult.ForSignedIn(account, accessToken);
                }
            }

            return result;
        }

        public GuestAccess LogInAsGuest()
        {
            string temporalDisplayName = GuestNamePrefix
                + new Random().Next(FirstGuestNumber, GuestNumberLimit).ToString(CultureInfo.InvariantCulture);
            GuestSession guest = _accessPolicy.CreateGuestSession(temporalDisplayName);
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                unitOfWork.GuestSessions.Add(guest);
                unitOfWork.Commit();
            }

            return new GuestAccess(guest, _accessPolicy.GrantGuestAccess(guest));
        }

        private LogInResult StartTwoFactor(IUnitOfWork unitOfWork, PlayerAccount account)
        {
            EmailDeliveryStatus deliveryStatus = _codeSender.Send(unitOfWork, account, VerificationPurpose.TwoFactorLogin);
            unitOfWork.Commit();
            if (deliveryStatus == EmailDeliveryStatus.Failed)
            {
                throw new VerificationEmailNotSentException("The code of the second factor could not be emailed.");
            }

            return LogInResult.ForPendingTwoFactor(account);
        }
    }
}
