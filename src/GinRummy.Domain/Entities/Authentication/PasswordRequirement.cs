namespace GinRummy.Domain.Entities.Authentication
{
    public enum PasswordRequirement
    {
        MinimumLength,
        MaximumLength,
        ContainsLetter,
        ContainsDigit,
        NoForbiddenCharacters
    }
}
