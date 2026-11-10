using System;
using System.Collections.Generic;
using System.Linq;

using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Domain.Gameplay;

namespace GinRummy.Application.UseCases.Gameplay
{
    public class ActiveRound
    {
        private readonly List<DuelPlayer> _players;

        public ActiveRound(GameRound round, DuelPlayer dealer, DuelPlayer nonDealer)
        {
            Round = round ?? throw new ArgumentNullException(nameof(round));
            _players = new List<DuelPlayer> { dealer, nonDealer };
            SyncRoot = new object();
            Record(RoundMove.Dealt, dealer.Id, null);
        }

        public GameRound Round { get; private set; }

        public object SyncRoot { get; private set; }

        public IReadOnlyList<DuelPlayer> Players
        {
            get { return _players; }
        }

        public int? KnockerId { get; private set; }

        public RoundMove LastMove { get; private set; }

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
            Record(RoundMove.DrewFromDiscardPile, playerId, takenCard);
        }

        public void PassFirstCard(int playerId)
        {
            Round.PassFirstCard(playerId);
            Record(RoundMove.PassedFirstCard, playerId, null);
        }

        public void DrawFromStock(int playerId)
        {
            Round.DrawFromStock(playerId);
            Record(RoundMove.DrewFromStock, playerId, null);
        }

        public void DrawFromDiscardPile(int playerId)
        {
            Card takenCard = Round.TopOfDiscardPile;
            Round.DrawFromDiscardPile(playerId);
            Record(RoundMove.DrewFromDiscardPile, playerId, takenCard);
        }

        public void Discard(int playerId, Card card)
        {
            Round.Discard(playerId, card);
            Record(RoundMove.Discarded, playerId, card);
        }

        public void Knock(int playerId, Card discardedCard)
        {
            List<Card> remainingCards = Round.GetHand(playerId).Where(card => !card.Equals(discardedCard)).ToList();
            Round.Knock(playerId, discardedCard, MeldFinder.FindBestMelds(remainingCards));
            KnockerId = playerId;
            int defenderId = FindOpponent(playerId).Id;
            Round.LayDownDefenderHand(defenderId, MeldFinder.FindBestMelds(Round.GetHand(defenderId).ToList()));
            Record(RoundMove.Discarded, playerId, discardedCard);
        }

        private void Record(RoundMove move, int playerId, Card card)
        {
            LastMove = move;
            LastMoverId = playerId;
            LastMoveCard = card;
        }
    }
}
