using System;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// A verification code right after it was issued: the stored code, which only keeps its hash, and the
    /// plain value that has to reach the player. The plain value exists only in memory, while it is sent.
    /// </summary>
    public class IssuedVerificationCode
    {
        /// <summary>
        /// Builds an issued code.
        /// </summary>
        /// <param name="storedCode">Code as it was stored, with its hash.</param>
        /// <param name="plainCode">Plain value of the code, to deliver to the player.</param>
        /// <exception cref="ArgumentNullException">Thrown when the stored code or the plain value is null.</exception>
        public IssuedVerificationCode(VerificationCode storedCode, string plainCode)
        {
            StoredCode = storedCode ?? throw new ArgumentNullException(nameof(storedCode));
            PlainCode = plainCode ?? throw new ArgumentNullException(nameof(plainCode));
        }

        /// <summary>
        /// Gets the code as it was stored, with its hash.
        /// </summary>
        public VerificationCode StoredCode { get; private set; }

        /// <summary>
        /// Gets the plain value of the code.
        /// </summary>
        public string PlainCode { get; private set; }
    }
}
