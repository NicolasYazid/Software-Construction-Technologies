using System;
using System.Security.Cryptography;
using System.Text;

using GinRummy.Domain.Security;
using Konscious.Security.Cryptography;

namespace GinRummy.Security
{
    // Fulfils IPasswordHasher using Argon2id with the parameters OWASP recommends for interactive login.
    // Those parameters are m=19456 KiB, t=2 and p=1.
    // Hashes are read and written in the PHC string format the database already uses.
    // An example of that format is "$argon2id$v=19$m=19456,t=2,p=1$<salt>$<hash>".
    public class Argon2PasswordHasher : IPasswordHasher
    {
        private const int MemorySizeInKibibytes = 19456;
        private const int Iterations = 2;
        private const int DegreeOfParallelism = 1;
        private const int SaltSizeInBytes = 16;
        private const int HashSizeInBytes = 32;
        private const int ArgonVersion = 19;
        private const int ExpectedFieldCount = 6;
        private const int SaltFieldIndex = 4;
        private const int HashFieldIndex = 5;
        private const string AlgorithmName = "argon2id";
        private const char FieldSeparator = '$';

        public string HashPassword(string plainTextPassword)
        {
            byte[] salt = GenerateSalt();
            byte[] hash = ComputeHash(plainTextPassword, salt);
            string encodedHash = Encode(salt, hash);

            return encodedHash;
        }

        public bool VerifyPassword(string plainTextPassword, string hash)
        {
            bool passwordMatches = false;
            byte[] storedSalt;
            byte[] storedHash;
            if (TryDecode(hash, out storedSalt, out storedHash))
            {
                byte[] candidateHash = ComputeHash(plainTextPassword, storedSalt);
                passwordMatches = AreEqual(candidateHash, storedHash);
            }

            return passwordMatches;
        }

        // A new salt every time makes two players with the same password end up with different stored hashes.
        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSizeInBytes];
            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(salt);
            }

            return salt;
        }

        // The same salt and parameters must be used when a password is first hashed and every time it is verified.
        // Otherwise the output never matches, even for the correct password.
        private static byte[] ComputeHash(string plainTextPassword, byte[] salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(plainTextPassword);
            byte[] hash;
            using (Argon2id argon2Id = new Argon2id(passwordBytes))
            {
                argon2Id.Salt = salt;
                argon2Id.DegreeOfParallelism = DegreeOfParallelism;
                argon2Id.Iterations = Iterations;
                argon2Id.MemorySize = MemorySizeInKibibytes;
                hash = argon2Id.GetBytes(HashSizeInBytes);
            }

            return hash;
        }

        // The salt and the hash are packed with the parameters that produced them.
        // The result uses the same PHC string format the database already stores.
        private static string Encode(byte[] salt, byte[] hash)
        {
            string saltText = Convert.ToBase64String(salt);
            string hashText = Convert.ToBase64String(hash);
            string encodedHash = string.Format(
                "${0}$v={1}$m={2},t={3},p={4}${5}${6}",
                AlgorithmName,
                ArgonVersion,
                MemorySizeInKibibytes,
                Iterations,
                DegreeOfParallelism,
                saltText,
                hashText);

            return encodedHash;
        }

        // A stored value without the expected shape returns false instead of throwing.
        // A corrupt or unrelated value should only make the login attempt fail, not crash the sign-in screen.
        private static bool TryDecode(string encodedHash, out byte[] salt, out byte[] hash)
        {
            bool decoded = false;
            salt = null;
            hash = null;
            string[] fields = encodedHash.Split(FieldSeparator);
            if (fields.Length == ExpectedFieldCount)
            {
                try
                {
                    salt = Convert.FromBase64String(fields[SaltFieldIndex]);
                    hash = Convert.FromBase64String(fields[HashFieldIndex]);
                    decoded = true;
                }
                catch (FormatException)
                {
                    decoded = false;
                }
            }

            return decoded;
        }

        // Every byte is compared without stopping at the first difference, as a defense against timing attacks.
        // The time the comparison takes then does not depend on how many leading bytes matched.
        private static bool AreEqual(byte[] first, byte[] second)
        {
            int difference = first.Length ^ second.Length;
            int upperBound = Math.Min(first.Length, second.Length);
            for (int index = 0; index < upperBound; index++)
            {
                difference |= first[index] ^ second[index];
            }

            bool areEqual = (difference == 0);

            return areEqual;
        }
    }
}
