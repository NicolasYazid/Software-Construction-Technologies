using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Security
{
    /// <summary>
    /// Contract the game's logic uses to protect and check player passwords, without
    /// knowing which hashing algorithm is behind it. A concrete adapter provides the
    /// implementation.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Produces a hash of the given plain text password, safe to store in place of it.
        /// </summary>
        /// <param name="plainTextPassword">The password exactly as the player typed it.</param>
        /// <returns>An encoded hash that carries its own salt and parameters.</returns>
        string HashPassword(string plainTextPassword);

        /// <summary>
        /// Checks whether a plain text password matches a previously stored hash.
        /// </summary>
        /// <param name="plainTextPassword">The password exactly as the player typed it.</param>
        /// <param name="hash">The stored hash to check against.</param>
        /// <returns>True when the password matches the hash.</returns>
        bool VerifyPassword(string plainTextPassword, string hash);
    }
}
