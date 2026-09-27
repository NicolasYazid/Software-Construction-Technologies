using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Client.Controllers
{
    /// <summary>
    /// Outcome of trying to create an account: either the generated verification code,
    /// or the localization key of the message to show.
    /// </summary>
    public class SignUpResult
    {
        private SignUpResult(bool succeeded, string errorMessageKey, string generatedCode)
        {
            Succeeded = succeeded;
            ErrorMessageKey = errorMessageKey;
            GeneratedCode = generatedCode;
        }

        /// <summary>
        /// Gets whether the account was created.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// Gets the localization key of the error to show, or null on success.
        /// </summary>
        public string ErrorMessageKey { get; }

        /// <summary>
        /// Gets the six-digit code the player must verify, or null on failure.
        /// </summary>
        public string GeneratedCode { get; }

        /// <summary>
        /// Builds a successful result carrying the generated code.
        /// </summary>
        /// <param name="generatedCode">The six-digit code shown to the player.</param>
        /// <returns>A successful result.</returns>
        public static SignUpResult Success(string generatedCode)
        {
            return new SignUpResult(true, null, generatedCode);
        }

        /// <summary>
        /// Builds a failed result carrying which message to show.
        /// </summary>
        /// <param name="errorMessageKey">Localization key of the message to show.</param>
        /// <returns>A failed result.</returns>
        public static SignUpResult Failure(string errorMessageKey)
        {
            return new SignUpResult(false, errorMessageKey, null);
        }
    }
}
