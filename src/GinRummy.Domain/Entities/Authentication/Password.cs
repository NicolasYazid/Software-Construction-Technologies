using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Domain.Shared;

namespace GinRummy.Domain.Entities.Authentication
{
    public sealed class Password
    {
        public const int MinimumLength = 8;
        public const int MaximumLength = 128;
        private const string MaskedValue = "********";
        private readonly char[] _characters;
        private bool _hasBeenRead;

        public Password(string plainText)
        {
            IList<PasswordRequirement> unmetRequirements = GetUnmetRequirements(plainText);

            if (unmetRequirements.Count > 0)
            {
                throw new ArgumentException(
                    "The password does not meet these requirements: " + string.Join(", ", unmetRequirements) + ".",
                    nameof(plainText));
            }

            _characters = plainText.ToCharArray();
        }

        public static IList<PasswordRequirement> GetUnmetRequirements(string plainText)
        {
            string text = plainText ?? string.Empty;
            List<PasswordRequirement> unmetRequirements = new List<PasswordRequirement>();

            if (text.Length < MinimumLength)
            {
                unmetRequirements.Add(PasswordRequirement.MinimumLength);
            }

            if (text.Length > MaximumLength)
            {
                unmetRequirements.Add(PasswordRequirement.MaximumLength);
            }

            if (!text.Any(character => char.IsLetter(character)))
            {
                unmetRequirements.Add(PasswordRequirement.ContainsLetter);
            }

            if (!text.Any(character => char.IsDigit(character)))
            {
                unmetRequirements.Add(PasswordRequirement.ContainsDigit);
            }

            if (ForbiddenCharacterCommon.ContainsForbiddenCharacters(text))
            {
                unmetRequirements.Add(PasswordRequirement.NoForbiddenCharacters);
            }

            return unmetRequirements;
        }

        public char[] ReadOnce()
        {
            if (_hasBeenRead)
            {
                throw new InvalidOperationException("The password was already read.");
            }

            char[] characters = (char[])_characters.Clone();
            Array.Clear(_characters, 0, _characters.Length);
            _hasBeenRead = true;

            return characters;
        }

        public override string ToString()
        {
            return MaskedValue;
        }
    }
}
