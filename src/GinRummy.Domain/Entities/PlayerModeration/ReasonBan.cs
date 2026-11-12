using System;
using System.Collections.Generic;
using System.Linq;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    public class ReasonBan
    {
        public const int NameMaxLength = 100;

        public const int RestrictionScopeNameMaxLength = 20;

        protected ReasonBan()
        {
            Translations = new List<ReasonBanLocale>();
        }

        public int ReasonBanId { get; private set; }

        public string Name { get; private set; }

        public string RestrictionScopeName { get; private set; }

        public virtual ICollection<ReasonBanLocale> Translations { get; private set; }

        public RestrictionScope RestrictionScope
        {
            get { return (RestrictionScope)Enum.Parse(typeof(RestrictionScope), RestrictionScopeName); }
        }

        public bool IsChatRestricted
        {
            get
            {
                bool isChatOnly = RestrictionScope == RestrictionScope.ChatOnly;
                bool isChatRestricted = isChatOnly || IsAccountRestricted;

                return isChatRestricted;
            }
        }

        public bool IsAccountRestricted
        {
            get { return RestrictionScope == RestrictionScope.FullAccount; }
        }

        public string GetNameFor(int localeId)
        {
            ReasonBanLocale translation = Translations.FirstOrDefault(candidate => candidate.LocaleId == localeId);
            string name = Name;
            if (translation != null)
            {
                name = translation.Name;
            }

            return name;
        }
    }
}
