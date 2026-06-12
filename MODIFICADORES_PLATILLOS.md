# ??? Sistema de Modificadores de Platillos

**Fecha:** Febrero 2024  
**Status:** ? Implementado y funcionando

---

## ?? Resumen

Se implementó un sistema **completo de modificadores por platillo** que permite personalizar cada platillo individualmente con grupos de opciones (ej: tamaño, término de cocción, extras, etc.).

### Características:
- ? Modificadores **propios** de cada platillo
- ? Compatibilidad con sistema anterior (modificadores por categoría)
- ? Grupos con tipo `single` (radio) o `multiple` (checkbox)
- ? Validaciones: obligatorio, min/max selecciones
- ? Ordenamiento personalizable
- ? Deltas de precio por opción
- ? Opciones por defecto

---

## ??? Estructura de Base de Datos

### Tabla: ModificadorGrupos
```sql
CREATE TABLE ModificadorGrupos (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PlatilloId NVARCHAR(36) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Tipo NVARCHAR(20) NOT NULL DEFAULT 'single',
    Obligatorio BIT NOT NULL DEFAULT 0,
    MinSelecciones INT NOT NULL DEFAULT 0,
    MaxSelecciones INT NOT NULL DEFAULT 0,
    Orden INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_ModificadorGrupos_Platillo
        FOREIGN KEY (PlatilloId) REFERENCES Platillos(Id) ON DELETE CASCADE
);
```

### Tabla: ModificadorOpciones
```sql
CREATE TABLE ModificadorOpciones (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    GrupoId UNIQUEIDENTIFIER NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    PrecioDelta DECIMAL(10,2) NOT NULL DEFAULT 0,
    EsDefault BIT NOT NULL DEFAULT 0,
    Activo BIT NOT NULL DEFAULT 1,
    Orden INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_ModificadorOpciones_Grupo
        FOREIGN KEY (GrupoId) REFERENCES ModificadorGrupos(Id) ON DELETE CASCADE
);
```

---

## ?? Endpoints (sin cambios en rutas)

Los endpoints de platillos **no cambiaron**, solo se extendieron:

| Método | Endpoint | Cambio |
|--------|----------|--------|
| GET | `/platillos` | ? Ahora incluye modificadores propios |
| GET | `/platillos/{id}` | ? Ahora incluye modificadores propios |
| POST | `/platillos` | ? Acepta modificadores en el body |
| PUT | `/platillos/{id}` | ? Acepta modificadores en el body |

---

## ?? Uso desde el Frontend

### 1?? GET - Obtener platillo con modificadores

**Request:**
```
GET /platillos/abc-123
Authorization: Bearer {token}
```

**Response 200:**
```json
{
  "id": "abc-123",
  "categoriaId": "cat-1",
  "categoriaNombre": "Principales",
  "nombre": "Filete de res",
  "descripcion": "300g de carne selecta",
  "precio": 285.00,
  "disponible": true,
  "imagenUrl": null,
  "modificadores": [
    {
      "grupoId": "grupo-1",
      "grupoNombre": "Término de cocción",
      "tipo": "single",
      "obligatorio": true,
      "minSelecciones": 1,
      "maxSelecciones": 1,
      "orden": 1,
      "opciones": [
        {
          "id": "op-1",
          "nombre": "Término medio",
          "precioDelta": 0.00,
          "esDefault": true,
          "activo": true,
          "orden": 1
        },
        {
          "id": "op-2",
          "nombre": "Bien cocido",
          "precioDelta": 0.00,
          "esDefault": false,
          "activo": true,
          "orden": 2
        }
      ]
    },
    {
      "grupoId": "grupo-2",
      "grupoNombre": "Extras",
      "tipo": "multiple",
      "obligatorio": false,
      "minSelecciones": 0,
      "maxSelecciones": 3,
      "orden": 2,
      "opciones": [
        {
          "id": "op-3",
          "nombre": "Champiñones",
          "precioDelta": 25.00,
          "esDefault": false,
          "activo": true,
          "orden": 1
        },
        {
          "id": "op-4",
          "nombre": "Queso azul",
          "precioDelta": 35.00,
          "esDefault": false,
          "activo": true,
          "orden": 2
        }
      ]
    }
  ]
}
```

