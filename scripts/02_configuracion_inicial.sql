-- ============================================
-- CONFIGURACIÓN INICIAL DEL SISTEMA
-- ============================================

-- MÉTODOS DE PAGO
INSERT INTO Metodos_Pago (id, codigo, nombre, activo, requiere_referencia) VALUES
(NEWID(), 'cash', 'Efectivo', 1, 0),
(NEWID(), 'card', 'Tarjeta', 1, 1),
(NEWID(), 'transfer', 'Transferencia', 1, 1);

-- CONFIGURACIÓN DE IMPUESTOS
IF NOT EXISTS (SELECT 1 FROM Config_Impuestos WHERE id = 1)
BEGIN
    INSERT INTO Config_Impuestos (
        id, iva_habilitado, iva_tasa, iva_incluido,
        propina_habilitada, propina_sugerida,
        propina_auto, propina_auto_min_comensales, propina_auto_tasa,
        cargo_servicio_habilitado, cargo_servicio_tasa
    ) VALUES (
        1, 1, 0.1600, 0,
        1, 0.1000,
        0, 6, 0.1000,
        0, 0.0000
    );
END

-- CONFIGURACIÓN DEL NEGOCIO
IF NOT EXISTS (SELECT 1 FROM Config_Negocio WHERE id = 1)
BEGIN
    INSERT INTO Config_Negocio (
        id, nombre, rfc, direccion, telefono, email,
        ticket_encabezado, ticket_pie, moneda, zona_horaria
    ) VALUES (
        1, 'Restaurante SF', 'RFC123456789', 'Calle Principal #123', '555-1234', 'info@restaurantesf.com',
        'Bienvenido a Restaurante SF', 'Gracias por su visita', 'MXN', 'America/Mexico_City'
    );
END

PRINT '? Configuración inicial creada';
PRINT '- 3 Métodos de pago';
PRINT '- Configuración de impuestos (IVA 16%)';
PRINT '- Configuración del negocio';
