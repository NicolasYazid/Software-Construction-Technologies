using System.ServiceModel;

using GinRummy.Contracts.DataContracts.PlayerCustomization;
using GinRummy.Contracts.Faults.Shared;

namespace GinRummy.Contracts.Services.PlayerCustomization
{
    [ServiceContract]
    public interface IPlayerProfileService
    {
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        PlayerProfileDetails GetPlayerProfile(string accessToken, int publicTag);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SaveProfileChanges(string accessToken, string username, string bio);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        byte[] GetProfilePicture(string accessToken, int publicTag);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ChangeProfilePicture(string accessToken, byte[] picture);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void RemoveProfilePicture(string accessToken);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        SocialLinkDetails LinkSocialMedia(string accessToken, string platformName, string url);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void UnlinkSocialMedia(string accessToken, int socialMediaId);

        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ChangeLanguage(string accessToken, string localeCode);
    }
}
