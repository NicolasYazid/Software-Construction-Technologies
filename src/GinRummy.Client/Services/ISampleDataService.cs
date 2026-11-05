using System.Collections.Generic;

using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Services
{
    // The view models depend on this contract and not on the sample data, so a test can hand them fake data instead.
    public interface ISampleDataService
    {
        LobbySnapshot GetLobby();

        LobbySnapshot GetGuestLobby();

        IList<FriendRequest> GetFriendRequests();

        IList<SanctionViewModel> GetSanctions();

        AccountSettings GetAccountSettings();

        PlayerProfile GetOwnProfile();

        PlayerProfile GetPlayerProfile(LobbyPlayerViewModel player);

        IList<string> GetPlatforms();

        GameRules GetGameRules();

        GameTableSnapshot GetGameTable();
    }
}
