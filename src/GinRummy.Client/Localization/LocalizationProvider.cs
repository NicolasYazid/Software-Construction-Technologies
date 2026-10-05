using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace GinRummy.Client.Localization
{
    // Resolves the visible text of the user interface against the resource file of the active
    // culture, and notifies the interface when that culture changes so that every binding
    // refreshes without reopening any window.
    public sealed class LocalizationProvider : INotifyPropertyChanged
    {
        public const string DefaultCultureCode = "es-MX";

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

        // Raised when the active culture changes, so that the bindings re-read their text.
        public event PropertyChangedEventHandler PropertyChanged;

        public static LocalizationProvider Instance
        {
            get { return SingleInstance; }
        }

        public string this[string resourceKey]
        {
            get { return GetText(resourceKey); }
        }

        public CultureInfo CurrentCulture
        {
            get { return _currentCulture; }
        }

        public IList<CultureOption> AvailableCultures
        {
            get { return _availableCultures; }
        }

        public CultureOption SelectedCulture
        {
            get { return _selectedCulture; }
            set
            {
                if ((value != null) && (value != _selectedCulture))
                {
                    SetCulture(value.Code);
                }
            }
        }

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

        // Visible messages are never assembled by concatenation, because word order changes
        // between languages.
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
