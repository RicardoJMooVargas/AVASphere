# Documentacion de Endpoints - Product Controller

## **Descripcion General**
Controlador para la gestion de productos. Permite crear, consultar, actualizar, eliminar, importar desde Excel y administrar imagenes de productos. Incluye filtros avanzados y endpoints de validacion de herrajes.

---

## **POST /api/common/Product/CreateProducts**

### **Descripcion**
Crea un nuevo producto.

### **Cuerpo de la peticion**
```json
{
	"mainName": "PANEL ACRILICO 3MM",
	"supplierName": "ACME",
	"unit": "PZA",
	"description": "Panel acrilico transparente",
	"quantity": 10,
	"taxes": 0,
	"idSupplier": 1,
	"imageUrls": [
		{
			"index": 0,
			"url": "https://cdn.example.com/products/panel-3mm.png",
			"fileName": "panel-3mm.png",
			"contentType": "image/png",
			"isMain": true
		}
	],
	"codeJson": [
		{ "index": 0, "type": "Principal", "code": "PANEL-3MM" }
	],
	"costsJson": [
		{ "index": 0, "amount": 120.5, "type": "Base" }
	],
	"categoriesJsons": [
		{ "index": 0, "name": "ACRILICOS", "normalizedName": "acrilicos" }
	],
	"solutionsJsons": [
		{ "index": 0, "name": "CERRAMIENTOS", "normalizedName": "cerramientos" }
	]
}
```

### **Validaciones**
- `mainName`, `supplierName`, `unit`, `description`: requeridos (si llegan vacios, el servicio puede rechazar la creacion)
- `idSupplier`: requerido, debe existir

