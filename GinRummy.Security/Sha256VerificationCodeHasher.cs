using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Repositories;

namespace GinRummy.Security
{
    /// <summary>
    /// Fulfils IVerificationCodeHasher using SHA-256. Unlike password hashing, a code
    /// needs no salt or slow algorithm: its whole six-digit space is small enough to
    /// brute-force instantly either way, so the real defenses are the code's short
    /// expiry and its limited number of attempts, not the hash itself.
    /// </summary>
    public class Sha256VerificationCodeHasher : IVerificationCodeHasher
    {
        /// <summary>
        /// Produces a hash of the given code, safe to store in place of it.
        /// </summary>
        /// <param name="code">The six-digit code exactly as generated.</param>
        /// <returns>A 64-character hexadecimal hash.</returns>
        public string ComputeHash(string code)
        {
            string hexHash;
            byte[] codeBytes = Encoding.UTF8.GetBytes(code);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(codeBytes);
                hexHash = ToHexString(hashBytes);
            }

            return hexHash;
        }

        /// <summary>
        /// Checks whether a code matches a previously stored hash.
        /// </summary>
        /// <param name="code">The six-digit code exactly as the player typed it.</param>
        /// <param name="hash">The stored hash to check against.</param>
        /// <returns>True when the code matches the hash.</returns>
        public bool VerifyCode(string code, string hash)
        {
            string candidateHash = ComputeHash(code);
            bool codeMatches = AreEqual(candidateHash, hash);

            return codeMatches;
        }

        // Turns raw hash bytes into the same lowercase hex text the database column
        // stores, two hex characters per byte.
        private static string ToHexString(byte[] bytes)
        {
            StringBuilder hexBuilder = new StringBuilder(bytes.Length * 2);
            foreach (byte currentByte in bytes)
            {
                hexBuilder.Append(currentByte.ToString("x2", CultureInfo.InvariantCulture));
            }

            string hexText = hexBuilder.ToString();

            return hexText;
        }

        // Compares every character without stopping at the first difference, for the
        // same timing-attack reason already explained for passwords.
        private static bool AreEqual(string first, string second)
        {
            int difference = first.Length ^ second.Length;
            int upperBound = Math.Min(first.Length, second.Length);
            for (int index = 0; index < upperBound; index++)
            {
                difference |= first[index] ^ second[index];
            }

            return difference == 0;
        }
    }
}
