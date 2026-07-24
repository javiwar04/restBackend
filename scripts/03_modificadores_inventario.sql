-- Permite que una opcion de modificador rebaje un insumo del inventario.
-- Ejecutar una vez en la base SQL Server desplegada.

IF COL_LENGTH('dbo.ModificadorOpciones', 'InsumoId') IS NULL
BEGIN
    ALTER TABLE dbo.ModificadorOpciones ADD InsumoId nvarchar(36) NULL;
END;

IF COL_LENGTH('dbo.ModificadorOpciones', 'CantidadInsumo') IS NULL
BEGIN
    ALTER TABLE dbo.ModificadorOpciones ADD CantidadInsumo decimal(12,4) NULL;
END;

IF COL_LENGTH('dbo.Orden_Item_Modificadores', 'opcion_id') IS NULL
BEGIN
    ALTER TABLE dbo.Orden_Item_Modificadores ADD opcion_id nvarchar(36) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ModificadorOpciones_Insumo')
BEGIN
    ALTER TABLE dbo.ModificadorOpciones
    ADD CONSTRAINT FK_ModificadorOpciones_Insumo
        FOREIGN KEY (InsumoId) REFERENCES dbo.Insumos(id)
        ON DELETE SET NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ModificadorOpciones_InsumoId' AND object_id = OBJECT_ID('dbo.ModificadorOpciones'))
BEGIN
    CREATE INDEX IX_ModificadorOpciones_InsumoId ON dbo.ModificadorOpciones(InsumoId);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrdenItemMod_OpcionId' AND object_id = OBJECT_ID('dbo.Orden_Item_Modificadores'))
BEGIN
    CREATE INDEX IX_OrdenItemMod_OpcionId ON dbo.Orden_Item_Modificadores(opcion_id);
END;
