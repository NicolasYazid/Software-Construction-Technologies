using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    public sealed class Meld
    {
        public MeldKind Kind { get; set; }
        public IList<Card> Cards { get; set; }
    }
}
