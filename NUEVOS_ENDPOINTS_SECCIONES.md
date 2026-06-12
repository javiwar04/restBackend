# ?? Nuevos Endpoints: CRUD de Secciones

**Fecha:** Febrero 2024  
**API Base URL:** `http://localhost:5006`  
**Status:** ? Implementado y funcionando

---

## ?? Resumen

Se agregaron **3 nuevos endpoints** para completar el CRUD de secciones:

| Método | Endpoint | Descripción | Requiere |
|--------|----------|-------------|----------|
| POST | `/secciones` | Crear nueva sección | Admin |
| PUT | `/secciones/{id}` | Editar sección existente | Admin |
| DELETE | `/secciones/{id}` | Eliminar sección | Admin |

**Nota:** El endpoint `GET /secciones` ya existía y sigue funcionando igual.

---

## ?? Autenticación

Todos los endpoints requieren:
```
Authorization: Bearer {token}
```

Y el usuario debe tener **rol `admin`**.

---

## ?? Endpoints Detallados

### 1. POST /secciones

Crea una nueva sección del salón.

**Request:**
```json
{
  "nombre": "Terraza",
  "orden": 4,
  "activa": true
}
```

**Campos:**
- `nombre` (string, requerido): Nombre de la sección
- `orden` (int, opcional): Orden de aparición (default: 0)
- `activa` (bool, opcional): Si está activa (default: true)

**Response 201 Created:**
```json
{
  "id": "a3f1b2c4-d5e6-7f8g-9h0i-1j2k3l4m5n6o",
  "nombre": "Terraza",
  "orden": 4,
  "activa": true,
  "mesas": []
}
```

**Errores:**
```json
// 400 Bad Request
{ "error": "El nombre es requerido" }

// 409 Conflict
{ "error": "Ya existe una sección con ese nombre" }
```

---

### 2. PUT /secciones/{id}

Actualiza una sección existente.

**Request (todos los campos son opcionales):**
```json
{
  "nombre": "Terraza Norte",
  "orden": 5,
  "activa": true
}
```

**Nota:** Solo se actualizan los campos que envíes (actualización parcial).

**Response 200 OK:**
```json
{
  "id": "a3f1b2c4-...",
  "nombre": "Terraza Norte",
  "orden": 5,
  "activa": true,
  "mesas": [
    {
      "id": "xyz...",
      "numero": 25,
      "etiqueta": "T1",
      "capacidad": 6,
      "seccionId": "a3f1b2c4-...",
      "activa": true,
      "notas": null
    }
  ]
}
```

**Errores:**
```json
// 404 Not Found
{ "error": "Sección no encontrada" }

// 409 Conflict
{ "error": "Ya existe una sección con ese nombre" }
```

---

### 3. DELETE /secciones/{id}

Elimina una sección.

**?? Validación importante:** No se puede eliminar una sección que tenga mesas asignadas.

**Response 200 OK:**
```json
{ "ok": true }
```

**Errores:**
```json
// 404 Not Found
{ "error": "Sección no encontrada" }

// 409 Conflict
{ "error": "La sección tiene mesas asignadas. Elimina o reasigna las mesas primero." }
```

---

## ?? Código para Frontend (TypeScript)

### Agregar al archivo `lib/api.ts`:

