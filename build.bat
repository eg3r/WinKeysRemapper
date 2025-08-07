@echo off
echo Building WinKeysRemapper...
dotnet build src\WinKeysRemapper.csproj
if %ERRORLEVEL% == 0 (
    echo Build successful!
    echo.
    echo To run the application:
    echo   dotnet run --project src\WinKeysRemapper.csproj
    echo.
    echo Or run the executable directly:
    echo   .\src\bin\Debug\net8.0-windows\WinKeysRemapper.exe
) else (
    echo Build failed!
)
pause
