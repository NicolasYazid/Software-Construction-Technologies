using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Security
{
    // Contract the game's logic uses to hash and check verification codes, without knowing
    // which algorithm is behind it.
    public interface IVerificationCodeHasher
    {
        string ComputeHash(string code);

        bool VerifyCode(string code, string hash);
    }
}
