-- ============================================
-- CREAR USUARIOS - INSERTS DIRECTOS
-- ============================================

-- ROLES
INSERT INTO Roles (id, nombre) VALUES 
('admin', 'Administrador'),
('supervisor', 'Supervisor'),
('mesero', 'Mesero'),
('cocina', 'Cocina'),
('caja', 'Caja');

-- MÓDULOS
INSERT INTO Modulos (id, nombre) VALUES 
('pos', 'Punto de Venta'),
('admin', 'Administración'),
('reports', 'Reportes'),
('inventory', 'Inventario'),
('billing', 'Facturación'),
('kitchen', 'Cocina');

-- USUARIOS (PIN hasheado con SHA-256)
DECLARE @AdminId NVARCHAR(36) = NEWID();
DECLARE @MeseroId NVARCHAR(36) = NEWID();
DECLARE @CocinaId NVARCHAR(36) = NEWID();

INSERT INTO Usuarios (id, nombre, username, pin_hash, rol_id, activo) VALUES
(@AdminId, 'Administrador', 'admin', '93b885adfe0da089cdf634904fd59f71', 'admin', 1),
(@MeseroId, 'Juan Mesero', 'mesero1', '03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4', 'mesero', 1),
(@CocinaId, 'Chef Principal', 'cocina1', 'ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f', 'cocina', 1);

-- ASIGNAR MÓDULOS
INSERT INTO Usuarios_Modulos (usuario_id, modulo_id)
SELECT @AdminId, id FROM Modulos;

INSERT INTO Usuarios_Modulos (usuario_id, modulo_id) VALUES
(@MeseroId, 'pos'),
(@CocinaId, 'kitchen');

PRINT '? Usuarios creados:';
PRINT 'admin   | PIN: 0000 | Rol: admin';
PRINT 'mesero1 | PIN: 1234 | Rol: mesero';
PRINT 'cocina1 | PIN: 5555 | Rol: cocina';
