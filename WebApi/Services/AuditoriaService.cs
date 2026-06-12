using AccesoDatos.Context;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Auditoria;

namespace WebApi.Services;

public class AuditoriaService
{
    private readonly RestauranteDbContext _context;

    public AuditoriaService(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<AuditoriaListDto> GetAuditoriaAsync(
        DateTime? desde = null,
        DateTime? hasta = null,
        string? usuarioId = null,
        string? accion = null,
        int pagina = 1,
        int porPagina = 50)
    {
        var query = _context.Auditoria.AsQueryable();

        if (desde.HasValue)
            query = query.Where(a => a.RegistradoEn >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(a => a.RegistradoEn <= hasta.Value);

        if (!string.IsNullOrEmpty(usuarioId))
            query = query.Where(a => a.UsuarioId == usuarioId);

        if (!string.IsNullOrEmpty(accion))
            query = query.Where(a => a.Accion == accion);

        var total = await query.CountAsync();

        var registros = await query
            .OrderByDescending(a => a.RegistradoEn)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .ToListAsync();

        return new AuditoriaListDto
        {
            Total = total,
            Datos = registros.Select(a => new AuditoriaDto
            {
                Id = a.Id,
                UsuarioId = a.UsuarioId ?? "",
                UsuarioNombre = a.UsuarioNombre ?? "",
                Accion = a.Accion,
                Descripcion = a.Descripcion ?? "",
                Ip = null,
                CreadoEn = a.RegistradoEn
            }).ToList()
        };
    }

    public async Task RegistrarAsync(string usuarioId, string usuarioNombre, string accion, string descripcion, string? ip = null)
    {
        var registro = new Auditorium
        {
            UsuarioId = usuarioId,
            UsuarioNombre = usuarioNombre,
            Accion = accion,
            Descripcion = descripcion,
            RegistradoEn = DateTime.UtcNow
        };

        _context.Auditoria.Add(registro);
        await _context.SaveChangesAsync();
    }
}
