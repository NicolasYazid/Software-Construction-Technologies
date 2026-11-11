using System.ComponentModel;

namespace GinRummy.Client.Models
{
    public sealed class Card : INotifyPropertyChanged
    {
        private bool _isInMeld;

        public Card(CardRank rank, CardSuit suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CardRank Rank { get; private set; }
        public CardSuit Suit { get; private set; }

        // The hand keeps the same card while the melds change around it, so the mark must notify the face that draws it.
        public bool IsInMeld
        {
            get { return _isInMeld; }
            set
            {
                if (_isInMeld != value)
                {
                    _isInMeld = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInMeld)));
                }
            }
        }
    }
}
