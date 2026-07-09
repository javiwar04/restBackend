using AccesoDatos.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// El secreto JWT vive en user-secrets (dev) o variables de entorno (prod).
// Por defecto .NET solo carga user-secrets en el entorno Development; lo
// cargamos en cualquier entorno para que la app arranque igual desde VS,
// `dotnet run` o el DLL publicado. Reañadimos las variables de entorno al
// final para que en producción (Jwt__Secret) sigan teniendo prioridad.
builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);
builder.Configuration.AddEnvironmentVariables();

// DbContext
builder.Services.AddDbContext<RestauranteDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<HashService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<MesasService>();
builder.Services.AddScoped<TurnosService>();
builder.Services.AddScoped<OrdenesService>();
builder.Services.AddScoped<CocinaService>();
builder.Services.AddScoped<PagosService>();
builder.Services.AddScoped<UsuariosService>();
builder.Services.AddScoped<ConfigService>();
builder.Services.AddScoped<InsumosService>();
builder.Services.AddScoped<RecetasService>();
builder.Services.AddScoped<ReportesService>();
builder.Services.AddScoped<FacturasService>();
builder.Services.AddScoped<AuditoriaService>();
builder.Services.AddScoped<EstablecimientosService>();
builder.Services.AddSingleton<RealtimeNotifier>();

// SignalR — eventos en tiempo real para POS y cocina
builder.Services.AddSignalR();

// JWT Authentication.
// En producción el secreto se pone con la variable de entorno Jwt__Secret
// (o Jwt:Secret en config). Si no hay ninguno, usamos una clave de
// desarrollo para que la app SIEMPRE arranque en local — no bloquea nada.
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
{
    jwtSecret = "restsf-clave-de-desarrollo-local-cambiar-en-produccion-2026";
    Console.WriteLine("[WARN] Jwt:Secret no configurado; usando clave de desarrollo. En producción define la variable de entorno Jwt__Secret.");
}
// Escribir el secreto resuelto de vuelta en la config para que JwtService
// (que lo lee de IConfiguration al generar/validar) use exactamente el mismo
// valor. Una sola fuente de verdad, sin importar entorno ni user-secrets.
builder.Configuration["Jwt:Secret"] = jwtSecret;
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// Rate limiting: frena fuerza bruta contra el login (PIN de 4 digitos).
// Maximo 10 intentos por IP cada 5 minutos; el resto recibe 429.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0
            }));
});

// CORS para Next.js
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresa el token JWT en el formato: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<WebApi.Hubs.RestauranteHub>("/hubs/restaurante");

app.Run();
