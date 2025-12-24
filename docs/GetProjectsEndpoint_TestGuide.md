# 🧪 Guía de Pruebas - Endpoint GET Principal de Projects

## 📋 Información General

**Endpoint:** `GET /api/projects/Project`  
**Propósito:** Obtener clientes con sus proyectos asociados  
**Fecha de implementación:** 24 de diciembre de 2024  
**Estado:** ✅ Implementado y listo para pruebas

---

## 🚀 Cómo Ejecutar las Pruebas

### 1. Iniciar la Aplicación

```powershell
cd "C:\Users\Angel Hidalgo\RiderProjects\AVASphere\src\AVASphere.WebApi"
dotnet run
```

La aplicación debería iniciar y mostrar la URL (generalmente `https://localhost:5001` o `http://localhost:5000`)

### 2. Acceder a Swagger UI

Abre tu navegador y ve a:
```
https://localhost:5001/swagger/index.html
```

O según el puerto que se muestre en la consola.

---

## 🧪 Casos de Prueba

### ✅ Prueba 1: Obtener Todos los Clientes con Proyectos

**Descripción:** Obtiene todos los clientes que tienen proyectos asociados.

**Método:** GET  
**URL:** `/api/projects/Project`  
**Query Parameters:** Ninguno

**Respuesta Esperada:**
```json
{
  "data": [
    {
      "idCustomer": 1,
      "externalId": 321,
      "name": "Juan Carlos",
      "lastName": "Lopez Portillo",
      "email": "juan.perez@example.com",
      "phoneNumber": "+54 999 0012 123",
      "taxId": "LOPJ850101ABC",
      "settingsCustomerJson": {
        "index": 1,
        "route": "Foraneo",
        "type": "Aluminiero",
        "discount": 5.0
      },
      "projects": [ /* array de proyectos */ ]
    }
  ],
  "message": "Customers with projects retrieved successfully",
  "statusCode": 200
}
```

**Código HTTP Esperado:** 200 OK

---

### ✅ Prueba 2: Filtrar por Cliente Específico

**Descripción:** Obtiene solo los proyectos de un cliente en particular.

**Método:** GET  
**URL:** `/api/projects/Project?idCustomer=1`  
**Query Parameters:** 
- `idCustomer=1`

**Respuesta Esperada:**
- Lista con un solo cliente (IdCustomer = 1)
- Todos los proyectos asociados a ese cliente

**Código HTTP Esperado:** 200 OK

---

### ✅ Prueba 3: Filtrar por Estado del Proyecto (Hito)

**Descripción:** Obtiene clientes con proyectos en un estado específico.

**Método:** GET  
**URL:** `/api/projects/Project?currentHito=Appointment`  
**Query Parameters:** 
- `currentHito=Appointment`

**Estados Disponibles:**
- `Appointment` (0)
- `Design` (1)
- `Production` (2)
- `Installation` (3)
- `Completed` (4)
- `Cancelled` (5)

**Respuesta Esperada:**
- Clientes que tienen al menos un proyecto en estado "Appointment"

**Código HTTP Esperado:** 200 OK

---

### ✅ Prueba 4: Filtrar por Categorías

**Descripción:** Obtiene proyectos que contengan al menos una de las categorías especificadas.

**Método:** GET  
**URL:** `/api/projects/Project?categoryIds=1&categoryIds=2`  
**Query Parameters:** 
- `categoryIds=1`
- `categoryIds=2`

**Respuesta Esperada:**
- Clientes con proyectos que tengan categoría ID 1 O categoría ID 2

**Código HTTP Esperado:** 200 OK

---

### ✅ Prueba 5: Filtros Combinados

**Descripción:** Combina múltiples filtros para una búsqueda más específica.

**Método:** GET  
**URL:** `/api/projects/Project?idCustomer=1&currentHito=Design&categoryIds=1`  
**Query Parameters:** 
- `idCustomer=1`
- `currentHito=Design`
- `categoryIds=1`

