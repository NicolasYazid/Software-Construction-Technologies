using System;

using GinRummy.Client.AccountSettingsServiceReference;
using GinRummy.Client.Models;

namespace GinRummy.Client.ViewModels
{
    public class TwoStepActivationViewModel : TwoStepViewModel
    {
        private const string RequiredFieldKey = "Error_ValRequiredField";
        private readonly AccountSettingsCaller _accountSettingsCaller;

        // The panel already asked for the first code, so this screen only confirms it or asks for a new one (CU-04).
        public TwoStepActivationViewModel(AccountSettingsCaller accountSettingsCaller)
            : base(TwoStepPurpose.EnableTwoStep)
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

            if (CallAccountSettings((service, accessToken) => service.ConfirmTwoFactorActivation(accessToken, Code)))
            {
                base.Verify();
            }
        }

        // A new code replaces the previous one on the server, so its ten minutes start over (CU-04 FA-03).
        protected override void ResendCode()
        {
            ErrorMessageKey = null;
            if (CallAccountSettings((service, accessToken) => service.RequestTwoFactorActivation(accessToken)))
            {
                base.ResendCode();
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
