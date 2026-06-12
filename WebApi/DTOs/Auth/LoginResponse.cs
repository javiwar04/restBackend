namespace WebApi.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public string Id { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Rol { get; set; } = null!;
    public List<string> Modules { get; set; } = new();
}
