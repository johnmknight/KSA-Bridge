@echo off
REM KSA-Bridge Examples Web Server
REM Serves the example consoles (Apollo Mission Control, Hard Sci-Fi FDO)
REM Access at: http://localhost:8088/apollo-mission-control/ or http://localhost:8088/hard-scifi/

cd /d "%~dp0..\examples"
echo.
echo Starting Python HTTP server on http://localhost:8088
echo.
echo Example consoles:
echo   - Apollo Mission Control: http://localhost:8088/apollo-mission-control/
echo   - Hard Sci-Fi FDO:        http://localhost:8088/hard-scifi/
echo.
echo Press Ctrl+C to stop the server
echo.

python -m http.server 8088

pause
