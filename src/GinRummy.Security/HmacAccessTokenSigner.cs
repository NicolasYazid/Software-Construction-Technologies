using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

using GinRummy.Domain;
using GinRummy.Domain.Entities;
using GinRummy.Domain.Ports.Security;

namespace GinRummy.Security
{
    /// <summary>
    /// Fulfils IAccessTokenSigner with JSON Web Tokens signed with HMAC-SHA256 (HS256). Only the server knows the
    /// key, so a token whose claims were changed no longer matches its signature, and the server needs no stored
    /// session to trust it.
    /// </summary>
    public class HmacAccessTokenSigner : IAccessTokenSigner
    {
        /// <summary>
        /// Smallest key accepted, because RFC 2104 recommends a key at least as long as the 32-byte output of
        /// SHA-256.
        /// </summary>
        public const int MinimumKeySizeInBytes = 32;
        private const string Algorithm = "HS256";
        private const string Issuer = "GinRummy.Server";
        private const string HeaderJson = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";
        private const string PayloadFormat = "{{\"iss\":\"{0}\",\"sub\":\"{1}\",\"iat\":{2},\"exp\":{3}}}";
        private const string AlgorithmClaim = "alg";
        private const string IssuerClaim = "iss";
        private const string SubjectClaim = "sub";
        private const string ExpirationClaim = "exp";
        private const string SegmentSeparator = ".";
        private const int SegmentCount = 3;
        private const int HeaderIndex = 0;
        private const int PayloadIndex = 1;
        private const int SignatureIndex = 2;
        private const int Base64BlockSize = 4;
        private const char Base64Padding = '=';
        private static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly byte[] _signingKey;

        /// <summary>
        /// Builds the signer with the secret key the composition root read when the server started.
        /// </summary>
        /// <param name="signingKey">Secret key of at least 32 bytes; a copy is kept.</param>
        /// <exception cref="ArgumentNullException">Thrown when the key is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the key is shorter than 32 bytes.</exception>
        public HmacAccessTokenSigner(byte[] signingKey)
        {
            byte[] key = signingKey ?? throw new ArgumentNullException(nameof(signingKey));
            if (key.Length < MinimumKeySizeInBytes)
            {
                throw new ArgumentException("The signing key must have at least 32 bytes.", nameof(signingKey));
            }

            _signingKey = (byte[])key.Clone();
        }

        /// <summary>
        /// Signs a new access token for a player.
        /// </summary>
        /// <param name="playerId">Player the token identifies.</param>
        /// <param name="issuedAt">Moment the token is issued, in UTC.</param>
        /// <param name="expiresAt">Moment the token stops being valid, in UTC.</param>
        /// <returns>The signed token.</returns>
        public AccessToken Sign(int playerId, DateTime issuedAt, DateTime expiresAt)
        {
            // The claims only identify the player, because anyone who holds the token can decode them.
            string payloadJson = string.Format(
                CultureInfo.InvariantCulture,
                PayloadFormat,
                Issuer,
                playerId,
                ToUnixSeconds(issuedAt),
                ToUnixSeconds(expiresAt));
            string signingInput = string.Concat(EncodeText(HeaderJson), SegmentSeparator, EncodeText(payloadJson));
            string token = string.Concat(signingInput, SegmentSeparator, ToBase64Url(ComputeSignature(signingInput)));

            return new AccessToken(token, expiresAt);
        }

        /// <summary>
        /// Checks the signature, the algorithm, the issuer and the expiry of a token and reads its player.
        /// </summary>
        /// <param name="token">Encoded token, as the client presented it.</param>
        /// <param name="moment">Moment of the check, in UTC.</param>
        /// <returns>The identifier of the player.</returns>
        /// <exception cref="InvalidAccessTokenException">Thrown when the token is malformed, altered or expired.</exception>
        public int ReadPlayerId(string token, DateTime moment)
        {
            string[] segments = SplitSegments(token);

            // The signature is checked before any claim is read, so nothing an attacker wrote is trusted.
            EnsureSignatureMatches(segments);
            EnsureExpectedAlgorithm(ReadSegment(segments[HeaderIndex]));
            IDictionary<string, object> claims = ReadSegment(segments[PayloadIndex]);
            EnsureIsCurrent(claims, moment);
            int playerId = ReadSubject(claims);

            return playerId;
        }

        private static string[] SplitSegments(string token)
        {
            string[] segments = (token ?? string.Empty).Split(new[] { SegmentSeparator }, StringSplitOptions.None);
            if (segments.Length != SegmentCount)
            {
                throw new InvalidAccessTokenException("The token does not have a header, a payload and a signature.");
            }

            return segments;
        }

