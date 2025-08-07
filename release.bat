@echo off
REM Create a new version tag and push to GitHub
REM Usage: release.bat [version]
REM Example: release.bat 1.0.1

if "%1"=="" (
    echo Usage: release.bat [version]
    echo Example: release.bat 1.0.1
    echo.
    echo This will create a git tag and trigger the release workflow
    pause
    exit /b 1
)

set VERSION=v%1

echo Creating release version %VERSION%...
echo.

REM Check if tag already exists
git tag -l %VERSION% | findstr %VERSION% >nul
if %ERRORLEVEL% == 0 (
    echo Error: Tag %VERSION% already exists!
    echo Existing tags:
    git tag -l
    pause
    exit /b 1
)

REM Create and push the tag
echo Creating tag %VERSION%...
git tag -a %VERSION% -m "Release %VERSION%"

echo Pushing tag to GitHub...
git push origin %VERSION%

echo.
echo ✅ Release %VERSION% created successfully!
echo.
echo The GitHub Actions workflow will now:
echo - Build the project for Windows x64, x86, and portable
echo - Create optimized single-file executables
echo - Generate a GitHub release with downloadable assets
echo - Include documentation and example configuration
echo.
echo Visit https://github.com/eg3r/WinKeysRemapper/releases to see the release
echo.
pause
