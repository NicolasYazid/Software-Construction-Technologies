using System;
using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    public sealed class Meld
    {
        public Meld(MeldKind kind, IList<Card> cards)
        {
            Kind = kind;
            Cards = cards ?? throw new ArgumentNullException(nameof(cards));
        }

        public MeldKind Kind { get; private set; }
        public IList<Card> Cards { get; private set; }
    }
}
