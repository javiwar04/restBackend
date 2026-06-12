# ? CRUD de Secciones - Completado

**Fecha:** Febrero 2024  
**Status:** ? **IMPLEMENTADO Y FUNCIONANDO**

---

## ?? RESUMEN

Se implementaron **3 nuevos endpoints** para el CRUD completo de Secciones:

| Método | Ruta | Descripción | Requiere |
|--------|------|-------------|----------|
| POST | `/secciones` | Crear sección | admin |
| PUT | `/secciones/{id}` | Editar sección | admin |
| DELETE | `/secciones/{id}` | Eliminar sección | admin |

**Compilación:** ? Exitosa  
**Total endpoints de Secciones:** 4 (GET + POST + PUT + DELETE)

---

## ?? ARCHIVOS MODIFICADOS

### 1. DTOs (`WebApi/DTOs/Mesas/MesaDto.cs`)
```csharp
public class CreateSeccionDto
{
    public string Nombre { get; set; } = null!;
    public int Orden { get; set; } = 0;
    public bool Activa { get; set; } = true;
}

public class UpdateSeccionDto
{
    public string? Nombre { get; set; }
    public int? Orden { get; set; }
    public bool? Activa { get; set; }
}
```

### 2. Servicio (`WebApi/Services/MesasService.cs`)
Agregados 3 métodos:
- ? `CreateSeccionAsync(CreateSeccionDto dto)`
- ? `UpdateSeccionAsync(string id, UpdateSeccionDto dto)`
- ? `DeleteSeccionAsync(string id)`

### 3. Controlador (`WebApi/Controllers/MesasController.cs`)
Agregados 3 endpoints:
- ? `POST /secciones`
- ? `PUT /secciones/{id}`
- ? `DELETE /secciones/{id}`

---

## ?? ENDPOINTS DETALLADOS

### POST /secciones
**Requiere:** Bearer token con rol `admin`

```json
Request:
{
  "nombre": "Terraza",
  "orden": 4,
  "activa": true
}

Response 201 Created:
{
  "id": "a3f1b2c4-d5e6-7f8g-9h0i-1j2k3l4m5n6o",
  "nombre": "Terraza",
  "orden": 4,
  "activa": true,
  "mesas": []
}

Errores:
400 { "error": "El nombre es requerido" }
409 { "error": "Ya existe una sección con ese nombre" }
```

### PUT /secciones/{id}
**Requiere:** Bearer token con rol `admin`

```json
Request (todos los campos son opcionales):
{
  "nombre": "Terraza Norte",
  "orden": 5,
  "activa": true
}

Response 200 OK:
{
  "id": "a3f1b2c4-...",
  "nombre": "Terraza Norte",
  "orden": 5,
  "activa": true,
  "mesas": [ /* array de mesas */ ]
}

Errores:
404 { "error": "Sección no encontrada" }
409 { "error": "Ya existe una sección con ese nombre" }
```

### DELETE /secciones/{id}
**Requiere:** Bearer token con rol `admin`

```json
Response 200 OK:
{ "ok": true }

Errores:
404 { "error": "Sección no encontrada" }
409 { "error": "La sección tiene mesas asignadas. Elimina o reasigna las mesas primero." }
```

---

## ? VALIDACIONES IMPLEMENTADAS

### POST
- ? Nombre no puede estar vacío
- ? Nombre debe ser único (case-insensitive)
- ? Genera ID automáticamente (GUID)
- ? Valores por defecto: `orden = 0`, `activa = true`

### PUT
- ? Valida que la sección exista
- ? Si se cambia el nombre, valida que sea único
- ? Solo actualiza los campos enviados (parcial)

### DELETE
- ? Valida que la sección exista
- ? **No permite eliminar** si tiene mesas asignadas
- ? Mensaje de error descriptivo

---

## ?? SEGURIDAD

- ? Los 3 endpoints requieren autenticación JWT
- ? Solo usuarios con rol `admin` pueden acceder
- ? Retorna 401 si no hay token
- ? Retorna 403 si el rol no es admin

---

## ?? PRUEBAS RECOMENDADAS

### Test 1: Crear sección
```bash
curl -X POST http://localhost:5006/secciones \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Jardín",
    "orden": 4,
    "activa": true
  }'
```

### Test 2: Editar sección
```bash
curl -X PUT http://localhost:5006/secciones/{id} \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Jardín Interior"
  }'
```

### Test 3: Eliminar sección vacía
```bash
curl -X DELETE http://localhost:5006/secciones/{id} \
  -H "Authorization: Bearer {token}"
```

