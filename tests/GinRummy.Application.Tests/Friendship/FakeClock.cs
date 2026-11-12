using System;

using GinRummy.Domain.Shared;

namespace GinRummy.Application.Tests.Friendship
{
    public class FakeClock : IClock
    {
        private DateTime _currentMoment;

        public FakeClock(DateTime startMoment)
        {
            _currentMoment = startMoment;
        }

        public DateTime UtcNow
        {
            get { return _currentMoment; }
        }

        public void MoveForward(TimeSpan elapsedTime)
        {
            _currentMoment = _currentMoment.Add(elapsedTime);
        }
    }
}
