-- Script SQL para limpiar campos duplicados en la base de datos
-- Ejecuta este script ANTES de generar nuevas migraciones

-- ======================================
-- ELIMINAR CONSTRAINTS Y FOREIGN KEYS DUPLICADAS
-- ======================================

-- Eliminar constraints de StockMovement
DO $$ 
BEGIN
    -- ProductIdProduct constraints
    IF EXISTS (SELECT 1 FROM information_schema.table_constraints 
              WHERE constraint_name = 'FK_StockMovement_Product_ProductIdProduct' 
              AND table_name = 'StockMovement') THEN
        ALTER TABLE "StockMovement" DROP CONSTRAINT "FK_StockMovement_Product_ProductIdProduct";
        RAISE NOTICE '✓ Eliminada FK_StockMovement_Product_ProductIdProduct';
    END IF;
    
    -- WarehouseIdWarehouse constraints
    IF EXISTS (SELECT 1 FROM information_schema.table_constraints 
              WHERE constraint_name = 'FK_StockMovement_Warehouse_WarehouseIdWarehouse' 
              AND table_name = 'StockMovement') THEN
        ALTER TABLE "StockMovement" DROP CONSTRAINT "FK_StockMovement_Warehouse_WarehouseIdWarehouse";
        RAISE NOTICE '✓ Eliminada FK_StockMovement_Warehouse_WarehouseIdWarehouse';
    END IF;
    
    -- LocationDetails constraints
    IF EXISTS (SELECT 1 FROM information_schema.table_constraints 
              WHERE constraint_name = 'FK_LocationDetails_Area_AreaIdArea' 
              AND table_name = 'LocationDetails') THEN
        ALTER TABLE "LocationDetails" DROP CONSTRAINT "FK_LocationDetails_Area_AreaIdArea";
        RAISE NOTICE '✓ Eliminada FK_LocationDetails_Area_AreaIdArea';
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.table_constraints 
              WHERE constraint_name = 'FK_LocationDetails_StorageStructure_StorageStructureIdStorageStructure' 
              AND table_name = 'LocationDetails') THEN
        ALTER TABLE "LocationDetails" DROP CONSTRAINT "FK_LocationDetails_StorageStructure_StorageStructureIdStorageStructure";
        RAISE NOTICE '✓ Eliminada FK_LocationDetails_StorageStructure_StorageStructureIdStorageStructure';
    END IF;
    
    -- PhysicalInventoryDetail constraints
    IF EXISTS (SELECT 1 FROM information_schema.table_constraints 
              WHERE constraint_name = 'FK_PhysicalInventoryDetail_Product_ProductIdProduct' 
              AND table_name = 'PhysicalInventoryDetail') THEN
        ALTER TABLE "PhysicalInventoryDetail" DROP CONSTRAINT "FK_PhysicalInventoryDetail_Product_ProductIdProduct";
        RAISE NOTICE '✓ Eliminada FK_PhysicalInventoryDetail_Product_ProductIdProduct';
    END IF;
    
    -- WarehouseTransferDetail constraints
    IF EXISTS (SELECT 1 FROM information_schema.table_constraints 
              WHERE constraint_name = 'FK_WarehouseTransferDetail_Product_ProductIdProduct' 
              AND table_name = 'WarehouseTransferDetail') THEN
        ALTER TABLE "WarehouseTransferDetail" DROP CONSTRAINT "FK_WarehouseTransferDetail_Product_ProductIdProduct";
        RAISE NOTICE '✓ Eliminada FK_WarehouseTransferDetail_Product_ProductIdProduct';
    END IF;
    
    -- Inventory constraints
    IF EXISTS (SELECT 1 FROM information_schema.table_constraints 
              WHERE constraint_name = 'FK_Inventory_Product_ProductIdProduct' 
              AND table_name = 'Inventory') THEN
        ALTER TABLE "Inventory" DROP CONSTRAINT "FK_Inventory_Product_ProductIdProduct";
        RAISE NOTICE '✓ Eliminada FK_Inventory_Product_ProductIdProduct';
    END IF;
END $$;

-- ======================================
-- ELIMINAR ÍNDICES DUPLICADOS
-- ======================================

DO $$ 
BEGIN
    -- StockMovement indexes
    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_StockMovement_ProductIdProduct') THEN
        DROP INDEX "IX_StockMovement_ProductIdProduct";
        RAISE NOTICE '✓ Eliminado índice IX_StockMovement_ProductIdProduct';
    END IF;
    
    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_StockMovement_WarehouseIdWarehouse') THEN
        DROP INDEX "IX_StockMovement_WarehouseIdWarehouse";
        RAISE NOTICE '✓ Eliminado índice IX_StockMovement_WarehouseIdWarehouse';
    END IF;
    
    -- LocationDetails indexes
    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_LocationDetails_AreaIdArea') THEN
        DROP INDEX "IX_LocationDetails_AreaIdArea";
        RAISE NOTICE '✓ Eliminado índice IX_LocationDetails_AreaIdArea';
    END IF;
    
    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_LocationDetails_StorageStructureIdStorageStructure') THEN
        DROP INDEX "IX_LocationDetails_StorageStructureIdStorageStructure";
        RAISE NOTICE '✓ Eliminado índice IX_LocationDetails_StorageStructureIdStorageStructure';
    END IF;
    
    -- PhysicalInventoryDetail indexes
    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_PhysicalInventoryDetail_ProductIdProduct') THEN
        DROP INDEX "IX_PhysicalInventoryDetail_ProductIdProduct";
        RAISE NOTICE '✓ Eliminado índice IX_PhysicalInventoryDetail_ProductIdProduct';
    END IF;
    
    -- WarehouseTransferDetail indexes
    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_WarehouseTransferDetail_ProductIdProduct') THEN
        DROP INDEX "IX_WarehouseTransferDetail_ProductIdProduct";
        RAISE NOTICE '✓ Eliminado índice IX_WarehouseTransferDetail_ProductIdProduct';
    END IF;
    
    -- Inventory indexes
    IF EXISTS (SELECT 1 FROM pg_indexes WHERE indexname = 'IX_Inventory_ProductIdProduct') THEN
        DROP INDEX "IX_Inventory_ProductIdProduct";
        RAISE NOTICE '✓ Eliminado índice IX_Inventory_ProductIdProduct';
    END IF;
