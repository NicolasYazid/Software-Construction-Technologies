using System.Collections.Generic;

namespace GinRummy.Client.Models
{
    /// <summary>
    /// Group of cards of a hand laid down when the hand closes, or the loose cards left out of
    /// every group.
    /// </summary>
    public sealed class MeldDto
    {
        /// <summary>
        /// Gets or sets the kind of the group.
        /// </summary>
        public MeldKind Kind { get; set; }
        /// <summary>
        /// Gets or sets the cards of the group, in the order they are laid down.
        /// </summary>
        public IList<CardDto> Cards { get; set; }
    }
}
