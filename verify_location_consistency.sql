-- Verificar inconsistencias en la estructura actual

-- 1. Verificar si hay StorageStructures que no coinciden con el Area del LocationDetail
SELECT 
    ld."IdLocationDetails",
    ld."Section", 
    ld."VerticalLevel",
    ld."IdArea" AS "LocationArea",
    a1."Name" AS "LocationAreaName",
    ld."IdStorageStructure",
    ss."IdArea" AS "StorageArea", 
    a2."Name" AS "StorageAreaName",
    ss."CodeRack",
    CASE 
        WHEN ld."IdArea" != ss."IdArea" THEN '❌ INCONSISTENTE' 
        ELSE '✅ OK' 
    END AS "Status"
FROM "LocationDetails" ld
INNER JOIN "StorageStructure" ss ON ld."IdStorageStructure" = ss."IdStorageStructure"
INNER JOIN "Area" a1 ON ld."IdArea" = a1."IdArea"
INNER JOIN "Area" a2 ON ss."IdArea" = a2."IdArea"
ORDER BY ld."IdLocationDetails";

-- 2. Verificar PhysicalInventoryDetails asociados a LocationDetails
SELECT 
    pid."IdPhysicalInventoryDetail",
    pid."IdPhysicalInventory",
    pid."IdProduct",
    p."MainName" AS "ProductName",
    pid."IdLocationDetails",
    ld."Section",
    ld."VerticalLevel",
    a."Name" AS "AreaName"
FROM "PhysicalInventoryDetail" pid
LEFT JOIN "Product" p ON pid."IdProduct" = p."IdProduct"
LEFT JOIN "LocationDetails" ld ON pid."IdLocationDetails" = ld."IdLocationDetails"
LEFT JOIN "Area" a ON ld."IdArea" = a."IdArea"
WHERE pid."IdLocationDetails" IS NOT NULL;

-- 3. Verificar Inventory con LocationDetail
SELECT 
    i."IdInventory",
    i."Stock",
    i."LocationDetail",
    p."MainName" AS "ProductName",
    w."Name" AS "WarehouseName"
FROM "Inventory" i
LEFT JOIN "Product" p ON i."IdProduct" = p."IdProduct"  
LEFT JOIN "Warehouse" w ON i."IdWarehouse" = w."IdWarehouse"
WHERE i."LocationDetail" IS NOT NULL;
