using System.Text.RegularExpressions;

namespace GinRummy.Domain.Services
{
    /// <summary>
    /// Decides whether a text contains characters that the game does not accept in any typed field:
    /// emojis, other symbols and line breaks. The username, the email and the password share this rule.
    /// </summary>
    public static class ForbiddenCharacterCommon
    {
        private const string ForbiddenCharacterPattern = @"[\p{So}\uD800-\uDFFF\r\n]";

        private static readonly Regex _forbiddenCharactersRegex = new Regex(ForbiddenCharacterPattern);

        public static bool ContainsForbiddenCharacters(string text)
        {
            bool hasForbiddenCharacters = false;

            if (text != null)
            {
                hasForbiddenCharacters = _forbiddenCharactersRegex.IsMatch(text);
            }

            return hasForbiddenCharacters;
        }
    }
}
