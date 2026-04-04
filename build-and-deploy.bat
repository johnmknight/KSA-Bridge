@echo off
setlocal enabledelayedexpansion
REM ========================================
REM KSA-Bridge: Build and Deploy with Backup
REM ========================================
REM
REM This script:
REM 1. Builds the project
REM 2. Rotates backups of existing DLLs (keeps N copies)
REM 3. Deploys new DLLs to the mods directory
REM
REM Set MAX_BACKUPS below to control how many backups to keep.
REM ========================================

set MAX_BACKUPS=5
set MOD_DIR=%USERPROFILE%\OneDrive\Documents\My Games\Kitten Space Agency\mods\KSA-Bridge
set BUILD_DIR=%~dp0KSA-Bridge\bin\Debug\net10.0
set BACKUP_DIR=%MOD_DIR%\backups

cd /d "%~dp0KSA-Bridge"

echo.
echo ========================================
echo Building KSA-Bridge...
echo ========================================
echo.

dotnet build
if errorlevel 1 (
    echo.
    echo BUILD FAILED!
    pause
    exit /b 1
)

echo.
echo Build successful!
echo.

REM Create backup directory if needed
if not exist "%BACKUP_DIR%" mkdir "%BACKUP_DIR%"

REM If there's an existing DLL, back it up with timestamp
if exist "%MOD_DIR%\KSA-Bridge.dll" (
    echo ========================================
    echo Backing up current DLLs...
    echo ========================================

    REM Generate timestamp for backup folder name
    for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value 2^>nul') do set "DT=%%I"
    set "TIMESTAMP=!DT:~0,4!-!DT:~4,2!-!DT:~6,2!_!DT:~8,2!-!DT:~10,2!-!DT:~12,2!"

    set "THIS_BACKUP=%BACKUP_DIR%\!TIMESTAMP!"
    mkdir "!THIS_BACKUP!"

    copy /Y "%MOD_DIR%\KSA-Bridge.dll" "!THIS_BACKUP!\KSA-Bridge.dll" >nul
    if exist "%MOD_DIR%\MQTTnet.dll" copy /Y "%MOD_DIR%\MQTTnet.dll" "!THIS_BACKUP!\MQTTnet.dll" >nul
    if exist "%MOD_DIR%\KSA-Bridge.deps.json" copy /Y "%MOD_DIR%\KSA-Bridge.deps.json" "!THIS_BACKUP!\KSA-Bridge.deps.json" >nul
    if exist "%MOD_DIR%\KSA-Bridge.pdb" copy /Y "%MOD_DIR%\KSA-Bridge.pdb" "!THIS_BACKUP!\KSA-Bridge.pdb" >nul

    for %%F in ("!THIS_BACKUP!\KSA-Bridge.dll") do echo   Backed up KSA-Bridge.dll ^(%%~zF bytes^) to !TIMESTAMP!

    REM Prune old backups beyond MAX_BACKUPS (oldest first)
    set COUNT=0
    for /f %%D in ('dir /b /ad /o-n "%BACKUP_DIR%" 2^>nul') do set /a COUNT+=1

    if !COUNT! GTR %MAX_BACKUPS% (
        echo   Pruning old backups ^(keeping %MAX_BACKUPS%^)...
        set SEEN=0
        for /f %%D in ('dir /b /ad /o-n "%BACKUP_DIR%" 2^>nul') do (
            set /a SEEN+=1
            if !SEEN! GTR %MAX_BACKUPS% (
                echo     Removing old backup: %%D
                rmdir /s /q "%BACKUP_DIR%\%%D"
            )
        )
    )
    echo.
)

echo ========================================
echo Deploying to mods directory...
echo ========================================
echo.

copy /Y "%BUILD_DIR%\KSA-Bridge.dll" "%MOD_DIR%\KSA-Bridge.dll"
copy /Y "%BUILD_DIR%\MQTTnet.dll" "%MOD_DIR%\MQTTnet.dll"
if exist "%BUILD_DIR%\KSA-Bridge.deps.json" copy /Y "%BUILD_DIR%\KSA-Bridge.deps.json" "%MOD_DIR%\KSA-Bridge.deps.json"
if exist "%BUILD_DIR%\KSA-Bridge.pdb" copy /Y "%BUILD_DIR%\KSA-Bridge.pdb" "%MOD_DIR%\KSA-Bridge.pdb"

echo.
echo ========================================
echo Deployed!
echo ========================================
echo.
for %%F in ("%MOD_DIR%\KSA-Bridge.dll") do echo   KSA-Bridge.dll: %%~zF bytes
for %%F in ("%MOD_DIR%\MQTTnet.dll") do echo   MQTTnet.dll:    %%~zF bytes
echo.
echo   Location: %MOD_DIR%
echo   Backups:  %BACKUP_DIR%
echo.
echo Close StarMap/KSA before launching to pick up the new DLL.
echo.
pause
