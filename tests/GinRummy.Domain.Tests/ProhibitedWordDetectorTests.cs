using System.Collections.Generic;

using GinRummy.Domain.Services;
using NUnit.Framework;

namespace GinRummy.Domain.Tests
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

            bool containsProhibitedWord = ProhibitedWordDetector.ContainsProhibitedWord(text, _prohibitedTerms);

            Assert.IsTrue(containsProhibitedWord);
        }

        [Test]
        public void Test_ContainsProhibitedWord_TermInsideAnotherWord_ReturnsFalse()
        {
            string text = "eso fue una tontera";

            bool containsProhibitedWord = ProhibitedWordDetector.ContainsProhibitedWord(text, _prohibitedTerms);

            Assert.IsFalse(containsProhibitedWord);
        }

        [Test]
        public void Test_ContainsProhibitedWord_UpperCaseAndAccents_ReturnsTrue()
        {
            string text = "ERES UN IDIÓTA";

            bool containsProhibitedWord = ProhibitedWordDetector.ContainsProhibitedWord(text, _prohibitedTerms);

            Assert.IsTrue(containsProhibitedWord);
        }

        [Test]
        public void Test_ContainsProhibitedWord_LookAlikeDigits_ReturnsTrue()
        {
            string text = "qué t0nt0";

            bool containsProhibitedWord = ProhibitedWordDetector.ContainsProhibitedWord(text, _prohibitedTerms);

            Assert.IsTrue(containsProhibitedWord);
        }

        [Test]
        public void Test_ContainsProhibitedWord_RepeatedLetters_ReturnsTrue()
        {
            string text = "toooontooo";

            bool containsProhibitedWord = ProhibitedWordDetector.ContainsProhibitedWord(text, _prohibitedTerms);

            Assert.IsTrue(containsProhibitedWord);
        }

        [Test]
        public void Test_ContainsProhibitedWord_PunctuationAroundTerm_ReturnsTrue()
        {
            string text = "¡tonto!";

            bool containsProhibitedWord = ProhibitedWordDetector.ContainsProhibitedWord(text, _prohibitedTerms);

            Assert.IsTrue(containsProhibitedWord);
        }

        [Test]
        public void Test_ContainsProhibitedWord_CleanText_ReturnsFalse()
        {
            string text = "buena partida";

            bool containsProhibitedWord = ProhibitedWordDetector.ContainsProhibitedWord(text, _prohibitedTerms);

            Assert.IsFalse(containsProhibitedWord);
        }
    }
}
