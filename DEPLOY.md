# Deploy a producción — Tacos Michoacán

Arquitectura:

```
   Tablets / navegadores
           │  https
           ▼
   rest.warforgegt.com  ──►  Vercel (frontend Next.js)
           │  https (API + WebSocket)
           ▼
   api.warforgegt.com   ──►  Caddy (HTTPS)  ──►  backend .NET (:8080)  ──►  SQL Server (Docker)
                                         VPS Contabo
```

- **Frontend** → Vercel, dominio `rest.warforgegt.com`.
- **Backend + BD** → VPS Contabo, dominio `api.warforgegt.com`, todo en Docker.

---

## 1. DNS (en tu proveedor de `warforgegt.com`)

| Registro | Tipo  | Valor |
|----------|-------|-------|
| `rest`   | CNAME | el que te dé Vercel (`cname.vercel-dns.com`) |
| `api`    | A     | la IP pública de tu VPS Contabo |

> El backend necesita su propio subdominio (`api.…`) porque Vercel sirve por
> HTTPS y el navegador **bloquea** llamadas a `http://` desde una página `https://`.

---

## 2. Backend en el VPS (Contabo)

Requisitos en el VPS: Docker + Docker Compose.

```bash
# 1. Clona el repo (o sube la carpeta) al VPS
git clone <tu-repo> && cd Restaurante/deploy

# 2. Configura los secretos
cp .env.example .env
nano .env            # rellena SA_PASSWORD, JWT_SECRET, FRONTEND_ORIGIN, API_DOMAIN

# Genera un JWT_SECRET fuerte:
openssl rand -base64 48

# 3. Levanta todo (SQL Server + backend + Caddy con HTTPS automático)
docker compose up -d --build
```

Caddy saca el certificado de Let's Encrypt solo (necesita que `api.warforgegt.com`
ya apunte al VPS y los puertos 80/443 abiertos en el firewall de Contabo).

### Esquema de la base de datos

El proyecto es *DB-first* (sin migraciones EF). La BD `restSF` arranca vacía, así que
hay que cargarle el esquema **una vez**:

- **Opción A (recomendada):** restaura un backup `.bak` de tu BD de desarrollo:
  ```bash
  docker cp restSF_dev.bak sqlserver:/var/opt/mssql/backups/
  docker exec sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C \
    -Q "RESTORE DATABASE [restSF] FROM DISK='/var/opt/mssql/backups/restSF_dev.bak' WITH MOVE ... , REPLACE"
  ```
- **Opción B:** corre tus scripts de creación de tablas + los seeds (insumos, usuarios, establecimientos).

Verifica que responde:  `curl https://api.warforgegt.com/reportes/ventas` (debe pedir auth, no fallar de red).

---

## 3. Frontend en Vercel

1. **New Project** → importa el repo.
2. **Root Directory:** `frontRest`.
3. **Environment Variables:**
   | Nombre | Valor |
   |--------|-------|
   | `NEXT_PUBLIC_API_BASE` | `https://api.warforgegt.com` |
4. **Domains:** agrega `rest.warforgegt.com`.
5. Deploy.

> Al cambiar `NEXT_PUBLIC_API_BASE` hay que **volver a desplegar** (es una variable
> de build, se hornea en el bundle).

---

## 4. Backups automáticos

Script incluido: `deploy/backup-db.sh` (backup diario con rotación de 14 días).

```bash
chmod +x deploy/backup-db.sh
mkdir -p /home/backups
crontab -e
# 2:00 AM cada día:
0 2 * * * SA_PASSWORD='tu-clave' /ruta/deploy/backup-db.sh >> /home/backups/backup.log 2>&1
```

> Recomendado: copiar `/home/backups` a un almacenamiento externo (otro disco,
> S3, Backblaze…) para no perderlo si el VPS falla.

---

## 5. Checklist de seguridad (antes de abrir al público)

- [ ] `JWT_SECRET` real y aleatorio en `.env` (NO el de desarrollo).
- [ ] `SA_PASSWORD` fuerte; la BD sin puerto público (ya está así en el compose).
- [ ] `.env` fuera de git (revisa `.gitignore`).
- [ ] Firewall de Contabo: abre solo 80, 443 y SSH; cierra el resto.
- [ ] HTTPS funcionando en ambos dominios (candado en el navegador).
- [ ] Backup probado: corre `backup-db.sh` a mano una vez y verifica el `.bak`.
- [ ] Cambia los PIN por defecto de los usuarios.

---

## Variables de entorno (referencia)

**Backend** (las lee de forma nativa, formato `Sección__Clave`):

| Variable | Para qué |
|----------|----------|
| `ConnectionStrings__DefaultConnection` | Conexión a SQL Server |
| `Jwt__Secret` | Firma de los tokens (≥32 chars) |
| `Cors__AllowedOrigins__0` | Origen permitido (el dominio de Vercel) |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

**Frontend** (Vercel):

| Variable | Para qué |
|----------|----------|
| `NEXT_PUBLIC_API_BASE` | URL pública del backend (`https://api.warforgegt.com`) |
