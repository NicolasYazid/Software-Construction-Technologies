using System;
using System.Collections.Generic;
using System.Linq;

namespace GinRummy.Domain
{
    public class GameRound
    {
        private readonly int _dealerId;
        private readonly int _nonDealerId;
        private readonly Dictionary<int, List<Card>> _hands;
        private readonly Stack<Card> _stock;
        private readonly Stack<Card> _discardPile;
        private bool _isDiscardPileClosed;
        private KnockedHand _knockedHand;

        public GameRound(int dealerId, int nonDealerId, IList<Card> shuffledDeck)
        {
            if (shuffledDeck == null)
            {
                throw new ArgumentNullException(nameof(shuffledDeck));
            }

            if (dealerId == nonDealerId)
            {
                throw new ArgumentException("The dealer and the other player must be different.", nameof(nonDealerId));
            }

            if (shuffledDeck.Distinct().Count() != StandardDeck.CardCount)
            {
                throw new ArgumentException("The deck must have the 52 different cards.", nameof(shuffledDeck));
            }

            _dealerId = dealerId;
            _nonDealerId = nonDealerId;
            _hands = new Dictionary<int, List<Card>> { { dealerId, new List<Card>() }, { nonDealerId, new List<Card>() } };
            Queue<Card> deck = new Queue<Card>(shuffledDeck);
            Deal(deck);
            _discardPile = new Stack<Card>();
            _discardPile.Push(deck.Dequeue());

            _stock = new Stack<Card>(deck.Reverse());
            CurrentPlayerId = nonDealerId;
            Phase = TurnPhase.FirstCardOfferToNonDealer;
        }

        public int DealerId
        {
            get { return _dealerId; }
        }

        public int NonDealerId
        {
            get { return _nonDealerId; }
        }

        public int CurrentPlayerId { get; private set; }
        public TurnPhase Phase { get; private set; }
        public RoundResult Result { get; private set; }
        public Card CardTakenFromDiscardPile { get; private set; }

        public int DefenderDeadwood
        {
            get { return _knockedHand.DefenderDeadwood; }
        }

        public IList<IList<Card>> KnockerMelds
        {
            get { return _knockedHand.Melds; }
        }

        public IReadOnlyList<Card> DefenderUnmatchedCards
        {
            get { return _knockedHand.DefenderUnmatchedCards; }
        }

        public int StockCount
        {
            get { return _stock.Count; }
        }

        public Card TopOfDiscardPile
        {
            get
            {
                Card topCard = null;
                if (_discardPile.Count > 0)
                {
                    topCard = _discardPile.Peek();
                }

                return topCard;
            }
        }

        public IReadOnlyList<Card> GetHand(int playerId)
        {
            return FindHand(playerId).AsReadOnly();
        }

        public void TakeFirstCard(int playerId)
        {
            EnsureFirstCardOffer(playerId);
            TakeTopOfDiscardPile(playerId);
        }

        public void PassFirstCard(int playerId)
        {
            EnsureFirstCardOffer(playerId);
            if (Phase == TurnPhase.FirstCardOfferToNonDealer)
            {
                Phase = TurnPhase.FirstCardOfferToDealer;
                CurrentPlayerId = _dealerId;
            }
            else
            {
                _isDiscardPileClosed = true;
                Phase = TurnPhase.Draw;
                CurrentPlayerId = _nonDealerId;
            }
        }

        public void DrawFromStock(int playerId)
        {
            EnsureTurn(playerId, TurnPhase.Draw);
            FindHand(playerId).Add(_stock.Pop());
            CardTakenFromDiscardPile = null;
            _isDiscardPileClosed = false;
            Phase = TurnPhase.Discard;
        }

        public void DrawFromDiscardPile(int playerId)
        {
            EnsureTurn(playerId, TurnPhase.Draw);
            if (_isDiscardPileClosed)
            {
                throw new InvalidMoveException("Both players passed the face-up card, so the hand starts from the stock.");
            }

            TakeTopOfDiscardPile(playerId);
        }

        public void Discard(int playerId, Card card)
        {
            EnsureTurn(playerId, TurnPhase.Discard);
            List<Card> hand = FindHand(playerId);
            EnsureCanDiscard(hand, card);
            PutOnDiscardPile(hand, card);
            if (_stock.Count <= RoundRules.StockCardsToVoid)
            {
                Result = new RoundResult(null, 0, RoundOutcome.Void);
                Phase = TurnPhase.Finished;
            }
            else
            {
                PassTurn();
            }
        }

