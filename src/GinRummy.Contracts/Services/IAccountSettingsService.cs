using System.ServiceModel;

using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Faults;

namespace GinRummy.Contracts.Services
{
    /// <summary>
    /// Operations of the settings of the own account: two-step verification (CU-04, CU-05), password (CU-06),
    /// email (CU-07) and chat filter (CU-17). The changes of security are applied only with the code emailed for
    /// them, except the deactivation of the second factor, which asks for the password.
    /// </summary>
    [ServiceContract]
    public interface IAccountSettingsService
    {
        /// <summary>
        /// Gets the email and the state of two-step verification of the player who asks.
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <returns>The settings of the account.</returns>
        /// <exception cref="FaultException{ServiceFault}">Thrown when the access token is not valid.</exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        AccountSettingsDetails GetAccountSettings(string accessToken);

        /// <summary>
        /// Emails the code that activates two-step verification. Asking again sends a new code.
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when the access token is not valid, or when the code could not be emailed.
        /// </exception>
        /// <exception cref="FaultException{ResendLimitFault}">Thrown when the limit of codes does not allow a new one yet.</exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        [FaultContract(typeof(ResendLimitFault))]
        void RequestTwoFactorActivation(string accessToken);

        /// <summary>
        /// Activates two-step verification with the code emailed for it.
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <param name="verificationCode">Code as the player typed it.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when the access token is not valid, or when the code is missing, malformed, incorrect, expired,
        /// already used or out of attempts.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ConfirmTwoFactorActivation(string accessToken, string verificationCode);

        /// <summary>
        /// Deactivates two-step verification after checking the password of the account.
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <param name="password">Password of the account, as the player typed it.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when the access token is not valid, or when the password is missing or incorrect.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void DisableTwoFactor(string accessToken, string password);

        /// <summary>
        /// Checks both passwords and emails the code that confirms the change. Asking again sends a new code.
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <param name="currentPassword">Password the account has now.</param>
        /// <param name="newPassword">Password that replaces it.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when the access token is not valid, a field is missing, the current password is incorrect, the
        /// new one does not meet the policy or is the same, or the code could not be emailed.
        /// </exception>
        /// <exception cref="FaultException{ResendLimitFault}">Thrown when the limit of codes does not allow a new one yet.</exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        [FaultContract(typeof(ResendLimitFault))]
        void RequestPasswordChange(string accessToken, string currentPassword, string newPassword);

        /// <summary>
        /// Replaces the password with the code emailed for the change.
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <param name="newPassword">Password that replaces the current one.</param>
        /// <param name="verificationCode">Code as the player typed it.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when the access token is not valid, the new password does not meet the policy or is the same,
        /// or the code is missing, malformed, incorrect, expired, already used or out of attempts.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ConfirmPasswordChange(string accessToken, string newPassword, string verificationCode);

        /// <summary>
        /// Emails to the new address the code that confirms the change. Asking again sends a new code.
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <param name="newEmail">Address that replaces the email of the account.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when the access token is not valid, the address is missing, has forbidden characters, is not
        /// valid or already belongs to an account, or the code could not be emailed.
        /// </exception>
        /// <exception cref="FaultException{ResendLimitFault}">Thrown when the limit of codes does not allow a new one yet.</exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        [FaultContract(typeof(ResendLimitFault))]
        void RequestEmailChange(string accessToken, string newEmail);

        /// <summary>
        /// Replaces the email with the address the code was emailed to.
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <param name="newEmail">Address the code was emailed to.</param>
        /// <param name="verificationCode">Code as the player typed it.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when the access token is not valid, the address already belongs to an account, or the code is
        /// missing, malformed, incorrect, expired, already used or out of attempts.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ConfirmEmailChange(string accessToken, string newEmail, string verificationCode);

        /// <summary>
        /// Saves whether the player reads the messages with prohibited words as censored (CU-17 step 10).
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <param name="isChatFilterEnabled">Whether those messages are shown as censored.</param>
        /// <exception cref="FaultException{ServiceFault}">Thrown when the access token is not valid.</exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ChangeChatFilter(string accessToken, bool isChatFilterEnabled);
    }
}
