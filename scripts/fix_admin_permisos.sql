-- ============================================
-- ARREGLAR PERMISOS DEL USUARIO ADMIN
-- ============================================

-- Primero verificamos que existan los módulos
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

-- Eliminar permisos existentes del admin (por si acaso)
DELETE FROM Usuarios_Modulos 
WHERE usuario_id IN (SELECT id FROM Usuarios WHERE username = 'admin');

-- Asignar TODOS los módulos al admin
INSERT INTO Usuarios_Modulos (usuario_id, modulo_id)
SELECT u.id, m.id
FROM Usuarios u
CROSS JOIN Modulos m
WHERE u.username = 'admin';

-- Verificar
SELECT 
    u.username,
    u.nombre,
    r.nombre as rol,
    STRING_AGG(um.modulo_id, ', ') as modulos
FROM Usuarios u
LEFT JOIN Roles r ON u.rol_id = r.id
LEFT JOIN Usuarios_Modulos um ON u.id = um.usuario_id
WHERE u.username = 'admin'
GROUP BY u.username, u.nombre, r.nombre;

PRINT '? Permisos actualizados para usuario admin';
