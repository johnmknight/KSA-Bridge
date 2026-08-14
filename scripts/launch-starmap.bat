@echo off
REM ============================================================
REM REFERENCE COPY -- the canonical launcher lives at:
REM   C:\Program Files\StarMap\launch-starmap.bat
REM (`setup.bat` deploys this file there at install time.)
REM
REM StarMap 0.4.6 merged the old Launcher/Loader split into a
REM single StarMap.exe -- StarMap.Loader.exe no longer exists.
REM The /D switch pins the working directory to the StarMap
REM install dir; StarMap inherits the launching shell's CWD and
REM requires its own dir for mod loading to work. Without it the
REM game appears to start but mods do not load and KSA-Bridge
REM telemetry does NOT flow.
REM ============================================================
start "" /D "C:\Program Files\StarMap" "C:\Program Files\StarMap\StarMap.exe"