### **Respuesta exitosa - 201 Created**
```json
{
	"success": true,
	"message": "Producto creado exitosamente",
	"data": {
		"idProduct": 101,
		"mainName": "PANEL ACRILICO 3MM",
		"supplierName": "ACME",
		"unit": "PZA",
		"description": "Panel acrilico transparente",
		"quantity": 10,
		"taxes": 0,
		"idSupplier": 1,
		"imageUrls": [
			{
				"index": 0,
				"url": "https://cdn.example.com/products/panel-3mm.png",
				"fileName": "panel-3mm.png",
				"contentType": "image/png",
				"isMain": true
			}
		],
		"codeJson": [
			{ "index": 0, "type": "Principal", "code": "PANEL-3MM" }
		],
		"costsJson": [
			{ "index": 0, "amount": 120.5, "type": "Base" }
		],
		"categoriesJsons": [
			{ "index": 0, "name": "ACRILICOS", "normalizedName": "acrilicos" }
		],
		"solutionsJsons": [
			{ "index": 0, "name": "CERRAMIENTOS", "normalizedName": "cerramientos" }
		],
		"productProperties": []
	},
	"statusCode": 201,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **404 Not Found - Proveedor no existe**
```json
{
	"success": false,
	"message": "El proveedor con ID 999 no existe.",
	"data": null,
	"statusCode": 404,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

---

## **POST /api/common/Product/CreateMultipleProducts**

### **Descripcion**
Crea multiples productos en una sola llamada.

### **Cuerpo de la peticion**
```json
[
	{
		"mainName": "PANEL ACRILICO 3MM",
		"supplierName": "ACME",
		"unit": "PZA",
		"description": "Panel acrilico transparente",
		"quantity": 10,
		"taxes": 0,
		"idSupplier": 1
	},
	{
		"mainName": "PERFIL ALUMINIO",
		"supplierName": "ACME",
		"unit": "PZA",
		"description": "Perfil de aluminio",
		"quantity": 20,
		"taxes": 0,
		"idSupplier": 1
	}
]
```

### **Respuesta exitosa - 200 OK**
```json
{
	"success": true,
	"message": "2 productos creados exitosamente",
	"data": [
		{ "idProduct": 101, "mainName": "PANEL ACRILICO 3MM" },
		{ "idProduct": 102, "mainName": "PERFIL ALUMINIO" }
	],
	"statusCode": 201,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

---

## **GET /api/common/Product/GetProduct**

### **Descripcion**
Obtiene un producto por ID o devuelve todos los productos con filtros y paginacion.

### **Parametros de consulta**
| Parametro | Tipo | Requerido | Descripcion |
|-----------|------|-----------|-------------|
| `id` | `int?` | No | ID del producto. Si no se envia, devuelve listado paginado |
| `mainName` | `string?` | No | Filtro por nombre del producto |
| `idSupplier` | `int?` | No | Filtro por ID del proveedor |
| `supplierName` | `string?` | No | Filtro por nombre del proveedor |
| `idProperty` | `int?` | No | Filtro por ID de propiedad (1=Familia, 2=Clase, 3=Linea) |
| `propertyName` | `string?` | No | Filtro por nombre de propiedad (Familia, Clase, Linea) |
| `idPropertyValue` | `int?` | No | Filtro por ID de valor de propiedad |
| `propertyValue` | `string?` | No | Filtro por valor de propiedad |
| `pageNumber` | `int` | No | Numero de pagina (base 1, por defecto 1) |
| `pageSize` | `int` | No | Tamano de pagina (por defecto 20) |

### **Ejemplos de uso**

#### 1. Obtener listado paginado
```
GET /api/common/Product/GetProduct?pageNumber=1&pageSize=20
```

#### 2. Buscar por ID
```
GET /api/common/Product/GetProduct?id=101
```

#### 3. Buscar por filtros
```
GET /api/common/Product/GetProduct?supplierName=ACME&propertyName=Familia&propertyValue=ACRILICOS
```

### **Respuesta exitosa - 200 OK (paginado)**
```json
{
	"success": true,
	"message": "Productos obtenidos exitosamente",
	"data": {
		"items": [
			{
				"idProduct": 101,
				"mainName": "PANEL ACRILICO 3MM",
				"supplierName": "ACME",
				"unit": "PZA",
				"description": "Panel acrilico transparente",
				"quantity": 10,
				"taxes": 0,
				"idSupplier": 1,
				"imageUrls": [],
				"codeJson": [
					{ "index": 0, "type": "Principal", "code": "PANEL-3MM" }
				],
				"costsJson": [],
				"categoriesJsons": [],
				"solutionsJsons": [],
				"productProperties": []
			}
		],
		"pageNumber": 1,
		"pageSize": 20,
		"totalCount": 120,
		"totalPages": 6,
		"hasPreviousPage": false,
		"hasNextPage": true
	},
	"statusCode": 200,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

### **Respuesta exitosa - 200 OK (producto por ID)**
```json
{
	"success": true,
	"message": "Producto obtenido exitosamente",
	"data": {
		"idProduct": 101,
		"mainName": "PANEL ACRILICO 3MM",
		"supplierName": "ACME",
		"unit": "PZA",
		"description": "Panel acrilico transparente",
		"quantity": 10,
		"taxes": 0,
		"idSupplier": 1,
		"imageUrls": [],
		"codeJson": [
			{ "index": 0, "type": "Principal", "code": "PANEL-3MM" }
		],
		"costsJson": [],
		"categoriesJsons": [],
		"solutionsJsons": [],
		"productProperties": []
	},
	"statusCode": 200,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **404 Not Found - Producto no encontrado**
```json
{
	"success": false,
	"message": "Producto con ID 999 no encontrado o no cumple con los filtros",
	"data": null,
	"statusCode": 404,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

---

## **GET /api/common/Product/is-herraje**

### **Descripcion**
Indica si un producto es herraje segun su codigo principal o su `mainName`.

### **Parametros de consulta**
| Parametro | Tipo | Requerido | Descripcion |
|-----------|------|-----------|-------------|
| `code` | `string?` | No | Codigo principal del producto |
| `mainName` | `string?` | No | Nombre principal del producto |

### **Reglas**
- Debe enviar `code` o `mainName` (al menos uno).

### **Respuesta exitosa - 200 OK**
```json
{
	"success": true,
	"message": "Consulta realizada exitosamente",
	"data": {
		"code": "PANEL-3MM",
		"mainName": null,
		"isHerraje": false
	},
	"statusCode": 200,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **400 Bad Request - Parametros insuficientes**
```json
{
	"success": false,
	"message": "Debe proporcionar el codigo o el nombre del producto",
	"data": null,
	"statusCode": 400,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **404 Not Found - Producto no existe**
```json
{
	"success": false,
	"message": "Producto con codigo 'PANEL-3MM' no encontrado",
	"data": null,
	"statusCode": 404,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

---

## **POST /api/common/Product/is-herraje/batch**

### **Descripcion**
Indica que codigos corresponden a productos herrajes.

### **Cuerpo de la peticion**
```json
["PANEL-3MM", "BISAGRA-XL", "CERRADURA-001"]
```

### **Respuesta exitosa - 200 OK**
```json
{
	"success": true,
	"message": "Consulta realizada exitosamente",
	"data": [
		{ "code": "PANEL-3MM", "found": true, "isHerraje": false, "mainName": "PANEL ACRILICO 3MM" },
		{ "code": "BISAGRA-XL", "found": true, "isHerraje": true, "mainName": "BISAGRA XL" },
		{ "code": "CERRADURA-001", "found": false, "isHerraje": false, "mainName": null }
	],
	"statusCode": 200,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

---

## **PUT /api/common/Product/UpdateProduct/{id}**

### **Descripcion**
Actualiza un producto existente.

### **Parametros de ruta**
| Parametro | Tipo | Requerido | Descripcion |
|-----------|------|-----------|-------------|
| `id` | `int` | Si | ID del producto a actualizar |

### **Cuerpo de la peticion**
```json
{
	"mainName": "PANEL ACRILICO 4MM",
	"unit": "PZA",
	"description": "Panel acrilico transparente",
	"quantity": 15,
	"taxes": 0,
	"idSupplier": 1,
	"codeJson": [
		{ "index": 0, "type": "Principal", "code": "PANEL-4MM" }
	]
}
```

### **Respuesta exitosa - 200 OK**
```json
{
	"success": true,
	"message": "Producto actualizado exitosamente",
	"data": {
		"idProduct": 101,
		"mainName": "PANEL ACRILICO 4MM"
	},
	"statusCode": 200,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

#### **404 Not Found - Producto no existe**
```json
{
	"success": false,
	"message": "Producto con ID 999 no encontrado",
	"data": null,
	"statusCode": 404,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

---

## **DELETE /api/common/Product/DeleteProduct/{id}**

### **Descripcion**
Elimina un producto existente.

### **Parametros de ruta**
| Parametro | Tipo | Requerido | Descripcion |
|-----------|------|-----------|-------------|
| `id` | `int` | Si | ID del producto a eliminar |

### **Respuesta exitosa - 200 OK**
```json
{
	"success": true,
	"message": "Producto con ID 101 eliminado exitosamente",
	"data": null,
	"statusCode": 200,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

---

## **POST /api/common/Product/{id}/upload-image**

### **Descripcion**
Agrega una imagen al producto. Permite multiples imagenes.

### **Parametros de ruta**
| Parametro | Tipo | Requerido | Descripcion |
|-----------|------|-----------|-------------|
| `id` | `int` | Si | ID del producto |

### **Body (multipart/form-data)**
- `file`: archivo de imagen (`jpg`, `jpeg`, `png`, `gif`, `webp`)

### **Respuesta exitosa - 200 OK**
```json
{
	"success": true,
	"message": "Imagen agregada exitosamente",
	"data": {
		"imageUrl": "https://cdn.example.com/products/product_101_638857985000000000.png",
		"totalImages": 3
	},
	"statusCode": 200,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

---

## **DELETE /api/common/Product/{id}/delete-image**

### **Descripcion**
Elimina una imagen especifica del producto por URL.

### **Parametros**
| Parametro | Tipo | Requerido | Descripcion |
|-----------|------|-----------|-------------|
| `id` | `int` | Si | ID del producto |
| `imageUrl` | `string` | Si | URL de la imagen a eliminar (query param) |

### **Ejemplo**
```
DELETE /api/common/Product/101/delete-image?imageUrl=https://cdn.example.com/products/panel-3mm.png
```

### **Respuesta exitosa - 200 OK**
```json
{
	"success": true,
	"message": "Imagen eliminada exitosamente",
	"data": null,
	"statusCode": 200,
	"timestamp": "2026-01-02T17:26:48.006925Z"
}
```

---

## **POST /api/common/Product/import**

### **Descripcion**
Importa productos desde un archivo Excel.

### **Body (multipart/form-data)**
- `file`: archivo Excel (`.xlsx` o `.xls`)

### **Respuesta exitosa - 200 OK**
```json
{
	"totalRows": 100,
	"successfulImports": 98,
	"failedImports": 2,
	"errors": [
		"Fila 10: proveedor no existe",
		"Fila 55: mainName vacio"
	]
}
```

#### **400 Bad Request - Archivo invalido**
```json
"El archivo debe ser un Excel (.xlsx o .xls)"
```

---

## **Modelos de Datos (DTOs)**

### **CreateProductDto**
```json
{
	"mainName": "string",
	"supplierName": "string",
	"unit": "string",
	"description": "string",
	"quantity": 0,
	"taxes": 0,
	"idSupplier": 0,
	"imageUrls": [
		{ "index": 0, "url": "string", "fileName": "string", "contentType": "string", "isMain": true }
	],
	"codeJson": [ { "index": 0, "type": "string", "code": "string" } ],
	"costsJson": [ { "index": 0, "amount": 0, "type": "string" } ],
	"categoriesJsons": [ { "index": 0, "name": "string", "normalizedName": "string" } ],
	"solutionsJsons": [ { "index": 0, "name": "string", "normalizedName": "string" } ]
}
```

### **UpdateProductDto**
```json
{
	"mainName": "string",
	"unit": "string",
	"description": "string",
	"quantity": 0,
	"taxes": 0,
	"idSupplier": 0,
	"codeJson": [ { "index": 0, "type": "string", "code": "string" } ],
	"costsJson": [ { "index": 0, "amount": 0, "type": "string" } ],
	"categoriesJsons": [ { "index": 0, "name": "string", "normalizedName": "string" } ],
	"solutionsJsons": [ { "index": 0, "name": "string", "normalizedName": "string" } ]
}
```

### **ProductResponseDto**
```json
{
	"idProduct": 0,
	"mainName": "string",
	"supplierName": "string",
	"unit": "string",
	"description": "string",
	"quantity": 0,
	"taxes": 0,
	"idSupplier": 0,
	"imageUrls": [
		{ "index": 0, "url": "string", "fileName": "string", "contentType": "string", "isMain": true }
	],
	"codeJson": [ { "index": 0, "type": "string", "code": "string" } ],
	"costsJson": [ { "index": 0, "amount": 0, "type": "string" } ],
	"categoriesJsons": [ { "index": 0, "name": "string", "normalizedName": "string" } ],
	"solutionsJsons": [ { "index": 0, "name": "string", "normalizedName": "string" } ],
	"productProperties": [
		{
			"idProductProperties": 0,
			"customValue": "string",
			"idProduct": 0,
			"idPropertyValue": 0,
			"propertyValueName": "string",
			"propertyName": "string"
		}
	]
}
```

### **ImportProductResultDto**
```json
{
	"totalRows": 0,
	"successfulImports": 0,
	"failedImports": 0,
	"errors": ["string"]
}
```

---

## **Notas**
1. Las respuestas estandar usan `ApiResponse` con `success`, `message`, `data`, `statusCode` y `timestamp`.
2. El endpoint de importacion devuelve directamente `ImportProductResultDto` (sin wrapper `ApiResponse`).
3. La busqueda por ID y filtros es compatible: si `id` no se envia, la respuesta es paginada; si se envia, devuelve un objeto unico.
4. En `CreateMultipleProducts` el HTTP status es 200, pero `statusCode` en la respuesta es 201.
5. `imageUrls` contiene metadatos de imagen y se puede poblar por `upload-image`.
