using System.Globalization;
using System.Resources;

using GinRummy.Domain.Entities.Authentication;
using GinRummy.Domain.Ports.Infrastructure;
using GinRummy.Domain.Services;

namespace GinRummy.Infrastructure
{
    /// <summary>
    /// Fulfils IVerificationEmailComposer with the texts of VerificationEmailTexts.resx, whose base file is
    /// in Spanish because es-MX is the default locale of the game, and whose en file serves every
    /// English locale.
    /// </summary>
    public class VerificationEmailComposer : IVerificationEmailComposer
    {
        private const string TextsBaseName = "GinRummy.Infrastructure.VerificationEmailTexts";
        private const string TwoFactorLogInSubjectKey = "TwoFactorLogInSubject";
        private const string TwoFactorLogInBodyKey = "TwoFactorLogInBody";
        private const string SignUpSubjectKey = "SignUpSubject";
        private const string SignUpBodyKey = "SignUpBody";
        private const string GenericSubjectKey = "GenericSubject";
        private const string GenericBodyKey = "GenericBody";
        private static readonly ResourceManager _texts =
            new ResourceManager(TextsBaseName, typeof(VerificationEmailComposer).Assembly);

        /// <summary>
        /// Writes the subject of the email.
        /// </summary>
        /// <param name="purpose">Operation the code confirms.</param>
        /// <param name="localeCode">Locale of the player, such as es-MX; the base texts are used when it is unknown.</param>
        /// <returns>The translated subject.</returns>
        public string ComposeSubject(VerificationPurpose purpose, string localeCode)
        {
            return _texts.GetString(ResolveSubjectKey(purpose), ResolveCulture(localeCode));
        }

        /// <summary>
        /// Writes the body of the email with the code and its lifetime in it.
        /// </summary>
        /// <param name="purpose">Operation the code confirms.</param>
        /// <param name="plainCode">Plain value of the code.</param>
        /// <param name="localeCode">Locale of the player, such as es-MX; the base texts are used when it is unknown.</param>
        /// <returns>The translated body.</returns>
        public string ComposeBody(VerificationPurpose purpose, string plainCode, string localeCode)
        {
            CultureInfo culture = ResolveCulture(localeCode);
            string body = string.Format(
                culture,
                _texts.GetString(ResolveBodyKey(purpose), culture),
                plainCode,
                VerificationCodeIssuer.LifetimeMinutes);

            return body;
        }

        // Only the sign-in and the sign-up have their own texts so far.
        // The other purposes share a generic text until their use cases exist.
        private static string ResolveSubjectKey(VerificationPurpose purpose)
        {
            string subjectKey;
            switch (purpose)
            {
                case VerificationPurpose.TwoFactorLogin:
                    subjectKey = TwoFactorLogInSubjectKey;
                    break;
                case VerificationPurpose.CreateAccount:
                    subjectKey = SignUpSubjectKey;
                    break;
                default:
                    subjectKey = GenericSubjectKey;
                    break;
            }

            return subjectKey;
        }

        private static string ResolveBodyKey(VerificationPurpose purpose)
        {
            string bodyKey;
            switch (purpose)
            {
                case VerificationPurpose.TwoFactorLogin:
                    bodyKey = TwoFactorLogInBodyKey;
                    break;
                case VerificationPurpose.CreateAccount:
                    bodyKey = SignUpBodyKey;
                    break;
                default:
                    bodyKey = GenericBodyKey;
                    break;
            }

            return bodyKey;
        }

        // A locale stored in the database that this machine does not know must not stop the email.
        // The invariant culture makes the resource manager fall back to the base texts.
        private static CultureInfo ResolveCulture(string localeCode)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            if (!string.IsNullOrWhiteSpace(localeCode))
            {
                try
                {
                    culture = CultureInfo.GetCultureInfo(localeCode);
                }
                catch (CultureNotFoundException)
                {
                    // The invariant culture assigned above is kept, which selects the base texts.
                }
            }

            return culture;
        }
    }
}
