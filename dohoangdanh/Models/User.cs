namespace dohoangdanh.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string? FullName { get; set; }
        public string Role { get; set; } = "Parent"; // Admin | Parent | Child
        public int? ParentId { get; set; }
        public string? Otp { get; set; }
        public DateTime? OtpExpiry { get; set; }
    }
}