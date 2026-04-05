@echo off
setlocal enabledelayedexpansion

REM KSA-Bridge Setup Script for Windows
REM This script verifies prerequisites, builds the mod, and deploys it to KSA

color 0F
cls

echo.
echo ========================================
echo   KSA-Bridge Setup
echo ========================================
echo.

REM Check for .NET SDK
echo [1/5] Checking .NET 10.0 SDK...
dotnet --version >nul 2>&1
if %ERRORLEVEL% neq 0 (
    color 0C
    echo ERROR: .NET SDK not found!
    echo.
    echo Download from: https://dotnet.microsoft.com/download/dotnet
    echo Choose: ".NET 10.0 SDK"
    echo.
    echo After installing, restart this script.
    pause
    exit /b 1
)
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo   ✓ Found .NET !DOTNET_VERSION!
echo.

REM Check for Mosquitto
echo [2/5] Checking Mosquitto MQTT Broker...
tasklist /FI "IMAGENAME eq mosquitto.exe" 2>nul | find /I /N "mosquitto.exe" >nul
if %ERRORLEVEL% neq 0 (
    echo.
    if exist "C:\Program Files\Mosquitto\mosquitto.exe" (
        echo   ! Mosquitto installed but not running
        echo   Starting Mosquitto...
        start "" "C:\Program Files\Mosquitto\mosquitto.exe" -c "config\mosquitto.conf"
        timeout /t 3 /nobreak
    ) else if exist "C:\Program Files (x86)\Mosquitto\mosquitto.exe" (
        echo   ! Mosquitto installed but not running
        echo   Starting Mosquitto...
        start "" "C:\Program Files (x86)\Mosquitto\mosquitto.exe" -c "config\mosquitto.conf"
        timeout /t 3 /nobreak
    ) else (
        color 0C
        echo ERROR: Mosquitto not found!
        echo.
        echo Download from: https://mosquitto.org/download/
        echo Choose: Windows installer
        echo.
        echo During installation, choose:
        echo   [X] Install as Windows Service
        echo.
        echo After installing, restart this script.
        pause
        exit /b 1
    )
)
echo   ✓ Mosquitto is running
echo.

REM Verify Mosquitto is listening
echo [3/5] Verifying Mosquitto is listening on ports 1884 and 9001...
netstat -ano | find "1884" >nul
if %ERRORLEVEL% neq 0 (
    color 0C
    echo ERROR: Mosquitto not listening on port 1884!
    echo.
    echo Try:
    echo   1. Restart Mosquitto
    echo   2. Check config\mosquitto.conf for port settings
    echo   3. Verify no other service is using port 1884
    echo.
    pause
    exit /b 1
)
echo   ✓ Mosquitto listening on required ports
echo.

REM Build the mod
echo [4/5] Building KSA-Bridge mod...
cd KSA-Bridge
dotnet build --configuration Release
if %ERRORLEVEL% neq 0 (
    color 0C
    echo ERROR: Build failed!
    echo.
    echo Check errors above. Common causes:
    echo   - Missing .NET dependencies
    echo   - NuGet package download failed
    echo.
    pause
    exit /b 1
)
cd ..
echo   ✓ Build complete
echo.

REM Deploy to KSA mods directory
echo [5/5] Deploying mod to KSA...

REM Try OneDrive path first (Windows 11 default)
set MODS_PATH=%USERPROFILE%\OneDrive\Documents\My Games\Kitten Space Agency\mods\KSA-Bridge
if not exist "%MODS_PATH%" (
    REM Fall back to standard Documents path
    set MODS_PATH=%USERPROFILE%\Documents\My Games\Kitten Space Agency\mods\KSA-Bridge
)

if not exist "%MODS_PATH%" (
    color 0C
    echo ERROR: KSA mods directory not found!
    echo.
    echo Expected one of:
    echo   %USERPROFILE%\OneDrive\Documents\My Games\Kitten Space Agency\mods\KSA-Bridge
    echo   %USERPROFILE%\Documents\My Games\Kitten Space Agency\mods\KSA-Bridge
    echo.
    echo Edit this script and set MODS_PATH to your KSA location.
    echo.
    pause
    exit /b 1
)

REM Deploy files
xcopy "KSA-Bridge\bin\Release\net10.0\*" "%MODS_PATH%\" /E /Y >nul
copy "KSA-Bridge\mod.toml" "%MODS_PATH%\mod.toml" /Y >nul
copy "KSA-Bridge\ksa-bridge.toml" "%MODS_PATH%\ksa-bridge.toml" /Y >nul

echo   ✓ Mod deployed to: !MODS_PATH!
echo.

REM Success
color 0A
echo ========================================
echo   ✓ Setup Complete!
echo ========================================
echo.
echo Next steps:
echo.
echo 1. Launch KSA:
echo    C:\Program Files\StarMap\launch-starmap.bat
echo.
echo 2. In a new terminal, start the web console:
echo    .\scripts\serve-examples.bat
echo.
echo 3. Open in your browser:
echo    http://localhost:8088/hard-scifi/hardscifi-fdo-console.html
echo.
echo 4. Fly a mission in KSA to see telemetry!
echo.
echo See SETUP.md for detailed troubleshooting.
echo.
pause
