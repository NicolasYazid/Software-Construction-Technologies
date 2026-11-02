using System.ServiceModel;

using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Faults;

namespace GinRummy.Contracts.Services
{
    /// <summary>
    /// Operations of the sign-in of a registered player (CU-02).
    /// </summary>
    [ServiceContract]
    public interface ILogInService
    {
        /// <summary>
        /// Signs a player in with the email and the password of the account. When the account has the second
        /// factor turned on, the code is emailed and no access token is granted until it is confirmed.
        /// </summary>
        /// <param name="email">Email of the account, as the player typed it.</param>
        /// <param name="password">Password of the account, as the player typed it.</param>
        /// <param name="keepSignedIn">Whether the player asked to stay signed in.</param>
        /// <returns>The access token, or the notice that the code of the second factor is pending.</returns>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when a field is empty or has forbidden characters, the credentials are not valid, the email
        /// is not verified, or the code of the second factor could not be emailed.
        /// </exception>
        /// <exception cref="FaultException{SanctionFault}">
        /// Thrown when a ban that restricts the whole account is in force.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        [FaultContract(typeof(SanctionFault))]
        LogInOutcome LogIn(string email, string password, bool keepSignedIn);

        /// <summary>
        /// Completes a sign-in whose password was accepted, with the code emailed to the account.
        /// </summary>
        /// <param name="email">Email of the account, as the player typed it to sign in.</param>
        /// <param name="verificationCode">Code of the second factor, as the player typed it.</param>
        /// <param name="keepSignedIn">Whether the player asked to stay signed in.</param>
        /// <returns>The access token.</returns>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when a field is empty or has forbidden characters, or the code is malformed, incorrect,
        /// expired, already used or out of attempts.
        /// </exception>
        /// <exception cref="FaultException{SanctionFault}">
        /// Thrown when a ban began to restrict the whole account after the password was accepted.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        [FaultContract(typeof(SanctionFault))]
        LogInOutcome VerifyLogInByTwoFactorAuthentication(string email, string verificationCode, bool keepSignedIn);
    }
}
