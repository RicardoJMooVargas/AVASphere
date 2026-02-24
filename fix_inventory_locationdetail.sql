-- Solución: Crear ubicación por defecto para productos sin asignar

-- 1. Crear una ubicación "Sin Asignar" en cada área
INSERT INTO "LocationDetails" ("Section", "VerticalLevel", "IdArea", "IdStorageStructure")
SELECT 
    'SIN_ASIGNAR' as "Section",
    0 as "VerticalLevel", 
    a."IdArea",
    (SELECT MIN(ss."IdStorageStructure") 
     FROM "StorageStructure" ss 
     WHERE ss."IdArea" = a."IdArea" 
     LIMIT 1) as "IdStorageStructure"
FROM "Area" a
WHERE NOT EXISTS (
    SELECT 1 FROM "LocationDetails" ld 
    WHERE ld."Section" = 'SIN_ASIGNAR' 
    AND ld."VerticalLevel" = 0 
    AND ld."IdArea" = a."IdArea"
);

-- 2. Actualizar registros de Inventory con LocationDetail = 0
UPDATE "Inventory" 
SET "LocationDetail" = (
    SELECT ld."IdLocationDetails"
    FROM "LocationDetails" ld
    INNER JOIN "Warehouse" w ON w."IdWarehouse" = "Inventory"."IdWarehouse"
    WHERE ld."Section" = 'SIN_ASIGNAR' 
    AND ld."VerticalLevel" = 0
    AND ld."IdArea" = w."IdArea"  -- Asumiendo que Warehouse tiene IdArea
    LIMIT 1
)
WHERE "LocationDetail" = 0;

-- 3. Si Warehouse no tiene IdArea, usar el área del primer StorageStructure disponible
UPDATE "Inventory" 
SET "LocationDetail" = (
    SELECT ld."IdLocationDetails"
    FROM "LocationDetails" ld
    WHERE ld."Section" = 'SIN_ASIGNAR' 
    AND ld."VerticalLevel" = 0
    LIMIT 1
)
WHERE "LocationDetail" = 0;
