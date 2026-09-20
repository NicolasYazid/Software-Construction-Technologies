using System.Windows;
using GinRummy.Client.Localization;

namespace GinRummy.Client
{
    /// <summary>
    /// Application entry point. Publishes the localization provider as an application
    /// resource so that every window can bind its visible text to the active culture.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Key under which the localization provider is published. Every binding to
        /// visible text uses it as its source.
        /// </summary>
        public const string LocalizationResourceKey = "Loc";

        /// <summary>
        /// Publishes the localization provider before the first window is loaded.
        /// </summary>
        /// <param name="e">Startup arguments supplied by the framework.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            Resources[LocalizationResourceKey] = LocalizationProvider.Instance;
            LocalizationProvider.Instance.SetCulture(LocalizationProvider.DefaultCultureCode);
            base.OnStartup(e);
        }
    }
}
