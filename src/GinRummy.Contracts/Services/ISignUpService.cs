using System.ServiceModel;

using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Faults;

namespace GinRummy.Contracts.Services
{
    /// <summary>
    /// Operations of the creation of an account (CU-01) and of the verification of its email (CU-09).
    /// </summary>
    [ServiceContract]
    public interface ISignUpService
    {
        /// <summary>
        /// Creates an account and emails the code that verifies its email. The account is created even when
        /// the email fails, and the answer says whether it was sent.
        /// </summary>
        /// <param name="newAccount">What the person typed, with the language the client was showing.</param>
        /// <returns>The stored email of the account and whether its code was sent.</returns>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when a field is empty, has forbidden characters or is too long, the email is not valid or
        /// already belongs to an account, or the password does not meet the policy.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        SignUpOutcome SignUp(NewAccount newAccount);

        /// <summary>
        /// Verifies the email of an account with the code emailed to it.
        /// </summary>
        /// <param name="email">Email of the account, as the server stored it.</param>
        /// <param name="verificationCode">Code that verifies the email, as the player typed it.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when a field is empty or has forbidden characters, the email was already verified, or the
        /// code is malformed, incorrect, expired, already used or out of attempts.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void VerifyEmailByVerificationCode(string email, string verificationCode);

        /// <summary>
        /// Emails a new code to an account whose email is not verified yet. The previous code stops working.
        /// </summary>
        /// <param name="email">Email of the account, as the server stored it.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when the email is empty or has forbidden characters, the email was already verified, or the
        /// new code could not be emailed.
        /// </exception>
        /// <exception cref="FaultException{ResendLimitFault}">
        /// Thrown when the limit of codes does not allow a new one yet.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        [FaultContract(typeof(ResendLimitFault))]
        void ResendVerificationCode(string email);
    }
}
