using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using GinRummy.Domain.Ports.Security;

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
        private const int HexCharactersPerByte = 2;
        private const string HexByteFormat = "x2";

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
            bool isCodeValid = AreEqual(candidateHash, hash);

            return isCodeValid;
        }

        // The text must be the same lowercase hexadecimal form the database column stores.
        private static string ToHexString(byte[] bytes)
        {
            StringBuilder hexBuilder = new StringBuilder(bytes.Length * HexCharactersPerByte);
            foreach (byte currentByte in bytes)
            {
                hexBuilder.Append(currentByte.ToString(HexByteFormat, CultureInfo.InvariantCulture));
            }

            string hexText = hexBuilder.ToString();

            return hexText;
        }

        // Every character is compared without stopping at the first difference.
        // The time the comparison takes therefore does not reveal how many leading characters matched.
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
