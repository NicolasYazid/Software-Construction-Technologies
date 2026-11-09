using System;

using GinRummy.Application.Ports;
using GinRummy.Domain.Entities.AccountProtection;
using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Shared;

namespace GinRummy.Application.UseCases
{
    public class SignUpUseCase : ISignUpPort
    {
        private readonly Func<IUnitOfWork> _unitOfWorkFactory;
        private readonly AccountRegistrar _accountRegistrar;
        private readonly VerificationCodeSender _codeSender;

        public SignUpUseCase(
            Func<IUnitOfWork> unitOfWorkFactory,
            AccountRegistrar accountRegistrar,
            VerificationCodeSender codeSender)
        {
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _accountRegistrar = accountRegistrar ?? throw new ArgumentNullException(nameof(accountRegistrar));
            _codeSender = codeSender ?? throw new ArgumentNullException(nameof(codeSender));
        }


        public SignUpResult SignUp(SignUpForm form, string localeCode)
        {
            SignUpResult result;
            using (IUnitOfWork unitOfWork = _unitOfWorkFactory())
            {
                PlayerAccount account = _accountRegistrar.Register(unitOfWork, form, localeCode);

                EmailDeliveryStatus deliveryStatus = _codeSender.Send(unitOfWork, account, VerificationPurpose.CreateAccount);
                unitOfWork.Commit();
                result = new SignUpResult(account, deliveryStatus);
            }

            return result;
        }
    }
}
