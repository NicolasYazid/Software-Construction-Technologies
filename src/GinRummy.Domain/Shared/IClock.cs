using System;

namespace GinRummy.Domain.Shared
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}
