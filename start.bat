@echo off
setlocal

set "ROOT=%~dp0"
set "API_PROJECT=E-CommereceAPI\E-CommereceAPI.csproj"
set "FRONTEND_DIR=frontend"

if not exist "%ROOT%E-Commerece.sln" (
    echo ERROR: E-Commerece.sln was not found beside start.bat.
    pause
    exit /b 1
)

if not exist "%ROOT%%API_PROJECT%" (
    echo ERROR: Backend project was not found: %API_PROJECT%
    pause
    exit /b 1
)

if not exist "%ROOT%%FRONTEND_DIR%\package.json" (
    echo ERROR: Frontend package.json was not found in %FRONTEND_DIR%.
    pause
    exit /b 1
)

where dotnet >nul 2>nul
if errorlevel 1 (
    echo ERROR: .NET SDK was not found. Install the .NET 9 SDK and try again.
    pause
    exit /b 1
)

where npm.cmd >nul 2>nul
if errorlevel 1 (
    echo ERROR: npm was not found. Install Node.js 20.19+ or 22.12+ and try again.
    pause
    exit /b 1
)

if not exist "%ROOT%%FRONTEND_DIR%\node_modules\.bin\vite.cmd" (
    echo ERROR: Frontend dependencies are not installed.
    echo Run "npm ci" from the frontend folder, then start this file again.
    pause
    exit /b 1
)

echo Starting E-Commerce Backend and Frontend in separate terminals...
echo Backend:  http://localhost:5187
echo Frontend: http://localhost:5173
echo The Vite proxy forwards /api requests to http://localhost:5187.

start "E-Commerce Backend" cmd /k "cd /d ""%ROOT%"" && dotnet run --project ""%API_PROJECT%"" --launch-profile http"
start "E-Commerce Frontend" cmd /k "cd /d ""%ROOT%%FRONTEND_DIR%"" && npm.cmd run dev -- --host 127.0.0.1"

echo Both terminal windows were opened.
exit /b 0
