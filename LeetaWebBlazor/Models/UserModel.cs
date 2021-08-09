namespace LeetaWebBlazor.Models
{
    public class UserModel
    {
        public long Id { get; set; }

        public string Alias { get; set; }

        public string EmailAddress { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        public string Password { get; set; }
    }

    public enum UserRole : int
    {
        Administrator = 2,
        Moderator = 1,
        User = 0
    }
}
