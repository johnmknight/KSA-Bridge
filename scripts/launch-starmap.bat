@echo off
REM ============================================================
REM REFERENCE COPY ONLY -- DO NOT RUN FROM THIS LOCATION.
REM
REM The canonical launcher lives at:
REM   C:\Program Files\StarMap\launch-starmap.bat
REM
REM StarMap.Loader.exe inherits the launching shell's working
REM directory. It requires CWD = its own install dir for mod loading
REM to work. Running this copy from the repo (or anywhere outside
REM the StarMap dir) launches the loader with the wrong working
REM directory: the game appears to start, but mods do not load and
REM KSA-Bridge telemetry does NOT flow.
REM
REM `setup.bat` deploys this file to C:\Program Files\StarMap\ at
REM install time so the in-place copy matches the repo. To run KSA,
REM execute the deployed copy:
REM
REM   "C:\Program Files\StarMap\launch-starmap.bat"
REM
REM (or double-click it from Explorer in that folder).
REM
REM Why StarMap.Loader.exe and not StarMap.exe:
REM   StarMap 0.4.5+ ships a separate Launcher entry point that is
REM   currently work-in-progress. Running StarMap.exe (the Launcher)
REM   prints:
REM     "Currently WIP, please use the standalone version or
REM      launch 'StarMap.Loader.exe'"
REM   and exits immediately. The Loader is the actual mod-loading
REM   process, so we point straight at it.
REM ============================================================
pause
start "" "C:\Program Files\StarMap\StarMap.Loader.exe"