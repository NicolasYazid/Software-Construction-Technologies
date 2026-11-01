using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using GinRummy.Domain.Ports.Security;
using GinRummy.Domain.Security;
using Konscious.Security.Cryptography;

namespace GinRummy.Security
{
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

        public string HashPassword(Password password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            char[] characters = password.ReadOnce();
            byte[] passwordBytes = Encoding.UTF8.GetBytes(characters);
            byte[] salt = GenerateSalt();
            string encodedHash;
            try
            {
                byte[] hash = ComputeHash(passwordBytes, salt);
                encodedHash = Encode(salt, hash);
            }
            finally
            {
                Array.Clear(characters, 0, characters.Length);
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
            }

            return encodedHash;
        }

        public bool VerifyPassword(string plainTextPassword, string hash)
        {
            bool isPasswordValid = false;
            byte[] storedSalt;
            byte[] storedHash;
            if (TryDecode(hash, out storedSalt, out storedHash))
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(plainTextPassword);
                byte[] candidateHash = ComputeHash(passwordBytes, storedSalt);
                isPasswordValid = AreEqual(candidateHash, storedHash);
            }

            return isPasswordValid;
        }

        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[SaltSizeInBytes];
            using (RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(salt);
            }

            return salt;
        }

        private static byte[] ComputeHash(byte[] passwordBytes, byte[] salt)
        {
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

        private static string Encode(byte[] salt, byte[] hash)
        {
            string saltText = Convert.ToBase64String(salt);
            string hashText = Convert.ToBase64String(hash);
            string encodedHash = string.Format(
                CultureInfo.InvariantCulture,
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

        private static bool TryDecode(string encodedHash, out byte[] salt, out byte[] hash)
        {
            bool isDecoded = false;
            salt = null;
            hash = null;
            string[] fields = encodedHash.Split(FieldSeparator);
            if (fields.Length == ExpectedFieldCount)
            {
                try
                {
                    salt = Convert.FromBase64String(fields[SaltFieldIndex]);
                    hash = Convert.FromBase64String(fields[HashFieldIndex]);
                    isDecoded = true;
                }
                catch (FormatException)
                {
                    isDecoded = false;
                }
            }

            return isDecoded;
        }

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
