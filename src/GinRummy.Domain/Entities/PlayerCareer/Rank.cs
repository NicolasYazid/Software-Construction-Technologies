using System.Collections.Generic;
using System.Linq;

namespace GinRummy.Domain.Entities.PlayerCareer
{
    public class Rank
    {
        public const int NameMaxLength = 100;

        protected Rank()
        {
            Translations = new List<RankLocale>();
        }

        public int RankId { get; private set; }

        public string Name { get; private set; }

        public int MinimumScore { get; private set; }

        public int MaximumScore { get; private set; }

        public virtual ICollection<RankLocale> Translations { get; private set; }

        public string GetNameFor(int localeId)
        {
            RankLocale translation = Translations.FirstOrDefault(candidate => candidate.LocaleId == localeId);
            string name = Name;
            if (translation != null)
            {
                name = translation.Name;
            }

            return name;
        }
    }
}
