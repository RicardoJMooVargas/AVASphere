-- SQL Script para mover TypeStorageSystem de LocationDetails a StorageStructure
-- Ejecutar este script en la base de datos avaspheredb

-- Paso 1: Agregar la columna TypeStorageSystem a la tabla StorageStructure
ALTER TABLE "StorageStructure" 
ADD COLUMN "TypeStorageSystem" character varying(100) DEFAULT 'Sistema General';

-- Paso 2: Actualizar StorageStructure con los valores de TypeStorageSystem de LocationDetails
-- Este query toma el primer TypeStorageSystem encontrado en LocationDetails para cada StorageStructure
UPDATE "StorageStructure" 
SET "TypeStorageSystem" = subquery.type_storage
FROM (
    SELECT 
        ld."IdStorageStructure",
        MIN(ld."TypeStorageSystem") as type_storage
    FROM "LocationDetails" ld
    GROUP BY ld."IdStorageStructure"
) AS subquery
WHERE "StorageStructure"."IdStorageStructure" = subquery."IdStorageStructure";

-- Paso 3: Hacer la columna TypeStorageSystem NOT NULL en StorageStructure
ALTER TABLE "StorageStructure" 
ALTER COLUMN "TypeStorageSystem" SET NOT NULL;

-- Paso 4: Eliminar la columna TypeStorageSystem de LocationDetails
ALTER TABLE "LocationDetails" 
DROP COLUMN "TypeStorageSystem";

-- Verificar los cambios
SELECT 'StorageStructure' as tabla, COUNT(*) as total_registros FROM "StorageStructure"
UNION ALL
SELECT 'LocationDetails' as tabla, COUNT(*) as total_registros FROM "LocationDetails";

-- Mostrar algunas filas de ejemplo para verificar
SELECT ss."IdStorageStructure", ss."CodeRack", ss."TypeStorageSystem", ss."IdArea"
FROM "StorageStructure" ss
LIMIT 5;

SELECT ld."IdLocationDetails", ld."Section", ld."VerticalLevel", ld."IdStorageStructure"
FROM "LocationDetails" ld
LIMIT 5;
