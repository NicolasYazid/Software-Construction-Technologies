namespace GinRummy.Contracts.DataContracts
{
    /// <summary>
    /// Presence state of a friend as the client shows it. Playing is reserved for a later feature.
    /// </summary>
    public enum PresenceStatus
    {
        Offline,
        Online,
        Playing
    }
}
