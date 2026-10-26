-- LABORATORIO ESPE - EJECUTAR EN SSMS
USE sakila;
GO

-- 1. Verificar existencia de tablas y columnas
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN ('country','city');

SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('country','city')
ORDER BY TABLE_NAME, ORDINAL_POSITION;

-- 2. Comprobar PK y FK registradas
SELECT t.name AS tabla, kc.name AS restriccion_pk, c.name AS columna_pk
FROM sys.key_constraints kc
JOIN sys.tables t ON t.object_id = kc.parent_object_id
JOIN sys.index_columns ic ON ic.object_id = t.object_id AND ic.index_id = kc.unique_index_id
JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = ic.column_id
WHERE kc.type = 'PK' AND t.name IN ('country','city');

SELECT fk.name AS nombre_fk, tp.name AS tabla_hija, cp.name AS columna_hija,
       tr.name AS tabla_padre, cr.name AS columna_padre
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
JOIN sys.tables tp ON tp.object_id = fkc.parent_object_id
JOIN sys.columns cp ON cp.object_id = tp.object_id AND cp.column_id = fkc.parent_column_id
JOIN sys.tables tr ON tr.object_id = fkc.referenced_object_id
JOIN sys.columns cr ON cr.object_id = tr.object_id AND cr.column_id = fkc.referenced_column_id
WHERE tp.name = 'city' AND tr.name = 'country';

-- 3. Join país-ciudades (captura)
SELECT co.country_id, co.country, ci.city_id, ci.city
FROM dbo.country AS co
INNER JOIN dbo.city AS ci ON co.country_id = ci.country_id
ORDER BY co.country, ci.city;

-- 4. Un país con varias ciudades (toma un ID del resultado)
SELECT TOP (1) co.country_id, co.country, COUNT(*) AS numero_ciudades
FROM dbo.country co
INNER JOIN dbo.city ci ON co.country_id = ci.country_id
GROUP BY co.country_id, co.country
HAVING COUNT(*) > 1
ORDER BY COUNT(*) DESC;

-- Sustituir @CountryId por el country_id hallado en la consulta anterior
DECLARE @CountryId INT = 1;
SELECT co.country, ci.city_id, ci.city
FROM dbo.country co
JOIN dbo.city ci ON co.country_id = ci.country_id
WHERE co.country_id = @CountryId
ORDER BY ci.city;

-- 5. Totales de Home
SELECT COUNT(*) AS total_paises FROM dbo.country;
SELECT COUNT(*) AS total_ciudades FROM dbo.city;

-- 6. Comprobar estructura de auto-incremento de claves
SELECT t.name AS tabla, c.name AS columna, c.is_identity
FROM sys.tables t
JOIN sys.columns c ON t.object_id = c.object_id
WHERE t.name IN ('country','city') AND c.name IN ('country_id','city_id');

-- 7. Después de un CRUD, verificar cambios con SELECT
SELECT TOP (30) * FROM dbo.country ORDER BY country_id DESC;
SELECT TOP (30) * FROM dbo.city ORDER BY city_id DESC;
