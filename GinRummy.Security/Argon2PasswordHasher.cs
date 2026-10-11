using System;
using System.Security.Cryptography;
using System.Text;

using GinRummy.Domain.Security;
using Konscious.Security.Cryptography;

namespace GinRummy.Security
{
    /// <summary>
    /// Fulfils IPasswordHasher using Argon2id, with the parameters OWASP recommends
    /// for interactive login (m=19456 KiB, t=2, p=1). Reads and writes hashes in the
    /// PHC string format the database already uses, e.g.
    /// "$argon2id$v=19$m=19456,t=2,p=1$&lt;salt&gt;$&lt;hash&gt;".
    /// </summary>
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

        /// <summary>
        /// Produces a hash of the given plain text password, safe to store in place of it.
        /// </summary>
        /// <param name="plainTextPassword">The password exactly as the player typed it.</param>
        /// <returns>An encoded hash that carries its own salt and parameters.</returns>
        public string HashPassword(string plainTextPassword)
        {
            byte[] salt = GenerateSalt();
            byte[] hash = ComputeHash(plainTextPassword, salt);
            string encodedHash = Encode(salt, hash);

            return encodedHash;
        }

        /// <summary>
        /// Checks whether a plain text password matches a previously stored hash.
        /// </summary>
        /// <param name="plainTextPassword">The password exactly as the player typed it.</param>
        /// <param name="hash">The stored hash to check against.</param>
        /// <returns>True when the password matches the hash.</returns>
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

        // Builds a fresh, random salt for a new password. A new salt every time is what
        // makes two players with the same password end up with different stored hashes.
        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSizeInBytes];
            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(salt);
            }

            return salt;
        }

        // Runs Argon2id itself. The same salt and the same parameters must be used both
        // when a password is first hashed and every time it is later verified, or the
        // output will never match, even for the correct password.
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

        // Packs the salt and the hash together with the parameters used to produce them,
        // in the same PHC string format the database already stores.
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

        // Reads the salt and the hash back out of a PHC-format string. Returns false,
        // instead of throwing, when the stored value does not have the expected shape —
        // a corrupt or unrelated value should mean "this login attempt fails", not crash
        // the sign-in screen.
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

        // Compares every byte, without stopping at the first difference, so the time
        // this takes does not reveal how many leading bytes matched. See the note on
        // timing attacks above.
        private static bool AreEqual(byte[] first, byte[] second)
        {
            int difference = first.Length ^ second.Length;
            int upperBound = Math.Min(first.Length, second.Length);
            for (int index = 0; index < upperBound; index++)
            {
                difference |= first[index] ^ second[index];
            }

            return difference == 0;
        }
    }
}
