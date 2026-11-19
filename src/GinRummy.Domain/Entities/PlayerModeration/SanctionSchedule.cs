using System;

namespace GinRummy.Domain.Entities.PlayerModeration
{
    public class SanctionSchedule
    {
        private SanctionSchedule(DateTime createdAt, DateTime startsAt)
        {
            CreatedAt = createdAt;
            StartsAt = startsAt;
            EndsAt = startsAt.AddMinutes(Sanction.DurationMinutes);
        }

        public DateTime CreatedAt { get; private set; }
        public DateTime StartsAt { get; private set; }
        public DateTime EndsAt { get; private set; }

        public static SanctionSchedule StartingAfter(DateTime createdAt, Sanction latestSanction)
        {
            DateTime startsAt = createdAt;
            if ((latestSanction != null) && (latestSanction.EndsAt > createdAt))
            {
                startsAt = latestSanction.EndsAt;
            }

            return new SanctionSchedule(createdAt, startsAt);
        }
    }
}
