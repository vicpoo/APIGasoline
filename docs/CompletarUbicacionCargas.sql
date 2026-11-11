-- =====================================================================================
-- Completa la ubicación de las cargas de combustible VIEJAS que se guardaron sin ella.
-- Misma regla que la API (UbicacionCargaEstimador):
--   punto_ruta del mismo viaje con la hora más cercana a fecha_carga (máx. 10 minutos),
--   prefiriendo puntos con precisión <= 50 m (o sin dato).
-- Solo toca cargas con ubicacion_lat / ubicacion_lng en NULL. Se puede correr varias veces.
-- SQL Server (T-SQL). Correr UNA vez después de desplegar (las cargas nuevas las completa la API sola).
-- =====================================================================================

DROP TABLE IF EXISTS #ubicacion_carga;

SELECT
    c.carga_combustible_id,
    (
        SELECT TOP (1) p.punto_ruta_id
        FROM punto_ruta p
        WHERE p.viaje_id = c.viaje_id
          AND ABS(DATEDIFF(SECOND, p.timestamp_captura, c.fecha_carga)) <= 600
        ORDER BY
            CASE WHEN p.precision_metros > 50 THEN 1 ELSE 0 END,
            ABS(DATEDIFF(SECOND, p.timestamp_captura, c.fecha_carga)),
            COALESCE(p.precision_metros, 0)
    ) AS punto_ruta_id
INTO #ubicacion_carga
FROM carga_combustible c
WHERE c.ubicacion_lat IS NULL OR c.ubicacion_lng IS NULL;

-- Revisar antes de actualizar (cuántas se pueden ubicar y cuántas no)
SELECT
    SUM(CASE WHEN punto_ruta_id IS NOT NULL THEN 1 ELSE 0 END) AS se_pueden_ubicar,
    SUM(CASE WHEN punto_ruta_id IS NULL THEN 1 ELSE 0 END)     AS sin_puntos_cercanos
FROM #ubicacion_carga;

UPDATE c
SET c.ubicacion_lat = p.latitud,
    c.ubicacion_lng = p.longitud
FROM carga_combustible c
JOIN #ubicacion_carga t ON t.carga_combustible_id = c.carga_combustible_id
JOIN punto_ruta p       ON p.punto_ruta_id = t.punto_ruta_id
WHERE c.ubicacion_lat IS NULL OR c.ubicacion_lng IS NULL;

DROP TABLE IF EXISTS #ubicacion_carga;