        // A token that names another algorithm, including "none", is rejected even if it carried a valid signature.
        private static void EnsureExpectedAlgorithm(IDictionary<string, object> header)
        {
            object algorithm;
            bool isExpected = header.TryGetValue(AlgorithmClaim, out algorithm) && (Algorithm == (algorithm as string));
            if (!isExpected)
            {
                throw new InvalidAccessTokenException("The token was not signed with the expected algorithm.");
            }
        }

        private static void EnsureIsCurrent(IDictionary<string, object> claims, DateTime moment)
        {
            object issuer;
            bool isOwnToken = claims.TryGetValue(IssuerClaim, out issuer) && (Issuer == (issuer as string));
            if (!isOwnToken)
            {
                throw new InvalidAccessTokenException("The token was not issued by this server.");
            }

            if (ReadExpiry(claims) <= ToUnixSeconds(moment))
            {
                throw new InvalidAccessTokenException("The token has expired.");
            }
        }

        private static long ReadExpiry(IDictionary<string, object> claims)
        {
            object expiry;
            if (!claims.TryGetValue(ExpirationClaim, out expiry))
            {
                throw new InvalidAccessTokenException("The token has no expiry.");
            }

            long expirySeconds;
            try
            {
                expirySeconds = Convert.ToInt64(expiry, CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                throw new InvalidAccessTokenException("The expiry of the token is not a number.", ex);
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidAccessTokenException("The expiry of the token is not a number.", ex);
            }
            catch (OverflowException ex)
            {
                throw new InvalidAccessTokenException("The expiry of the token is out of range.", ex);
            }

            return expirySeconds;
        }

        private static int ReadSubject(IDictionary<string, object> claims)
        {
            object subject;
            int playerId = 0;
            bool hasPlayerId = claims.TryGetValue(SubjectClaim, out subject)
                && int.TryParse(subject as string, NumberStyles.None, CultureInfo.InvariantCulture, out playerId);
            if (!hasPlayerId)
            {
                throw new InvalidAccessTokenException("The token does not identify a player.");
            }

            return playerId;
        }

        // The JSON is read only after its signature matched, but a malformed segment still has to end as an invalid token.
        private static IDictionary<string, object> ReadSegment(string segment)
        {
            IDictionary<string, object> values;
            try
            {
                string json = Encoding.UTF8.GetString(FromBase64Url(segment));
                values = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);
            }
            catch (FormatException ex)
            {
                throw new InvalidAccessTokenException("A segment of the token is not valid Base64Url.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidAccessTokenException("A segment of the token is not valid JSON.", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidAccessTokenException("A segment of the token is not a JSON object.", ex);
            }

            if (values == null)
            {
                throw new InvalidAccessTokenException("A segment of the token is empty.");
            }

            return values;
        }

        private static string EncodeText(string text)
        {
            return ToBase64Url(Encoding.UTF8.GetBytes(text));
        }

        // Base64Url is Base64 without padding and with the two characters that have a meaning in URLs replaced (RFC 7515).
        private static string ToBase64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd(Base64Padding).Replace('+', '-').Replace('/', '_');
        }

        private static byte[] FromBase64Url(string segment)
        {
            string base64 = segment.Replace('-', '+').Replace('_', '/');
            int paddingLength = (Base64BlockSize - (base64.Length % Base64BlockSize)) % Base64BlockSize;

            return Convert.FromBase64String(base64 + new string(Base64Padding, paddingLength));
        }

        private static long ToUnixSeconds(DateTime moment)
        {
            return (long)Math.Floor((moment - _unixEpoch).TotalSeconds);
        }

        // Every byte is compared without stopping at the first difference, so the time taken does not reveal how much matched.
        private static bool AreEqual(byte[] first, byte[] second)
        {
            int difference = first.Length ^ second.Length;
            int upperBound = Math.Min(first.Length, second.Length);
            for (int index = 0; index < upperBound; index++)
            {
                difference |= first[index] ^ second[index];
            }

            return difference == 0;
        }

        private void EnsureSignatureMatches(string[] segments)
        {
            string signingInput = string.Concat(segments[HeaderIndex], SegmentSeparator, segments[PayloadIndex]);
            byte[] presentedSignature;
            try
            {
                presentedSignature = FromBase64Url(segments[SignatureIndex]);
            }
            catch (FormatException ex)
            {
                throw new InvalidAccessTokenException("The signature of the token is not valid Base64Url.", ex);
            }

            if (!AreEqual(ComputeSignature(signingInput), presentedSignature))
            {
                throw new InvalidAccessTokenException("The signature of the token does not match its content.");
            }
        }

        private byte[] ComputeSignature(string signingInput)
        {
            byte[] signature;
            using (HMACSHA256 hmac = new HMACSHA256(_signingKey))
            {
                signature = hmac.ComputeHash(Encoding.ASCII.GetBytes(signingInput));
            }

            return signature;
        }
    }
}
