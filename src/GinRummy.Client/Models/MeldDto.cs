using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    // Group of cards of a hand laid down when the hand closes, or the loose cards left out of every group.
    public sealed class MeldDto
    {
        public MeldKind Kind { get; set; }
        public IList<CardDto> Cards { get; set; }
    }
}
