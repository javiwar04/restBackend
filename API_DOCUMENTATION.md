# ?? BACKEND API - DOCUMENTACIÓN COMPLETA

**Base URL:** `http://localhost:5006`

**Última actualización:** Febrero 2024

---

## ?? TABLA DE CONTENIDO

1. [Autenticación](#autenticación)
2. [Categorías del Menú](#categorías-del-menú)
3. [Platillos](#platillos)
4. [Secciones y Mesas](#secciones-y-mesas)
5. [Turnos](#turnos)
6. [Órdenes](#órdenes)
7. [Cocina](#cocina)
8. [Pagos](#pagos)
9. [Usuarios de Prueba](#usuarios-de-prueba)
10. [Códigos de Error](#códigos-de-error)
11. [Notas Importantes](#notas-importantes)
12. [Flujo Completo](#flujo-completo-de-uso)

---

## ?? AUTENTICACIÓN

Todos los endpoints (excepto `/auth/login`) requieren el header:
```
Authorization: Bearer {token}
```

### POST /auth/login

Iniciar sesión con username y PIN.

**Request:**
```json
{
  "username": "admin",
  "pin": "0000"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "nombre": "Administrador",
    "username": "admin",
    "rol": "admin",
    "modules": ["pos", "admin", "reports", "inventory", "billing", "kitchen"]
  }
}
```

**Response 401:**
```json
{
  "error": "Credenciales inválidas"
}
```

---

### GET /auth/me

Obtener usuario autenticado actual.

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "nombre": "Administrador",
  "username": "admin",
  "rol": "admin",
  "modules": ["pos", "admin", "reports", "inventory", "billing", "kitchen"]
}
```

---

### POST /auth/logout

Cerrar sesión.

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
{
  "ok": true
}
```

---

## ?? CATEGORÍAS DEL MENÚ

### GET /categorias-menu

Listar todas las categorías ordenadas por campo `orden`.

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "nombre": "Entradas",
    "orden": 1,
    "activa": true
  },
  {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "nombre": "Principales",
    "orden": 2,
    "activa": true
  }
]
```

---

### GET /categorias-menu/{id}

Obtener una categoría específica.

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "nombre": "Entradas",
  "orden": 1,
  "activa": true
}
```

**Response 404:**
```json
{
  "error": "Categoría no encontrada"
}
```

---

### POST /categorias-menu

Crear nueva categoría.

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "nombre": "Postres",
  "orden": 4,
  "activa": true
}
```

**Response 201:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "nombre": "Postres",
  "orden": 4,
  "activa": true
}
```

---

### PUT /categorias-menu/{id}

Actualizar categoría existente.

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "nombre": "Postres Especiales",
  "orden": 4,
  "activa": false
}
```

**Response 200:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "nombre": "Postres Especiales",
  "orden": 4,
  "activa": false
}
```

---

### DELETE /categorias-menu/{id}

Eliminar categoría (solo si no tiene platillos asociados).

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
{
  "ok": true
}
```

**Response 409:**
```json
{
  "error": "La categoría tiene platillos asociados"
}
```

---

## ??? PLATILLOS

### GET /platillos

Listar platillos con filtros opcionales.

**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `categoria_id` (opcional): UUID de la categoría
- `disponible` (opcional): `true` o `false`
- `q` (opcional): Texto de búsqueda en nombre y descripción

**Ejemplo:** `GET /platillos?categoria_id=550e8400-e29b-41d4-a716-446655440000&disponible=true`

**Response 200:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440010",
    "categoriaId": "550e8400-e29b-41d4-a716-446655440001",
    "categoriaNombre": "Principales",
    "nombre": "Filete al Gusto",
    "descripcion": "300g de filete con guarnición",
    "precio": 285.00,
    "disponible": true,
    "imagenUrl": null,
    "modificadores": [
      {
        "grupoId": "550e8400-e29b-41d4-a716-446655440020",
        "grupoNombre": "Término",
        "tipo": "single",
        "opciones": [
          {
            "id": "550e8400-e29b-41d4-a716-446655440021",
            "nombre": "Término medio",
            "precioDelta": 0.00,
            "esDefault": true
          },
          {
            "id": "550e8400-e29b-41d4-a716-446655440022",
            "nombre": "Bien cocido",
            "precioDelta": 0.00,
            "esDefault": false
          }
        ]
      }
    ]
  }
]
```

---

### GET /platillos/{id}

Obtener platillo con modificadores.

**Headers:** `Authorization: Bearer {token}`

**Response 200:** (mismo formato que lista)

**Response 404:**
```json
{
  "error": "Platillo no encontrado"
}
```

---

### POST /platillos

Crear nuevo platillo.

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "categoriaId": "550e8400-e29b-41d4-a716-446655440001",
  "nombre": "Pizza Hawaiana",
  "descripcion": "Piña y jamón",
  "precio": 145.00,
  "disponible": true,
  "imagenUrl": null
}
```

**Response 201:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440030",
  "categoriaId": "550e8400-e29b-41d4-a716-446655440001",
  "categoriaNombre": "Principales",
  "nombre": "Pizza Hawaiana",
  "descripcion": "Piña y jamón",
  "precio": 145.00,
  "disponible": true,
  "imagenUrl": null,
  "modificadores": []
}
```

---

### PUT /platillos/{id}

Actualizar platillo.

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "categoriaId": "550e8400-e29b-41d4-a716-446655440001",
  "nombre": "Pizza Hawaiana Grande",
  "descripcion": "Piña y jamón - tamaño grande",
  "precio": 185.00,
  "disponible": true,
  "imagenUrl": "https://example.com/pizza.jpg"
}
```

**Response 200:** Platillo actualizado

---

### PATCH /platillos/{id}/disponible

Cambiar disponibilidad rápida (sin modificar otros campos).

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "disponible": false
}
```

**Response 200:**
```json
{
  "ok": true
}
```

---

### DELETE /platillos/{id}

Eliminar platillo.

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
{
  "ok": true
}
```

---

## ?? SECCIONES Y MESAS

### GET /secciones

Listar secciones con sus mesas incluidas.

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440100",
    "nombre": "Restaurante",
    "orden": 1,
    "activa": true,
    "mesas": [
      {
        "id": "550e8400-e29b-41d4-a716-446655440110",
        "numero": 1,
        "etiqueta": "R1",
        "capacidad": 4,
        "seccionId": "550e8400-e29b-41d4-a716-446655440100",
        "activa": true,
        "notas": null
      },
      {
        "id": "550e8400-e29b-41d4-a716-446655440111",
        "numero": 2,
        "etiqueta": "R2",
        "capacidad": 6,
        "seccionId": "550e8400-e29b-41d4-a716-446655440100",
        "activa": false,
        "notas": "Ocupada"
      }
    ]
  }
]
```

---

### GET /mesas

Listar mesas con filtros.

**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `seccion_id` (opcional): UUID de la sección
- `activa` (opcional): `true` o `false`

**Ejemplo:** `GET /mesas?seccion_id=550e8400-e29b-41d4-a716-446655440100&activa=true`

**Response 200:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440110",
    "numero": 1,
    "etiqueta": "R1",
    "capacidad": 4,
    "seccionId": "550e8400-e29b-41d4-a716-446655440100",
    "seccionNombre": "Restaurante",
    "activa": true,
    "notas": null
  }
]
```

---

### POST /mesas

Crear nueva mesa.

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "numero": 15,
  "etiqueta": "R15",
  "capacidad": 6,
  "seccionId": "550e8400-e29b-41d4-a716-446655440100",
  "activa": true,
  "notas": "Cerca de la ventana"
}
```

**Response 201:** Mesa creada

---

### PUT /mesas/{id}

Actualizar mesa.

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "numero": 15,
  "etiqueta": "R15-VIP",
  "capacidad": 8,
  "seccionId": "550e8400-e29b-41d4-a716-446655440100",
  "activa": true,
  "notas": "Ventana con vista"
}
```

**Response 200:** Mesa actualizada

---

### DELETE /mesas/{id}

Eliminar mesa.

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
{
  "ok": true
}
```

---

## ? TURNOS

### GET /turnos/activo

Obtener turno activo del usuario autenticado.

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440200",
  "usuarioId": "550e8400-e29b-41d4-a716-446655440050",
  "usuarioNombre": "Ana Mesera",
  "inicio": "2024-02-23T08:00:00Z",
  "fin": null,
  "totalVentas": 1250.00,
  "totalOrdenes": 12,
  "ventasEfectivo": 600.00,
  "ventasTarjeta": 500.00,
  "ventasTransfer": 150.00,
  "notas": null
}
```

**Response 404:**
```json
{
  "error": "Sin turno activo"
}
```

---

### GET /turnos/{id}

Obtener turno específico.

**Headers:** `Authorization: Bearer {token}`

**Response 200:** (mismo formato que turno activo)

---

### POST /turnos

Abrir nuevo turno.

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "efectivoInicial": 500.00
}
```

**Response 201:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440200",
  "usuarioId": "550e8400-e29b-41d4-a716-446655440050",
  "usuarioNombre": "Ana",
  "inicio": "2024-02-23T08:00:00Z",
  "fin": null,
  "totalVentas": 0,
  "totalOrdenes": 0,
  "ventasEfectivo": 0,
  "ventasTarjeta": 0,
  "ventasTransfer": 0,
  "notas": null
}
```

**Response 409:**
```json
{
  "error": "Ya existe un turno activo para este usuario"
}
```

---

### PATCH /turnos/{id}/cerrar

Cerrar turno y generar corte automático.

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "efectivoFinalReal": 1150.00,
  "notas": "Todo correcto"
}
```

