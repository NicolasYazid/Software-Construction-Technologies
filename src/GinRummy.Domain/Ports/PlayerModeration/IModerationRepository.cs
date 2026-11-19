using System;
using System.Collections.Generic;

using GinRummy.Domain.Entities.PlayerModeration;

namespace GinRummy.Domain.Ports.PlayerModeration
{
    public interface IModerationRepository
    {
        void AddReport(Report newReport);

        void AddWarning(Warning newWarning);

        void AddSanction(Sanction newSanction);

        void AddPermanentBan(PermanentBan newPermanentBan);

        int CountWarningsOf(int playerAccountId);

        int CountSanctionsOf(int playerAccountId);

        IList<Sanction> GetUnfinishedSanctionsOf(int playerAccountId, DateTime moment);

        IList<Warning> GetWarningsOf(int playerAccountId);

        bool IsPermanentlyBanned(int playerAccountId);

        IList<int> GetPermanentlyBannedPlayerIds();

        IList<ProhibitedWord> GetProhibitedWords();
    }
}
