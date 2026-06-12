# ??? RestBackend - API del Sistema de Restaurante

Backend en **.NET 8 Web API** con autenticación JWT para el sistema de gestión de restaurante.

## ?? Stack Tecnológico

- .NET 8 Web API
- Entity Framework Core 8
- SQL Server (LocalDB o Express)
- JWT Bearer Authentication
- SHA-256 para hash de PINs
- Swagger/OpenAPI

## ?? Configuración Inicial

### 1. Restaurar paquetes
```bash
dotnet restore
```

### 2. Configurar Base de Datos

La cadena de conexión está en `WebApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS01;Database=restSF;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### 3. Crear usuarios de prueba

Ejecuta el script SQL en tu base de datos:

```bash
sqlcmd -S localhost\SQLEXPRESS01 -d restSF -i scripts\crear_usuarios_prueba.sql
```

O manualmente en SQL Server Management Studio.

**Usuarios creados:**

| Username | PIN  | Rol          | Módulos                                           |
|----------|------|--------------|---------------------------------------------------|
| admin    | 0000 | Administrador| pos, admin, reports, inventory, billing, kitchen  |
| mesero1  | 1234 | Mesero       | pos                                               |

### 4. Ejecutar la API

```bash
cd WebApi
dotnet run
```

La API estará disponible en:
- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001
- **Swagger:** http://localhost:5000/swagger

## ?? Autenticación

### Login

**POST** `/auth/login`

```json
{
  "username": "admin",
  "pin": "0000"
}
```

**Respuesta exitosa (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "...",
    "nombre": "Administrador",
    "username": "admin",
    "rol": "admin",
    "modules": ["pos", "admin", "reports", "inventory", "billing", "kitchen"]
  }
}
```

### Uso del Token

Incluir en todas las peticiones (excepto `/auth/login`):

```
Authorization: Bearer <token>
```

### Verificar sesión

**GET** `/auth/me`

```
Authorization: Bearer <token>
```

### Logout

**POST** `/auth/logout`

```
Authorization: Bearer <token>
```

## ?? Endpoints Disponibles

### ?? Autenticación
- `POST /auth/login` - Iniciar sesión
- `POST /auth/logout` - Cerrar sesión
- `GET /auth/me` - Obtener usuario actual

### ?? Categorías del Menú
- `GET /categorias-menu` - Listar categorías
- `GET /categorias-menu/{id}` - Obtener categoría
- `POST /categorias-menu` - Crear categoría
- `PUT /categorias-menu/{id}` - Actualizar categoría
- `DELETE /categorias-menu/{id}` - Eliminar categoría

## ??? Estructura del Proyecto

```
RestBackend/
??? AccesoDatos/
?   ??? Context/
?   ?   ??? RestauranteDbContext.cs    # DbContext principal
?   ??? Models/                         # Entidades generadas
?       ??? Usuario.cs
?       ??? CategoriasMenu.cs
?       ??? Platillo.cs
?       ??? ... (29 tablas + 4 vistas)
?
??? WebApi/
?   ??? Controllers/
?   ?   ??? AuthController.cs          # Autenticación
?   ?   ??? CategoriasMenuController.cs
?   ?
?   ??? Services/
?   ?   ??? AuthService.cs             # Lógica de autenticación
?   ?   ??? HashService.cs             # SHA-256 para PINs
?   ?   ??? JwtService.cs              # Generación de tokens
?   ?   ??? MenuService.cs
?   ?
?   ??? DTOs/
?   ?   ??? Auth/
?   ?   ?   ??? LoginRequest.cs
?   ?   ?   ??? LoginResponse.cs
?   ?   ??? Menu/
?   ?   ?   ??? CategoriaMenuDto.cs
?   ?   ??? ErrorResponse.cs
?   ?
?   ??? Program.cs                      # Configuración principal
?   ??? appsettings.json                # Configuración
?
??? scripts/
    ??? crear_usuarios_prueba.sql       # Script inicial
```

## ?? Configuración JWT

En `appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "tu-super-secreto-key-muy-segura-de-al-menos-32-caracteres-2026",
    "Issuer": "RestauranteAPI",
    "Audience": "RestauranteFrontend"
  }
}
```

**?? IMPORTANTE:** Cambia el `Secret` en producción por una clave segura.

## ?? CORS

Configurado para aceptar peticiones desde:
- `http://localhost:3000` (Next.js frontend)

Modificar en `Program.cs` si es necesario.

## ?? Próximos Endpoints a Implementar

Según los requerimientos del documento:

- [ ] **Usuarios** (`/usuarios`)
- [ ] **Secciones y Mesas** (`/secciones`, `/mesas`)
- [ ] **Platillos** (`/platillos`)
- [ ] **Modificadores** (`/modificadores`)
- [ ] **Inventario** (`/insumos`, `/recetas`)
- [ ] **Turnos** (`/turnos`)
- [ ] **Órdenes** (`/ordenes`)
- [ ] **Cocina** (`/cocina/ordenes`)
- [ ] **Pagos** (`/pagos`)
- [ ] **Facturación** (`/facturas`)
- [ ] **Reportes** (`/reportes`)
- [ ] **Configuración** (`/config`)

## ?? Testing con Swagger

1. Ejecuta la API: `dotnet run`
2. Abre: http://localhost:5000/swagger
3. Prueba el endpoint `/auth/login`
4. Copia el token recibido
5. Click en "Authorize" (candado verde)
6. Pega el token: `Bearer <tu-token>`
7. Ahora puedes probar endpoints protegidos

## ?? Troubleshooting

### Error de conexión a SQL Server

Verifica que SQL Server esté corriendo:
```bash
# Ver servicios SQL Server
services.msc
```

### Error de autenticación JWT

- Verifica que el token no haya expirado (12 horas)
- Incluye el prefijo `Bearer ` antes del token
- Verifica que el `Secret` en appsettings.json sea el mismo

### Error al compilar

```bash
dotnet clean
dotnet restore
dotnet build
```

## ?? Soporte

Para más información, revisa el documento de requerimientos completo en la raíz del proyecto.

---

**Desarrollado con .NET 8 ??**
