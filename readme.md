# AVASphere

## Configuración de MongoDB con Docker

Este proyecto incluye un Dockerfile para MongoDB que facilita la configuración de la base de datos.

### Prerequisitos
- Docker instalado en tu sistema
- Docker Compose (opcional, para gestión más avanzada)

### Ejecutar MongoDB

#### Opción 1: Construir y ejecutar el contenedor

1. **Construir la imagen de MongoDB:**
   ```bash
   docker build -f dockerfile.mongodb -t vyaa-mongodb .
   ```

2. **Ejecutar el contenedor:**
   ```bash
   docker run -d --name vyaa-mongo -p 27017:27017 vyaa-mongodb
   ```

#### Opción 2: Ejecutar directamente (recomendado)

```bash
docker run -d --name vyaa-mongo -p 27017:27017 -e MONGO_INITDB_ROOT_USERNAME=admin -e MONGO_INITDB_ROOT_PASSWORD=admin123 mongo:latest
```

### Credenciales de acceso

- **Usuario:** admin
- **Contraseña:** admin123
- **Puerto:** 27017
- **Host:** localhost

### Comandos útiles

- **Ver contenedores en ejecución:**
  ```bash
  docker ps
  ```

- **Detener el contenedor:**
  ```bash
  docker stop vyaa-mongo
  ```

- **Iniciar el contenedor detenido:**
  ```bash
  docker start vyaa-mongo
  ```

- **Eliminar el contenedor:**
  ```bash
  docker rm vyaa-mongo
  ```

- **Ver logs del contenedor:**
  ```bash
  docker logs vyaa-mongo
  ```

### Conectar a MongoDB

Puedes conectarte a MongoDB usando:

1. **MongoDB Compass:** `mongodb://admin:admin123@localhost:27017`
2. **Línea de comandos:**
   ```bash
   docker exec -it vyaa-mongo mongosh -u admin -p admin123
   ```

### Persistencia de datos (opcional)

Para mantener los datos después de eliminar el contenedor, puedes usar un volumen:

```bash
docker run -d --name vyaa-mongo -p 27017:27017 -v mongodb_data:/data/db -e MONGO_INITDB_ROOT_USERNAME=admin -e MONGO_INITDB_ROOT_PASSWORD=admin123 mongo:latest
```

## Ejecutar la aplicación

### Prerequisitos
1. Tener MongoDB ejecutándose (usar las instrucciones anteriores)
2. .NET 9.0 SDK instalado

### Pasos para ejecutar

1. **Restaurar paquetes NuGet:**
   ```bash
   dotnet restore
   ```

2. **Compilar la solución:**
   ```bash
   dotnet build
   ```

3. **Ejecutar la aplicación:**
   ```bash
   cd src/VYAACentralInforApi.WebApi
   dotnet run
   ```

4. **Acceder a la API:**
   - La aplicación estará disponible en: `https://localhost:7071` o `http://localhost:5071`
   - **Documentación Swagger UI:** `https://localhost:7071/swagger` o `http://localhost:5071/swagger`
   - Documentación OpenAPI JSON: `https://localhost:7071/swagger/v1/swagger.json`

### Endpoints disponibles

#### Módulo System - Usuarios

- **GET** `/api/system/users` - Obtener todos los usuarios
- **GET** `/api/system/users/{id}` - Obtener usuario por ID
- **GET** `/api/system/users/by-username/{username}` - Obtener usuario por nombre de usuario

### Usuario por defecto

Al iniciar la aplicación por primera vez, se creará automáticamente un usuario administrador:

- **Usuario:** admin
- **Contraseña:** admin123
- **Rol:** Admin

### Estructura del proyecto

El proyecto sigue Clean Architecture con los siguientes módulos:

- **Domain:** Entidades y interfaces (reglas de negocio)
- **Application:** Servicios de aplicación y casos de uso
- **Infrastructure:** Implementación de repositorios y conexión a MongoDB
- **WebApi:** Controladores y configuración de la API

### Módulos implementados

- **System:** Gestión de usuarios y configuración del sistema
