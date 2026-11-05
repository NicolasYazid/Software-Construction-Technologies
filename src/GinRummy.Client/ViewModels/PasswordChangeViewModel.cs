using System;

using GinRummy.Client.AccountSettingsServiceReference;
using GinRummy.Client.Models;

namespace GinRummy.Client.ViewModels
{
    public class PasswordChangeViewModel : NewPasswordViewModel
    {
        private readonly AccountSettingsCaller _accountSettingsCaller;

        public PasswordChangeViewModel(AccountSettingsCaller accountSettingsCaller)
            : base(true)
        {
            _accountSettingsCaller = accountSettingsCaller ?? throw new ArgumentNullException(nameof(accountSettingsCaller));
        }

        public event EventHandler<ErrorDialogKind> ErrorDialogRequested;

        // The server checks both passwords before it sends the code, so a mistake shows here and not after the email (CU-06).
        protected override void SubmitPassword()
        {
            bool isRequested = _accountSettingsCaller.Call(RequestPasswordChange);
            ErrorMessageKey = _accountSettingsCaller.ErrorMessageKey;
            if (isRequested)
            {
                base.SubmitPassword();
            }
            else if (_accountSettingsCaller.ErrorDialog.HasValue)
            {
                ErrorDialogRequested?.Invoke(this, _accountSettingsCaller.ErrorDialog.Value);
            }
        }

        private void RequestPasswordChange(IAccountSettingsService service, string accessToken)
        {
            service.RequestPasswordChange(accessToken, CurrentPassword, NewPassword);
        }
    }
}
