using System;

using GinRummy.Client.AccountSettingsServiceReference;
using GinRummy.Client.Models;

namespace GinRummy.Client.ViewModels
{
    public class EmailChangeVerifyEmailViewModel : VerifyEmailViewModel
    {
        private const string RequiredFieldKey = "Error_ValRequiredField";
        private const int AttemptJustUsed = 1;
        private readonly AccountSettingsCaller _accountSettingsCaller;

        // The code goes to the new address, so the screen shows that address and confirms the change with it (CU-07).
        public EmailChangeVerifyEmailViewModel(AccountSettingsCaller accountSettingsCaller, string newEmail)
            : base(VerificationPurpose.EmailChange, newEmail)
        {
            _accountSettingsCaller = accountSettingsCaller ?? throw new ArgumentNullException(nameof(accountSettingsCaller));
        }

        protected override void Verify()
        {
            ErrorMessageKey = null;
            if (string.IsNullOrEmpty(Code))
            {
                ErrorMessageKey = RequiredFieldKey;
                return;
            }

            if (CallAccountSettings((service, accessToken) => service.ConfirmEmailChange(accessToken, DestinationAddress, Code)))
            {
                base.Verify();
            }
            else
            {
                CountUsedAttempt();
            }
        }

        // The new code exists even when its email failed, so its time and the wait for another one start anyway.
        protected override void ResendCode()
        {
            ErrorMessageKey = null;
            bool isRequested = CallAccountSettings((service, accessToken) => service.RequestEmailChange(accessToken, DestinationAddress));
            if (isRequested || (_accountSettingsCaller.FaultCode == ServiceFaultCode.CodeNotSent))
            {
                base.ResendCode();
                RemainingAttempts = MaximumAttempts;
            }
            else if (_accountSettingsCaller.ResendWait > TimeSpan.Zero)
            {
                RestartResendCountdown(_accountSettingsCaller.ResendWait);
            }
        }

        private void CountUsedAttempt()
        {
            if (_accountSettingsCaller.FaultCode == ServiceFaultCode.CodeIncorrect)
            {
                RemainingAttempts = Math.Max(RemainingAttempts - AttemptJustUsed, 0);
            }
            else if (_accountSettingsCaller.FaultCode == ServiceFaultCode.CodeTooManyAttempts)
            {
                RemainingAttempts = 0;
            }
        }

        private bool CallAccountSettings(Action<IAccountSettingsService, string> operation)
        {
            bool isCompleted = _accountSettingsCaller.Call(operation);
            ErrorMessageKey = _accountSettingsCaller.ErrorMessageKey;
            if (_accountSettingsCaller.ErrorDialog.HasValue)
            {
                RequestErrorDialog(_accountSettingsCaller.ErrorDialog.Value);
            }

            return isCompleted;
        }
    }
}
