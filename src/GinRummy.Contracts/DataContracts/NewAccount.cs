using System.Runtime.Serialization;

namespace GinRummy.Contracts.DataContracts
{
    /// <summary>
    /// What a person typed to create an account, together with the language the client was showing. The
    /// confirmation of the password stays in the client, which is the only one that checks it (CU-01 FA-04).
    /// </summary>
    [DataContract]
    public class NewAccount
    {
        /// <summary>
        /// Email of the new account, as the person typed it.
        /// </summary>
        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Username of the new account, as the person typed it.
        /// </summary>
        [DataMember]
        public string Username { get; set; }

        /// <summary>
        /// Password of the new account, as the person typed it.
        /// </summary>
        [DataMember]
        public string Password { get; set; }

        /// <summary>
        /// Culture code of the language the client was showing, such as es-MX. The server assigns its default
        /// language when the catalog does not have this one (CU-01 EX-02).
        /// </summary>
        [DataMember]
        public string LocaleCode { get; set; }
    }
}
