using System;
using System.Globalization;
using System.Security.Cryptography;

using GinRummy.Domain.Security;

namespace GinRummy.Security
{
    // A cryptographically secure generator is used instead of Random, whose sequence can be predicted.
    public class RandomVerificationCodeGenerator : IVerificationCodeGenerator
    {
        private const int CodeLength = 6;
        private const long CodeUpperBound = 1000000;
        private const char PaddingDigit = '0';

        public string GenerateCode()
        {
            byte[] randomBytes = new byte[sizeof(uint)];
            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomBytes);
            }

            uint randomNumber = BitConverter.ToUInt32(randomBytes, 0);

            // A tiny bias, well under one in four billion, exists because 2^32 is not a multiple of 1,000,000.
            // It does not matter for a six-digit code that expires in minutes and is rate-limited by Attempts.
            long codeValue = randomNumber % CodeUpperBound;
            string code = codeValue.ToString(CultureInfo.InvariantCulture).PadLeft(CodeLength, PaddingDigit);

            return code;
        }
    }
}
