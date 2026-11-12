namespace GinRummy.Domain.Ports.PlayerPreferences
{
    public interface IPlayerPreferencesRepository
    {
        Entities.PlayerPreferences.PlayerPreferences FindById(int playerAccountId);

        void Add(Entities.PlayerPreferences.PlayerPreferences newPlayerPreferences);
    }
}