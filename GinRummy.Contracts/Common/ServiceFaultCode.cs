using System.Runtime.Serialization;

namespace GinRummy.Contracts.Common
{
    /// <summary>
    /// Reasons a request-reply operation can fail, as listed in annex A.2 of the services document.
    /// </summary>
    [DataContract]
    public enum ServiceFaultCode
    {
        /// <summary>
        /// The request data did not pass the server validation.
        /// </summary>
        [EnumMember]
        InvalidData,

        /// <summary>
        /// The email or the password is not correct.
        /// </summary>
        [EnumMember]
        InvalidCredentials,

        /// <summary>
        /// The verification code is not correct.
        /// </summary>
        [EnumMember]
        InvalidCode,

        /// <summary>
        /// The verification code is no longer valid.
        /// </summary>
        [EnumMember]
        CodeExpired,

        /// <summary>
        /// The account has not confirmed its email address yet.
        /// </summary>
        [EnumMember]
        EmailNotVerified,

        /// <summary>
        /// The session token is unknown or was closed.
        /// </summary>
        [EnumMember]
        SessionExpired,

        /// <summary>
        /// The account has an active sanction that blocks the operation.
        /// </summary>
        [EnumMember]
        AccountSuspended,

        /// <summary>
        /// The requested element does not exist or the player cannot see it.
        /// </summary>
        [EnumMember]
        NotFound,

        /// <summary>
        /// The operation clashes with the current state, such as joining a match that already finished.
        /// </summary>
        [EnumMember]
        Conflict,

        /// <summary>
        /// The player is already in a match.
        /// </summary>
        [EnumMember]
        PlayerBusy,

        /// <summary>
        /// A resource the server depends on, such as the database, did not answer.
        /// </summary>
        [EnumMember]
        ServiceUnavailable,

        /// <summary>
        /// An unexpected error; the details stay in the server log.
        /// </summary>
        [EnumMember]
        InternalError
    }
}