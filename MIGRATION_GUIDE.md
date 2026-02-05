## ✅ SOLUCIÓN: Migración con StorageStructure-Area

### 🚨 **Problema identificado:**
- Las migraciones no se encuentran en el assembly
- La carpeta `System/Migrations` está vacía
- Los cambios de `Area` ↔ `StorageStructure` necesitan ser migrados

### 🛠️ **Solución Recomendada (Según reglas de desarrollo):**

#### **OPCIÓN 1: Sistema Automatizado (Recomendado)**
1. Asegúrate de que la aplicación esté ejecutándose:
   ```bash
   cd /home/ricardomogas/RiderProjects/AVASphere/src/AVASphere.WebApi
   dotnet run
   ```

2. Usa el endpoint automatizado:
   ```http
   POST http://localhost:5001/api/system/DbTools/full-migration?name=InitialWithAreaRelation
   ```

   O desde Swagger UI:
   - Navega a: `http://localhost:5001/swagger`
   - Busca: **"System - Database Tools"**
   - Usa: `POST /api/system/DbTools/full-migration`

#### **OPCIÓN 2: Manual (Si la automatizada no funciona)**
```bash
# 1. Desde el directorio raíz del proyecto:
cd /home/ricardomogas/RiderProjects/AVASphere

# 2. Crear migración inicial:
dotnet ef migrations add InitialWithAreaRelation \
  --project src/AVASphere.Infrastructure \
  --startup-project src/AVASphere.Infrastructure \
  --context MasterDbContext \
  --output-dir System/Migrations

# 3. Aplicar migración:
dotnet ef database update \
  --project src/AVASphere.Infrastructure \
  --startup-project src/AVASphere.Infrastructure \
  --context MasterDbContext
```

### 📋 **Cambios que se aplicarán:**
- ✅ Tabla `StorageStructure` con columna `IdArea` nullable
- ✅ Foreign Key: `FK_StorageStructure_Area_IdArea` con `ON DELETE SET NULL`
- ✅ Relación bidireccional `Area` ↔ `StorageStructures`
- ✅ Todas las demás tablas y relaciones existentes

### 🔍 **Verificación Post-Migración:**
```sql
-- Verificar que la columna IdArea existe en StorageStructure
SELECT column_name, is_nullable, data_type 
FROM information_schema.columns 
WHERE table_name = 'StorageStructure' 
AND column_name = 'IdArea';

-- Verificar la foreign key
SELECT constraint_name, table_name, column_name 
FROM information_schema.key_column_usage 
WHERE table_name = 'StorageStructure' 
AND column_name = 'IdArea';
```
