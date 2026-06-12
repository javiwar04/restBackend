# ??? RestBackend - API del Sistema de Restaurante

Backend en **.NET 8 Web API** con autenticaci�n JWT para el sistema de gesti�n de restaurante.

## ?? Stack Tecnol�gico

- .NET 8 Web API
- Entity Framework Core 8
- SQL Server (LocalDB o Express)
- JWT Bearer Authentication
- SHA-256 para hash de PINs
- Swagger/OpenAPI

## ?? Configuraci�n Inicial

### 1. Restaurar paquetes
```bash
dotnet restore
```

### 2. Configurar Base de Datos

La cadena de conexi�n est� en `WebApi/appsettings.json`:

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

| Username | PIN  | Rol          | M�dulos                                           |
|----------|------|--------------|---------------------------------------------------|
| admin    | 0000 | Administrador| pos, admin, reports, inventory, billing, kitchen  |
| mesero1  | 1234 | Mesero       | pos                                               |

### 4. Ejecutar la API

```bash
cd WebApi
dotnet run
```

La API estar� disponible en:
- **HTTP:** http://localhost:5000
- **HTTPS:** https://localhost:5001
- **Swagger:** http://localhost:5000/swagger

## ?? Autenticaci�n

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

### Verificar sesi�n

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

### ?? Autenticaci�n
- `POST /auth/login` - Iniciar sesi�n
- `POST /auth/logout` - Cerrar sesi�n
- `GET /auth/me` - Obtener usuario actual

### ?? Categor�as del Men�
- `GET /categorias-menu` - Listar categor�as
- `GET /categorias-menu/{id}` - Obtener categor�a
- `POST /categorias-menu` - Crear categor�a
- `PUT /categorias-menu/{id}` - Actualizar categor�a
- `DELETE /categorias-menu/{id}` - Eliminar categor�a

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
?   ?   ??? AuthController.cs          # Autenticaci�n
?   ?   ??? CategoriasMenuController.cs
?   ?
?   ??? Services/
?   ?   ??? AuthService.cs             # L�gica de autenticaci�n
?   ?   ??? HashService.cs             # SHA-256 para PINs
?   ?   ??? JwtService.cs              # Generaci�n de tokens
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
?   ??? Program.cs                      # Configuraci�n principal
?   ??? appsettings.json                # Configuraci�n
?
??? scripts/
    ??? crear_usuarios_prueba.sql       # Script inicial
```

## ?? Configuraci�n JWT

El secreto JWT NO vive en el codigo. `appsettings.json` solo tiene Issuer/Audience:

```json
{
  "Jwt": {
    "Issuer": "RestauranteAPI",
    "Audience": "RestauranteFrontend"
  }
}
```

El secreto se configura fuera del repositorio (minimo 32 caracteres; la API no arranca sin el):

- **Desarrollo:** `dotnet user-secrets set "Jwt:Secret" "<valor-aleatorio>"` (en la carpeta WebApi)
- **Produccion:** variable de entorno `Jwt__Secret`

## ?? CORS

Configurado para aceptar peticiones desde:
- `http://localhost:3000` (Next.js frontend)

Modificar en `Program.cs` si es necesario.

## ?? Pr�ximos Endpoints a Implementar

Seg�n los requerimientos del documento:

- [ ] **Usuarios** (`/usuarios`)
- [ ] **Secciones y Mesas** (`/secciones`, `/mesas`)
- [ ] **Platillos** (`/platillos`)
- [ ] **Modificadores** (`/modificadores`)
- [ ] **Inventario** (`/insumos`, `/recetas`)
- [ ] **Turnos** (`/turnos`)
- [ ] **�rdenes** (`/ordenes`)
- [ ] **Cocina** (`/cocina/ordenes`)
- [ ] **Pagos** (`/pagos`)
- [ ] **Facturaci�n** (`/facturas`)
- [ ] **Reportes** (`/reportes`)
- [ ] **Configuraci�n** (`/config`)

## ?? Testing con Swagger

1. Ejecuta la API: `dotnet run`
2. Abre: http://localhost:5000/swagger
3. Prueba el endpoint `/auth/login`
4. Copia el token recibido
5. Click en "Authorize" (candado verde)
6. Pega el token: `Bearer <tu-token>`
7. Ahora puedes probar endpoints protegidos

## ?? Troubleshooting

### Error de conexi�n a SQL Server

Verifica que SQL Server est� corriendo:
```bash
# Ver servicios SQL Server
services.msc
```

### Error de autenticaci�n JWT

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

Para m�s informaci�n, revisa el documento de requerimientos completo en la ra�z del proyecto.

---

**Desarrollado con .NET 8 ??**
