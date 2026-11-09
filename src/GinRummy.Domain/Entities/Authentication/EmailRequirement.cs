namespace GinRummy.Domain.Entities.Authentication
{
    public enum EmailRequirement
    {
        Required,
        MaximumLength,
        ValidFormat,
        NoForbiddenCharacters
    }
}
