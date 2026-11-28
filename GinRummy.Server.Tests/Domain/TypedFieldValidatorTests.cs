using GinRummy.Domain.Shared;
using NUnit.Framework;

namespace GinRummy.Server.Tests.Domain
{
    [TestFixture]
    public class TypedFieldValidatorTests
    {
        private const string TypedEmail = "player@example.com";
        private const string TypedPassword = "Secret123!";
        private const string TextWithLineBreak = "player\n@example.com";

        [Test]
        public void Test_AreTyped_AllFieldsTyped_ReturnsTrue()
        {
            bool areTyped = TypedFieldValidator.AreTyped(TypedEmail, TypedPassword);

            Assert.IsTrue(areTyped);
        }

        [Test]
        public void Test_AreTyped_OneFieldEmpty_ReturnsFalse()
        {
            bool areTyped = TypedFieldValidator.AreTyped(TypedEmail, string.Empty);

            Assert.IsFalse(areTyped);
        }

        [Test]
        public void Test_AreTyped_OneFieldNull_ReturnsFalse()
        {
            bool areTyped = TypedFieldValidator.AreTyped(null, TypedPassword);

            Assert.IsFalse(areTyped);
        }

        [Test]
        public void Test_HaveForbiddenCharacters_PlainText_ReturnsFalse()
        {
            bool haveForbiddenCharacters = TypedFieldValidator.HaveForbiddenCharacters(TypedEmail, TypedPassword);

            Assert.IsFalse(haveForbiddenCharacters);
        }

        [Test]
        public void Test_HaveForbiddenCharacters_LineBreak_ReturnsTrue()
        {
            bool haveForbiddenCharacters = TypedFieldValidator.HaveForbiddenCharacters(TextWithLineBreak, TypedPassword);

            Assert.IsTrue(haveForbiddenCharacters);
        }
    }
}
