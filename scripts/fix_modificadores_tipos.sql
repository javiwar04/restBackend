-- ===============================================
-- FIX: Cambiar ModificadorGrupos y ModificadorOpciones a NVARCHAR(36)
-- Fecha: Febrero 2024
-- Problema: InvalidCastException porque SQL tiene UNIQUEIDENTIFIER pero C# espera string
-- ===============================================

USE RestauranteSF;
GO

-- 1. Eliminar tablas existentes (si existen)
IF OBJECT_ID('ModificadorOpciones', 'U') IS NOT NULL
    DROP TABLE ModificadorOpciones;

IF OBJECT_ID('ModificadorGrupos', 'U') IS NOT NULL
    DROP TABLE ModificadorGrupos;
GO

-- 2. Crear ModificadorGrupos con NVARCHAR(36)
CREATE TABLE ModificadorGrupos (
    Id NVARCHAR(36) PRIMARY KEY,
    PlatilloId NVARCHAR(36) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Tipo NVARCHAR(20) NOT NULL DEFAULT 'single',
    Obligatorio BIT NOT NULL DEFAULT 0,
    MinSelecciones INT NOT NULL DEFAULT 0,
    MaxSelecciones INT NOT NULL DEFAULT 0,
    Orden INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_ModificadorGrupos_Platillo
        FOREIGN KEY (PlatilloId) REFERENCES Platillos(Id) ON DELETE CASCADE
);
GO

-- 3. Crear ModificadorOpciones con NVARCHAR(36)
CREATE TABLE ModificadorOpciones (
    Id NVARCHAR(36) PRIMARY KEY,
    GrupoId NVARCHAR(36) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    PrecioDelta DECIMAL(10,2) NOT NULL DEFAULT 0,
    EsDefault BIT NOT NULL DEFAULT 0,
    Activo BIT NOT NULL DEFAULT 1,
    Orden INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_ModificadorOpciones_Grupo
        FOREIGN KEY (GrupoId) REFERENCES ModificadorGrupos(Id) ON DELETE CASCADE
);
GO

-- 4. Crear índices para mejorar performance
CREATE INDEX IX_ModificadorGrupos_PlatilloId ON ModificadorGrupos(PlatilloId);
CREATE INDEX IX_ModificadorOpciones_GrupoId ON ModificadorOpciones(GrupoId);
GO

PRINT '? Tablas recreadas correctamente con NVARCHAR(36)';
PRINT '? Ahora coinciden con los tipos de las entidades C#';
GO
