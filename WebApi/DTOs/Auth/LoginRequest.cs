namespace WebApi.DTOs.Auth;

public class LoginRequest
{
    public string Username { get; set; } = null!;
    public string Pin { get; set; } = null!;
}
