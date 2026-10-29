namespace dohoangdanh.Models
{
    public record RegisterDto(string Email, string Password, string? FullName, string? Role = null);
    public record LoginDto(string Email, string Password);
    public record OtpDto(string Email, string Otp);

}