**Response 200:**
```json
{
  "turno": {
    "id": "550e8400-e29b-41d4-a716-446655440200",
    "usuarioId": "550e8400-e29b-41d4-a716-446655440050",
    "usuarioNombre": "Ana",
    "inicio": "2024-02-23T08:00:00Z",
    "fin": "2024-02-23T18:00:00Z",
    "totalVentas": 2500.00,
    "totalOrdenes": 25,
    "ventasEfectivo": 600.00,
    "ventasTarjeta": 1500.00,
    "ventasTransfer": 400.00,
    "notas": "Todo correcto"
  },
  "corte": {
    "id": "550e8400-e29b-41d4-a716-446655440300",
    "turnoId": "550e8400-e29b-41d4-a716-446655440200",
    "fechaInicio": "2024-02-23T08:00:00Z",
    "fechaFin": "2024-02-23T18:00:00Z",
    "efectivoInicial": 500.00,
    "efectivoFinalSistema": 1100.00,
    "efectivoFinalReal": 1150.00,
    "diferencia": 50.00,
    "totalVentas": 2500.00,
    "totalOrdenes": 25,
    "totalEfectivo": 600.00,
    "totalTarjeta": 1500.00,
    "totalTransferencia": 400.00,
    "totalPropinas": 250.00,
    "totalImpuestos": 400.00,
    "totalDescuentos": 50.00,
    "notas": "Todo correcto"
  }
}
```

