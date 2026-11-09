using System.Linq;

namespace GinRummy.Domain.Shared
{
    /// <summary>
    /// Validates on the server the fields a player types, because the checks the client makes for comfort
    /// can be skipped by a modified client. Every field is checked for emptiness before any is checked for
    /// forbidden characters, so an empty field is always the error the player sees first (CU-02 step 4).
    /// </summary>
    public static class TypedFieldValidator
    {
        /// <summary>
        /// Checks that every field holds at least one character.
        /// </summary>
        /// <param name="fieldValues">Values exactly as the player typed them.</param>
        /// <exception cref="RequiredFieldMissingException">Thrown when a field is null or empty.</exception>
        public static void EnsureAreTyped(params string[] fieldValues)
        {
            if (fieldValues.Any(fieldValue => string.IsNullOrEmpty(fieldValue)))
            {
                throw new RequiredFieldMissingException("A required field arrived empty.");
            }
        }

        /// <summary>
        /// Checks that a field fits the column that stores it.
        /// </summary>
        /// <param name="fieldValue">Value as it will be stored; null counts as empty.</param>
        /// <param name="maxLength">Length of the column.</param>
        /// <exception cref="FieldTooLongException">Thrown when the field is longer than the column.</exception>
        public static void EnsureIsNotLongerThan(string fieldValue, int maxLength)
        {
            if ((fieldValue != null) && (fieldValue.Length > maxLength))
            {
                throw new FieldTooLongException("A typed field is longer than its column.");
            }
        }

        /// <summary>
        /// Checks that no field contains a character the game does not accept.
        /// </summary>
        /// <param name="fieldValues">Values exactly as the player typed them.</param>
        /// <exception cref="DisallowedCharactersException">Thrown when a field contains a forbidden character.</exception>
        public static void EnsureHaveNoForbiddenCharacters(params string[] fieldValues)
        {
            if (fieldValues.Any(fieldValue => ForbiddenCharacterCommon.ContainsForbiddenCharacters(fieldValue)))
            {
                throw new DisallowedCharactersException("A typed field contains characters that are not allowed.");
            }
        }
    }
}
