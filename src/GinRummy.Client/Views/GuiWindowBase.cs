using System;
using System.ComponentModel;
using System.Windows;
using GinRummy.Client.Localization;

namespace GinRummy.Client.Views
{
    /// <summary>
    /// Base window of every screen. It keeps the window subscribed to the culture change so
    /// that the texts built from a format string can be rebuilt, and releases the
    /// subscription when the window closes.
    /// </summary>
    public class GuiWindowBase : Window
    {
        private readonly LocalizationProvider _localization;

        /// <summary>
        /// Subscribes the window to the culture change.
        /// </summary>
        protected GuiWindowBase()
        {
            _localization = LocalizationProvider.Instance;
            _localization.PropertyChanged += OnLocalizationChanged;
            Closed += OnWindowClosed;
        }

        /// <summary>
        /// Gets the localization provider shared by the whole application.
        /// </summary>
        protected LocalizationProvider Localization
        {
            get { return _localization; }
        }

        /// <summary>
        /// Rebuilds the texts that are assembled from a format string. Screens that show
        /// counters, durations or interpolated values override it.
        /// </summary>
        protected virtual void RefreshFormattedText()
        {
        }

        private void OnLocalizationChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshFormattedText();
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            _localization.PropertyChanged -= OnLocalizationChanged;
            Closed -= OnWindowClosed;
        }
    }
}