---

## ?? ÓRDENES

### GET /ordenes

Listar órdenes con filtros.

**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `estado` (opcional): pendiente, en_cocina, servido, pagado, cancelado
- `mesa_id` (opcional): UUID de la mesa
- `turno_id` (opcional): UUID del turno
- `desde` (opcional): Fecha ISO 8601 (ej: 2024-02-23T00:00:00Z)
- `hasta` (opcional): Fecha ISO 8601
- `limit` (opcional): Número máximo de resultados (default: 100)

**Ejemplo:** `GET /ordenes?estado=pendiente&turno_id=550e8400-e29b-41d4-a716-446655440200`

**Response 200:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440400",
    "mesaId": "550e8400-e29b-41d4-a716-446655440110",
    "numeroMesa": 5,
    "tipoServicio": "mesa",
    "estado": "pendiente",
    "comensales": 4,
    "usuarioNombre": "Carlos",
    "meseroNombre": "Ana",
    "descuento": 0.00,
    "propina": 0.00,
    "subtotal": 520.00,
    "impuestos": 83.20,
    "total": 603.20,
    "creadoEn": "2024-02-23T12:30:00Z",
    "actualizadoEn": "2024-02-23T12:30:00Z",
    "notas": null,
    "items": [
      {
        "id": 123,
        "platilloId": "550e8400-e29b-41d4-a716-446655440010",
        "nombre": "Filete al Gusto",
        "precioUnitario": 185.00,
        "cantidad": 2,
        "notas": "Sin sal",
        "estado": "pendiente",
        "modificadores": [
          {
            "grupoNombre": "Término",
            "opcionNombre": "Bien cocido",
            "precioDelta": 0.00
          }
        ]
      },
      {
        "id": 124,
        "platilloId": "550e8400-e29b-41d4-a716-446655440011",
        "nombre": "Agua Natural",
        "precioUnitario": 25.00,
        "cantidad": 4,
        "notas": null,
        "estado": "pendiente",
        "modificadores": []
      }
    ]
  }
]
```

---

### GET /ordenes/{id}

Obtener orden completa con items.

**Headers:** `Authorization: Bearer {token}`

**Response 200:** (mismo formato que lista)

**Response 404:**
```json
{
  "error": "Orden no encontrada"
}
```

---

### POST /ordenes

Crear nueva orden.

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "mesaId": "550e8400-e29b-41d4-a716-446655440110",
  "tipoServicio": "mesa",
  "comensales": 4,
  "meseroId": "550e8400-e29b-41d4-a716-446655440050",
  "turnoId": "550e8400-e29b-41d4-a716-446655440200",
  "notas": "Cumpleaños",
  "items": [
    {
      "platilloId": "550e8400-e29b-41d4-a716-446655440010",
      "nombre": "Filete al Gusto",
      "precioUnitario": 185.00,
      "cantidad": 2,
      "notas": "Sin sal",
      "modificadores": [
        {
          "grupoNombre": "Término",
          "opcionNombre": "Bien cocido",
          "precioDelta": 0.00
        }
      ]
    },
    {
      "platilloId": "550e8400-e29b-41d4-a716-446655440011",
      "nombre": "Coca Cola",
      "precioUnitario": 35.00,
      "cantidad": 2,
      "notas": null,
      "modificadores": []
    }
  ]
}
```

