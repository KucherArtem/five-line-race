@echo off
setlocal
set "UNITY=C:\Program Files\Unity\Hub\Editor\6000.5.8f1\Editor\Unity.exe"
set "PROJECT=%~dp0."
set "LOGDIR=%~dp0Builds"
set "LOGFILE=%LOGDIR%\build-windows.log"

if not exist "%UNITY%" (
  echo [ERROR] Unity not found: "%UNITY%"
  pause
  exit /b 1
)

if not exist "%LOGDIR%" mkdir "%LOGDIR%"

echo Building Windows player...
"%UNITY%" -batchmode -quit -projectPath "%PROJECT%" -executeMethod FiveLineRace.EditorTools.FiveLineRaceBuilder.BuildWindows -logFile "%LOGFILE%"

if errorlevel 1 (
  echo [ERROR] Build failed. See "%LOGFILE%"
  pause
  exit /b 1
)

echo.
echo [OK] Builds\Windows\FiveLineRace.exe
pause