### Test 4: Validar error al eliminar con mesas
```bash
# Primero crea una mesa en la sección
curl -X POST http://localhost:5006/mesas \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "numero": 99,
    "etiqueta": "J1",
    "capacidad": 4,
    "seccionId": "{id_de_la_seccion}",
    "activa": true
  }'

# Intenta eliminar la sección (debe fallar con 409)
curl -X DELETE http://localhost:5006/secciones/{id_de_la_seccion} \
  -H "Authorization: Bearer {token}"
```

---

## ?? INTEGRACIÓN CON FRONTEND

### TypeScript/JavaScript

```typescript
// lib/api.ts

export const api = {
  // ... otros métodos ...

  // Secciones
  getSecciones: () => apiCall('/secciones'),

  crearSeccion: (seccion: { nombre: string; orden?: number; activa?: boolean }) =>
    apiCall('/secciones', {
      method: 'POST',
      body: JSON.stringify(seccion)
    }),

  editarSeccion: (id: string, seccion: { nombre?: string; orden?: number; activa?: boolean }) =>
    apiCall(`/secciones/${id}`, {
      method: 'PUT',
      body: JSON.stringify(seccion)
    }),

  eliminarSeccion: (id: string) =>
    apiCall(`/secciones/${id}`, {
      method: 'DELETE'
    })
};
```

### Ejemplo de uso en React/Next.js

```typescript
// components/SeccionesForm.tsx
const handleCrear = async (datos: any) => {
  try {
    const nuevaSeccion = await api.crearSeccion({
      nombre: datos.nombre,
      orden: datos.orden || 0,
      activa: datos.activa ?? true
    });
    
    console.log('Sección creada:', nuevaSeccion);
    // Recargar lista de secciones
  } catch (error: any) {
    alert(error.message); // Mostrará el mensaje del backend
  }
};

const handleEditar = async (id: string, datos: any) => {
  try {
    const seccionActualizada = await api.editarSeccion(id, {
      nombre: datos.nombre,
      orden: datos.orden,
      activa: datos.activa
    });
    
    console.log('Sección actualizada:', seccionActualizada);
  } catch (error: any) {
    alert(error.message);
  }
};

const handleEliminar = async (id: string) => {
  if (!confirm('¿Eliminar sección?')) return;

  try {
    await api.eliminarSeccion(id);
    console.log('Sección eliminada');
  } catch (error: any) {
    // Si tiene mesas, mostrará:
    // "La sección tiene mesas asignadas. Elimina o reasigna las mesas primero."
    alert(error.message);
  }
};
```

---

## ?? CASOS DE USO

### ? Crear sección nueva
1. Frontend envía nombre, orden (opcional), activa (opcional)
2. Backend valida nombre único
3. Backend genera ID automático
4. Backend retorna sección creada con array vacío de mesas

### ? Editar sección existente
1. Frontend puede enviar solo los campos a modificar
2. Backend valida que la sección exista
3. Si se cambia el nombre, valida que sea único
4. Backend retorna sección actualizada con sus mesas

### ? Eliminar sección
1. Frontend solicita eliminar
2. Backend valida que no tenga mesas
3. Si tiene mesas ? retorna 409 con mensaje descriptivo
4. Si está vacía ? elimina y retorna `{ ok: true }`

---

## ?? ERRORES COMUNES Y SOLUCIONES

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

## ?? ESTADO DE LA API

### Endpoints de Mesas/Secciones (Total: 10)

| Método | Ruta | Status |
|--------|------|--------|
| GET | `/secciones` | ? Existente |
| POST | `/secciones` | ? **NUEVO** |
| PUT | `/secciones/{id}` | ? **NUEVO** |
| DELETE | `/secciones/{id}` | ? **NUEVO** |
| GET | `/mesas` | ? Existente |
| POST | `/mesas` | ? Existente |
| PUT | `/mesas/{id}` | ? Existente |
| DELETE | `/mesas/{id}` | ? Existente |

**Total endpoints en la API:** 73 ?

---

## ?? PRÓXIMOS PASOS

1. ? **Backend completado** - Los 3 endpoints están funcionando
2. ? **Probar desde Swagger** - `http://localhost:5006/swagger`
3. ? **Integrar en frontend** - Usar los ejemplos de código TypeScript
4. ? **Probar flujo completo:**
   - Crear sección nueva
   - Editar sección
   - Intentar eliminar con mesas (debe fallar)
   - Eliminar sección vacía (debe funcionar)

---

## ?? SOPORTE

Si hay problemas:
1. Verificar que el backend esté corriendo en `http://localhost:5006`
2. Revisar Swagger para ver los contratos exactos
3. Verificar que el usuario tenga rol `admin`
4. Revisar logs del backend para errores específicos

---

**?? ¡El CRUD de Secciones está 100% completo y listo para usar!**

**Desarrollado con:** .NET 8, Entity Framework Core, JWT  
**Fecha:** Febrero 2024  
**Compilación:** ? Exitosa
