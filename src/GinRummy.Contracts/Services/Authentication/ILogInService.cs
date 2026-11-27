using System.ServiceModel;

using GinRummy.Contracts.DataContracts.Authentication;
using GinRummy.Contracts.Faults.AccountProtection;
using GinRummy.Contracts.Faults.PlayerModeration;
using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.Authentication
{
    [ServiceContract]
    public interface ILogInService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        [FaultContract(typeof(SanctionFault))]
        [FaultContract(typeof(ResendLimitFault))]
        LogInOutcome LogIn(string email, string password, bool isKeepSignedIn);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        [FaultContract(typeof(SanctionFault))]
        LogInOutcome VerifyLogInByTwoFactorAuthentication(string email, string verificationCode, bool isKeepSignedIn);

        [OperationContract]
        LogInOutcome LogInAsGuest();
    }
}
