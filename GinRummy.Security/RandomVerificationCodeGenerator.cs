using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Security;

namespace GinRummy.Security
{
    // Fulfils IVerificationCodeGenerator using a cryptographically secure random number, so a
    // code cannot be predicted the way a plain Random sequence could be.
    public class RandomVerificationCodeGenerator : IVerificationCodeGenerator
    {
        private const int CodeLength = 6;
        private const long CodeUpperBound = 1000000;

        // Returned as a string so a leading zero is never lost.
        public string GenerateCode()
        {
            byte[] randomBytes = new byte[sizeof(uint)];
            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomBytes);
            }

            uint randomNumber = BitConverter.ToUInt32(randomBytes, 0);

            // A tiny bias (well under one in four billion) exists because 2^32 does not
            // divide evenly into 1,000,000. It does not matter for a six-digit code that
            // expires in minutes and is rate-limited by Attempts.
            long codeValue = randomNumber % CodeUpperBound;
            string code = codeValue.ToString(CultureInfo.InvariantCulture).PadLeft(CodeLength, '0');

            return code;
        }
    }
}
