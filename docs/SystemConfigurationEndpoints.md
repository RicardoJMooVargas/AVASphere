# Endpoints de Configuración Inicial del Sistema

Este documento describe los nuevos endpoints para configurar el sistema AVASphere y crear el usuario administrador inicial.

## 1. Verificar Configuración Inicial

**Endpoint:** `GET /api/system/config/check-initial-config`

### Descripción
Verifica si existe configuración inicial del sistema (ConfigSys) y si las tablas de la base de datos están creadas.

### Respuesta

#### Caso 1: Base de datos no conectada
```json
{
  "hasConfiguration": false,
  "tableExists": false,
  "requiresMigration": true,
  "message": "No se puede conectar a la base de datos",
  "data": null
}
```

#### Caso 2: Tablas no existen (requiere migración)
```json
{
  "hasConfiguration": false,
  "tableExists": false,
  "requiresMigration": true,
  "message": "La tabla ConfigSys no existe. Se requiere ejecutar migraciones.",
  "data": null
}
```

#### Caso 3: Tabla existe pero sin configuración
```json
{
  "hasConfiguration": false,
  "tableExists": true,
  "requiresMigration": false,
  "message": "La tabla ConfigSys existe pero no hay configuración inicial del sistema",
  "data": null
}
```

#### Caso 4: Configuración existente
```json
{
  "hasConfiguration": true,
  "tableExists": true,
  "requiresMigration": false,
  "message": "Configuración inicial encontrada y tabla ConfigSys existe",
  "data": {
    "idConfigSys": 1,
    "companyName": "Mi Empresa",
    "branchName": "Sucursal Principal",
    "logoUrl": "https://example.com/logo.png",
    "colorsCount": 3,
    "notUseModulesCount": 1,
    "createdAt": "2025-11-05T10:00:00Z"
  }
}
```

---

## 2. Configurar Sistema

**Endpoint:** `POST /api/system/config/configure-system`

### Descripción
Crea la configuración inicial del sistema. Solo se puede ejecutar una vez.

### Request Body
```json
{
  "companyName": "Mi Empresa S.A.",
  "branchName": "Sucursal Principal",
  "logoUrl": "https://example.com/logo.png",
  "colors": [
    {
      "index": 0,
      "nameColor": "Primario",
      "colorCode": "#007bff",
      "colorRgb": "0, 123, 255"
    },
    {
      "index": 1,
      "nameColor": "Secundario",
      "colorCode": "#6c757d",
      "colorRgb": "108, 117, 125"
    }
  ],
  "notUseModules": [5, 6]
}
```

### Campos

- **companyName** (string, requerido): Nombre de la empresa
- **branchName** (string, requerido): Nombre de la sucursal
- **logoUrl** (string, requerido): URL del logo de la empresa
- **colors** (array, opcional): Colores personalizados del sistema
  - **index** (int): Índice del color
  - **nameColor** (string): Nombre descriptivo
  - **colorCode** (string): Código hexadecimal
  - **colorRgb** (string): Valores RGB
- **notUseModules** (array de int, opcional): Índices de módulos que NO se usarán

### Módulos del Sistema (SystemModule Enum)
```
0 = General
1 = Common
2 = System
3 = Sales
4 = Projects
5 = Inventory
6 = Shopping
```

### Respuesta Exitosa (200)
```json
{
  "success": true,
  "message": "Configuración del sistema creada exitosamente",
  "data": {
    "idConfigSys": 1,
    "companyName": "Mi Empresa S.A.",
    "branchName": "Sucursal Principal",
    "logoUrl": "https://example.com/logo.png",
    "colorsCount": 2,
    "notUseModulesCount": 2,
    "createdAt": "2025-11-05T10:00:00Z"
  }
}
```

### Respuesta Error (400)
```json
{
  "success": false,
  "message": "Ya existe una configuración del sistema. Use el endpoint de actualización."
}
```

---

