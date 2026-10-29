using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Security
{
    /// <summary>
    /// Contract the game's logic uses to obtain a new verification code, without knowing
    /// how it is generated.
    /// </summary>
    public interface IVerificationCodeGenerator
    {
        /// <summary>
        /// Generates a new six-digit code, as a string so a leading zero is never lost.
        /// </summary>
        /// <returns>A six-character string of digits.</returns>
        string GenerateCode();
    }
}
