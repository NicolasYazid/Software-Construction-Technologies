using System.Collections.Generic;

using GinRummy.Domain.Entities.PlayerModeration;

namespace GinRummy.Domain.Ports.PlayerModeration
{
    public interface IModerationRepository
    {
        ReasonBan FindReasonBanById(int reasonBanId);

        void AddWarning(Warning newWarning);

        void AddReport(Report newReport);

        void AddBan(Ban newBan);

        void AssociateWarningWithBan(int warningId, int banId);

        IList<Ban> GetBansOf(int playerAccountId);

        IList<Warning> GetWarningsOf(int playerAccountId);

        IList<ProhibitedWord> GetProhibitedWords();
    }
}
