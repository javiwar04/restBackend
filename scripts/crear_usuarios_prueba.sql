-- Script para crear usuario administrador de prueba
-- PIN: 0000
-- Hash SHA-256 de "0000": 93b885adfe0da089cdf634904fd59f71

-- Primero verificar que existan los roles y módulos
IF NOT EXISTS (SELECT 1 FROM Roles WHERE id = 'admin')
BEGIN
    INSERT INTO Roles (id, nombre) VALUES ('admin', 'Administrador');
END

IF NOT EXISTS (SELECT 1 FROM Modulos WHERE id = 'pos')
BEGIN
    INSERT INTO Modulos (id, nombre) VALUES 
        ('pos', 'Punto de Venta'),
        ('admin', 'Administración'),
        ('reports', 'Reportes'),
        ('inventory', 'Inventario'),
        ('billing', 'Facturación'),
        ('kitchen', 'Cocina');
END

-- Crear usuario admin si no existe
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE username = 'admin')
BEGIN
    DECLARE @AdminId NVARCHAR(36) = NEWID();
    
    INSERT INTO Usuarios (id, nombre, username, pin_hash, rol_id, activo)
    VALUES (@AdminId, 'Administrador', 'admin', '93b885adfe0da089cdf634904fd59f71', 'admin', 1);
    
    -- Asignar todos los módulos
    INSERT INTO Usuarios_Modulos (usuario_id, modulo_id)
    SELECT @AdminId, id FROM Modulos;
    
    PRINT 'Usuario admin creado exitosamente';
    PRINT 'Username: admin';
    PRINT 'PIN: 0000';
END
ELSE
BEGIN
    PRINT 'El usuario admin ya existe';
END

-- Crear usuario mesero de prueba
IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE username = 'mesero1')
BEGIN
    DECLARE @MeseroId NVARCHAR(36) = NEWID();
    
    -- Crear rol mesero si no existe
    IF NOT EXISTS (SELECT 1 FROM Roles WHERE id = 'mesero')
    BEGIN
        INSERT INTO Roles (id, nombre) VALUES ('mesero', 'Mesero');
    END
    
    -- PIN: 1234, Hash: 03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4
    INSERT INTO Usuarios (id, nombre, username, pin_hash, rol_id, activo)
    VALUES (@MeseroId, 'Juan Mesero', 'mesero1', '03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4', 'mesero', 1);
    
    -- Asignar módulo POS
    INSERT INTO Usuarios_Modulos (usuario_id, modulo_id)
    VALUES (@MeseroId, 'pos');
    
    PRINT 'Usuario mesero1 creado exitosamente';
    PRINT 'Username: mesero1';
    PRINT 'PIN: 1234';
END
ELSE
BEGIN
    PRINT 'El usuario mesero1 ya existe';
END
