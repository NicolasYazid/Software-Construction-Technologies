namespace GinRummy.Domain.Security
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
