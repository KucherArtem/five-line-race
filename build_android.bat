@echo off
setlocal
set "UNITY=C:\Program Files\Unity\Hub\Editor\6000.5.8f1\Editor\Unity.exe"
set "PROJECT=%~dp0."
set "LOGDIR=%~dp0Builds"
set "LOGFILE=%LOGDIR%\build-android.log"

if not exist "%UNITY%" (
  echo [ERROR] Unity not found: "%UNITY%"
  pause
  exit /b 1
)

if not exist "%LOGDIR%" mkdir "%LOGDIR%"

echo Building Android APK (IL2CPP ARM64, first build can take 10+ minutes)...
"%UNITY%" -batchmode -quit -projectPath "%PROJECT%" -executeMethod FiveLineRace.EditorTools.FiveLineRaceBuilder.BuildAndroid -logFile "%LOGFILE%"

if errorlevel 1 (
  echo [ERROR] Build failed. See "%LOGFILE%"
  pause
  exit /b 1
)

echo.
echo [OK] Builds\Android\FiveLineRace.apk
pause
