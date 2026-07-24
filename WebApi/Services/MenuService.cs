using AccesoDatos.Context;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Menu;

namespace WebApi.Services;

public class MenuService
{
    private readonly RestauranteDbContext _context;

    public MenuService(RestauranteDbContext context)
    {
        _context = context;
    }

    // Categor�as
    public async Task<List<CategoriaMenuDto>> GetCategoriasAsync()
    {
        return await _context.CategoriasMenus
            .OrderBy(c => c.Orden)
            .Select(c => new CategoriaMenuDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Orden = c.Orden,
                Activa = c.Activa
            })
            .ToListAsync();
    }

    public async Task<CategoriaMenuDto?> GetCategoriaByIdAsync(string id)
    {
        var categoria = await _context.CategoriasMenus.FindAsync(id);
        
        if (categoria == null)
            return null;

        return new CategoriaMenuDto
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Orden = categoria.Orden,
            Activa = categoria.Activa
        };
    }

    public async Task<CategoriaMenuDto> CreateCategoriaAsync(CreateCategoriaMenuDto dto)
    {
        var categoria = new CategoriasMenu
        {
            Id = Guid.NewGuid().ToString(),
            Nombre = dto.Nombre,
            Orden = dto.Orden,
            Activa = dto.Activa
        };

        _context.CategoriasMenus.Add(categoria);
        await _context.SaveChangesAsync();

        return new CategoriaMenuDto
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Orden = categoria.Orden,
            Activa = categoria.Activa
        };
    }

    public async Task<CategoriaMenuDto?> UpdateCategoriaAsync(string id, UpdateCategoriaMenuDto dto)
    {
        var categoria = await _context.CategoriasMenus.FindAsync(id);
        
        if (categoria == null)
            return null;

        categoria.Nombre = dto.Nombre;
        categoria.Orden = dto.Orden;
        categoria.Activa = dto.Activa;

        await _context.SaveChangesAsync();

        return new CategoriaMenuDto
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Orden = categoria.Orden,
            Activa = categoria.Activa
        };
    }

    public async Task<bool> DeleteCategoriaAsync(string id)
    {
        var categoria = await _context.CategoriasMenus
            .Include(c => c.Platillos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null)
            return false;

        if (categoria.Platillos.Any())
            throw new InvalidOperationException("La categor�a tiene platillos asociados");

        _context.CategoriasMenus.Remove(categoria);
        await _context.SaveChangesAsync();

        return true;
    }

    // Platillos
    public async Task<List<PlatilloDto>> GetPlatillosAsync(string? categoriaId = null, bool? disponible = null, string? q = null, string? establecimientoId = null)
    {
        var query = _context.Platillos
            .Include(p => p.Categoria)
            .AsQueryable();

        // Solo los platillos ofrecidos en la sucursal activa
        if (!string.IsNullOrEmpty(establecimientoId))
            query = query.Where(p => _context.PlatillosEstablecimientos
                .Any(pe => pe.PlatilloId == p.Id && pe.EstablecimientoId == establecimientoId));

        if (!string.IsNullOrEmpty(categoriaId))
            query = query.Where(p => p.CategoriaId == categoriaId);

        if (disponible.HasValue)
            query = query.Where(p => p.Disponible == disponible.Value);

        if (!string.IsNullOrEmpty(q))
            query = query.Where(p => p.Nombre.Contains(q) || (p.Descripcion != null && p.Descripcion.Contains(q)));

        var platillos = await query
            .OrderBy(p => p.Categoria.Orden)
            .ThenBy(p => p.Nombre)
            .ToListAsync();

        // Sucursales asignadas por platillo (para la vista/edición del admin)
        var ids = platillos.Select(p => p.Id).ToList();
        var asignaciones = await _context.PlatillosEstablecimientos
            .Where(pe => ids.Contains(pe.PlatilloId))
            .ToListAsync();
        var porPlatillo = asignaciones.GroupBy(pe => pe.PlatilloId)
            .ToDictionary(g => g.Key, g => g.Select(pe => pe.EstablecimientoId).ToList());

        var result = new List<PlatilloDto>();

        foreach (var platillo in platillos)
        {
            var modificadores = await GetModificadoresPorPlatilloAsync(platillo.CategoriaId, platillo.Id);

            result.Add(new PlatilloDto
            {
                Id = platillo.Id,
                CategoriaId = platillo.CategoriaId,
                CategoriaNombre = platillo.Categoria.Nombre,
                Nombre = platillo.Nombre,
                Descripcion = platillo.Descripcion,
                Precio = platillo.Precio,
                Disponible = platillo.Disponible,
                ImagenUrl = platillo.ImagenUrl,
                Establecimientos = porPlatillo.TryGetValue(platillo.Id, out var es) ? es : new List<string>(),
                Modificadores = modificadores
            });
        }

        return result;
    }

    public async Task<PlatilloDto?> GetPlatilloByIdAsync(string id)
    {
        var platillo = await _context.Platillos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (platillo == null)
            return null;

        var modificadores = await GetModificadoresPorPlatilloAsync(platillo.CategoriaId, platillo.Id);
        var estIds = await _context.PlatillosEstablecimientos
            .Where(pe => pe.PlatilloId == platillo.Id)
            .Select(pe => pe.EstablecimientoId)
            .ToListAsync();

        return new PlatilloDto
        {
            Id = platillo.Id,
            CategoriaId = platillo.CategoriaId,
            CategoriaNombre = platillo.Categoria.Nombre,
            Nombre = platillo.Nombre,
            Descripcion = platillo.Descripcion,
            Precio = platillo.Precio,
            Disponible = platillo.Disponible,
            ImagenUrl = platillo.ImagenUrl,
            Establecimientos = estIds,
            Modificadores = modificadores
        };
    }

    // Sincroniza en qué sucursales está el platillo (reemplaza las asignaciones)
    private async Task SincronizarEstablecimientosAsync(string platilloId, List<string> establecimientoIds)
    {
        var existentes = await _context.PlatillosEstablecimientos
            .Where(pe => pe.PlatilloId == platilloId)
            .ToListAsync();
        if (existentes.Any())
        {
            _context.PlatillosEstablecimientos.RemoveRange(existentes);
            await _context.SaveChangesAsync();
        }

        var idsValidos = establecimientoIds.Distinct().ToList();
        foreach (var estId in idsValidos)
        {
            _context.PlatillosEstablecimientos.Add(new PlatilloEstablecimiento
            {
                PlatilloId = platilloId,
                EstablecimientoId = estId
            });
        }
        await _context.SaveChangesAsync();
    }

    public async Task<PlatilloDto> CreatePlatilloAsync(CreatePlatilloDto dto)
    {
        var categoria = await _context.CategoriasMenus.FindAsync(dto.CategoriaId);
        if (categoria == null)
            throw new InvalidOperationException("Categor�a no encontrada");

        var platillo = new Platillo
        {
            Id = Guid.NewGuid().ToString(),
            CategoriaId = dto.CategoriaId,
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Disponible = dto.Disponible,
            ImagenUrl = dto.ImagenUrl,
            CreadoEn = DateTime.UtcNow
        };

        _context.Platillos.Add(platillo);
        await _context.SaveChangesAsync();

        // ? Solo sincronizar si el campo modificadores viene en el request
        // null = No envi� el campo ? No tocar modificadores
        // [] = Envi� array vac�o ? Borrar todos
        // [...] = Envi� array con elementos ? Crear esos
        if (dto.Modificadores != null)
        {
            await SincronizarModificadoresAsync(platillo.Id, dto.Modificadores);
        }

        // Sucursales donde se ofrece el platillo
        if (dto.EstablecimientoIds != null)
        {
            await SincronizarEstablecimientosAsync(platillo.Id, dto.EstablecimientoIds);
        }

        return await GetPlatilloByIdAsync(platillo.Id) ?? throw new InvalidOperationException("Error al crear platillo");
    }

    public async Task<PlatilloDto?> UpdatePlatilloAsync(string id, UpdatePlatilloDto dto)
    {
        var platillo = await _context.Platillos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (platillo == null)
            return null;

        var categoria = await _context.CategoriasMenus.FindAsync(dto.CategoriaId);
        if (categoria == null)
            throw new InvalidOperationException("Categor�a no encontrada");

        platillo.CategoriaId = dto.CategoriaId;
        platillo.Nombre = dto.Nombre;
        platillo.Descripcion = dto.Descripcion;
        platillo.Precio = dto.Precio;
        platillo.Disponible = dto.Disponible;
        platillo.ImagenUrl = dto.ImagenUrl;

        await _context.SaveChangesAsync();

        // ? Solo sincronizar si el campo modificadores viene en el request
        // null = No envi� el campo ? No tocar modificadores
        // [] = Envi� array vac�o ? Borrar todos
        // [...] = Envi� array con elementos ? Reemplazar todos
        if (dto.Modificadores != null)
        {
            await SincronizarModificadoresAsync(id, dto.Modificadores);
        }

        if (dto.EstablecimientoIds != null)
        {
            await SincronizarEstablecimientosAsync(id, dto.EstablecimientoIds);
        }

        return await GetPlatilloByIdAsync(id);
    }

    public async Task<bool> UpdateDisponibleAsync(string id, bool disponible)
    {
        var platillo = await _context.Platillos.FindAsync(id);

        if (platillo == null)
            return false;

        platillo.Disponible = disponible;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeletePlatilloAsync(string id)
    {
        var platillo = await _context.Platillos.FindAsync(id);

        if (platillo == null)
            return false;

        _context.Platillos.Remove(platillo);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<List<ModificadorGrupoDto>> GetModificadoresPorPlatilloAsync(string categoriaId, string platilloId)
    {
        var result = new List<ModificadorGrupoDto>();

        try
        {
            // ? 1. Modificadores PROPIOS del platillo (nueva funcionalidad)
            var modificadoresPropios = await _context.ModificadorGrupos
                .Where(g => g.PlatilloId == platilloId)
                .Include(g => g.Opciones)
                .ThenInclude(o => o.Insumo)
                .OrderBy(g => g.Orden)
                .ToListAsync();

            if (modificadoresPropios != null && modificadoresPropios.Any())
            {
                foreach (var grupo in modificadoresPropios)
                {
                    result.Add(new ModificadorGrupoDto
                    {
                        GrupoId = grupo.Id,
                        GrupoNombre = grupo.Nombre,
                        Tipo = grupo.Tipo,
                        Obligatorio = grupo.Obligatorio,
                        MinSelecciones = grupo.MinSelecciones,
                        MaxSelecciones = grupo.MaxSelecciones,
                        Orden = grupo.Orden,
                        Opciones = grupo.Opciones?
                            .OrderBy(o => o.Orden)
                            .Select(o => new ModificadorOpcionDto
                            {
                                Id = o.Id,
                                Nombre = o.Nombre,
                                PrecioDelta = o.PrecioDelta,
                                InsumoId = o.InsumoId,
                                InsumoNombre = o.Insumo?.Nombre,
                                CantidadInsumo = o.CantidadInsumo,
                                EsDefault = o.EsDefault,
                                Activo = o.Activo,
                                Orden = o.Orden
                            }).ToList() ?? new List<ModificadorOpcionDto>()
                    });
                }
            }

            // ? 2. Modificadores de CATEGOR�A (sistema anterior - mantener compatibilidad)
            var gruposDeCategoria = await _context.CategoriasMenus
                .Where(c => c.Id == categoriaId)
                .SelectMany(c => c.Grupos)
                .Include(g => g.ModificadoresOpcions)
                .ToListAsync();

            var overrides = await _context.PlatilloModificadoresOverrides
                .Where(o => o.PlatilloId == platilloId)
                .ToDictionaryAsync(o => o.GrupoId, o => o.Habilitado);

            if (gruposDeCategoria != null && gruposDeCategoria.Any())
            {
                foreach (var grupo in gruposDeCategoria)
                {
                    if (overrides.TryGetValue(grupo.Id, out var habilitado) && !habilitado)
                        continue;

                    result.Add(new ModificadorGrupoDto
                    {
                        GrupoId = grupo.Id,
                        GrupoNombre = grupo.Nombre,
                        Tipo = grupo.Tipo,
                        Obligatorio = false,
                        MinSelecciones = 0,
                        MaxSelecciones = 0,
                        Orden = 999,
                        Opciones = grupo.ModificadoresOpcions?
                            .Select(o => new ModificadorOpcionDto
                            {
                                Id = o.Id,
                                Nombre = o.Nombre,
                                PrecioDelta = o.PrecioDelta,
                                EsDefault = o.EsDefault,
                                Activo = true,
                                Orden = 0
                            }).ToList() ?? new List<ModificadorOpcionDto>()
                    });
                }
            }
        }
        catch (Exception)
        {
            // Si falla al obtener modificadores, devolver lista vac�a para que el platillo se muestre igual
            return new List<ModificadorGrupoDto>();
        }

        return result.OrderBy(m => m.Orden).ToList();
    }

    /// <summary>
    /// Sincroniza los modificadores propios de un platillo (borra existentes y crea nuevos)
    /// </summary>
    private async Task SincronizarModificadoresAsync(string platilloId, List<CreateModificadorGrupoDto>? modificadores)
    {
        // ? 1. SIEMPRE borrar grupos existentes (sea [] o con elementos)
        var gruposExistentes = await _context.ModificadorGrupos
            .Where(g => g.PlatilloId == platilloId)
            .ToListAsync();

        if (gruposExistentes.Any())
        {
            _context.ModificadorGrupos.RemoveRange(gruposExistentes);
            await _context.SaveChangesAsync();
        }

        // ? 2. Si modificadores es null o vac�o, solo borrar y terminar
        if (modificadores == null || !modificadores.Any())
            return;

        // ? 3. Crear los nuevos grupos y opciones
        foreach (var grupoDto in modificadores)
        {
            var nuevoGrupo = new ModificadorGrupo
            {
                Id = Guid.NewGuid().ToString(),
                PlatilloId = platilloId,
                Nombre = grupoDto.GrupoNombre,
                Tipo = grupoDto.Tipo,
                Obligatorio = grupoDto.Obligatorio,
                MinSelecciones = grupoDto.MinSelecciones,
                MaxSelecciones = grupoDto.MaxSelecciones,
                Orden = grupoDto.Orden
            };

            _context.ModificadorGrupos.Add(nuevoGrupo);
            await _context.SaveChangesAsync(); // Guardar para obtener el ID

            // Agregar opciones del grupo (si tiene)
            if (grupoDto.Opciones != null && grupoDto.Opciones.Any())
            {
                foreach (var opcionDto in grupoDto.Opciones)
                {
                    var nuevaOpcion = new ModificadorOpcion
                    {
                        Id = Guid.NewGuid().ToString(),
                        GrupoId = nuevoGrupo.Id,
                        Nombre = opcionDto.Nombre,
                        PrecioDelta = opcionDto.PrecioDelta,
                        InsumoId = string.IsNullOrWhiteSpace(opcionDto.InsumoId) ? null : opcionDto.InsumoId,
                        CantidadInsumo = opcionDto.CantidadInsumo,
                        EsDefault = opcionDto.EsDefault,
                        Activo = opcionDto.Activo,
                        Orden = opcionDto.Orden
                    };

                    _context.ModificadorOpciones.Add(nuevaOpcion);
                }
            }
        }

        await _context.SaveChangesAsync();
    }
}
