using System.Collections.Generic;
using System.Linq;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    public class ProhibitedWord
    {
        public const int TermMaxLength = 100;

        protected ProhibitedWord()
        {
            Locales = new List<ProhibitedWordLocale>();
        }

        public int ProhibitedWordId { get; private set; }

        public string Term { get; private set; }

        public virtual ICollection<ProhibitedWordLocale> Locales { get; private set; }

        public bool AppliesTo(int localeId)
        {
            bool isProhibitedInLocale = Locales.Any(link => link.LocaleId == localeId);

            return isProhibitedInLocale;
        }
    }
}
