using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    public static class ProhibitedWordDetector
    {
        private const char WordSeparator = ' ';

        private static readonly Dictionary<char, char> _lookAlikeLetters = new Dictionary<char, char>
        {
            { '4', 'a' },
            { '@', 'a' },
            { '3', 'e' },
            { '1', 'i' },
            { '0', 'o' },
            { '5', 's' },
            { '$', 's' },
            { '7', 't' }
        };

        public static bool ContainsProhibitedWord(string text, IEnumerable<string> prohibitedTerms)
        {
            HashSet<string> words = new HashSet<string>(Normalize(text).Split(WordSeparator));
            bool containsProhibitedWord = prohibitedTerms.Any(term => words.Contains(Normalize(term)));

            return containsProhibitedWord;
        }

        private static string Normalize(string text)
        {
            string decomposed = (text ?? string.Empty).ToLowerInvariant().Normalize(NormalizationForm.FormD);
            IEnumerable<char> letters = decomposed
                .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                .Select(ToLetter);
            StringBuilder normalized = new StringBuilder(decomposed.Length);
            char previous = WordSeparator;
            foreach (char letter in letters)
            {
                if (letter != previous)
                {
                    normalized.Append(letter);
                    previous = letter;
                }
            }

            return normalized.ToString().Trim();
        }

        private static char ToLetter(char character)
        {
            char letter;
            if (!_lookAlikeLetters.TryGetValue(character, out letter))
            {
                letter = char.IsLetterOrDigit(character) ? character : WordSeparator;
            }

            return letter;
        }
    }
}
