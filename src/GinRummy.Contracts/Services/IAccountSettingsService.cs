using System.ServiceModel;

using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Faults;

namespace GinRummy.Contracts.Services
{
    [ServiceContract]
    public interface IAccountSettingsService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        AccountSettingsDetails GetAccountSettings(string accessToken);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        [FaultContract(typeof(ResendLimitFault))]
        void RequestTwoFactorActivation(string accessToken);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ConfirmTwoFactorActivation(string accessToken, string verificationCode);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void DisableTwoFactor(string accessToken, string password);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        [FaultContract(typeof(ResendLimitFault))]
        void RequestPasswordChange(string accessToken, string currentPassword, string newPassword);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ConfirmPasswordChange(string accessToken, string newPassword, string verificationCode);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        [FaultContract(typeof(ResendLimitFault))]
        void RequestEmailChange(string accessToken, string newEmail);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ConfirmEmailChange(string accessToken, string newEmail, string verificationCode);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ChangeChatFilter(string accessToken, bool isChatFilterEnabled);
    }
}
