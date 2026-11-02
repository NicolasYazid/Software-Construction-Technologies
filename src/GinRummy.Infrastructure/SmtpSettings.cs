using System.Collections.Specialized;
using System.Globalization;

namespace GinRummy.Infrastructure
{
    /// <summary>
    /// Connection values of the mail service that delivers the verification codes. The host reads them from its
    /// appSettings, which come from SmtpSettings.config, a file kept out of the repository because it holds the
    /// password of the sender account.
    /// </summary>
    public class SmtpSettings
    {
        /// <summary>
        /// Key of the mail server name.
        /// </summary>
        public const string HostKey = "SmtpHost";

        /// <summary>
        /// Key of the mail server port.
        /// </summary>
        public const string PortKey = "SmtpPort";

        /// <summary>
        /// Key of the address the emails are sent from, which is also the account that signs in.
        /// </summary>
        public const string SenderAddressKey = "SmtpSenderAddress";

        /// <summary>
        /// Key of the name shown as the sender.
        /// </summary>
        public const string SenderNameKey = "SmtpSenderName";

        /// <summary>
        /// Key of the password of the sender account.
        /// </summary>
        public const string SenderPasswordKey = "SmtpSenderPassword";

        /// <summary>
        /// Key of the seconds a delivery may take before it counts as failed.
        /// </summary>
        public const string TimeoutSecondsKey = "SmtpTimeoutSeconds";

        private const int MissingNumber = 0;
        private const int MinPort = 1;
        private const int MaxPort = 65535;
        private const int MinTimeoutSeconds = 1;
        private const int MillisecondsPerSecond = 1000;

        // SmtpClient takes the timeout in milliseconds as an int, so a larger number of seconds would overflow.
        private const int MaxTimeoutSeconds = int.MaxValue / MillisecondsPerSecond;

        private SmtpSettings()
        {
        }

        /// <summary>
        /// Gets whether every required value is present and inside its valid range. When it is false, no delivery
        /// is attempted and every email ends as Failed, so the rest of the server keeps working (QAS-12).
        /// </summary>
        public bool IsComplete
        {
            get
            {
                bool isPortValid = (Port >= MinPort) && (Port <= MaxPort);
                bool isTimeoutValid = (TimeoutSeconds >= MinTimeoutSeconds) && (TimeoutSeconds <= MaxTimeoutSeconds);
                bool hasServer = !string.IsNullOrWhiteSpace(Host) && isPortValid;
                bool hasSender = !string.IsNullOrWhiteSpace(SenderAddress) && !string.IsNullOrWhiteSpace(SenderPassword);
                bool isComplete = hasServer && hasSender && isTimeoutValid;

                return isComplete;
            }
        }

        internal string Host { get; private set; }
        internal int Port { get; private set; }
        internal string SenderAddress { get; private set; }
        internal string SenderName { get; private set; }
        internal string SenderPassword { get; private set; }
        internal int TimeoutSeconds { get; private set; }

        internal int TimeoutMilliseconds
        {
            get { return TimeoutSeconds * MillisecondsPerSecond; }
        }

        /// <summary>
        /// Reads the mail service values from the given appSettings. A missing or malformed value is kept as empty
        /// instead of throwing, and is reported through <see cref="IsComplete"/>.
        /// </summary>
        /// <param name="appSettings">The appSettings of the host, usually ConfigurationManager.AppSettings.</param>
        /// <returns>The values found, complete or not.</returns>
        public static SmtpSettings FromAppSettings(NameValueCollection appSettings)
        {
            SmtpSettings settings = new SmtpSettings();
            settings.Host = ReadText(appSettings, HostKey);
            settings.Port = ReadNumber(appSettings, PortKey);
            settings.SenderAddress = ReadText(appSettings, SenderAddressKey);
            settings.SenderName = ReadText(appSettings, SenderNameKey);
            settings.SenderPassword = ReadText(appSettings, SenderPasswordKey);
            settings.TimeoutSeconds = ReadNumber(appSettings, TimeoutSecondsKey);

            return settings;
        }

        private static string ReadText(NameValueCollection appSettings, string key)
        {
            string text = string.Empty;
            string storedValue = appSettings?[key];
            if (storedValue != null)
            {
                text = storedValue.Trim();
            }

            return text;
        }

        private static int ReadNumber(NameValueCollection appSettings, string key)
        {
            int number = MissingNumber;
            if (int.TryParse(ReadText(appSettings, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedNumber))
            {
                number = parsedNumber;
            }

            return number;
        }
    }
}