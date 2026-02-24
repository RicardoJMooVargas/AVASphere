-- Script para implementar Opción 3: Crear ubicaciones "SIN_ASIGNAR" y corregir Inventory.LocationDetail

-- 1. Crear ubicaciones "SIN_ASIGNAR" para cada área que tenga StorageStructure
INSERT INTO "LocationDetails" ("Section", "VerticalLevel", "IdArea", "IdStorageStructure")
SELECT DISTINCT
    'SIN_ASIGNAR' as "Section",
    0 as "VerticalLevel",
    ss."IdArea" as "IdArea",
    ss."IdStorageStructure"
FROM "StorageStructure" ss
WHERE ss."IdArea" IS NOT NULL
AND NOT EXISTS (
    SELECT 1 
    FROM "LocationDetails" ld 
    WHERE ld."Section" = 'SIN_ASIGNAR' 
    AND ld."VerticalLevel" = 0 
    AND ld."IdArea" = ss."IdArea"
    AND ld."IdStorageStructure" = ss."IdStorageStructure"
);

-- 2. Actualizar registros de Inventory con LocationDetail = 0 usando la primera ubicación "SIN_ASIGNAR" disponible
UPDATE "Inventory" 
SET "LocationDetail" = (
    SELECT ld."IdLocationDetails"
    FROM "LocationDetails" ld
    INNER JOIN "StorageStructure" ss ON ld."IdStorageStructure" = ss."IdStorageStructure"
    WHERE ld."Section" = 'SIN_ASIGNAR' 
    AND ld."VerticalLevel" = 0
    AND ss."IdWarehouse" = "Inventory"."IdWarehouse"
    LIMIT 1
)
WHERE "LocationDetail" = 0;

-- 3. Para inventarios que aún tengan LocationDetail = 0 (sin StorageStructure en su warehouse), usar cualquier ubicación disponible
UPDATE "Inventory" 
SET "LocationDetail" = (
    SELECT ld."IdLocationDetails"
    FROM "LocationDetails" ld
    WHERE ld."Section" = 'SIN_ASIGNAR' 
    AND ld."VerticalLevel" = 0
    LIMIT 1
)
WHERE "LocationDetail" = 0;

-- 4. Verificar resultados
SELECT 'Resultados después de la corrección:' as info;
SELECT 
    COUNT(*) as "Total_Inventory_Records",
    COUNT(CASE WHEN "LocationDetail" IS NULL THEN 1 END) as "NULL_LocationDetail",
    COUNT(CASE WHEN "LocationDetail" = 0 THEN 1 END) as "Zero_LocationDetail_Restantes", 
    COUNT(CASE WHEN "LocationDetail" > 0 THEN 1 END) as "Valid_LocationDetail"
FROM "Inventory";

-- 5. Mostrar ubicaciones "SIN_ASIGNAR" creadas
SELECT 'Ubicaciones SIN_ASIGNAR creadas:' as info;
SELECT 
    ld."IdLocationDetails",
    ld."Section",
    ld."VerticalLevel", 
    ld."IdArea",
    a."Name" as "AreaName",
    ss."CodeRack",
    ss."TypeStorageSystem"
FROM "LocationDetails" ld
INNER JOIN "Area" a ON ld."IdArea" = a."IdArea"
INNER JOIN "StorageStructure" ss ON ld."IdStorageStructure" = ss."IdStorageStructure"
WHERE ld."Section" = 'SIN_ASIGNAR' AND ld."VerticalLevel" = 0
ORDER BY ld."IdArea";
