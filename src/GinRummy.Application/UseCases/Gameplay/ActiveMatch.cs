using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Domain;

namespace GinRummy.Application.UseCases.Gameplay
{
    public class ActiveMatch
    {
        private readonly List<DuelPlayer> _players;
        private readonly Dictionary<int, Timer> _reconnectionTimersByPlayerId;

        public ActiveMatch(GameMatch match, DuelPlayer dealer, DuelPlayer nonDealer)
        {
            Match = match ?? throw new ArgumentNullException(nameof(match));
            _players = new List<DuelPlayer> { dealer, nonDealer };
            _reconnectionTimersByPlayerId = new Dictionary<int, Timer>();
            SyncRoot = new object();
            Record(RoundMoveKind.Dealt, dealer.Id, null);
        }

        public GameMatch Match { get; private set; }

        public GameRound Round
        {
            get { return Match.CurrentRound; }
        }

        public object SyncRoot { get; private set; }

        public bool IsPaused
        {
            get { return _reconnectionTimersByPlayerId.Count > 0 ? true : false; }
        }

        public IReadOnlyList<DuelPlayer> Players
        {
            get { return _players; }
        }

        public int? KnockerId { get; private set; }

        public RoundMoveKind LastMove { get; private set; }

        public int LastMoverId { get; private set; }

        public Card LastMoveCard { get; private set; }

        public DuelPlayer FindPlayer(int playerId)
        {
            return _players.First(player => player.Id == playerId);
        }

        public DuelPlayer FindOpponent(int playerId)
        {
            return _players.First(player => player.Id != playerId);
        }

        public void TakeFirstCard(int playerId)
        {
            Card takenCard = Round.TopOfDiscardPile;
            Round.TakeFirstCard(playerId);
            Record(RoundMoveKind.DrewFromDiscardPile, playerId, takenCard);
        }

        public void PassFirstCard(int playerId)
        {
            Round.PassFirstCard(playerId);
            Record(RoundMoveKind.PassedFirstCard, playerId, null);
        }

        public void DrawFromStock(int playerId)
        {
            Round.DrawFromStock(playerId);
            Record(RoundMoveKind.DrewFromStock, playerId, null);
        }

        public void DrawFromDiscardPile(int playerId)
        {
            Card takenCard = Round.TopOfDiscardPile;
            Round.DrawFromDiscardPile(playerId);
            Record(RoundMoveKind.DrewFromDiscardPile, playerId, takenCard);
        }

        public void Discard(int playerId, Card card)
        {
            Round.Discard(playerId, card);
            Record(RoundMoveKind.Discarded, playerId, card);
        }

        public void Knock(int playerId, Card discardedCard)
        {
            List<Card> remainingCards = Round.GetHand(playerId).Where(card => !card.Equals(discardedCard)).ToList();
            Round.Knock(playerId, discardedCard, MeldFinder.FindBestMelds(remainingCards));
            KnockerId = playerId;
            int defenderId = FindOpponent(playerId).Id;
            Round.LayDownDefenderHand(defenderId, MeldFinder.FindBestMelds(Round.GetHand(defenderId).ToList()));
            Record(RoundMoveKind.Discarded, playerId, discardedCard);
        }

        public bool IsWaitingForReconnection(int playerId)
        {
            return _reconnectionTimersByPlayerId.ContainsKey(playerId);
        }

        public void WaitForReconnection(int playerId, TimeSpan reconnectionWindow, Action onWindowExpired)
        {
            StopWaitingForReconnection(playerId);
            _reconnectionTimersByPlayerId[playerId] = new Timer(
                state => onWindowExpired(),
                null,
                reconnectionWindow,
                Timeout.InfiniteTimeSpan);
        }

        public bool StopWaitingForReconnection(int playerId)
        {
            Timer reconnectionTimer;
            bool wasWaiting = _reconnectionTimersByPlayerId.TryGetValue(playerId, out reconnectionTimer);
            if (wasWaiting)
            {
                reconnectionTimer.Dispose();
                _reconnectionTimersByPlayerId.Remove(playerId);
            }

            return wasWaiting;
        }

        public void StopWaitingForReconnections()
        {
            foreach (Timer reconnectionTimer in _reconnectionTimersByPlayerId.Values)
            {
                reconnectionTimer.Dispose();
            }

            _reconnectionTimersByPlayerId.Clear();
        }

        public void DealNextRound(IList<Card> shuffledDeck)
        {
            Match.DealNextRound(shuffledDeck);
            KnockerId = null;
            Record(RoundMoveKind.Dealt, Round.DealerId, null);
        }

        public IList<Card> FindKnockDiscards(int playerId)
        {
            List<Card> hand = Round.GetHand(playerId).ToList();
            List<Card> discards = new List<Card>();
            if ((Round.CurrentPlayerId == playerId) && (Round.Phase == TurnPhase.Discard))
            {
                discards = hand
                    .Where(card => !card.Equals(Round.CardTakenFromDiscardPile))
                    .Where(card => CountDeadwoodWithout(hand, card) <= RoundRules.KnockThreshold)
                    .ToList();
            }

            return discards;
        }

        private static int CountDeadwoodWithout(List<Card> hand, Card discardedCard)
        {
            return MeldFinder.CountDeadwood(hand.Where(card => !card.Equals(discardedCard)).ToList());
        }

        private void Record(RoundMoveKind move, int playerId, Card card)
        {
            LastMove = move;
            LastMoverId = playerId;
            LastMoveCard = card;
        }
    }
}
