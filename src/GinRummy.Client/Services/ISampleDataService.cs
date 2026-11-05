using System.Collections.Generic;

using GinRummy.Client.Models;
using GinRummy.Client.ViewModels;

namespace GinRummy.Client.Services
{
    // The view models depend on this contract and not on the sample data, so a test can hand them fake data instead.
    public interface ISampleDataService
    {
        IList<FriendRequest> GetFriendRequests();

        IList<SanctionViewModel> GetSanctions();

        GameRules GetGameRules();

        GameTableSnapshot GetGameTable();
    }
}
