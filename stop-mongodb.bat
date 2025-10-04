@echo off
echo Parando MongoDB...
echo ==================

REM Parar y remover contenedores
docker-compose down

echo.
echo MongoDB ha sido parado correctamente.
echo.
pause