---

### 2?? POST - Crear platillo con modificadores

**Request:**
```json
{
  "categoriaId": "cat-1",
  "nombre": "Hamburguesa Premium",
  "descripcion": "180g de carne angus",
  "precio": 120.00,
  "disponible": true,
  "imagenUrl": null,
  "modificadores": [
    {
      "grupoNombre": "Tamaño",
      "tipo": "single",
      "obligatorio": true,
      "minSelecciones": 1,
      "maxSelecciones": 1,
      "orden": 1,
      "opciones": [
        {
          "nombre": "Sencilla",
          "precioDelta": 0,
          "esDefault": true,
          "activo": true,
          "orden": 1
        },
        {
          "nombre": "Doble carne",
          "precioDelta": 40,
          "esDefault": false,
          "activo": true,
          "orden": 2
        }
      ]
    },
    {
      "grupoNombre": "Extras",
      "tipo": "multiple",
      "obligatorio": false,
      "minSelecciones": 0,
      "maxSelecciones": 5,
      "orden": 2,
      "opciones": [
        {
          "nombre": "Tocino",
          "precioDelta": 15,
          "esDefault": false,
          "activo": true,
          "orden": 1
        },
        {
          "nombre": "Queso extra",
          "precioDelta": 10,
          "esDefault": false,
          "activo": true,
          "orden": 2
        }
      ]
    }
  ]
}
```

**Response 201:** Mismo formato que GET

---

### 3?? PUT - Actualizar platillo y modificadores

**Request:**
```json
{
  "categoriaId": "cat-1",
  "nombre": "Hamburguesa Premium Plus",
  "descripcion": "200g de carne angus",
  "precio": 140.00,
  "disponible": true,
  "imagenUrl": "https://...",
  "modificadores": [
    {
      "grupoNombre": "Tamaño",
      "tipo": "single",
      "obligatorio": true,
      "minSelecciones": 1,
      "maxSelecciones": 1,
      "orden": 1,
      "opciones": [
        {
          "nombre": "Sencilla",
          "precioDelta": 0,
          "esDefault": true,
          "activo": true,
          "orden": 1
        },
        {
          "nombre": "Triple carne",
          "precioDelta": 60,
          "esDefault": false,
          "activo": true,
          "orden": 2
        }
      ]
    }
  ]
}
```

**Nota:** Los modificadores se **reemplazan completamente** (borra los antiguos y crea los nuevos).

---

## ?? Código TypeScript para Frontend

