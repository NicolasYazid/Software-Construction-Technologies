using System.Linq;

namespace GinRummy.Domain.Services
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