**Respuesta Esperada:**
- Cliente con ID 1
- Solo proyectos en estado "Design"
- Que tengan la categoría ID 1

**Código HTTP Esperado:** 200 OK

---

### ✅ Prueba 6: Sin Resultados

**Descripción:** Filtros que no coinciden con ningún registro.

**Método:** GET  
**URL:** `/api/projects/Project?idCustomer=99999`  
**Query Parameters:** 
- `idCustomer=99999` (cliente inexistente)

**Respuesta Esperada:**
```json
{
  "data": [],
  "message": "Customers with projects retrieved successfully",
  "statusCode": 200
}
```

**Código HTTP Esperado:** 200 OK (array vacío)

---

## 🔍 Verificaciones en la Respuesta

Para cada prueba exitosa, verifica que la respuesta contenga:

### ✅ Estructura del Cliente
- ✅ `idCustomer` (número)
- ✅ `externalId` (número)
- ✅ `name` (string)
- ✅ `lastName` (string)
- ✅ `email` (string)
- ✅ `phoneNumber` (string)
- ✅ `taxId` (string)
- ✅ `settingsCustomerJson` (objeto con index, route, type, discount)
- ✅ `projects` (array)

### ✅ Estructura del Proyecto
- ✅ `idProject` (número)
- ✅ `currentHito` (string: Appointment, Design, etc.)
- ✅ `writtenAddress` (string)
- ✅ `exactAddress` (string)
- ✅ `appointmentJson` (objeto con datos de la cita)
- ✅ `visitsJson` (array de visitas)
- ✅ `projectQuote` (objeto con totales)
- ✅ `listOfCategories` (array de categorías)

### ✅ Estructura de ProjectQuote
- ✅ `idProjectQuotes` (número)
- ✅ `grandTotal` (número decimal)
- ✅ `totalTaxes` (número decimal)

### ✅ Estructura de Category
- ✅ `idListOfCategories` (número)
- ✅ `projectCategory` (objeto)
  - ✅ `idProjectCategory` (número)
  - ✅ `name` (string)
  - ✅ `normalizedName` (string)

---

## 🐛 Pruebas de Manejo de Errores

### Prueba 7: Error 500 Simulado

Si hay un error interno del servidor (por ejemplo, base de datos no disponible), deberías recibir:

```json
{
  "data": null,
  "message": "Internal server error: [mensaje del error]",
  "statusCode": 500
}
```

**Código HTTP Esperado:** 500 Internal Server Error

---

## 📊 Usando Postman

### Colección de Postman

Puedes crear una colección con estas pruebas:

```json
{
  "info": {
    "name": "AVASphere - Projects GET",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Get All Customers with Projects",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "{{baseUrl}}/api/projects/Project",
          "host": ["{{baseUrl}}"],
          "path": ["api", "projects", "Project"]
        }
      }
    },
    {
      "name": "Get by Customer ID",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "{{baseUrl}}/api/projects/Project?idCustomer=1",
          "host": ["{{baseUrl}}"],
          "path": ["api", "projects", "Project"],
          "query": [
            {
              "key": "idCustomer",
              "value": "1"
            }
          ]
        }
      }
    },
    {
      "name": "Get by Hito",
      "request": {
        "method": "GET",
        "header": [],
        "url": {
          "raw": "{{baseUrl}}/api/projects/Project?currentHito=Appointment",
          "host": ["{{baseUrl}}"],
          "path": ["api", "projects", "Project"],
          "query": [
            {
              "key": "currentHito",
              "value": "Appointment"
            }
          ]
        }
      }
    }
  ],
  "variable": [
    {
      "key": "baseUrl",
      "value": "https://localhost:5001"
    }
  ]
}
```

---

## 🧪 Usando cURL

