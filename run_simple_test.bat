@echo off
REM ============================================
REM Simple Test Execution Batch File
REM NOTE: Run the built game to execute test
REM ============================================

setlocal

REM Built Game Executable Path
set GAME_EXE=%~dp0unity\Build\Shibuya.exe

REM Log File Path
set LOG_PATH=%~dp0test_result.log

echo ============================================
echo 100m Clear Test Execution
echo ============================================
echo.

REM Check if built game exists
if not exist "%GAME_EXE%" (
    echo ERROR: Game executable not found.
    echo Path: %GAME_EXE%
    echo.
    echo Please build the game in Unity first.
    echo   Unity Editor ^> File ^> Build Settings ^> Build
    pause
    exit /b 1
)

echo Launching game...
echo Executable: %GAME_EXE%
echo.

REM Execute game
"%GAME_EXE%" -logFile "%LOG_PATH%"

REM Check exit code
set EXIT_CODE=%ERRORLEVEL%

echo.
echo ============================================

if %EXIT_CODE% EQU 0 (
    echo Test SUCCESS!
    echo 100m cleared successfully.
) else (
    echo Test FAILED
    echo Collision with enemy or error occurred.
)

echo ============================================
echo.
echo Log file: %LOG_PATH%
echo Exit code: %EXIT_CODE%
echo.
pause

exit /b %EXIT_CODE%

endlocal
