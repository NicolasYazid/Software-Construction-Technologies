using System;

using GinRummy.Application.Ports;
using GinRummy.Domain;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Entities.Players;
using GinRummy.Domain.Ports.Repositories;
using GinRummy.Domain.Services;

namespace GinRummy.Application.UseCases
{
    /// <summary>
    /// The Create an account use case (CU-01): creates the account with everything that comes with it and
    /// emails the code that verifies its email, all in one transaction.
    /// </summary>
    public class SignUpUseCase : ISignUpPort
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountRegistrar _accountRegistrar;
        private readonly VerificationCodeSender _codeSender;

        /// <summary>
        /// Builds the use case with the collaborators the composition root assembled.
        /// </summary>
        /// <param name="unitOfWorkFactory">Opens the transaction that groups the writes of one sign-up.</param>
        /// <param name="accountRegistrar">Checks what was typed and creates the account.</param>
        /// <param name="codeSender">Emails the code that verifies the email.</param>
        /// <exception cref="ArgumentNullException">Thrown when a dependency is null.</exception>
        public SignUpUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountRegistrar accountRegistrar,
            VerificationCodeSender codeSender)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accountRegistrar = accountRegistrar ?? throw new ArgumentNullException(nameof(accountRegistrar));
            _codeSender = codeSender ?? throw new ArgumentNullException(nameof(codeSender));
        }

        /// <summary>
        /// Creates an account with what the person typed and emails the code that verifies its email.
        /// </summary>
        /// <param name="form">What the person typed in the sign-up screen.</param>
        /// <param name="localeCode">Culture code of the language the client was showing, such as es-MX.</param>
        /// <returns>The created account and whether the email with the code was accepted.</returns>
        /// <exception cref="RequiredFieldMissingException">Thrown when a field is empty.</exception>
        /// <exception cref="DisallowedCharactersException">Thrown when a field contains forbidden characters.</exception>
        /// <exception cref="InvalidEmailFormatException">Thrown when the email is not a valid address.</exception>
        /// <exception cref="FieldTooLongException">Thrown when the username is longer than its column.</exception>
        /// <exception cref="WeakPasswordException">Thrown when the password does not meet the policy.</exception>
        /// <exception cref="EmailAlreadyRegisteredException">Thrown when another account already has the email.</exception>
        public SignUpResult SignUp(SignUpForm form, string localeCode)
        {
            SignUpResult result;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = _accountRegistrar.Register(unitOfWork, form, localeCode);

                // A failed email does not undo the account: the failure is recorded and the code can be sent again (CU-01 EX-06).
                EmailDeliveryStatus deliveryStatus = _codeSender.Send(unitOfWork, account, VerificationPurpose.CreateAccount);
                unitOfWork.Commit();
                result = new SignUpResult(account, deliveryStatus);
            }

            return result;
        }
    }
}
