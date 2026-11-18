using GinRummy.Application.UseCases.Gameplay;

namespace GinRummy.Application.Ports.Gameplay
{
    public interface IMatchRecorder
    {
        void RecordRoundScores(ActiveMatch activeMatch);

        void RecordEnd(ActiveMatch activeMatch, int winnerId);
    }
}
