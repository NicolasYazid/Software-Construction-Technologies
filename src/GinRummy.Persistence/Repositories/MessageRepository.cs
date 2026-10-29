using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using GinRummy.Persistence.Persistence;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Repositories;

namespace GinRummy.Persistence.Repositories
{
    /// <summary>
    /// Fulfils IMessageRepository using Entity Framework against GinRummy_Dev. It always runs on
    /// the context of the unit of work that created it, so its changes share that unit's
    /// transaction.
    /// </summary>
    public class MessageRepository : IMessageRepository
    {
        private readonly GinRummyContext _context;

        /// <summary>
        /// Builds the repository on a context someone else opened. The caller stays responsible
        /// for disposing that context.
        /// </summary>
        /// <param name="context">Context to run every operation on.</param>
        public MessageRepository(GinRummyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Finds a message by its identifier, with its sender loaded, or null when it does
        /// not exist.
        /// </summary>
        /// <param name="messageId">Identifier of the message.</param>
        /// <returns>The matching message, or null.</returns>
        public Message FindById(int messageId)
        {
            return _context.Messages
                .Include(message => message.Player)
                .FirstOrDefault(message => message.MessageId == messageId);
        }

        /// <summary>
        /// Adds a new message and persists it immediately.
        /// </summary>
        /// <param name="newMessage">Message to create.</param>
        public void Add(Message newMessage)
        {
            _context.Messages.Add(newMessage);
            _context.SaveChanges();
        }

        /// <summary>
        /// Gets the prohibited words with the locales each one applies to.
        /// </summary>
        /// <returns>The prohibited words.</returns>
        public IList<ProhibitedWord> GetProhibitedWords()
        {
            return _context.ProhibitedWords
                .Include(word => word.Locales)
                .ToList();
        }
    }
}
