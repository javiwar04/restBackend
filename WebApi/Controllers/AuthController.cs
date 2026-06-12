using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.DTOs;
using WebApi.DTOs.Auth;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Pin))
            return BadRequest(new ErrorResponse { Error = "Username y PIN son requeridos" });

        var result = await _authService.LoginAsync(request.Username, request.Pin);

        if (result == null)
            return Unauthorized(new ErrorResponse { Error = "Credenciales inválidas" });

        return Ok(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // Con JWT stateless, simplemente devolvemos OK
        // El frontend eliminará el token
        return Ok(new { ok = true });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new ErrorResponse { Error = "No autorizado" });

        var user = await _authService.GetUserByIdAsync(userId);

        if (user == null)
            return NotFound(new ErrorResponse { Error = "Usuario no encontrado" });

        return Ok(user);
    }
}
