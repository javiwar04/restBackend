#!/usr/bin/env bash
# Backup diario de la BD restSF (SQL Server en Docker) con rotación de 14 días.
#
# Instalación (en el VPS):
#   1) chmod +x backup-db.sh
#   2) mkdir -p /home/backups
#   3) Programar en cron (2:00 AM cada día):
#        crontab -e
#        0 2 * * * SA_PASSWORD='tu-clave' /ruta/al/deploy/backup-db.sh >> /home/backups/backup.log 2>&1
#
# Requiere que SA_PASSWORD esté en el entorno (lo pasa el cron de arriba).
set -euo pipefail

CONTAINER="${DB_CONTAINER:-sqlserver}"     # nombre del contenedor de SQL Server
DB="${DB_NAME:-restSF}"
OUT_DIR="${BACKUP_DIR:-/home/backups}"
KEEP_DAYS="${KEEP_DAYS:-14}"
STAMP="$(date +%F_%H%M)"
BAK="${DB}_${STAMP}.bak"

# sqlcmd cambió de ruta según la versión de la imagen; probar ambas.
SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
docker exec "$CONTAINER" test -x "$SQLCMD" || SQLCMD="/opt/mssql-tools/bin/sqlcmd"

mkdir -p "$OUT_DIR"
docker exec "$CONTAINER" mkdir -p /var/opt/mssql/backups

# Backup dentro del contenedor (-C = confiar en el certificado autofirmado)
docker exec "$CONTAINER" "$SQLCMD" -S localhost -U sa -P "$SA_PASSWORD" -C \
  -Q "BACKUP DATABASE [$DB] TO DISK='/var/opt/mssql/backups/$BAK' WITH INIT, COMPRESSION, STATS=10"

# Sacar el archivo fuera del contenedor (al disco del host)
docker cp "$CONTAINER:/var/opt/mssql/backups/$BAK" "$OUT_DIR/$BAK"

# Rotación: borrar backups locales más viejos que KEEP_DAYS
find "$OUT_DIR" -name "${DB}_*.bak" -mtime "+$KEEP_DAYS" -delete

echo "[$(date '+%F %T')] Backup OK: $OUT_DIR/$BAK"
