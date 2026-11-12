using System.Collections.Generic;

using GinRummy.Domain.Entities.PlayerPreferences;

namespace GinRummy.Domain.Ports.PlayerPreferences
{
    public interface ILocaleRepository
    {
        Locale FindByCode(string localeCode);

        Locale FindById(int localeId);

        Locale FindDefault();

        IList<Locale> GetAll();
    }
}
