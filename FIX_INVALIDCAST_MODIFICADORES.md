# ?? Fix: InvalidCastException en Modificadores

**Fecha:** Febrero 2024  
**Status:** ? Corregido (v3 - Final)

---

## ?? Problema Real

```
System.InvalidCastException: Unable to cast object of type 'System.Guid' to type 'System.String'
at MenuService.SincronizarModificadoresAsync() line 369
```

---

## ?? Causa

**Mismatch de tipos entre SQL Server y C#:**

| Entidad C# | Tipo C# | Columna SQL (antes) | Tipo SQL (antes) |
|------------|---------|---------------------|------------------|
| `ModificadorGrupo.Id` | `string` | `Id` | ? `UNIQUEIDENTIFIER` |
| `ModificadorGrupo.PlatilloId` | `string` | `PlatilloId` | ? `UNIQUEIDENTIFIER` |
| `ModificadorOpcion.Id` | `string` | `Id` | ? `UNIQUEIDENTIFIER` |
| `ModificadorOpcion.GrupoId` | `string` | `GrupoId` | ? `UNIQUEIDENTIFIER` |

**Entity Framework intentaba leer `UNIQUEIDENTIFIER` (Guid) como `string` y explotaba.**

---

## ? Solución

### **Opción elegida: Cambiar SQL a NVARCHAR(36)**

**¿Por qué?**
- ? Todo el resto del sistema usa `string` (Platillos, Categorias, Usuarios, etc.)
- ? Mantiene consistencia con el patrón existente
- ? No requiere cambiar todas las entidades C#

### **Cambios aplicados:**

**1. Script SQL de corrección:**
```sql
-- Eliminar tablas existentes
DROP TABLE ModificadorOpciones;
DROP TABLE ModificadorGrupos;

-- Recrear con NVARCHAR(36)
CREATE TABLE ModificadorGrupos (
    Id NVARCHAR(36) PRIMARY KEY,              -- ? NVARCHAR en lugar de UNIQUEIDENTIFIER
    PlatilloId NVARCHAR(36) NOT NULL,         -- ? NVARCHAR en lugar de UNIQUEIDENTIFIER
    Nombre NVARCHAR(100) NOT NULL,
    Tipo NVARCHAR(20) NOT NULL DEFAULT 'single',
    Obligatorio BIT NOT NULL DEFAULT 0,
    MinSelecciones INT NOT NULL DEFAULT 0,
    MaxSelecciones INT NOT NULL DEFAULT 0,
    Orden INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_ModificadorGrupos_Platillo
        FOREIGN KEY (PlatilloId) REFERENCES Platillos(Id) ON DELETE CASCADE
);

CREATE TABLE ModificadorOpciones (
    Id NVARCHAR(36) PRIMARY KEY,              -- ? NVARCHAR en lugar de UNIQUEIDENTIFIER
    GrupoId NVARCHAR(36) NOT NULL,            -- ? NVARCHAR en lugar de UNIQUEIDENTIFIER
    Nombre NVARCHAR(100) NOT NULL,
    PrecioDelta DECIMAL(10,2) NOT NULL DEFAULT 0,
    EsDefault BIT NOT NULL DEFAULT 0,
    Activo BIT NOT NULL DEFAULT 1,
    Orden INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_ModificadorOpciones_Grupo
        FOREIGN KEY (GrupoId) REFERENCES ModificadorGrupos(Id) ON DELETE CASCADE
);
```

**2. Actualizada configuración de EF Core:**
```csharp
// Quitado .HasDefaultValueSql("(newid())") porque genera UNIQUEIDENTIFIER
entity.Property(e => e.Id)
    .HasMaxLength(36)
    .IsRequired();  // ? Sin DefaultValueSql
```

---

## ?? Para Aplicar

### **1. Ejecutar script SQL:**

```sql
-- En SQL Server Management Studio o Azure Data Studio
-- Ejecutar: scripts/fix_modificadores_tipos.sql
```

**?? ADVERTENCIA:** Esto **borra** las tablas existentes y las recrea. Si ya tienes datos de modificadores, se perderán.

### **2. Reiniciar backend:**

```sh
# Detener (Ctrl+C)
cd WebApi
dotnet run
```

### **3. Probar crear platillo con modificadores:**

```typescript
const platillo = {
  categoriaId: "cat-123",
  nombre: "Test Burger",
  precio: 100,
  disponible: true,
  modificadores: [
    {
      grupoNombre: "Tamaño",
      tipo: "single",
      obligatorio: true,
      minSelecciones: 1,
      maxSelecciones: 1,
      orden: 1,
      opciones: [
        { nombre: "Chica", precioDelta: 0, esDefault: true, activo: true, orden: 1 }
      ]
    }
  ]
};

const response = await fetch('http://localhost:5006/platillos', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(platillo)
});

console.log(response.status); // ? Debe ser 201 (no 500)
```

