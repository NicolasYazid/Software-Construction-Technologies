using System;

namespace GinRummy.Domain.Entities.AccountProtection
{
    public class IssuedVerificationCode
    {
        public IssuedVerificationCode(VerificationCode storedCode, string plainCode)
        {
            StoredCode = storedCode ?? throw new ArgumentNullException(nameof(storedCode));
            PlainCode = plainCode ?? throw new ArgumentNullException(nameof(plainCode));
        }

        public VerificationCode StoredCode { get; private set; }

        public string PlainCode { get; private set; }
    }
}
