using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using GinRummy.Client.Models;
using GinRummy.Client.Services;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Sanctions screen (P13). Implements CU-33: lists the sanctions of the player, marks the
    /// ones in force and counts down how long they have left. Its first tab is the
    /// notifications screen (P12).
    /// </summary>
    public partial class GuiSanctions : GuiWindowBase
    {
        private const int TimerIntervalSeconds = 1;

        private readonly DispatcherTimer _countdownTimer;
        private IList<SanctionDto> _sanctions;
        private TimeSpan _elapsedTime;

        /// <summary>
        /// Builds the screen with the sanctions of the player and starts their countdown.
        /// </summary>
        public GuiSanctions()
        {
            InitializeComponent();
            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(TimerIntervalSeconds);
            _countdownTimer.Tick += OnCountdownTick;
            Closed += OnScreenClosed;
            LoadSanctions();
            _countdownTimer.Start();
        }

        /// <summary>
        /// Loads the sanctions again, because the name of their reason arrives already
        /// translated and has to be asked for in the new language (CU-33 step 3).
        /// </summary>
        protected override void RefreshFormattedText()
        {
            if (lstSanctions != null)
            {
                LoadSanctions();
            }
        }

        private void OnCountdownTick(object sender, EventArgs e)
        {
            TimeSpan step = TimeSpan.FromSeconds(TimerIntervalSeconds);
            _elapsedTime = _elapsedTime.Add(step);
            foreach (SanctionDto sanction in _sanctions)
            {
                sanction.RemainingTime = Shorten(sanction.RemainingTime, step);
            }
        }

        private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && tabFriendRequests.IsSelected)
            {
                ReplaceInPlace(new GuiNotifications());
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnScreenClosed(object sender, EventArgs e)
        {
            _countdownTimer.Stop();
            _countdownTimer.Tick -= OnCountdownTick;
            Closed -= OnScreenClosed;
        }

        private void LoadSanctions()
        {
            // A change of language loads the sanctions again with the time they had when the
            // screen opened, so the time already counted is taken off to keep the countdown
            // where it was.
            SampleDataService dataService = new SampleDataService();
            _sanctions = dataService.GetSanctions();
            foreach (SanctionDto sanction in _sanctions)
            {
                sanction.RemainingTime = Shorten(sanction.RemainingTime, _elapsedTime);
            }

            lstSanctions.ItemsSource = _sanctions;
            lblEmptyState.Visibility = VisibilityCommon.FromCondition(_sanctions.Count == 0);
        }

        private static TimeSpan Shorten(TimeSpan remainingTime, TimeSpan elapsedTime)
        {
            TimeSpan shortened = remainingTime.Subtract(elapsedTime);
            if (shortened < TimeSpan.Zero)
            {
                shortened = TimeSpan.Zero;
            }

            return shortened;
        }
    }
}
