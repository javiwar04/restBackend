using AccesoDatos.Context;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Inventario;

namespace WebApi.Services;

public class CorteInventarioService
{
    private readonly RestauranteDbContext _context;

    public CorteInventarioService(RestauranteDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Cuánto DEBIÓ consumirse de cada insumo en el turno según lo vendido y las
    /// recetas (Σ receta.cantidad × cantidad vendida). Insumo sin receta => 0.
    /// </summary>
    private async Task<Dictionary<string, decimal>> VendidoTeoricoAsync(string turnoId)
    {
        var turno = await _context.Turnos.FindAsync(turnoId);
        var establecimientoId = turno?.EstablecimientoId;

        // Items vendidos (órdenes pagadas del turno) con su platillo
        var items = await _context.OrdenItems
            .Include(i => i.OrdenItemModificadores)
            .Where(i => i.Orden.TurnoId == turnoId && i.Orden.Estado == "pagado" && i.PlatilloId != null)
            .ToListAsync();

        if (items.Count == 0) return new Dictionary<string, decimal>();

        var platilloIds = items.Select(i => i.PlatilloId!).Distinct().ToList();
        var recetas = await _context.Recetas
            .Include(r => r.Insumo)
            .Where(r => platilloIds.Contains(r.PlatilloId))
            .ToListAsync();

        var recetasPorPlatillo = recetas.GroupBy(r => r.PlatilloId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var opcionIds = items
            .SelectMany(i => i.OrdenItemModificadores)
            .Select(m => m.OpcionId)
            .Where(id => !string.IsNullOrEmpty(id))
            .Select(id => id!)
            .Distinct()
            .ToList();

        var opcionesInventario = opcionIds.Count == 0
            ? new Dictionary<string, AccesoDatos.Models.ModificadorOpcion>()
            : await _context.ModificadorOpciones
                .Include(o => o.Insumo)
                .Where(o => opcionIds.Contains(o.Id) && o.InsumoId != null && o.CantidadInsumo != null && o.CantidadInsumo > 0)
                .ToDictionaryAsync(o => o.Id, o => o);

        // Nombre -> insumoId de la sucursal del turno (para atribuir al insumo correcto)
        var insumosSucursal = string.IsNullOrEmpty(establecimientoId)
            ? new Dictionary<string, string>()
            : await _context.Insumos
                .Where(i => i.EstablecimientoId == establecimientoId)
                .ToDictionaryAsync(i => i.Nombre, i => i.Id);

        var teorico = new Dictionary<string, decimal>();
        foreach (var it in items)
        {
            if (!recetasPorPlatillo.TryGetValue(it.PlatilloId!, out var recs)) continue;
            foreach (var r in recs)
            {
                // Resolver al insumo de la sucursal por nombre; fallback al de la receta
                var insumoId = insumosSucursal.TryGetValue(r.Insumo.Nombre, out var id) ? id : r.InsumoId;
                teorico.TryGetValue(insumoId, out var acc);
                teorico[insumoId] = acc + r.Cantidad * it.Cantidad;
            }

            foreach (var mod in it.OrdenItemModificadores)
            {
                if (mod.OpcionId == null || !opcionesInventario.TryGetValue(mod.OpcionId, out var opt) || opt.Insumo == null)
                    continue;

                var insumoId = insumosSucursal.TryGetValue(opt.Insumo.Nombre, out var id) ? id : opt.InsumoId;
                if (string.IsNullOrEmpty(insumoId)) continue;
                teorico.TryGetValue(insumoId, out var acc);
                teorico[insumoId] = acc + (opt.CantidadInsumo ?? 0) * it.Cantidad;
            }
        }
        return teorico;
    }

    public async Task<PreconteoDto?> GetPreconteoAsync(string turnoId)
    {
        var turno = await _context.Turnos.FindAsync(turnoId);
        if (turno == null) return null;

        var insumos = await _context.Insumos
            .Where(i => i.Activo && (turno.EstablecimientoId == null || i.EstablecimientoId == turno.EstablecimientoId))
            .OrderBy(i => i.Nombre)
            .ToListAsync();

        // "Encontré" sugerido = quedó del último corte de esta sucursal
        var ultimoCorte = await _context.CorteInventarios
            .Where(c => c.EstablecimientoId == turno.EstablecimientoId && c.TurnoId != turnoId)
            .OrderByDescending(c => c.Fecha)
            .FirstOrDefaultAsync();

        var ultimoQuedo = new Dictionary<string, decimal>();
        if (ultimoCorte != null)
        {
            ultimoQuedo = await _context.CorteInventarioDetalles
                .Where(d => d.CorteId == ultimoCorte.Id)
                .ToDictionaryAsync(d => d.InsumoId, d => d.Quedo);
        }

        var teorico = await VendidoTeoricoAsync(turnoId);

        return new PreconteoDto
        {
            TurnoId = turnoId,
            EstablecimientoId = turno.EstablecimientoId,
            Items = insumos.Select(i => new PreconteoItemDto
            {
                InsumoId = i.Id,
                Nombre = i.Nombre,
                Unidad = i.Unidad,
                CostoUnitario = i.CostoPorUnidad,
                Encontre = ultimoQuedo.TryGetValue(i.Id, out var q) ? q : i.StockActual,
                Ingreso = 0,
                Quedo = i.StockActual,
                VendidoTeorico = teorico.TryGetValue(i.Id, out var t) ? t : 0
            }).ToList()
        };
    }

    public async Task<CorteInventarioDto> CreateCorteAsync(CreateCorteInventarioDto dto, string? usuarioId)
    {
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var turno = await _context.Turnos.FindAsync(dto.TurnoId)
                ?? throw new InvalidOperationException("Turno no encontrado");

            var teorico = await VendidoTeoricoAsync(dto.TurnoId);

            var corte = new CorteInventario
            {
                Id = Guid.NewGuid().ToString(),
                TurnoId = dto.TurnoId,
                EstablecimientoId = turno.EstablecimientoId,
                Fecha = DateTime.UtcNow,
                Notas = dto.Notas,
                RegistradoEn = DateTime.UtcNow
            };
            _context.CorteInventarios.Add(corte);

            decimal totalMerma = 0;
            foreach (var d in dto.Detalles)
            {
                var insumo = await _context.Insumos.FindAsync(d.InsumoId);
                if (insumo == null) continue;

                var consumido = d.Encontre + d.Ingreso - d.Quedo;
                var vt = teorico.TryGetValue(d.InsumoId, out var t) ? t : 0;
                var merma = consumido - vt;
                var costo = insumo.CostoPorUnidad;
                totalMerma += merma * costo;

                _context.CorteInventarioDetalles.Add(new CorteInventarioDetalle
                {
                    CorteId = corte.Id,
                    InsumoId = d.InsumoId,
                    Encontre = d.Encontre,
                    Ingreso = d.Ingreso,
                    Quedo = d.Quedo,
                    VendidoTeorico = vt,
                    ConsumidoFisico = consumido,
                    Merma = merma,
                    CostoUnitario = costo
                });

                // Ajustar el stock del sistema al conteo físico (quedó) y dejar rastro
                if (insumo.StockActual != d.Quedo)
                {
                    _context.InsumosMovimientos.Add(new InsumosMovimiento
                    {
                        InsumoId = insumo.Id,
                        Tipo = "ajuste",
                        Cantidad = Math.Abs(d.Quedo - insumo.StockActual),
                        CostoPorUnidad = costo,
                        Motivo = $"Corte de inventario (turno {dto.TurnoId})",
                        UsuarioId = usuarioId,
                        RegistradoEn = DateTime.UtcNow
                    });
                    insumo.StockActual = d.Quedo;
                }
            }

            corte.TotalMermaValor = totalMerma;
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return (await GetCorteAsync(corte.Id))!;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<CorteInventarioDto?> GetCorteAsync(string id)
    {
        var corte = await _context.CorteInventarios
            .Include(c => c.Detalles).ThenInclude(d => d.Insumo)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (corte == null) return null;

        return new CorteInventarioDto
        {
            Id = corte.Id,
            TurnoId = corte.TurnoId,
            Fecha = corte.Fecha,
            TotalMermaValor = corte.TotalMermaValor,
            Detalles = corte.Detalles.OrderBy(d => d.Insumo.Nombre).Select(d => new CorteDetalleDto
            {
                InsumoId = d.InsumoId,
                InsumoNombre = d.Insumo.Nombre,
                Unidad = d.Insumo.Unidad,
                Encontre = d.Encontre,
                Ingreso = d.Ingreso,
                Quedo = d.Quedo,
                VendidoTeorico = d.VendidoTeorico,
                ConsumidoFisico = d.ConsumidoFisico,
                Merma = d.Merma,
                ValorMerma = d.Merma * d.CostoUnitario
            }).ToList()
        };
    }
}
