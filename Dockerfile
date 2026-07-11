# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restaurar primero solo los .csproj para aprovechar la cache de capas
COPY ["WebApi/WebApi.csproj", "WebApi/"]
COPY ["AccesoDatos/AccesoDatos.csproj", "AccesoDatos/"]
RUN dotnet restore "WebApi/WebApi.csproj"

# Copiar el resto y publicar
COPY . .
RUN dotnet publish "WebApi/WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Escucha HTTP interno; el TLS lo termina Caddy (proxy inverso) por fuera.
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebApi.dll"]
