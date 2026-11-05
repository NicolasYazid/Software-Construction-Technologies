using System.ServiceModel;

using GinRummy.Contracts.DataContracts;
using GinRummy.Contracts.Faults;

namespace GinRummy.Contracts.Services
{
    /// <summary>
    /// Operations to look at the profile of a registered player (CU-27).
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
    }
}
