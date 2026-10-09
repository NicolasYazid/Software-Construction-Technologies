namespace GinRummy.Domain.Dtos
{
    public class NewPlayerDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int LocaleId { get; set; }
    }
}
