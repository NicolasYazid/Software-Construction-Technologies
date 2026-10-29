using System;

using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class AccountSuspendedViewModel : ViewModelBase
    {
        public AccountSuspendedViewModel(string reasonName, TimeSpan remainingTime)
        {
            ReasonName = reasonName;
            Countdown = new CountdownViewModel(remainingTime);
            Countdown.Start();
        }

        public string ReasonName { get; private set; }
        public CountdownViewModel Countdown { get; private set; }
    }
}
