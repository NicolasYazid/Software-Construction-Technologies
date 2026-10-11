using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GinRummy.Domain.Entities;

namespace GinRummy.Domain.Repositories
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
        /// Adds a new code and persists it immediately.
        /// </summary>
        /// <param name="newVerificationCode">Code to create.</param>
        void Add(VerificationCode newVerificationCode);

        /// <summary>
        /// Adds one to the failed-attempt counter of the given code.
        /// </summary>
        /// <param name="verificationCodeId">Code that received a wrong guess.</param>
        void RegisterFailedAttempt(int verificationCodeId);

        /// <summary>
        /// Marks the given code as used, right now.
        /// </summary>
        /// <param name="verificationCodeId">Code that was successfully verified.</param>
        void MarkAsUsed(int verificationCodeId);
    }
}
