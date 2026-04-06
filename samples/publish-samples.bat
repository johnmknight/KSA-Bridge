@echo off
REM -----------------------------------------------------------------------
REM publish-samples.bat
REM Publishes sample telemetry to the MQTT broker for console testing.
REM Requires: mosquitto_pub (included with Mosquitto installation)
REM Usage: publish-samples.bat [broker_host] [broker_port]
REM -----------------------------------------------------------------------

set HOST=%1
if "%HOST%"=="" set HOST=127.0.0.1
set PORT=%2
if "%PORT%"=="" set PORT=1884
set DIR=%~dp0

echo Publishing sample telemetry to %HOST%:%PORT% ...
echo.

mosquitto_pub -h %HOST% -p %PORT% -t ksa/telemetry/vehicle    -f "%DIR%vehicle.json"
echo   -^> ksa/telemetry/vehicle
mosquitto_pub -h %HOST% -p %PORT% -t ksa/telemetry/orbit      -f "%DIR%orbit.json"
echo   -^> ksa/telemetry/orbit
mosquitto_pub -h %HOST% -p %PORT% -t ksa/telemetry/velocity   -f "%DIR%velocity.json"
echo   -^> ksa/telemetry/velocity
mosquitto_pub -h %HOST% -p %PORT% -t ksa/telemetry/altitude   -f "%DIR%altitude.json"
echo   -^> ksa/telemetry/altitude
mosquitto_pub -h %HOST% -p %PORT% -t ksa/telemetry/attitude   -f "%DIR%attitude.json"
echo   -^> ksa/telemetry/attitude
mosquitto_pub -h %HOST% -p %PORT% -t ksa/telemetry/resources  -f "%DIR%resources.json"
echo   -^> ksa/telemetry/resources
mosquitto_pub -h %HOST% -p %PORT% -t ksa/telemetry/maneuver   -f "%DIR%maneuver.json"
echo   -^> ksa/telemetry/maneuver
mosquitto_pub -h %HOST% -p %PORT% -t ksa/telemetry/mission-time -f "%DIR%mission-time.json"
echo   -^> ksa/telemetry/mission-time

echo.
echo All topics published. Console should now show data.
echo Sending mission time updates every second (Ctrl+C to stop)...
echo.

REM Keep the console alive by incrementing mission time each second.
set MET=5765
:loop
mosquitto_pub -h %HOST% -p %PORT% -t ksa/telemetry/mission-time -m "{\"value\": %MET%}"
set /a MET=%MET%+1
timeout /t 1 /nobreak >nul
goto loop
