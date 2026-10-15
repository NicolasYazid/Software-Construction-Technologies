using System.Collections.Generic;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to reach chat messages and the prohibited word list,
    /// without knowing how or where they are stored.
    /// </summary>
    public interface IMessageRepository
    {
        /// <summary>
        /// Finds a message by its identifier, with its sender loaded, or null when it does
        /// not exist.
        /// </summary>
        /// <param name="messageId">Identifier of the message.</param>
        /// <returns>The matching message, or null.</returns>
        Message FindById(int messageId);

        /// <summary>
        /// Adds a new message and persists it immediately.
        /// </summary>
        /// <param name="newMessage">Message to create.</param>
        void Add(Message newMessage);

        /// <summary>
        /// Gets the prohibited words with the locales each one applies to.
        /// </summary>
        /// <returns>The prohibited words.</returns>
        IList<ProhibitedWord> GetProhibitedWords();
    }
}
