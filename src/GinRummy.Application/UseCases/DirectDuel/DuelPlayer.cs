namespace GinRummy.Application.UseCases.DirectDuel
{
    public class DuelPlayer
    {
        public DuelPlayer(int id, string name, int publicTag)
        {
            Id = id;
            Name = name;
            PublicTag = publicTag;
        }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public int PublicTag { get; private set; }
    }
}
