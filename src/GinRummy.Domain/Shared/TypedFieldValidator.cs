using System.Linq;

namespace GinRummy.Domain.Shared
{
    public static class TypedFieldValidator
    {
        public static bool AreTyped(params string[] fieldValues)
        {
            return fieldValues.All(fieldValue => !string.IsNullOrEmpty(fieldValue)) ? true : false;
        }

        public static bool HaveForbiddenCharacters(params string[] fieldValues)
        {
            return fieldValues.Any(fieldValue => ForbiddenCharacterCommon.ContainsForbiddenCharacters(fieldValue)) ? true : false;
        }

        public static void EnsureAreTyped(params string[] fieldValues)
        {
            if (!AreTyped(fieldValues))
            {
                throw new RequiredFieldMissingException("A required field arrived empty.");
            }
        }

        public static void EnsureIsNotLongerThan(string fieldValue, int maxLength)
        {
            if ((fieldValue != null) && (fieldValue.Length > maxLength))
            {
                throw new FieldTooLongException("A typed field is longer than its column.");
            }
        }

        public static void EnsureHaveNoForbiddenCharacters(params string[] fieldValues)
        {
            if (HaveForbiddenCharacters(fieldValues))
            {
                throw new DisallowedCharactersException("A typed field contains characters that are not allowed.");
            }
        }
    }
}
