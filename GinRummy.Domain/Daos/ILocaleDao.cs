using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Daos
{
    // Contract the game's logic uses to reach the Locale catalog.
    // The logic does not need to know how or where the catalog is stored.
    public interface ILocaleDao
    {
        Locale FindByCode(string localeCode);
    }
}
