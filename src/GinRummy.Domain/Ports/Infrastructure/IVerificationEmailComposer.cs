using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Ports.Infrastructure
{
    /// <summary>
    /// Contract the game's logic uses to write the email that carries a verification code, in the language
    /// of the player, without knowing where the texts are kept.
    /// </summary>
    public interface IVerificationEmailComposer
    {
        /// <summary>
        /// Writes the subject of the email.
        /// </summary>
        /// <param name="purpose">Operation the code confirms.</param>
        /// <param name="localeCode">Locale of the player, such as es-MX; the default language is used when it is unknown.</param>
        /// <returns>The translated subject.</returns>
        string ComposeSubject(VerificationPurpose purpose, string localeCode);

        /// <summary>
        /// Writes the body of the email with the code in it.
        /// </summary>
        /// <param name="purpose">Operation the code confirms.</param>
        /// <param name="plainCode">Plain value of the code.</param>
        /// <param name="localeCode">Locale of the player, such as es-MX; the default language is used when it is unknown.</param>
        /// <returns>The translated body.</returns>
        string ComposeBody(VerificationPurpose purpose, string plainCode, string localeCode);
    }
}
