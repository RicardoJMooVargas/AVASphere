-- Consulta para verificar registros existentes que causan el conflicto
SELECT 
    ld."IdLocationDetails",
    ld."Section", 
    ld."VerticalLevel",
    ld."IdArea",
    a."Name" AS "AreaName",
    a."NormalizedName" AS "AreaNormalizedName",
    ld."IdStorageStructure",
    ss."CodeRack",
    ss."TypeStorageSystem"
FROM "LocationDetails" ld
INNER JOIN "Area" a ON ld."IdArea" = a."IdArea"
LEFT JOIN "StorageStructure" ss ON ld."IdStorageStructure" = ss."IdStorageStructure"
WHERE ld."IdArea" = 4 
  AND ld."IdStorageStructure" = 4 
  AND ld."Section" = 'A' 
  AND ld."VerticalLevel" = 1;

-- Consulta adicional para ver TODOS los registros del área y estructura
SELECT 
    ld."IdLocationDetails",
    ld."Section", 
    ld."VerticalLevel",
    ld."IdArea",
    ld."IdStorageStructure",
    a."Name" AS "AreaName",
    ss."CodeRack"
FROM "LocationDetails" ld
INNER JOIN "Area" a ON ld."IdArea" = a."IdArea"
LEFT JOIN "StorageStructure" ss ON ld."IdStorageStructure" = ss."IdStorageStructure"
WHERE ld."IdArea" = 4 AND ld."IdStorageStructure" = 4
ORDER BY ld."Section", ld."VerticalLevel";

-- Consulta para verificar si hay PhysicalInventoryDetails asociados
SELECT 
    pid."IdPhysicalInventoryDetail",
    pid."IdPhysicalInventory",
    pid."IdProduct",
    pid."IdLocationDetails",
    p."MainName" AS "ProductName"
FROM "PhysicalInventoryDetail" pid
LEFT JOIN "Product" p ON pid."IdProduct" = p."IdProduct"
WHERE pid."IdLocationDetails" IN (
    SELECT ld."IdLocationDetails"
    FROM "LocationDetails" ld
    WHERE ld."IdArea" = 4 AND ld."IdStorageStructure" = 4
);
