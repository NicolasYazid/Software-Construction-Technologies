using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Security
{
    // Contract the game's logic uses to obtain a new verification code, without knowing how it
    // is generated.
    public interface IVerificationCodeGenerator
    {
        // Returned as a string so a leading zero is never lost.
        string GenerateCode();
    }
}
