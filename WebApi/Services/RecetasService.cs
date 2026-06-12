using AccesoDatos.Context;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Inventario;

namespace WebApi.Services;

public class RecetasService
{
    private readonly RestauranteDbContext _context;

    public RecetasService(RestauranteDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecetaDto>> GetRecetasAsync()
    {
        var platillos = await _context.Platillos
            .Include(p => p.Receta)
            .ThenInclude(r => r.Insumo)
            .Where(p => p.Receta.Any())
            .ToListAsync();

        return platillos.Select(p => new RecetaDto
        {
            PlatilloId = p.Id,
            PlatilloNombre = p.Nombre,
            Ingredientes = p.Receta.Select(r => new IngredienteDto
            {
                InsumoId = r.InsumoId,
                InsumoNombre = r.Insumo.Nombre,
                Unidad = r.Insumo.Unidad,
                Cantidad = r.Cantidad
            }).ToList()
        }).ToList();
    }

    public async Task<RecetaDto?> GetRecetaByPlatilloIdAsync(string platilloId)
    {
        var platillo = await _context.Platillos
            .Include(p => p.Receta)
            .ThenInclude(r => r.Insumo)
            .FirstOrDefaultAsync(p => p.Id == platilloId);

        if (platillo == null)
            return null;

        return new RecetaDto
        {
            PlatilloId = platillo.Id,
            PlatilloNombre = platillo.Nombre,
            Ingredientes = platillo.Receta.Select(r => new IngredienteDto
            {
                InsumoId = r.InsumoId,
                InsumoNombre = r.Insumo.Nombre,
                Unidad = r.Insumo.Unidad,
                Cantidad = r.Cantidad
            }).ToList()
        };
    }

    public async Task<RecetaDto?> UpdateRecetaAsync(string platilloId, UpdateRecetaDto dto)
    {
        var platillo = await _context.Platillos
            .Include(p => p.Receta)
            .FirstOrDefaultAsync(p => p.Id == platilloId);

        if (platillo == null)
            return null;

        _context.Recetas.RemoveRange(platillo.Receta);
        await _context.SaveChangesAsync();

        foreach (var ingrediente in dto.Ingredientes)
        {
            var receta = new Receta
            {
                PlatilloId = platilloId,
                InsumoId = ingrediente.InsumoId,
                Cantidad = ingrediente.Cantidad
            };

            _context.Recetas.Add(receta);
        }

        await _context.SaveChangesAsync();

        return await GetRecetaByPlatilloIdAsync(platilloId);
    }

    public async Task<bool> DeleteRecetaAsync(string platilloId)
    {
        var platillo = await _context.Platillos
            .Include(p => p.Receta)
            .FirstOrDefaultAsync(p => p.Id == platilloId);

        if (platillo == null)
            return false;

        _context.Recetas.RemoveRange(platillo.Receta);
        await _context.SaveChangesAsync();

        return true;
    }
}
