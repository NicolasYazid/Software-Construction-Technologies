using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Application.Ports
{
    /// <summary>
    /// Entry point of the creation of an account (CU-01). The network boundary calls it without knowing which
    /// use case answers.
    /// </summary>
    public interface ISignUpPort
    {
        /// <summary>
        /// Creates an account with what the person typed and emails the code that verifies its email.
        /// </summary>
        /// <param name="form">What the person typed in the sign-up screen.</param>
        /// <param name="localeCode">Culture code of the language the client was showing, such as es-MX.</param>
        /// <returns>The created account and whether the email with the code was accepted.</returns>
        SignUpResult SignUp(SignUpForm form, string localeCode);
    }
}
