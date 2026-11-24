using GinRummy.Application.UseCases.DirectDuel;
using GinRummy.Application.UseCases.Gameplay;
using GinRummy.Domain;
using NUnit.Framework;

namespace GinRummy.Server.Tests.Application.DirectDuel
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
            ChallengeBoard board = new ChallengeBoard(new MatchRegistry(StandardDeck.CreateOrdered), new FakeChallengeRecorder());
            DuelPlayer player = CreatePlayer(ChallengerId);

            TestDelegate post = () => board.Post(new PendingChallenge(player, player));

            Assert.Throws<SelfChallengeException>(post);
        }

        [Test]
        public void Test_Post_SecondOpenChallenge_ReturnsAlreadyChallenging()
        {
            ChallengeBoard board = new ChallengeBoard(new MatchRegistry(StandardDeck.CreateOrdered), new FakeChallengeRecorder());
            board.Post(new PendingChallenge(CreatePlayer(ChallengerId), CreatePlayer(ChallengedId)));

            ChallengeOutcome outcome = board.Post(new PendingChallenge(CreatePlayer(ChallengerId), CreatePlayer(ThirdPlayerId)));

            Assert.AreEqual(ChallengeOutcome.AlreadyChallenging, outcome);
        }

        [Test]
        public void Test_Post_ChallengedPlayerInRound_ReturnsOpponentBusy()
        {
            MatchRegistry matchRegistry = new MatchRegistry(StandardDeck.CreateOrdered);
            matchRegistry.Start(CreatePlayer(ChallengedId), CreatePlayer(ThirdPlayerId));
            ChallengeBoard board = new ChallengeBoard(matchRegistry, new FakeChallengeRecorder());

            ChallengeOutcome outcome = board.Post(new PendingChallenge(CreatePlayer(ChallengerId), CreatePlayer(ChallengedId)));

            Assert.AreEqual(ChallengeOutcome.OpponentBusy, outcome);
        }

        [Test]
        public void Test_Post_CrossedChallenge_ThrowsConcurrentChallengeException()
        {
            ChallengeBoard board = new ChallengeBoard(new MatchRegistry(StandardDeck.CreateOrdered), new FakeChallengeRecorder());
            board.Post(new PendingChallenge(CreatePlayer(ChallengerId), CreatePlayer(ChallengedId)));

            TestDelegate post = () => board.Post(new PendingChallenge(CreatePlayer(ChallengedId), CreatePlayer(ChallengerId)));

            Assert.Throws<ConcurrentChallengeException>(post);
        }

        [Test]
        public void Test_Accept_PostedChallenge_StartsTheRound()
        {
            MatchRegistry matchRegistry = new MatchRegistry(StandardDeck.CreateOrdered);
            ChallengeBoard board = new ChallengeBoard(matchRegistry, new FakeChallengeRecorder());
            DuelPlayer challenger = CreatePlayer(ChallengerId);
            board.Post(new PendingChallenge(challenger, CreatePlayer(ChallengedId)));

            board.Accept(ChallengedId, challenger.PublicTag, out ActiveMatch startedMatch);

            Assert.IsTrue(matchRegistry.IsPlaying(ChallengerId));
        }

        [Test]
        public void Test_Accept_CancelledChallenge_ReturnsChallengeExpired()
        {
            ChallengeBoard board = new ChallengeBoard(new MatchRegistry(StandardDeck.CreateOrdered), new FakeChallengeRecorder());
            DuelPlayer challenger = CreatePlayer(ChallengerId);
            board.Post(new PendingChallenge(challenger, CreatePlayer(ChallengedId)));
            board.Cancel(ChallengerId);

            ChallengeOutcome outcome = board.Accept(ChallengedId, challenger.PublicTag, out ActiveMatch startedMatch);

            Assert.AreEqual(ChallengeOutcome.ChallengeExpired, outcome);
        }

        [Test]
        public void Test_Accept_ChallengerInRound_ReturnsOpponentBusy()
        {
            MatchRegistry matchRegistry = new MatchRegistry(StandardDeck.CreateOrdered);
            ChallengeBoard board = new ChallengeBoard(matchRegistry, new FakeChallengeRecorder());
            DuelPlayer challenger = CreatePlayer(ChallengerId);
            board.Post(new PendingChallenge(challenger, CreatePlayer(ChallengedId)));
            matchRegistry.Start(challenger, CreatePlayer(ThirdPlayerId));

            ChallengeOutcome outcome = board.Accept(ChallengedId, challenger.PublicTag, out ActiveMatch startedMatch);

            Assert.AreEqual(ChallengeOutcome.OpponentBusy, outcome);
        }

        [Test]
        public void Test_Accept_ExpiredChallenge_ReturnsChallengeExpired()
        {
            FakeChallengeRecorder challengeRecorder = new FakeChallengeRecorder();
            challengeRecorder.IsChallengeExpired = true;
            ChallengeBoard board = new ChallengeBoard(new MatchRegistry(StandardDeck.CreateOrdered), challengeRecorder);
            DuelPlayer challenger = CreatePlayer(ChallengerId);
            board.Post(new PendingChallenge(challenger, CreatePlayer(ChallengedId)));

            ChallengeOutcome outcome = board.Accept(ChallengedId, challenger.PublicTag, out ActiveMatch startedMatch);

            Assert.AreEqual(ChallengeOutcome.ChallengeExpired, outcome);
        }

        private static DuelPlayer CreatePlayer(int id)
        {
            return new DuelPlayer(id, "Player " + id, 1000 + id);
        }
    }
}