```typescript
// Tipos
interface ModificadorOpcion {
  id: string;
  nombre: string;
  precioDelta: number;
  esDefault: boolean;
  activo: boolean;
  orden: number;
}

interface ModificadorGrupo {
  grupoId: string;
  grupoNombre: string;
  tipo: 'single' | 'multiple';
  obligatorio: boolean;
  minSelecciones: number;
  maxSelecciones: number;
  orden: number;
  opciones: ModificadorOpcion[];
}

interface Platillo {
  id: string;
  categoriaId: string;
  categoriaNombre: string;
  nombre: string;
  descripcion: string | null;
  precio: number;
  disponible: boolean;
  imagenUrl: string | null;
  modificadores: ModificadorGrupo[];
}

// Crear platillo con modificadores
const crearPlatillo = async () => {
  const platillo = {
    categoriaId: "cat-123",
    nombre: "Hamburguesa Clásica",
    descripcion: "Con queso y tocino",
    precio: 85,
    disponible: true,
    imagenUrl: null,
    modificadores: [
      {
        grupoNombre: "Término",
        tipo: "single",
        obligatorio: true,
        minSelecciones: 1,
        maxSelecciones: 1,
        orden: 1,
        opciones: [
          { nombre: "Término medio", precioDelta: 0, esDefault: true, activo: true, orden: 1 },
          { nombre: "Bien cocido", precioDelta: 0, esDefault: false, activo: true, orden: 2 }
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

  const nuevoPlatillo: Platillo = await response.json();
  console.log('Platillo creado:', nuevoPlatillo);
};

// Renderizar modificadores en UI
const renderizarModificadores = (modificadores: ModificadorGrupo[]) => {
  return modificadores.map(grupo => (
    <div key={grupo.grupoId} className="modificador-grupo">
      <h3>
        {grupo.grupoNombre}
        {grupo.obligatorio && <span className="required">*</span>}
      </h3>
      
      {grupo.tipo === 'single' ? (
        // Radio buttons
        <div className="opciones">
          {grupo.opciones
            .filter(op => op.activo)
            .sort((a, b) => a.orden - b.orden)
            .map(opcion => (
              <label key={opcion.id}>
                <input
                  type="radio"
                  name={grupo.grupoId}
                  value={opcion.id}
                  defaultChecked={opcion.esDefault}
                />
                {opcion.nombre}
                {opcion.precioDelta > 0 && ` (+$${opcion.precioDelta})`}
              </label>
            ))}
        </div>
      ) : (
        // Checkboxes
        <div className="opciones">
          {grupo.opciones
            .filter(op => op.activo)
            .sort((a, b) => a.orden - b.orden)
            .map(opcion => (
              <label key={opcion.id}>
                <input
                  type="checkbox"
                  name={`${grupo.grupoId}[]`}
                  value={opcion.id}
                  defaultChecked={opcion.esDefault}
                />
                {opcion.nombre}
                {opcion.precioDelta > 0 && ` (+$${opcion.precioDelta})`}
              </label>
            ))}
        </div>
      )}
      
      {grupo.tipo === 'multiple' && (
        <small>
          Selecciona {grupo.minSelecciones > 0 && `mínimo ${grupo.minSelecciones}`}
          {grupo.minSelecciones > 0 && grupo.maxSelecciones > 0 && ' y '}
          {grupo.maxSelecciones > 0 && `máximo ${grupo.maxSelecciones}`}
        </small>
      )}
    </div>
  ));
};
```

---

## ?? Casos de Uso

### Caso 1: Hamburguesas con tamaño
```json
{
  "grupoNombre": "Tamaño",
  "tipo": "single",
  "obligatorio": true,
  "minSelecciones": 1,
  "maxSelecciones": 1,
  "orden": 1,
  "opciones": [
    { "nombre": "Sencilla", "precioDelta": 0, "esDefault": true },
    { "nombre": "Doble", "precioDelta": 30, "esDefault": false },
    { "nombre": "Triple", "precioDelta": 50, "esDefault": false }
  ]
}
```

### Caso 2: Filete con término
```json
{
  "grupoNombre": "Término de cocción",
  "tipo": "single",
  "obligatorio": true,
  "minSelecciones": 1,
  "maxSelecciones": 1,
  "orden": 1,
  "opciones": [
    { "nombre": "Término medio", "precioDelta": 0, "esDefault": true },
    { "nombre": "Tres cuartos", "precioDelta": 0, "esDefault": false },
    { "nombre": "Bien cocido", "precioDelta": 0, "esDefault": false }
  ]
}
```

### Caso 3: Extras opcionales
```json
{
  "grupoNombre": "Extras",
  "tipo": "multiple",
  "obligatorio": false,
  "minSelecciones": 0,
  "maxSelecciones": 5,
  "orden": 2,
  "opciones": [
    { "nombre": "Queso extra", "precioDelta": 10, "esDefault": false },
    { "nombre": "Tocino", "precioDelta": 15, "esDefault": false },
    { "nombre": "Aguacate", "precioDelta": 20, "esDefault": false }
  ]
}
```

