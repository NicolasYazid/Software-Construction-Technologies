using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Daos
{
    public interface ILocaleDao
    {
        Locale FindByCode(string localeCode);
    }
}
