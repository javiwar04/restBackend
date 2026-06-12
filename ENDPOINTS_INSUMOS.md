# ?? Endpoints de Insumos (Inventario)

**Fecha:** Febrero 2024  
**API Base URL:** `http://localhost:5006`  
**Status:** ? Implementado y funcionando

---

## ?? Resumen

Endpoints disponibles para gestión de inventario de insumos:

| Método | Endpoint | Descripción | Requiere |
|--------|----------|-------------|----------|
| GET | `/insumos` | Listar todos los insumos | Admin o Inventory |
| GET | `/insumos/{id}` | Obtener un insumo específico | Admin o Inventory |
| POST | `/insumos` | Crear nuevo insumo | Admin |
| PUT | `/insumos/{id}` | Editar insumo existente | Admin |
| DELETE | `/insumos/{id}` | Eliminar insumo | Admin |
| PATCH | `/insumos/{id}/ajuste` | Ajustar stock (entrada/salida) | Admin o Inventory |

---

## ?? Autenticación

Todos los endpoints requieren:
```
Authorization: Bearer {token}
```

**Roles permitidos:**
- GET, PATCH: `admin` o `inventory`
- POST, PUT, DELETE: solo `admin`

---

## ?? Endpoints Detallados

### 1. GET /insumos

Lista todos los insumos del inventario.

**Request:** No requiere body

**Response 200 OK:**
```json
[
  {
    "id": "uuid",
    "nombre": "Carne de res",
    "unidad": "kg",
    "stockActual": 5.5,
    "stockMinimo": 2.0,
    "costoUnitario": 120.00,
    "activo": true,
    "actualizadoEn": "2024-01-01T00:00:00Z"
  },
  {
    "id": "uuid",
    "nombre": "Pan para hamburguesa",
    "unidad": "pza",
    "stockActual": 50,
    "stockMinimo": 10,
    "costoUnitario": 3.50,
    "activo": true,
    "actualizadoEn": "2024-01-01T00:00:00Z"
  }
]
```

**Unidades válidas:** `kg`, `g`, `L`, `mL`, `pza`, `caja`

---

### 2. GET /insumos/{id}

Obtiene un insumo específico por su ID.

**Request:** No requiere body

**Response 200 OK:**
```json
{
  "id": "uuid",
  "nombre": "Carne de res",
  "unidad": "kg",
  "stockActual": 5.5,
  "stockMinimo": 2.0,
  "costoUnitario": 120.00,
  "activo": true,
  "actualizadoEn": "2024-01-01T00:00:00Z"
}
```

**Errores:**
```json
// 404 Not Found
{ "error": "Insumo no encontrado" }
```

---

### 3. POST /insumos

Crea un nuevo insumo en el inventario.

**Request:**
```json
{
  "nombre": "Carne de res",
  "unidad": "kg",
  "stockActual": 10.0,
  "stockMinimo": 2.0,
  "costoUnitario": 120.00
}
```

**Campos:**
- `nombre` (string, requerido): Nombre del insumo
- `unidad` (string, requerido): Unidad de medida (`kg`, `g`, `L`, `mL`, `pza`, `caja`)
- `stockActual` (decimal, opcional): Stock inicial (default: 0)
- `stockMinimo` (decimal, opcional): Stock mínimo para alertas (default: 0)
- `costoUnitario` (decimal, opcional): Costo por unidad (default: 0)

**Response 201 Created:**
```json
{
  "id": "uuid-generado",
  "nombre": "Carne de res",
  "unidad": "kg",
  "stockActual": 10.0,
  "stockMinimo": 2.0,
  "costoUnitario": 120.00,
  "activo": true,
  "actualizadoEn": "2024-01-01T00:00:00Z"
}
```

**Errores:**
```json
// 400 Bad Request
{ "error": "El nombre es requerido" }
{ "error": "La unidad es requerida" }
```

---

### 4. PUT /insumos/{id}

Actualiza un insumo existente.

**Request (todos los campos son opcionales):**
```json
{
  "nombre": "Carne de res premium",
  "unidad": "kg",
  "stockActual": 8.0,
  "stockMinimo": 3.0,
  "costoUnitario": 150.00,
  "activo": true
}
```

