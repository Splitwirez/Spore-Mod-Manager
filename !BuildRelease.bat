@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

powershell "cmd /c .\build\BuildSMM.bat | tee buildLog.txt"

if errorlevel 1 GOTO FAIL
if not errorlevel 1 GOTO SUCCESS


:SUCCESS
color 0A
echo Spore Mod Manager built successfully.
GOTO END



:FAIL
color 0C
echo Spore Mod Manager failed to build.
GOTO END


:END
pause