        public void Knock(int playerId, Card discardedCard, IList<IList<Card>> melds)
        {
            if (melds == null)
            {
                throw new ArgumentNullException(nameof(melds));
            }

            EnsureTurn(playerId, TurnPhase.Discard);
            List<Card> hand = FindHand(playerId);
            EnsureCanDiscard(hand, discardedCard);
            List<Card> remainingCards = hand.Where(card => !card.Equals(discardedCard)).ToList();
            EnsureMeldsOfHand(remainingCards, melds);
            int deadwood = CountUnmatchedPoints(FindUnmatchedCards(remainingCards, melds));
            if (deadwood > RoundRules.KnockThreshold)
            {
                throw new InvalidMoveException("The deadwood is above the knock threshold.");
            }

            PutOnDiscardPile(hand, discardedCard);
            _knockedHand = new KnockedHand(playerId, deadwood, melds);
            Phase = TurnPhase.DefenderLayDown;
            CurrentPlayerId = FindOpponentOf(playerId);
        }

        public RoundResult LayDownDefenderHand(int playerId, IList<IList<Card>> melds)
        {
            if (melds == null)
            {
                throw new ArgumentNullException(nameof(melds));
            }

            EnsureTurn(playerId, TurnPhase.DefenderLayDown);
            List<Card> hand = FindHand(playerId);
            EnsureMeldsOfHand(hand, melds);
            Result = _knockedHand.ResolveDefense(playerId, FindUnmatchedCards(hand, melds));
            Phase = TurnPhase.Finished;

            return Result;
        }

        private void Deal(Queue<Card> deck)
        {
            for (int dealtCount = 0; dealtCount < RoundRules.CardsPerHand; dealtCount++)
            {
                _hands[_nonDealerId].Add(deck.Dequeue());
                _hands[_dealerId].Add(deck.Dequeue());
            }
        }

        private List<Card> FindHand(int playerId)
        {
            List<Card> hand;
            if (!_hands.TryGetValue(playerId, out hand))
            {
                throw new InvalidMoveException("The player does not take part in this round.");
            }

            return hand;
        }

        private int FindOpponentOf(int playerId)
        {
            int opponentId = _dealerId;
            if (playerId == _dealerId)
            {
                opponentId = _nonDealerId;
            }

            return opponentId;
        }

        private void EnsureTurn(int playerId, TurnPhase expectedPhase)
        {
            if ((playerId != CurrentPlayerId) || (Phase != expectedPhase))
            {
                throw new InvalidMoveException("The move does not belong to this moment of the turn.");
            }
        }

        private void EnsureFirstCardOffer(int playerId)
        {
            bool isOffering = (Phase == TurnPhase.FirstCardOfferToNonDealer) || (Phase == TurnPhase.FirstCardOfferToDealer) ? true : false;
            if (!isOffering || (playerId != CurrentPlayerId))
            {
                throw new InvalidMoveException("The face-up card is not being offered to this player.");
            }
        }

        private void EnsureCanDiscard(List<Card> hand, Card card)
        {
            if (!hand.Contains(card))
            {
                throw new InvalidMoveException("The card is not in the hand of the player.");
            }

            if (card.Equals(CardTakenFromDiscardPile))
            {
                throw new InvalidMoveException("The card just taken from the discard pile cannot be discarded in the same turn.");
            }
        }

        private static void EnsureMeldsOfHand(List<Card> hand, IList<IList<Card>> melds)
        {
            bool areMeldsOfHand = melds.All(meld => MeldRules.IsValidMeld(meld));
            if (areMeldsOfHand)
            {
                List<Card> meldedCards = melds.SelectMany(meld => meld).ToList();
                bool isEachCardInOneMeld = meldedCards.Distinct().Count() == meldedCards.Count ? true : false;
                areMeldsOfHand = isEachCardInOneMeld && meldedCards.All(card => hand.Contains(card)) ? true : false;
            }

            if (!areMeldsOfHand)
            {
                throw new InvalidMoveException("The melds are not melds made with the cards of the hand.");
            }
        }

        private void TakeTopOfDiscardPile(int playerId)
        {
            CardTakenFromDiscardPile = _discardPile.Pop();
            FindHand(playerId).Add(CardTakenFromDiscardPile);
            Phase = TurnPhase.Discard;
        }

        private void PutOnDiscardPile(List<Card> hand, Card card)
        {
            hand.Remove(card);
            _discardPile.Push(card);
        }

        private void PassTurn()
        {
            CurrentPlayerId = FindOpponentOf(CurrentPlayerId);
            CardTakenFromDiscardPile = null;
            Phase = TurnPhase.Draw;
        }

        private static List<Card> FindUnmatchedCards(List<Card> hand, IList<IList<Card>> melds)
        {
            List<Card> meldedCards = melds.SelectMany(meld => meld).ToList();

            return hand.Where(card => !meldedCards.Contains(card)).ToList();
        }

        private static int CountUnmatchedPoints(List<Card> unmatchedCards)
        {
            return unmatchedCards.Sum(card => card.DeadwoodValue);
        }
    }
}
