using AccesoDatos.Context;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.Config;

namespace WebApi.Services;

public class ConfigService
{
    private readonly RestauranteDbContext _context;
    private readonly HashService _hashService;

    public ConfigService(RestauranteDbContext context, HashService hashService)
    {
        _context = context;
        _hashService = hashService;
    }

    public async Task<ConfigNegocioDto> GetConfigNegocioAsync()
    {
        var config = await _context.ConfigNegocios.FirstOrDefaultAsync();

        if (config == null)
        {
            return new ConfigNegocioDto
            {
                Nombre = "Restaurante SF",
                Moneda = "MXN",
                ZonaHoraria = "America/Mexico_City"
            };
        }

        return new ConfigNegocioDto
        {
            Nombre = config.Nombre,
            Rfc = config.Rfc,
            Direccion = config.Direccion,
            Telefono = config.Telefono,
            Email = config.Email,
            Logo = null,
            Moneda = config.Moneda ?? "MXN",
            ZonaHoraria = config.ZonaHoraria ?? "America/Mexico_City",
            TicketHeader = config.TicketEncabezado,
            TicketFooter = config.TicketPie
        };
    }

    public async Task<ConfigNegocioDto> UpdateConfigNegocioAsync(ConfigNegocioDto dto)
    {
        var config = await _context.ConfigNegocios.FirstOrDefaultAsync();

        if (config == null)
        {
            config = new ConfigNegocio
            {
                Id = 1,
                Nombre = dto.Nombre,
                Rfc = dto.Rfc,
                Direccion = dto.Direccion,
                Telefono = dto.Telefono,
                Email = dto.Email,
                Moneda = dto.Moneda,
                ZonaHoraria = dto.ZonaHoraria,
                TicketEncabezado = dto.TicketHeader,
                TicketPie = dto.TicketFooter
            };

            _context.ConfigNegocios.Add(config);
        }
        else
        {
            config.Nombre = dto.Nombre;
            config.Rfc = dto.Rfc;
            config.Direccion = dto.Direccion;
            config.Telefono = dto.Telefono;
            config.Email = dto.Email;
            config.Moneda = dto.Moneda;
            config.ZonaHoraria = dto.ZonaHoraria;
            config.TicketEncabezado = dto.TicketHeader;
            config.TicketPie = dto.TicketFooter;
        }

        await _context.SaveChangesAsync();

        return dto;
    }

    public async Task<ConfigImpuestosDto> GetConfigImpuestosAsync()
    {
        var config = await _context.ConfigImpuestos.FirstOrDefaultAsync();

        if (config == null)
        {
            return new ConfigImpuestosDto
            {
                IvaActivo = true,
                IvaPorcentaje = 16.0m,
                IepsTabaco = 0,
                IepsBebidas = 0,
                PreciosConIva = false,
                PropinaActiva = false,
                PropinaSugerida = 10m,
                CargoServicioActivo = false,
                CargoServicioPorcentaje = 0
            };
        }

        return new ConfigImpuestosDto
        {
            IvaActivo = config.IvaHabilitado,
            IvaPorcentaje = config.IvaTasa * 100,
            IepsTabaco = 0,
            IepsBebidas = 0,
            PreciosConIva = config.IvaIncluido,
            PropinaActiva = config.PropinaHabilitada,
            PropinaSugerida = config.PropinaSugerida * 100,
            CargoServicioActivo = config.CargoServicioHabilitado,
            CargoServicioPorcentaje = config.CargoServicioTasa * 100
        };
    }