**Nota:** Solo se actualizan los campos que envíes (actualización parcial).

**Response 200 OK:**
```json
{
  "id": "uuid",
  "nombre": "Carne de res premium",
  "unidad": "kg",
  "stockActual": 8.0,
  "stockMinimo": 3.0,
  "costoUnitario": 150.00,
  "activo": true,
  "actualizadoEn": "2024-01-01T12:30:00Z"
}
```

**Errores:**
```json
// 404 Not Found
{ "error": "Insumo no encontrado" }
```

---

### 5. DELETE /insumos/{id}

Elimina un insumo del inventario.

**?? Validación importante:** No se puede eliminar un insumo que esté asignado a recetas.

**Response 204 No Content:** Sin body (exitoso)

**Errores:**
```json
// 404 Not Found
{ "error": "Insumo no encontrado" }

// 400 Bad Request
{ "error": "No se puede eliminar el insumo porque está asignado a recetas" }
```

---

### 6. PATCH /insumos/{id}/ajuste

Ajusta el stock de un insumo (entradas o salidas).

**Request:**
```json
{
  "tipo": "entrada",
  "cantidad": 5.0,
  "motivo": "Compra proveedor XYZ"
}
```

**Campos:**
- `tipo` (string, requerido): `"entrada"` o `"salida"`
- `cantidad` (decimal, requerido): Cantidad a agregar o quitar (debe ser > 0)
- `motivo` (string, requerido): Descripción del movimiento

**Response 200 OK:**
```json
{
  "id": "uuid",
  "nombre": "Carne de res",
  "unidad": "kg",
  "stockActual": 10.5,
  "stockMinimo": 2.0,
  "costoUnitario": 120.00,
  "activo": true,
  "actualizadoEn": "2024-01-01T14:00:00Z"
}
```

**Errores:**
```json
// 404 Not Found
{ "error": "Insumo no encontrado" }

// 400 Bad Request
{ "error": "La cantidad debe ser mayor a 0" }
{ "error": "El motivo es requerido" }
{ "error": "Stock insuficiente para realizar la salida" }
```

**Nota:** Este endpoint registra automáticamente el movimiento en la tabla `Insumos_Movimientos` con el usuario que lo realizó.

---

## ?? Código para Frontend (TypeScript)

### Agregar al archivo `lib/api.ts`:

```typescript
export const api = {
  // ... tus métodos existentes ...

  // Insumos
  getInsumos: () => apiCall('/insumos'),

  getInsumo: (id: string) => apiCall(`/insumos/${id}`),

  crearInsumo: (data: {
    nombre: string;
    unidad: string;
    stockActual?: number;
    stockMinimo?: number;
    costoUnitario?: number;
  }) =>
    apiCall('/insumos', {
      method: 'POST',
      body: JSON.stringify(data)
    }),

  editarInsumo: (id: string, data: {
    nombre?: string;
    unidad?: string;
    stockActual?: number;
    stockMinimo?: number;
    costoUnitario?: number;
    activo?: boolean;
  }) =>
    apiCall(`/insumos/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data)
    }),

  eliminarInsumo: (id: string) =>
    apiCall(`/insumos/${id}`, {
      method: 'DELETE'
    }),

  ajustarStock: (id: string, data: {
    tipo: 'entrada' | 'salida';
    cantidad: number;
    motivo: string;
  }) =>
    apiCall(`/insumos/${id}/ajuste`, {
      method: 'PATCH',
      body: JSON.stringify(data)
    })
};
```

---

## ?? Ejemplos de Uso

### Listar Insumos

```typescript
try {
  const insumos = await api.getInsumos();
  console.log('Insumos:', insumos);

  // Filtrar los que están bajo stock mínimo
  const bajosEnStock = insumos.filter(i => i.stockActual <= i.stockMinimo);
  console.log('Alertas:', bajosEnStock);
} catch (error: any) {
  alert(error.message);
}
```

### Crear Insumo

```typescript
try {
  const nuevoInsumo = await api.crearInsumo({
    nombre: "Aceite de oliva",
    unidad: "L",
    stockActual: 5,
    stockMinimo: 2,
    costoUnitario: 85.50
  });
  
  console.log('Insumo creado:', nuevoInsumo);
} catch (error: any) {
  alert(error.message);
}
```

### Ajustar Stock (Entrada)

```typescript
try {
  const insumoActualizado = await api.ajustarStock("abc-123", {
    tipo: "entrada",
    cantidad: 10,
    motivo: "Compra semanal - Proveedor ABC"
  });
  
  console.log('Stock actualizado:', insumoActualizado.stockActual);
} catch (error: any) {
  alert(error.message);
}
```

### Ajustar Stock (Salida)

```typescript
try {
  const insumoActualizado = await api.ajustarStock("abc-123", {
    tipo: "salida",
    cantidad: 2.5,
    motivo: "Merma por vencimiento"
  });
  
  console.log('Stock actualizado:', insumoActualizado.stockActual);
} catch (error: any) {
  // Si no hay suficiente stock, mostrará:
  // "Stock insuficiente para realizar la salida"
  alert(error.message);
}
```

### Eliminar Insumo

```typescript
try {
  await api.eliminarInsumo("abc-123");
  console.log('Insumo eliminado');
} catch (error: any) {
  // Si está en recetas, mostrará:
  // "No se puede eliminar el insumo porque está asignado a recetas"
  alert(error.message);
}
```

---

## ?? Pruebas con cURL

### 1. Login (obtener token)
```bash
curl -X POST http://localhost:5006/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "pin": "0000"
  }'
