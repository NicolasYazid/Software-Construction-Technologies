using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using NUnit.Framework;

namespace GinRummy.Application.Tests.DirectDuel
{
    [TestFixture]
    public class ChallengeBoardTests
    {
        private const int ChallengerId = 1;
        private const int ChallengedId = 2;
        private const int ThirdPlayerId = 3;

        [Test]
        public void Test_TryPost_ChallengeToOneself_ReturnsFalse()
        {
            ChallengeBoard board = new ChallengeBoard(new RoundRegistry());
            DuelPlayer player = CreatePlayer(ChallengerId);

            bool isPosted = board.TryPost(new PendingChallenge(player, player));

            Assert.IsFalse(isPosted);
        }

        [Test]
        public void Test_TryPost_SecondOpenChallenge_ReturnsFalse()
        {
            ChallengeBoard board = new ChallengeBoard(new RoundRegistry());
            board.TryPost(new PendingChallenge(CreatePlayer(ChallengerId), CreatePlayer(ChallengedId)));

            bool isPosted = board.TryPost(new PendingChallenge(CreatePlayer(ChallengerId), CreatePlayer(ThirdPlayerId)));

            Assert.IsFalse(isPosted);
        }

        [Test]
        public void Test_TryPost_ChallengedPlayerInRound_ReturnsFalse()
        {
            RoundRegistry roundRegistry = new RoundRegistry();
            roundRegistry.Start(CreatePlayer(ChallengedId), CreatePlayer(ThirdPlayerId));
            ChallengeBoard board = new ChallengeBoard(roundRegistry);

            bool isPosted = board.TryPost(new PendingChallenge(CreatePlayer(ChallengerId), CreatePlayer(ChallengedId)));

            Assert.IsFalse(isPosted);
        }

        [Test]
        public void Test_Accept_PostedChallenge_StartsTheRound()
        {
            RoundRegistry roundRegistry = new RoundRegistry();
            ChallengeBoard board = new ChallengeBoard(roundRegistry);
            DuelPlayer challenger = CreatePlayer(ChallengerId);
            board.TryPost(new PendingChallenge(challenger, CreatePlayer(ChallengedId)));

            board.Accept(ChallengedId, challenger.PublicTag);

            Assert.IsTrue(roundRegistry.IsPlaying(ChallengerId));
        }

        [Test]
        public void Test_Accept_CancelledChallenge_ReturnsNull()
        {
            ChallengeBoard board = new ChallengeBoard(new RoundRegistry());
            DuelPlayer challenger = CreatePlayer(ChallengerId);
            board.TryPost(new PendingChallenge(challenger, CreatePlayer(ChallengedId)));
            board.RemoveFrom(ChallengerId);

            PendingChallenge acceptedChallenge = board.Accept(ChallengedId, challenger.PublicTag);

            Assert.IsNull(acceptedChallenge);
        }

        private static DuelPlayer CreatePlayer(int id)
        {
            return new DuelPlayer(id, "Player " + id, 1000 + id);
        }
    }
}
