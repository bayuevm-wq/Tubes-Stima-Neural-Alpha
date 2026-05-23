@echo off
set MODE=dev
if "%MODE%"=="dev" (
    rmdir /s /q bin obj >nul 2>&1
    dotnet build
    dotnet run --no-build
)