    public async Task<ConfigImpuestosDto> UpdateConfigImpuestosAsync(ConfigImpuestosDto dto)
    {
        var config = await _context.ConfigImpuestos.FirstOrDefaultAsync();

        if (config == null)
        {
            config = new ConfigImpuesto
            {
                Id = 1,
                IvaHabilitado = dto.IvaActivo,
                IvaTasa = dto.IvaPorcentaje / 100,
                IvaIncluido = dto.PreciosConIva,
                PropinaHabilitada = dto.PropinaActiva,
                PropinaSugerida = dto.PropinaSugerida / 100,
                PropinaAuto = false,
                PropinaAutoMinComensales = 6,
                PropinaAutoTasa = dto.PropinaSugerida / 100,
                CargoServicioHabilitado = dto.CargoServicioActivo,
                CargoServicioTasa = dto.CargoServicioPorcentaje / 100
            };

            _context.ConfigImpuestos.Add(config);
        }
        else
        {
            config.IvaHabilitado = dto.IvaActivo;
            config.IvaTasa = dto.IvaPorcentaje / 100;
            config.IvaIncluido = dto.PreciosConIva;
            config.PropinaHabilitada = dto.PropinaActiva;
            config.PropinaSugerida = dto.PropinaSugerida / 100;
            config.CargoServicioHabilitado = dto.CargoServicioActivo;
            config.CargoServicioTasa = dto.CargoServicioPorcentaje / 100;
        }

        await _context.SaveChangesAsync();

        return await GetConfigImpuestosAsync();
    }

    // ?? Métodos de Pago ????????????????????????????????????????????????????????

    public async Task<List<MetodoPagoDto>> GetMetodosPagoAsync()
    {
        var metodos = await _context.MetodosPagos
            .OrderBy(m => m.Nombre)
            .ToListAsync();

        return metodos.Select(m => new MetodoPagoDto
        {
            Id = m.Id,
            Nombre = m.Nombre,
            Codigo = m.Codigo,
            Activo = m.Activo,
            RequiereReferencia = m.RequiereReferencia
        }).ToList();
    }

    public async Task<MetodoPagoDto> CreateMetodoPagoAsync(CreateMetodoPagoDto dto)
    {
        var metodo = new MetodosPago
        {
            Id = Guid.NewGuid().ToString(),
            Nombre = dto.Nombre,
            Codigo = dto.Codigo,
            Activo = true,
            RequiereReferencia = dto.RequiereReferencia
        };

        _context.MetodosPagos.Add(metodo);
        await _context.SaveChangesAsync();

        return new MetodoPagoDto
        {
            Id = metodo.Id,
            Nombre = metodo.Nombre,
            Codigo = metodo.Codigo,
            Activo = metodo.Activo,
            RequiereReferencia = metodo.RequiereReferencia
        };
    }

    public async Task<MetodoPagoDto?> UpdateMetodoPagoAsync(string id, UpdateMetodoPagoDto dto)
    {
        var metodo = await _context.MetodosPagos.FindAsync(id);

        if (metodo == null)
            return null;

        if (dto.Nombre != null)
            metodo.Nombre = dto.Nombre;

        if (dto.Activo.HasValue)
            metodo.Activo = dto.Activo.Value;

        if (dto.RequiereReferencia.HasValue)
            metodo.RequiereReferencia = dto.RequiereReferencia.Value;

        await _context.SaveChangesAsync();

        return new MetodoPagoDto
        {
            Id = metodo.Id,
            Nombre = metodo.Nombre,
            Codigo = metodo.Codigo,
            Activo = metodo.Activo,
            RequiereReferencia = metodo.RequiereReferencia
        };
    }

    public async Task<bool> DeleteMetodoPagoAsync(string id)
    {
        var metodo = await _context.MetodosPagos.FindAsync(id);

        if (metodo == null)
            return false;

        _context.MetodosPagos.Remove(metodo);
        await _context.SaveChangesAsync();

        return true;
    }

    // ?? Comandas Admin ?????????????????????????????????????????????????????????

