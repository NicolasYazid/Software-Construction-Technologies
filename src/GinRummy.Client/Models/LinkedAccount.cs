using System;

namespace GinRummy.Client.Models
{
    public sealed class LinkedAccount
    {
        public LinkedAccount(string platformName, bool isLinked)
        {
            PlatformName = platformName ?? throw new ArgumentNullException(nameof(platformName));
            IsLinked = isLinked;
        }

        public string PlatformName { get; private set; }
        public bool IsLinked { get; private set; }
    }
}
