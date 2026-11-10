namespace GinRummy.Contracts.DataContracts.PlayerPresence
{
    [DataContract]
    public enum PresenceStatus
    {
        [DataMember]
        Offline,

        [DataMember]
        Online
    }
}