END $$;

-- ======================================
-- ELIMINAR COLUMNAS DUPLICADAS
-- ======================================

DO $$ 
BEGIN
    -- StockMovement columns
    IF EXISTS (SELECT 1 FROM information_schema.columns 
              WHERE table_name = 'StockMovement' AND column_name = 'ProductIdProduct') THEN
        ALTER TABLE "StockMovement" DROP COLUMN "ProductIdProduct";
        RAISE NOTICE '✓ Eliminada columna StockMovement.ProductIdProduct';
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.columns 
              WHERE table_name = 'StockMovement' AND column_name = 'WarehouseIdWarehouse') THEN
        ALTER TABLE "StockMovement" DROP COLUMN "WarehouseIdWarehouse";
        RAISE NOTICE '✓ Eliminada columna StockMovement.WarehouseIdWarehouse';
    END IF;
    
    -- LocationDetails columns
    IF EXISTS (SELECT 1 FROM information_schema.columns 
              WHERE table_name = 'LocationDetails' AND column_name = 'AreaIdArea') THEN
        ALTER TABLE "LocationDetails" DROP COLUMN "AreaIdArea";
        RAISE NOTICE '✓ Eliminada columna LocationDetails.AreaIdArea';
    END IF;
    
    IF EXISTS (SELECT 1 FROM information_schema.columns 
              WHERE table_name = 'LocationDetails' AND column_name = 'StorageStructureIdStorageStructure') THEN
        ALTER TABLE "LocationDetails" DROP COLUMN "StorageStructureIdStorageStructure";
        RAISE NOTICE '✓ Eliminada columna LocationDetails.StorageStructureIdStorageStructure';
    END IF;
    
    -- PhysicalInventoryDetail columns
    IF EXISTS (SELECT 1 FROM information_schema.columns 
              WHERE table_name = 'PhysicalInventoryDetail' AND column_name = 'ProductIdProduct') THEN
        ALTER TABLE "PhysicalInventoryDetail" DROP COLUMN "ProductIdProduct";
        RAISE NOTICE '✓ Eliminada columna PhysicalInventoryDetail.ProductIdProduct';
    END IF;
    
    -- WarehouseTransferDetail columns
    IF EXISTS (SELECT 1 FROM information_schema.columns 
              WHERE table_name = 'WarehouseTransferDetail' AND column_name = 'ProductIdProduct') THEN
        ALTER TABLE "WarehouseTransferDetail" DROP COLUMN "ProductIdProduct";
        RAISE NOTICE '✓ Eliminada columna WarehouseTransferDetail.ProductIdProduct';
    END IF;
    
    -- Inventory columns
    IF EXISTS (SELECT 1 FROM information_schema.columns 
              WHERE table_name = 'Inventory' AND column_name = 'ProductIdProduct') THEN
        ALTER TABLE "Inventory" DROP COLUMN "ProductIdProduct";
        RAISE NOTICE '✓ Eliminada columna Inventory.ProductIdProduct';
    END IF;
END $$;

-- ======================================
-- VERIFICAR RESULTADOS
-- ======================================

-- Verificar que los campos duplicados han sido eliminados
DO $$ 
DECLARE
    duplicate_count INTEGER := 0;
BEGIN
    -- Contar campos duplicados restantes
    SELECT COUNT(*) INTO duplicate_count
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
    AND (column_name LIKE '%IdProduct' OR 
         column_name LIKE '%IdWarehouse' OR 
         column_name LIKE '%IdArea' OR 
         column_name LIKE '%IdStorageStructure')
    AND (column_name LIKE '%ProductIdProduct%' OR 
         column_name LIKE '%WarehouseIdWarehouse%' OR 
         column_name LIKE '%AreaIdArea%' OR 
         column_name LIKE '%StorageStructureIdStorageStructure%');
    
    IF duplicate_count = 0 THEN
        RAISE NOTICE '✅ SUCCESS: Todos los campos duplicados han sido eliminados!';
    ELSE
        RAISE NOTICE '⚠️  WARNING: Aún quedan % campos duplicados', duplicate_count;
    END IF;
END $$;

RAISE NOTICE '';
RAISE NOTICE '🚀 PROCESO COMPLETADO! Ahora puedes:';
RAISE NOTICE '1. Eliminar todas las migraciones: rm -rf src/AVASphere.Infrastructure/System/Migrations/*';
RAISE NOTICE '2. Generar nueva migración: dotnet ef migrations add CleanInitial';
RAISE NOTICE '3. Aplicar migración: dotnet ef database update';
