using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using GinRummy.Domain.Security;

namespace GinRummy.Security
{
    // Unlike password hashing, a code needs no salt or slow algorithm.
    // Its whole six-digit space is small enough to brute-force instantly either way.
    // The real defenses are the code's short expiry and its limited number of attempts, not the hash itself.
    public class Sha256VerificationCodeHasher : IVerificationCodeHasher
    {
        private const int HexCharactersPerByte = 2;
        private const string HexByteFormat = "x2";

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

        public bool VerifyCode(string code, string hash)
        {
            string candidateHash = ComputeHash(code);
            bool isCodeValid = AreEqual(candidateHash, hash);

            return isCodeValid;
        }

        // The database column stores the hash as lowercase hex text, two hex characters per byte.
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

        // Every character is compared without stopping at the first difference, as a defense against timing attacks.
        // The time the comparison takes then does not depend on how many leading characters matched.
        private static bool AreEqual(string first, string second)
        {
            int difference = first.Length ^ second.Length;
            int upperBound = Math.Min(first.Length, second.Length);
            for (int index = 0; index < upperBound; index++)
            {
                difference |= first[index] ^ second[index];
            }

            bool isEqual = (difference == 0);

            return isEqual;
        }
    }
}
