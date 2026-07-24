-- Normaliza el catalogo de insumos para Tacos Michoacan.
-- Mantiene solo estos productos visibles:
-- Banano, Chocolate, Coca Cola, Crema, Desechable, Desechable Q6, Fresa,
-- Harina, Leche, Melon, Mora, Pastel, Power, Pura, Valle, Vidrio.
--
-- Seguridad:
-- - No borra historial.
-- - Une duplicados por sucursal y producto.
-- - Reasigna recetas, movimientos, cortes y modificadores al registro conservado.
-- - Desactiva lo que no esta en la lista para que desaparezca de la app.

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID('tempdb..#Canonical') IS NOT NULL DROP TABLE #Canonical;
CREATE TABLE #Canonical (
    Pattern nvarchar(120) COLLATE Latin1_General_CI_AI NOT NULL,
    CanonicalName nvarchar(120) NOT NULL
);

INSERT INTO #Canonical (Pattern, CanonicalName) VALUES
('banano', 'Banano'),
('chocolate', 'Chocolate'),
('coca cola', 'Coca Cola'),
('coca cola lata', 'Coca Cola'),
('coca cola (lata)', 'Coca Cola'),
('crema', 'Crema'),
('desechable q6', 'Desechable Q6'),
('desechable q.6', 'Desechable Q6'),
('desechable q 6', 'Desechable Q6'),
('desechable', 'Desechable'),
('fresa', 'Fresa'),
('harina', 'Harina'),
('leche', 'Leche'),
('melon', 'Melon'),
('mora', 'Mora'),
('pastel', 'Pastel'),
('power', 'Power'),
('pura', 'Pura'),
('valle', 'Valle'),
('vidrio', 'Vidrio');

IF OBJECT_ID('tempdb..#Matched') IS NOT NULL DROP TABLE #Matched;
SELECT
    i.id AS InsumoId,
    i.establecimiento_id AS EstablecimientoId,
    c.CanonicalName,
    ROW_NUMBER() OVER (
        PARTITION BY i.establecimiento_id, c.CanonicalName
        ORDER BY CASE WHEN i.stock_actual > 0 THEN 0 ELSE 1 END, i.creado_en, i.id
    ) AS rn
INTO #Matched
FROM dbo.Insumos i
CROSS APPLY (
    SELECT TOP 1 CanonicalName
    FROM #Canonical c
    WHERE LOWER(
        REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(i.nombre, '(', ''), ')', ''), '.', ''), ',', ''), '  ', ' ')
    ) COLLATE Latin1_General_CI_AI LIKE '%' + c.Pattern + '%'
    ORDER BY LEN(c.Pattern) DESC
) c;

IF OBJECT_ID('tempdb..#Keep') IS NOT NULL DROP TABLE #Keep;
SELECT InsumoId, EstablecimientoId, CanonicalName
INTO #Keep
FROM #Matched
WHERE rn = 1;

IF OBJECT_ID('tempdb..#Merge') IS NOT NULL DROP TABLE #Merge;
SELECT m.InsumoId AS SourceId, k.InsumoId AS TargetId
INTO #Merge
FROM #Matched m
JOIN #Keep k
  ON ((m.EstablecimientoId = k.EstablecimientoId) OR (m.EstablecimientoId IS NULL AND k.EstablecimientoId IS NULL))
 AND m.CanonicalName = k.CanonicalName
WHERE m.InsumoId <> k.InsumoId;

-- Consolidar existencias y valores utiles en el registro conservado.
UPDATE k
SET
    k.nombre = keepRows.CanonicalName,
    k.stock_actual = sums.StockActual,
    k.stock_minimo = CASE WHEN sums.StockMinimo > k.stock_minimo THEN sums.StockMinimo ELSE k.stock_minimo END,
    k.costo_por_unidad = CASE WHEN sums.CostoUnitario > 0 THEN sums.CostoUnitario ELSE k.costo_por_unidad END,
    k.activo = 1
FROM dbo.Insumos k
JOIN #Keep keepRows ON keepRows.InsumoId = k.id
CROSS APPLY (
    SELECT
        SUM(i.stock_actual) AS StockActual,
        MAX(i.stock_minimo) AS StockMinimo,
        MAX(NULLIF(i.costo_por_unidad, 0)) AS CostoUnitario
    FROM #Matched m
    JOIN dbo.Insumos i ON i.id = m.InsumoId
    WHERE ((m.EstablecimientoId = keepRows.EstablecimientoId) OR (m.EstablecimientoId IS NULL AND keepRows.EstablecimientoId IS NULL))
      AND m.CanonicalName = keepRows.CanonicalName
) sums;

-- Reasignar referencias de duplicados. Recetas usa llave compuesta
-- (platillo_id, insumo_id), asi que primero fusionamos cantidades.
UPDATE rt
SET rt.cantidad = rt.cantidad + rs.cantidad
FROM dbo.Recetas rs
JOIN #Merge m ON m.SourceId = rs.insumo_id
JOIN dbo.Recetas rt ON rt.platillo_id = rs.platillo_id AND rt.insumo_id = m.TargetId;

DELETE rs
FROM dbo.Recetas rs
JOIN #Merge m ON m.SourceId = rs.insumo_id
JOIN dbo.Recetas rt ON rt.platillo_id = rs.platillo_id AND rt.insumo_id = m.TargetId;

UPDATE r SET r.insumo_id = m.TargetId
FROM dbo.Recetas r
JOIN #Merge m ON m.SourceId = r.insumo_id;

UPDATE mov SET mov.insumo_id = m.TargetId
FROM dbo.Insumos_Movimientos mov
JOIN #Merge m ON m.SourceId = mov.insumo_id;

UPDATE det SET det.insumo_id = m.TargetId
FROM dbo.Corte_Inventario_Detalle det
JOIN #Merge m ON m.SourceId = det.insumo_id;

IF COL_LENGTH('dbo.ModificadorOpciones', 'InsumoId') IS NOT NULL
BEGIN
    UPDATE mo SET mo.InsumoId = m.TargetId
    FROM dbo.ModificadorOpciones mo
    JOIN #Merge m ON m.SourceId = mo.InsumoId;
END;

-- Borrar duplicados ya sin referencias directas; si alguno queda bloqueado,
-- lo dejamos inactivo para no arriesgar historial.
DELETE i
FROM dbo.Insumos i
JOIN #Merge m ON m.SourceId = i.id
WHERE NOT EXISTS (SELECT 1 FROM dbo.Recetas r WHERE r.insumo_id = i.id)
  AND NOT EXISTS (SELECT 1 FROM dbo.Insumos_Movimientos mov WHERE mov.insumo_id = i.id)
  AND NOT EXISTS (SELECT 1 FROM dbo.Corte_Inventario_Detalle det WHERE det.insumo_id = i.id)
  AND (COL_LENGTH('dbo.ModificadorOpciones', 'InsumoId') IS NULL OR NOT EXISTS (SELECT 1 FROM dbo.ModificadorOpciones mo WHERE mo.InsumoId = i.id));

UPDATE i
SET activo = 0
FROM dbo.Insumos i
LEFT JOIN #Matched m ON m.InsumoId = i.id
WHERE m.InsumoId IS NULL
   OR EXISTS (SELECT 1 FROM #Merge mm WHERE mm.SourceId = i.id);

COMMIT TRANSACTION;

SELECT nombre, establecimiento_id, unidad, stock_actual, stock_minimo, activo
FROM dbo.Insumos
WHERE activo = 1
ORDER BY nombre, establecimiento_id;
