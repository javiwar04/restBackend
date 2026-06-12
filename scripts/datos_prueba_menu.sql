-- ============================================
-- DATOS DE PRUEBA - MENÚ COMPLETO
-- ============================================

-- SECCIONES DEL RESTAURANTE
INSERT INTO Secciones (id, nombre, orden, activa) VALUES
(NEWID(), 'Restaurante', 1, 1),
(NEWID(), 'Terraza', 2, 1),
(NEWID(), 'Bar', 3, 1);

-- MESAS (ajusta los IDs de sección después de ejecutar el INSERT anterior)
-- O usa esto para obtener los IDs:
DECLARE @RestId NVARCHAR(36) = (SELECT TOP 1 id FROM Secciones WHERE nombre = 'Restaurante');
DECLARE @TerrazaId NVARCHAR(36) = (SELECT TOP 1 id FROM Secciones WHERE nombre = 'Terraza');
DECLARE @BarId NVARCHAR(36) = (SELECT TOP 1 id FROM Secciones WHERE nombre = 'Bar');

-- Mesas Restaurante
INSERT INTO Mesas (id, numero, etiqueta, capacidad, seccion_id, activa) VALUES
(NEWID(), 1, 'R1', 4, @RestId, 1),
(NEWID(), 2, 'R2', 4, @RestId, 1),
(NEWID(), 3, 'R3', 6, @RestId, 1),
(NEWID(), 4, 'R4', 2, @RestId, 1),
(NEWID(), 5, 'R5', 8, @RestId, 1);

-- Mesas Terraza
INSERT INTO Mesas (id, numero, etiqueta, capacidad, seccion_id, activa) VALUES
(NEWID(), 6, 'T1', 4, @TerrazaId, 1),
(NEWID(), 7, 'T2', 4, @TerrazaId, 1),
(NEWID(), 8, 'T3', 6, @TerrazaId, 1);

-- Mesas Bar
INSERT INTO Mesas (id, numero, etiqueta, capacidad, seccion_id, activa) VALUES
(NEWID(), 9, 'B1', 2, @BarId, 1),
(NEWID(), 10, 'B2', 2, @BarId, 1);

-- CATEGORÍAS DEL MENÚ
INSERT INTO Categorias_Menu (id, nombre, orden, activa) VALUES
(NEWID(), 'Entradas', 1, 1),
(NEWID(), 'Ensaladas', 2, 1),
(NEWID(), 'Principales', 3, 1),
(NEWID(), 'Postres', 4, 1),
(NEWID(), 'Bebidas', 5, 1);

-- PLATILLOS (ajusta los IDs de categoría)
DECLARE @EntradasId NVARCHAR(36) = (SELECT TOP 1 id FROM Categorias_Menu WHERE nombre = 'Entradas');
DECLARE @EnsaladasId NVARCHAR(36) = (SELECT TOP 1 id FROM Categorias_Menu WHERE nombre = 'Ensaladas');
DECLARE @PrincipalesId NVARCHAR(36) = (SELECT TOP 1 id FROM Categorias_Menu WHERE nombre = 'Principales');
DECLARE @PostresId NVARCHAR(36) = (SELECT TOP 1 id FROM Categorias_Menu WHERE nombre = 'Postres');
DECLARE @BebidasId NVARCHAR(36) = (SELECT TOP 1 id FROM Categorias_Menu WHERE nombre = 'Bebidas');

-- Entradas
INSERT INTO Platillos (id, categoria_id, nombre, descripcion, precio, disponible) VALUES
(NEWID(), @EntradasId, 'Alitas BBQ', '10 piezas con salsa BBQ y aderezo ranch', 120.00, 1),
(NEWID(), @EntradasId, 'Nachos Supremos', 'Nachos con queso, carne, guacamole y crema', 95.00, 1),
(NEWID(), @EntradasId, 'Dedos de Queso', '6 piezas con marinara', 85.00, 1);

-- Ensaladas
INSERT INTO Platillos (id, categoria_id, nombre, descripcion, precio, disponible) VALUES
(NEWID(), @EnsaladasId, 'César', 'Lechuga romana, crutones, parmesano, aderezo césar', 110.00, 1),
(NEWID(), @EnsaladasId, 'Griega', 'Tomate, pepino, aceitunas, queso feta', 105.00, 1);

-- Principales
INSERT INTO Platillos (id, categoria_id, nombre, descripcion, precio, disponible) VALUES
(NEWID(), @PrincipalesId, 'Filete al Gusto', '300g de filete con guarnición', 285.00, 1),
(NEWID(), @PrincipalesId, 'Salmón a la Parrilla', 'Filete de salmón con vegetales', 245.00, 1),
(NEWID(), @PrincipalesId, 'Pasta Alfredo', 'Fettuccine en salsa alfredo', 165.00, 1),
(NEWID(), @PrincipalesId, 'Hamburguesa Clásica', 'Carne 180g, queso, lechuga, tomate, papas', 145.00, 1);

-- Postres
INSERT INTO Platillos (id, categoria_id, nombre, descripcion, precio, disponible) VALUES
(NEWID(), @PostresId, 'Cheesecake', 'Pastel de queso con frutos rojos', 85.00, 1),
(NEWID(), @PostresId, 'Brownie con Helado', 'Brownie caliente con helado de vainilla', 75.00, 1),
(NEWID(), @PostresId, 'Flan Napolitano', 'Flan casero con caramelo', 65.00, 1);

-- Bebidas
INSERT INTO Platillos (id, categoria_id, nombre, descripcion, precio, disponible) VALUES
(NEWID(), @BebidasId, 'Coca Cola', 'Refresco 355ml', 35.00, 1),
(NEWID(), @BebidasId, 'Agua Natural', 'Botella 600ml', 25.00, 1),
(NEWID(), @BebidasId, 'Cerveza Corona', '355ml', 45.00, 1),
(NEWID(), @BebidasId, 'Limonada Natural', 'Vaso grande', 40.00, 1);

PRINT '? Datos de prueba insertados correctamente';
PRINT '';
PRINT '?? Resumen:';
PRINT '- 3 Secciones';
PRINT '- 10 Mesas';
PRINT '- 5 Categorías';
PRINT '- 16 Platillos';