```

### 2. Listar Insumos
```bash
curl -X GET http://localhost:5006/insumos \
  -H "Authorization: Bearer {TU_TOKEN}"
```

### 3. Crear Insumo
```bash
curl -X POST http://localhost:5006/insumos \
  -H "Authorization: Bearer {TU_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Aceite de oliva",
    "unidad": "L",
    "stockActual": 5,
    "stockMinimo": 2,
    "costoUnitario": 85.50
  }'
```

### 4. Ajustar Stock (Entrada)
```bash
curl -X PATCH http://localhost:5006/insumos/{ID}/ajuste \
  -H "Authorization: Bearer {TU_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "tipo": "entrada",
    "cantidad": 10,
    "motivo": "Compra semanal"
  }'
```

### 5. Ajustar Stock (Salida)
```bash
curl -X PATCH http://localhost:5006/insumos/{ID}/ajuste \
  -H "Authorization: Bearer {TU_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "tipo": "salida",
    "cantidad": 2.5,
    "motivo": "Uso en cocina"
  }'
```

### 6. Editar Insumo
```bash
curl -X PUT http://localhost:5006/insumos/{ID} \
  -H "Authorization: Bearer {TU_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Aceite de oliva extra virgen",
    "costoUnitario": 95.00
  }'
```

### 7. Eliminar Insumo
```bash
curl -X DELETE http://localhost:5006/insumos/{ID} \
  -H "Authorization: Bearer {TU_TOKEN}"
