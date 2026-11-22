using System;
using System.Globalization;
using System.Security.Cryptography;

using GinRummy.Domain.Entities.AccountProtection;
using GinRummy.Domain.Ports.AccountProtection;

namespace GinRummy.Security
{
    /// <summary>
    /// Fulfils IVerificationCodeGenerator using a cryptographically secure random number,
    /// so a code cannot be predicted the way a plain Random sequence could be.
    /// </summary>
    public class RandomVerificationCodeGenerator : IVerificationCodeGenerator
    {
        private const int DecimalBase = 10;

        private static readonly long s_codeUpperBound = (long)Math.Pow(DecimalBase, VerificationCodeChecker.CodeLength);

        /// <summary>
        /// Generates a new six-digit code, as a string so a leading zero is never lost.
        /// </summary>
        /// <returns>A six-character string of digits.</returns>
        public string GenerateCode()
        {
            byte[] randomBytes = new byte[sizeof(uint)];
            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomBytes);
            }

            uint randomNumber = BitConverter.ToUInt32(randomBytes, 0);

            // A tiny bias, well under one in four billion, exists because 2^32 does not divide evenly into 1,000,000.
            // It does not matter for a six-digit code that expires in minutes and allows a limited number of attempts.
            long codeValue = randomNumber % s_codeUpperBound;
            string code = codeValue.ToString(CultureInfo.InvariantCulture).PadLeft(VerificationCodeChecker.CodeLength, '0');

            return code;
        }
    }
}
