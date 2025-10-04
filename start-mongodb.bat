@echo off
echo Iniciando MongoDB con Docker...
echo ================================

REM Verificar si Docker está corriendo
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Docker no está instalado o no está corriendo.
    echo Por favor instala Docker Desktop desde: https://www.docker.com/products/docker-desktop
    pause
    exit /b 1
)

echo Docker detectado correctamente.

REM Parar contenedores existentes si los hay
echo Parando contenedores existentes...
docker-compose down

REM Iniciar MongoDB
echo Iniciando MongoDB...
docker-compose up -d

REM Verificar estado
echo Verificando estado de MongoDB...
timeout /t 5 /nobreak >nul
docker-compose ps

echo.
echo MongoDB iniciado correctamente!
echo Conexión: mongodb://admin:admin123@localhost:27017
echo.
echo Para parar MongoDB, ejecuta: docker-compose down
echo Para ver logs: docker-compose logs mongodb
echo.
pause
