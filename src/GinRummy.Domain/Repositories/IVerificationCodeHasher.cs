using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to hash and check verification codes, without
    /// knowing which algorithm is behind it.
    /// </summary>
    public interface IVerificationCodeHasher
    {

        /// <summary>
        /// Produces a hash of the given code, safe to store in place of it.
        /// </summary>
        /// <param name="code">The six-digit code exactly as generated.</param>
        /// <returns>A 64-character hexadecimal hash.</returns>
        string ComputeHash(string code);

        /// <summary>
        /// Checks whether a code matches a previously stored hash.
        /// </summary>
        /// <param name="code">The six-digit code exactly as the player typed it.</param>
        /// <param name="hash">The stored hash to check against.</param>
        /// <returns>True when the code matches the hash.</returns>
        bool VerifyCode(string code, string hash);
    }
}
