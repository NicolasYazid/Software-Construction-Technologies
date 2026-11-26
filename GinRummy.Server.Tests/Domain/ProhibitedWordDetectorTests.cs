using System.Collections.Generic;

using GinRummy.Domain.Entities.PlayerModeration;
using NUnit.Framework;

namespace GinRummy.Server.Tests.Domain
{
    [TestFixture]
    public class ProhibitedWordDetectorTests
    {
        private IList<string> _prohibitedTerms;

        [SetUp]
        public void SetUp()
        {
            _prohibitedTerms = new List<string> { "tonto", "idiota" };
        }

        [Test]
        public void Test_ContainsProhibitedWord_TermAsWholeWord_ReturnsTrue()
        {
            string text = "eres un tonto";

            bool hasProhibitedWord = ProhibitedWordDetector.ContainsProhibitedWord(text, _prohibitedTerms);

            Assert.IsTrue(hasProhibitedWord);
        }

        [Test]
        public void Test_ContainsProhibitedWord_TermInsideAnotherWord_ReturnsFalse()
        {
            string text = "eso fue una tontera";

            bool hasProhibitedWord = ProhibitedWordDetector.ContainsProhibitedWord(text, _prohibitedTerms);

            Assert.IsFalse(hasProhibitedWord);
        }

        [Test]
        public void Test_ContainsProhibitedWord_UpperCaseAndAccents_ReturnsTrue()
        {
            string text = "ERES UN IDIÓTA";

            bool hasProhibitedWord = ProhibitedWordDetector.ContainsProhibitedWord(text, _prohibitedTerms);

            Assert.IsTrue(hasProhibitedWord);
        }

        [Test]
        public void Test_ContainsProhibitedWord_LookAlikeDigits_ReturnsTrue()
        {
            string text = "qué t0nt0";

            bool hasProhibitedWord = ProhibitedWordDetector.ContainsProhibitedWord(text, _prohibitedTerms);

            Assert.IsTrue(hasProhibitedWord);
        }

        [Test]
        public void Test_ContainsProhibitedWord_RepeatedLetters_ReturnsTrue()
        {
            string text = "toooontooo";

            bool hasProhibitedWord = ProhibitedWordDetector.ContainsProhibitedWord(text, _prohibitedTerms);

            Assert.IsTrue(hasProhibitedWord);
        }

        [Test]
        public void Test_ContainsProhibitedWord_PunctuationAroundTerm_ReturnsTrue()
        {
            string text = "¡tonto!";

            bool hasProhibitedWord = ProhibitedWordDetector.ContainsProhibitedWord(text, _prohibitedTerms);

            Assert.IsTrue(hasProhibitedWord);
        }

        [Test]
        public void Test_ContainsProhibitedWord_CleanText_ReturnsFalse()
        {
            string text = "buena partida";

            bool hasProhibitedWord = ProhibitedWordDetector.ContainsProhibitedWord(text, _prohibitedTerms);

            Assert.IsFalse(hasProhibitedWord);
        }
    }
}
