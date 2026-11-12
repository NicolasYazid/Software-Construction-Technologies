using System.ServiceModel;

using GinRummy.Contracts.DataContracts.Authentication;
using GinRummy.Contracts.Faults.AccountProtection;
using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.Authentication
{
    [ServiceContract]
    public interface ISignUpService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        SignUpOutcome SignUp(NewAccount newAccount);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void VerifyEmailByVerificationCode(string email, string verificationCode);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        [FaultContract(typeof(ResendLimitFault))]
        void ResendVerificationCode(string email);
    }
}
