using System.ServiceModel;

using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Faults;

namespace GinRummy.Contracts.Services
{
    /// <summary>
    /// Operations to look at the profile of a registered player (CU-27) and to edit the own one
    /// (CU-28 to CU-32).
    /// </summary>
    [ServiceContract]
    public interface IPlayerProfileService
    {
        /// <summary>
        /// Gets the public information and the performance of a registered player.
        /// </summary>
        /// <param name="accessToken">Access token of the player who asks.</param>
        /// <param name="publicTag">Public tag of the player whose profile is shown.</param>
        /// <returns>The profile of the player.</returns>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when the access token is not valid, or when no player has the public tag.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        PlayerProfileDetails GetPlayerProfile(string accessToken, int publicTag);

        /// <summary>
        /// Saves the name and the biography of the player who asks.
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <param name="username">New name; required.</param>
        /// <param name="bio">New biography; empty clears it.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when the access token is not valid, or when a field is empty, too long or has characters that are not allowed.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void SaveProfileChanges(string accessToken, string username, string bio);

        /// <summary>
        /// Gets the picture of the player who asks.
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <returns>Bytes of the image, or null when the player uses the default avatar.</returns>
        /// <exception cref="FaultException{ServiceFault}">Thrown when the access token is not valid.</exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        byte[] GetOwnProfilePicture(string accessToken);

        /// <summary>
        /// Replaces the picture of the player who asks.
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <param name="picture">Bytes of a JPG or PNG image of at most 2 MB.</param>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when the access token is not valid, or when the picture is not an accepted image.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void ChangeProfilePicture(string accessToken, byte[] picture);

        /// <summary>
        /// Removes the picture of the player who asks, so the default avatar is shown.
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <exception cref="FaultException{ServiceFault}">Thrown when the access token is not valid.</exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void RemoveProfilePicture(string accessToken);

        /// <summary>
        /// Adds a link to a social network to the profile of the player who asks.
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <param name="platformName">Name of the network, as the player writes it.</param>
        /// <param name="url">Address of the player's page; must be a web address.</param>
        /// <returns>The new link, with its identifier.</returns>
        /// <exception cref="FaultException{ServiceFault}">
        /// Thrown when the access token is not valid, when a field is empty, too long or has characters that are not allowed,
        /// or when the address is not a web address.
        /// </exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        SocialLinkDetails LinkSocialMedia(string accessToken, string platformName, string url);

        /// <summary>
        /// Removes a link from the profile of the player who asks. A link that no longer exists is ignored.
        /// </summary>
        /// <param name="accessToken">Access token of the player.</param>
        /// <param name="socialMediaId">Identifier of the link.</param>
        /// <exception cref="FaultException{ServiceFault}">Thrown when the access token is not valid.</exception>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void UnlinkSocialMedia(string accessToken, int socialMediaId);
    }
}
