using GinRummy.Application.Ports.DirectDuel;
using GinRummy.Application.UseCases.DirectDuel;

namespace GinRummy.Application.Tests.DirectDuel
{
    public class FakeChallengeRecorder : IChallengeRecorder
    {
        public bool IsChallengeExpired { get; set; }

        public void RecordSent(PendingChallenge challenge)
        {
        }

        public void RecordCancelled(PendingChallenge challenge)
        {
        }

        public void RecordDeclined(PendingChallenge challenge)
        {
        }

        public void RecordExpired(PendingChallenge challenge)
        {
        }

        public bool RecordAccepted(PendingChallenge challenge)
        {
            return !IsChallengeExpired;
        }
    }
}