---

## ? Validaciones Implementadas

### Backend:
- ? Platillo debe existir para agregar modificadores
- ? Al actualizar platillo, reemplaza modificadores completamente
- ? Cascade delete: si borras platillo, borra sus modificadores
- ? IDs generados automáticamente (GUID)

### Frontend (recomendadas):
- ?? Validar que grupos obligatorios tengan selección
- ?? Validar min/max selecciones en grupos `multiple`
- ?? Calcular precio total con deltas antes de enviar orden

---

## ?? Compatibilidad

### Sistema Anterior (Modificadores por Categoría):
? **SE MANTIENE** funcionando. Ahora un platillo puede tener:
1. **Modificadores propios** (los nuevos, de ModificadorGrupos)
2. **Modificadores de categoría** (los anteriores, de Modificadores_Grupo)

Ambos se devuelven en el mismo array `modificadores` del response.

---

## ?? Pruebas Recomendadas

### 1. Crear platillo con modificadores
```bash
curl -X POST http://localhost:5006/platillos \
  -H "Authorization: Bearer {TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "categoriaId": "cat-123",
    "nombre": "Test Burger",
    "precio": 100,
    "disponible": true,
    "modificadores": [
      {
        "grupoNombre": "Tamaño",
        "tipo": "single",
        "obligatorio": true,
        "minSelecciones": 1,
        "maxSelecciones": 1,
        "orden": 1,
        "opciones": [
          {"nombre": "Chica", "precioDelta": 0, "esDefault": true, "activo": true, "orden": 1},
          {"nombre": "Grande", "precioDelta": 20, "esDefault": false, "activo": true, "orden": 2}
        ]
      }
    ]
  }'
```

### 2. Actualizar modificadores
```bash
curl -X PUT http://localhost:5006/platillos/{ID} \
  -H "Authorization: Bearer {TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "categoriaId": "cat-123",
    "nombre": "Test Burger Plus",
    "precio": 120,
    "disponible": true,
    "modificadores": [
      {
        "grupoNombre": "Tamaño",
        "tipo": "single",
        "obligatorio": true,
        "minSelecciones": 1,
        "maxSelecciones": 1,
        "orden": 1,
        "opciones": [
          {"nombre": "Mediana", "precioDelta": 10, "esDefault": true, "activo": true, "orden": 1},
          {"nombre": "Jumbo", "precioDelta": 40, "esDefault": false, "activo": true, "orden": 2}
        ]
      }
    ]
  }'
```

### 3. Verificar GET
```bash
curl -X GET http://localhost:5006/platillos/{ID} \
  -H "Authorization: Bearer {TOKEN}"
```

---

## ?? Archivos Modificados/Creados

| Archivo | Tipo | Descripción |
|---------|------|-------------|
| `AccesoDatos/Models/ModificadorGrupo.cs` | ? Nuevo | Entidad para grupos |
| `AccesoDatos/Models/ModificadorOpcion.cs` | ? Nuevo | Entidad para opciones |
| `AccesoDatos/Models/Platillo.cs` | ?? Modificado | Agregada relación `Modificadores` |
| `AccesoDatos/Context/RestauranteDbContext.cs` | ?? Modificado | Configuración EF Core |
| `WebApi/DTOs/Menu/PlatilloDto.cs` | ?? Modificado | Agregados DTOs de modificadores |
| `WebApi/Services/MenuService.cs` | ?? Modificado | Lógica de sincronización |

---

## ?? ¡Listo para usar!

**Compilación:** ? Exitosa  
**Base de datos:** ? Tablas creadas  
**Endpoints:** ? Funcionando  
**Documentación:** ? Completa

---

**Desarrollado con:** .NET 8, Entity Framework Core, SQL Server  
**Fecha:** Febrero 2024
