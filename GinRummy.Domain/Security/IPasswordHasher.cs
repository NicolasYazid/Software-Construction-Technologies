using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GinRummy.Domain.Security
{
    // Contract the game's logic uses to protect and check player passwords, without knowing
    // which hashing algorithm is behind it. A concrete adapter provides the implementation.
    public interface IPasswordHasher
    {
        string HashPassword(string plainTextPassword);

        bool VerifyPassword(string plainTextPassword, string hash);
    }
}
