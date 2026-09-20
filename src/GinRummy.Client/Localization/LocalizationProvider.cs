using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace GinRummy.Client.Localization
{
    /// <summary>
    /// Resolves the visible text of the user interface against the resource file of the
    /// active culture, and notifies the interface when that culture changes so that every
    /// binding refreshes without reopening any window.
    /// </summary>
    public sealed class LocalizationProvider : INotifyPropertyChanged
    {
        /// <summary>
        /// Culture the application starts with, and the one the neutral resource file holds.
        /// </summary>
        public const string DefaultCultureCode = "es-MX";

        /// <summary>
        /// Additional culture required by the internationalization constraint.
        /// </summary>
        public const string AdditionalCultureCode = "en-US";

        private const string ResourceBaseName = "GinRummy.Client.Resources.Strings";
        private const string IndexerPropertyName = "Item[]";
        private const string CurrentCulturePropertyName = "CurrentCulture";
        private const string SelectedCulturePropertyName = "SelectedCulture";

        private static readonly LocalizationProvider SingleInstance = new LocalizationProvider();

        private readonly ResourceManager _resourceManager;
        private readonly List<CultureOption> _availableCultures;
        private CultureInfo _currentCulture;
        private CultureOption _selectedCulture;

        private LocalizationProvider()
        {
            _resourceManager = new ResourceManager(ResourceBaseName, typeof(LocalizationProvider).Assembly);
            _availableCultures = new List<CultureOption>
            {
                new CultureOption(DefaultCultureCode, "Español", "Español (México)"),
                new CultureOption(AdditionalCultureCode, "English", "English (United States)")
            };
            _currentCulture = CultureInfo.GetCultureInfo(DefaultCultureCode);
            _selectedCulture = _availableCultures[0];
        }

        /// <summary>
        /// Raised when the active culture changes, so that the bindings re-read their text.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the only instance of the provider.
        /// </summary>
        public static LocalizationProvider Instance
        {
            get { return SingleInstance; }
        }

        /// <summary>
        /// Gets the text associated with a resource key in the active culture. This indexer
        /// is what the bindings declared in XAML use.
        /// </summary>
        /// <param name="resourceKey">Key declared in the internationalization dictionary.</param>
        /// <returns>The text of the key, or the key itself when it is missing.</returns>
        public string this[string resourceKey]
        {
            get { return GetText(resourceKey); }
        }

        /// <summary>
        /// Gets the active culture, used to format dates, durations and numbers.
        /// </summary>
        public CultureInfo CurrentCulture
        {
            get { return _currentCulture; }
        }

        /// <summary>
        /// Gets the cultures offered to the player.
        /// </summary>
        public IList<CultureOption> AvailableCultures
        {
            get { return _availableCultures; }
        }

        /// <summary>
        /// Gets or sets the culture chosen in the language selector.
        /// </summary>
        public CultureOption SelectedCulture
        {
            get { return _selectedCulture; }
            set
            {
                if (value != null && value != _selectedCulture)
                {
                    SetCulture(value.Code);
                }
            }
        }

        /// <summary>
        /// Applies a culture to the whole application and refreshes every visible text.
        /// </summary>
        /// <param name="cultureCode">Culture code, for example en-US.</param>
        public void SetCulture(string cultureCode)
        {
            CultureInfo culture = CultureInfo.GetCultureInfo(cultureCode);
            _currentCulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            _selectedCulture = FindOption(cultureCode);
            _resourceManager.ReleaseAllResources();
            RaisePropertyChanged(IndexerPropertyName);
            RaisePropertyChanged(CurrentCulturePropertyName);
            RaisePropertyChanged(SelectedCulturePropertyName);
        }

        /// <summary>
        /// Returns the text of a resource key in the active culture.
        /// </summary>
        /// <param name="resourceKey">Key declared in the internationalization dictionary.</param>
        /// <returns>The text of the key, or the key itself when it is missing.</returns>
        public string GetText(string resourceKey)
        {
            string text = resourceKey;
            if (!string.IsNullOrEmpty(resourceKey))
            {
                string stored = _resourceManager.GetString(resourceKey, _currentCulture);
                if (stored != null)
                {
                    text = stored;
                }
            }

            return text;
        }

        /// <summary>
        /// Builds a message from a format string of the dictionary and its arguments. Visible
        /// messages are never assembled by concatenation, because word order changes between
        /// languages.
        /// </summary>
        /// <param name="resourceKey">Key of the format string.</param>
        /// <param name="arguments">Values that replace the placeholders.</param>
        /// <returns>The message already formatted with the active culture.</returns>
        public string Format(string resourceKey, params object[] arguments)
        {
            return string.Format(_currentCulture, GetText(resourceKey), arguments);
        }

        private CultureOption FindOption(string cultureCode)
        {
            CultureOption found = _availableCultures[0];
            foreach (CultureOption option in _availableCultures)
            {
                if (string.Equals(option.Code, cultureCode, StringComparison.OrdinalIgnoreCase))
                {
                    found = option;
                }
            }

            return found;
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
