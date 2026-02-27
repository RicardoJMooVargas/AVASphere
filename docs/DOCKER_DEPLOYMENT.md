# AVASphere Backend Docker Deployment

## Recomendación para Dockploy
Para Dockploy no necesitas `docker-compose`. La forma recomendada es:
- Usar solo `src/AVASphere.WebApi/Dockerfile`
- Configurar todas las variables en la sección **Environment** de Dockploy

## Variable para cambiar entorno
- `ASPNETCORE_ENVIRONMENT=Development` para desarrollo
- `ASPNETCORE_ENVIRONMENT=Production` para producción

## Variables mínimas en Dockploy
- `ASPNETCORE_ENVIRONMENT`
- `POSTGRES_CONNECTION_STRING`
- `JWT_KEY`
- `JWT_ISSUER`
- `JWT_AUDIENCE`
- `JWT_EXPIRATION_MINUTES`
- `DEFAULT_USER_USERNAME`
- `DEFAULT_USER_NAME`
- `DEFAULT_USER_LASTNAME`
- `DEFAULT_USER_PASSWORD`
- `DEFAULT_USER_ROLE`

## Ejemplo Development (DB externa)
```env
ASPNETCORE_ENVIRONMENT=Development
POSTGRES_CONNECTION_STRING=Host=YOUR_DEV_DB_HOST;Port=5432;Database=avaspheredbtest;Username=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;
JWT_KEY=YOUR_SECRET
JWT_ISSUER=VYAACentralInforApi
JWT_AUDIENCE=VYAACentralInforApiUsers
JWT_EXPIRATION_MINUTES=60
DEFAULT_USER_USERNAME=admin
DEFAULT_USER_NAME=Administrator
DEFAULT_USER_LASTNAME=System
DEFAULT_USER_PASSWORD=admin123
DEFAULT_USER_ROLE=Admin
```

## Ejemplo Production (DB externa)
```env
ASPNETCORE_ENVIRONMENT=Production
POSTGRES_CONNECTION_STRING=Host=YOUR_PROD_DB_HOST;Port=5432;Database=avaspheredb;Username=YOUR_DB_USER;Password=YOUR_DB_PASSWORD;
JWT_KEY=YOUR_SECRET
JWT_ISSUER=VYAACentralInforApi
JWT_AUDIENCE=VYAACentralInforApiUsers
JWT_EXPIRATION_MINUTES=60
DEFAULT_USER_USERNAME=admin
DEFAULT_USER_NAME=Administrator
DEFAULT_USER_LASTNAME=System
DEFAULT_USER_PASSWORD=CHANGE_ME
DEFAULT_USER_ROLE=Admin
```

## Prioridad de configuración DB
1. `POSTGRES_CONNECTION_STRING` (Environment en Dockploy)
2. `DbSettings:ConnectionString` en `appsettings.{Environment}.json`
3. fallback interno (`avaspheredbtest`)

## Nota sobre `.env`
El archivo `.env` queda solo para ejecución local. Si Dockploy inyecta variables, esas tienen prioridad y no serán sobrescritas.