**Response 201:** Orden completa con totales calculados

**Nota:** Automáticamente marca la mesa como ocupada y crea alerta para cocina.

---

### PUT /ordenes/{id}

Actualizar descuento, propina, notas (recalcula totales).

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "descuento": 50.00,
  "propina": 60.00,
  "notas": "Cliente frecuente",
  "comensales": 4
}
```

**Response 200:** Orden con totales recalculados

---

### PATCH /ordenes/{id}/estado

Cambiar estado de la orden.

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "estado": "en_cocina"
}
```

**Estados válidos:** pendiente, en_cocina, servido, pagado, cancelado

**Response 200:**
```json
{
  "ok": true
}
```

---

### DELETE /ordenes/{id}

Cancelar orden (cambia estado a cancelado, libera mesa).

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
{
  "ok": true
}
```

---

### POST /ordenes/{id}/items

Agregar item a orden existente (recalcula totales).

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "platilloId": "550e8400-e29b-41d4-a716-446655440012",
  "nombre": "Cerveza Corona",
  "precioUnitario": 45.00,
  "cantidad": 2,
  "notas": "Bien fría",
  "modificadores": []
}
```

**Response 201:** Orden actualizada con nuevo item

---

### PUT /ordenes/{id}/items/{itemId}

Actualizar cantidad o notas de un item (recalcula totales).

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "cantidad": 3,
  "notas": "Sin hielo"
}
```

**Response 200:** Orden con item actualizado

---

### DELETE /ordenes/{id}/items/{itemId}

Eliminar item de la orden (recalcula totales).

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
{
  "ok": true
}
```

