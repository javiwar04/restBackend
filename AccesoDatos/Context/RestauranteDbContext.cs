using System;
using System.Collections.Generic;
using AccesoDatos.Models;
using Microsoft.EntityFrameworkCore;

namespace AccesoDatos.Context;

public partial class RestauranteDbContext : DbContext
{
    public RestauranteDbContext()
    {
    }

    public RestauranteDbContext(DbContextOptions<RestauranteDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auditorium> Auditoria { get; set; }

    public virtual DbSet<CategoriasInventario> CategoriasInventarios { get; set; }

    public virtual DbSet<CategoriasMenu> CategoriasMenus { get; set; }

    public virtual DbSet<CocinaAlerta> CocinaAlertas { get; set; }

    public virtual DbSet<ConfigImpuesto> ConfigImpuestos { get; set; }

    public virtual DbSet<ConfigNegocio> ConfigNegocios { get; set; }

    public virtual DbSet<CorteCaja> CorteCajas { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<Insumo> Insumos { get; set; }

    public virtual DbSet<InsumosMovimiento> InsumosMovimientos { get; set; }

    public virtual DbSet<Mesa> Mesas { get; set; }

    public virtual DbSet<MetodosPago> MetodosPagos { get; set; }

    public virtual DbSet<ModificadoresGrupo> ModificadoresGrupos { get; set; }

    public virtual DbSet<ModificadoresOpcion> ModificadoresOpcions { get; set; }

    public virtual DbSet<Modulo> Modulos { get; set; }

    public virtual DbSet<OrdenItem> OrdenItems { get; set; }

    public virtual DbSet<OrdenItemModificadore> OrdenItemModificadores { get; set; }

    public virtual DbSet<Ordene> Ordenes { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<PagosDetalle> PagosDetalles { get; set; }

    public virtual DbSet<PagosDividido> PagosDivididos { get; set; }

    public virtual DbSet<Platillo> Platillos { get; set; }

    public virtual DbSet<PlatilloModificadoresOverride> PlatilloModificadoresOverrides { get; set; }

    public virtual DbSet<Receta> Recetas { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Seccione> Secciones { get; set; }

    public virtual DbSet<Turno> Turnos { get; set; }

    public virtual DbSet<MovimientoCaja> MovimientosCaja { get; set; }

    public virtual DbSet<Establecimiento> Establecimientos { get; set; }

    public virtual DbSet<UsuarioEstablecimiento> UsuariosEstablecimientos { get; set; }

    public virtual DbSet<PlatilloEstablecimiento> PlatillosEstablecimientos { get; set; }

    public virtual DbSet<CorteInventario> CorteInventarios { get; set; }

    public virtual DbSet<CorteInventarioDetalle> CorteInventarioDetalles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VwMesero> VwMeseros { get; set; }

    public virtual DbSet<VwTopPlatillo> VwTopPlatillos { get; set; }

    public virtual DbSet<VwVentasDiaria> VwVentasDiarias { get; set; }

    public virtual DbSet<VwVentasMesero> VwVentasMeseros { get; set; }

    public virtual DbSet<ModificadorGrupo> ModificadorGrupos { get; set; }

    public virtual DbSet<ModificadorOpcion> ModificadorOpciones { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Respaldo solo para herramientas de diseño (scaffold/migrations);
        // la app real configura la conexión desde appsettings en Program.cs
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS01;Database=restSF;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auditorium>(entity =>
        {
            entity.HasIndex(e => new { e.UsuarioId, e.RegistradoEn }, "IX_Auditoria_Usuario").IsDescending(false, true);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Accion)
                .HasMaxLength(100)
                .HasColumnName("accion");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .HasColumnName("descripcion");
            entity.Property(e => e.RegistradoEn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("registrado_en");
            entity.Property(e => e.Rol)
                .HasMaxLength(30)
                .HasColumnName("rol");
            entity.Property(e => e.UsuarioId)
                .HasMaxLength(36)
                .HasColumnName("usuario_id");
            entity.Property(e => e.UsuarioNombre)
                .HasMaxLength(100)
                .HasColumnName("usuario_nombre");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Auditoria_Usuario");
        });

        modelBuilder.Entity<CategoriasInventario>(entity =>
        {
            entity.ToTable("Categorias_Inventario");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<CategoriasMenu>(entity =>
        {
            entity.ToTable("Categorias_Menu");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
            entity.Property(e => e.Orden).HasColumnName("orden");

            entity.HasMany(d => d.Grupos).WithMany(p => p.Categoria)
                .UsingEntity<Dictionary<string, object>>(
                    "PlatilloModificadoresCategorium",
                    r => r.HasOne<ModificadoresGrupo>().WithMany()
                        .HasForeignKey("GrupoId")
                        .HasConstraintName("FK_PlatModCat_Grupo"),
                    l => l.HasOne<CategoriasMenu>().WithMany()
                        .HasForeignKey("CategoriaId")
                        .HasConstraintName("FK_PlatModCat_Categoria"),
                    j =>
                    {
                        j.HasKey("CategoriaId", "GrupoId").HasName("PK_PlatilloModCat");
                        j.ToTable("Platillo_Modificadores_Categoria");
                        j.IndexerProperty<string>("CategoriaId")
                            .HasMaxLength(36)
                            .HasColumnName("categoria_id");
                        j.IndexerProperty<string>("GrupoId")
                            .HasMaxLength(36)
                            .HasColumnName("grupo_id");
                    });
        });

        modelBuilder.Entity<CocinaAlerta>(entity =>
        {
            entity.ToTable("Cocina_Alertas");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.OrdenId)
                .HasMaxLength(36)
                .HasColumnName("orden_id");
            entity.Property(e => e.RegistradoEn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("registrado_en");
            entity.Property(e => e.Tipo)
                .HasMaxLength(30)
                .HasDefaultValue("nueva_orden")
                .HasColumnName("tipo");
            entity.Property(e => e.Vista).HasColumnName("vista");

            entity.HasOne(d => d.Orden).WithMany(p => p.CocinaAlerta)
                .HasForeignKey(d => d.OrdenId)
                .HasConstraintName("FK_CocinaAlertas_Orden");
        });

        modelBuilder.Entity<ConfigImpuesto>(entity =>
        {
            entity.ToTable("Config_Impuestos");

            entity.Property(e => e.Id)
                .HasDefaultValue(1)
                .HasColumnName("id");
            entity.Property(e => e.EstablecimientoId)
                .HasMaxLength(36)
                .HasColumnName("establecimiento_id");
            entity.Property(e => e.CargoServicioHabilitado).HasColumnName("cargo_servicio_habilitado");
            entity.Property(e => e.CargoServicioTasa)
                .HasColumnType("decimal(5, 4)")
                .HasColumnName("cargo_servicio_tasa");
            entity.Property(e => e.IvaHabilitado)
                .HasDefaultValue(true)
                .HasColumnName("iva_habilitado");
            entity.Property(e => e.IvaIncluido).HasColumnName("iva_incluido");
            entity.Property(e => e.IvaTasa)
                .HasDefaultValue(0.1600m)
                .HasColumnType("decimal(5, 4)")
                .HasColumnName("iva_tasa");
            entity.Property(e => e.PropinaAuto).HasColumnName("propina_auto");
            entity.Property(e => e.PropinaAutoMinComensales)
                .HasDefaultValue(6)
                .HasColumnName("propina_auto_min_comensales");
            entity.Property(e => e.PropinaAutoTasa)
                .HasDefaultValue(0.1000m)
                .HasColumnType("decimal(5, 4)")
                .HasColumnName("propina_auto_tasa");
            entity.Property(e => e.PropinaHabilitada)
                .HasDefaultValue(true)
                .HasColumnName("propina_habilitada");
            entity.Property(e => e.PropinaSugerida)
                .HasDefaultValue(0.1000m)
                .HasColumnType("decimal(5, 4)")
                .HasColumnName("propina_sugerida");
        });

        modelBuilder.Entity<ConfigNegocio>(entity =>
        {
            entity.ToTable("Config_Negocio");

            entity.Property(e => e.Id)
                .HasDefaultValue(1)
                .HasColumnName("id");
            entity.Property(e => e.Direccion)
                .HasMaxLength(300)
                .HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Moneda)
                .HasMaxLength(3)
                .HasDefaultValue("MXN")
                .IsFixedLength()
                .HasColumnName("moneda");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .HasColumnName("nombre");
            entity.Property(e => e.Rfc)
                .HasMaxLength(20)
                .HasColumnName("rfc");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .HasColumnName("telefono");
            entity.Property(e => e.TicketEncabezado)
                .HasMaxLength(500)
                .HasColumnName("ticket_encabezado");
            entity.Property(e => e.TicketPie)
                .HasMaxLength(500)
                .HasColumnName("ticket_pie");
            entity.Property(e => e.ZonaHoraria)
                .HasMaxLength(60)
                .HasDefaultValue("America/Mexico_City")
                .HasColumnName("zona_horaria");
        });

        modelBuilder.Entity<CorteCaja>(entity =>
        {
            entity.ToTable("Corte_Caja");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Diferencia)
                .HasComputedColumnSql("(CONVERT([decimal](12,2),isnull([efectivo_final_real],(0))-[efectivo_final_sistema]))", true)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("diferencia");
            entity.Property(e => e.EfectivoFinalReal)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("efectivo_final_real");
            entity.Property(e => e.EfectivoFinalSistema)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("efectivo_final_sistema");
            entity.Property(e => e.EfectivoInicial)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("efectivo_inicial");
            entity.Property(e => e.FechaFin)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.Notas)
                .HasMaxLength(500)
                .HasColumnName("notas");
            entity.Property(e => e.RegistradoEn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("registrado_en");
            entity.Property(e => e.TotalDescuentos)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total_descuentos");
            entity.Property(e => e.TotalEfectivo)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total_efectivo");
            entity.Property(e => e.TotalImpuestos)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total_impuestos");
            entity.Property(e => e.TotalOrdenes).HasColumnName("total_ordenes");
            entity.Property(e => e.TotalPropinas)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total_propinas");
            entity.Property(e => e.TotalTarjeta)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total_tarjeta");
            entity.Property(e => e.TotalTransferencia)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total_transferencia");
            entity.Property(e => e.TotalVentas)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total_ventas");
            entity.Property(e => e.TurnoId)
                .HasMaxLength(36)
                .HasColumnName("turno_id");
            entity.Property(e => e.UsuarioId)
                .HasMaxLength(36)
                .HasColumnName("usuario_id");
            entity.Property(e => e.UsuarioNombre)
                .HasMaxLength(100)
                .HasColumnName("usuario_nombre");

            entity.HasOne(d => d.Turno).WithMany(p => p.CorteCajas)
                .HasForeignKey(d => d.TurnoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Corte_Turno");

            entity.HasOne(d => d.Usuario).WithMany(p => p.CorteCajas)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Corte_Usuario");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CfdiUuid)
                .HasMaxLength(40)
                .HasColumnName("cfdi_uuid");
            entity.Property(e => e.CfdiXml).HasColumnName("cfdi_xml");
            entity.Property(e => e.ClienteEmail)
                .HasMaxLength(150)
                .HasColumnName("cliente_email");
            entity.Property(e => e.ClienteNombre)
                .HasMaxLength(150)
                .HasColumnName("cliente_nombre");
            entity.Property(e => e.ClienteRfc)
                .HasMaxLength(20)
                .HasColumnName("cliente_rfc");
            entity.Property(e => e.EmitidaEn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("emitida_en");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("emitida")
                .HasColumnName("estado");
            entity.Property(e => e.Folio)
                .HasMaxLength(30)
                .HasColumnName("folio");
            entity.Property(e => e.Impuestos)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("impuestos");
            entity.Property(e => e.OrdenId)
                .HasMaxLength(36)
                .HasColumnName("orden_id");
            entity.Property(e => e.PagoId)
                .HasMaxLength(36)
                .HasColumnName("pago_id");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.Orden).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.OrdenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Facturas_Orden");

            entity.HasOne(d => d.Pago).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.PagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Facturas_Pago");
        });

        modelBuilder.Entity<Insumo>(entity =>
        {
            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CategoriaId)
                .HasMaxLength(36)
                .HasColumnName("categoria_id");
            entity.Property(e => e.EstablecimientoId)
                .HasMaxLength(36)
                .HasColumnName("establecimiento_id");
            entity.Property(e => e.CostoPorUnidad)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("costo_por_unidad");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("creado_en");
            entity.Property(e => e.Nombre)
                .HasMaxLength(120)
                .HasColumnName("nombre");
            entity.Property(e => e.Notas)
                .HasMaxLength(300)
                .HasColumnName("notas");
            entity.Property(e => e.Proveedor)
                .HasMaxLength(150)
                .HasColumnName("proveedor");
            entity.Property(e => e.StockActual)
                .HasColumnType("decimal(12, 4)")
                .HasColumnName("stock_actual");
            entity.Property(e => e.StockMinimo)
                .HasColumnType("decimal(12, 4)")
                .HasColumnName("stock_minimo");
            entity.Property(e => e.Unidad)
                .HasMaxLength(20)
                .HasDefaultValue("pza")
                .HasColumnName("unidad");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Insumos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Insumos_Categoria");
        });

        modelBuilder.Entity<InsumosMovimiento>(entity =>
        {
            entity.ToTable("Insumos_Movimientos");

            entity.HasIndex(e => new { e.InsumoId, e.RegistradoEn }, "IX_Insumos_Movimientos").IsDescending(false, true);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cantidad)
                .HasColumnType("decimal(12, 4)")
                .HasColumnName("cantidad");
            entity.Property(e => e.CostoPorUnidad)
                .HasColumnType("decimal(10, 4)")
                .HasColumnName("costo_por_unidad");
            entity.Property(e => e.InsumoId)
                .HasMaxLength(36)
                .HasColumnName("insumo_id");
            entity.Property(e => e.Motivo)
                .HasMaxLength(300)
                .HasColumnName("motivo");
            entity.Property(e => e.OrdenId)
                .HasMaxLength(36)
                .HasColumnName("orden_id");
            entity.Property(e => e.RegistradoEn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("registrado_en");
            entity.Property(e => e.Tipo)
                .HasMaxLength(10)
                .HasColumnName("tipo");
            entity.Property(e => e.UsuarioId)
                .HasMaxLength(36)
                .HasColumnName("usuario_id");

            entity.HasOne(d => d.Insumo).WithMany(p => p.InsumosMovimientos)
                .HasForeignKey(d => d.InsumoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InsMov_Insumo");

            entity.HasOne(d => d.Usuario).WithMany(p => p.InsumosMovimientos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_InsMov_Usuario");
        });

        modelBuilder.Entity<Mesa>(entity =>
        {
            entity.HasIndex(e => e.Numero, "UQ_Mesas_numero").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.Capacidad)
                .HasDefaultValue((byte)4)
                .HasColumnName("capacidad");
            entity.Property(e => e.Etiqueta)
                .HasMaxLength(10)
                .HasColumnName("etiqueta");
            entity.Property(e => e.Notas)
                .HasMaxLength(300)
                .HasColumnName("notas");
            entity.Property(e => e.Numero).HasColumnName("numero");
            entity.Property(e => e.SeccionId)
                .HasMaxLength(36)
                .HasColumnName("seccion_id");

            entity.HasOne(d => d.Seccion).WithMany(p => p.Mesas)
                .HasForeignKey(d => d.SeccionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mesas_Seccion");
        });

        modelBuilder.Entity<MetodosPago>(entity =>
        {
            entity.ToTable("Metodos_Pago");

            entity.HasIndex(e => e.Codigo, "UQ_MetodoPago_codigo").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
            entity.Property(e => e.RequiereReferencia).HasColumnName("requiere_referencia");
        });

        modelBuilder.Entity<ModificadoresGrupo>(entity =>
        {
            entity.ToTable("Modificadores_Grupo");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
            entity.Property(e => e.Tipo)
                .HasMaxLength(10)
                .HasDefaultValue("single")
                .HasColumnName("tipo");
        });

        modelBuilder.Entity<ModificadoresOpcion>(entity =>
        {
            entity.ToTable("Modificadores_Opcion");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.EsDefault).HasColumnName("es_default");
            entity.Property(e => e.GrupoId)
                .HasMaxLength(36)
                .HasColumnName("grupo_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
            entity.Property(e => e.PrecioDelta)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("precio_delta");

            entity.HasOne(d => d.Grupo).WithMany(p => p.ModificadoresOpcions)
                .HasForeignKey(d => d.GrupoId)
                .HasConstraintName("FK_ModOpcion_Grupo");
        });

        modelBuilder.Entity<Modulo>(entity =>
        {
            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(60)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<OrdenItem>(entity =>
        {
            entity.ToTable("Orden_Items");

            entity.HasIndex(e => e.OrdenId, "IX_Orden_Items_Orden");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cantidad)
                .HasDefaultValue((short)1)
                .HasColumnName("cantidad");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(120)
                .HasColumnName("nombre");
            entity.Property(e => e.Notas)
                .HasMaxLength(300)
                .HasColumnName("notas");
            entity.Property(e => e.OrdenId)
                .HasMaxLength(36)
                .HasColumnName("orden_id");
            entity.Property(e => e.PlatilloId)
                .HasMaxLength(36)
                .HasColumnName("platillo_id");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_unitario");

            entity.HasOne(d => d.Orden).WithMany(p => p.OrdenItems)
                .HasForeignKey(d => d.OrdenId)
                .HasConstraintName("FK_OrdenItems_Orden");

            entity.HasOne(d => d.Platillo).WithMany(p => p.OrdenItems)
                .HasForeignKey(d => d.PlatilloId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_OrdenItems_Platillo");
        });

        modelBuilder.Entity<OrdenItemModificadore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_OrdenItemMod");

            entity.ToTable("Orden_Item_Modificadores");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GrupoNombre)
                .HasMaxLength(80)
                .HasColumnName("grupo_nombre");
            entity.Property(e => e.OpcionNombre)
                .HasMaxLength(80)
                .HasColumnName("opcion_nombre");
            entity.Property(e => e.OpcionId)
                .HasMaxLength(36)
                .HasColumnName("opcion_id");
            entity.Property(e => e.OrdenItemId).HasColumnName("orden_item_id");
            entity.Property(e => e.PrecioDelta)
                .HasColumnType("decimal(8, 2)")
                .HasColumnName("precio_delta");

            entity.HasOne(d => d.OrdenItem).WithMany(p => p.OrdenItemModificadores)
                .HasForeignKey(d => d.OrdenItemId)
                .HasConstraintName("FK_OrdenItemMod_Item");
        });

        modelBuilder.Entity<Ordene>(entity =>
        {
            entity.HasIndex(e => new { e.Estado, e.CreadoEn }, "IX_Ordenes_Estado").IsDescending(false, true);

            entity.HasIndex(e => new { e.MesaId, e.Estado }, "IX_Ordenes_Mesa");

            entity.HasIndex(e => e.TurnoId, "IX_Ordenes_Turno");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.EstablecimientoId)
                .HasMaxLength(36)
                .HasColumnName("establecimiento_id");
            entity.Property(e => e.ActualizadoEn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("actualizado_en");
            entity.Property(e => e.Comensales)
                .HasDefaultValue((byte)1)
                .HasColumnName("comensales");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("creado_en");
            entity.Property(e => e.Descuento)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("descuento");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValue("pendiente")
                .HasColumnName("estado");
            entity.Property(e => e.Impuestos)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("impuestos");
            entity.Property(e => e.MesaId)
                .HasMaxLength(36)
                .HasColumnName("mesa_id");
            entity.Property(e => e.MeseroId)
                .HasMaxLength(36)
                .HasColumnName("mesero_id");
            entity.Property(e => e.MeseroNombre)
                .HasMaxLength(100)
                .HasColumnName("mesero_nombre");
            entity.Property(e => e.Notas)
                .HasMaxLength(500)
                .HasColumnName("notas");
            entity.Property(e => e.NumeroMesa).HasColumnName("numero_mesa");
            entity.Property(e => e.Propina)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("propina");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.TipoServicio)
                .HasMaxLength(20)
                .HasDefaultValue("mesa")
                .HasColumnName("tipo_servicio");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total");
            entity.Property(e => e.TurnoId)
                .HasMaxLength(36)
                .HasColumnName("turno_id");
            entity.Property(e => e.UsuarioId)
                .HasMaxLength(36)
                .HasColumnName("usuario_id");
            entity.Property(e => e.UsuarioNombre)
                .HasMaxLength(100)
                .HasColumnName("usuario_nombre");

            entity.HasOne(d => d.Mesa).WithMany(p => p.Ordenes)
                .HasForeignKey(d => d.MesaId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Ordenes_Mesa");

            entity.HasOne(d => d.Mesero).WithMany(p => p.OrdeneMeseros)
                .HasForeignKey(d => d.MeseroId)
                .HasConstraintName("FK_Ordenes_Mesero");

            entity.HasOne(d => d.Turno).WithMany(p => p.Ordenes)
                .HasForeignKey(d => d.TurnoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Ordenes_Turno");

            entity.HasOne(d => d.Usuario).WithMany(p => p.OrdeneUsuarios)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Ordenes_Usuario");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasIndex(e => e.OrdenId, "IX_Pagos_Orden");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Facturado).HasColumnName("facturado");
            entity.Property(e => e.MeseroId)
                .HasMaxLength(36)
                .HasColumnName("mesero_id");
            entity.Property(e => e.MeseroNombre)
                .HasMaxLength(100)
                .HasColumnName("mesero_nombre");
            entity.Property(e => e.MontoTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto_total");
            entity.Property(e => e.OrdenId)
                .HasMaxLength(36)
                .HasColumnName("orden_id");
            entity.Property(e => e.RegistradoEn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("registrado_en");
            entity.Property(e => e.TurnoId)
                .HasMaxLength(36)
                .HasColumnName("turno_id");
            entity.Property(e => e.UsuarioId)
                .HasMaxLength(36)
                .HasColumnName("usuario_id");
            entity.Property(e => e.UsuarioNombre)
                .HasMaxLength(100)
                .HasColumnName("usuario_nombre");

            entity.HasOne(d => d.Orden).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.OrdenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pagos_Orden");

            entity.HasOne(d => d.Turno).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.TurnoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Pagos_Turno");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Pagos_Usuario");
        });

        modelBuilder.Entity<PagosDetalle>(entity =>
        {
            entity.ToTable("Pagos_Detalles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MetodoCodigo)
                .HasMaxLength(20)
                .HasColumnName("metodo_codigo");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.PagoId)
                .HasMaxLength(36)
                .HasColumnName("pago_id");
            entity.Property(e => e.ReferenciaLote)
                .HasMaxLength(60)
                .HasColumnName("referencia_lote");
            entity.Property(e => e.ReferenciaTransf)
                .HasMaxLength(100)
                .HasColumnName("referencia_transf");

            entity.HasOne(d => d.Pago).WithMany(p => p.PagosDetalles)
                .HasForeignKey(d => d.PagoId)
                .HasConstraintName("FK_PagosDetalle_Pago");
        });

        modelBuilder.Entity<PagosDividido>(entity =>
        {
            entity.ToTable("Pagos_Divididos");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cobrado).HasColumnName("cobrado");
            entity.Property(e => e.CobradoEn).HasColumnName("cobrado_en");
            entity.Property(e => e.MetodoCodigo)
                .HasMaxLength(20)
                .HasColumnName("metodo_codigo");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.PagoId)
                .HasMaxLength(36)
                .HasColumnName("pago_id");
            entity.Property(e => e.PersonaNum).HasColumnName("persona_num");

            entity.HasOne(d => d.Pago).WithMany(p => p.PagosDivididos)
                .HasForeignKey(d => d.PagoId)
                .HasConstraintName("FK_PagosDivididos_Pago");
        });

        modelBuilder.Entity<Platillo>(entity =>
        {
            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CategoriaId)
                .HasMaxLength(36)
                .HasColumnName("categoria_id");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("creado_en");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(400)
                .HasColumnName("descripcion");
            entity.Property(e => e.Disponible)
                .HasDefaultValue(true)
                .HasColumnName("disponible");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(300)
                .HasColumnName("imagen_url");
            entity.Property(e => e.Nombre)
                .HasMaxLength(120)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Platillos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Platillos_Categoria");
        });

        modelBuilder.Entity<PlatilloModificadoresOverride>(entity =>
        {
            entity.HasKey(e => new { e.PlatilloId, e.GrupoId }).HasName("PK_PlatilloModOverride");

            entity.ToTable("Platillo_Modificadores_Override");

            entity.Property(e => e.PlatilloId)
                .HasMaxLength(36)
                .HasColumnName("platillo_id");
            entity.Property(e => e.GrupoId)
                .HasMaxLength(36)
                .HasColumnName("grupo_id");
            entity.Property(e => e.Habilitado)
                .HasDefaultValue(true)
                .HasColumnName("habilitado");

            entity.HasOne(d => d.Grupo).WithMany(p => p.PlatilloModificadoresOverrides)
                .HasForeignKey(d => d.GrupoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlatModOvr_Grupo");

            entity.HasOne(d => d.Platillo).WithMany(p => p.PlatilloModificadoresOverrides)
                .HasForeignKey(d => d.PlatilloId)
                .HasConstraintName("FK_PlatModOvr_Platillo");
        });

        modelBuilder.Entity<Receta>(entity =>
        {
            entity.HasKey(e => new { e.PlatilloId, e.InsumoId });

            entity.Property(e => e.PlatilloId)
                .HasMaxLength(36)
                .HasColumnName("platillo_id");
            entity.Property(e => e.InsumoId)
                .HasMaxLength(36)
                .HasColumnName("insumo_id");
            entity.Property(e => e.Cantidad)
                .HasColumnType("decimal(12, 4)")
                .HasColumnName("cantidad");

            entity.HasOne(d => d.Insumo).WithMany(p => p.Receta)
                .HasForeignKey(d => d.InsumoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Recetas_Insumo");

            entity.HasOne(d => d.Platillo).WithMany(p => p.Receta)
                .HasForeignKey(d => d.PlatilloId)
                .HasConstraintName("FK_Recetas_Platillo");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(60)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Seccione>(entity =>
        {
            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activa)
                .HasDefaultValue(true)
                .HasColumnName("activa");
            entity.Property(e => e.EstablecimientoId)
                .HasMaxLength(36)
                .HasColumnName("establecimiento_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
            entity.Property(e => e.Orden).HasColumnName("orden");
        });

        modelBuilder.Entity<Turno>(entity =>
        {
            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Fin).HasColumnName("fin");
            entity.Property(e => e.EstablecimientoId)
                .HasMaxLength(36)
                .HasColumnName("establecimiento_id");
            entity.Property(e => e.EfectivoInicial)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("efectivo_inicial");
            entity.Property(e => e.Inicio)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("inicio");
            entity.Property(e => e.Notas)
                .HasMaxLength(300)
                .HasColumnName("notas");
            entity.Property(e => e.TotalOrdenes).HasColumnName("total_ordenes");
            entity.Property(e => e.TotalVentas)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total_ventas");
            entity.Property(e => e.UsuarioId)
                .HasMaxLength(36)
                .HasColumnName("usuario_id");
            entity.Property(e => e.UsuarioNombre)
                .HasMaxLength(100)
                .HasColumnName("usuario_nombre");
            entity.Property(e => e.VentasEfectivo)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("ventas_efectivo");
            entity.Property(e => e.VentasTarjeta)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("ventas_tarjeta");
            entity.Property(e => e.VentasTransfer)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("ventas_transfer");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Turnos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Turnos_Usuario");
        });

        modelBuilder.Entity<Establecimiento>(entity =>
        {
            entity.ToTable("Establecimientos");

            entity.Property(e => e.Id).HasMaxLength(36).HasColumnName("id");
            entity.Property(e => e.Nombre).HasMaxLength(100).HasColumnName("nombre");
            entity.Property(e => e.Direccion).HasMaxLength(200).HasColumnName("direccion");
            entity.Property(e => e.Telefono).HasMaxLength(30).HasColumnName("telefono");
            entity.Property(e => e.Activo).HasDefaultValue(true).HasColumnName("activo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("creado_en");
        });

        modelBuilder.Entity<UsuarioEstablecimiento>(entity =>
        {
            entity.ToTable("Usuarios_Establecimientos");
            entity.HasKey(e => new { e.UsuarioId, e.EstablecimientoId });
            entity.Property(e => e.UsuarioId).HasMaxLength(36).HasColumnName("usuario_id");
            entity.Property(e => e.EstablecimientoId).HasMaxLength(36).HasColumnName("establecimiento_id");
        });

        modelBuilder.Entity<PlatilloEstablecimiento>(entity =>
        {
            entity.ToTable("Platillos_Establecimientos");
            entity.HasKey(e => new { e.PlatilloId, e.EstablecimientoId });
            entity.Property(e => e.PlatilloId).HasMaxLength(36).HasColumnName("platillo_id");
            entity.Property(e => e.EstablecimientoId).HasMaxLength(36).HasColumnName("establecimiento_id");
        });

        modelBuilder.Entity<CorteInventario>(entity =>
        {
            entity.ToTable("Corte_Inventario");
            entity.Property(e => e.Id).HasMaxLength(36).HasColumnName("id");
            entity.Property(e => e.TurnoId).HasMaxLength(36).HasColumnName("turno_id");
            entity.Property(e => e.EstablecimientoId).HasMaxLength(36).HasColumnName("establecimiento_id");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.TotalMermaValor).HasColumnType("decimal(12, 2)").HasColumnName("total_merma_valor");
            entity.Property(e => e.Notas).HasMaxLength(300).HasColumnName("notas");
            entity.Property(e => e.RegistradoEn).HasDefaultValueSql("(sysutcdatetime())").HasColumnName("registrado_en");
        });

        modelBuilder.Entity<CorteInventarioDetalle>(entity =>
        {
            entity.ToTable("Corte_Inventario_Detalle");
            entity.HasIndex(e => e.CorteId, "IX_CorteInvDet_Corte");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CorteId).HasMaxLength(36).HasColumnName("corte_id");
            entity.Property(e => e.InsumoId).HasMaxLength(36).HasColumnName("insumo_id");
            entity.Property(e => e.Encontre).HasColumnType("decimal(12, 4)").HasColumnName("encontre");
            entity.Property(e => e.Ingreso).HasColumnType("decimal(12, 4)").HasColumnName("ingreso");
            entity.Property(e => e.Quedo).HasColumnType("decimal(12, 4)").HasColumnName("quedo");
            entity.Property(e => e.VendidoTeorico).HasColumnType("decimal(12, 4)").HasColumnName("vendido_teorico");
            entity.Property(e => e.ConsumidoFisico).HasColumnType("decimal(12, 4)").HasColumnName("consumido_fisico");
            entity.Property(e => e.Merma).HasColumnType("decimal(12, 4)").HasColumnName("merma");
            entity.Property(e => e.CostoUnitario).HasColumnType("decimal(10, 4)").HasColumnName("costo_unitario");

            entity.HasOne(d => d.Corte).WithMany(p => p.Detalles)
                .HasForeignKey(d => d.CorteId)
                .HasConstraintName("FK_CorteInvDet_Corte");
            entity.HasOne(d => d.Insumo).WithMany()
                .HasForeignKey(d => d.InsumoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CorteInvDet_Insumo");
        });

        modelBuilder.Entity<MovimientoCaja>(entity =>
        {
            entity.ToTable("Movimientos_Caja");

            entity.HasIndex(e => new { e.TurnoId, e.RegistradoEn }, "IX_MovCaja_Turno");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TurnoId)
                .HasMaxLength(36)
                .HasColumnName("turno_id");
            entity.Property(e => e.Tipo)
                .HasMaxLength(10)
                .HasColumnName("tipo");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.Motivo)
                .HasMaxLength(300)
                .HasColumnName("motivo");
            entity.Property(e => e.UsuarioId)
                .HasMaxLength(36)
                .HasColumnName("usuario_id");
            entity.Property(e => e.UsuarioNombre)
                .HasMaxLength(150)
                .HasColumnName("usuario_nombre");
            entity.Property(e => e.RegistradoEn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("registrado_en");

            entity.HasOne(d => d.Turno).WithMany(p => p.MovimientosCaja)
                .HasForeignKey(d => d.TurnoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MovCaja_Turno");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(e => e.Username, "UQ_Usuarios_username").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("creado_en");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Notas)
                .HasMaxLength(300)
                .HasColumnName("notas");
            entity.Property(e => e.PinHash)
                .HasMaxLength(64)
                .HasColumnName("pin_hash");
            entity.Property(e => e.RolId)
                .HasMaxLength(30)
                .HasColumnName("rol_id");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Rol");

            entity.HasMany(d => d.Modulos).WithMany(p => p.Usuarios)
                .UsingEntity<Dictionary<string, object>>(
                    "UsuariosModulo",
                    r => r.HasOne<Modulo>().WithMany()
                        .HasForeignKey("ModuloId")
                        .HasConstraintName("FK_UsuariosModulos_Modulo"),
                    l => l.HasOne<Usuario>().WithMany()
                        .HasForeignKey("UsuarioId")
                        .HasConstraintName("FK_UsuariosModulos_Usuario"),
                    j =>
                    {
                        j.HasKey("UsuarioId", "ModuloId");
                        j.ToTable("Usuarios_Modulos");
                        j.IndexerProperty<string>("UsuarioId")
                            .HasMaxLength(36)
                            .HasColumnName("usuario_id");
                        j.IndexerProperty<string>("ModuloId")
                            .HasMaxLength(30)
                            .HasColumnName("modulo_id");
                    });
        });

        modelBuilder.Entity<VwMesero>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_Meseros");

            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        modelBuilder.Entity<VwTopPlatillo>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_Top_Platillos");

            entity.Property(e => e.IngresoTotal)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("ingreso_total");
            entity.Property(e => e.Platillo)
                .HasMaxLength(120)
                .HasColumnName("platillo");
            entity.Property(e => e.PlatilloId)
                .HasMaxLength(36)
                .HasColumnName("platillo_id");
            entity.Property(e => e.UnidadesVendidas).HasColumnName("unidades_vendidas");
        });

        modelBuilder.Entity<VwVentasDiaria>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_Ventas_Diarias");

            entity.Property(e => e.Descuentos)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("descuentos");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Impuestos)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("impuestos");
            entity.Property(e => e.Propinas)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("propinas");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("total");
            entity.Property(e => e.TotalOrdenes).HasColumnName("total_ordenes");
        });

        modelBuilder.Entity<VwVentasMesero>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_Ventas_Mesero");

            entity.Property(e => e.MeseroId)
                .HasMaxLength(36)
                .HasColumnName("mesero_id");
            entity.Property(e => e.MeseroNombre)
                .HasMaxLength(100)
                .HasColumnName("mesero_nombre");
            entity.Property(e => e.Ordenes).HasColumnName("ordenes");
            entity.Property(e => e.Propinas)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("propinas");
            entity.Property(e => e.TotalVentas)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("total_ventas");
        });

        modelBuilder.Entity<ModificadorGrupo>(entity =>
        {
            entity.ToTable("ModificadorGrupos");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.PlatilloId)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .HasDefaultValue("single");

            entity.Property(e => e.Obligatorio)
                .HasDefaultValue(false);

            entity.Property(e => e.MinSelecciones)
                .HasDefaultValue(0);

            entity.Property(e => e.MaxSelecciones)
                .HasDefaultValue(0);

            entity.Property(e => e.Orden)
                .HasDefaultValue(0);

            entity.HasOne(d => d.Platillo)
                .WithMany(p => p.Modificadores)
                .HasForeignKey(d => d.PlatilloId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ModificadorGrupos_Platillo");
        });

        modelBuilder.Entity<ModificadorOpcion>(entity =>
        {
            entity.ToTable("ModificadorOpciones");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.GrupoId)
                .HasMaxLength(36)
                .IsRequired();

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.PrecioDelta)
                .HasColumnType("decimal(10, 2)")
                .HasDefaultValue(0);
            entity.Property(e => e.InsumoId)
                .HasMaxLength(36);
            entity.Property(e => e.CantidadInsumo)
                .HasColumnType("decimal(12, 4)");

            entity.Property(e => e.EsDefault)
                .HasDefaultValue(false);

            entity.Property(e => e.Activo)
                .HasDefaultValue(true);

            entity.Property(e => e.Orden)
                .HasDefaultValue(0);

            entity.HasOne(d => d.Grupo)
                .WithMany(g => g.Opciones)
                .HasForeignKey(d => d.GrupoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ModificadorOpciones_Grupo");

            entity.HasOne(d => d.Insumo)
                .WithMany()
                .HasForeignKey(d => d.InsumoId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ModificadorOpciones_Insumo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
