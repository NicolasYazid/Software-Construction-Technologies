using System;
using System.Collections.Generic;

namespace GinRummy.Domain.Entities.Players
{
    public class PlayerProfile
    {
        public const int BioMaxLength = 500;
        public const int ProfilePictureMaxLength = 255;

        public PlayerProfile(int playerId)
        {
            ValidatePlayerId(playerId);

            PlayerId = playerId;
            SocialMediaLinks = new List<SocialMedia>();
        }

        protected PlayerProfile()
        {
            SocialMediaLinks = new List<SocialMedia>();
        }

        public int PlayerId { get; private set; }
        public string Bio { get; private set; }
        public string ProfilePicture { get; private set; }
        public virtual PlayerAccount PlayerAccount { get; private set; }
        public virtual ICollection<SocialMedia> SocialMediaLinks { get; private set; }

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

        public void ChangeProfilePicture(string profilePicture)
        {
            ValidateProfilePicture(profilePicture);

            ProfilePicture = profilePicture.Trim();
        }

        public void RemoveProfilePicture()
        {
            ProfilePicture = null;
        }

        private static void ValidatePlayerId(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentException("Player id must be positive.", nameof(playerId));
            }
        }

        private static void ValidateBio(string bio)
        {
            if ((bio != null) && (bio.Length > BioMaxLength))
            {
                throw new ArgumentException("Bio is too long.", nameof(bio));
            }
        }

        private static void ValidateProfilePicture(string profilePicture)
        {
            if (string.IsNullOrWhiteSpace(profilePicture))
            {
                throw new ArgumentException("Profile picture path is required.", nameof(profilePicture));
            }

            if (profilePicture.Trim().Length > ProfilePictureMaxLength)
            {
                throw new ArgumentException("Profile picture path is too long.", nameof(profilePicture));
            }
        }
    }
}
