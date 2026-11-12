using System.Text.RegularExpressions;

namespace GinRummy.Domain.Shared
{
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
