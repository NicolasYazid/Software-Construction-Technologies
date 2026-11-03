using System;
using System.Collections.Generic;

using GinRummy.Domain.Entities.Authentication;

namespace GinRummy.Domain.Ports.Repositories
{
    /// <summary>
    /// Contract the game's logic uses to reach VerificationCode data, without knowing
    /// how or where it is stored.
    /// </summary>
    public interface IVerificationCodeRepository
    {
        /// <summary>
        /// Finds the most recently created code for a player and a purpose, or null
        /// when none exists.
        /// </summary>
        /// <param name="playerId">Player the code belongs to.</param>
        /// <param name="purpose">Reason the code was generated for.</param>
        /// <returns>The matching code, or null.</returns>
        VerificationCode FindMostRecent(int playerId, VerificationPurpose purpose);

        /// <summary>
        /// Gets the codes created for a player and a purpose from a moment on, oldest first.
        /// </summary>
        /// <param name="playerId">Player the codes belong to.</param>
        /// <param name="purpose">Reason the codes were generated for.</param>
        /// <param name="since">Earliest creation moment to include, in UTC.</param>
        /// <returns>The matching codes, ordered by creation moment; empty when none exists.</returns>
        IList<VerificationCode> FindCreatedSince(int playerId, VerificationPurpose purpose, DateTime since);

        /// <summary>
        /// Adds a new code and persists it immediately.
        /// </summary>
        /// <param name="newVerificationCode">Code to create.</param>
        void Add(VerificationCode newVerificationCode);

        /// <summary>
        /// Records the email sent to deliver a verification code, whether the delivery worked
        /// or failed, and persists it immediately.
        /// </summary>
        /// <param name="newEmail">Email to record.</param>
        void AddEmail(Email newEmail);

        /// <summary>
        /// Adds one to the failed-attempt counter of the given code.
        /// </summary>
        /// <param name="verificationCodeId">Code that received a wrong guess.</param>
        void RegisterFailedAttempt(int verificationCodeId);

        /// <summary>
        /// Marks the given code as used at the given moment.
        /// </summary>
        /// <param name="verificationCodeId">Code that was successfully verified.</param>
        /// <param name="usedAt">Moment the code was used, in UTC, read from the server clock.</param>
        void MarkAsUsed(int verificationCodeId, DateTime usedAt);
    }
}
