using System.ServiceModel;

using GinRummy.Contracts.DataContracts.Authentication;
using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.Authentication
{
    [ServiceContract]
    public interface ILogInService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        LogInOutcome LogIn(string email, string password, bool isKeepSignedIn);

        [OperationContract]
        LogInOutcome VerifyLogInByTwoFactorAuthentication(string email, string verificationCode, bool isKeepSignedIn);

        [OperationContract]
        LogInOutcome LogInAsGuest();
    }
}
