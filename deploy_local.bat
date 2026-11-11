@echo off
echo Updating Revit Shuttering Add-in DLL...
taskkill /F /IM Revit.exe 2>nul
timeout /t 1 /nobreak >nul
copy /Y "%~dp0bin\Release\net48\RevitShutteringAddin.dll" "%APPDATA%\Autodesk\Revit\Addins\2020\RevitShutteringAddin.dll"
if %ERRORLEVEL% EQU 0 (
    echo [SUCCESS] Updated %APPDATA%\Autodesk\Revit\Addins\2020\RevitShutteringAddin.dll
) else (
    echo [ERROR] Failed to copy DLL. Make sure Revit is fully closed and try again.
)
pause
