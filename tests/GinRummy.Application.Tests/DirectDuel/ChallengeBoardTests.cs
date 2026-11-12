using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Domain;
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
        public void Test_Post_ChallengeToOneself_ThrowsSelfChallengeException()
        {
            ChallengeBoard board = new ChallengeBoard(new RoundRegistry());
            DuelPlayer player = CreatePlayer(ChallengerId);

            TestDelegate post = () => board.Post(new PendingChallenge(player, player));

            Assert.Throws<SelfChallengeException>(post);
        }

        [Test]
        public void Test_Post_SecondOpenChallenge_ReturnsAlreadyChallenging()
        {
            ChallengeBoard board = new ChallengeBoard(new RoundRegistry());
            board.Post(new PendingChallenge(CreatePlayer(ChallengerId), CreatePlayer(ChallengedId)));

            ChallengeOutcome outcome = board.Post(new PendingChallenge(CreatePlayer(ChallengerId), CreatePlayer(ThirdPlayerId)));

            Assert.AreEqual(ChallengeOutcome.AlreadyChallenging, outcome);
        }

        [Test]
        public void Test_Post_ChallengedPlayerInRound_ReturnsOpponentBusy()
        {
            RoundRegistry roundRegistry = new RoundRegistry();
            roundRegistry.Start(CreatePlayer(ChallengedId), CreatePlayer(ThirdPlayerId));
            ChallengeBoard board = new ChallengeBoard(roundRegistry);

            ChallengeOutcome outcome = board.Post(new PendingChallenge(CreatePlayer(ChallengerId), CreatePlayer(ChallengedId)));

            Assert.AreEqual(ChallengeOutcome.OpponentBusy, outcome);
        }

        [Test]
        public void Test_Post_CrossedChallenge_ThrowsConcurrentChallengeException()
        {
            ChallengeBoard board = new ChallengeBoard(new RoundRegistry());
            board.Post(new PendingChallenge(CreatePlayer(ChallengerId), CreatePlayer(ChallengedId)));

            TestDelegate post = () => board.Post(new PendingChallenge(CreatePlayer(ChallengedId), CreatePlayer(ChallengerId)));

            Assert.Throws<ConcurrentChallengeException>(post);
        }

        [Test]
        public void Test_Accept_PostedChallenge_StartsTheRound()
        {
            RoundRegistry roundRegistry = new RoundRegistry();
            ChallengeBoard board = new ChallengeBoard(roundRegistry);
            DuelPlayer challenger = CreatePlayer(ChallengerId);
            board.Post(new PendingChallenge(challenger, CreatePlayer(ChallengedId)));

            board.Accept(ChallengedId, challenger.PublicTag, out ActiveRound startedRound);

            Assert.IsTrue(roundRegistry.IsPlaying(ChallengerId));
        }

        [Test]
        public void Test_Accept_CancelledChallenge_ReturnsChallengeExpired()
        {
            ChallengeBoard board = new ChallengeBoard(new RoundRegistry());
            DuelPlayer challenger = CreatePlayer(ChallengerId);
            board.Post(new PendingChallenge(challenger, CreatePlayer(ChallengedId)));
            board.RemoveFrom(ChallengerId);

            ChallengeOutcome outcome = board.Accept(ChallengedId, challenger.PublicTag, out ActiveRound startedRound);

            Assert.AreEqual(ChallengeOutcome.ChallengeExpired, outcome);
        }

        [Test]
        public void Test_Accept_ChallengerInRound_ReturnsOpponentBusy()
        {
            RoundRegistry roundRegistry = new RoundRegistry();
            ChallengeBoard board = new ChallengeBoard(roundRegistry);
            DuelPlayer challenger = CreatePlayer(ChallengerId);
            board.Post(new PendingChallenge(challenger, CreatePlayer(ChallengedId)));
            roundRegistry.Start(challenger, CreatePlayer(ThirdPlayerId));

            ChallengeOutcome outcome = board.Accept(ChallengedId, challenger.PublicTag, out ActiveRound startedRound);

            Assert.AreEqual(ChallengeOutcome.OpponentBusy, outcome);
        }

        private static DuelPlayer CreatePlayer(int id)
        {
            return new DuelPlayer(id, "Player " + id, 1000 + id);
        }
    }
}