```

---

## ? Validaciones Implementadas

### POST
- ? Nombre es requerido (no puede estar vacío)
- ? Unidad es requerida
- ? Unidad debe ser válida (`kg`, `g`, `L`, `mL`, `pza`, `caja`)
- ? El backend genera el ID automáticamente (GUID)

### PUT
- ? Valida que el insumo exista
- ? Solo actualiza los campos enviados

### DELETE
- ? Valida que el insumo exista
- ? No permite eliminar si está asignado a recetas
- ? Mensaje de error descriptivo

### PATCH (Ajuste)
- ? Tipo debe ser `"entrada"` o `"salida"`
- ? Cantidad debe ser mayor a 0
- ? Motivo es requerido
- ? Para salidas, valida que haya stock suficiente
- ? Registra el movimiento con usuario y fecha

---

## ?? Errores Comunes

### Error 401: Unauthorized
**Causa:** Token no enviado o inválido  
**Solución:** Verificar que el header `Authorization: Bearer {token}` esté presente

### Error 403: Forbidden
**Causa:** Usuario no tiene rol admin o inventory  
**Solución:** 
- GET/PATCH: Usuario debe tener rol `admin` o `inventory`
- POST/PUT/DELETE: Solo rol `admin`

### Error 400: Bad Request (Stock insuficiente)
**Causa:** Intentando hacer una salida mayor al stock disponible  
**Solución:** Verificar el stock actual antes de hacer salidas

### Error 400: Bad Request (Insumo en recetas)
**Causa:** Intentando eliminar insumo asignado a recetas  
**Solución:** Primero eliminar el insumo de todas las recetas

---

## ?? Unidades de Medida

| Código | Nombre | Uso Común |
|--------|--------|-----------|
| `kg` | Kilogramos | Carnes, frutas, verduras |
| `g` | Gramos | Especias, condimentos |
| `L` | Litros | Líquidos (aceite, leche) |
| `mL` | Mililitros | Líquidos pequeñas cantidades |
| `pza` | Piezas | Unidades individuales (huevos, panes) |
| `caja` | Cajas | Empaques completos |

---

## ?? Flujo de Trabajo Recomendado

### Alta de Insumo Nuevo
1. Crear insumo con `POST /insumos`
2. Hacer entrada inicial con `PATCH /insumos/{id}/ajuste`
3. Asignar a recetas si es necesario

### Compra de Proveedor
1. Hacer entrada con `PATCH /insumos/{id}/ajuste`
2. Tipo: `"entrada"`
3. Motivo: "Compra proveedor [Nombre]"

### Uso en Cocina (Manual)
1. Hacer salida con `PATCH /insumos/{id}/ajuste`
2. Tipo: `"salida"`
3. Motivo: "Uso en preparación [Platillo]"

**Nota:** Cuando se procesa un pago, el sistema **descuenta automáticamente** el stock según las recetas. No necesitas hacer ajustes manuales para ventas normales.

### Merma o Desperdicio
1. Hacer salida con `PATCH /insumos/{id}/ajuste`
2. Tipo: `"salida"`
3. Motivo: "Merma por [razón]"

### Alertas de Stock Bajo
```typescript
const insumos = await api.getInsumos();
const alertas = insumos.filter(i => 
  i.activo && i.stockActual <= i.stockMinimo
);

if (alertas.length > 0) {
  console.warn('Insumos bajo stock:', alertas);
  // Mostrar notificación en UI
}
```

---

## ?? Notas Importantes

- **IDs:** Todos los IDs son GUIDs (strings), no enteros
- **Stock Actual:** Se actualiza automáticamente con ventas (si hay recetas) y ajustes manuales
- **Stock Mínimo:** Es solo informativo para alertas, no impide hacer salidas
- **Costo Unitario:** Es el último costo de compra, útil para calcular valor de inventario
- **Activo:** Los insumos inactivos no deberían mostrarse en selecciones de recetas
- **Movimientos:** Todos los ajustes se registran en `Insumos_Movimientos` para auditoría

---

## ?? Para Empezar

1. **Verificar que el backend esté corriendo:**
   ```bash
   cd WebApi
   dotnet run
   ```
   Debe estar en: `http://localhost:5006`

2. **Probar en Swagger:**
   ```
   http://localhost:5006/swagger
   ```

3. **Integrar en frontend:**
   - Copiar el código TypeScript de arriba
   - Agregarlo a tu archivo `lib/api.ts`
   - Empezar a usar los métodos

---

## ?? Soporte

Si hay problemas:
1. Verificar que el backend esté corriendo en `http://localhost:5006`
2. Verificar que el usuario tenga rol `admin` o `inventory`
3. Revisar Swagger para ver los contratos exactos
4. Revisar logs del backend para errores específicos

---

**?? ¡Los endpoints de insumos están listos para integrarse!**

**Compilación:** ? Exitosa  
**Tests:** Pendientes desde frontend  
**Documentación:** ? Completa

---

**Desarrollado con:** .NET 8, Entity Framework Core, JWT, SQL Server  
**Fecha:** Febrero 2024
