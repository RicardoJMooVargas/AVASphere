@echo off
setlocal enabledelayedexpansion
echo Eliminando el seguimiento de archivos que están en .gitignore pero aún siendo rastreados...
echo.

cd /d "%~dp0"

echo Verificando si hay cambios staged...
git status --porcelain | findstr /r "^[MADRC]" >nul
if %errorlevel% == 0 (
    echo Se encontraron archivos con cambios staged.
    echo.
    echo Opciones disponibles:
    echo 1 - Hacer commit de los cambios pendientes
    echo 2 - Resetear los cambios staged  
    echo 3 - Forzar la limpieza del cache
    echo.
    set /p choice="Selecciona una opción (1/2/3): "
    
    if "!choice!"=="1" (
        echo Haciendo commit de cambios pendientes...
        git commit -m "Commit automático antes de limpiar archivos ignorados"
        echo Eliminando archivos del cache de Git...
        git rm -r --cached .
    ) else if "!choice!"=="2" (
        echo Reseteando cambios staged...
        git reset HEAD .
        echo Eliminando archivos del cache de Git...
        git rm -r --cached .
    ) else if "!choice!"=="3" (
        echo Forzando la limpieza del cache...
        git rm -r --cached . -f
    ) else (
        echo Opción no válida. Saliendo...
        pause
        exit /b 1
    )
) else (
    echo No hay cambios staged, procediendo...
    echo Eliminando archivos del cache de Git...
    git rm -r --cached .
)

echo.
echo Agregando todos los archivos de nuevo (respetando .gitignore)...
git add .

echo.
echo Verificando el estado...
git status

echo.
echo ¡Listo! Los archivos que están en .gitignore ya no serán rastreados.
echo Ahora puedes hacer commit de estos cambios:
echo git commit -m "Eliminar seguimiento de archivos ignorados"
echo.
pause
