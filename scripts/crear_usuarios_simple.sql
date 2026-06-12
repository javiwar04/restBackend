-- ============================================
-- SCRIPT SIMPLE - CREAR USUARIOS DE PRUEBA
-- ============================================
-- NO ejecutar si ya tienes usuarios creados

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

-- USUARIOS (con PIN hasheado en SHA-256)

-- Usuario: admin | PIN: 0000
INSERT INTO Usuarios (id, nombre, username, pin_hash, rol_id, activo)
VALUES (NEWID(), 'Administrador', 'admin', '93b885adfe0da089cdf634904fd59f71', 'admin', 1);

-- Usuario: mesero1 | PIN: 1234
INSERT INTO Usuarios (id, nombre, username, pin_hash, rol_id, activo)
VALUES (NEWID(), 'Juan Mesero', 'mesero1', '03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4', 'mesero', 1);

-- Usuario: cocina1 | PIN: 5555
INSERT INTO Usuarios (id, nombre, username, pin_hash, rol_id, activo)
VALUES (NEWID(), 'Chef Principal', 'cocina1', 'ef797c8118f02dfb649607dd5d3f8c7623048c9c063d532cc95c5ed7a898a64f', 'cocina', 1);

-- ASIGNAR MÓDULOS

-- Admin = TODOS los módulos
INSERT INTO Usuarios_Modulos (usuario_id, modulo_id)
SELECT u.id, m.id 
FROM Usuarios u
CROSS JOIN Modulos m
WHERE u.username = 'admin';

-- Mesero = solo POS
INSERT INTO Usuarios_Modulos (usuario_id, modulo_id)
SELECT id, 'pos' FROM Usuarios WHERE username = 'mesero1';

-- Cocina = solo KITCHEN
INSERT INTO Usuarios_Modulos (usuario_id, modulo_id)
SELECT id, 'kitchen' FROM Usuarios WHERE username = 'cocina1';

-- ============================================
-- RESULTADO:
-- ============================================
-- admin    | PIN: 0000 | Todos los módulos
-- mesero1  | PIN: 1234 | Solo POS
-- cocina1  | PIN: 5555 | Solo Cocina