### Ejemplo 1: Todos los clientes
```bash
curl -X GET "https://localhost:5001/api/projects/Project" -H "accept: application/json" -k
```

### Ejemplo 2: Por cliente
```bash
curl -X GET "https://localhost:5001/api/projects/Project?idCustomer=1" -H "accept: application/json" -k
```

### Ejemplo 3: Por estado
```bash
curl -X GET "https://localhost:5001/api/projects/Project?currentHito=Appointment" -H "accept: application/json" -k
```

### Ejemplo 4: Por categorías
```bash
curl -X GET "https://localhost:5001/api/projects/Project?categoryIds=1&categoryIds=2" -H "accept: application/json" -k
```

**Nota:** El flag `-k` es para aceptar certificados SSL autofirmados en desarrollo.

---

## ✅ Checklist de Verificación Final

Antes de considerar las pruebas como exitosas, verifica:

- [ ] La aplicación compila sin errores
- [ ] La aplicación inicia correctamente
- [ ] Swagger UI carga y muestra el endpoint
- [ ] El endpoint GET /api/projects/Project aparece en Swagger
- [ ] La documentación del endpoint es visible en Swagger
- [ ] Prueba 1 (sin filtros) funciona correctamente
- [ ] Prueba 2 (filtro por cliente) funciona correctamente
- [ ] Prueba 3 (filtro por hito) funciona correctamente
- [ ] Prueba 4 (filtro por categorías) funciona correctamente
- [ ] Prueba 5 (filtros combinados) funciona correctamente
- [ ] Prueba 6 (sin resultados) devuelve array vacío
- [ ] La estructura JSON de respuesta es correcta
- [ ] Los datos están agrupados por cliente
- [ ] Las relaciones (cotizaciones, categorías) se cargan correctamente

---

## 🔧 Solución de Problemas

### Problema: No se cargan las categorías

**Causa:** Puede que no haya registros en `ListOfCategories` o `ProjectCategory`.

**Solución:** Verifica que existan datos en estas tablas:
```sql
SELECT * FROM ListOfCategories;
SELECT * FROM ProjectCategories;
```

### Problema: No se cargan las cotizaciones

**Causa:** Proyectos sin cotización asociada (`IdProjectQuote = 0` o inexistente).

**Solución:** Normal, algunos proyectos pueden no tener cotización aún. El campo `projectQuote` será `null`.

### Problema: SettingsCustomerJson es null

**Causa:** Cliente sin configuraciones JSON.

**Solución:** Normal, es un campo opcional.

### Problema: Error 500 al ejecutar

**Causa:** Posible error de conexión a base de datos o datos inconsistentes.

**Solución:** 
1. Verifica la cadena de conexión en `appsettings.json`
2. Revisa los logs de la aplicación
3. Verifica que la base de datos esté disponible

---

## 📝 Resultados Esperados

Si todo funciona correctamente, deberías ver:

✅ **Rendimiento:** Respuestas en menos de 1 segundo (depende del volumen de datos)  
✅ **Estructura:** JSON bien formado con jerarquía Cliente → Proyectos → Detalles  
✅ **Filtros:** Aplicados correctamente según los parámetros  
✅ **Datos completos:** Todas las relaciones cargadas (Customer, ProjectQuote, Categories)  
✅ **Sin errores:** Código 200 para consultas exitosas  

---

## 🎯 Próximos Pasos Después de las Pruebas

Una vez que las pruebas sean exitosas:

1. ✅ Documentar cualquier comportamiento inesperado
2. ✅ Considerar agregar paginación si hay muchos registros
3. ✅ Implementar caché si las consultas son frecuentes
4. ✅ Agregar más filtros si es necesario (fechas, búsqueda por texto)
5. ✅ Implementar autorización/permisos según los roles de usuario

---

**Fecha de creación:** 24 de diciembre de 2024  
**Última actualización:** 24 de diciembre de 2024  
**Estado:** ✅ Listo para pruebas

