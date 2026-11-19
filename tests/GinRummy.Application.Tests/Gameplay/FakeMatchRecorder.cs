using System.Collections.Generic;

using GinRummy.Application.Ports.Gameplay;
using GinRummy.Application.UseCases.Gameplay;

namespace GinRummy.Application.Tests.Gameplay
{
    public class FakeMatchRecorder : IMatchRecorder
    {
        public FakeMatchRecorder()
        {
            WinnerIds = new List<int>();
        }

        public IList<int> WinnerIds { get; private set; }

        public void RecordRoundScores(ActiveMatch activeMatch)
        {
        }

        public void RecordEnd(ActiveMatch activeMatch, int winnerId)
        {
            WinnerIds.Add(winnerId);
        }

        public void RecordEndWithoutWinner(ActiveMatch activeMatch)
        {
        }
    }
}