---

### PATCH /ordenes/{id}/items/{itemId}/estado

Cambiar estado de un item específico.

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "estado": "listo"
}
```

**Estados válidos:** pendiente, en_cocina, listo, entregado

**Response 200:**
```json
{
  "ok": true
}
```

---

## ?? COCINA

### GET /cocina/ordenes

Listar órdenes activas en cocina (solo pendiente y en_cocina).

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440400",
    "mesaId": "550e8400-e29b-41d4-a716-446655440110",
    "numeroMesa": 5,
    "tipoServicio": "mesa",
    "estado": "pendiente",
    "comensales": 4,
    "usuarioNombre": "Carlos",
    "meseroNombre": "Ana",
    "descuento": 0,
    "propina": 0,
    "subtotal": 520.00,
    "impuestos": 83.20,
    "total": 603.20,
    "creadoEn": "2024-02-23T12:30:00Z",
    "actualizadoEn": "2024-02-23T12:30:00Z",
    "notas": null,
    "items": [
      {
        "id": 123,
        "platilloId": "550e8400-e29b-41d4-a716-446655440010",
        "nombre": "Filete al Gusto",
        "precioUnitario": 185.00,
        "cantidad": 2,
        "notas": "Sin sal",
        "estado": "pendiente",
        "modificadores": [
          {
            "grupoNombre": "Término",
            "opcionNombre": "Bien cocido",
            "precioDelta": 0
          }
        ]
      }
    ]
  }
]
```

---

### PATCH /cocina/ordenes/{id}/iniciar

Marcar orden como "en_cocina".

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
{
  "ok": true
}
```

---

### PATCH /cocina/ordenes/{id}/listo

Marcar orden como "servido".

**Headers:** `Authorization: Bearer {token}`

**Response 200:**
```json
{
  "ok": true
}
```

---

## ?? PAGOS

### GET /pagos

Listar pagos con filtros.

**Headers:** `Authorization: Bearer {token}`

**Query Parameters:**
- `turno_id` (opcional): UUID del turno
- `desde` (opcional): Fecha ISO 8601
- `hasta` (opcional): Fecha ISO 8601
- `facturado` (opcional): `true` o `false`
- `limit` (opcional): Número máximo de resultados (default: 100)

**Ejemplo:** `GET /pagos?turno_id=550e8400-e29b-41d4-a716-446655440200&facturado=false`

**Response 200:**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440500",
    "ordenId": "550e8400-e29b-41d4-a716-446655440400",
    "turnoId": "550e8400-e29b-41d4-a716-446655440200",
    "meseroId": "550e8400-e29b-41d4-a716-446655440050",
    "meseroNombre": "Ana",
    "usuarioId": "550e8400-e29b-41d4-a716-446655440051",
    "usuarioNombre": "Carlos",
    "montoTotal": 603.20,
    "facturado": false,
    "registradoEn": "2024-02-23T13:00:00Z",
    "tenders": [
      {
        "metodo": "cash",
        "monto": 400.00,
        "referenciaLote": null,
        "referenciaTransf": null
      },
      {
        "metodo": "card",
        "monto": 203.20,
        "referenciaLote": "L001",
        "referenciaTransf": null
      }
    ]
  }
]
```

---

### GET /pagos/{id}

Obtener pago específico.

**Headers:** `Authorization: Bearer {token}`

**Response 200:** (mismo formato que lista)

---

### POST /pagos

