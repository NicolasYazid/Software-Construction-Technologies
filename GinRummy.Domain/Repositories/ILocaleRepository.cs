using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Repositories
{
    public interface ILocaleRepository
    {
        Locale FindByCode(string localeCode);
    }
}
