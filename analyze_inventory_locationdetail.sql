-- Consulta para verificar el estado actual de LocationDetails vs Inventory

-- 1. Verificar IDs válidos de LocationDetails
SELECT 'LocationDetails válidos:' as info;
SELECT "IdLocationDetails", "Section", "VerticalLevel", "IdArea", "IdStorageStructure" 
FROM "LocationDetails" 
ORDER BY "IdLocationDetails";

-- 2. Verificar valores únicos en Inventory.LocationDetail
SELECT 'Valores únicos de LocationDetail en Inventory:' as info;
SELECT DISTINCT "LocationDetail", COUNT(*) as count
FROM "Inventory" 
GROUP BY "LocationDetail"
ORDER BY "LocationDetail";

-- 3. Encontrar registros de Inventory con LocationDetail inválido (que no existe en LocationDetails)
SELECT 'Inventory con LocationDetail inválido:' as info;
SELECT 
    i."IdInventory",
    i."LocationDetail",
    p."MainName" as "ProductName",
    CASE 
        WHEN i."LocationDetail" IS NULL THEN 'NULL (válido)'
        WHEN i."LocationDetail" = 0 THEN 'CERO (inválido - no existe LocationDetails con ID 0)'
        WHEN ld."IdLocationDetails" IS NULL THEN 'NO EXISTE (inválido)'
        ELSE 'VÁLIDO'
    END as "Status"
FROM "Inventory" i
LEFT JOIN "Product" p ON i."IdProduct" = p."IdProduct"
LEFT JOIN "LocationDetails" ld ON i."LocationDetail"::integer = ld."IdLocationDetails"
WHERE i."LocationDetail" IS NOT NULL 
  AND (i."LocationDetail" = 0 OR ld."IdLocationDetails" IS NULL)
ORDER BY i."IdInventory"
LIMIT 10;

-- 4. Estadísticas generales
SELECT 'Estadísticas de Inventory.LocationDetail:' as info;
SELECT 
    COUNT(*) as "Total_Inventory_Records",
    COUNT(CASE WHEN "LocationDetail" IS NULL THEN 1 END) as "NULL_LocationDetail",
    COUNT(CASE WHEN "LocationDetail" = 0 THEN 1 END) as "Zero_LocationDetail", 
    COUNT(CASE WHEN "LocationDetail" > 0 THEN 1 END) as "Positive_LocationDetail"
FROM "Inventory";
