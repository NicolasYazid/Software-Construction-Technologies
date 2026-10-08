using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    public sealed class MeldDto
    {
        public MeldKind Kind { get; set; }
        public IList<CardDto> Cards { get; set; }
    }
}
