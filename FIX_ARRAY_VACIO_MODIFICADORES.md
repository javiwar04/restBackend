# ? Fix: Array Vacío en Modificadores

**Fecha:** Febrero 2024  
**Status:** ? Corregido (v2 - Final)

---

## ?? Problema

El PUT con `modificadores: []` (array vacío) devolvía **500 Internal Server Error**.

```json
// ? Error 500
{
  "nombre": "Hamburguesa",
  "precio": 100,
  "modificadores": []  // Causaba excepción
}
```

---

## ? Solución

### **Cambio en la lógica:**

**Antes:**
```csharp
// ? Solo llamaba si había elementos
if (dto.Modificadores != null && dto.Modificadores.Any())
{
    await SincronizarModificadoresAsync(id, dto.Modificadores);
}
```

**Ahora:**
```csharp
// ? Llama si el campo viene (sea vacío o con elementos)
if (dto.Modificadores != null)
{
    await SincronizarModificadoresAsync(id, dto.Modificadores);
}
```

---

## ?? Comportamiento (v2)

| Payload | Campo `modificadores` | Resultado |
|---------|----------------------|-----------|
| Sin campo | `null` (no viene) | ? **No toca** modificadores existentes |
| `"modificadores": []` | `[]` (array vacío) | ? **Borra todos** los modificadores |
| `"modificadores": [...]` | Array con elementos | ? **Reemplaza todos** los modificadores |

---

## ?? Casos de Prueba

### **Test 1: Actualizar sin tocar modificadores**

```typescript
// ? No enviar campo "modificadores"
const payload = {
  categoriaId: "cat-123",
  nombre: "Hamburguesa Renamed",
  precio: 110,
  disponible: true
  // Sin campo "modificadores"
};

await fetch(`/platillos/${id}`, {
  method: 'PUT',
  body: JSON.stringify(payload)
});

// ? Modificadores se mantienen sin cambios
```

### **Test 2: Borrar todos los modificadores**

```typescript
// ? Enviar array vacío
const payload = {
  categoriaId: "cat-123",
  nombre: "Hamburguesa",
  precio: 100,
  disponible: true,
  modificadores: []  // ? Borra todos
};

await fetch(`/platillos/${id}`, {
  method: 'PUT',
  body: JSON.stringify(payload)
});

// ? Todos los modificadores se eliminan
```

### **Test 3: Crear/Reemplazar modificadores**

```typescript
// ? Enviar array con elementos
const payload = {
  categoriaId: "cat-123",
  nombre: "Hamburguesa",
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

await fetch(`/platillos/${id}`, {
  method: 'PUT',
  body: JSON.stringify(payload)
});

// ? Reemplaza todos los modificadores con los nuevos
```

---

## ? Cambios Aplicados

| Archivo | Método | Cambio |
|---------|--------|--------|
| `MenuService.cs` | `CreatePlatilloAsync()` | ? Cambiado `!= null && Any()` ? `!= null` |
| `MenuService.cs` | `UpdatePlatilloAsync()` | ? Cambiado `!= null && Any()` ? `!= null` |
| `MenuService.cs` | `SincronizarModificadoresAsync()` | ? Maneja correctamente `[]` y `null` |

---

## ?? Para Aplicar

1. **Reiniciar backend:**
```sh
# Detener (Ctrl+C)
cd WebApi
dotnet run
```

2. **Probar los 3 casos:**
   - Sin campo ? No toca
   - `[]` ? Borra
   - `[...]` ? Reemplaza

---

## ? Validaciones

- ? PUT sin campo `modificadores` ? 200 (no toca)
- ? PUT con `modificadores: []` ? 200 (borra todos)
- ? PUT con `modificadores: [...]` ? 200 (reemplaza)
- ? POST funciona igual que PUT
- ? GET no lanza 500 con platillos sin modificadores

---

**Compilación:** ? Exitosa  
**Requiere:** ?? Reiniciar backend  
**Listo para usar:** ? Sí

---

**Desarrollado con:** .NET 8, Entity Framework Core  
**Fecha:** Febrero 2024
