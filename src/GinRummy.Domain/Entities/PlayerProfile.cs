using System;
using System.Collections.Generic;
using System.Linq;

namespace GinRummy.Domain.Entities
{
    /// <summary>
    /// The public data of a registered player's profile, mapped to the PlayerProfile table:
    /// the biography, the picture and the social media links. It shares the key of the
    /// account that owns it. Protects its own invariants: text fields never exceed their
    /// columns.
    /// </summary>
    public class PlayerProfile
    {
        private const int MaxBioLength = 500;
        private const int MaxProfilePictureLength = 255;

        /// <summary>
        /// Creates the empty profile every new account starts with: no biography and no
        /// picture.
        /// </summary>
        /// <param name="playerId">Identifier of the account that owns the profile; must be positive.</param>
        public PlayerProfile(int playerId)
        {
            ValidatePlayerId(playerId);

            PlayerId = playerId;
            SocialMediaLinks = new List<SocialMedia>();
        }

        /// <summary>
        /// Parameterless constructor reserved for Entity Framework's materialization; not
        /// for application code, which must use the validating constructor.
        /// </summary>
        protected PlayerProfile()
        {
            SocialMediaLinks = new List<SocialMedia>();
        }

        /// <summary>
        /// Gets the identifier of the account that owns the profile, which is also the key of
        /// the profile.
        /// </summary>
        public int PlayerId { get; private set; }

        /// <summary>
        /// Gets the text the player uses to introduce themselves. Null while the player has
        /// not written one.
        /// </summary>
        public string Bio { get; private set; }

        /// <summary>
        /// Gets the path of the profile picture file, never the image itself. Null while the
        /// player uses the default avatar.
        /// </summary>
        public string ProfilePicture { get; private set; }

        /// <summary>
        /// Gets the account that owns the profile, loaded from the relationship between the
        /// two tables.
        /// </summary>
        public virtual PlayerAccount PlayerAccount { get; private set; }

        /// <summary>
        /// Gets the social media links the profile shows.
        /// </summary>
        public virtual ICollection<SocialMedia> SocialMediaLinks { get; private set; }

        /// <summary>
        /// Tells whether the profile already shows a link to the given platform.
        /// </summary>
        /// <param name="platformId">Identifier of the platform to look for.</param>
        /// <returns>True when a link to that platform exists.</returns>
        public bool HasLinkFor(int platformId)
        {
            bool hasLink = SocialMediaLinks.Any(link => link.PlatformId == platformId);

            return hasLink;
        }

        /// <summary>
        /// Replaces the biography. A blank text clears it.
        /// </summary>
        /// <param name="bio">New text, kept as written; at most 500 characters.</param>
        public void ChangeBio(string bio)
        {
            ValidateBio(bio);

            string newBio = null;
            if (!string.IsNullOrWhiteSpace(bio))
            {
                newBio = bio;
            }

            Bio = newBio;
        }

        /// <summary>
        /// Replaces the profile picture with the file stored at the given path.
        /// </summary>
        /// <param name="profilePicture">Path of the stored file; required, at most 255 characters.</param>
        public void ChangeProfilePicture(string profilePicture)
        {
            ValidateProfilePicture(profilePicture);

            ProfilePicture = profilePicture.Trim();
        }

        /// <summary>
        /// Removes the profile picture so the default avatar is shown.
        /// </summary>
        public void RemoveProfilePicture()
        {
            ProfilePicture = null;
        }

        // The key of the profile is the key of an account that already exists.
        private static void ValidatePlayerId(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentException("Player id must be positive.", nameof(playerId));
            }
        }

        // The biography must fit the column; it may be empty.
        private static void ValidateBio(string bio)
        {
            if ((bio != null) && (bio.Length > MaxBioLength))
            {
                throw new ArgumentException("Bio is too long.", nameof(bio));
            }
        }

        // The column stores a path, so it can never be empty or longer than the column.
        private static void ValidateProfilePicture(string profilePicture)
        {
            if (string.IsNullOrWhiteSpace(profilePicture))
            {
                throw new ArgumentException("Profile picture path is required.", nameof(profilePicture));
            }

            if (profilePicture.Trim().Length > MaxProfilePictureLength)
            {
                throw new ArgumentException("Profile picture path is too long.", nameof(profilePicture));
            }
        }
    }
}
