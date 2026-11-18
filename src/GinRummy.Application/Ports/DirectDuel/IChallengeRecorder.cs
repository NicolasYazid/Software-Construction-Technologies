using GinRummy.Application.UseCases.DirectDuel;

namespace GinRummy.Application.Ports.DirectDuel
{
    public interface IChallengeRecorder
    {
        void RecordSent(PendingChallenge challenge);

        void RecordCancelled(PendingChallenge challenge);

        void RecordDeclined(PendingChallenge challenge);

        void RecordExpired(PendingChallenge challenge);

        bool RecordAccepted(PendingChallenge challenge);
    }
}
