namespace GinRummy.Client.Models
{
    // Kind of notice a challenge leaves in the lobby chat.
    public enum ChallengeNoticeKind
    {
        // The player challenged someone and waits for the answer (CU-24).
        Sent,
        // The challenged player declined the challenge of the player (CU-25).
        Declined,
        // Another player challenged the player (CU-23).
        Received
    }
}
