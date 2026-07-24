-- Prepara ordenes/pagos para cliente en cuenta y correlativo de ticket por sucursal.
-- - Ordenes.cliente_nombre: default "Consumidor Final".
-- - Pagos.ticket_numero/ticket_correlativo: numero bonito y estable para tickets.
-- - Pagos.establecimiento_id: copia la sucursal de la orden para numerar por sucursal.
--
-- Nota tecnica:
-- SQL Server compila cada batch antes de ejecutar ALTER TABLE. Por eso las
-- columnas se agregan en batches separados con GO antes de referenciarlas.

SET XACT_ABORT ON;

IF COL_LENGTH('dbo.Ordenes', 'cliente_nombre') IS NULL
BEGIN
    ALTER TABLE dbo.Ordenes
      ADD cliente_nombre nvarchar(160) NOT NULL
          CONSTRAINT DF_Ordenes_cliente_nombre DEFAULT N'Consumidor Final';
END;
GO

IF COL_LENGTH('dbo.Pagos', 'establecimiento_id') IS NULL
BEGIN
    ALTER TABLE dbo.Pagos ADD establecimiento_id nvarchar(36) NULL;
END;
GO

IF COL_LENGTH('dbo.Pagos', 'ticket_numero') IS NULL
BEGIN
    ALTER TABLE dbo.Pagos ADD ticket_numero int NULL;
END;
GO

IF COL_LENGTH('dbo.Pagos', 'ticket_correlativo') IS NULL
BEGIN
    ALTER TABLE dbo.Pagos ADD ticket_correlativo nvarchar(40) NULL;
END;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

UPDATE p
SET p.establecimiento_id = o.establecimiento_id
FROM dbo.Pagos p
JOIN dbo.Ordenes o ON o.id = p.orden_id
WHERE p.establecimiento_id IS NULL;

;WITH numbered AS (
    SELECT
        p.id,
        ROW_NUMBER() OVER (
            PARTITION BY ISNULL(p.establecimiento_id, 'SIN-SUCURSAL')
            ORDER BY p.registrado_en, p.id
        ) AS rn
    FROM dbo.Pagos p
    WHERE p.ticket_numero IS NULL
)
UPDATE p
SET p.ticket_numero = numbered.rn
FROM dbo.Pagos p
JOIN numbered ON numbered.id = p.id;

UPDATE p
SET p.ticket_correlativo =
    UPPER(LEFT(REPLACE(REPLACE(REPLACE(ISNULL(e.nombre, 'TCK'), ' ', ''), '-', ''), '.', '') + 'XXX', 3))
    + '-' + RIGHT('000000' + CONVERT(varchar(20), p.ticket_numero), 6)
FROM dbo.Pagos p
LEFT JOIN dbo.Establecimientos e ON e.id = p.establecimiento_id
WHERE p.ticket_correlativo IS NULL
  AND p.ticket_numero IS NOT NULL;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_Pagos_Establecimiento_TicketNumero'
      AND object_id = OBJECT_ID('dbo.Pagos')
)
BEGIN
    CREATE UNIQUE INDEX UX_Pagos_Establecimiento_TicketNumero
      ON dbo.Pagos(establecimiento_id, ticket_numero)
      WHERE establecimiento_id IS NOT NULL AND ticket_numero IS NOT NULL;
END;

COMMIT TRANSACTION;
GO

SELECT TOP 50
    p.ticket_correlativo,
    p.ticket_numero,
    e.nombre AS sucursal,
    o.cliente_nombre,
    p.registrado_en
FROM dbo.Pagos p
JOIN dbo.Ordenes o ON o.id = p.orden_id
LEFT JOIN dbo.Establecimientos e ON e.id = p.establecimiento_id
ORDER BY p.registrado_en DESC;
GO
