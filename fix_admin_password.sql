-- Script SQL para actualizar la contraseña del usuario admin
-- Generado automáticamente con el hash correcto usando PBKDF2

-- Actualizar la contraseña del usuario admin
UPDATE public."User" 
SET "HashPassword" = 'BUOSHlKrIWuiSiZHkK30ojjWUThW07seuQaAVlbugSulbAoD' 
WHERE "UserName" = 'admin';

-- Verificar que el status esté correcto (debe ser 'Active' o 'active')
UPDATE public."User" 
SET "Status" = 'Active' 
WHERE "UserName" = 'admin';

-- Verificar que el usuario esté verificado
UPDATE public."User" 
SET "Verified" = true 
WHERE "UserName" = 'admin';

-- Ver el resultado
SELECT "IdUser", "UserName", "Name", "Status", "Verified", "IdRol", "IdConfigSys", 
       LENGTH("HashPassword") as "HashLength"
FROM public."User" 
WHERE "UserName" = 'admin';
