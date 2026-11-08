using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    /// <summary>
    /// Finds the prohibited words of a text. The chat and the reports use it, so a message counts as offensive by
    /// the same list wherever it is checked (CU-17 step 7).
    /// </summary>
    public static class ProhibitedWordDetector
    {
        private const char WordSeparator = ' ';

        // A letter written as a look-alike digit or symbol does not hide a prohibited word.
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

        /// <summary>
        /// Tells whether a text contains one of the prohibited terms as a whole word. Upper case, accents, look-alike
        /// digits and symbols, and repeated letters do not hide a term.
        /// </summary>
        /// <param name="text">Text as the player wrote it.</param>
        /// <param name="prohibitedTerms">Prohibited terms of the language of the player.</param>
        /// <returns>True when at least one word of the text is a prohibited term.</returns>
        public static bool ContainsProhibitedWord(string text, IEnumerable<string> prohibitedTerms)
        {
            HashSet<string> words = new HashSet<string>(Normalize(text).Split(WordSeparator));
            bool containsProhibitedWord = prohibitedTerms.Any(term => words.Contains(Normalize(term)));

            return containsProhibitedWord;
        }

        // The term and the text go through the same steps, so both end in the same form when they are the same word.
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