Procesar pago de una orden.

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "ordenId": "550e8400-e29b-41d4-a716-446655440400",
  "turnoId": "550e8400-e29b-41d4-a716-446655440200",
  "meseroId": "550e8400-e29b-41d4-a716-446655440050",
  "tenders": [
    {
      "metodo": "cash",
      "monto": 400.00,
      "referenciaLote": null,
      "referenciaTransf": null
    },
    {
      "metodo": "card",
      "monto": 203.20,
      "referenciaLote": "L001",
      "referenciaTransf": null
    }
  ]
}
```

**Métodos válidos:** cash, card, transfer

**Response 201:**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440500",
  "ordenId": "550e8400-e29b-41d4-a716-446655440400",
  "turnoId": "550e8400-e29b-41d4-a716-446655440200",
  "meseroId": "550e8400-e29b-41d4-a716-446655440050",
  "meseroNombre": "Ana",
  "usuarioId": "550e8400-e29b-41d4-a716-446655440051",
  "usuarioNombre": "Carlos",
  "montoTotal": 603.20,
  "facturado": false,
  "registradoEn": "2024-02-23T13:00:00Z",
  "tenders": [
    {
      "metodo": "cash",
      "monto": 400.00,
      "referenciaLote": null,
      "referenciaTransf": null
    },
    {
      "metodo": "card",
      "monto": 203.20,
      "referenciaLote": "L001",
      "referenciaTransf": null
    }
  ]
}
```

**Nota:** Automáticamente:
- Marca orden como "pagado"
- Libera la mesa (activa = true)
- Descuenta inventario según recetas de platillos
- Actualiza totales del turno

**Response 400:**
```json
{
  "error": "El monto total de pagos es menor al total de la orden"
}
```

---

### PATCH /pagos/{id}/facturado

Marcar pago como facturado.

**Headers:** `Authorization: Bearer {token}`

**Request:**
```json
{
  "facturado": true
}
```

**Response 200:**
```json
{
  "ok": true
}
```

---

## ?? USUARIOS DE PRUEBA

| Username | PIN  | Rol    | Módulos |
|----------|------|--------|---------|
| admin    | 0000 | admin  | pos, admin, reports, inventory, billing, kitchen |
| mesero1  | 1234 | mesero | pos |
| cocina1  | 5555 | cocina | kitchen |

**Nota:** Ejecutar script SQL `scripts/01_crear_usuarios.sql` para crear estos usuarios.

---

## ? CÓDIGOS DE ERROR

| Código | Descripción |
|--------|-------------|
| 200 | OK - Operación exitosa |
| 201 | Created - Recurso creado exitosamente |
| 400 | Bad Request - Error de validación en los datos enviados |
| 401 | Unauthorized - Token inválido, expirado o no proporcionado |
| 404 | Not Found - Recurso no encontrado |
| 409 | Conflict - Conflicto con el estado actual (ej: turno ya abierto) |
| 500 | Internal Server Error - Error del servidor |

**Formato de error:**
```json
{
  "error": "Mensaje descriptivo del error"
}
```

---

## ?? NOTAS IMPORTANTES

### Token JWT
- Expira en **12 horas** desde su emisión
- Debe enviarse en cada petición (excepto `/auth/login`)
- Header: `Authorization: Bearer {token}`

### Fechas
- Todas las fechas en **UTC** formato **ISO 8601**
- Ejemplo: `2024-02-23T12:30:00Z`

### IDs
- Todos los IDs son **GUIDs/UUIDs** en formato string
- Ejemplo: `550e8400-e29b-41d4-a716-446655440000`

### PINs
- Se hashean con **SHA-256** antes de guardar en BD
- Nunca se devuelven en las respuestas

### Cálculo de Totales
```
Subtotal = suma((precio_unitario + suma(precio_delta_modificadores)) * cantidad)
Impuestos = (subtotal - descuento) * IVA_TASA
Total = (subtotal - descuento) + impuestos + propina
```

**IVA por default:** 16% (configurable en `Config_Impuestos`)

### Descuento de Inventario
Al procesar un pago con `POST /pagos`:
1. Lee las recetas de cada platillo en la orden
2. Multiplica la cantidad de receta por la cantidad de items
3. Crea movimientos tipo "salida" en `Insumos_Movimientos`
4. Actualiza `stock_actual` en tabla `Insumos`

---