    public async Task<List<ComandaAdminDto>> GetComandasAsync(
        string? estado = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        int limit = 100)
    {
        limit = Math.Clamp(limit, 1, 500);

        var query = _context.Ordenes
            .Include(o => o.OrdenItems)
            .AsQueryable();

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(o => o.Estado == estado);

        if (desde.HasValue)
            query = query.Where(o => o.CreadoEn >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(o => o.CreadoEn <= hasta.Value);

        var ordenes = await query
            .OrderByDescending(o => o.CreadoEn)
            .Take(limit)
            .ToListAsync();

        return ordenes.Select(MapComandaToDto).ToList();
    }

    public async Task<ComandaAdminDto?> GetComandaByIdAsync(string id)
    {
        var orden = await _context.Ordenes
            .Include(o => o.OrdenItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null)
            return null;

        return MapComandaToDto(orden);
    }

    public async Task<ComandaAdminDto?> EditarComandaAsync(string id, EditarComandaDto dto)
    {
        var orden = await _context.Ordenes
            .Include(o => o.OrdenItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null)
            return null;

        var config = await _context.ConfigImpuestos.FirstOrDefaultAsync();
        var ivaTasa = config?.IvaTasa ?? 0.16m;

        if (dto.Descuento.HasValue)
            orden.Descuento = dto.Descuento.Value;

        if (dto.Propina.HasValue)
            orden.Propina = dto.Propina.Value;

        if (dto.Notas != null)
            orden.Notas = dto.Notas;

        var subtotalConDescuento = Math.Max(0, orden.Subtotal - orden.Descuento);
        orden.Impuestos = subtotalConDescuento * ivaTasa;
        orden.Total = subtotalConDescuento + orden.Impuestos + orden.Propina;
        orden.ActualizadoEn = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapComandaToDto(orden);
    }

    public async Task<bool> AnularComandaAsync(string id)
    {
        var orden = await _context.Ordenes
            .Include(o => o.Mesa)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null)
            return false;

        orden.Estado = "cancelado";
        orden.ActualizadoEn = DateTime.UtcNow;

        if (orden.Mesa != null)
            orden.Mesa.Activa = true;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> EliminarComandaAsync(string id)
    {
        var orden = await _context.Ordenes
            .Include(o => o.Mesa)
            .Include(o => o.OrdenItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null)
            return false;

        if (orden.Mesa != null)
            orden.Mesa.Activa = true;

        _context.Ordenes.Remove(orden);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<TicketReimpresionDto?> GetTicketReimpresionAsync(string id)
    {
        var orden = await _context.Ordenes
            .Include(o => o.OrdenItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (orden == null)
            return null;

        var negocio = await _context.ConfigNegocios.FirstOrDefaultAsync();

        return new TicketReimpresionDto
        {
            OrdenId = orden.Id,
            NegocioNombre = negocio?.Nombre ?? "Restaurante",
            NegocioDireccion = negocio?.Direccion,
            NegocioTelefono = negocio?.Telefono,
            TicketHeader = negocio?.TicketEncabezado,
            TicketFooter = negocio?.TicketPie,
            NumeroMesa = orden.NumeroMesa,
            MeseroNombre = orden.MeseroNombre,
            FechaHora = orden.CreadoEn,
            Items = orden.OrdenItems.Select(i => new ComandaItemAdminDto
            {
                Id = i.Id,
                Nombre = i.Nombre,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                Notas = i.Notas,
                Estado = i.Estado
            }).ToList(),
            Subtotal = orden.Subtotal,
            Impuestos = orden.Impuestos,
            Descuento = orden.Descuento,
            Propina = orden.Propina,
            Total = orden.Total
        };
    }

    public async Task<UsuarioVerificadoDto?> VerificarPinAsync(string pin)
    {
        var pinHash = _hashService.HashPin(pin);

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u =>
                u.PinHash == pinHash &&
                u.Activo &&
                (u.RolId == "admin" || u.RolId == "supervisor"));

        if (usuario == null)
            return null;

        return new UsuarioVerificadoDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Rol = usuario.RolId
        };
    }

    private static ComandaAdminDto MapComandaToDto(AccesoDatos.Models.Ordene orden)
    {
        return new ComandaAdminDto
        {
            Id = orden.Id,
            MesaId = orden.MesaId,
            NumeroMesa = orden.NumeroMesa,
            TipoServicio = orden.TipoServicio,
            Estado = orden.Estado,
            Comensales = orden.Comensales,
            UsuarioNombre = orden.UsuarioNombre,
            MeseroNombre = orden.MeseroNombre,
            Subtotal = orden.Subtotal,
            Impuestos = orden.Impuestos,
            Descuento = orden.Descuento,
            Propina = orden.Propina,
            Total = orden.Total,
            Notas = orden.Notas,
            CreadoEn = orden.CreadoEn,
            ActualizadoEn = orden.ActualizadoEn,
            Items = orden.OrdenItems.Select(i => new ComandaItemAdminDto
            {
                Id = i.Id,
                Nombre = i.Nombre,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
                Notas = i.Notas,
                Estado = i.Estado
            }).ToList()
        };
    }
}
