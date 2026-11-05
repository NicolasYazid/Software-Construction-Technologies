using System;

using GinRummy.Client.AccountSettingsServiceReference;
using GinRummy.Client.Models;

namespace GinRummy.Client.ViewModels
{
    public class TwoStepPasswordChangeViewModel : TwoStepViewModel
    {
        private const string RequiredFieldKey = "Error_ValRequiredField";
        private readonly AccountSettingsCaller _accountSettingsCaller;
        private readonly string _currentPassword;
        private readonly string _newPassword;

        // The server sends a new code only after checking both passwords again, so the screen keeps them until it closes (CU-06).
        public TwoStepPasswordChangeViewModel(AccountSettingsCaller accountSettingsCaller, string currentPassword, string newPassword)
            : base(TwoStepPurpose.ChangePassword)
        {
            _accountSettingsCaller = accountSettingsCaller ?? throw new ArgumentNullException(nameof(accountSettingsCaller));
            _currentPassword = currentPassword;
            _newPassword = newPassword;
        }

        protected override void Verify()
        {
            ErrorMessageKey = null;
            if (string.IsNullOrEmpty(Code))
            {
                ErrorMessageKey = RequiredFieldKey;
                return;
            }

            if (CallAccountSettings((service, accessToken) => service.ConfirmPasswordChange(accessToken, _newPassword, Code)))
            {
                base.Verify();
            }
        }

        protected override void ResendCode()
        {
            ErrorMessageKey = null;
            if (CallAccountSettings((service, accessToken) => service.RequestPasswordChange(accessToken, _currentPassword, _newPassword)))
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