## ?? FLUJO COMPLETO DE USO

### 1. Autenticación
```
POST /auth/login
Body: { "username": "admin", "pin": "0000" }
? Guardar token en localStorage o cookies
```

### 2. Abrir Turno
```
POST /turnos
Headers: Authorization: Bearer {token}
Body: { "efectivoInicial": 500.00 }
? Guardar turnoId para usar en órdenes
```

### 3. Ver Mesas Disponibles
```
GET /secciones
Headers: Authorization: Bearer {token}
? Mostrar layout de mesas
```

### 4. Ver Menú
```
GET /platillos?disponible=true
Headers: Authorization: Bearer {token}
? Mostrar categorías y platillos disponibles
```

### 5. Crear Orden
```
POST /ordenes
Headers: Authorization: Bearer {token}
Body: {
  "mesaId": "uuid",
  "turnoId": "uuid",
  "meseroId": "uuid",
  "items": [...]
}
? Orden creada, mesa ocupada, alerta a cocina
```

### 6. Cocina Ve Orden
```
GET /cocina/ordenes
Headers: Authorization: Bearer {token}
? Polling cada 20 segundos para nuevas órdenes
```

### 7. Cocina Marca Lista
```
PATCH /cocina/ordenes/{id}/listo
Headers: Authorization: Bearer {token}
? Notificar al mesero
```

### 8. Procesar Pago
```
POST /pagos
Headers: Authorization: Bearer {token}
Body: {
  "ordenId": "uuid",
  "turnoId": "uuid",
  "tenders": [...]
}
? Descuenta inventario, libera mesa, actualiza turno
```

### 9. Cerrar Turno
```
PATCH /turnos/{id}/cerrar
Headers: Authorization: Bearer {token}
Body: {
  "efectivoFinalReal": 1150.00,
  "notas": "..."
}
? Genera corte de caja automático
```

---

## ?? EJEMPLO DE INTEGRACIÓN EN FRONTEND

```typescript
// Configuración base
const API_BASE_URL = 'http://localhost:5006';
const getToken = () => localStorage.getItem('token');

// Helper para llamadas
async function apiCall(endpoint: string, options: RequestInit = {}) {
  const token = getToken();
  const headers = {
    'Content-Type': 'application/json',
    ...(token && { 'Authorization': `Bearer ${token}` }),
    ...options.headers
  };

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || 'Error en la petición');
  }

  return response.json();
}

// Ejemplo de uso
async function login(username: string, pin: string) {
  const data = await apiCall('/auth/login', {
    method: 'POST',
    body: JSON.stringify({ username, pin })
  });
  localStorage.setItem('token', data.token);
  return data.user;
}

async function getPlatillos() {
  return await apiCall('/platillos?disponible=true');
}

async function crearOrden(orden: any) {
  return await apiCall('/ordenes', {
    method: 'POST',
    body: JSON.stringify(orden)
  });
}

async function procesarPago(pago: any) {
  return await apiCall('/pagos', {
    method: 'POST',
    body: JSON.stringify(pago)
  });
}
```

---

## ?? SCRIPTS SQL DE INICIALIZACIÓN

### 1. Usuarios
```bash
sqlcmd -S localhost\SQLEXPRESS01 -d restSF -E -i scripts/01_crear_usuarios.sql
```

### 2. Configuración
```bash
sqlcmd -S localhost\SQLEXPRESS01 -d restSF -E -i scripts/02_configuracion_inicial.sql
```

### 3. Datos de Prueba
```bash
sqlcmd -S localhost\SQLEXPRESS01 -d restSF -E -i scripts/datos_prueba_menu.sql
```

---

## ?? SOPORTE

Para más información, revisa:
- `README.md` - Configuración inicial
- `PROGRESO.md` - Estado actual del proyecto
- Swagger UI: http://localhost:5006/swagger

---

**Última actualización:** Febrero 2024
**Versión API:** 1.0
**Backend:** .NET 8 Web API
**Base de datos:** SQL Server