## 3. Configurar Usuario Administrador

**Endpoint:** `POST /api/system/config/configure-admin`

### Descripción
Crea el usuario administrador inicial del sistema con acceso completo a todos los módulos. Solo se puede ejecutar una vez y requiere que la configuración del sistema ya exista.

### Request Body
```json
{
  "userName": "admin",
  "password": "Admin123!"
}
```

### Campos

- **userName** (string, requerido): Nombre de usuario para el administrador
- **password** (string, requerido): Contraseña para el administrador

### Configuración Automática

El endpoint crea automáticamente:

1. **Área de Sistema**
   - Name: "Sistema"
   - NormalizedName: "SYSTEM"

2. **Rol de Administrador**
   - Name: "Administrador"
   - NormalizedName: "admin"
   - Permisos:
     ```json
     {
       "index": 0,
       "name": "Acceso Total",
       "normalized": "FULL_ACCESS",
       "type": "SUPER_ADMIN",
       "status": "active"
     }
     ```
   - Módulos: Todos los módulos del enum SystemModule (0-6)

3. **Usuario Administrador**
   - UserName: (el proporcionado)
   - Name: "Administrador"
   - LastName: "Sistema"
   - Status: "active"
   - Verified: "true"
   - HashPassword: (generado automáticamente usando PBKDF2)

### Respuesta Exitosa (200)
```json
{
  "success": true,
  "message": "Usuario administrador creado exitosamente",
  "data": {
    "idUser": 1,
    "userName": "admin",
    "role": {
      "idRol": 1,
      "name": "Administrador",
      "normalizedName": "admin",
      "permissionsCount": 1,
      "modulesCount": 7
    },
    "area": {
      "idArea": 1,
      "name": "Sistema",
      "normalizedName": "SYSTEM"
    }
  }
}
```

### Respuestas de Error

#### Sin configuración del sistema (400)
```json
{
  "success": false,
  "message": "Debe configurar el sistema primero antes de crear el usuario administrador."
}
```

#### Administrador ya existe (400)
```json
{
  "success": false,
  "message": "Ya existe un usuario administrador configurado."
}
```

---

## Flujo de Configuración Inicial

1. **Verificar estado**: `GET /api/system/config/check-initial-config`
   - Si `requiresMigration: true`, ejecutar migraciones primero
   
2. **Configurar sistema**: `POST /api/system/config/configure-system`
   - Proporcionar datos de la empresa y configuración

3. **Crear administrador**: `POST /api/system/config/configure-admin`
   - Proporcionar usuario y contraseña

4. **Verificar nuevamente**: `GET /api/system/config/check-initial-config`
   - Debe retornar `hasConfiguration: true`

---

## Ejemplo de Uso Completo

### Paso 1: Verificar
```bash
curl -X GET http://localhost:5000/api/system/config/check-initial-config
```

### Paso 2: Configurar Sistema
```bash
curl -X POST http://localhost:5000/api/system/config/configure-system \
  -H "Content-Type: application/json" \
  -d '{
    "companyName": "AVASphere Corp",
    "branchName": "Oficina Central",
    "logoUrl": "https://avasphere.com/logo.png",
    "colors": [
      {
        "index": 0,
        "nameColor": "Principal",
        "colorCode": "#1976d2",
        "colorRgb": "25, 118, 210"
      }
    ],
    "notUseModules": []
  }'
```

### Paso 3: Crear Administrador
```bash
curl -X POST http://localhost:5000/api/system/config/configure-admin \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "admin",
    "password": "SecurePassword123!"
  }'
```

---

## Notas Importantes

- Los endpoints de configuración solo pueden ejecutarse una vez
- El usuario administrador tiene acceso completo a todos los módulos
- La contraseña se almacena hasheada usando PBKDF2 con salt aleatorio
- El usuario administrador se vincula automáticamente a la configuración del sistema
- No se puede eliminar el área o rol de sistema una vez creados


