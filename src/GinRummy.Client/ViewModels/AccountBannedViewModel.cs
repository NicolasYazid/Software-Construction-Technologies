using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class AccountBannedViewModel : ViewModelBase
    {
        public AccountBannedViewModel(string reasonName)
        {
            ReasonName = reasonName;
        }

        public string ReasonName { get; private set; }
    }
}