```typescript
export const api = {
  // ... tus métodos existentes ...

  // Secciones
  getSecciones: () => apiCall('/secciones'),

  crearSeccion: (data: { nombre: string; orden?: number; activa?: boolean }) =>
    apiCall('/secciones', {
      method: 'POST',
      body: JSON.stringify(data)
    }),

  editarSeccion: (id: string, data: { nombre?: string; orden?: number; activa?: boolean }) =>
    apiCall(`/secciones/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data)
    }),

  eliminarSeccion: (id: string) =>
    apiCall(`/secciones/${id}`, {
      method: 'DELETE'
    })
};
```

---

## ?? Ejemplos de Uso

### Crear Sección

```typescript
try {
  const nuevaSeccion = await api.crearSeccion({
    nombre: "Jardín",
    orden: 4,
    activa: true
  });
  
  console.log('Sección creada:', nuevaSeccion);
  // Recargar lista de secciones
} catch (error: any) {
  alert(error.message); // Mostrará el error del backend
}
```

### Editar Sección

```typescript
try {
  const seccionActualizada = await api.editarSeccion("abc-123", {
    nombre: "Jardín Interior"
    // Puedes enviar solo los campos que quieras cambiar
  });
  
  console.log('Sección actualizada:', seccionActualizada);
} catch (error: any) {
  alert(error.message);
}
```

### Eliminar Sección

```typescript
try {
  await api.eliminarSeccion("abc-123");
  console.log('Sección eliminada');
} catch (error: any) {
  // Si tiene mesas asignadas, mostrará:
  // "La sección tiene mesas asignadas. Elimina o reasigna las mesas primero."
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

### 2. Crear Sección
```bash
curl -X POST http://localhost:5006/secciones \
  -H "Authorization: Bearer {TU_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Terraza",
    "orden": 4,
    "activa": true
  }'
```

### 3. Editar Sección
```bash
curl -X PUT http://localhost:5006/secciones/{ID} \
  -H "Authorization: Bearer {TU_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Terraza Norte"
  }'
```

### 4. Eliminar Sección
```bash
curl -X DELETE http://localhost:5006/secciones/{ID} \
  -H "Authorization: Bearer {TU_TOKEN}"
```

---

## ? Validaciones Implementadas

### POST
- ? Nombre es requerido (no puede estar vacío)
- ? Nombre debe ser único (case-insensitive)
- ? El backend genera el ID automáticamente (GUID)

### PUT
- ? Valida que la sección exista
- ? Si se cambia el nombre, valida que sea único
- ? Solo actualiza los campos enviados

### DELETE
- ? Valida que la sección exista
- ? No permite eliminar si tiene mesas asignadas
- ? Mensaje de error descriptivo

---

## ?? Errores Comunes

### Error 401: Unauthorized
**Causa:** Token no enviado o inválido  
**Solución:** Verificar que el header `Authorization: Bearer {token}` esté presente

### Error 403: Forbidden
**Causa:** Usuario no tiene rol admin  
**Solución:** Solo usuarios con rol `admin` pueden crear/editar/eliminar secciones

### Error 409: Conflict (nombre duplicado)
**Causa:** Ya existe una sección con ese nombre  
**Solución:** Usar otro nombre único

### Error 409: Conflict (tiene mesas)
**Causa:** Intentando eliminar sección con mesas asignadas  
**Solución:** Primero eliminar o reasignar las mesas a otra sección

---

## ?? Estado Actual de la API

### Endpoints de Secciones (Total: 4)

| Método | Endpoint | Status |
|--------|----------|--------|
| GET | `/secciones` | ? Existente |
| POST | `/secciones` | ? **NUEVO** |
| PUT | `/secciones/{id}` | ? **NUEVO** |
| DELETE | `/secciones/{id}` | ? **NUEVO** |

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
2. Verificar que el usuario tenga rol `admin`
3. Revisar Swagger para ver los contratos exactos
4. Revisar logs del backend para errores específicos

---

## ?? Notas Importantes

- **IDs:** Todos los IDs son GUIDs (strings), no enteros
- **Nombres:** Son case-insensitive para validación de duplicados
- **Orden:** Se usa para ordenar las secciones en la UI (menor orden = primero)
- **Activa:** Las secciones inactivas no deberían mostrarse en el POS

---

**?? ¡Los endpoints están listos para integrarse!**

**Compilación:** ? Exitosa  
**Tests:** Pendientes desde frontend  
**Documentación:** ? Completa

---

**Desarrollado con:** .NET 8, Entity Framework Core, JWT  
**Fecha:** Febrero 2024
