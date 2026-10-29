using System;

using GinRummy.Client.Mvvm;

namespace GinRummy.Client.ViewModels
{
    public class ReconnectingViewModel : ViewModelBase
    {
        // The length of the reconnection window is still an open decision of the team.
        // This value only drives the countdown until the server supplies the real one.
        private const int ReconnectionWindowSeconds = 60;

        public ReconnectingViewModel()
        {
            Countdown = new CountdownViewModel(TimeSpan.FromSeconds(ReconnectionWindowSeconds));
            Countdown.Start();
        }

        public CountdownViewModel Countdown { get; private set; }
    }
}
