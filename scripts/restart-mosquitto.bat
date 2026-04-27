@echo off
REM KSA-Bridge Mosquitto Restart Script
REM Cleanly restarts the Mosquitto MQTT broker

color 0F
cls

echo.
echo ========================================
echo   Restarting Mosquitto MQTT Broker
echo ========================================
echo.

echo Stopping Mosquitto...
taskkill /IM mosquitto.exe /F >nul 2>&1

echo Waiting 2 seconds...
timeout /t 2 /nobreak

echo Starting Mosquitto...
start "" "C:\Program Files\Mosquitto\mosquitto.exe" -c "%~dp0..\config\mosquitto.conf"

timeout /t 2 /nobreak

echo.
echo Verifying Mosquitto is running...
tasklist /FI "IMAGENAME eq mosquitto.exe" | find /I /N "mosquitto.exe" >nul
if %ERRORLEVEL% equ 0 (
    color 0A
    echo ✓ Mosquitto restarted successfully!
    echo.
    echo Remember to reload your web console in the browser (Ctrl+Shift+R)
) else (
    color 0C
    echo ERROR: Mosquitto failed to start!
    echo.
    echo Verify:
    echo   1. C:\Program Files\Mosquitto\mosquitto.exe exists
    echo   2. config\mosquitto.conf is accessible
    echo   3. No other process is using ports 1884/9001
)
echo.
pause
