@echo off
REM ============================================
REM 100m Clear Test Execution Batch File
REM ============================================

setlocal

REM Unity Editor Path
set UNITY_PATH="D:\Unity\Hub\6000.0.49f1\Editor\Unity.exe"

REM Project Path
set PROJECT_PATH=%~dp0unity

REM Log File Path
set LOG_PATH=%~dp0test_log.txt

REM Test Scene Name
set TEST_SCENE=Assets/Scenes/SampleScene.unity

echo ============================================
echo 100m Clear Test Start
echo ============================================
echo.
echo Unity Path: %UNITY_PATH%
echo Project Path: %PROJECT_PATH%
echo Test Scene: %TEST_SCENE%
echo Log Path: %LOG_PATH%
echo.

REM Execute Unity in batch mode to run test
"%UNITY_PATH%" -batchmode -projectPath "%PROJECT_PATH%" -executeMethod TestRunner.RunGameClearTest -logFile "%LOG_PATH%" -quit

REM Check exit code
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ============================================
    echo Test SUCCESS!
    echo ============================================
    echo.
    echo Please check log file: %LOG_PATH%
    exit /b 0
) else (
    echo.
    echo ============================================
    echo Test FAILED
    echo ============================================
    echo.
    echo Error Code: %ERRORLEVEL%
    echo Please check log file: %LOG_PATH%
    exit /b 1
)

endlocal
