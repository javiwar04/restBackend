using System.Security.Claims;

namespace WebApi.Extensions;

public static class HttpContextExtensions
{
    /// <summary>
    /// Sucursal activa enviada por el cliente en el header X-Establecimiento.
    /// null si no se envió (el llamador decide si es requerido).
    /// </summary>
    public static string? GetEstablecimiento(this HttpContext ctx)
    {
        var val = ctx.Request.Headers["X-Establecimiento"].FirstOrDefault();
        return string.IsNullOrWhiteSpace(val) ? null : val;
    }

    public static string? GetUsuarioId(this HttpContext ctx) =>
        ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public static bool EsAdmin(this HttpContext ctx) => ctx.User.IsInRole("admin");
}