---

## ?? Tipos Finales (Después del Fix)

| Entidad C# | Tipo C# | Columna SQL | Tipo SQL |
|------------|---------|-------------|----------|
| `ModificadorGrupo.Id` | `string` | `Id` | ? `NVARCHAR(36)` |
| `ModificadorGrupo.PlatilloId` | `string` | `PlatilloId` | ? `NVARCHAR(36)` |
| `ModificadorOpcion.Id` | `string` | `Id` | ? `NVARCHAR(36)` |
| `ModificadorOpcion.GrupoId` | `string` | `GrupoId` | ? `NVARCHAR(36)` |
| `Platillo.Id` | `string` | `Id` | ? `NVARCHAR(36)` |

**Ahora todos coinciden** ?

---

## ? Validaciones

Después del fix, verificar:

- [ ] Script SQL ejecutado sin errores
- [ ] Tablas `ModificadorGrupos` y `ModificadorOpciones` recreadas
- [ ] Backend reiniciado
- [ ] POST /platillos con modificadores ? 201 (no 500)
- [ ] PUT /platillos con modificadores ? 200 (no 500)
- [ ] GET /platillos ? 200 (no 500)

---

## ?? Alternativa (No elegida)

**Opción B: Cambiar C# a Guid**

Si prefirieras usar `Guid` en lugar de `string`:

```csharp
public class ModificadorGrupo
{
    public Guid Id { get; set; }           // ? Pero requiere cambiar TODAS las entidades
    public Guid PlatilloId { get; set; }   // ? Y Platillo.Id también a Guid
    // ...
}
```

**Problemas:**
- ? Platillos usa `string Id` (NVARCHAR)
- ? Habría que cambiar **TODO** el sistema
- ? Inconsistente con el resto de la BD

**Por eso elegimos mantener `string` y cambiar solo el SQL** ?

---

## ?? Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `scripts/fix_modificadores_tipos.sql` | ? **NUEVO** - Script de corrección |
| `AccesoDatos/Context/RestauranteDbContext.cs` | ? Quitado `.HasDefaultValueSql("(newid())")` |
| `AccesoDatos/Models/ModificadorGrupo.cs` | ? Sin cambios (ya usa `string`) |
| `AccesoDatos/Models/ModificadorOpcion.cs` | ? Sin cambios (ya usa `string`) |

---

## ?? Notas Importantes

### **IDs generados en C#, no en SQL:**

```csharp
// ? En MenuService.cs ya se generan en C#:
var nuevoGrupo = new ModificadorGrupo
{
    Id = Guid.NewGuid().ToString(),  // ? Genera "abc-123-..."
    // ...
};
```

**No necesitamos `DEFAULT NEWID()` en SQL** porque C# genera los IDs.

---

## ?? Prueba Completa

```sql
-- 1. Verificar tipos de columnas
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('ModificadorGrupos', 'ModificadorOpciones')
ORDER BY TABLE_NAME, ORDINAL_POSITION;

-- ? Debe mostrar NVARCHAR(36) para Id, PlatilloId, GrupoId
```

```typescript
// 2. Crear platillo desde frontend
const response = await api.crearPlatillo({
  categoriaId: "...",
  nombre: "Test",
  precio: 100,
  modificadores: [
    {
      grupoNombre: "Test",
      tipo: "single",
      obligatorio: true,
      minSelecciones: 1,
      maxSelecciones: 1,
      orden: 1,
      opciones: [
        { nombre: "Op1", precioDelta: 0, esDefault: true, activo: true, orden: 1 }
      ]
    }
  ]
});

console.log(response); // ? Debe devolver 201 con el platillo creado
```

---

## ?? Si Sigue Fallando

1. **Verificar que el script SQL se ejecutó correctamente**
2. **Verificar tipos de columnas en SQL:**
   ```sql
   EXEC sp_columns @table_name = 'ModificadorGrupos';
   EXEC sp_columns @table_name = 'ModificadorOpciones';
   ```
3. **Verificar que backend se reinició** (no hot reload, reinicio completo)
4. **Revisar logs** del backend para ver el stack trace completo

---

**Compilación:** ? Exitosa  
**Script SQL:** ? Creado en `scripts/fix_modificadores_tipos.sql`  
**Requiere:** ?? **Ejecutar script SQL + Reiniciar backend**  
**Listo para usar:** ? Después de ejecutar el script

---

**Desarrollado con:** .NET 8, Entity Framework Core, SQL Server  
**Fecha:** Febrero 2024
