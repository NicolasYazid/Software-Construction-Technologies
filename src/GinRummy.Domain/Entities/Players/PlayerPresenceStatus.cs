namespace GinRummy.Domain.Entities.Players
{
    [DataContract]
    public enum PlayerPresenceStatus
    {
        [DataMember]
        Offline,
        [DataMember]
        Online
    }
}
