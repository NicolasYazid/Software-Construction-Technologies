using System;
using System.Collections.Generic;

using GinRummy.Domain.Shared;

namespace GinRummy.Domain.Entities.PlayerCustomization
{
    public class PlayerProfile
    {
        public const int UsernameMaxLength = 20;
        public const int BioMaxLength = 500;
        public const int ProfilePictureMaxLength = 255;

        public PlayerProfile(int playerAccountId, string username)
        {
            ValidatePlayerAccountId(playerAccountId);
            ValidateUsername(username);

            PlayerAccountId = playerAccountId;
            Username = username.Trim();
            SocialMediaLinks = new List<SocialMedia>();
        }

        protected PlayerProfile()
        {
            SocialMediaLinks = new List<SocialMedia>();
        }

        public int PlayerAccountId { get; private set; }
        public string Username { get; private set; }
        public int PublicTag { get; private set; }
        public string Bio { get; private set; }
        public string ProfilePicture { get; private set; }
        public virtual ICollection<SocialMedia> SocialMediaLinks { get; private set; }

        public static IList<UsernameRequirement> GetUnmetUsernameRequirements(string username)
        {
            string text;

            if (username != null)
            {
                text = username;
            }
            else
            {
                text = string.Empty;
            }

            List<UsernameRequirement> unmetRequirements = new List<UsernameRequirement>();

            if (string.IsNullOrWhiteSpace(text))
            {
                unmetRequirements.Add(UsernameRequirement.Required);
            }

            if (text.Trim().Length > UsernameMaxLength)
            {
                unmetRequirements.Add(UsernameRequirement.MaximumLength);
            }

            if (ForbiddenCharacterCommon.ContainsForbiddenCharacters(text))
            {
                unmetRequirements.Add(UsernameRequirement.NoForbiddenCharacters);
            }

            return unmetRequirements;
        }

        public void ChangeUsername(string username)
        {
            ValidateUsername(username);

            Username = username.Trim();
        }

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

        private static void ValidatePlayerAccountId(int playerAccountId)
        {
            if (playerAccountId <= 0)
            {
                throw new ArgumentException("Player account id must be positive.", nameof(playerAccountId));
            }
        }

        private static void ValidateUsername(string username)
        {
            IList<UsernameRequirement> unmetRequirements = GetUnmetUsernameRequirements(username);

            if (unmetRequirements.Count > 0)
            {
                throw new ArgumentException(
                    "The username does not meet these requirements: " + string.Join(", ", unmetRequirements) + ".",
                    nameof(username));
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